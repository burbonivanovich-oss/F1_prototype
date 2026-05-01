using System.Collections.Generic;
using System.Linq;

namespace F1Manager
{
    /// <summary>
    /// Runtime data access layer. Call GameDataFactory.Initialize() once at
    /// startup (e.g. from a GameManager Awake). All simulation code reads data
    /// through this static class rather than through Unity ScriptableObject assets,
    /// so the game works identically in the Editor and in standalone builds.
    /// </summary>
    public static class GameDataFactory
    {
        // ─────────────────────────────────────────────────────────────────────
        // Public data arrays — populated by Initialize()
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>All 24 circuits in calendar order.</summary>
        public static CircuitInfo[] Circuits { get; private set; }

        /// <summary>All 10 constructor teams.</summary>
        public static TeamInfo[] Teams { get; private set; }

        /// <summary>
        /// The 20 race-active drivers (isReserve == false).
        /// Reserve drivers are excluded; use GetDriver(id) to reach them by id.
        /// </summary>
        public static DriverInfo[] Drivers { get; private set; }

        // Internal full driver roster (active + reserves) for id look-up.
        private static DriverInfo[] _allDrivers;

        private static bool _initialised;

        // ─────────────────────────────────────────────────────────────────────
        // Initialisation
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Loads all static game data. Safe to call multiple times — subsequent
        /// calls are no-ops.
        /// </summary>
        public static void Initialize()
        {
            if (_initialised) return;

            GameData data = StaticGameData.CreateData();

            Circuits    = data.circuits;
            Teams       = data.teams;
            _allDrivers = data.drivers;
            Drivers     = data.drivers.Where(d => !d.isReserve).ToArray();

            _initialised = true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Look-up helpers
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the CircuitInfo with the given id, or null if not found.
        /// </summary>
        public static CircuitInfo GetCircuit(int id)
        {
            EnsureInitialised();
            return Circuits.FirstOrDefault(c => c.id == id);
        }

        /// <summary>
        /// Returns the TeamInfo with the given id, or null if not found.
        /// </summary>
        public static TeamInfo GetTeam(int id)
        {
            EnsureInitialised();
            return Teams.FirstOrDefault(t => t.id == id);
        }

        /// <summary>
        /// Returns the DriverInfo with the given id (active or reserve), or
        /// null if not found.
        /// </summary>
        public static DriverInfo GetDriver(int id)
        {
            EnsureInitialised();
            return _allDrivers.FirstOrDefault(d => d.id == id);
        }

        /// <summary>
        /// Returns all drivers (active and reserve) assigned to the given team id.
        /// </summary>
        public static List<DriverInfo> GetTeamDrivers(int teamId)
        {
            EnsureInitialised();
            return _allDrivers.Where(d => d.teamID == teamId).ToList();
        }

        /// <summary>Returns all drivers including reserves.</summary>
        public static DriverInfo[] GetAllDrivers()
        {
            EnsureInitialised();
            return _allDrivers;
        }

        /// <summary>Returns non-reserve drivers as id→DriverInfo dictionary.</summary>
        public static Dictionary<int, DriverInfo> GetAllDriversDict()
        {
            EnsureInitialised();
            var dict = new Dictionary<int, DriverInfo>();
            foreach (var d in _allDrivers) dict[d.id] = d;
            return dict;
        }

        /// <summary>Returns teams as id→TeamInfo dictionary.</summary>
        public static Dictionary<int, TeamInfo> GetTeamsDict()
        {
            EnsureInitialised();
            var dict = new Dictionary<int, TeamInfo>();
            foreach (var t in Teams) dict[t.id] = t;
            return dict;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Internal helpers
        // ─────────────────────────────────────────────────────────────────────

        private static void EnsureInitialised()
        {
            if (!_initialised)
                Initialize();
        }
    }
}
