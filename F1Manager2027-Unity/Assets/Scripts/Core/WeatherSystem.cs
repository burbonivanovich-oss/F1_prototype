// WeatherSystem.cs — Dynamic weather state machine for the F1Manager race engine.
// Mirrors Python core/weather.py exactly.  Uses System.Random, no Unity dependencies.
using System;
using System.Collections.Generic;
using System.Linq;

namespace F1Manager
{
    /// <summary>
    /// Manages weather transitions across a race weekend, including:
    /// <list type="bullet">
    ///   <item>Hidden rain event planning at construction time.</item>
    ///   <item>Lap-by-lap condition advancement with event messages.</item>
    ///   <item>5-lap forecast with accuracy degradation at longer horizons.</item>
    ///   <item>Lap-time penalties for running the wrong tyre compound.</item>
    ///   <item>Aquaplaning probability in rain conditions.</item>
    /// </list>
    /// </summary>
    public class WeatherSystem
    {
        // ── Current state ─────────────────────────────────────────────────────
        public WeatherCondition Condition  { get; private set; } = WeatherCondition.DRY;
        public float  TrackTempC          { get; private set; }
        public float  AirTempC            { get; private set; }
        public float  WaterDepthMm        { get; private set; }

        // ── Forecast ──────────────────────────────────────────────────────────
        /// <summary>5-lap rolling forecast; index 0 = next lap. Accuracy degrades with horizon.</summary>
        public List<WeatherCondition> Forecast { get; private set; } = new List<WeatherCondition>();

        // ── Rain event plan (hidden from player) ─────────────────────────────
        public int   RainStartLap        { get; private set; } = -1; // -1 = no rain
        public int   RainEndLap          { get; private set; } = -1;
        public float RainIntensityPeak   { get; private set; }        // 0–1

        // ── Private backing fields ────────────────────────────────────────────
        private readonly CircuitInfo _circuit;
        private readonly System.Random _rng;
        private readonly int _totalLaps;

        // ─────────────────────────────────────────────────────────────────────
        // Construction
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Initialises weather for one race.  Calls <c>PlanWeatherEvents</c> and
        /// <c>UpdateForecast(0)</c> so the pre-race state is immediately valid.
        /// </summary>
        /// <param name="circuit">Circuit ScriptableObject supplying temperature ranges and rain probability.</param>
        /// <param name="rng">Seeded random instance for reproducible QA runs.</param>
        /// <param name="totalLaps">Race distance in laps.</param>
        public WeatherSystem(CircuitInfo circuit, System.Random rng, int totalLaps)
        {
            _circuit   = circuit;
            _rng       = rng;
            _totalLaps = totalLaps;

            // Randomise starting track temperature within the circuit's range
            float tempRange = circuit.trackTempRangeMax - circuit.trackTempRangeMin;
            TrackTempC = circuit.trackTempRangeMin + (float)(_rng.NextDouble() * tempRange);
            AirTempC   = TrackTempC - (float)(_rng.NextDouble() * 6.0 + 4.0); // 4–10°C below track

            PlanWeatherEvents();
            UpdateForecast(0);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Internal helpers
        // ─────────────────────────────────────────────────────────────────────

        private void PlanWeatherEvents()
        {
            if (_rng.NextDouble() > _circuit.rainProbability)
                return; // Dry race

            int minStart = 5;
            int maxStart = Math.Max(6, _totalLaps - 10);
            RainStartLap = _rng.Next(minStart, maxStart + 1);

            int duration = _rng.Next(5, 21);
            RainEndLap   = Math.Min(_totalLaps, RainStartLap + duration);

            float baseIntensity = _circuit.rainProbability * 1.2f;
            float lo = baseIntensity * 0.4f;
            float hi = Math.Min(1.0f, baseIntensity * 1.6f);
            RainIntensityPeak = lo + (float)(_rng.NextDouble() * (hi - lo));
        }

        private void UpdateForecast(int currentLap)
        {
            Forecast = new List<WeatherCondition>();
            for (int offset = 1; offset <= 5; offset++)
            {
                int lap = currentLap + offset;
                Forecast.Add(ForecastCondition(lap, offset));
            }
        }

        private WeatherCondition ForecastCondition(int lap, int horizon)
        {
            // Accuracy degrades with distance: horizon 1 = 97%, 3 = 91%, 5 = 85%
            double accuracy = Math.Max(0.80, 0.97 - horizon * 0.03);
            if (_rng.NextDouble() > accuracy)
            {
                // Deliberate mis-prediction: flip DRY↔LIGHT_RAIN
                var trueCondition = TrueConditionAt(lap);
                return trueCondition != WeatherCondition.DRY
                    ? WeatherCondition.DRY
                    : WeatherCondition.LIGHT_RAIN;
            }
            return TrueConditionAt(lap);
        }

        private WeatherCondition TrueConditionAt(int lap)
        {
            if (RainStartLap == -1)        return WeatherCondition.DRY;
            if (lap < RainStartLap)        return WeatherCondition.DRY;
            if (lap < RainEndLap)
                return RainIntensityPeak > 0.6f
                    ? WeatherCondition.HEAVY_RAIN
                    : WeatherCondition.LIGHT_RAIN;
            if (lap < RainEndLap + 3)      return WeatherCondition.DRYING;
            return WeatherCondition.DRY;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Advances weather to <paramref name="lap"/>.
        /// Returns a list of human-readable event messages (empty list on a quiet lap).
        /// Called once per lap tick by <c>RaceEngine</c> before lap-time computation.
        /// </summary>
        public List<string> Advance(int lap)
        {
            var messages = new List<string>();

            if (RainStartLap != -1)
            {
                if (lap == RainStartLap)
                {
                    if (RainIntensityPeak > 0.6f)
                    {
                        Condition     = WeatherCondition.HEAVY_RAIN;
                        WaterDepthMm  = RainIntensityPeak * 12f;
                        messages.Add("Heavy rain has arrived! Wet tyres strongly advised.");
                    }
                    else
                    {
                        Condition     = WeatherCondition.LIGHT_RAIN;
                        WaterDepthMm  = RainIntensityPeak * 5f;
                        messages.Add("Light rain falling. Intermediates coming into play.");
                    }
                    // Track cools when rain arrives
                    TrackTempC = Math.Max(10f, TrackTempC - (float)(_rng.NextDouble() * 5.0 + 5.0));
                }
                else if (lap == RainEndLap)
                {
                    Condition    = WeatherCondition.DRYING;
                    WaterDepthMm *= 0.2f;
                    messages.Add("Rain has stopped. Track is drying — slick tyres soon viable.");
                }
                else if (lap > RainEndLap && Condition == WeatherCondition.DRYING)
                {
                    int dryingLaps = lap - RainEndLap;
                    if (dryingLaps >= 3)
                    {
                        Condition    = WeatherCondition.DRY;
                        WaterDepthMm = 0f;
                        float maxTemp = _circuit.trackTempRangeMax;
                        TrackTempC = Math.Min(maxTemp, TrackTempC + dryingLaps * 1.5f);
                        messages.Add("Track fully dry. Slick tyres are now optimal.");
                    }
                    else
                    {
                        WaterDepthMm *= 0.5f;
                    }
                }
            }

            // Natural temperature oscillation ±0.3°C per lap
            TrackTempC += (float)(_rng.NextDouble() * 0.6 - 0.3);
            TrackTempC  = Math.Max(5f, Math.Min(50f, TrackTempC));
            AirTempC    = TrackTempC - (float)(_rng.NextDouble() * 4.0 + 4.0);

            UpdateForecast(lap);
            return messages;
        }

        /// <summary>
        /// Returns the lap-time penalty (seconds) for running <paramref name="compound"/>
        /// in the current weather condition.  Correct tyre = 0 s; wrong tyre = large penalty.
        /// Matches Python weather.py <c>lap_time_weather_penalty_s</c> exactly.
        /// </summary>
        public float LapTimeWeatherPenaltyS(TireCompound compound)
        {
            bool isDryTyre = compound.IsDry();
            bool isInter   = compound == TireCompound.INTER;
            bool isWet     = compound == TireCompound.WET;

            switch (Condition)
            {
                case WeatherCondition.DRY:
                    if (isDryTyre) return 0f;
                    if (isInter)   return 1.5f;  // Inters on dry — overheating graining
                    return 2.5f;                  // Full wets on dry — extremely slow

                case WeatherCondition.LIGHT_RAIN:
                    if (isInter) return 0f;       // Optimal
                    if (isDryTyre)
                    {
                        if (WaterDepthMm < 2f) return 0.6f;
                        if (WaterDepthMm < 4f) return 1.2f;
                        return 2.0f;              // Aquaplaning risk
                    }
                    return 0.8f;                  // Full wets in light rain — too much rubber

                case WeatherCondition.HEAVY_RAIN:
                    if (isWet) return 0f;          // Optimal
                    if (isInter)
                    {
                        if (WaterDepthMm < 5f)  return 0.3f;
                        if (WaterDepthMm < 8f)  return 0.8f;
                        return 1.5f;
                    }
                    return 3.5f;                  // Slicks in heavy rain — dangerous

                case WeatherCondition.DRYING:
                    if (isInter)   return 0.2f;   // Still reasonable
                    if (isDryTyre) return 0.5f;   // Damp track — small penalty
                    return 1.0f;                  // Full wets on drying track

                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Probability (0–1) of an aquaplaning incident this lap.
        /// Only non-zero when slicks or inters are used in heavy standing water.
        /// Matches Python weather.py <c>aquaplaning_chance</c> exactly.
        /// </summary>
        public float AquaplaningChance(TireCompound compound)
        {
            if (compound.IsDry() &&
                (Condition == WeatherCondition.LIGHT_RAIN ||
                 Condition == WeatherCondition.HEAVY_RAIN))
            {
                if (WaterDepthMm < 3f) return 0.012f;
                if (WaterDepthMm < 6f) return 0.025f;
                return 0.040f;
            }

            if (compound == TireCompound.INTER && Condition == WeatherCondition.HEAVY_RAIN)
            {
                if (WaterDepthMm > 10f) return 0.004f;
                if (WaterDepthMm >  7f) return 0.002f;
            }

            return 0f;
        }

        /// <summary>
        /// Returns the currently optimal tyre compound for the prevailing conditions.
        /// For dry conditions returns <see cref="TireCompound.C5"/> as a generic default;
        /// callers should override with the best available dry compound for the circuit.
        /// </summary>
        public TireCompound RecommendedCompound()
        {
            if (Condition == WeatherCondition.HEAVY_RAIN)                             return TireCompound.WET;
            if (Condition == WeatherCondition.LIGHT_RAIN ||
                Condition == WeatherCondition.DRYING)                                  return TireCompound.INTER;
            return TireCompound.C5; // Dry default — caller should resolve to circuit soft
        }
    }
}
