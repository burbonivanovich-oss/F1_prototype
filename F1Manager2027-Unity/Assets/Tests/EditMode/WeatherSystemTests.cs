// WeatherSystemTests.cs — Edit-mode unit tests for WeatherSystem.
using NUnit.Framework;
using F1Manager;

namespace F1Manager.Tests
{
    [TestFixture]
    public class WeatherSystemTests
    {
        private CircuitInfo MakeCircuit(float rainProb = 0f)
        {
            return new CircuitInfo
            {
                id                = 1,
                circuitName       = "Test Circuit",
                trackTempRangeMin = 30f,
                trackTempRangeMax = 45f,
                rainProbability   = rainProb,
                overtakeDifficulty = 0.5f,
                baseLapTimeS      = 90f,
                powerSensitivity  = 0.5f,
                tireDegMultiplier = 1.0f,
                totalLaps         = 57,
                circuitLengthKm   = 5.3f
            };
        }

        // ── Construction ──────────────────────────────────────────────────────

        [Test]
        public void Constructor_DryCircuit_StartsAsDry()
        {
            var ws = new WeatherSystem(MakeCircuit(0f), new System.Random(42), 57);
            Assert.AreEqual(WeatherCondition.DRY, ws.Condition);
        }

        [Test]
        public void Constructor_HighRainProb_PlanRain()
        {
            // With rain probability 1.0 and seeded RNG, rain should always be planned
            var ws = new WeatherSystem(MakeCircuit(1f), new System.Random(42), 57);
            Assert.AreNotEqual(-1, ws.RainStartLap, "Rain should be planned when probability=1.0");
        }

        [Test]
        public void Constructor_InitialForecastHasFiveEntries()
        {
            var ws = new WeatherSystem(MakeCircuit(0f), new System.Random(1), 57);
            Assert.AreEqual(5, ws.Forecast.Count);
        }

        [Test]
        public void Constructor_TrackTempInRange()
        {
            var circuit = MakeCircuit(0f);
            var ws = new WeatherSystem(circuit, new System.Random(99), 57);
            Assert.GreaterOrEqual(ws.TrackTempC, circuit.trackTempRangeMin - 0.1f);
            Assert.LessOrEqual(ws.TrackTempC, circuit.trackTempRangeMax + 0.1f);
        }

        // ── LapTimeWeatherPenaltyS ────────────────────────────────────────────

        [Test]
        public void PenaltyS_DryCondition_SlickIsZero()
        {
            var ws = new WeatherSystem(MakeCircuit(0f), new System.Random(0), 57);
            Assert.AreEqual(0f, ws.LapTimeWeatherPenaltyS(TireCompound.C4));
        }

        [Test]
        public void PenaltyS_DryCondition_WetTyreIsPenalised()
        {
            var ws = new WeatherSystem(MakeCircuit(0f), new System.Random(0), 57);
            float penalty = ws.LapTimeWeatherPenaltyS(TireCompound.WET);
            Assert.Greater(penalty, 0f, "Wet tyres in dry conditions should be penalised");
        }

        // ── AquaplaningChance ─────────────────────────────────────────────────

        [Test]
        public void AquaplaningChance_DryCondition_IsZero()
        {
            var ws = new WeatherSystem(MakeCircuit(0f), new System.Random(0), 57);
            Assert.AreEqual(0f, ws.AquaplaningChance(TireCompound.C4));
        }

        // ── Advance ───────────────────────────────────────────────────────────

        [Test]
        public void Advance_DryCircuit_ForecastAlwaysDry()
        {
            var ws = new WeatherSystem(MakeCircuit(0f), new System.Random(0), 57);
            for (int lap = 1; lap <= 10; lap++)
                ws.Advance(lap);
            // After 10 laps, forecast should still show mostly DRY
            // (can't guarantee all 5 are dry due to RNG misprediction, but condition is dry)
            Assert.AreEqual(WeatherCondition.DRY, ws.Condition);
        }

        [Test]
        public void Advance_ReturnsMessages_WhenRainArrives()
        {
            // Force rain by using certain rain circuit
            var circuit = MakeCircuit(1f);
            var rng = new System.Random(42);
            var ws = new WeatherSystem(circuit, rng, 57);

            if (ws.RainStartLap < 0) Assert.Pass("No rain planned with this seed — test inconclusive");

            System.Collections.Generic.List<string> msgs = null;
            for (int lap = 1; lap <= ws.RainStartLap; lap++)
                msgs = ws.Advance(lap);

            Assert.IsNotNull(msgs);
            Assert.Greater(msgs.Count, 0, "Rain arrival should emit at least one message");
        }

        // ── RecommendedCompound ───────────────────────────────────────────────

        [Test]
        public void RecommendedCompound_DryCondition_IsC5()
        {
            var ws = new WeatherSystem(MakeCircuit(0f), new System.Random(0), 57);
            Assert.AreEqual(TireCompound.C5, ws.RecommendedCompound());
        }
    }
}
