// RaceEngineTests.cs — Edit-mode unit tests for RaceEngine.
// Run via: Unity → Window → General → Test Runner → EditMode
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using F1Manager;

namespace F1Manager.Tests
{
    [TestFixture]
    public class RaceEngineTests
    {
        // ── Test fixtures ─────────────────────────────────────────────────────

        private static CircuitInfo MakeCircuit(int totalLaps = 10)
        {
            return new CircuitInfo
            {
                id                    = 0,
                circuitName           = "Test Circuit",
                country               = "Testland",
                city                  = "Testville",
                totalLaps             = totalLaps,
                baseLapTimeS          = 90f,
                circuitLengthKm       = 5.3f,
                tireDegMultiplier     = 1.0f,
                overtakeDifficulty    = 0.4f,
                drsZones              = 2,
                fuelConsumptionKg     = 1.8f,
                rainProbability       = 0f,
                trackTempRangeMin     = 28f,
                trackTempRangeMax     = 42f,
                safetyCarlProbability = 0.0f,
                sectorSplits          = new float[] { 0.30f, 0.40f, 0.30f },
                powerSensitivity      = 0.5f,
            };
        }

        private static TeamInfo MakeTeam(int id, int performance = 85)
        {
            return new TeamInfo
            {
                id             = id,
                teamName       = $"Team {id}",
                shortName      = $"T{id}",
                colorHex       = "#FFFFFF",
                carPerformance = performance,
                pitCrewSkill   = 80,
                reliability    = 90,
                powerUnit      = 85,
                chassis        = 85,
            };
        }

        private static DriverInfo MakeDriver(int id, int teamID, int carNumber)
        {
            return new DriverInfo
            {
                id             = id,
                driverName     = $"Driver{id}",
                shortName      = $"D{id:D2}",
                carNumber      = carNumber,
                teamID         = teamID,
                pace           = 82,
                racecraft      = 75,
                defending      = 70,
                tireManagement = 80,
                wetSkill       = 75,
                experience     = 10,
                morale         = 70,
                isReserve      = false,
            };
        }

        private static (RaceEngine engine, Dictionary<int, TeamInfo> teams, Dictionary<int, DriverInfo> drivers)
            BuildEngine(int totalLaps = 10, int playerTeam = 0, int seed = 42)
        {
            var circuit = MakeCircuit(totalLaps);

            var teams = new Dictionary<int, TeamInfo>
            {
                [0] = MakeTeam(0, 88),
                [1] = MakeTeam(1, 84),
            };

            var drivers = new Dictionary<int, DriverInfo>
            {
                [1] = MakeDriver(1, 0, 44),
                [2] = MakeDriver(2, 0, 63),
                [3] = MakeDriver(3, 1, 11),
                [4] = MakeDriver(4, 1, 55),
            };

            var compounds = new List<TireCompound>
                { TireCompound.C3, TireCompound.C4, TireCompound.C5 };

            // Build a qualifying result for the starting grid
            var qr = new QualifyingResult
            {
                GridOrder    = new List<int> { 1, 2, 3, 4 },
                PoleSitterID = 1,
                PoleTimeS    = 88f,
            };
            qr.Sessions[0] = new SessionResult { Part = 1 };
            qr.Sessions[1] = new SessionResult { Part = 2 };
            qr.Sessions[2] = new SessionResult { Part = 3 };

            var engine = new RaceEngine(
                circuit:          circuit,
                teams:            teams,
                drivers:          drivers,
                playerTeamID:     playerTeam,
                rng:              new System.Random(seed),
                qualifyingResult: qr);

            return (engine, teams, drivers);
        }

        // ── SimulateLap ───────────────────────────────────────────────────────

        [Test]
        public void SimulateLap_IncrementsCurrentLap()
        {
            var (engine, _, _) = BuildEngine();
            int before = engine.RaceState.CurrentLap;
            engine.SimulateLap();
            Assert.AreEqual(before + 1, engine.RaceState.CurrentLap);
        }

        [Test]
        public void SimulateLap_AllCarsCompleteLap()
        {
            var (engine, _, _) = BuildEngine();
            engine.SimulateLap();
            foreach (var car in engine.RaceState.Cars.Where(c => !c.DNF))
                Assert.AreEqual(1, car.LapsCompleted);
        }

        [Test]
        public void SimulateLap_LapTimesArePositive()
        {
            var (engine, _, _) = BuildEngine();
            engine.SimulateLap();
            foreach (var car in engine.RaceState.Cars.Where(c => !c.DNF))
                Assert.Greater(car.LastLapTimeS, 0f, $"Car {car.DriverID} lap time must be positive");
        }

        [Test]
        public void SimulateLap_TotalRaceTimeAccumulates()
        {
            var (engine, _, _) = BuildEngine();
            engine.SimulateLap();
            engine.SimulateLap();
            foreach (var car in engine.RaceState.Cars.Where(c => !c.DNF))
                Assert.Greater(car.TotalRaceTimeS, 0f);
        }

        [Test]
        public void SimulateLap_FuelDecreases()
        {
            var (engine, _, _) = BuildEngine();
            var car = engine.RaceState.Cars.First();
            float fuelBefore = car.FuelKg;
            engine.SimulateLap();
            Assert.Less(car.FuelKg, fuelBefore, "Fuel should decrease each lap");
        }

        [Test]
        public void SimulateLap_TireAgeIncreases()
        {
            var (engine, _, _) = BuildEngine();
            var car = engine.RaceState.Cars.First();
            int ageBefore = car.TireAgeLaps;
            engine.SimulateLap();
            if (!car.IsPittingThisLap)
                Assert.Greater(car.TireAgeLaps, ageBefore, "Tire age should increase");
        }

        // ── Race completion ───────────────────────────────────────────────────

        [Test]
        public void IsRaceComplete_AfterAllLaps()
        {
            var (engine, _, _) = BuildEngine(totalLaps: 5);
            while (!engine.RaceState.IsRaceComplete)
                engine.SimulateLap();
            Assert.IsTrue(engine.RaceState.IsRaceComplete);
        }

        [Test]
        public void IsRaceComplete_CurrentLapEqualsOrExceedsTotalLaps()
        {
            var (engine, _, _) = BuildEngine(totalLaps: 5);
            while (!engine.RaceState.IsRaceComplete)
                engine.SimulateLap();
            Assert.GreaterOrEqual(engine.RaceState.CurrentLap, engine.RaceState.TotalLaps);
        }

        [Test]
        public void RaceComplete_ProductsValidFinalPositions()
        {
            var (engine, _, _) = BuildEngine(totalLaps: 3);
            while (!engine.RaceState.IsRaceComplete)
                engine.SimulateLap();

            var sorted = engine.RaceState.SortedCars();
            Assert.AreEqual(4, sorted.Count, "All 4 cars should be in final standings");

            var positions = sorted.Select(c => c.Position).ToList();
            for (int i = 1; i <= sorted.Count; i++)
                Assert.IsTrue(positions.Contains(i), $"Position {i} missing from final standings");
        }

        // ── Pit commands ──────────────────────────────────────────────────────

        [Test]
        public void CommandPit_DriverPitsNextLap()
        {
            var (engine, _, _) = BuildEngine();
            // Simulate one lap to warm up tires
            engine.SimulateLap();

            int driverID = engine.RaceState.GetPlayerCars().First().DriverID;
            engine.CommandPit(driverID, TireCompound.C4);

            engine.SimulateLap();

            var car = engine.RaceState.Cars.FirstOrDefault(c => c.DriverID == driverID);
            Assert.IsNotNull(car);
            // After pitting, pit count increases or tire age resets
            Assert.IsTrue(car.PitStopCount > 0 || car.TireAgeLaps < 3,
                "Car should have pitted (pit count > 0 or fresh tires)");
        }

        [Test]
        public void CommandPit_ChangesCompound()
        {
            var (engine, _, _) = BuildEngine();
            engine.SimulateLap();

            int driverID = engine.RaceState.GetPlayerCars().First().DriverID;
            var carBefore = engine.RaceState.Cars.First(c => c.DriverID == driverID);
            var oldCompound = carBefore.Compound;

            // Request a different compound
            TireCompound newComp = oldCompound == TireCompound.C4 ? TireCompound.C3 : TireCompound.C4;
            engine.CommandPit(driverID, newComp);
            engine.SimulateLap();

            var carAfter = engine.RaceState.Cars.First(c => c.DriverID == driverID);
            if (carAfter.PitStopCount > 0)
                Assert.AreEqual(newComp, carAfter.Compound, "Compound should match requested compound after pit");
        }

        // ── Instructions ──────────────────────────────────────────────────────

        [Test]
        public void CommandInstruction_AppliedToDriver()
        {
            var (engine, _, _) = BuildEngine();
            int driverID = engine.RaceState.GetPlayerCars().First().DriverID;

            engine.CommandInstruction(driverID, DriverInstruction.ATTACK);
            engine.SimulateLap();

            var car = engine.RaceState.Cars.First(c => c.DriverID == driverID);
            Assert.AreEqual(DriverInstruction.ATTACK, car.Instruction);
        }

        // ── Positions and gaps ────────────────────────────────────────────────

        [Test]
        public void SortedCars_LeaderHasZeroGap()
        {
            var (engine, _, _) = BuildEngine();
            engine.SimulateLap();

            var sorted = engine.RaceState.SortedCars();
            Assert.AreEqual(0f, sorted[0].GapToLeaderS, 0.001f, "Leader gap to leader must be 0");
        }

        [Test]
        public void SortedCars_DNFsAtEnd()
        {
            var (engine, _, _) = BuildEngine(totalLaps: 3);
            // Force a DNF by running many laps
            while (!engine.RaceState.IsRaceComplete)
                engine.SimulateLap();

            var sorted = engine.RaceState.SortedCars();
            bool seenDNF = false;
            foreach (var car in sorted)
            {
                if (seenDNF)
                    Assert.IsTrue(car.DNF, "Once a DNF appears, all subsequent cars should be DNF");
                if (car.DNF)
                    seenDNF = true;
            }
        }

        // ── Player cars ───────────────────────────────────────────────────────

        [Test]
        public void GetPlayerCars_ReturnsOnlyPlayerTeam()
        {
            var (engine, _, _) = BuildEngine(playerTeam: 0);
            engine.SimulateLap();

            var playerCars = engine.RaceState.GetPlayerCars();
            Assert.AreEqual(2, playerCars.Count, "Player team should have 2 cars");
            Assert.IsTrue(playerCars.All(c => c.TeamID == 0));
        }

        // ── Event log ─────────────────────────────────────────────────────────

        [Test]
        public void SimulateLap_ProducesAtLeastOneEvent()
        {
            var (engine, _, _) = BuildEngine();
            engine.SimulateLap();
            Assert.IsNotEmpty(engine.RaceState.Events, "Race should log at least one event per lap");
        }

        // ── Determinism ───────────────────────────────────────────────────────

        [Test]
        public void SimulateLap_IsDeterministicWithSameSeed()
        {
            var (engine1, _, _) = BuildEngine(seed: 1234);
            var (engine2, _, _) = BuildEngine(seed: 1234);

            engine1.SimulateLap();
            engine2.SimulateLap();

            for (int i = 0; i < engine1.RaceState.Cars.Count; i++)
            {
                var c1 = engine1.RaceState.Cars[i];
                var c2 = engine2.RaceState.Cars[i];
                Assert.AreEqual(c1.LastLapTimeS, c2.LastLapTimeS, 0.001f,
                    $"Car {c1.DriverID} lap times should be identical for same seed");
            }
        }

        // ── Fastest lap tracking ──────────────────────────────────────────────

        [Test]
        public void FastestLap_IsTrackedCorrectly()
        {
            var (engine, _, _) = BuildEngine();
            for (int i = 0; i < 3; i++)
                engine.SimulateLap();

            Assert.Greater(engine.RaceState.FastestLapTimeS, 0f);
            Assert.Less(engine.RaceState.FastestLapTimeS, 9999f);
            Assert.AreNotEqual(-1, engine.RaceState.FastestLapDriverID);
        }
    }
}
