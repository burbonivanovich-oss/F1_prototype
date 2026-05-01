// Enums.cs — All F1Manager enumerations. Pure C#, no Unity dependencies.
using System;

namespace F1Manager
{
    // ─────────────────────────────────────────────────────────────────────────
    // Tire Compounds
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// F1 tyre compound spectrum. C1 = ultra-hard (longest life, least grip) through
    /// C6 = ultra-soft (shortest life, most grip). INTER and WET are rain compounds.
    /// Each race weekend uses three consecutive dry compounds selected per circuit.
    /// </summary>
    public enum TireCompound
    {
        C1   = 1,   // Ultra-Hard   — Monza / Baku only
        C2   = 2,   // Hard
        C3   = 3,   // Hard (standard)
        C4   = 4,   // Medium
        C5   = 5,   // Soft
        C6   = 6,   // Ultra-Soft  — Monaco / Singapore / Hungary
        INTER = 7,  // Intermediate (light-to-medium rain)
        WET   = 8,  // Full Wet    (heavy rain)
    }

    /// <summary>Extension helpers for TireCompound — display name, UI colour, and pace data.</summary>
    public static class TireCompoundExtensions
    {
        /// <summary>Short label shown in timing tower and telemetry HUD.</summary>
        public static string DisplayName(this TireCompound c)
        {
            switch (c)
            {
                case TireCompound.C1:    return "UH";
                case TireCompound.C2:    return "H";
                case TireCompound.C3:    return "H";
                case TireCompound.C4:    return "M";
                case TireCompound.C5:    return "S";
                case TireCompound.C6:    return "US";
                case TireCompound.INTER: return "INT";
                case TireCompound.WET:   return "WET";
                default:                 return "?";
            }
        }

        /// <summary>
        /// CSS/hex colour string for UI rendering.
        /// Matches the FIA visual identity: hard = white, medium = yellow,
        /// soft = red, inter = green, wet = blue.
        /// </summary>
        public static string Color(this TireCompound c)
        {
            switch (c)
            {
                case TireCompound.C1:
                case TireCompound.C2:
                case TireCompound.C3:    return "#FFFFFF";   // White — hard family
                case TireCompound.C4:    return "#FFD700";   // Yellow — medium
                case TireCompound.C5:
                case TireCompound.C6:    return "#FF3333";   // Red — soft family
                case TireCompound.INTER: return "#00CC44";   // Green — intermediate
                case TireCompound.WET:   return "#3399FF";   // Blue — full wet
                default:                 return "#888888";
            }
        }

        /// <summary>True for slick (dry) compounds; false for INTER and WET.</summary>
        public static bool IsDry(this TireCompound c)
        {
            return c != TireCompound.INTER && c != TireCompound.WET;
        }

        /// <summary>
        /// Lap-time advantage in seconds relative to the C4 (Medium) baseline at optimal
        /// operating temperature. Positive = faster than C4; negative = slower than C4.
        ///
        /// C1 = +0.25 s  (harder/slower but durable)
        /// C4 =  0.00 s  (reference)
        /// C6 = -0.30 s  (softest, quickest in clean air)
        /// INTER/WET = 0.00 s (comparison irrelevant — weather-dependent)
        /// </summary>
        public static float GripAdvantageS(this TireCompound c)
        {
            switch (c)
            {
                case TireCompound.C1:    return  0.25f;
                case TireCompound.C2:    return  0.20f;
                case TireCompound.C3:    return  0.15f;
                case TireCompound.C4:    return  0.00f;
                case TireCompound.C5:    return -0.25f;
                case TireCompound.C6:    return -0.30f;
                case TireCompound.INTER: return  0.00f;
                case TireCompound.WET:   return  0.00f;
                default:                 return  0.00f;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Driver Instructions
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Per-driver tactical instruction. Changes lap-time delta, tyre wear rate,
    /// fuel consumption, and defender aggression in the overtake system.
    /// </summary>
    public enum DriverInstruction
    {
        /// <summary>Economy mode. -0.15 s/lap pace; -15 % tyre wear; -12 % fuel burn.</summary>
        MANAGE,

        /// <summary>Attack mode. -0.25 s/lap pace (faster); +15 % tyre wear.</summary>
        ATTACK,

        /// <summary>Defensive mode. Activates high defender aggression in overtake sigmoid.</summary>
        DEFEND,

        /// <summary>Fuel-saving mode. -0.10 s/lap pace; -12 % fuel consumption.</summary>
        FUEL_SAVE,
    }

    public static class DriverInstructionExtensions
    {
        public static string DisplayName(this DriverInstruction d)
        {
            switch (d)
            {
                case DriverInstruction.MANAGE:    return "Manage";
                case DriverInstruction.ATTACK:    return "Attack";
                case DriverInstruction.DEFEND:    return "Defend";
                case DriverInstruction.FUEL_SAVE: return "Fuel Save";
                default:                          return d.ToString();
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Team Orders
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Constructor-level instruction governing how the two teammates interact.</summary>
    public enum TeamOrder
    {
        /// <summary>No restriction — both drivers race freely.</summary>
        FREE_RACE,

        /// <summary>Faster car holds a set gap; slower car must not overtake teammate.</summary>
        HOLD_GAP,

        /// <summary>Request a position swap when conditions are met (tyre delta ≥ 5, gap &lt; 0.5 s).</summary>
        SWAP_DRIVERS,

        /// <summary>Both drivers set to ATTACK instruction simultaneously.</summary>
        PUSH_BOTH,
    }

    public static class TeamOrderExtensions
    {
        public static string DisplayName(this TeamOrder t)
        {
            switch (t)
            {
                case TeamOrder.FREE_RACE:    return "Free Race";
                case TeamOrder.HOLD_GAP:     return "Hold Gap";
                case TeamOrder.SWAP_DRIVERS: return "Swap Drivers";
                case TeamOrder.PUSH_BOTH:    return "Push Both";
                default:                     return t.ToString();
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Weather Conditions
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Current atmospheric / track-surface condition driving tyre-choice logic.</summary>
    public enum WeatherCondition
    {
        /// <summary>Track fully dry; slick tyres optimal.</summary>
        DRY,

        /// <summary>Light rainfall; intermediates optimal.</summary>
        LIGHT_RAIN,

        /// <summary>Heavy rainfall; full wet tyres required.</summary>
        HEAVY_RAIN,

        /// <summary>Rain has stopped but track still damp; transitional window.</summary>
        DRYING,
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Safety Car States
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Current safety-car deployment status affecting lap times and pit strategy.</summary>
    public enum SafetyCarState
    {
        /// <summary>Green flag; normal racing.</summary>
        NONE,

        /// <summary>Full Safety Car on track; cars bunched behind SC.</summary>
        DEPLOYED,

        /// <summary>Virtual Safety Car; cars must respect minimum lap-time delta.</summary>
        VSC,

        /// <summary>Safety Car returning to pit lane; restart next lap.</summary>
        ENDING,
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tire Phases
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Four-phase tyre degradation model.
    /// WARM_UP → PLATEAU → LINEAR → CLIFF
    /// </summary>
    public enum TirePhase
    {
        /// <summary>First 1-2 laps after a pit stop; fixed lap-time penalty while tyre heats up.</summary>
        WARM_UP,

        /// <summary>Peak performance window; near-zero degradation per lap.</summary>
        PLATEAU,

        /// <summary>Progressive wear; predictable and calculable per-lap time loss.</summary>
        LINEAR,

        /// <summary>Rapid performance collapse; driver must box immediately.</summary>
        CLIFF,
    }
}
