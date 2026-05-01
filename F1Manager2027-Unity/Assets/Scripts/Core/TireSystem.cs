// TireSystem.cs — Static tyre-degradation calculations for the F1Manager race engine.
// Pure C#, no Unity dependencies. Mirrors Python core/tire.py.
using System;
using System.Collections.Generic;
using System.Linq;

namespace F1Manager
{
    /// <summary>
    /// Stateless helper class that implements the four-phase tyre degradation model.
    ///
    /// The four phases per compound (all durations scaled by circuit deg-multiplier):
    ///   WARM_UP  → first 1-2 laps after a pit stop (fixed time penalty)
    ///   PLATEAU  → stable near-peak performance
    ///   LINEAR   → progressive, predictable wear (seconds/lap)
    ///   CLIFF    → rapid performance collapse — driver must pit immediately
    ///
    /// All methods are pure functions of their inputs; no mutable state.
    /// </summary>
    public static class TireSystem
    {
        // ─────────────────────────────────────────────────────────────────────
        // Phase classification
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the current <see cref="TirePhase"/> for a tyre of the given age.
        /// </summary>
        /// <param name="p">Compound profile from <see cref="TireProfiles.All"/>.</param>
        /// <param name="age">Laps completed on this tyre set (0-indexed: 0 = brand new).</param>
        /// <param name="degMult">Circuit tyre-degradation multiplier (Silverstone=1.0).</param>
        public static TirePhase GetPhase(TireProfile p, int age, float degMult)
        {
            int effectivePlateau = Math.Max(3, (int)(p.PlateauLaps    / degMult));
            int effectiveLinear  = Math.Max(3, (int)(p.LinearPhaseLaps / degMult));
            int cliffStart       = effectivePlateau + effectiveLinear;

            if (age <= p.WarmUpLaps)      return TirePhase.WARM_UP;
            if (age <= effectivePlateau)  return TirePhase.PLATEAU;
            if (age <= cliffStart)        return TirePhase.LINEAR;
            return TirePhase.CLIFF;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Degradation penalty
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the lap-time penalty (seconds) from tyre degradation.
        /// </summary>
        /// <param name="p">Compound profile.</param>
        /// <param name="age">Tyre age in laps.</param>
        /// <param name="degMult">Circuit deg multiplier.</param>
        /// <param name="mgmtRating">
        ///   Driver tyre-management rating (0–100). Default 85 = neutral.
        ///   Each point above 85 reduces wear by 0.2%; each point below increases it.
        /// </param>
        public static float DegPenaltyS(TireProfile p, int age, float degMult, int mgmtRating = 85)
        {
            float mgmtFactor   = 1f - (mgmtRating - 85) * 0.002f;
            float effectiveDeg = p.LinearDegPerLapS * degMult * Math.Max(0.7f, mgmtFactor);

            int effectivePlateau = Math.Max(3, (int)(p.PlateauLaps    / degMult));
            int effectiveLinear  = Math.Max(3, (int)(p.LinearPhaseLaps / degMult));
            int cliffStart       = effectivePlateau + effectiveLinear;

            TirePhase phase = GetPhase(p, age, degMult);

            switch (phase)
            {
                case TirePhase.WARM_UP:
                    // Fixed warm-up penalty — temperature-independent version.
                    // (Temperature modifier lives in WarmUpPenaltyS for track-temp-aware callers.)
                    if (age == 1)
                        return p.WarmUpPenaltyS;
                    if (age == 2 && p.WarmUpLaps >= 2)
                        return p.WarmUpPenaltyLap2S;
                    return 0f;

                case TirePhase.PLATEAU:
                    // Near-zero degradation; residual 20% of per-lap rate
                    return Math.Max(0f, effectiveDeg * 0.2f);

                case TirePhase.LINEAR:
                    int lapsIntoLinear = age - effectivePlateau;
                    return effectiveDeg * lapsIntoLinear;

                default: // CLIFF
                    int lapsIntoCliff = age - cliffStart;
                    float baseLinear  = effectiveDeg * effectiveLinear;
                    return baseLinear + p.CliffPenaltyS * (1f + lapsIntoCliff * 0.3f);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Window remaining
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// How many laps remain before the tyre enters the cliff phase.
        /// Returns 0 once the cliff has started.
        /// </summary>
        public static int WindowRemaining(TireProfile p, int age, float degMult)
        {
            int effectivePlateau = Math.Max(3, (int)(p.PlateauLaps    / degMult));
            int effectiveLinear  = Math.Max(3, (int)(p.LinearPhaseLaps / degMult));
            int cliffStart       = effectivePlateau + effectiveLinear;
            return Math.Max(0, cliffStart - age);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Degradation percentage
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a 0–1 wear fraction for UI colour-coding (0 = fresh, 1 = destroyed).
        /// The denominator adds a small buffer beyond cliff-start so colours transition
        /// smoothly rather than jumping to 1.0 the moment cliff begins.
        /// </summary>
        public static float DegPct(TireProfile p, int age, float degMult)
        {
            int effectivePlateau = Math.Max(3, (int)(p.PlateauLaps    / degMult));
            int effectiveLinear  = Math.Max(3, (int)(p.LinearPhaseLaps / degMult));
            int totalLife        = effectivePlateau + effectiveLinear + 5; // +5 buffer
            return Math.Min(1f, (float)age / totalLife);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Warm-up penalty with track-temperature modifier
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Lap-time warm-up penalty including a cold-track multiplier.
        /// Used by the engine immediately after a pit stop to give an authentic
        /// "tyres not up to temperature" effect on cool circuits.
        /// </summary>
        /// <param name="p">Compound profile.</param>
        /// <param name="age">Tyre age in laps (1 or 2 are meaningful).</param>
        /// <param name="trackTemp">Current track surface temperature in °C.</param>
        public static float WarmUpPenaltyS(TireProfile p, int age, float trackTemp)
        {
            if (age > p.WarmUpLaps)
                return 0f;

            // Below 22°C the rubber needs more heat cycles — penalty grows linearly.
            float coldMult = 1f + Math.Max(0f, (22f - trackTemp) * 0.04f);

            if (age == 1)
                return p.WarmUpPenaltyS * coldMult;
            if (age == 2 && p.WarmUpLaps >= 2)
                return p.WarmUpPenaltyLap2S * coldMult;
            return 0f;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Compound selection helper
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Selects the fastest available dry compound that will survive
        /// <paramref name="stintLaps"/> laps without entering the cliff phase.
        ///
        /// Algorithm:
        ///   1. Filter to dry compounds in <paramref name="available"/>.
        ///   2. Sort ascending by GripBonusS (fastest / softest first).
        ///   3. Pick the first compound whose WindowRemaining(age=0, degMult) >= stintLaps.
        ///   4. Fallback: return the hardest available compound if nothing survives.
        /// </summary>
        /// <param name="available">Compounds available this race weekend.</param>
        /// <param name="stintLaps">Planned number of laps for the stint.</param>
        /// <param name="degMult">Circuit tyre-degradation multiplier.</param>
        public static TireCompound BestCompoundForStint(
            List<TireCompound> available,
            int                stintLaps,
            float              degMult)
        {
            // Collect dry compounds only
            var dry = available
                .Where(c => c.IsDry())
                .ToList();

            if (dry.Count == 0)
            {
                // Rain compounds only — return INTER if present, else WET
                if (available.Contains(TireCompound.INTER)) return TireCompound.INTER;
                return TireCompound.WET;
            }

            // Sort ascending by GripBonusS → softest (fastest) first
            dry.Sort((a, b) =>
            {
                float ga = TireProfiles.All[a].GripBonusS;
                float gb = TireProfiles.All[b].GripBonusS;
                return ga.CompareTo(gb);
            });

            foreach (TireCompound compound in dry)
            {
                TireProfile profile = TireProfiles.All[compound];
                int window = WindowRemaining(profile, 0, degMult);
                if (window >= stintLaps)
                    return compound;
            }

            // Fallback: return the hardest compound (longest plateau)
            return dry.OrderByDescending(c => TireProfiles.All[c].PlateauLaps).First();
        }
    }
}
