using System.Collections.Generic;

namespace F1Manager
{
    // Plain C# data classes — no ScriptableObject, safe to instantiate at runtime
    // without the Unity Editor asset pipeline.

    public class CircuitInfo
    {
        public int id;
        public string circuitName;
        public string country;
        public string city;
        public int totalLaps;
        public float baseLapTimeS;
        public float circuitLengthKm;
        public float tireDegMultiplier;
        public float overtakeDifficulty;
        public int drsZones;
        public float fuelConsumptionKg;
        public float rainProbability;
        public float trackTempRangeMin;
        public float trackTempRangeMax;
        public float safetyCarlProbability;
        public float[] sectorSplits;   // always length 3: [s1, s2, s3]
        public float powerSensitivity;
    }

    public class TeamInfo
    {
        public int id;
        public string teamName;
        public string shortName;
        public string colorHex;
        public int carPerformance;
        public int pitCrewSkill;
        public int reliability;
        public int powerUnit;
        public int chassis;
    }

    public class DriverInfo
    {
        public int id;
        public string driverName;
        public string shortName;
        public int carNumber;
        public int teamID;
        public int pace;
        public int racecraft;
        public int defending;
        public int tireManagement;
        public int wetSkill;
        public int experience;
        public int morale;
        public bool isReserve;
    }

    public struct GameData
    {
        public CircuitInfo[] circuits;
        public TeamInfo[] teams;
        public DriverInfo[] drivers;
    }

    public static class StaticGameData
    {
        public static GameData CreateData()
        {
            return new GameData
            {
                circuits = CreateCircuits(),
                teams    = CreateTeams(),
                drivers  = CreateDrivers()
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // CIRCUITS — 24 rounds, F1 2024/2025 authentic values
        // trackTempRangeMin/Max are reasonable defaults per venue
        // ─────────────────────────────────────────────────────────────────────
        private static CircuitInfo[] CreateCircuits()
        {
            return new CircuitInfo[]
            {
                new CircuitInfo
                {
                    id = 0, circuitName = "Bahrain GP",
                    country = "Bahrain", city = "Sakhir",
                    totalLaps = 57, baseLapTimeS = 91.5f,
                    circuitLengthKm = 5.412f, tireDegMultiplier = 1.05f,
                    overtakeDifficulty = 0.35f, drsZones = 3,
                    fuelConsumptionKg = 1.95f, rainProbability = 0.05f,
                    trackTempRangeMin = 28f, trackTempRangeMax = 45f,
                    safetyCarlProbability = 0.12f,
                    sectorSplits = new float[] { 0.28f, 0.44f, 0.28f },
                    powerSensitivity = 0.45f
                },
                new CircuitInfo
                {
                    id = 1, circuitName = "Saudi Arabian GP",
                    country = "Saudi Arabia", city = "Jeddah",
                    totalLaps = 50, baseLapTimeS = 88.2f,
                    circuitLengthKm = 6.174f, tireDegMultiplier = 0.90f,
                    overtakeDifficulty = 0.25f, drsZones = 3,
                    fuelConsumptionKg = 2.10f, rainProbability = 0.02f,
                    trackTempRangeMin = 30f, trackTempRangeMax = 50f,
                    safetyCarlProbability = 0.20f,
                    sectorSplits = new float[] { 0.32f, 0.38f, 0.30f },
                    powerSensitivity = 0.75f
                },
                new CircuitInfo
                {
                    id = 2, circuitName = "Australian GP",
                    country = "Australia", city = "Melbourne",
                    totalLaps = 58, baseLapTimeS = 80.2f,
                    circuitLengthKm = 5.278f, tireDegMultiplier = 1.00f,
                    overtakeDifficulty = 0.50f, drsZones = 3,
                    fuelConsumptionKg = 1.85f, rainProbability = 0.15f,
                    trackTempRangeMin = 18f, trackTempRangeMax = 38f,
                    safetyCarlProbability = 0.18f,
                    sectorSplits = new float[] { 0.35f, 0.32f, 0.33f },
                    powerSensitivity = 0.50f
                },
                new CircuitInfo
                {
                    id = 3, circuitName = "Japanese GP",
                    country = "Japan", city = "Suzuka",
                    totalLaps = 53, baseLapTimeS = 91.3f,
                    circuitLengthKm = 5.807f, tireDegMultiplier = 1.15f,
                    overtakeDifficulty = 0.60f, drsZones = 2,
                    fuelConsumptionKg = 2.00f, rainProbability = 0.20f,
                    trackTempRangeMin = 16f, trackTempRangeMax = 34f,
                    safetyCarlProbability = 0.12f,
                    sectorSplits = new float[] { 0.33f, 0.38f, 0.29f },
                    powerSensitivity = 0.40f
                },
                new CircuitInfo
                {
                    id = 4, circuitName = "Chinese GP",
                    country = "China", city = "Shanghai",
                    totalLaps = 56, baseLapTimeS = 93.1f,
                    circuitLengthKm = 5.451f, tireDegMultiplier = 1.10f,
                    overtakeDifficulty = 0.45f, drsZones = 2,
                    fuelConsumptionKg = 1.95f, rainProbability = 0.18f,
                    trackTempRangeMin = 14f, trackTempRangeMax = 32f,
                    safetyCarlProbability = 0.15f,
                    sectorSplits = new float[] { 0.36f, 0.34f, 0.30f },
                    powerSensitivity = 0.50f
                },
                new CircuitInfo
                {
                    id = 5, circuitName = "Miami GP",
                    country = "USA", city = "Miami",
                    totalLaps = 57, baseLapTimeS = 89.8f,
                    circuitLengthKm = 5.412f, tireDegMultiplier = 1.10f,
                    overtakeDifficulty = 0.40f, drsZones = 3,
                    fuelConsumptionKg = 1.95f, rainProbability = 0.12f,
                    trackTempRangeMin = 26f, trackTempRangeMax = 46f,
                    safetyCarlProbability = 0.15f,
                    sectorSplits = new float[] { 0.32f, 0.36f, 0.32f },
                    powerSensitivity = 0.60f
                },
                new CircuitInfo
                {
                    id = 6, circuitName = "Emilia Romagna GP",
                    country = "Italy", city = "Imola",
                    totalLaps = 63, baseLapTimeS = 75.5f,
                    circuitLengthKm = 4.909f, tireDegMultiplier = 1.05f,
                    overtakeDifficulty = 0.55f, drsZones = 2,
                    fuelConsumptionKg = 1.75f, rainProbability = 0.20f,
                    trackTempRangeMin = 18f, trackTempRangeMax = 36f,
                    safetyCarlProbability = 0.18f,
                    sectorSplits = new float[] { 0.33f, 0.35f, 0.32f },
                    powerSensitivity = 0.45f
                },
                new CircuitInfo
                {
                    id = 7, circuitName = "Monaco GP",
                    country = "Monaco", city = "Monte Carlo",
                    totalLaps = 78, baseLapTimeS = 71.5f,
                    circuitLengthKm = 3.337f, tireDegMultiplier = 1.40f,
                    overtakeDifficulty = 0.95f, drsZones = 1,
                    fuelConsumptionKg = 1.30f, rainProbability = 0.25f,
                    trackTempRangeMin = 20f, trackTempRangeMax = 38f,
                    safetyCarlProbability = 0.30f,
                    sectorSplits = new float[] { 0.34f, 0.40f, 0.26f },
                    powerSensitivity = 0.10f
                },
                new CircuitInfo
                {
                    id = 8, circuitName = "Canadian GP",
                    country = "Canada", city = "Montreal",
                    totalLaps = 70, baseLapTimeS = 73.7f,
                    circuitLengthKm = 4.361f, tireDegMultiplier = 1.20f,
                    overtakeDifficulty = 0.45f, drsZones = 2,
                    fuelConsumptionKg = 1.55f, rainProbability = 0.25f,
                    trackTempRangeMin = 16f, trackTempRangeMax = 34f,
                    safetyCarlProbability = 0.22f,
                    sectorSplits = new float[] { 0.35f, 0.35f, 0.30f },
                    powerSensitivity = 0.55f
                },
                new CircuitInfo
                {
                    id = 9, circuitName = "Spanish GP",
                    country = "Spain", city = "Barcelona",
                    totalLaps = 66, baseLapTimeS = 75.9f,
                    circuitLengthKm = 4.657f, tireDegMultiplier = 1.10f,
                    overtakeDifficulty = 0.50f, drsZones = 2,
                    fuelConsumptionKg = 1.65f, rainProbability = 0.10f,
                    trackTempRangeMin = 22f, trackTempRangeMax = 42f,
                    safetyCarlProbability = 0.12f,
                    sectorSplits = new float[] { 0.30f, 0.42f, 0.28f },
                    powerSensitivity = 0.45f
                },
                new CircuitInfo
                {
                    id = 10, circuitName = "Austrian GP",
                    country = "Austria", city = "Spielberg",
                    totalLaps = 71, baseLapTimeS = 63.5f,
                    circuitLengthKm = 4.318f, tireDegMultiplier = 1.15f,
                    overtakeDifficulty = 0.45f, drsZones = 3,
                    fuelConsumptionKg = 1.55f, rainProbability = 0.22f,
                    trackTempRangeMin = 16f, trackTempRangeMax = 34f,
                    safetyCarlProbability = 0.15f,
                    sectorSplits = new float[] { 0.32f, 0.40f, 0.28f },
                    powerSensitivity = 0.50f
                },
                new CircuitInfo
                {
                    id = 11, circuitName = "British GP",
                    country = "United Kingdom", city = "Silverstone",
                    totalLaps = 52, baseLapTimeS = 88.5f,
                    circuitLengthKm = 5.891f, tireDegMultiplier = 1.00f,
                    overtakeDifficulty = 0.45f, drsZones = 2,
                    fuelConsumptionKg = 2.10f, rainProbability = 0.30f,
                    trackTempRangeMin = 14f, trackTempRangeMax = 32f,
                    safetyCarlProbability = 0.15f,
                    sectorSplits = new float[] { 0.31f, 0.36f, 0.33f },
                    powerSensitivity = 0.50f
                },
                new CircuitInfo
                {
                    id = 12, circuitName = "Hungarian GP",
                    country = "Hungary", city = "Budapest",
                    totalLaps = 70, baseLapTimeS = 76.6f,
                    circuitLengthKm = 4.381f, tireDegMultiplier = 1.25f,
                    overtakeDifficulty = 0.75f, drsZones = 2,
                    fuelConsumptionKg = 1.55f, rainProbability = 0.20f,
                    trackTempRangeMin = 24f, trackTempRangeMax = 44f,
                    safetyCarlProbability = 0.12f,
                    sectorSplits = new float[] { 0.28f, 0.44f, 0.28f },
                    powerSensitivity = 0.20f
                },
                new CircuitInfo
                {
                    id = 13, circuitName = "Belgian GP",
                    country = "Belgium", city = "Spa-Francorchamps",
                    totalLaps = 44, baseLapTimeS = 101.8f,
                    circuitLengthKm = 7.004f, tireDegMultiplier = 0.95f,
                    overtakeDifficulty = 0.40f, drsZones = 2,
                    fuelConsumptionKg = 2.45f, rainProbability = 0.30f,
                    trackTempRangeMin = 12f, trackTempRangeMax = 30f,
                    safetyCarlProbability = 0.18f,
                    sectorSplits = new float[] { 0.36f, 0.34f, 0.30f },
                    powerSensitivity = 0.65f
                },
                new CircuitInfo
                {
                    id = 14, circuitName = "Dutch GP",
                    country = "Netherlands", city = "Zandvoort",
                    totalLaps = 72, baseLapTimeS = 72.9f,
                    circuitLengthKm = 4.259f, tireDegMultiplier = 1.15f,
                    overtakeDifficulty = 0.65f, drsZones = 2,
                    fuelConsumptionKg = 1.55f, rainProbability = 0.20f,
                    trackTempRangeMin = 14f, trackTempRangeMax = 30f,
                    safetyCarlProbability = 0.15f,
                    sectorSplits = new float[] { 0.32f, 0.38f, 0.30f },
                    powerSensitivity = 0.35f
                },
                new CircuitInfo
                {
                    id = 15, circuitName = "Italian GP",
                    country = "Italy", city = "Monza",
                    totalLaps = 53, baseLapTimeS = 81.6f,
                    circuitLengthKm = 5.793f, tireDegMultiplier = 0.60f,
                    overtakeDifficulty = 0.15f, drsZones = 2,
                    fuelConsumptionKg = 2.10f, rainProbability = 0.12f,
                    trackTempRangeMin = 20f, trackTempRangeMax = 38f,
                    safetyCarlProbability = 0.10f,
                    sectorSplits = new float[] { 0.30f, 0.34f, 0.36f },
                    powerSensitivity = 0.92f
                },
                new CircuitInfo
                {
                    id = 16, circuitName = "Azerbaijan GP",
                    country = "Azerbaijan", city = "Baku",
                    totalLaps = 51, baseLapTimeS = 103.0f,
                    circuitLengthKm = 6.003f, tireDegMultiplier = 0.85f,
                    overtakeDifficulty = 0.30f, drsZones = 2,
                    fuelConsumptionKg = 2.15f, rainProbability = 0.05f,
                    trackTempRangeMin = 22f, trackTempRangeMax = 42f,
                    safetyCarlProbability = 0.28f,
                    sectorSplits = new float[] { 0.35f, 0.30f, 0.35f },
                    powerSensitivity = 0.80f
                },
                new CircuitInfo
                {
                    id = 17, circuitName = "Singapore GP",
                    country = "Singapore", city = "Singapore",
                    totalLaps = 62, baseLapTimeS = 99.5f,
                    circuitLengthKm = 4.940f, tireDegMultiplier = 1.30f,
                    overtakeDifficulty = 0.90f, drsZones = 3,
                    fuelConsumptionKg = 1.80f, rainProbability = 0.30f,
                    trackTempRangeMin = 28f, trackTempRangeMax = 40f,
                    safetyCarlProbability = 0.25f,
                    sectorSplits = new float[] { 0.33f, 0.38f, 0.29f },
                    powerSensitivity = 0.15f
                },
                new CircuitInfo
                {
                    id = 18, circuitName = "US GP (Austin)",
                    country = "USA", city = "Austin",
                    totalLaps = 56, baseLapTimeS = 95.8f,
                    circuitLengthKm = 5.513f, tireDegMultiplier = 1.10f,
                    overtakeDifficulty = 0.50f, drsZones = 2,
                    fuelConsumptionKg = 1.95f, rainProbability = 0.15f,
                    trackTempRangeMin = 22f, trackTempRangeMax = 42f,
                    safetyCarlProbability = 0.15f,
                    sectorSplits = new float[] { 0.33f, 0.35f, 0.32f },
                    powerSensitivity = 0.55f
                },
                new CircuitInfo
                {
                    id = 19, circuitName = "Mexico City GP",
                    country = "Mexico", city = "Mexico City",
                    totalLaps = 71, baseLapTimeS = 79.1f,
                    circuitLengthKm = 4.304f, tireDegMultiplier = 1.05f,
                    overtakeDifficulty = 0.45f, drsZones = 3,
                    fuelConsumptionKg = 1.55f, rainProbability = 0.08f,
                    trackTempRangeMin = 18f, trackTempRangeMax = 36f,
                    safetyCarlProbability = 0.12f,
                    sectorSplits = new float[] { 0.30f, 0.40f, 0.30f },
                    powerSensitivity = 0.50f
                },
                new CircuitInfo
                {
                    id = 20, circuitName = "São Paulo GP",
                    country = "Brazil", city = "São Paulo",
                    totalLaps = 71, baseLapTimeS = 71.5f,
                    circuitLengthKm = 4.309f, tireDegMultiplier = 1.15f,
                    overtakeDifficulty = 0.55f, drsZones = 2,
                    fuelConsumptionKg = 1.55f, rainProbability = 0.35f,
                    trackTempRangeMin = 20f, trackTempRangeMax = 40f,
                    safetyCarlProbability = 0.20f,
                    sectorSplits = new float[] { 0.32f, 0.36f, 0.32f },
                    powerSensitivity = 0.55f
                },
                new CircuitInfo
                {
                    id = 21, circuitName = "Las Vegas GP",
                    country = "USA", city = "Las Vegas",
                    totalLaps = 50, baseLapTimeS = 93.5f,
                    circuitLengthKm = 6.120f, tireDegMultiplier = 0.85f,
                    overtakeDifficulty = 0.30f, drsZones = 3,
                    fuelConsumptionKg = 2.20f, rainProbability = 0.02f,
                    trackTempRangeMin = 10f, trackTempRangeMax = 22f,
                    safetyCarlProbability = 0.18f,
                    sectorSplits = new float[] { 0.35f, 0.32f, 0.33f },
                    powerSensitivity = 0.80f
                },
                new CircuitInfo
                {
                    id = 22, circuitName = "Qatar GP",
                    country = "Qatar", city = "Lusail",
                    totalLaps = 57, baseLapTimeS = 83.3f,
                    circuitLengthKm = 5.380f, tireDegMultiplier = 1.45f,
                    overtakeDifficulty = 0.55f, drsZones = 2,
                    fuelConsumptionKg = 1.95f, rainProbability = 0.04f,
                    trackTempRangeMin = 30f, trackTempRangeMax = 48f,
                    safetyCarlProbability = 0.12f,
                    sectorSplits = new float[] { 0.32f, 0.38f, 0.30f },
                    powerSensitivity = 0.55f
                },
                new CircuitInfo
                {
                    id = 23, circuitName = "Abu Dhabi GP",
                    country = "UAE", city = "Abu Dhabi",
                    totalLaps = 58, baseLapTimeS = 85.0f,
                    circuitLengthKm = 5.281f, tireDegMultiplier = 0.95f,
                    overtakeDifficulty = 0.40f, drsZones = 3,
                    fuelConsumptionKg = 1.90f, rainProbability = 0.02f,
                    trackTempRangeMin = 28f, trackTempRangeMax = 44f,
                    safetyCarlProbability = 0.10f,
                    sectorSplits = new float[] { 0.32f, 0.36f, 0.32f },
                    powerSensitivity = 0.50f
                },
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // TEAMS — 10 constructors, 2027 grid
        // ─────────────────────────────────────────────────────────────────────
        private static TeamInfo[] CreateTeams()
        {
            return new TeamInfo[]
            {
                new TeamInfo { id = 0, teamName = "Red Bull Racing",     shortName = "RBR", colorHex = "#3B67A8", carPerformance = 97, pitCrewSkill = 94, reliability = 93, powerUnit = 89, chassis = 99 },
                new TeamInfo { id = 1, teamName = "Scuderia Ferrari",    shortName = "FER", colorHex = "#E8002D", carPerformance = 95, pitCrewSkill = 92, reliability = 91, powerUnit = 97, chassis = 93 },
                new TeamInfo { id = 2, teamName = "Mercedes-AMG",        shortName = "MER", colorHex = "#00D2BE", carPerformance = 93, pitCrewSkill = 93, reliability = 92, powerUnit = 96, chassis = 91 },
                new TeamInfo { id = 3, teamName = "McLaren F1 Team",     shortName = "MCL", colorHex = "#FF8000", carPerformance = 94, pitCrewSkill = 90, reliability = 90, powerUnit = 90, chassis = 95 },
                new TeamInfo { id = 4, teamName = "Aston Martin",        shortName = "AMR", colorHex = "#358C75", carPerformance = 88, pitCrewSkill = 87, reliability = 88, powerUnit = 90, chassis = 86 },
                new TeamInfo { id = 5, teamName = "Alpine F1 Team",      shortName = "ALP", colorHex = "#0090FF", carPerformance = 85, pitCrewSkill = 85, reliability = 86, powerUnit = 88, chassis = 83 },
                new TeamInfo { id = 6, teamName = "Williams Racing",     shortName = "WIL", colorHex = "#005AFF", carPerformance = 82, pitCrewSkill = 83, reliability = 84, powerUnit = 90, chassis = 79 },
                new TeamInfo { id = 7, teamName = "Haas F1 Team",        shortName = "HAS", colorHex = "#B6BABD", carPerformance = 81, pitCrewSkill = 82, reliability = 83, powerUnit = 97, chassis = 77 },
                new TeamInfo { id = 8, teamName = "Visa Cash App RB",    shortName = "VCR", colorHex = "#1434CB", carPerformance = 83, pitCrewSkill = 84, reliability = 85, powerUnit = 89, chassis = 81 },
                new TeamInfo { id = 9, teamName = "Audi F1 Team",        shortName = "AUD", colorHex = "#C6A84B", carPerformance = 76, pitCrewSkill = 78, reliability = 79, powerUnit = 84, chassis = 73 },
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // DRIVERS — 20 active + 2 reserves
        // ─────────────────────────────────────────────────────────────────────
        private static DriverInfo[] CreateDrivers()
        {
            return new DriverInfo[]
            {
                // ── Red Bull Racing (team 0) ──────────────────────────────────
                new DriverInfo { id = 0,  driverName = "Max Verstappen",          shortName = "VER", carNumber = 1,  teamID = 0, pace = 98, racecraft = 97, defending = 95, tireManagement = 90, wetSkill = 92, experience = 95, morale = 90, isReserve = false },
                new DriverInfo { id = 1,  driverName = "Liam Lawson",             shortName = "LAW", carNumber = 30, teamID = 0, pace = 85, racecraft = 84, defending = 81, tireManagement = 83, wetSkill = 80, experience = 72, morale = 82, isReserve = false },

                // ── Scuderia Ferrari (team 1) ─────────────────────────────────
                new DriverInfo { id = 2,  driverName = "Charles Leclerc",         shortName = "LEC", carNumber = 16, teamID = 1, pace = 96, racecraft = 95, defending = 88, tireManagement = 87, wetSkill = 88, experience = 88, morale = 87, isReserve = false },
                new DriverInfo { id = 3,  driverName = "Lewis Hamilton",          shortName = "HAM", carNumber = 44, teamID = 1, pace = 95, racecraft = 98, defending = 92, tireManagement = 94, wetSkill = 98, experience = 99, morale = 88, isReserve = false },

                // ── Mercedes-AMG (team 2) ─────────────────────────────────────
                new DriverInfo { id = 4,  driverName = "George Russell",          shortName = "RUS", carNumber = 63, teamID = 2, pace = 92, racecraft = 91, defending = 87, tireManagement = 88, wetSkill = 85, experience = 83, morale = 85, isReserve = false },
                new DriverInfo { id = 5,  driverName = "Andrea Kimi Antonelli",   shortName = "ANT", carNumber = 12, teamID = 2, pace = 88, racecraft = 84, defending = 78, tireManagement = 82, wetSkill = 81, experience = 62, morale = 84, isReserve = false },

                // ── McLaren F1 Team (team 3) ──────────────────────────────────
                new DriverInfo { id = 6,  driverName = "Lando Norris",            shortName = "NOR", carNumber = 4,  teamID = 3, pace = 94, racecraft = 93, defending = 86, tireManagement = 89, wetSkill = 87, experience = 86, morale = 88, isReserve = false },
                new DriverInfo { id = 7,  driverName = "Oscar Piastri",           shortName = "PIA", carNumber = 81, teamID = 3, pace = 92, racecraft = 90, defending = 84, tireManagement = 88, wetSkill = 83, experience = 80, morale = 86, isReserve = false },

                // ── Aston Martin (team 4) ─────────────────────────────────────
                new DriverInfo { id = 8,  driverName = "Fernando Alonso",         shortName = "ALO", carNumber = 14, teamID = 4, pace = 93, racecraft = 97, defending = 93, tireManagement = 96, wetSkill = 90, experience = 99, morale = 85, isReserve = false },
                new DriverInfo { id = 9,  driverName = "Lance Stroll",            shortName = "STR", carNumber = 18, teamID = 4, pace = 82, racecraft = 81, defending = 77, tireManagement = 79, wetSkill = 71, experience = 80, morale = 77, isReserve = false },

                // ── Alpine F1 Team (team 5) ───────────────────────────────────
                new DriverInfo { id = 10, driverName = "Pierre Gasly",            shortName = "GAS", carNumber = 10, teamID = 5, pace = 87, racecraft = 88, defending = 82, tireManagement = 84, wetSkill = 83, experience = 86, morale = 82, isReserve = false },
                new DriverInfo { id = 11, driverName = "Jack Doohan",             shortName = "DOO", carNumber = 7,  teamID = 5, pace = 82, racecraft = 81, defending = 77, tireManagement = 80, wetSkill = 78, experience = 65, morale = 78, isReserve = false },

                // ── Williams Racing (team 6) ──────────────────────────────────
                new DriverInfo { id = 12, driverName = "Alexander Albon",         shortName = "ALB", carNumber = 23, teamID = 6, pace = 88, racecraft = 87, defending = 83, tireManagement = 86, wetSkill = 84, experience = 82, morale = 83, isReserve = false },
                new DriverInfo { id = 13, driverName = "Carlos Sainz",            shortName = "SAI", carNumber = 55, teamID = 6, pace = 90, racecraft = 91, defending = 87, tireManagement = 90, wetSkill = 87, experience = 90, morale = 84, isReserve = false },

                // ── Haas F1 Team (team 7) ─────────────────────────────────────
                new DriverInfo { id = 14, driverName = "Esteban Ocon",            shortName = "OCO", carNumber = 31, teamID = 7, pace = 84, racecraft = 83, defending = 79, tireManagement = 82, wetSkill = 80, experience = 83, morale = 79, isReserve = false },
                new DriverInfo { id = 15, driverName = "Oliver Bearman",          shortName = "BEA", carNumber = 87, teamID = 7, pace = 83, racecraft = 80, defending = 75, tireManagement = 79, wetSkill = 76, experience = 63, morale = 80, isReserve = false },

                // ── Visa Cash App RB (team 8) ─────────────────────────────────
                new DriverInfo { id = 16, driverName = "Yuki Tsunoda",            shortName = "TSU", carNumber = 22, teamID = 8, pace = 86, racecraft = 87, defending = 80, tireManagement = 83, wetSkill = 82, experience = 81, morale = 81, isReserve = false },
                new DriverInfo { id = 17, driverName = "Isack Hadjar",            shortName = "HAD", carNumber = 6,  teamID = 8, pace = 84, racecraft = 82, defending = 77, tireManagement = 80, wetSkill = 78, experience = 64, morale = 79, isReserve = false },

                // ── Audi F1 Team (team 9) ─────────────────────────────────────
                new DriverInfo { id = 18, driverName = "Nico Hulkenberg",         shortName = "HUL", carNumber = 27, teamID = 9, pace = 85, racecraft = 86, defending = 82, tireManagement = 84, wetSkill = 83, experience = 90, morale = 80, isReserve = false },
                new DriverInfo { id = 19, driverName = "Gabriel Bortoleto",       shortName = "BOR", carNumber = 5,  teamID = 9, pace = 83, racecraft = 81, defending = 76, tireManagement = 80, wetSkill = 77, experience = 62, morale = 79, isReserve = false },

                // ── Reserve drivers ───────────────────────────────────────────
                new DriverInfo { id = 20, driverName = "Nyck de Vries",           shortName = "DEV", carNumber = 0,  teamID = 0, pace = 80, racecraft = 79, defending = 74, tireManagement = 78, wetSkill = 76, experience = 75, morale = 74, isReserve = true  },
                new DriverInfo { id = 21, driverName = "Mick Schumacher",         shortName = "MSC", carNumber = 0,  teamID = 1, pace = 81, racecraft = 80, defending = 75, tireManagement = 79, wetSkill = 78, experience = 72, morale = 76, isReserve = true  },
            };
        }
    }
}
