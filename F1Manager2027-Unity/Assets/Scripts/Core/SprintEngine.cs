// SprintEngine.cs — Sprint weekend format: Sprint Qualifying (SQ1/SQ2/SQ3) + 100 km sprint race.
// 6 of the 24 calendar rounds are sprint weekends.
// Sprint points: P1=8, P2=7 … P8=1. No two-compound rule. No fastest-lap bonus.
// Pure C#, no Unity dependencies.
using System;
using System.Collections.Generic;
using System.Linq;

namespace F1Manager
{
    // ─────────────────────────────────────────────────────────────────────────
    // Sprint Qualifying result
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Outcome of one Sprint Qualifying session (SQ1/SQ2/SQ3).</summary>
    public class SprintQualifyingSession
    {
        public int                 Part;             // 1=SQ1, 2=SQ2, 3=SQ3
        public int                 DurationMinutes;  // 12/10/8
        public List<QualifyingLap> Laps    = new List<QualifyingLap>();
        public List<int>           EliminatedDriverIDs = new List<int>();

        public List<QualifyingLap> BestLaps()
        {
            var best = new Dictionary<int, QualifyingLap>();
            foreach (var lap in Laps)
                if (!best.ContainsKey(lap.DriverID) || lap.LapTimeS < best[lap.DriverID].LapTimeS)
                    best[lap.DriverID] = lap;
            return best.Values.OrderBy(l => l.LapTimeS).ToList();
        }
    }

    /// <summary>Complete Sprint Qualifying outcome: SQ1→SQ2→SQ3, producing the sprint grid.</summary>
    public class SprintQualifyingResult
    {
        public SprintQualifyingSession[] Sessions = new SprintQualifyingSession[3]; // [0]=SQ1,[1]=SQ2,[2]=SQ3
        public List<int>                 GridOrder = new List<int>();
        public float                     PoleTimeS;
        public int                       PoleSitterID = -1;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Sprint race result
    // ─────────────────────────────────────────────────────────────────────────

    public class SprintRaceResult
    {
        /// <summary>Full RaceState at race completion (contains final standings, events, etc.).</summary>
        public RaceState FinalState;

        /// <summary>driverID → sprint points awarded (0–8).</summary>
        public Dictionary<int, int> PointsAwarded = new Dictionary<int, int>();

        /// <summary>Finishing order (driverIDs, leader first).</summary>
        public List<int> FinishingOrder = new List<int>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SprintEngine
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Orchestrates a full sprint weekend:
    /// 1. Sprint Qualifying (SQ1/SQ2/SQ3) — produces the sprint grid.
    /// 2. Sprint Race (~100 km) — runs via <see cref="RaceEngine"/>, awards sprint points.
    ///
    /// The main Grand Prix grid comes from the normal Friday qualifying session
    /// run separately via <see cref="QualifyingEngine"/>.
    /// </summary>
    public class SprintEngine
    {
        private readonly CircuitInfo                 _circuit;
        private readonly Dictionary<int, DriverInfo> _drivers;
        private readonly Dictionary<int, TeamInfo>   _teams;
        private readonly int                         _playerTeamID;
        private readonly System.Random               _rng;

        // Sprint qualifying cut-off sizes: SQ1=20→15, SQ2=15→10, SQ3=10→pole
        private const int SQ1_ELIMINATE_TO = 15;
        private const int SQ2_ELIMINATE_TO = 10;
        private const int SQ3_FIELD        = 10;

        // Sprint qualifying session durations (minutes)
        private static readonly int[] SQ_DURATIONS = { 12, 10, 8 };

        // Sprint race target distance: 100 km
        private const float SPRINT_DISTANCE_KM = 100f;

        public SprintEngine(
            CircuitInfo                  circuit,
            Dictionary<int, DriverInfo>  drivers,
            Dictionary<int, TeamInfo>    teams,
            int                          playerTeamID,
            System.Random                rng = null)
        {
            _circuit      = circuit;
            _drivers      = drivers;
            _teams        = teams;
            _playerTeamID = playerTeamID;
            _rng          = rng ?? new System.Random();
        }

        // ── Sprint Qualifying ─────────────────────────────────────────────────

        /// <summary>
        /// Runs the full Sprint Qualifying (SQ1→SQ2→SQ3) simulation.
        /// Returns the sprint grid order (driverID list, pole first).
        /// </summary>
        public SprintQualifyingResult RunSprintQualifying()
        {
            var result    = new SprintQualifyingResult();
            var remaining = _drivers.Keys.ToList();

            for (int part = 1; part <= 3; part++)
            {
                var session = SimulateSQSession(part, remaining);
                result.Sessions[part - 1] = session;

                var bestLaps = session.BestLaps();

                // Determine cut-off
                int keepCount = part == 1 ? SQ1_ELIMINATE_TO
                              : part == 2 ? SQ2_ELIMINATE_TO
                              : SQ3_FIELD;

                // Eliminated = slowest drivers beyond cutoff
                var eliminated = bestLaps.Skip(keepCount).Select(l => l.DriverID).ToList();
                session.EliminatedDriverIDs.AddRange(eliminated);

                // Drivers with no timed lap are also eliminated
                var timedIDs   = bestLaps.Select(l => l.DriverID).ToHashSet();
                foreach (int id in remaining.Where(id => !timedIDs.Contains(id)))
                    session.EliminatedDriverIDs.Add(id);

                remaining = bestLaps.Take(keepCount).Select(l => l.DriverID).ToList();
            }

            // Build final grid: SQ3 order first, then SQ2 eliminated, then SQ1 eliminated
            for (int part = 2; part >= 0; part--)
            {
                if (part == 2)
                {
                    // SQ3 — final session: sorted by best lap
                    var sq3 = result.Sessions[2].BestLaps();
                    result.GridOrder.AddRange(sq3.Select(l => l.DriverID));
                    var pole = sq3.FirstOrDefault();
                    if (pole != null)
                    {
                        result.PoleSitterID = pole.DriverID;
                        result.PoleTimeS    = pole.LapTimeS;
                    }
                }
                else
                {
                    // Eliminated from SQ1/SQ2: add in reverse order (fastest eliminated first)
                    var elimLaps = result.Sessions[part].Laps
                        .GroupBy(l => l.DriverID)
                        .Select(g => g.OrderBy(l => l.LapTimeS).First())
                        .Where(l => result.Sessions[part].EliminatedDriverIDs.Contains(l.DriverID))
                        .OrderBy(l => l.LapTimeS)
                        .Select(l => l.DriverID)
                        .ToList();
                    result.GridOrder.AddRange(elimLaps);

                    // No-time drivers at back
                    var noTime = result.Sessions[part].EliminatedDriverIDs
                        .Where(id => !result.Sessions[part].Laps.Any(l => l.DriverID == id));
                    result.GridOrder.AddRange(noTime);
                }
            }

            return result;
        }

        private SprintQualifyingSession SimulateSQSession(int part, List<int> driverIDs)
        {
            var session = new SprintQualifyingSession
            {
                Part            = part,
                DurationMinutes = SQ_DURATIONS[part - 1],
            };

            // Track rubber evolution: up to −0.15s over the session
            float trackEvolution = -0.15f;
            int   lapsPerDriver  = 2 + (int)(_rng.NextDouble() * 2); // 2–3 flying laps
            float evolStep       = trackEvolution / (driverIDs.Count * lapsPerDriver);
            float evolAccum      = 0f;

            // All drivers attempt laps — softest compound available
            var availableComps = _circuit.id <= 23
                ? GetAvailableCompounds()
                : new List<TireCompound> { TireCompound.C5 };
            TireCompound comp = availableComps.Last(); // softest

            foreach (int driverID in driverIDs)
            {
                var driver = _drivers[driverID];
                var team   = _teams[driver.teamID];

                for (int attempt = 0; attempt < lapsPerDriver; attempt++)
                {
                    evolAccum += evolStep;
                    float lapTime = SimulateQualifyingLap(driver, team, comp, evolAccum);

                    // SQ qualifying: lower impeded chance than main qualifying
                    bool impeded = (_circuit.overtakeDifficulty > 0.6f) && (_rng.NextDouble() < 0.08f);
                    if (impeded) lapTime += (float)(_rng.NextDouble() * 0.8 + 0.3);

                    session.Laps.Add(new QualifyingLap
                    {
                        DriverID     = driverID,
                        Compound     = comp,
                        LapTimeS     = lapTime,
                        IsImpeded    = impeded,
                        SessionPart  = part,
                    });
                }
            }

            return session;
        }

        private float SimulateQualifyingLap(
            DriverInfo   driver,
            TeamInfo     team,
            TireCompound compound,
            float        trackEvolution)
        {
            float baseTime   = _circuit.baseLapTimeS;
            float carGap     = (team.carPerformance - 90) * -0.055f;
            float driverPace = (driver.pace - 80) * -0.020f;
            float gripBonus  = compound.GripAdvantageS();
            float noise      = (float)((_rng.NextDouble() - 0.5) * 0.25);

            // Power sensitivity adjustment
            float ps     = _circuit.powerSensitivity;
            float pwrAdj = (team.powerUnit - 90) * ps * -0.015f;
            float aeroAdj= (team.chassis    - 90) * (1f - ps) * -0.015f;

            return baseTime + carGap + driverPace + gripBonus + noise + pwrAdj + aeroAdj + trackEvolution;
        }

        // ── Sprint Race ───────────────────────────────────────────────────────

        /// <summary>
        /// Simulates the sprint race (100 km) and returns the result with points awarded.
        /// Uses the existing <see cref="RaceEngine"/> with a reduced lap count.
        /// </summary>
        public SprintRaceResult RunSprintRace(SprintQualifyingResult sqResult)
        {
            // Sprint lap count = 100 km / circuit length, rounded to nearest lap
            int sprintLaps = SprintLapCount();

            // Build a modified circuit for the sprint (same data, fewer laps)
            var sprintCircuit = CloneCircuitWithLaps(_circuit, sprintLaps);

            var engine = new RaceEngine(
                circuit:          sprintCircuit,
                teams:            _teams,
                drivers:          _drivers,
                playerTeamID:     _playerTeamID,
                rng:              new System.Random(_rng.Next()),
                qualifyingResult: BuildQualifyingResultFromSQ(sqResult));

            // Simulate all laps
            while (!engine.RaceState.IsRaceComplete)
                engine.SimulateLap();

            // Compile result
            var raceResult = new SprintRaceResult { FinalState = engine.RaceState };
            var sorted     = engine.RaceState.SortedCars();

            for (int i = 0; i < sorted.Count; i++)
            {
                int driverID = sorted[i].DriverID;
                bool dnf     = sorted[i].DNF;
                int pos      = i + 1;
                int pts      = dnf ? 0 : PointsTables.ForPosition(pos, isSprint: true);
                raceResult.PointsAwarded[driverID] = pts;
                raceResult.FinishingOrder.Add(driverID);
            }

            return raceResult;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Number of laps in the sprint race (≈100 km).
        /// Always at least 10 laps; capped at total race laps − 1.
        /// </summary>
        public int SprintLapCount()
        {
            float len   = _circuit.circuitLengthKm > 0f ? _circuit.circuitLengthKm : 5.0f;
            int   laps  = (int)Math.Round(SPRINT_DISTANCE_KM / len);
            return Math.Max(10, Math.Min(laps, _circuit.totalLaps - 1));
        }

        private List<TireCompound> GetAvailableCompounds()
        {
            // Fall back to a generic C3/C4/C5 selection if circuit has no explicit data
            return new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 };
        }

        private static CircuitInfo CloneCircuitWithLaps(CircuitInfo src, int laps)
        {
            // Shallow clone + override totalLaps only
            return new CircuitInfo
            {
                id                   = src.id,
                circuitName          = src.circuitName + " (Sprint)",
                country              = src.country,
                city                 = src.city,
                totalLaps            = laps,
                baseLapTimeS         = src.baseLapTimeS,
                circuitLengthKm      = src.circuitLengthKm,
                tireDegMultiplier    = src.tireDegMultiplier,
                overtakeDifficulty   = src.overtakeDifficulty,
                drsZones             = src.drsZones,
                fuelConsumptionKg    = src.fuelConsumptionKg,
                rainProbability      = src.rainProbability * 0.5f, // Halved for sprint
                trackTempRangeMin    = src.trackTempRangeMin,
                trackTempRangeMax    = src.trackTempRangeMax,
                safetyCarlProbability = src.safetyCarlProbability * 0.7f,
                sectorSplits         = src.sectorSplits,
                powerSensitivity     = src.powerSensitivity,
            };
        }

        private QualifyingResult BuildQualifyingResultFromSQ(SprintQualifyingResult sq)
        {
            // Wrap sprint qualifying grid into the QualifyingResult format that RaceEngine expects
            var qr = new QualifyingResult
            {
                GridOrder     = new List<int>(sq.GridOrder),
                PoleTimeS     = sq.PoleTimeS,
                PoleSitterID  = sq.PoleSitterID,
            };

            // Sessions: SQ maps to Q1/Q2/Q3 slots
            for (int i = 0; i < 3; i++)
            {
                var sq_sess = sq.Sessions[i];
                var sr      = new SessionResult { Part = i + 1 };
                if (sq_sess != null)
                    sr.Laps.AddRange(sq_sess.Laps);
                qr.Sessions[i] = sr;
            }

            // No Q2 compound rule for sprint
            return qr;
        }
    }
}
