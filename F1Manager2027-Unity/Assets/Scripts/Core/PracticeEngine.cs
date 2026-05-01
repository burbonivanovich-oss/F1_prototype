// PracticeEngine.cs — FP1/FP2/FP3 practice session simulation.
// Collects tire degradation and car balance data; produces a setup recommendation
// and a strategy window that feeds into qualifying and race strategy.
// Pure C#, no Unity dependencies.
using System;
using System.Collections.Generic;
using System.Linq;

namespace F1Manager
{
    // ─────────────────────────────────────────────────────────────────────────
    // Practice session types
    // ─────────────────────────────────────────────────────────────────────────

    public enum PracticeSessionType
    {
        FP1,   // Baseline setup, long run data, tire survey
        FP2,   // Race simulation, tire comparison, strategy validation
        FP3,   // Qualifying trim, final setup refinement
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Car setup (output of practice)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Car setup configuration tuned during practice.
    /// Each axis is -2 (extreme low) to +2 (extreme high).
    /// </summary>
    public class CarSetup
    {
        /// <summary>Wing angle: -2 = low drag (Monza), +2 = high downforce (Monaco).</summary>
        public int DownforceLevel;  // -2 to +2

        /// <summary>Brake bias: -2 = rear biased, +2 = front biased.</summary>
        public int BrakeBias;       // -2 to +2

        /// <summary>Suspension stiffness: -2 = soft (smoother), +2 = stiff (responsive).</summary>
        public int Suspension;      // -2 to +2

        /// <summary>Lap time delta from optimal setup in seconds (lower = better setup found).</summary>
        public float SetupPenaltyS;

        public string Describe()
        {
            string df = DownforceLevel < -1 ? "low-drag" : DownforceLevel > 1 ? "high-downforce" : "balanced";
            string bb = BrakeBias > 0 ? "front-biased" : BrakeBias < 0 ? "rear-biased" : "neutral";
            string sus = Suspension > 1 ? "stiff" : Suspension < -1 ? "soft" : "medium";
            return $"{df} wing, {bb} brakes, {sus} suspension (−{SetupPenaltyS:F2}s vs optimal)";
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Strategy recommendation (output of FP2)
    // ─────────────────────────────────────────────────────────────────────────

    public enum StrategyType { OneStop, TwoStop, ThreeStop }

    public class StrategyRecommendation
    {
        public StrategyType Strategy;

        /// <summary>Optimal lap to make the first pit stop.</summary>
        public int FirstPitLap;

        /// <summary>Optimal lap for second pit (TwoStop/ThreeStop only).</summary>
        public int SecondPitLap;   // 0 if not applicable

        /// <summary>Compound to start on.</summary>
        public TireCompound StartCompound;

        /// <summary>Compounds for each subsequent stint.</summary>
        public List<TireCompound> StintCompounds = new List<TireCompound>();

        /// <summary>Observed degradation multiplier (may differ slightly from circuit baseline).</summary>
        public float ObservedDegMultiplier;

        public string Describe()
        {
            string stints = string.Join("→", StintCompounds.Select(c => c.DisplayName()));
            string pit2   = SecondPitLap > 0 ? $"+L{SecondPitLap}" : "";
            return $"{Strategy} | Start: {StartCompound.DisplayName()} → {stints} | Pit L{FirstPitLap}{pit2}";
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tire run data collected per compound per session
    // ─────────────────────────────────────────────────────────────────────────

    public class TireRunData
    {
        public TireCompound Compound;
        public int          Laps;              // Laps run on this stint
        public float        AvgDegPerLapS;     // Observed deg per lap
        public float        WarmUpPenaltyS;    // Observed warm-up penalty
        public float        CliffLap;          // Estimated lap when cliff begins (circuit-adjusted)
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Practice run (one stint within a session)
    // ─────────────────────────────────────────────────────────────────────────

    public class PracticeRun
    {
        public int          DriverID;
        public TireCompound Compound;
        public int          Laps;
        public float        BestLapTimeS;
        public float        FirstLapTimeS;     // Warm laps delta
        public float        LastLapTimeS;      // End of stint pace
        public bool         IsLongRun;         // True for race-sim run
        public float        FuelLoad;          // Starting fuel kg (long run only)
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Session result
    // ─────────────────────────────────────────────────────────────────────────

    public class PracticeSessionResult
    {
        public PracticeSessionType Session;
        public List<PracticeRun>   Runs         = new List<PracticeRun>();
        public CarSetup            BestSetup     = new CarSetup();
        public StrategyRecommendation Recommendation; // Populated by FP2, null for FP1/FP3

        /// <summary>Estimated time gap gained vs a default (no-practice) setup.</summary>
        public float SetupGainS;

        /// <summary>Per-compound tire data observed in this session.</summary>
        public Dictionary<TireCompound, TireRunData> TireData
            = new Dictionary<TireCompound, TireRunData>();

        /// <summary>Balance feedback: oversteer/understeer or neutral.</summary>
        public string BalanceFeedback = "Neutral balance — no changes required.";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Weekend practice summary (aggregated after all three sessions)
    // ─────────────────────────────────────────────────────────────────────────

    public class PracticeWeekendSummary
    {
        public CarSetup               FinalSetup;
        public StrategyRecommendation FinalStrategy;

        /// <summary>Degradation multiplier adjustment discovered during practice (±0.10 from circuit baseline).</summary>
        public float DegMultAdjustment;

        /// <summary>
        /// Lap time improvement vs no-practice baseline.
        /// Feeds into race simulation as a bonus for player cars.
        /// </summary>
        public float TotalSetupGainS;

        public List<PracticeSessionResult> Sessions = new List<PracticeSessionResult>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PracticeEngine
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Simulates FP1, FP2, and FP3 practice sessions.
    /// Each session produces tire degradation data, balance feedback, and a car setup.
    /// FP2 additionally produces a strategy recommendation used in the race.
    /// </summary>
    public class PracticeEngine
    {
        private readonly CircuitInfo          _circuit;
        private readonly List<TireCompound>   _availableCompounds;
        private readonly Dictionary<int, DriverInfo> _drivers;
        private readonly Dictionary<int, TeamInfo>   _teams;
        private readonly System.Random        _rng;

        // Runs per session (short-run + long-run slots)
        private const int FP1_RUNS = 8;
        private const int FP2_RUNS = 10; // Includes long-run simulation
        private const int FP3_RUNS = 6;

        // Maximum practice lap time improvement (good setup vs none)
        private const float MAX_SETUP_GAIN_S = 0.35f;

        public PracticeEngine(
            CircuitInfo                  circuit,
            List<TireCompound>           availableCompounds,
            Dictionary<int, DriverInfo>  drivers,
            Dictionary<int, TeamInfo>    teams,
            System.Random                rng = null)
        {
            _circuit            = circuit;
            _availableCompounds = availableCompounds;
            _drivers            = drivers;
            _teams              = teams;
            _rng                = rng ?? new System.Random();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Simulates one full practice session and returns the result.</summary>
        public PracticeSessionResult SimulateSession(
            PracticeSessionType session,
            int playerTeamID,
            CarSetup previousSetup = null)
        {
            var result = new PracticeSessionResult { Session = session };
            var playerDriverIDs = _drivers.Values
                .Where(d => d.teamID == playerTeamID)
                .Select(d => d.id)
                .ToList();

            int runCount = session == PracticeSessionType.FP1 ? FP1_RUNS
                         : session == PracticeSessionType.FP2 ? FP2_RUNS
                         : FP3_RUNS;

            // Simulate runs for player drivers
            foreach (int driverID in playerDriverIDs)
            {
                var driver = _drivers[driverID];
                var team   = _teams[driver.teamID];
                SimulateDriverRuns(result, session, driver, team, runCount / 2);
            }

            // Collect tire data
            BuildTireData(result);

            // Determine best setup
            result.BestSetup  = OptimiseSetup(session, previousSetup);
            result.SetupGainS = CalculateSetupGain(session, previousSetup);

            // Balance feedback
            result.BalanceFeedback = GenerateBalanceFeedback(session, result.BestSetup);

            // Strategy recommendation (FP2 only, uses race-sim run data)
            if (session == PracticeSessionType.FP2)
                result.Recommendation = BuildStrategyRecommendation(result);

            return result;
        }

        /// <summary>
        /// Runs all three sessions in sequence and returns the weekend summary.
        /// </summary>
        public PracticeWeekendSummary SimulateFullWeekend(int playerTeamID)
        {
            var summary = new PracticeWeekendSummary();
            CarSetup setup = null;

            foreach (PracticeSessionType session in Enum.GetValues(typeof(PracticeSessionType)))
            {
                var result = SimulateSession(session, playerTeamID, setup);
                summary.Sessions.Add(result);
                setup = result.BestSetup;

                if (result.Recommendation != null)
                    summary.FinalStrategy = result.Recommendation;
            }

            summary.FinalSetup       = setup;
            summary.TotalSetupGainS  = Math.Min(summary.Sessions.Sum(s => s.SetupGainS), MAX_SETUP_GAIN_S);
            summary.DegMultAdjustment = ObservedDegAdjustment(summary);

            return summary;
        }

        // ── Internal simulation ───────────────────────────────────────────────

        private void SimulateDriverRuns(
            PracticeSessionResult result,
            PracticeSessionType   session,
            DriverInfo            driver,
            TeamInfo              team,
            int                   numRuns)
        {
            var dryCompounds = _availableCompounds.Where(c => c.IsDry()).ToList();

            for (int i = 0; i < numRuns; i++)
            {
                bool isLongRun = (session == PracticeSessionType.FP2) && (i == numRuns - 1);
                var  compound  = PickPracticeCompound(session, dryCompounds, i, numRuns);
                var  run       = SimulateRun(driver, team, compound, isLongRun);
                result.Runs.Add(run);
            }
        }

        private TireCompound PickPracticeCompound(
            PracticeSessionType session,
            List<TireCompound>  compounds,
            int                 runIndex,
            int                 totalRuns)
        {
            if (!compounds.Any()) return TireCompound.C4;

            switch (session)
            {
                case PracticeSessionType.FP1:
                    // Survey all compounds: start hard, end soft
                    return compounds[runIndex % compounds.Count];

                case PracticeSessionType.FP2:
                    // Long run on medium/hard; short runs on soft
                    if (runIndex == totalRuns - 1) return compounds[0]; // hardest for long run
                    return compounds[compounds.Count - 1];               // softest for short runs

                case PracticeSessionType.FP3:
                    // Qualifying simulation — softest compound
                    return compounds[compounds.Count - 1];

                default:
                    return compounds[1 % compounds.Count];
            }
        }

        private PracticeRun SimulateRun(
            DriverInfo  driver,
            TeamInfo    team,
            TireCompound compound,
            bool        isLongRun)
        {
            int laps = isLongRun
                ? Math.Max(10, _circuit.totalLaps / 3)
                : (int)(_rng.Next(3, 8));

            float baseTime = _circuit.baseLapTimeS;
            float carGap   = (team.carPerformance - 90) * -0.055f;
            float driverPace = (driver.pace - 80) * -0.015f;
            float noise    = (float)(_rng.NextDouble() * 0.4 - 0.2);

            var profile    = TireProfiles.All[compound];
            float warmUp   = profile.WarmUpPenaltyS;
            float bestLap  = baseTime + carGap + driverPace + compound.GripAdvantageS() + noise;
            float lastLap  = isLongRun
                ? bestLap + TireSystem.DegPenaltyS(profile, laps, _circuit.tireDegMultiplier)
                : bestLap + (float)(_rng.NextDouble() * 0.3);

            return new PracticeRun
            {
                DriverID      = driver.id,
                Compound      = compound,
                Laps          = laps,
                BestLapTimeS  = bestLap,
                FirstLapTimeS = bestLap + warmUp,
                LastLapTimeS  = lastLap,
                IsLongRun     = isLongRun,
                FuelLoad      = isLongRun ? laps * _circuit.fuelConsumptionKg : 0f,
            };
        }

        private void BuildTireData(PracticeSessionResult result)
        {
            var grouped = result.Runs.GroupBy(r => r.Compound);
            foreach (var group in grouped)
            {
                var compound = group.Key;
                if (!compound.IsDry()) continue;

                var profile  = TireProfiles.All[compound];
                float degMult = _circuit.tireDegMultiplier;
                float noise  = (float)(_rng.NextDouble() * 0.10 - 0.05); // ±5% observation noise

                int   cliffStart = (int)((profile.PlateauLaps + profile.LinearPhaseLaps) / degMult);
                float avgDeg     = profile.LinearDegPerLapS * degMult * (1f + noise);

                result.TireData[compound] = new TireRunData
                {
                    Compound        = compound,
                    Laps            = group.Max(r => r.Laps),
                    AvgDegPerLapS   = avgDeg,
                    WarmUpPenaltyS  = profile.WarmUpPenaltyS,
                    CliffLap        = cliffStart,
                };
            }
        }

        private CarSetup OptimiseSetup(PracticeSessionType session, CarSetup previous)
        {
            // Each session refines the setup further
            float progress = session == PracticeSessionType.FP1 ? 0.4f
                           : session == PracticeSessionType.FP2 ? 0.75f : 1.0f;

            // Ideal setup per circuit type
            int idealDownforce = _circuit.overtakeDifficulty > 0.7f ? 2 : // Monaco/Hungary
                                 _circuit.powerSensitivity    > 0.7f ? -2 : // Monza/Baku
                                 0; // balanced

            int idealBrakeBias   = (int)(_rng.Next(-1, 2));
            int idealSuspension  = _circuit.overtakeDifficulty > 0.6f ? 1 : -1;

            // Approach the ideal with noise based on session
            float noiseRange = (1f - progress) * 2f;
            int df  = Clamp((int)(idealDownforce + (_rng.NextDouble() * noiseRange - noiseRange / 2)), -2, 2);
            int bb  = Clamp(idealBrakeBias, -2, 2);
            int sus = Clamp((int)(idealSuspension + (_rng.NextDouble() * noiseRange - noiseRange / 2)), -2, 2);

            float penalty = (1f - progress) * 0.20f; // 0.20s on FP1, 0.05s on FP3

            return new CarSetup
            {
                DownforceLevel = df,
                BrakeBias      = bb,
                Suspension     = sus,
                SetupPenaltyS  = penalty,
            };
        }

        private float CalculateSetupGain(PracticeSessionType session, CarSetup previous)
        {
            float maxPerSession = MAX_SETUP_GAIN_S / 3f;
            float noise = (float)(_rng.NextDouble() * 0.05);
            switch (session)
            {
                case PracticeSessionType.FP1: return maxPerSession * 0.5f + noise;
                case PracticeSessionType.FP2: return maxPerSession * 0.35f + noise;
                case PracticeSessionType.FP3: return maxPerSession * 0.15f + noise;
                default:                      return 0f;
            }
        }

        private string GenerateBalanceFeedback(PracticeSessionType session, CarSetup setup)
        {
            // Generate realistic engineer-style balance feedback
            string[] oversteers = { "Rear is stepping out in slow corners — add rear downforce.", "Oversteer on exit — stiffen rear suspension.", "Rear instability in S2 — reduce rear wing angle." };
            string[] understeers = { "Understeer on corner entry — soften front suspension.", "Front is washing wide — increase front wing.", "Understeer in fast corners — lower ride height." };
            string[] neutrals    = { "Balance looks good — no significant changes required.", "Car is well-balanced — focus on tire prep for qualifying.", "Neutral feedback — minor brake bias adjustment suggested." };

            // Bias feedback based on circuit type
            double r = _rng.NextDouble();
            if (_circuit.overtakeDifficulty > 0.6f && r < 0.4)
                return oversteers[_rng.Next(oversteers.Length)];
            if (_circuit.powerSensitivity > 0.6f && r < 0.4)
                return understeers[_rng.Next(understeers.Length)];
            return neutrals[_rng.Next(neutrals.Length)];
        }

        private StrategyRecommendation BuildStrategyRecommendation(PracticeSessionResult fp2Result)
        {
            var dryComps    = _availableCompounds.Where(c => c.IsDry()).OrderBy(c => (int)c).ToList();
            if (!dryComps.Any()) dryComps.Add(TireCompound.C4);

            float degMult   = _circuit.tireDegMultiplier;
            int   totalLaps = _circuit.totalLaps;

            // Use observed deg or fall back to profile
            TireCompound startComp = dryComps.Last(); // softest
            var startProfile       = TireProfiles.All[startComp];
            int startWindow        = (int)((startProfile.PlateauLaps + startProfile.LinearPhaseLaps) / degMult);

            // Decide 1-stop vs 2-stop based on whether a hard compound can cover remaining laps
            TireCompound hardComp = dryComps.First();
            var hardProfile       = TireProfiles.All[hardComp];
            int hardWindow        = (int)((hardProfile.PlateauLaps + hardProfile.LinearPhaseLaps) / degMult);

            bool canOneStop = (startWindow + hardWindow) >= totalLaps * 0.95f;

            int firstPit = Math.Max(startWindow - 2, 5);
            firstPit     = Math.Min(firstPit, totalLaps - hardWindow - 2);

            var rec = new StrategyRecommendation
            {
                ObservedDegMultiplier = degMult,
                StartCompound         = startComp,
            };

            if (canOneStop)
            {
                rec.Strategy      = StrategyType.OneStop;
                rec.FirstPitLap   = firstPit;
                rec.SecondPitLap  = 0;
                rec.StintCompounds.Add(hardComp);
            }
            else
            {
                // 2-stop: soft → medium → hard
                TireCompound midComp = dryComps.Count >= 2 ? dryComps[dryComps.Count / 2] : hardComp;
                var midProfile       = TireProfiles.All[midComp];
                int midWindow        = (int)((midProfile.PlateauLaps + midProfile.LinearPhaseLaps) / degMult);

                rec.Strategy         = StrategyType.TwoStop;
                rec.FirstPitLap      = firstPit;
                rec.SecondPitLap     = Math.Min(firstPit + midWindow - 2, totalLaps - 10);
                rec.StintCompounds.Add(midComp);
                rec.StintCompounds.Add(hardComp);
            }

            return rec;
        }

        private float ObservedDegAdjustment(PracticeWeekendSummary summary)
        {
            // Average deviation from circuit baseline across all tire data
            var allData = summary.Sessions
                .SelectMany(s => s.TireData.Values)
                .ToList();

            if (!allData.Any()) return 0f;

            float baselineDeg = _availableCompounds
                .Where(c => c.IsDry() && TireProfiles.All.ContainsKey(c))
                .Select(c => TireProfiles.All[c].LinearDegPerLapS * _circuit.tireDegMultiplier)
                .DefaultIfEmpty(0.04f)
                .Average();

            float observedDeg = allData.Average(d => d.AvgDegPerLapS);
            return Math.Max(-0.10f, Math.Min(0.10f, observedDeg - baselineDeg));
        }

        private static int Clamp(int v, int lo, int hi)
            => v < lo ? lo : v > hi ? hi : v;
    }
}
