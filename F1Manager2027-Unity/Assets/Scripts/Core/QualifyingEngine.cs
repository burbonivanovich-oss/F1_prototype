// QualifyingEngine.cs — Q1/Q2/Q3 elimination qualifying simulation.
// Pure C#, no Unity dependencies. Mirrors Python core/qualifying.py.
using System;
using System.Collections.Generic;
using System.Linq;

namespace F1Manager
{
    // ── Data classes ──────────────────────────────────────────────────────────

    public class QualifyingLap
    {
        public int          DriverID;
        public TireCompound Compound;
        public float        LapTimeS;
        public bool         IsImpeded;
        public int          SessionPart;
    }

    public class SessionResult
    {
        public int                 Part;
        /// <summary>All individual lap records logged during the session.</summary>
        public List<QualifyingLap> Laps = new List<QualifyingLap>();
        public List<int>           EliminatedDriverIDs = new List<int>();

        /// <summary>Best lap per driver, sorted fastest first. Alias for compatibility with spec.</summary>
        public List<QualifyingLap> Results => BestLaps();

        public List<QualifyingLap> BestLaps()
        {
            var best = new Dictionary<int, QualifyingLap>();
            foreach (var lap in Laps)
                if (!best.ContainsKey(lap.DriverID) || lap.LapTimeS < best[lap.DriverID].LapTimeS)
                    best[lap.DriverID] = lap;
            return best.Values.OrderBy(l => l.LapTimeS).ToList();
        }
    }

    public class QualifyingResult
    {
        public SessionResult[]              Sessions      = new SessionResult[3];
        public List<int>                    GridOrder     = new List<int>();
        public Dictionary<int,TireCompound> Q2CompoundMap = new Dictionary<int,TireCompound>();
        public float                        PoleTimeS;
        public int                          PoleSitterID  = -1;
    }

    // ─────────────────────────────────────────────────────────────────────────

    public class QualifyingEngine
    {
        private readonly CircuitInfo                 _circuit;
        private readonly Dictionary<int, DriverInfo> _drivers;
        private readonly Dictionary<int, TeamInfo>   _teams;
        private readonly System.Random               _rng;

        public QualifyingEngine(
            CircuitInfo                 circuit,
            Dictionary<int, DriverInfo> drivers,
            Dictionary<int, TeamInfo>   teams,
            System.Random               rng)
        {
            _circuit = circuit;
            _drivers = drivers;
            _teams   = teams;
            _rng     = rng;
        }

        public QualifyingResult RunQualifying(List<TireCompound> availableCompounds, int playerTeamID)
        {
            var result = new QualifyingResult();
            var dry = availableCompounds.Where(c => c.IsDry()).OrderBy(c => (int)c).ToList();

            var activeIDs = _drivers.Values
                .Where(d => !d.isReserve)
                .OrderBy(d => d.id)
                .Select(d => d.id)
                .ToList();

            // Q1: 20 → 15
            var s1 = RunSession(1, activeIDs, dry);
            result.Sessions[0] = s1;
            var s1Best = s1.BestLaps();
            s1.EliminatedDriverIDs = s1Best.Skip(15).Select(l => l.DriverID).ToList();
            var q2IDs = s1Best.Take(15).Select(l => l.DriverID).ToList();

            // Q2: 15 → 10
            var s2 = RunSession(2, q2IDs, dry);
            result.Sessions[1] = s2;
            var s2Best = s2.BestLaps();
            s2.EliminatedDriverIDs = s2Best.Skip(10).Select(l => l.DriverID).ToList();
            var q3IDs = s2Best.Take(10).Select(l => l.DriverID).ToList();

            // Q2 compound rule: top-10 must start race on their Q2 compound
            foreach (var lap in s2Best.Take(10))
                result.Q2CompoundMap[lap.DriverID] = lap.Compound;

            // Q3: 10 cars
            var s3 = RunSession(3, q3IDs, dry);
            result.Sessions[2] = s3;
            var s3Best = s3.BestLaps();

            // Grid: Q3 (P1-10), Q2 eliminees (P11-15), Q1 eliminees (P16-20)
            result.GridOrder.AddRange(s3Best.Select(l => l.DriverID));
            result.GridOrder.AddRange(s2.EliminatedDriverIDs);
            result.GridOrder.AddRange(s1.EliminatedDriverIDs);

            if (s3Best.Count > 0)
            {
                result.PoleSitterID = s3Best[0].DriverID;
                result.PoleTimeS    = s3Best[0].LapTimeS;
            }

            return result;
        }

        // ── Public per-driver lap (for interactive UI use) ─────────────────────

        /// <summary>
        /// Simulates the player's qualifying lap interactively.
        /// Mirrors the spec's RunQualifyingLapForDriver signature for UI integration.
        /// </summary>
        /// <param name="driverID">Driver taking the lap.</param>
        /// <param name="compound">Compound chosen for this attempt.</param>
        /// <param name="sessionPart">Session part (1/2/3) — affects track evolution baseline.</param>
        public float RunQualifyingLapForDriver(int driverID, TireCompound compound, int sessionPart)
        {
            if (!_drivers.TryGetValue(driverID, out var drv))  return 9999f;
            if (!_teams.TryGetValue(drv.teamID,  out var team)) return 9999f;

            // Mid-session lap: use a mid-point lap index in a 20-lap session window
            float evoBase;
            switch (sessionPart)
            {
                case 2:  evoBase = -0.05f; break;
                case 3:  evoBase = -0.10f; break;
                default: evoBase =  0.00f; break;
            }
            return SimulateLap(drv, team, compound, lapIdx: 10, totalLaps: 20, part: sessionPart)
                   + evoBase;
        }

        // ── Public lap time formula (exposed for UI compound comparison) ────────

        /// <summary>
        /// Returns a simulated qualifying lap time for the given driver, team, and compound.
        /// Track evolution is applied based on <paramref name="lapIndex"/> position in the session.
        /// </summary>
        /// <param name="driver">Driver stats.</param>
        /// <param name="team">Team/car stats.</param>
        /// <param name="compound">Tyre compound used.</param>
        /// <param name="lapIndex">Zero-based run order position (0 = first car out, n = last).</param>
        /// <param name="totalLapsInSession">Total lap slots in the session for evo scaling.</param>
        public float SimulateQualifyingLap(
            DriverInfo   driver,
            TeamInfo     team,
            TireCompound compound,
            int          lapIndex,
            int          totalLapsInSession)
            => SimulateLap(driver, team, compound, lapIndex, totalLapsInSession, part: 1);

        // ─────────────────────────────────────────────────────────────────────

        private SessionResult RunSession(int part, List<int> ids, List<TireCompound> dry)
        {
            var session = new SessionResult { Part = part };
            int total = ids.Count * 2;
            int idx = 0;
            for (int run = 0; run < 2; run++)
            {
                var order = ids.OrderBy(_ => _rng.NextDouble()).ToList();
                foreach (int id in order)
                {
                    if (!_drivers.TryGetValue(id, out var drv)) continue;
                    if (!_teams.TryGetValue(drv.teamID, out var team)) continue;
                    TireCompound cmpd = ChooseCompound(dry, part, run);
                    float t = SimulateLap(drv, team, cmpd, idx, total, part);
                    bool impeded = IsImpeded();
                    if (impeded) t += (float)(_rng.NextDouble() * 0.6 + 0.2);
                    session.Laps.Add(new QualifyingLap
                        { DriverID=id, Compound=cmpd, LapTimeS=t, IsImpeded=impeded, SessionPart=part });
                    idx++;
                }
            }
            return session;
        }

        private float SimulateLap(DriverInfo drv, TeamInfo team, TireCompound cmpd,
            int lapIdx, int totalLaps, int part)
        {
            float carGap  = (100f - team.carPerformance) * 0.050f;
            float ps      = _circuit.powerSensitivity;
            carGap -= (team.powerUnit - 90f) * ps * 0.015f + (team.chassis - 90f) * (1f - ps) * 0.015f;
            float drvGain = (drv.pace - 75f) * 0.012f;
            float evo     = -0.20f * (lapIdx / (float)Math.Max(1, totalLaps - 1));
            float noise   = NextGaussian(0f, 0.08f);
            return _circuit.baseLapTimeS + carGap - drvGain + evo + cmpd.GripAdvantageS() + noise;
        }

        private TireCompound ChooseCompound(List<TireCompound> dry, int part, int run)
        {
            if (dry.Count == 0) return TireCompound.C5;
            TireCompound softest = dry[dry.Count - 1];
            TireCompound medium  = dry.Count >= 2 ? dry[dry.Count - 2] : softest;
            if (part == 1 || part == 3) return softest;
            return _rng.NextDouble() < 0.80 ? softest : medium;
        }

        private bool IsImpeded()
            => _rng.NextDouble() < (_circuit.overtakeDifficulty >= 0.80f ? 0.15 : 0.04);

        private float NextGaussian(float mean, float sigma)
        {
            double u1 = 1.0 - _rng.NextDouble();
            double u2 = 1.0 - _rng.NextDouble();
            double z  = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            return (float)(mean + sigma * z);
        }
    }
}
