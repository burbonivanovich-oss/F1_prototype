// OvertakeSystem.cs — Sigmoid overtake resolution for the F1Manager race engine.
// Pure C#, no Unity dependencies. Mirrors Python core/overtake.py exactly.
// Formula source: GDD §08_TUNING_OVERTAKE_TIRE_v0
//
//   logit  = W1*(skill_atk-skill_def) + W2*pace_delta + W3*(-tire_age_delta)
//          + W4*DRS_flag - W5*track_difficulty - W6*defender_aggression
//   P_raw  = 1 / (1 + exp(-logit))
//   P(s)   = min(P_raw, P_CAP)
using System;
using System.Collections.Generic;
using System.Linq;

namespace F1Manager
{
    /// <summary>
    /// Stateless resolver for lap-by-lap on-track overtake attempts.
    ///
    /// <para>
    /// <c>ResolveOvertakes</c> is the primary entry point: pass it the current
    /// <see cref="RaceState"/>, the driver dictionary, the circuit, and a seeded
    /// <see cref="System.Random"/> instance.  It returns a list of
    /// <see cref="OvertakeResult"/> records that the engine uses to update positions
    /// and emit event-log entries.
    /// </para>
    /// </summary>
    public static class OvertakeSystem
    {
        // ── Sigmoid weight constants (v0 — see tuning doc §1.3) ───────────────
        /// <summary>Skill gap contribution (racecraft_atk - defending_def).</summary>
        public const float W1_SKILL  = 0.04f;

        /// <summary>Pace delta contribution (defender_last_lap - attacker_last_lap, clamped ±3 s).</summary>
        public const float W2_PACE   = 1.80f;

        /// <summary>Tyre age delta contribution (fresher attacker = positive logit).</summary>
        public const float W3_TIRE   = 0.05f;

        /// <summary>Flat DRS bonus when attacker is within 1 s on a DRS circuit.</summary>
        public const float W4_DRS    = 1.50f;

        /// <summary>Track overtake-difficulty penalty (0 = Monza, 1 = Monaco).</summary>
        public const float W5_TRACK  = 3.00f;

        /// <summary>Defender aggression penalty (0 = passive, 1 = full-block).</summary>
        public const float W6_DEFEND = 2.00f;

        /// <summary>Hard probability cap.  Producer decision 2026-04-25; do not remove.</summary>
        public const float P_CAP     = 0.85f;

        // ─────────────────────────────────────────────────────────────────────
        // Sub-calculations
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Defender aggression factor in [0, 1].
        /// DEFEND instruction → 0.85 base; ATTACK → 0.15; otherwise 0.30.
        /// Scaled by driver morale so demoralised defenders fight less hard.
        /// </summary>
        public static float DefenderAggression(CarState car, DriverInfo driver)
        {
            float baseAggression;
            switch (car.Instruction)
            {
                case DriverInstruction.DEFEND: baseAggression = 0.85f; break;
                case DriverInstruction.ATTACK: baseAggression = 0.15f; break;
                default:                       baseAggression = 0.30f; break;
            }
            float moraleFactor = driver.morale / 100f;
            return Math.Min(1f, baseAggression * moraleFactor * 1.1f);
        }

        /// <summary>
        /// Pace delta: how many seconds per lap faster the attacker is.
        /// Positive = attacker faster; clamped to [-3, +3] to prevent outliers.
        /// Returns 0 if either car has no valid lap time yet.
        /// </summary>
        public static float PaceDelta(CarState attacker, CarState defender)
        {
            if (attacker.LastLapTimeS <= 0f || defender.LastLapTimeS <= 0f)
                return 0f;
            float delta = defender.LastLapTimeS - attacker.LastLapTimeS;
            return Math.Max(-3f, Math.Min(3f, delta));
        }

        /// <summary>
        /// Returns true when the attacker is eligible for DRS this lap.
        /// Conditions: circuit has DRS zones, lap >= 2, gap is 0 &lt; gap &lt;= 1.0 s.
        /// </summary>
        public static bool DRSAvailable(float gapS, CircuitInfo circuit, int lap)
        {
            return circuit.drsZones > 0
                && lap >= 2
                && gapS > 0f
                && gapS <= 1.0f;
        }

        /// <summary>
        /// Lap-time penalty (seconds) from aerodynamic dirty air when following closely.
        /// Applied to the attacker's lap time to reflect real-world wake turbulence.
        ///
        /// <para>High-downforce circuits (overtakeDifficulty &gt; 0.7) amplify the penalty;
        /// low-downforce circuits (Monza; overtakeDifficulty &lt; 0.2) reduce it.</para>
        /// </summary>
        public static float DirtyAirPenaltyS(float gapAhead, CircuitInfo circuit)
        {
            if (gapAhead <= 0f || gapAhead > 2f)
                return 0f;

            float dfMult;
            if      (circuit.overtakeDifficulty > 0.7f) dfMult = 1.3f;
            else if (circuit.overtakeDifficulty < 0.2f) dfMult = 0.6f;
            else                                         dfMult = 1.0f;

            float basePenalty;
            if      (gapAhead <= 0.5f) basePenalty = 0.30f;
            else if (gapAhead <= 1.0f) basePenalty = 0.20f;
            else                       basePenalty = 0.10f;

            return basePenalty * dfMult;
        }

        /// <summary>
        /// Computes the sigmoid overtake probability for one attacker–defender pair.
        /// </summary>
        /// <param name="attacker">Attacker car state.</param>
        /// <param name="defender">Defender car state.</param>
        /// <param name="atkDrv">Attacker driver data (racecraft, morale).</param>
        /// <param name="defDrv">Defender driver data (defending, morale).</param>
        /// <param name="circuit">Circuit data (drsZones, overtakeDifficulty).</param>
        /// <param name="gapS">Current gap between the cars in seconds.</param>
        /// <param name="lap">Current race lap.</param>
        /// <param name="drsOn">Out: whether DRS was active for this calculation.</param>
        /// <returns>Clamped success probability in (0, <see cref="P_CAP"/>].</returns>
        public static float ComputeProbability(
            CarState   attacker,
            CarState   defender,
            DriverInfo atkDrv,
            DriverInfo defDrv,
            CircuitInfo circuit,
            float      gapS,
            int        lap,
            out bool   drsOn)
        {
            float skillDelta  = atkDrv.racecraft  - defDrv.defending;
            float paceD       = PaceDelta(attacker, defender);
            // Positive tire_delta = attacker has older tyres → negative contribution
            float tireDelta   = attacker.TireAgeLaps - defender.TireAgeLaps;
            drsOn             = DRSAvailable(gapS, circuit, lap);
            float trackDiff   = circuit.overtakeDifficulty;
            float aggDef      = DefenderAggression(defender, defDrv);

            double logit =
                  W1_SKILL  * skillDelta
                + W2_PACE   * paceD
                + W3_TIRE   * (-tireDelta)          // fresher attacker = positive
                + W4_DRS    * (drsOn ? 1f : 0f)
                - W5_TRACK  * trackDiff
                - W6_DEFEND * aggDef;

            float pRaw = (float)(1.0 / (1.0 + Math.Exp(-logit)));
            return Math.Min(pRaw, P_CAP);
        }

        /// <summary>
        /// Direct sigmoid computation from pre-calculated inputs.
        /// Used by tests and the player HUD to compute probability without full CarState objects.
        /// </summary>
        public static float ComputeSuccessProbability(
            float skillDelta,
            float paceDelta,
            float tireAgeDelta,
            bool  drsActive,
            float trackDifficulty,
            float defenderAggression)
        {
            double logit =
                  W1_SKILL  * skillDelta
                + W2_PACE   * paceDelta
                + W3_TIRE   * (-tireAgeDelta)
                + W4_DRS    * (drsActive ? 1f : 0f)
                - W5_TRACK  * trackDifficulty
                - W6_DEFEND * defenderAggression;

            float pRaw = (float)(1.0 / (1.0 + Math.Exp(-logit)));
            return Math.Min(pRaw, P_CAP);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Main resolver
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Resolves all eligible on-track overtake attempts for one lap tick.
        ///
        /// <para>Eligibility filter (mirrors Python overtake.py):</para>
        /// <list type="bullet">
        ///   <item>Both cars are active (not DNF, not pitting this lap).</item>
        ///   <item>Gap is &gt; 0 and &lt;= 1.2 s.</item>
        ///   <item>Cars are on the same lap (prevents trivial lap-down passes).</item>
        ///   <item>Attacker has ≥ 0.15 s/lap pace advantage, or ≥ 0.05 s with DRS.</item>
        /// </list>
        /// </summary>
        /// <param name="state">Current race state (sorted cars, current lap).</param>
        /// <param name="drivers">Map from driverID to DriverInfo ScriptableObject.</param>
        /// <param name="circuit">Circuit ScriptableObject.</param>
        /// <param name="rng">Seeded random instance.</param>
        /// <returns>List of resolved overtake attempts (successful and failed).</returns>
        public static List<OvertakeResult> ResolveOvertakes(
            RaceState                   state,
            Dictionary<int, DriverInfo> drivers,
            CircuitInfo                 circuit,
            System.Random               rng)
        {
            var results    = new List<OvertakeResult>();
            var sortedCars = state.SortedCars();

            for (int i = 1; i < sortedCars.Count; i++)
            {
                CarState attacker = sortedCars[i];
                CarState defender = sortedCars[i - 1];

                // Skip inactive or transitioning cars
                if (attacker.DNF || defender.DNF)                         continue;
                if (attacker.IsPittingThisLap || defender.IsPittingThisLap) continue;

                // Must be within striking distance
                if (attacker.GapToAheadS <= 0f || attacker.GapToAheadS > 1.2f) continue;

                // Must be on the same lap (avoid trivial lap-down passes)
                if (attacker.LapsCompleted != defender.LapsCompleted)     continue;

                // Pace-advantage filter
                float paceD  = PaceDelta(attacker, defender);
                bool  drsCheck = DRSAvailable(attacker.GapToAheadS, circuit, state.CurrentLap);
                float minPace = drsCheck ? 0.05f : 0.15f;
                if (paceD < minPace) continue;

                // Driver data lookup
                if (!drivers.TryGetValue(attacker.DriverID, out DriverInfo atkDrv)) continue;
                if (!drivers.TryGetValue(defender.DriverID, out DriverInfo defDrv)) continue;

                float pSuccess = ComputeProbability(
                    attacker, defender,
                    atkDrv, defDrv,
                    circuit,
                    attacker.GapToAheadS,
                    state.CurrentLap,
                    out bool drsOn);

                bool success = rng.NextDouble() < pSuccess;

                results.Add(new OvertakeResult(
                    attackerID:          attacker.DriverID,
                    defenderID:          defender.DriverID,
                    lap:                 state.CurrentLap,
                    success:             success,
                    pSuccess:            pSuccess,
                    wasDRS:              drsOn,
                    newPositionAttacker: success ? attacker.Position - 1 : attacker.Position
                ));
            }

            return results;
        }
    }
}
