// Models.cs — Runtime data models for the F1Manager race simulation.
// Pure C#, no Unity dependencies. Mirrors Python core/models.py + core/tire.py.
using System;
using System.Collections.Generic;
using System.Linq;

namespace F1Manager
{
    // ─────────────────────────────────────────────────────────────────────────
    // Tire Profile — per-compound degradation constants
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Immutable degradation parameters for one tyre compound at the baseline
    /// deg-multiplier of 1.0 (Silverstone).  All runtime phase calculations
    /// live in TireSystem; this struct is pure data.
    /// </summary>
    public struct TireProfile
    {
        public TireCompound Compound;

        // Phase 1 — warm-up
        public int   WarmUpLaps;            // Laps before full grip is available
        public float WarmUpPenaltyS;        // Lap-time loss on lap 1 of a fresh set
        public float WarmUpPenaltyLap2S;    // Lap-time loss on lap 2 (if warmUpLaps >= 2)

        // Phase 2 — plateau
        public int   PlateauLaps;           // Stable near-peak performance laps

        // Phase 3 — linear
        public float LinearDegPerLapS;      // Added seconds per lap during linear phase
        public int   LinearPhaseLaps;       // Duration of linear phase before cliff

        // Phase 4 — cliff
        public float CliffPenaltyS;         // Extra time loss per lap once in cliff

        // Pace offset relative to C4/Medium at optimal conditions.
        // Positive = slower than C4 (harder compound); negative = faster (softer).
        public float GripBonusS;

        public TireProfile(
            TireCompound compound,
            int   warmUpLaps,
            float warmUpPenaltyS,
            float warmUpPenaltyLap2S,
            int   plateauLaps,
            float linearDegPerLapS,
            int   linearPhaseLaps,
            float cliffPenaltyS,
            float gripBonusS)
        {
            Compound             = compound;
            WarmUpLaps           = warmUpLaps;
            WarmUpPenaltyS       = warmUpPenaltyS;
            WarmUpPenaltyLap2S   = warmUpPenaltyLap2S;
            PlateauLaps          = plateauLaps;
            LinearDegPerLapS     = linearDegPerLapS;
            LinearPhaseLaps      = linearPhaseLaps;
            CliffPenaltyS        = cliffPenaltyS;
            GripBonusS           = gripBonusS;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TireProfiles — static lookup table
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Canonical tyre profile data for all eight compounds.
    /// Values calibrated to GDD §6.2.1 target lap counts at Silverstone (deg_mult=1.0):
    ///   C3 Hard  → plateau ~22, cliff ~44–48 laps
    ///   C4 Medium → plateau ~18, cliff ~34–36 laps
    ///   C5 Soft  → plateau ~12, cliff ~23–24 laps
    /// </summary>
    public static class TireProfiles
    {
        public static readonly Dictionary<TireCompound, TireProfile> All =
            new Dictionary<TireCompound, TireProfile>
        {
            // compound         warm  pen1  pen2  plat  linDeg linLps cliff  grip
            { TireCompound.C1, new TireProfile(
                TireCompound.C1,   2, 2.5f, 1.5f,  35, 0.018f, 28, 0.30f,  0.25f) },

            { TireCompound.C2, new TireProfile(
                TireCompound.C2,   2, 2.0f, 1.2f,  28, 0.022f, 24, 0.35f,  0.20f) },

            { TireCompound.C3, new TireProfile(
                TireCompound.C3,   2, 2.0f, 1.0f,  22, 0.028f, 22, 0.40f,  0.15f) },

            { TireCompound.C4, new TireProfile(
                TireCompound.C4,   1, 1.5f, 0.8f,  18, 0.040f, 16, 0.50f,  0.00f) },

            { TireCompound.C5, new TireProfile(
                TireCompound.C5,   1, 1.0f, 0.4f,  12, 0.058f, 11, 0.60f, -0.25f) },

            { TireCompound.C6, new TireProfile(
                TireCompound.C6,   1, 0.8f, 0.3f,   9, 0.080f,  9, 0.80f, -0.30f) },

            { TireCompound.INTER, new TireProfile(
                TireCompound.INTER, 1, 0.5f, 0.2f, 20, 0.040f, 15, 0.40f,  0.00f) },

            { TireCompound.WET, new TireProfile(
                TireCompound.WET,  1, 0.4f, 0.1f,  25, 0.030f, 20, 0.30f,  0.00f) },
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CarState — per-car mutable race state
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// All mutable state for one car during a race lap tick.
    /// Written by RaceEngine; read by UI, AI, and strategy systems.
    /// </summary>
    public class CarState
    {
        // Identity
        public int DriverID;
        public int TeamID;
        public int CarNumber;

        // Race progress
        public int   Position;
        public int   LapsCompleted;
        public float TotalRaceTimeS;
        public float LastLapTimeS;
        public float BestLapTimeS;
        public float GapToLeaderS;
        public float GapToAheadS;

        // Tyre state
        public TireCompound Compound      = TireCompound.C5;
        public int          TireAgeLaps;
        public TirePhase    TirePhase     = TirePhase.PLATEAU;
        public float        TireDegPct;   // 0 = fresh, 1 = destroyed

        // Fuel
        public float FuelKg = 105f;

        // Driver instruction
        public DriverInstruction Instruction = DriverInstruction.MANAGE;

        // Morale modifier — pace bonus/penalty from driver psychology (seconds)
        public float MoraleModifierS;

        // Incident / damage
        public bool   DNF;
        public string DNFReason = string.Empty;
        public float  DamageS;              // Lap-time penalty from bodywork damage

        // Pit stop state
        public bool  IsPittingThisLap;
        public bool  PittedLastLap;
        public float PitStopDurationS;
        public int   PitStopCount;

        // Compound rule tracking
        public List<TireCompound> CompoundsUsed = new List<TireCompound>();

        // ERS
        public float ErsChargePct = 0.80f; // 0–1

        // Historical data (sparklines, gap trend)
        public List<float> LapTimes   = new List<float>();
        public List<float> GapHistory = new List<float>();

        // Sector times — [0]=S1, [1]=S2, [2]=S3
        public float[] LastSectorTimes = new float[3];
        public float[] BestSectorTimes = new float[3] { 9999f, 9999f, 9999f };

        public CarState(int driverID, int teamID, int carNumber)
        {
            DriverID  = driverID;
            TeamID    = teamID;
            CarNumber = carNumber;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RaceEvent — event log entry
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// A single entry in the race event log (pit stops, overtakes, incidents, etc.).
    /// Displayed in the live commentary panel.
    /// </summary>
    public class RaceEvent
    {
        public int    Lap;
        public string DriverName;
        public string Category;     // "PIT" | "OVERTAKE" | "DNF" | "SC" | "WEATHER" | "INFO"
        public string Message;
        public bool   IsPlayerEvent;

        public RaceEvent(int lap, string driverName, string category, string message, bool isPlayerEvent = false)
        {
            Lap           = lap;
            DriverName    = driverName;
            Category      = category;
            Message       = message;
            IsPlayerEvent = isPlayerEvent;
        }

        public override string ToString()
        {
            string prefix = IsPlayerEvent ? "★ " : "  ";
            return $"{prefix}[L{Lap:D2}] {Message}";
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PitStopResult
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Outcome record for a completed pit stop.</summary>
    public class PitStopResult
    {
        public int          DriverID;
        public int          Lap;
        public TireCompound OldCompound;
        public TireCompound NewCompound;
        public float        DurationS;

        public PitStopResult(int driverID, int lap, TireCompound oldCompound, TireCompound newCompound, float durationS)
        {
            DriverID    = driverID;
            Lap         = lap;
            OldCompound = oldCompound;
            NewCompound = newCompound;
            DurationS   = durationS;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // OvertakeResult
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Outcome record for one resolved overtake attempt.</summary>
    public class OvertakeResult
    {
        public int   AttackerID;
        public int   DefenderID;
        public int   Lap;
        public bool  Success;
        public float PSuccess;          // Probability that was rolled against
        public bool  WasDRS;
        public int   NewPositionAttacker;

        public OvertakeResult(int attackerID, int defenderID, int lap,
                              bool success, float pSuccess, bool wasDRS, int newPositionAttacker)
        {
            AttackerID           = attackerID;
            DefenderID           = defenderID;
            Lap                  = lap;
            Success              = success;
            PSuccess             = pSuccess;
            WasDRS               = wasDRS;
            NewPositionAttacker  = newPositionAttacker;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RaceState — top-level race snapshot
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Complete snapshot of a race at the end of one lap tick.
    /// Written by RaceEngine; read by all other systems and the UI layer.
    /// </summary>
    public class RaceState
    {
        // Circuit info
        public string CircuitName;
        public int    TotalLaps;
        public int    CurrentLap;
        public bool   IsRaceComplete;

        // Cars
        public List<CarState> Cars = new List<CarState>();

        // Weather
        public WeatherCondition       Weather         = WeatherCondition.DRY;
        public float                  TrackTempC      = 28f;
        public float                  AirTempC        = 22f;
        public List<WeatherCondition> WeatherForecast = new List<WeatherCondition>(); // next 5 laps

        // Safety car
        public SafetyCarState SafetyCar                = SafetyCarState.NONE;
        public int            SafetyCarLapsRemaining;

        // Event log
        public List<RaceEvent> Events = new List<RaceEvent>();

        // Player's constructor
        public int PlayerTeamID;

        // Available compounds for this race weekend
        public List<TireCompound> AvailableCompounds = new List<TireCompound>();

        // Fastest-lap tracking
        public float FastestLapTimeS    = 9999f;
        public int   FastestLapDriverID = -1;

        // Global overtake counter (for stats / achievements)
        public int OverallOvertakeCount;

        public RaceState(string circuitName, int totalLaps)
        {
            CircuitName = circuitName;
            TotalLaps   = totalLaps;
        }

        /// <summary>
        /// Returns all cars ordered by race position (leader first).
        /// Active cars sort by laps completed desc then total time asc;
        /// DNF cars are appended at the end.
        /// </summary>
        public List<CarState> SortedCars()
        {
            var active = Cars.Where(c => !c.DNF)
                             .OrderByDescending(c => c.LapsCompleted)
                             .ThenBy(c => c.TotalRaceTimeS)
                             .ToList();
            var dnfd   = Cars.Where(c => c.DNF).ToList();
            active.AddRange(dnfd);
            return active;
        }

        /// <summary>
        /// Returns the player's active (non-DNF) cars.
        /// Used to populate the player telemetry panel.
        /// </summary>
        public List<CarState> GetPlayerCars()
        {
            return Cars.Where(c => c.TeamID == PlayerTeamID && !c.DNF).ToList();
        }
    }
}
