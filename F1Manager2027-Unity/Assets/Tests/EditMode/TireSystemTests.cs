// TireSystemTests.cs — Edit-mode unit tests for TireSystem.
// Run via: Unity → Window → General → Test Runner → EditMode
using System.Collections.Generic;
using NUnit.Framework;
using F1Manager;

namespace F1Manager.Tests
{
    [TestFixture]
    public class TireSystemTests
    {
        private static TireProfile P(TireCompound c) => TireProfiles.All[c];

        // ── GetPhase ──────────────────────────────────────────────────────────

        [Test]
        public void GetPhase_FreshTire_IsWarmUp()
        {
            // C4: warmUpLaps=1 → age 0 is fresh, age 1 is still warm-up
            Assert.AreEqual(TirePhase.WARM_UP, TireSystem.GetPhase(P(TireCompound.C4), 0, 1.0f));
        }

        [Test]
        public void GetPhase_AfterWarmUp_IsPlateau()
        {
            // C4 warmUpLaps=1 → age 2 should be in plateau
            Assert.AreEqual(TirePhase.PLATEAU, TireSystem.GetPhase(P(TireCompound.C4), 2, 1.0f));
        }

        [Test]
        public void GetPhase_LateInStint_IsLinear()
        {
            // C4: warmUp=1, plateau=18 → age 20 is into linear phase
            Assert.AreEqual(TirePhase.LINEAR, TireSystem.GetPhase(P(TireCompound.C4), 20, 1.0f));
        }

        [Test]
        public void GetPhase_VeryLateInStint_IsCliff()
        {
            // C5: warmUp=1, plateau=12, linear=11 → age 25 is well past cliff
            Assert.AreEqual(TirePhase.CLIFF, TireSystem.GetPhase(P(TireCompound.C5), 25, 1.0f));
        }

        [Test]
        public void GetPhase_HighDegMult_ShortensWindow()
        {
            // With degMult=2.0 the cliff should arrive sooner
            var phaseNormal = TireSystem.GetPhase(P(TireCompound.C4), 30, 1.0f);
            var phaseHigh   = TireSystem.GetPhase(P(TireCompound.C4), 30, 3.0f);
            // At age 30 with 3x deg, should be at least as worn as 1x
            Assert.IsTrue(phaseHigh >= phaseNormal,
                "Higher deg multiplier should produce equal or more worn phase");
        }

        // ── DegPenaltyS ───────────────────────────────────────────────────────

        [Test]
        public void DegPenaltyS_FreshTire_HasWarmUpPenalty()
        {
            float penalty = TireSystem.DegPenaltyS(P(TireCompound.C4), 1, 1.0f);
            Assert.Greater(penalty, 0f, "Lap 1 on fresh tyre should have warm-up penalty");
        }

        [Test]
        public void DegPenaltyS_PlateauLap_IsNearZero()
        {
            float penalty = TireSystem.DegPenaltyS(P(TireCompound.C4), 5, 1.0f);
            Assert.Less(penalty, 0.5f, "Plateau lap should have minimal degradation");
        }

        [Test]
        public void DegPenaltyS_CliffIsHigherThanPlateau()
        {
            float plateau = TireSystem.DegPenaltyS(P(TireCompound.C5), 5,  1.0f);
            float cliff   = TireSystem.DegPenaltyS(P(TireCompound.C5), 30, 1.0f);
            Assert.Greater(cliff, plateau, "Cliff penalty should exceed plateau penalty");
        }

        [Test]
        public void DegPenaltyS_HigherDegMult_ScalesPenalty()
        {
            float normal = TireSystem.DegPenaltyS(P(TireCompound.C4), 25, 1.0f);
            float high   = TireSystem.DegPenaltyS(P(TireCompound.C4), 25, 2.0f);
            Assert.Greater(high, normal, "Higher deg multiplier should increase penalty");
        }

        // ── WindowRemaining ───────────────────────────────────────────────────

        [Test]
        public void WindowRemaining_FreshSoft_IsPositive()
        {
            int remaining = TireSystem.WindowRemaining(P(TireCompound.C5), 0, 1.0f);
            Assert.Greater(remaining, 0, "Fresh soft should have positive window remaining");
        }

        [Test]
        public void WindowRemaining_PastCliff_IsZero()
        {
            // WindowRemaining clamps to 0 (not negative) per the implementation
            int remaining = TireSystem.WindowRemaining(P(TireCompound.C5), 100, 1.0f);
            Assert.AreEqual(0, remaining, "Past cliff should return 0 remaining");
        }

        [Test]
        public void WindowRemaining_HardCompoundMoreThanSoft()
        {
            int hardWin = TireSystem.WindowRemaining(P(TireCompound.C2), 0, 1.0f);
            int softWin = TireSystem.WindowRemaining(P(TireCompound.C5), 0, 1.0f);
            Assert.Greater(hardWin, softWin, "Hard compound should last longer than soft");
        }

        // ── BestCompoundForStint ──────────────────────────────────────────────

        [Test]
        public void BestCompoundForStint_ShortStint_PrefersSoft()
        {
            var compounds = new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 };
            TireCompound best = TireSystem.BestCompoundForStint(compounds, 8, 1.0f);
            Assert.AreEqual(TireCompound.C5, best, "Short stint should prefer softest compound");
        }

        [Test]
        public void BestCompoundForStint_LongStint_PrefersHard()
        {
            var compounds = new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 };
            TireCompound best = TireSystem.BestCompoundForStint(compounds, 40, 1.0f);
            Assert.AreEqual(TireCompound.C3, best, "Long stint should prefer hardest compound");
        }

        // ── GripAdvantageS extension ──────────────────────────────────────────

        [Test]
        public void GripAdvantage_SoftIsFasterThanHard()
        {
            float soft = TireCompound.C5.GripAdvantageS();
            float hard = TireCompound.C2.GripAdvantageS();
            Assert.Less(soft, hard, "Soft should have negative grip advantage (faster lap time)");
        }

        [Test]
        public void GripAdvantage_C4_IsZeroBaseline()
        {
            Assert.AreEqual(0f, TireCompound.C4.GripAdvantageS(), 0.001f,
                "C4 (medium) is the zero baseline — its GripAdvantageS should be 0");
        }

        // ── IsDry extension ───────────────────────────────────────────────────

        [Test]
        public void IsDry_SlickCompounds_ReturnTrue()
        {
            Assert.IsTrue(TireCompound.C3.IsDry());
            Assert.IsTrue(TireCompound.C5.IsDry());
        }

        [Test]
        public void IsDry_WetCompounds_ReturnFalse()
        {
            Assert.IsFalse(TireCompound.INTER.IsDry());
            Assert.IsFalse(TireCompound.WET.IsDry());
        }
    }
}
