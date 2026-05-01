// QualifyingEngineTests.cs — Edit-mode unit tests for QualifyingEngine.
using System.Collections.Generic;
using NUnit.Framework;
using F1Manager;

namespace F1Manager.Tests
{
    [TestFixture]
    public class QualifyingEngineTests
    {
        private CircuitInfo _circuit;
        private Dictionary<int, DriverInfo> _drivers;
        private Dictionary<int, TeamInfo>   _teams;

        [SetUp]
        public void SetUp()
        {
            _circuit = new CircuitInfo
            {
                id                = 1,
                circuitName       = "Test Circuit",
                baseLapTimeS      = 90f,
                powerSensitivity  = 0.5f,
                tireDegMultiplier = 1.0f,
                overtakeDifficulty = 0.3f,
                totalLaps         = 57,
                circuitLengthKm   = 5.3f,
                trackTempRangeMin = 30f,
                trackTempRangeMax = 45f,
                rainProbability   = 0f
            };

            // 2 teams, 4 drivers (no reserves)
            _teams = new Dictionary<int, TeamInfo>
            {
                { 1, new TeamInfo { id=1, teamName="Alpha", colorHex="#FF0000", carPerformance=80, powerUnit=92, chassis=88 } },
                { 2, new TeamInfo { id=2, teamName="Beta",  colorHex="#0000FF", carPerformance=75, powerUnit=89, chassis=85 } },
            };

            _drivers = new Dictionary<int, DriverInfo>();
            for (int i = 1; i <= 20; i++)
            {
                _drivers[i] = new DriverInfo
                {
                    id          = i,
                    driverName  = $"Driver{i}",
                    teamID      = i <= 2 ? 1 : 2,
                    pace        = 75 + i,
                    racecraft   = 70,
                    defending   = 70,
                    isReserve   = false,
                    carNumber   = i
                };
            }
        }

        // ── RunQualifying ─────────────────────────────────────────────────────

        [Test]
        public void RunQualifying_ReturnsThreeSessions()
        {
            var engine = new QualifyingEngine(_circuit, _drivers, _teams, new System.Random(1));
            var result = engine.RunQualifying(
                new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 }, 1);

            Assert.IsNotNull(result.Sessions[0], "Q1 session should not be null");
            Assert.IsNotNull(result.Sessions[1], "Q2 session should not be null");
            Assert.IsNotNull(result.Sessions[2], "Q3 session should not be null");
        }

        [Test]
        public void RunQualifying_Q1Eliminates5Drivers()
        {
            var engine = new QualifyingEngine(_circuit, _drivers, _teams, new System.Random(2));
            var result = engine.RunQualifying(
                new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 }, 1);

            Assert.AreEqual(5, result.Sessions[0].EliminatedDriverIDs.Count,
                "Q1 should eliminate exactly 5 drivers (20→15)");
        }

        [Test]
        public void RunQualifying_Q2Eliminates5Drivers()
        {
            var engine = new QualifyingEngine(_circuit, _drivers, _teams, new System.Random(3));
            var result = engine.RunQualifying(
                new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 }, 1);

            Assert.AreEqual(5, result.Sessions[1].EliminatedDriverIDs.Count,
                "Q2 should eliminate exactly 5 drivers (15→10)");
        }

        [Test]
        public void RunQualifying_GridOrderHas20Drivers()
        {
            var engine = new QualifyingEngine(_circuit, _drivers, _teams, new System.Random(4));
            var result = engine.RunQualifying(
                new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 }, 1);

            Assert.AreEqual(20, result.GridOrder.Count, "Grid must have 20 driver entries");
        }

        [Test]
        public void RunQualifying_GridOrderContainsAllDriverIDs()
        {
            var engine = new QualifyingEngine(_circuit, _drivers, _teams, new System.Random(5));
            var result = engine.RunQualifying(
                new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 }, 1);

            var gridSet = new HashSet<int>(result.GridOrder);
            foreach (var id in _drivers.Keys)
                Assert.IsTrue(gridSet.Contains(id), $"Driver {id} missing from grid");
        }

        [Test]
        public void RunQualifying_PoleSitterIsFirstInGrid()
        {
            var engine = new QualifyingEngine(_circuit, _drivers, _teams, new System.Random(6));
            var result = engine.RunQualifying(
                new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 }, 1);

            Assert.AreEqual(result.PoleSitterID, result.GridOrder[0],
                "Pole sitter should start P1");
        }

        [Test]
        public void RunQualifying_Q2CompoundMapHas10Entries()
        {
            var engine = new QualifyingEngine(_circuit, _drivers, _teams, new System.Random(7));
            var result = engine.RunQualifying(
                new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 }, 1);

            Assert.AreEqual(10, result.Q2CompoundMap.Count,
                "Q2 compound map must cover all 10 Q3 qualifiers");
        }

        [Test]
        public void RunQualifying_IsDeterministicWithSameSeed()
        {
            var compounds = new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 };
            var r1 = new QualifyingEngine(_circuit, _drivers, _teams, new System.Random(99))
                         .RunQualifying(compounds, 1);
            var r2 = new QualifyingEngine(_circuit, _drivers, _teams, new System.Random(99))
                         .RunQualifying(compounds, 1);

            Assert.AreEqual(r1.PoleSitterID, r2.PoleSitterID, "Same seed must produce same pole sitter");
            Assert.AreEqual(r1.PoleTimeS, r2.PoleTimeS, 0.0001f, "Same seed must produce same pole time");
        }

        // ── RunQualifyingLapForDriver ──────────────────────────────────────────

        [Test]
        public void RunQualifyingLapForDriver_ValidDriver_ReturnsPlausibleTime()
        {
            var engine = new QualifyingEngine(_circuit, _drivers, _teams, new System.Random(0));
            float t = engine.RunQualifyingLapForDriver(1, TireCompound.C5, 1);
            Assert.Greater(t, 60f, "Lap time should be over 60s");
            Assert.Less(t, 200f, "Lap time should be under 200s");
        }

        [Test]
        public void RunQualifyingLapForDriver_InvalidDriver_Returns9999()
        {
            var engine = new QualifyingEngine(_circuit, _drivers, _teams, new System.Random(0));
            float t = engine.RunQualifyingLapForDriver(9999, TireCompound.C5, 1);
            Assert.AreEqual(9999f, t, "Unknown driver should return sentinel 9999");
        }

        [Test]
        public void RunQualifyingLapForDriver_Q3IsFasterThanQ1()
        {
            // Q3 has more track evolution, so laps should be marginally faster on average
            // Run multiple times with different seeds to verify the trend holds
            float sumQ1 = 0, sumQ3 = 0;
            for (int seed = 0; seed < 20; seed++)
            {
                var e = new QualifyingEngine(_circuit, _drivers, _teams, new System.Random(seed));
                sumQ1 += e.RunQualifyingLapForDriver(1, TireCompound.C5, 1);
                var e2 = new QualifyingEngine(_circuit, _drivers, _teams, new System.Random(seed));
                sumQ3 += e2.RunQualifyingLapForDriver(1, TireCompound.C5, 3);
            }
            Assert.Less(sumQ3 / 20, sumQ1 / 20, "Q3 should be faster on average due to track evolution");
        }
    }
}
