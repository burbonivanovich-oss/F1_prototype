// SeasonEngine.cs — Championship season management: points, standings, 24-race schedule, save state.
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
        public int Podiums;    // P1+P2+P3
        public int Poles;
        public int FastestLaps;
        public int Races;      // Races started

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

        /// <summary>driverID → fastest lap time this race (for fastest lap bonus).</summary>
        public Dictionary<int, float> FastestLaps = new Dictionary<int, float>();

        /// <summary>driverID → pole lap time (qualifying). -1 if sprint weekend.</summary>
        public int PoleSitterID = -1;

        /// <summary>driverIDs that DNF'd.</summary>
        public HashSet<int> DNFs = new HashSet<int>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Season state (serialisable between sessions)
    // ─────────────────────────────────────────────────────────────────────────

    public class SeasonState
    {
        public int  Season = 2027;
        public int  CurrentRound = 1;   // 1-indexed, next race to run
        public bool IsSeasonComplete => CurrentRound > Calendar.Count;

        public List<CalendarEntry>       Calendar            = new List<CalendarEntry>();
        public List<DriverStanding>      DriverStandings     = new List<DriverStanding>();
        public List<ConstructorStanding> ConstructorStandings = new List<ConstructorStanding>();
        public List<RaceResultRecord>    History             = new List<RaceResultRecord>();

        // Player's team ID (set at season start)
        public int PlayerTeamID;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Points tables
    // ─────────────────────────────────────────────────────────────────────────

    public static class PointsTables
    {
        /// <summary>Standard race points: P1=25 … P10=1.</summary>
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
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SeasonEngine
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Manages the full 24-round championship: calendar, points tally, standings,
    /// and result persistence between race weekends.
    /// </summary>
    public class SeasonEngine
    {
        public SeasonState State { get; private set; }

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
            // (round, circuitID, isSprint, dateLabel)
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
                State.DriverStandings.Add(new DriverStanding(driver.id));

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
        /// Records a completed race result and updates all standings.
        /// Call once per race weekend after both main race (and sprint if applicable) complete.
        /// </summary>
        public void RecordRaceResult(RaceResultRecord result)
        {
            State.History.Add(result);
            ApplyPoints(result);
            State.CurrentRound++;
        }

        /// <summary>
        /// Builds a RaceResultRecord from a completed RaceState and optional qualifying result.
        /// </summary>
        public static RaceResultRecord BuildRecord(
            int             round,
            int             circuitID,
            bool            isSprint,
            RaceState       raceState,
            QualifyingResult qualifyingResult = null)
        {
            var record = new RaceResultRecord
            {
                Round     = round,
                CircuitID = circuitID,
                IsSprint  = isSprint,
            };

            // Finishing order (driverIDs in finishing position order)
            var sorted = raceState.SortedCars();
            foreach (var car in sorted)
                record.FinishingOrder.Add(car.DriverID);

            // DNFs
            foreach (var car in raceState.Cars.Where(c => c.DNF))
                record.DNFs.Add(car.DriverID);

            // Fastest lap
            if (raceState.FastestLapDriverID >= 0)
                record.FastestLaps[raceState.FastestLapDriverID] = raceState.FastestLapTimeS;

            // Pole sitter
            if (qualifyingResult != null)
                record.PoleSitterID = qualifyingResult.PoleSitterID;

            return record;
        }

        // ── Points application ────────────────────────────────────────────────

        private void ApplyPoints(RaceResultRecord result)
        {
            GameDataFactory.Initialize();

            // Determine fastest lap driver (must finish P1-P10)
            int flDriverID = result.FastestLaps.Keys.FirstOrDefault(-1);
            bool flInPoints = flDriverID >= 0
                && result.FinishingOrder.Take(10).Contains(flDriverID)
                && !result.DNFs.Contains(flDriverID);

            for (int i = 0; i < result.FinishingOrder.Count; i++)
            {
                int driverID = result.FinishingOrder[i];
                bool dnf     = result.DNFs.Contains(driverID);
                int  pos     = i + 1; // 1-based
                int  pts     = dnf ? 0 : PointsTables.ForPosition(pos, result.IsSprint);

                // Fastest lap bonus: +1 for non-sprint races if in points (P1-P10)
                if (!result.IsSprint && driverID == flDriverID && flInPoints)
                    pts += 1;

                // Update driver standing
                var dstanding = State.DriverStandings.FirstOrDefault(d => d.DriverID == driverID);
                if (dstanding == null) continue;
                dstanding.Points += pts;
                dstanding.Races  += 1;
                if (!dnf && pos == 1) dstanding.Wins++;
                if (!dnf && pos <= 3) dstanding.Podiums++;
                if (!result.IsSprint && driverID == flDriverID && flInPoints)
                    dstanding.FastestLaps++;
                if (!result.IsSprint && driverID == result.PoleSitterID)
                    dstanding.Poles++;

                // Update constructor standing
                var driver   = GameDataFactory.GetDriver(driverID);
                if (driver == null) continue;
                var tstanding = State.ConstructorStandings.FirstOrDefault(t => t.TeamID == driver.teamID);
                if (tstanding == null) continue;
                tstanding.Points += pts;
                tstanding.Races  += 1;
                if (!dnf && pos == 1) tstanding.Wins++;
                if (!dnf && pos <= 3) tstanding.Podiums++;
            }
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

        /// <summary>Returns maximum points still available in remaining races (including FL bonus).</summary>
        public int MaxPointsRemaining(bool isSprint = false)
        {
            int remaining = State.Calendar.Count - State.CurrentRound + 1;
            if (remaining <= 0) return 0;
            int perRace = isSprint ? PointsTables.Sprint[0] : PointsTables.Race[0] + 1; // +1 for FL
            return remaining * perRace;
        }

        /// <summary>
        /// Gap in points between the leader and the given driver.
        /// Negative means the driver is ahead (shouldn't happen for non-leaders).
        /// </summary>
        public int ChampionshipGap(int driverID)
        {
            var sorted  = SortedDriverStandings();
            var leader  = sorted.FirstOrDefault();
            var driver  = sorted.FirstOrDefault(d => d.DriverID == driverID);
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
