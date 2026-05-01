// OvertakeSystemTests.cs — Edit-mode unit tests for OvertakeSystem.
// Run via: Unity → Window → General → Test Runner → EditMode
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using F1Manager;

namespace F1Manager.Tests
{
    [TestFixture]
    public class OvertakeSystemTests
    {
        // ── Fixtures ──────────────────────────────────────────────────────────

        private static CircuitInfo MakeCircuit(float overtakeDiff = 0.3f)
        {
            return new CircuitInfo
            {
                id                    = 0,
                circuitName           = "Test Circuit",
                totalLaps             = 50,
                baseLapTimeS          = 90f,
                circuitLengthKm       = 5.3f,
                tireDegMultiplier     = 1.0f,
                overtakeDifficulty    = overtakeDiff,
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

        private static DriverInfo MakeDriver(int id, int racecraft = 75, int defending = 70)
        {
            return new DriverInfo
            {
                id             = id,
                driverName     = $"Driver{id}",
                shortName      = $"D{id}",
                carNumber      = id,
                teamID         = id / 2,
                pace           = 80,
                racecraft      = racecraft,
                defending      = defending,
                tireManagement = 80,
                wetSkill       = 75,
                experience     = 10,
                morale         = 70,
                isReserve      = false,
            };
        }

        private static CarState MakeCar(int driverID, int teamID, int position,
            float gapToAhead = 0.5f, float lastLap = 90f, int tireAge = 10,
            DriverInstruction instruction = DriverInstruction.MANAGE)
        {
            return new CarState(driverID, teamID, driverID)
            {
                Position       = position,
                GapToAheadS    = gapToAhead,
                LastLapTimeS   = lastLap,
                TireAgeLaps    = tireAge,
                Instruction    = instruction,
                LapsCompleted  = 5,
                TirePhase      = TirePhase.LINEAR,
                Compound       = TireCompound.C4,
            };
        }

        private static RaceState MakeRaceWith(params CarState[] cars)
        {
            var state = new RaceState("Test", 50) { CurrentLap = 5 };
            state.Cars.AddRange(cars);
            // Set gaps and positions
            for (int i = 0; i < state.Cars.Count; i++)
                state.Cars[i].Position = i + 1;
            return state;
        }

        private static Dictionary<int, DriverInfo> ToDrv(params DriverInfo[] drivers)
        {
            var d = new Dictionary<int, DriverInfo>();
            foreach (var dr in drivers) d[dr.id] = dr;
            return d;
        }

        // ── DefenderAggression ────────────────────────────────────────────────

        [Test]
        public void DefenderAggression_DefendInstruction_IsHigh()
        {
            var car    = MakeCar(1, 0, 2, instruction: DriverInstruction.DEFEND);
            var driver = MakeDriver(1);
            float agg  = OvertakeSystem.DefenderAggression(car, driver);
            Assert.GreaterOrEqual(agg, 0.70f, "DEFEND instruction should produce high aggression (≥ 0.70)");
        }

        [Test]
        public void DefenderAggression_AttackInstruction_IsLow()
        {
            var car    = MakeCar(1, 0, 2, instruction: DriverInstruction.ATTACK);
            var driver = MakeDriver(1);
            float agg  = OvertakeSystem.DefenderAggression(car, driver);
            Assert.Less(agg, 0.40f, "ATTACK instruction should produce low defender aggression (< 0.40)");
        }

        [Test]
        public void DefenderAggression_ManageInstruction_IsMid()
        {
            var car    = MakeCar(1, 0, 2, instruction: DriverInstruction.MANAGE);
            var driver = MakeDriver(1);
            float agg  = OvertakeSystem.DefenderAggression(car, driver);
            Assert.Greater(agg, 0.10f, "MANAGE aggression should be greater than 0.10");
            Assert.Less(agg,    0.70f, "MANAGE aggression should be less than 0.70");
        }

        [Test]
        public void DefenderAggression_IsNeverNegative()
        {
            foreach (DriverInstruction instr in System.Enum.GetValues(typeof(DriverInstruction)))
            {
                var car    = MakeCar(1, 0, 2, instruction: instr);
                var driver = MakeDriver(1);
                float agg  = OvertakeSystem.DefenderAggression(car, driver);
                Assert.GreaterOrEqual(agg, 0f, $"Aggression for {instr} must be non-negative");
            }
        }

        // ── Attempt filter ────────────────────────────────────────────────────

        [Test]
        public void ResolveOvertakes_NoCarsClose_ReturnsEmpty()
        {
            // Both cars far apart — no overtake attempt
            var leader   = MakeCar(1, 0, 1, lastLap: 90.0f, tireAge: 5);
            var follower = MakeCar(2, 1, 2, gapToAhead: 5.0f, lastLap: 90.0f, tireAge: 5);
            var state    = MakeRaceWith(leader, follower);
            var drivers  = ToDrv(MakeDriver(1), MakeDriver(2));
            var circuit  = MakeCircuit();

            var results = OvertakeSystem.ResolveOvertakes(state, drivers, circuit, new System.Random(42));
            Assert.IsEmpty(results, "Cars 5s apart should not attempt overtake");
        }

        [Test]
        public void ResolveOvertakes_CloseButNoPaceAdvantage_NoAttempt()
        {
            // Close gap but equal pace — pace filter rejects attempt
            var leader   = MakeCar(1, 0, 1, lastLap: 90.0f, tireAge: 10);
            var follower = MakeCar(2, 1, 2, gapToAhead: 0.5f, lastLap: 90.0f, tireAge: 10);
            var state    = MakeRaceWith(leader, follower);
            var drivers  = ToDrv(MakeDriver(1), MakeDriver(2));
            var circuit  = MakeCircuit();

            var results = OvertakeSystem.ResolveOvertakes(state, drivers, circuit, new System.Random(42));
            // No attempt because pace advantage is 0.0 (< 0.15 threshold)
            Assert.IsEmpty(results, "Equal pace cars should not attempt overtake");
        }

        [Test]
        public void ResolveOvertakes_CloseWithPaceAdvantage_AttemptMade()
        {
            // Attacker is significantly faster — attempt should be made
            var leader   = MakeCar(1, 0, 1, lastLap: 91.0f, tireAge: 25); // Degraded tyres
            var follower = MakeCar(2, 1, 2, gapToAhead: 0.5f, lastLap: 90.5f, tireAge: 3); // Fresh tyres
            var state    = MakeRaceWith(leader, follower);
            var drivers  = ToDrv(MakeDriver(1, racecraft: 70, defending: 65),
                                  MakeDriver(2, racecraft: 85, defending: 80));
            var circuit  = MakeCircuit(overtakeDiff: 0.2f); // Easy circuit

            // Run many seeds to ensure at least one attempt succeeds
            bool anyAttempt = false;
            for (int seed = 0; seed < 30 && !anyAttempt; seed++)
            {
                var results = OvertakeSystem.ResolveOvertakes(state, drivers, circuit, new System.Random(seed));
                if (results.Count > 0) anyAttempt = true;
            }
            Assert.IsTrue(anyAttempt, "Faster car with pace advantage should attempt overtake");
        }

        // ── DRS ───────────────────────────────────────────────────────────────

        [Test]
        public void DRS_ActiveInDryRace_BoostsLogit()
        {
            // Test that DRS flag is set when conditions are met
            var leader   = MakeCar(1, 0, 1, lastLap: 91.0f, tireAge: 20);
            var follower = MakeCar(2, 1, 2, gapToAhead: 0.8f, lastLap: 90.5f, tireAge: 5);
            follower.LapsCompleted = 10;
            leader.LapsCompleted   = 10;
            var state    = MakeRaceWith(leader, follower);
            state.Weather   = WeatherCondition.DRY;
            state.CurrentLap = 10;
            state.TrackTempC = 35f;

            var drivers = ToDrv(MakeDriver(1, racecraft: 70), MakeDriver(2, racecraft: 85));
            var circuit = MakeCircuit(overtakeDiff: 0.2f);

            // With DRS active (circuit has DRS zones, gap < 1s, dry conditions),
            // more overtakes should succeed than without DRS
            int successWithDRS = 0;
            for (int seed = 0; seed < 50; seed++)
            {
                var results = OvertakeSystem.ResolveOvertakes(state, drivers, circuit, new System.Random(seed));
                successWithDRS += results.Count(r => r.Success);
            }

            // Without DRS zones
            var noDrsCircuit = MakeCircuit(overtakeDiff: 0.2f);
            noDrsCircuit.drsZones = 0;
            int successNoDRS = 0;
            for (int seed = 0; seed < 50; seed++)
            {
                var results = OvertakeSystem.ResolveOvertakes(state, drivers, noDrsCircuit, new System.Random(seed));
                successNoDRS += results.Count(r => r.Success);
            }

            Assert.GreaterOrEqual(successWithDRS, successNoDRS,
                "DRS should result in equal or more overtakes than without DRS");
        }

        // ── Monaco — overtake-proof circuit ──────────────────────────────────

        [Test]
        public void Monaco_HighOvertakeDifficulty_VeryFewOvertakes()
        {
            var leader   = MakeCar(1, 0, 1, lastLap: 92.0f, tireAge: 30);
            var follower = MakeCar(2, 1, 2, gapToAhead: 0.3f, lastLap: 90.5f, tireAge: 2);
            var state    = MakeRaceWith(leader, follower);
            var drivers  = ToDrv(MakeDriver(1, racecraft: 60, defending: 90),
                                  MakeDriver(2, racecraft: 98, defending: 50));
            var monaco   = MakeCircuit(overtakeDiff: 1.0f); // Maximum difficulty
            monaco.drsZones = 0;

            int successes = 0;
            for (int seed = 0; seed < 100; seed++)
            {
                var results = OvertakeSystem.ResolveOvertakes(state, drivers, monaco, new System.Random(seed));
                successes += results.Count(r => r.Success);
            }

            // At maximum difficulty, very few overtakes should succeed
            Assert.Less(successes, 30, "Monaco-type circuit should produce fewer than 30% successful overtakes");
        }

        // ── Probability cap ───────────────────────────────────────────────────

        [Test]
        public void OvertakeSuccessProbability_NeverExceedsCap()
        {
            // Even with maximum advantage, P must not exceed P_CAP
            float p = OvertakeSystem.ComputeSuccessProbability(
                skillDelta:          100f,   // Huge skill advantage
                paceDelta:           10f,    // Huge pace advantage
                tireAgeDelta:        50f,    // Huge tire advantage
                drsActive:           true,
                trackDifficulty:     0f,     // Easiest possible circuit
                defenderAggression:  0f);    // No defense

            Assert.LessOrEqual(p, OvertakeSystem.P_CAP + 0.001f,
                $"Success probability must not exceed P_CAP ({OvertakeSystem.P_CAP})");
        }

        [Test]
        public void OvertakeSuccessProbability_ZeroWhenHugeDisadvantage()
        {
            // With maximum disadvantage, P should be very low (near 0)
            float p = OvertakeSystem.ComputeSuccessProbability(
                skillDelta:         -100f,   // Huge skill disadvantage
                paceDelta:          -10f,    // Huge pace disadvantage
                tireAgeDelta:       -50f,    // Much older tires
                drsActive:           false,
                trackDifficulty:     1.0f,   // Hardest track
                defenderAggression:  1.0f);  // Max defense

            Assert.Less(p, 0.10f,
                "Overtake probability with maximum disadvantage should be near zero");
        }

        // ── Result structure ──────────────────────────────────────────────────

        [Test]
        public void OvertakeResult_SuccessfulOvertake_SwapsPositions()
        {
            // Run many seeds and verify that when an overtake succeeds, positions are swapped
            var leader   = MakeCar(1, 0, 1, lastLap: 91.5f, tireAge: 25);
            var follower = MakeCar(2, 1, 2, gapToAhead: 0.4f, lastLap: 90.0f, tireAge: 2);
            var drivers  = ToDrv(MakeDriver(1, racecraft: 60, defending: 55),
                                  MakeDriver(2, racecraft: 95, defending: 80));
            var circuit  = MakeCircuit(0.15f);

            bool foundSuccess = false;
            for (int seed = 0; seed < 200 && !foundSuccess; seed++)
            {
                var state = MakeRaceWith(
                    MakeCar(1, 0, 1, lastLap: 91.5f, tireAge: 25),
                    MakeCar(2, 1, 2, gapToAhead: 0.4f, lastLap: 90.0f, tireAge: 2));
                state.Cars[0].LapsCompleted = 5;
                state.Cars[1].LapsCompleted = 5;

                var results = OvertakeSystem.ResolveOvertakes(state, drivers, circuit, new System.Random(seed));
                var success = results.FirstOrDefault(r => r.Success);
                if (success != null)
                {
                    foundSuccess = true;
                    // Attacker should be in P1 after successful overtake
                    Assert.AreEqual(1, success.NewPositionAttacker,
                        "Successful overtaker should be in P1");
                }
            }

            // As long as the system is working, a success should occur in 200 tries
            Assert.IsTrue(foundSuccess, "A successful overtake should occur in 200 seeds with pace advantage");
        }

        // ── Static helpers ────────────────────────────────────────────────────

        [Test]
        public void ComputeSuccessProbability_IsBetweenZeroAndCap()
        {
            var rng = new System.Random(0);
            for (int i = 0; i < 50; i++)
            {
                float skill = (float)(rng.NextDouble() * 40 - 20);
                float pace  = (float)(rng.NextDouble() * 6 - 3);
                float tire  = (float)(rng.NextDouble() * 20 - 10);
                bool  drs   = rng.NextDouble() > 0.5;
                float diff  = (float)rng.NextDouble();
                float agg   = (float)rng.NextDouble();

                float p = OvertakeSystem.ComputeSuccessProbability(skill, pace, tire, drs, diff, agg);
                Assert.GreaterOrEqual(p, 0f, "P must be ≥ 0");
                Assert.LessOrEqual(p, OvertakeSystem.P_CAP + 0.001f, "P must be ≤ P_CAP");
            }
        }
    }
}
