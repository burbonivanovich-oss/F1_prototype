// SeasonEngine.cs — Championship season management: points, standings, 24-race schedule,
// EDUO power-unit tracking, and end-of-season prize money distribution.
// Pure C#, no Unity dependencies.
using System;
using System.Collections.Generic;
using System.Linq;

namespace F1Manager
{
    // ─────────────────────────────────────────────────────────────────────────
    // Calendar
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>One entry in the 24-round season calendar.</summary>
    public class CalendarEntry
    {
        public int    Round;        // 1-indexed round number
        public int    CircuitID;    // Maps to CircuitInfo.id
        public bool   IsSprint;     // Sprint weekend (6 per season)
        public string DateLabel;    // Human-readable date string ("Mar 2")

        public CalendarEntry(int round, int circuitID, bool isSprint, string dateLabel)
        {
            Round      = round;
            CircuitID  = circuitID;
            IsSprint   = isSprint;
            DateLabel  = dateLabel;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Standings rows
    // ─────────────────────────────────────────────────────────────────────────

    public class DriverStanding
    {
        public int DriverID;
        public int Points;
        public int Wins;
        public int Podiums;     // P1+P2+P3
        public int Poles;
        public int FastestLaps; // Stat only — no bonus points (rule removed 2025)
        public int Races;       // Races started

        public DriverStanding(int driverID)
        {
            DriverID = driverID;
        }
    }

    public class ConstructorStanding
    {
        public int TeamID;
        public int Points;
        public int Wins;
        public int Podiums;
        public int Races;
        public int PrizeMoney;  // End-of-season distribution in million USD

        public ConstructorStanding(int teamID)
        {
            TeamID = teamID;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Race result record (persisted between races)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Compact result record stored after each race, used for standings and history.</summary>
    public class RaceResultRecord
    {
        public int Round;
        public int CircuitID;
        public bool IsSprint;

        /// <summary>Finishing order: index = position (0-based), value = driverID. DNFs at end.</summary>
        public List<int> FinishingOrder = new List<int>();

        /// <summary>driverID → fastest lap time (statistics only, no points awarded).</summary>
        public Dictionary<int, float> FastestLaps = new Dictionary<int, float>();

        /// <summary>driverID → pole lap time. -1 if not available.</summary>
        public int PoleSitterID = -1;

        /// <summary>driverIDs that DNF'd.</summary>
        public HashSet<int> DNFs = new HashSet<int>();

        /// <summary>driverIDs that DNF'd due to mechanical failure (triggers EDUO emergency change).</summary>
        public HashSet<int> MechanicalDNFs = new HashSet<int>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // EDUO — Engine Development Unlock Order / Power Unit tracking
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Tracks power unit component usage per driver across the season.
    /// Exceeding the FIA allocation triggers grid penalties at checkpoint rounds.
    /// Regulation: max 3 ICE, 2 MGU-K, 2 MGU-H, 2 TC, 2 ES, 2 CE per driver per season.
    /// </summary>
    public class DriverPUUsage
    {
        public int DriverID;

        // Components currently fitted (start with 1 of each at season open)
        public int ICEUsed  = 1;
        public int MGUKUsed = 1;
        public int MGUHUsed = 1;
        public int TCUsed   = 1;
        public int ESUsed   = 1;
        public int CEUsed   = 1;

        // FIA 2025-2027 allocation limits
        public const int MaxICE  = 3;
        public const int MaxMGUK = 2;
        public const int MaxMGUH = 2;
        public const int MaxTC   = 2;
        public const int MaxES   = 2;
        public const int MaxCE   = 2;

        public DriverPUUsage(int driverID)
        {
            DriverID = driverID;
        }

        /// <summary>
        /// Total grid penalty places earned by exceeding allocation.
        /// New ICE = 10 places, each other component = 5 places.
        /// </summary>
        public int TotalPenaltyPlaces()
        {
            int p = 0;
            p += Math.Max(0, ICEUsed  - MaxICE)  * 10;
            p += Math.Max(0, MGUKUsed - MaxMGUK) * 5;
            p += Math.Max(0, MGUHUsed - MaxMGUH) * 5;
            p += Math.Max(0, TCUsed   - MaxTC)   * 5;
            p += Math.Max(0, ESUsed   - MaxES)   * 5;
            p += Math.Max(0, CEUsed   - MaxCE)   * 5;
            return p;
        }
    }

    /// <summary>A grid penalty generated by an EDUO checkpoint, applied to the next race.</summary>
    public class EDUOPenalty
    {
        public int    DriverID;
        public int    GridPenaltyPlaces;
        public int    ApplicableRound;  // Round where the penalty applies
        public string Reason;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Season state (serialisable between sessions)
    // ─────────────────────────────────────────────────────────────────────────

    public class SeasonState
    {
        public int  Season = 2027;
        public int  CurrentRound = 1;   // 1-indexed, next race to run
        public bool IsSeasonComplete => CurrentRound > Calendar.Count;

        public List<CalendarEntry>       Calendar             = new List<CalendarEntry>();
        public List<DriverStanding>      DriverStandings      = new List<DriverStanding>();
        public List<ConstructorStanding> ConstructorStandings = new List<ConstructorStanding>();
        public List<RaceResultRecord>    History              = new List<RaceResultRecord>();

        // EDUO tracking
        public Dictionary<int, DriverPUUsage> PUUsage             = new Dictionary<int, DriverPUUsage>();
        public List<EDUOPenalty>              PendingGridPenalties = new List<EDUOPenalty>();

        // Player's team ID (set at season start)
        public int PlayerTeamID;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Points tables
    // ─────────────────────────────────────────────────────────────────────────

    public static class PointsTables
    {
        /// <summary>Standard race points: P1=25 … P10=1. No fastest lap bonus (removed 2025).</summary>
        public static readonly int[] Race = { 25, 18, 15, 12, 10, 8, 6, 4, 2, 1 };

        /// <summary>Sprint race points: P1=8 … P8=1.</summary>
        public static readonly int[] Sprint = { 8, 7, 6, 5, 4, 3, 2, 1 };

        /// <summary>Returns race points for a given 1-based position, 0 if outside top 10.</summary>
        public static int ForPosition(int position, bool isSprint)
        {
            int[] table = isSprint ? Sprint : Race;
            int idx = position - 1;
            if (idx < 0 || idx >= table.Length) return 0;
            return table[idx];
        }

        /// <summary>
        /// End-of-season prize money by constructor championship position (million USD).
        /// Total pool ≈ $450M, distributed P1–P10 on sliding scale.
        /// </summary>
        public static readonly int[] PrizeMoney = { 69, 61, 55, 50, 45, 41, 37, 33, 30, 27 };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SeasonEngine
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Manages the full 24-round championship: calendar, points tally, standings,
    /// EDUO power-unit penalty tracking, and prize money distribution.
    /// </summary>
    public class SeasonEngine
    {
        public SeasonState State { get; private set; }

        // EDUO checkpoint rounds: mid-way checks on power unit usage
        private static readonly HashSet<int> _EDUOCheckpoints = new HashSet<int> { 6, 12, 18 };

        // ── Construction ──────────────────────────────────────────────────────

        /// <summary>Start a brand-new season for the given player team.</summary>
        public SeasonEngine(int playerTeamID, int season = 2027)
        {
            State = new SeasonState
            {
                Season       = season,
                PlayerTeamID = playerTeamID,
            };
            State.Calendar = BuildCalendar();
            InitStandings();
        }

        /// <summary>Resume from a saved SeasonState.</summary>
        public SeasonEngine(SeasonState saved)
        {
            State = saved;
        }

        // ── Calendar ──────────────────────────────────────────────────────────

        /// <summary>
        /// 24-round calendar in authentic 2027 order.
        /// Sprint weekends: rounds 5, 6, 11, 19, 21, 23.
        /// Circuit IDs map to StaticGameData circuit indices (0-based).
        /// </summary>
        public static List<CalendarEntry> BuildCalendar()
        {
            var entries = new (int r, int c, bool s, string d)[]
            {
                (1,  0,  false, "Mar 2"),    // Bahrain GP
                (2,  1,  false, "Mar 16"),   // Saudi Arabian GP
                (3,  2,  false, "Mar 30"),   // Australian GP
                (4,  3,  false, "Apr 6"),    // Japanese GP
                (5,  4,  true,  "Apr 20"),   // Chinese GP ★ Sprint
                (6,  5,  true,  "May 4"),    // Miami GP ★ Sprint
                (7,  6,  false, "May 18"),   // Emilia Romagna GP
                (8,  7,  false, "May 25"),   // Monaco GP
                (9,  8,  false, "Jun 1"),    // Spanish GP
                (10, 9,  false, "Jun 15"),   // Canadian GP
                (11, 10, true,  "Jun 29"),   // Austrian GP ★ Sprint
                (12, 11, false, "Jul 6"),    // British GP
                (13, 12, false, "Jul 27"),   // Belgian GP
                (14, 13, false, "Aug 3"),    // Hungarian GP
                (15, 14, false, "Aug 24"),   // Dutch GP
                (16, 15, false, "Sep 7"),    // Italian GP
                (17, 16, false, "Sep 21"),   // Azerbaijan GP
                (18, 17, false, "Oct 5"),    // Singapore GP
                (19, 18, true,  "Oct 19"),   // United States GP ★ Sprint
                (20, 19, false, "Oct 26"),   // Mexican GP
                (21, 20, true,  "Nov 9"),    // São Paulo GP ★ Sprint
                (22, 21, false, "Nov 22"),   // Las Vegas GP
                (23, 22, true,  "Nov 30"),   // Qatar GP ★ Sprint
                (24, 23, false, "Dec 7"),    // Abu Dhabi GP
            };

            var cal = new List<CalendarEntry>();
            foreach (var (r, c, s, d) in entries)
                cal.Add(new CalendarEntry(r, c, s, d));
            return cal;
        }

        // ── Standings initialisation ──────────────────────────────────────────

        private void InitStandings()
        {
            GameDataFactory.Initialize();

            foreach (var driver in GameDataFactory.Drivers)
            {
                State.DriverStandings.Add(new DriverStanding(driver.id));
                if (!driver.isReserve)
                    State.PUUsage[driver.id] = new DriverPUUsage(driver.id);
            }

            foreach (var team in GameDataFactory.Teams)
                State.ConstructorStandings.Add(new ConstructorStanding(team.id));
        }

        // ── Calendar helpers ──────────────────────────────────────────────────

        /// <summary>Returns the CalendarEntry for the current round (null if season is over).</summary>
        public CalendarEntry CurrentRoundEntry()
        {
            if (State.IsSeasonComplete) return null;
            return State.Calendar.FirstOrDefault(e => e.Round == State.CurrentRound);
        }

        /// <summary>Returns the next N calendar entries (including current).</summary>
        public List<CalendarEntry> UpcomingRounds(int count = 5)
        {
            return State.Calendar
                .Where(e => e.Round >= State.CurrentRound)
                .Take(count)
                .ToList();
        }

        // ── Result recording ──────────────────────────────────────────────────

        /// <summary>
        /// Records a completed race result, updates standings, runs EDUO checkpoint if applicable,
        /// and distributes prize money at season end.
        /// </summary>
        public void RecordRaceResult(RaceResultRecord result)
        {
            State.History.Add(result);
            ApplyPoints(result);
            RecordEDUOMechanicalChanges(result);

            // EDUO checkpoint: check PU usage after races 6, 12, 18
            if (_EDUOCheckpoints.Contains(State.CurrentRound))
                CheckEDUOPenalties(State.CurrentRound);

            State.CurrentRound++;

            // Season complete: distribute prize money
            if (State.IsSeasonComplete)
                DistributePrizeMoney();
        }

        /// <summary>
        /// Builds a RaceResultRecord from a completed RaceState and optional qualifying result.
        /// </summary>
        public static RaceResultRecord BuildRecord(
            int              round,
            int              circuitID,
            bool             isSprint,
            RaceState        raceState,
            QualifyingResult qualifyingResult = null)
        {
            var record = new RaceResultRecord
            {
                Round     = round,
                CircuitID = circuitID,
                IsSprint  = isSprint,
            };

            var sorted = raceState.SortedCars();
            foreach (var car in sorted)
                record.FinishingOrder.Add(car.DriverID);

            foreach (var car in raceState.Cars.Where(c => c.DNF))
            {
                record.DNFs.Add(car.DriverID);
                if (car.DNFReason != null && car.DNFReason.Contains("mechanical"))
                    record.MechanicalDNFs.Add(car.DriverID);
            }

            if (raceState.FastestLapDriverID >= 0)
                record.FastestLaps[raceState.FastestLapDriverID] = raceState.FastestLapTimeS;

            if (qualifyingResult != null)
                record.PoleSitterID = qualifyingResult.PoleSitterID;

            return record;
        }

        // ── Points application ────────────────────────────────────────────────

        private void ApplyPoints(RaceResultRecord result)
        {
            GameDataFactory.Initialize();

            // Track fastest lap holder for statistics only (no bonus point — removed 2025)
            int flDriverID = result.FastestLaps.Count > 0 ? result.FastestLaps.Keys.First() : -1;

            for (int i = 0; i < result.FinishingOrder.Count; i++)
            {
                int driverID = result.FinishingOrder[i];
                bool dnf     = result.DNFs.Contains(driverID);
                int  pos     = i + 1;
                int  pts     = dnf ? 0 : PointsTables.ForPosition(pos, result.IsSprint);

                var dstanding = State.DriverStandings.FirstOrDefault(d => d.DriverID == driverID);
                if (dstanding == null) continue;

                dstanding.Points += pts;
                dstanding.Races  += 1;
                if (!dnf && pos == 1) dstanding.Wins++;
                if (!dnf && pos <= 3) dstanding.Podiums++;
                if (driverID == flDriverID) dstanding.FastestLaps++; // stat only
                if (!result.IsSprint && driverID == result.PoleSitterID) dstanding.Poles++;

                var driver    = GameDataFactory.GetDriver(driverID);
                if (driver == null) continue;
                var tstanding = State.ConstructorStandings.FirstOrDefault(t => t.TeamID == driver.teamID);
                if (tstanding == null) continue;

                tstanding.Points += pts;
                tstanding.Races  += 1;
                if (!dnf && pos == 1) tstanding.Wins++;
                if (!dnf && pos <= 3) tstanding.Podiums++;
            }
        }

        // ── EDUO — Power unit tracking ────────────────────────────────────────

        /// <summary>
        /// Records that a team has fitted a new power unit specification (e.g. after a PU upgrade).
        /// Increments ICE and, if includeERS, also MGU-K and MGU-H usage counters.
        /// Call this when CarDevelopmentSystem completes a PU-area upgrade for a team.
        /// </summary>
        public void RecordPUUpgrade(int teamID, bool includeERS = false)
        {
            GameDataFactory.Initialize();
            var drivers = GameDataFactory.GetTeamDrivers(teamID);
            foreach (var driver in drivers)
            {
                if (!State.PUUsage.TryGetValue(driver.id, out var usage)) continue;
                usage.ICEUsed++;
                if (includeERS)
                {
                    usage.MGUKUsed++;
                    usage.MGUHUsed++;
                }
            }
        }

        /// <summary>
        /// Records an emergency component change for a single driver after a mechanical DNF.
        /// Call this when a driver retires with a mechanical failure.
        /// </summary>
        public void RecordEmergencyPUChange(int driverID)
        {
            if (State.PUUsage.TryGetValue(driverID, out var usage))
                usage.ICEUsed++;
        }

        /// <summary>
        /// Evaluates EDUO allocation at a checkpoint round.
        /// Drivers over their allocation receive grid penalties for the following race.
        /// Penalty resets after being applied (one-time per excess component batch).
        /// </summary>
        public List<EDUOPenalty> CheckEDUOPenalties(int currentRound)
        {
            var penalties = new List<EDUOPenalty>();
            foreach (var kvp in State.PUUsage)
            {
                int places = kvp.Value.TotalPenaltyPlaces();
                if (places <= 0) continue;

                var penalty = new EDUOPenalty
                {
                    DriverID          = kvp.Key,
                    GridPenaltyPlaces = places,
                    ApplicableRound   = currentRound + 1,
                    Reason = $"PU allocation exceeded at checkpoint R{currentRound} "
                           + $"(ICE: {kvp.Value.ICEUsed}/{DriverPUUsage.MaxICE})",
                };
                penalties.Add(penalty);
                State.PendingGridPenalties.Add(penalty);
            }
            return penalties;
        }

        /// <summary>
        /// Returns total grid penalty places for a driver at a specific round.
        /// Use this to offset their qualifying grid position before race start.
        /// </summary>
        public int GetGridPenaltyForDriver(int driverID, int round)
        {
            return State.PendingGridPenalties
                .Where(p => p.DriverID == driverID && p.ApplicableRound == round)
                .Sum(p => p.GridPenaltyPlaces);
        }

        /// <summary>
        /// Consumes (removes) applied penalties for a round so they don't carry over.
        /// Call after the grid has been set for the race.
        /// </summary>
        public void ConsumeGridPenalties(int round)
        {
            State.PendingGridPenalties.RemoveAll(p => p.ApplicableRound == round);
        }

        // Records emergency PU changes for all mechanical DNFs in a race result
        private void RecordEDUOMechanicalChanges(RaceResultRecord result)
        {
            foreach (int driverID in result.MechanicalDNFs)
                RecordEmergencyPUChange(driverID);
        }

        // ── Prize money ───────────────────────────────────────────────────────

        /// <summary>
        /// Distributes end-of-season prize money based on final constructor standings.
        /// Total pool ≈ $450M. Called automatically when season completes.
        /// </summary>
        public void DistributePrizeMoney()
        {
            var sorted = SortedConstructorStandings();
            for (int i = 0; i < sorted.Count && i < PointsTables.PrizeMoney.Length; i++)
                sorted[i].PrizeMoney = PointsTables.PrizeMoney[i];
        }

        // ── Sorted standings ──────────────────────────────────────────────────

        /// <summary>Driver standings sorted by points desc, then wins desc.</summary>
        public List<DriverStanding> SortedDriverStandings()
        {
            return State.DriverStandings
                .OrderByDescending(d => d.Points)
                .ThenByDescending(d => d.Wins)
                .ThenByDescending(d => d.Podiums)
                .ToList();
        }

        /// <summary>Constructor standings sorted by points desc, then wins desc.</summary>
        public List<ConstructorStanding> SortedConstructorStandings()
        {
            return State.ConstructorStandings
                .OrderByDescending(c => c.Points)
                .ThenByDescending(c => c.Wins)
                .ToList();
        }

        // ── Championship gap helper ───────────────────────────────────────────

        /// <summary>Returns maximum points still available in remaining races.</summary>
        public int MaxPointsRemaining(bool isSprint = false)
        {
            int remaining = State.Calendar.Count - State.CurrentRound + 1;
            if (remaining <= 0) return 0;
            int perRace = isSprint ? PointsTables.Sprint[0] : PointsTables.Race[0];
            return remaining * perRace;
        }

        /// <summary>Gap in points between the championship leader and the given driver.</summary>
        public int ChampionshipGap(int driverID)
        {
            var sorted = SortedDriverStandings();
            var leader = sorted.FirstOrDefault();
            var driver = sorted.FirstOrDefault(d => d.DriverID == driverID);
            if (leader == null || driver == null) return 0;
            return leader.Points - driver.Points;
        }

        // ── Season summary ────────────────────────────────────────────────────

        /// <summary>Returns a short display string for the season status.</summary>
        public string StatusLine()
        {
            if (State.IsSeasonComplete)
                return $"Season {State.Season} — FINAL";
            var entry = CurrentRoundEntry();
            return $"Season {State.Season} — Round {State.CurrentRound}/24"
                + (entry?.IsSprint == true ? " [SPRINT]" : "");
        }
    }
}
