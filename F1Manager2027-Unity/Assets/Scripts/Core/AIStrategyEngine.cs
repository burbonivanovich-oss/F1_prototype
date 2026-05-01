// AIStrategyEngine.cs — Per-lap AI pit strategy, compound selection, driver instructions,
// pit stop timing, and overcut/undercut awareness for the F1Manager race simulation.
// Pure C#, no Unity dependencies. Mirrors Python core/ai.py.
using System;
using System.Collections.Generic;
using System.Linq;

namespace F1Manager
{
    // ─────────────────────────────────────────────────────────────────────────
    // PitProjection — result of PitStopProjection() static helper
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Estimated outcome if the given car pits this lap.
    /// Used by the player HUD undercut analysis panel.
    /// </summary>
    public class PitProjection
    {
        /// <summary>Best compound to fit for the remaining stint.</summary>
        public TireCompound Compound;

        /// <summary>Car's position before entering the pit lane.</summary>
        public int PositionBefore;

        /// <summary>Estimated position after exiting the pit lane.</summary>
        public int PositionAfter;

        /// <summary>Number of cars that will pass during the pit stop window.</summary>
        public int CarsLost;

        /// <summary>Estimated laps needed to recover lost positions on fresh rubber.</summary>
        public int LapsToRecover;

        /// <summary>Estimated per-lap pace gain from fresh tyre vs current degraded tyre (seconds).</summary>
        public float PaceGainPerLap;

        /// <summary>True if the lost positions can realistically be recovered before the race ends.</summary>
        public bool CanRecover;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AIStrategyEngine
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Decides when and how each AI-controlled car should pit, which compound to
    /// choose, how long the pit stop takes, what driver instruction to issue, and
    /// how much fuel to add.  Called once per lap for every non-player car.
    ///
    /// All randomness is routed through the supplied <see cref="System.Random"/> so
    /// that race replays are deterministic when seeded.
    /// </summary>
    public class AIStrategyEngine
    {
        private readonly CircuitInfo          _circuit;
        private readonly List<TireCompound>   _availableDry;
        private readonly System.Random        _rng;

        /// <param name="circuit">Immutable circuit info for the current race.</param>
        /// <param name="availableCompounds">All compounds available this weekend (dry + rain).</param>
        /// <param name="rng">Seeded random number generator for deterministic replay.</param>
        public AIStrategyEngine(
            CircuitInfo          circuit,
            List<TireCompound>   availableCompounds,
            System.Random        rng)
        {
            _circuit      = circuit;
            _availableDry = availableCompounds.Where(c => c.IsDry()).ToList();
            _rng          = rng;
        }

        // ── Pit decision ──────────────────────────────────────────────────────

        /// <summary>
        /// Returns true if the AI should pit this lap.
        /// Checks tyre phase, mandatory two-compound rule, window remaining,
        /// safety car opportunity, fuel deficit, weather mismatch, and
        /// undercut / cover-undercut awareness.
        /// </summary>
        public bool ShouldPit(CarState car, RaceState race, DriverInfo driver)
        {
            if (!TireProfiles.All.TryGetValue(car.Compound, out TireProfile profile))
                return false;

            float degMult      = _circuit.tireDegMultiplier;
            TirePhase phase    = TireSystem.GetPhase(profile, car.TireAgeLaps, degMult);
            int   window       = TireSystem.WindowRemaining(profile, car.TireAgeLaps, degMult);
            int   lapsRemaining = race.TotalLaps - race.CurrentLap;

            // ── Hard rule: cliff phase — box immediately ──────────────────────
            if (phase == TirePhase.CLIFF)
                return true;

            // ── Mandatory two-compound rule ───────────────────────────────────
            int distinctUsed = car.CompoundsUsed.Distinct().Count();
            if (distinctUsed < 2 && lapsRemaining <= 10)
            {
                var alternatives = _availableDry
                    .Where(c => !car.CompoundsUsed.Contains(c))
                    .ToList();
                if (alternatives.Count > 0)
                    return true;
            }

            // ── Window closing while race not nearly over ─────────────────────
            if (window <= 2 && lapsRemaining > 5)
                return true;

            // ── Safety car opportunity ────────────────────────────────────────
            if (race.SafetyCar == SafetyCarState.DEPLOYED
                    && car.TireAgeLaps >= 8
                    && lapsRemaining > 8)
            {
                return _rng.NextDouble() < 0.85;
            }

            // ── Fuel deficit — will run dry before the end ────────────────────
            float fuelNeeded = lapsRemaining * _circuit.fuelConsumptionKg;
            if (car.FuelKg < fuelNeeded * 0.92f)
                return true;

            // ── Weather mismatch ──────────────────────────────────────────────
            bool onDryTyre = car.Compound.IsDry();
            if (race.Weather == WeatherCondition.HEAVY_RAIN && onDryTyre)
                return true;
            if (race.Weather == WeatherCondition.LIGHT_RAIN && onDryTyre)
                return true;
            if ((race.Weather == WeatherCondition.DRY || race.Weather == WeatherCondition.DRYING)
                    && !onDryTyre
                    && car.Compound != TireCompound.INTER)
            {
                // On full wets in dry/drying conditions — switch after 2 drying laps
                // (tracked externally; approximate here by tire age proxy)
                if (car.TireAgeLaps >= 2)
                    return true;
            }
            // Intermediates in pure DRY: pit after 2 laps of improving track
            if (race.Weather == WeatherCondition.DRY
                    && car.Compound == TireCompound.INTER
                    && car.TireAgeLaps >= 2)
                return true;

            // ── Undercut / overcut awareness ──────────────────────────────────
            if (lapsRemaining > 8 && car.TireAgeLaps >= 5)
            {
                var sorted = race.SortedCars();
                int myPos = car.Position;

                foreach (var other in sorted)
                {
                    if (other.DriverID == car.DriverID || other.DNF)
                        continue;

                    int posDelta = myPos - other.Position; // positive = other is behind us

                    // Car 1-2 positions behind pitted last lap → undercut threat
                    if (posDelta > 0 && posDelta <= 2 && other.PittedLastLap)
                    {
                        if (_rng.NextDouble() < 0.60)
                            return true;
                    }

                    // Car 1-2 positions ahead is pitting this lap → cover their undercut
                    if (posDelta < 0 && posDelta >= -2 && other.IsPittingThisLap)
                    {
                        if (_rng.NextDouble() < 0.45)
                            return true;
                    }
                }
            }

            return false;
        }

        // ── Compound selection ────────────────────────────────────────────────

        /// <summary>
        /// Chooses the best compound for the next stint.
        /// Weather overrides take priority; then the mandatory two-compound rule
        /// is respected; finally <see cref="TireSystem.BestCompoundForStint"/> is used.
        /// </summary>
        public TireCompound ChooseCompound(CarState car, RaceState race)
        {
            int lapsRemaining = race.TotalLaps - race.CurrentLap;

            // ── Weather overrides ─────────────────────────────────────────────
            if (race.Weather == WeatherCondition.HEAVY_RAIN)
                return TireCompound.WET;
            if (race.Weather == WeatherCondition.LIGHT_RAIN
                    || race.Weather == WeatherCondition.DRYING)
                return TireCompound.INTER;

            var compoundsUsed = new HashSet<TireCompound>(car.CompoundsUsed);

            // ── Mandatory two-compound rule ───────────────────────────────────
            if (compoundsUsed.Count < 2)
            {
                var alternatives = _availableDry
                    .Where(c => !compoundsUsed.Contains(c))
                    .ToList();

                if (alternatives.Count > 0 && lapsRemaining <= 15)
                {
                    // Pick the hardest (most durable) alternative for a long final stint
                    return alternatives
                        .OrderByDescending(c => TireProfiles.All[c].PlateauLaps)
                        .First();
                }
            }

            // ── Best compound that will survive the remaining stint ───────────
            TireCompound best = TireSystem.BestCompoundForStint(
                _availableDry, lapsRemaining, _circuit.tireDegMultiplier);

            // Safety: never return the current compound if the mandatory rule is still open
            if (compoundsUsed.Count < 2 && compoundsUsed.Contains(best))
            {
                var fallback = _availableDry.Where(c => !compoundsUsed.Contains(c)).ToList();
                if (fallback.Count > 0)
                    return fallback[0];
            }

            return best;
        }

        // ── Pit stop duration ─────────────────────────────────────────────────

        /// <summary>
        /// Calculates a realistic pit stop tyre-change duration based on crew skill.
        /// <list type="bullet">
        ///   <item>0.5% chance of critical error: 4.0–6.0 s</item>
        ///   <item>skill ≥ 90 → 1.92–2.05 s</item>
        ///   <item>skill ≥ 80 → 2.10–2.40 s</item>
        ///   <item>skill ≥ 70 → 2.40–2.80 s</item>
        ///   <item>else      → 2.80–4.00 s</item>
        /// </list>
        /// The returned value does NOT include pit-lane time loss (add ~20 s separately).
        /// </summary>
        public float PitStopDurationS(int pitCrewSkill, System.Random rng)
        {
            // Critical error
            if (rng.NextDouble() < 0.005)
                return (float)(4.0 + rng.NextDouble() * (6.0 - 4.0));

            if (pitCrewSkill >= 90)
                return (float)(1.92 + rng.NextDouble() * (2.05 - 1.92));
            if (pitCrewSkill >= 80)
                return (float)(2.10 + rng.NextDouble() * (2.40 - 2.10));
            if (pitCrewSkill >= 70)
                return (float)(2.40 + rng.NextDouble() * (2.80 - 2.40));

            return (float)(2.80 + rng.NextDouble() * (4.00 - 2.80));
        }

        // ── Driver instruction ────────────────────────────────────────────────

        /// <summary>
        /// Chooses the most appropriate <see cref="DriverInstruction"/> for the next lap.
        /// Priority order: final push > fuel save > tyre management > fresh-tyre attack >
        /// overcut push > defend > default MANAGE.
        /// </summary>
        public DriverInstruction ChooseInstruction(CarState car, RaceState race, DriverInfo driver)
        {
            int lapsRemaining = race.TotalLaps - race.CurrentLap;
            float degMult     = _circuit.tireDegMultiplier;

            TirePhase phase  = TirePhase.PLATEAU;
            int       window = 20;

            if (TireProfiles.All.TryGetValue(car.Compound, out TireProfile profile))
            {
                phase  = TireSystem.GetPhase(profile, car.TireAgeLaps, degMult);
                window = TireSystem.WindowRemaining(profile, car.TireAgeLaps, degMult);
            }

            // ── Final laps: push hard if tyres can take it ────────────────────
            if (lapsRemaining <= 5 && phase != TirePhase.CLIFF)
                return DriverInstruction.ATTACK;

            // ── Fuel conservation ─────────────────────────────────────────────
            float fuelNeeded = lapsRemaining * _circuit.fuelConsumptionKg;
            if (car.FuelKg < fuelNeeded * 0.98f && car.PitStopCount >= 1)
                return DriverInstruction.FUEL_SAVE;

            // ── Approaching cliff — protect the tyres ─────────────────────────
            if (window <= 3)
                return DriverInstruction.MANAGE;

            // ── Fresh tyres: attack to maximise pace ──────────────────────────
            if (car.TireAgeLaps <= 3 && car.Position > 1)
                return DriverInstruction.ATTACK;

            // ── Overcut push: car directly ahead is in (or just exited) pits ──
            var sorted = race.SortedCars();
            int myPos  = car.Position;
            foreach (var other in sorted)
            {
                if (other.DriverID == car.DriverID || other.DNF)
                    continue;
                if (other.Position == myPos - 1
                        && (other.IsPittingThisLap || other.PittedLastLap))
                    return DriverInstruction.ATTACK;
            }

            // ── Defend when being closely followed ────────────────────────────
            if (car.GapToAheadS < 0.8f && car.Position < 5)
                return DriverInstruction.DEFEND;

            return DriverInstruction.MANAGE;
        }

        // ── Fuel load at pit stop ─────────────────────────────────────────────

        /// <summary>
        /// Returns the amount of fuel (kg) to add during a pit stop.
        /// Adds enough to reach the end plus a 3 kg safety buffer; capped at 30 kg.
        /// </summary>
        public float FuelToAddKg(CarState car, RaceState race)
        {
            int   lapsRemaining = race.TotalLaps - race.CurrentLap;
            float needed        = lapsRemaining * _circuit.fuelConsumptionKg;
            float toAdd         = needed - car.FuelKg + 3f;
            return Math.Min(30f, Math.Max(0f, toAdd));
        }

        // ── Static pit stop projection (undercut analysis) ────────────────────

        /// <summary>
        /// Projects the estimated position change and pace recovery if <paramref name="car"/>
        /// pits on the current lap.  Used by the player HUD to display undercut analysis.
        /// </summary>
        /// <param name="car">The car considering pitting.</param>
        /// <param name="sortedCars">All cars sorted by race position (leader first).</param>
        /// <param name="circuit">Current circuit info.</param>
        /// <param name="availableDry">Dry compounds available this weekend.</param>
        /// <param name="lapsRemaining">Laps left in the race.</param>
        /// <returns>
        /// A <see cref="PitProjection"/> describing the outcome, or <c>null</c> if
        /// a projection cannot be computed (e.g. race nearly over).
        /// </returns>
        public static PitProjection PitStopProjection(
            CarState            car,
            List<CarState>      sortedCars,
            CircuitInfo         circuit,
            List<TireCompound>  availableDry,
            int                 lapsRemaining)
        {
            const float PIT_LANE_LOSS_S = 22.0f;

            if (lapsRemaining <= 1)
                return null;

            TireCompound bestCompound = TireSystem.BestCompoundForStint(
                availableDry, lapsRemaining, circuit.tireDegMultiplier);

            if (!TireProfiles.All.TryGetValue(car.Compound, out TireProfile curProfile))
                return null;
            if (!TireProfiles.All.TryGetValue(bestCompound, out TireProfile freshProfile))
                return null;

            // Current tyre degradation penalty
            float curDegPen = TireSystem.DegPenaltyS(
                curProfile, car.TireAgeLaps, circuit.tireDegMultiplier, mgmtRating: 80);

            // Grip delta between current and fresh compound (positive = fresh is faster)
            float curSpeed   = car.Compound.GripAdvantageS();   // positive = slower vs C4
            float freshSpeed = bestCompound.GripAdvantageS();

            float paceGainPerLap = Math.Max(0.05f, curDegPen + (curSpeed - freshSpeed));

            // Count cars that will jump us during the pit stop window
            int carsLost = sortedCars.Count(c =>
                c.DriverID  != car.DriverID
                && !c.DNF
                && c.Position > car.Position
                && (c.GapToLeaderS - car.GapToLeaderS) >= 0f
                && (c.GapToLeaderS - car.GapToLeaderS) <= PIT_LANE_LOSS_S);

            int positionAfter = car.Position + carsLost;

            int lapsToRecover = carsLost == 0
                ? 0
                : Math.Max(1, (int)(carsLost * 3.5f / paceGainPerLap));

            bool canRecover = lapsToRecover < lapsRemaining - 2;

            return new PitProjection
            {
                Compound        = bestCompound,
                PositionBefore  = car.Position,
                PositionAfter   = positionAfter,
                CarsLost        = carsLost,
                LapsToRecover   = lapsToRecover,
                PaceGainPerLap  = paceGainPerLap,
                CanRecover      = canRecover,
            };
        }
    }
}
