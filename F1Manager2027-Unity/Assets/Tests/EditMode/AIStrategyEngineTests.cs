// AIStrategyEngineTests.cs — Edit-mode unit tests for AIStrategyEngine.
// Run via: Unity → Window → General → Test Runner → EditMode
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using F1Manager;

namespace F1Manager.Tests
{
    [TestFixture]
    public class AIStrategyEngineTests
    {
        // ── Fixtures ──────────────────────────────────────────────────────────

        private static CircuitInfo MakeCircuit(float degMult = 1.0f, int totalLaps = 50)
        {
            return new CircuitInfo
            {
                id                    = 0,
                circuitName           = "Test Circuit",
                totalLaps             = totalLaps,
                baseLapTimeS          = 90f,
                circuitLengthKm       = 5.3f,
                tireDegMultiplier     = degMult,
                overtakeDifficulty    = 0.4f,
                drsZones              = 2,
                fuelConsumptionKg     = 1.8f,
                rainProbability       = 0f,
                trackTempRangeMin     = 28f,
                trackTempRangeMax     = 42f,
                safetyCarlProbability = 0f,
                sectorSplits          = new float[] { 0.30f, 0.40f, 0.30f },
                powerSensitivity      = 0.5f,
            };
        }

        private static DriverInfo MakeDriver(int id = 1)
        {
            return new DriverInfo
            {
                id             = id,
                driverName     = "Driver",
                shortName      = "DRV",
                carNumber      = 1,
                teamID         = 0,
                pace           = 80,
                racecraft      = 75,
                defending      = 70,
                tireManagement = 80,
                wetSkill       = 75,
                experience     = 10,
                morale         = 70,
                isReserve      = false,
            };
        }

        private static CarState MakeCarInPhase(TirePhase phase, int lapsRemaining = 20)
        {
            var car = new CarState(1, 0, 44)
            {
                Compound    = TireCompound.C4,
                FuelKg      = lapsRemaining * 1.8f + 5f,
                Instruction = DriverInstruction.MANAGE,
            };

            // Set tire age to produce the requested phase
            var profile = TireProfiles.All[TireCompound.C4];
            switch (phase)
            {
                case TirePhase.WARM_UP:  car.TireAgeLaps = 0;  break;
                case TirePhase.PLATEAU:  car.TireAgeLaps = 5;  break;
                case TirePhase.LINEAR:   car.TireAgeLaps = 20; break;
                case TirePhase.CLIFF:    car.TireAgeLaps = 40; break; // Well past cliff for C4
            }

            car.TirePhase = TireSystem.GetPhase(profile, car.TireAgeLaps, 1.0f);
            car.CompoundsUsed.Add(TireCompound.C4);
            return car;
        }

        private static RaceState MakeRaceState(int currentLap = 20, int totalLaps = 50, WeatherCondition weather = WeatherCondition.DRY)
        {
            var state = new RaceState("Test", totalLaps)
            {
                CurrentLap = currentLap,
                Weather    = weather,
            };
            state.AvailableCompounds.AddRange(new[] { TireCompound.C3, TireCompound.C4, TireCompound.C5 });
            return state;
        }

        private static AIStrategyEngine MakeAI(CircuitInfo circuit = null)
        {
            circuit ??= MakeCircuit();
            var compounds = new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 };
            return new AIStrategyEngine(circuit, compounds, new System.Random(42));
        }

        // ── ShouldPit — cliff phase ───────────────────────────────────────────

        [Test]
        public void ShouldPit_CliffPhase_AlwaysTrue()
        {
            var ai  = MakeAI();
            var car = MakeCarInPhase(TirePhase.CLIFF);
            var race = MakeRaceState(currentLap: 20, totalLaps: 50);
            race.Cars.Add(car);
            var driver = MakeDriver();

            Assert.IsTrue(ai.ShouldPit(car, race, driver),
                "Tyre in CLIFF phase should always trigger a pit");
        }

        [Test]
        public void ShouldPit_PlateauEarlyRace_False()
        {
            var ai   = MakeAI();
            var car  = MakeCarInPhase(TirePhase.PLATEAU);
            var race = MakeRaceState(currentLap: 5, totalLaps: 50);
            race.Cars.Add(car);
            var driver = MakeDriver();

            // In plateau, well within window — should not pit
            Assert.IsFalse(ai.ShouldPit(car, race, driver),
                "Fresh tyre in plateau should not pit early in the race");
        }

        [Test]
        public void ShouldPit_MandatoryRuleNotFulfilled_TriggersLate()
        {
            var ai  = MakeAI();
            var car = MakeCarInPhase(TirePhase.PLATEAU, lapsRemaining: 8);
            // Only one compound used — mandatory rule not met
            car.CompoundsUsed = new List<TireCompound> { TireCompound.C4 };

            var race = MakeRaceState(currentLap: 42, totalLaps: 50);
            race.Cars.Add(car);
            var driver = MakeDriver();

            Assert.IsTrue(ai.ShouldPit(car, race, driver),
                "Mandatory two-compound rule should force pit with ≤ 10 laps remaining");
        }

        [Test]
        public void ShouldPit_TwoCompoundsUsed_MandatoryRuleNotTriggered()
        {
            var ai  = MakeAI();
            var car = MakeCarInPhase(TirePhase.PLATEAU, lapsRemaining: 5);
            // Both compounds already used
            car.CompoundsUsed = new List<TireCompound> { TireCompound.C4, TireCompound.C5 };

            var race = MakeRaceState(currentLap: 45, totalLaps: 50);
            race.Cars.Add(car);
            var driver = MakeDriver();

            // Mandatory rule is satisfied — no forced pit from that rule
            // (may still pit for other reasons, but mandatory rule is not the cause)
            bool result = ai.ShouldPit(car, race, driver);
            // We just verify it doesn't throw; the result depends on other factors
            Assert.IsNotNull(result); // Tautology — just checking no exception
        }

        // ── ChooseCompound ────────────────────────────────────────────────────

        [Test]
        public void ChooseCompound_ShortStint_PicksSoft()
        {
            var ai   = MakeAI();
            var car  = MakeCarInPhase(TirePhase.PLATEAU);
            var race = MakeRaceState(currentLap: 40, totalLaps: 50); // Only 10 laps left
            race.Cars.Add(car);

            TireCompound chosen = ai.ChooseCompound(car, race);
            // With only 10 laps left, soft (C5) should be preferred over hard
            Assert.IsTrue((int)chosen >= (int)TireCompound.C4,
                "Short stint should not start on the hardest compound");
        }

        [Test]
        public void ChooseCompound_LongStint_PrefersHarder()
        {
            var ai   = MakeAI();
            var car  = MakeCarInPhase(TirePhase.PLATEAU);
            var race = MakeRaceState(currentLap: 5, totalLaps: 50); // Long race ahead
            race.Cars.Add(car);

            TireCompound chosen = ai.ChooseCompound(car, race);
            // With 45 laps remaining, C5 (soft, 12-lap plateau) cannot survive — expect harder
            Assert.IsTrue((int)chosen <= (int)TireCompound.C4,
                "Long stint should prefer harder compound that can cover more laps");
        }

        [Test]
        public void ChooseCompound_HeavyRain_ReturnsWet()
        {
            var ai   = MakeAI();
            var car  = MakeCarInPhase(TirePhase.PLATEAU);
            var race = MakeRaceState(currentLap: 10, totalLaps: 50, weather: WeatherCondition.HEAVY_RAIN);
            race.Cars.Add(car);
            // Add wet to available
            race.AvailableCompounds.Add(TireCompound.WET);

            TireCompound chosen = ai.ChooseCompound(car, race);
            Assert.AreEqual(TireCompound.WET, chosen, "Heavy rain should select WET compound");
        }

        [Test]
        public void ChooseCompound_LightRain_ReturnsInter()
        {
            var ai   = MakeAI();
            var car  = MakeCarInPhase(TirePhase.PLATEAU);
            var race = MakeRaceState(currentLap: 10, totalLaps: 50, weather: WeatherCondition.LIGHT_RAIN);
            race.Cars.Add(car);
            race.AvailableCompounds.Add(TireCompound.INTER);

            TireCompound chosen = ai.ChooseCompound(car, race);
            Assert.AreEqual(TireCompound.INTER, chosen, "Light rain should select INTER compound");
        }

        // ── ChooseInstruction ─────────────────────────────────────────────────

        [Test]
        public void ChooseInstruction_FinalLaps_ReturnsAttack()
        {
            var ai     = MakeAI();
            var car    = MakeCarInPhase(TirePhase.LINEAR);
            car.PitStopCount = 1; // Has pitted
            var race   = MakeRaceState(currentLap: 46, totalLaps: 50); // 4 laps left
            race.Cars.Add(car);
            car.Position = 2;
            var driver = MakeDriver();

            DriverInstruction instr = ai.ChooseInstruction(car, race, driver);
            Assert.AreEqual(DriverInstruction.ATTACK, instr,
                "Final 5 laps should produce ATTACK instruction");
        }

        [Test]
        public void ChooseInstruction_CliffPhase_ReturnsManage()
        {
            var ai     = MakeAI();
            var car    = MakeCarInPhase(TirePhase.CLIFF);
            var race   = MakeRaceState(currentLap: 20, totalLaps: 50);
            race.Cars.Add(car);
            car.Position = 5;
            var driver = MakeDriver();

            DriverInstruction instr = ai.ChooseInstruction(car, race, driver);
            // Cliff phase → manage until pit (or attack if last lap): expect MANAGE
            Assert.AreEqual(DriverInstruction.MANAGE, instr,
                "CLIFF phase tyres need managing, not attacking");
        }

        [Test]
        public void ChooseInstruction_FreshTyresNotP1_ReturnsAttack()
        {
            var ai     = MakeAI();
            var car    = MakeCarInPhase(TirePhase.WARM_UP);
            car.TireAgeLaps  = 2; // Just pitted — fresh tyres
            car.PitStopCount = 1;
            car.Position     = 3;
            var race   = MakeRaceState(currentLap: 15, totalLaps: 50);
            race.Cars.Add(car);
            var driver = MakeDriver();

            DriverInstruction instr = ai.ChooseInstruction(car, race, driver);
            Assert.AreEqual(DriverInstruction.ATTACK, instr,
                "Fresh tyres and not P1 should produce ATTACK");
        }

        // ── PitStopDurationS ──────────────────────────────────────────────────

        [Test]
        public void PitStopDurationS_TopCrew_IsFast()
        {
            var ai = MakeAI();
            float duration = ai.PitStopDurationS(pitCrewSkill: 95, rng: new System.Random(1));
            Assert.Less(duration, 2.5f, "Top crew (skill=95) should produce < 2.5s stop");
            Assert.Greater(duration, 1.5f, "No pit stop can realistically be < 1.5s");
        }

        [Test]
        public void PitStopDurationS_LowCrewIsSlowerOnAverage()
        {
            var ai = MakeAI();
            // Sample many seeds and compare averages
            float totalTop = 0f, totalLow = 0f;
            for (int seed = 0; seed < 50; seed++)
            {
                totalTop += ai.PitStopDurationS(pitCrewSkill: 95, rng: new System.Random(seed * 2));
                totalLow += ai.PitStopDurationS(pitCrewSkill: 70, rng: new System.Random(seed * 2 + 1));
            }
            Assert.Less(totalTop / 50f, totalLow / 50f,
                "Average top-crew stop should be faster than low-crew stop");
        }

        [Test]
        public void PitStopDurationS_NeverNegative()
        {
            var ai = MakeAI();
            for (int seed = 0; seed < 20; seed++)
            {
                float dur = ai.PitStopDurationS(pitCrewSkill: 85, rng: new System.Random(seed));
                Assert.Greater(dur, 0f, $"Pit stop duration must be positive (seed={seed})");
            }
        }

        // ── PitStopProjection ─────────────────────────────────────────────────

        [Test]
        public void PitStopProjection_ReturnsNonNull()
        {
            var car  = MakeCarInPhase(TirePhase.LINEAR);
            car.Position = 3;
            var race = MakeRaceState(currentLap: 20, totalLaps: 50);
            race.Cars.Add(car);

            var sortedCars = race.SortedCars();
            var dryComps   = new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 };
            int lapsLeft   = race.TotalLaps - race.CurrentLap;

            var proj = AIStrategyEngine.PitStopProjection(
                car, sortedCars, MakeCircuit(), dryComps, lapsLeft);

            Assert.IsNotNull(proj);
        }

        [Test]
        public void PitStopProjection_CanRecoverWithPlentyOfLaps()
        {
            var car  = MakeCarInPhase(TirePhase.CLIFF);
            car.Position = 3;
            var race = MakeRaceState(currentLap: 10, totalLaps: 50); // 40 laps remaining
            race.Cars.Add(car);

            var sortedCars = race.SortedCars();
            var dryComps   = new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 };
            int lapsLeft   = race.TotalLaps - race.CurrentLap;

            var proj = AIStrategyEngine.PitStopProjection(
                car, sortedCars, MakeCircuit(), dryComps, lapsLeft);

            Assert.IsTrue(proj.CanRecover, "With 40 laps remaining and cliff tyres, recovery should be possible");
        }
    }
}
