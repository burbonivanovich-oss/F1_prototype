// CarDevelopmentSystem.cs — Car upgrade tree with two-phase research/development workflow,
// ATR (Aerodynamic Testing Resources) budget, learning-curve discounts, and breakthrough rolls.
// Pure C#, no Unity dependencies.
using System;
using System.Collections.Generic;
using System.Linq;

namespace F1Manager
{
    // ─────────────────────────────────────────────────────────────────────────
    // Upgrade areas (9 areas matching the 8 R&D centres + sub-specialisms)
    // ─────────────────────────────────────────────────────────────────────────

    public enum UpgradeArea
    {
        POWER_UNIT,     // Engine power, reliability
        AERO_FRONT,     // Front wing, DRS   — requires Research (CFD + wind tunnel)
        AERO_REAR,      // Rear wing          — requires Research
        AERO_FLOOR,     // Floor / ground effect — requires Research
        CHASSIS,        // Chassis stiffness, weight
        SUSPENSION,     // Geometry, springs, dampers
        BRAKES,         // Braking system, heat management
        GEARBOX,        // Transmission reliability
        COOLING,        // Thermal management, circuit heat performance
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Upgrade phase state machine
    //   None → Research → ResearchComplete → Development → Complete
    //   (Nodes with ResearchWeeks == 0 skip straight to Development)
    // ─────────────────────────────────────────────────────────────────────────

    public enum UpgradePhase
    {
        None,             // Not started
        Research,         // CFD/WT work in progress
        ResearchComplete, // Research done; player must commit tokens+R&D to manufacture
        Development,      // Manufacturing and bench testing in progress
        Complete,         // Deployed on car
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Upgrade node
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// One node in the upgrade tree.
    /// Aero nodes have a research phase (CFD + wind-tunnel); all others skip to development.
    /// </summary>
    public class UpgradeNode
    {
        public string      ID;
        public UpgradeArea Area;
        public int         Tier;

        // ── Development cost (manufacturing phase) ────────────────────────────
        public int   TokenCost;
        public int   RDPointsCost;

        // ── Research phase (Aero nodes only; 0 = skip) ───────────────────────
        public int ResearchWeeks;          // Weeks of CFD/WT work
        public int ResearchCFDCost;        // CFD units consumed
        public int ResearchWindTunnelHours; // Wind-tunnel hours consumed

        // ── Development phase ─────────────────────────────────────────────────
        public int   DevelopmentWeeks;
        public float SuccessProbability;

        // ── Reward ────────────────────────────────────────────────────────────
        public float CarPerformanceGain;
        public float PowerUnitGain;
        public float ChassisGain;
        public float ReliabilityGain;

        // ── Tree dependency ───────────────────────────────────────────────────
        public string RequiredUpgradeID;

        // ── Runtime state ─────────────────────────────────────────────────────
        public bool        IsUnlocked         = false;
        public UpgradePhase Phase             = UpgradePhase.None;
        public int         PhaseWeeksRemaining;
        public float       LearningDiscount;   // Applied at manufacture start (0–0.30)
        public bool        WasBreakthrough;    // True if 2× bonus was applied

        // ── Legacy compatibility aliases ──────────────────────────────────────
        public bool IsInDevelopment  => Phase == UpgradePhase.Development;
        public bool IsInResearch     => Phase == UpgradePhase.Research;
        public bool IsResearchDone   => Phase == UpgradePhase.ResearchComplete;
        public bool IsComplete       => Phase == UpgradePhase.Complete;
        public int  WeeksRemaining   => PhaseWeeksRemaining;
        public int  WeeksToDevelop   => DevelopmentWeeks;

        public float TotalImpact =>
            CarPerformanceGain + PowerUnitGain * 0.5f + ChassisGain * 0.5f + ReliabilityGain * 0.2f;

        public string Describe()
        {
            string phase = Phase switch
            {
                UpgradePhase.Research         => $"[RES {PhaseWeeksRemaining}w]",
                UpgradePhase.ResearchComplete => "[READY TO BUILD]",
                UpgradePhase.Development      => $"[DEV {PhaseWeeksRemaining}w]",
                UpgradePhase.Complete         => WasBreakthrough ? "[DONE ⚡BREAKTHROUGH]" : "[DONE]",
                _                             => "",
            };
            return $"{ID} | {Area} T{Tier} | {TokenCost}T/{RDPointsCost}R | "
                 + $"{SuccessProbability * 100:F0}% | +{TotalImpact:F1} {phase}";
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Development state per team
    // ─────────────────────────────────────────────────────────────────────────

    public class TeamDevelopmentState
    {
        public int TeamID;

        // ── Resource pools ────────────────────────────────────────────────────
        public int   Tokens;
        public int   RDPoints;
        public int   RDPointsPerWeek;
        public float BudgetAllocation; // 0–1; scales R&D accrual

        // ── ATR — Aerodynamic Testing Resources (reset each season) ───────────
        public int WindTunnelHoursRemaining;  // Max 56–80 hrs/yr by team tier
        public int CFDUnitsRemaining;          // Max 1120–1840 units/yr by team tier

        // ── Applied performance deltas ────────────────────────────────────────
        public float CarPerformanceDelta;
        public float PowerUnitDelta;
        public float ChassisDelta;
        public float ReliabilityDelta;

        // ── Tree state ────────────────────────────────────────────────────────
        public List<UpgradeNode> Tree         = new List<UpgradeNode>();
        public List<string>      CompletedIDs = new List<string>();
        public List<string>      ActiveIDs    = new List<string>(); // in research OR development

        public TeamDevelopmentState(int teamID, int initialTokens, int rdPerWeek,
                                    int wtHours, int cfdUnits)
        {
            TeamID                   = teamID;
            Tokens                   = initialTokens;
            RDPointsPerWeek          = rdPerWeek;
            WindTunnelHoursRemaining = wtHours;
            CFDUnitsRemaining        = cfdUnits;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Development result (per-week tick)
    // ─────────────────────────────────────────────────────────────────────────

    public class DevelopmentTickResult
    {
        public List<string> CompletedUpgrades  = new List<string>(); // Development finished, applied
        public List<string> FailedUpgrades     = new List<string>(); // Failed success roll
        public List<string> ResearchCompleted  = new List<string>(); // Research done, ready to build
        public List<string> Breakthroughs      = new List<string>(); // Got 2× performance bonus
        public int          RDPointsEarned;
        public string       Summary;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CarDevelopmentSystem
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Manages the two-phase upgrade system for all 10 teams.
    ///
    /// Aero upgrades: player calls StartDevelopment() → enters Research (uses ATR budget),
    /// then when research completes they call StartDevelopment() again to begin manufacturing
    /// (uses tokens + R&D points). Learning-curve discounts reduce cost for subsequent
    /// iterations of the same area. Breakthroughs (10% chance) double the performance gain.
    ///
    /// Non-aero upgrades skip the research phase entirely.
    /// </summary>
    public class CarDevelopmentSystem
    {
        private readonly Dictionary<int, TeamDevelopmentState> _states
            = new Dictionary<int, TeamDevelopmentState>();

        private readonly System.Random _rng;

        private const float BreakthroughChance = 0.10f;

        // ── Token / R&D rate tiers ────────────────────────────────────────────

        private static readonly Dictionary<string, int> _tierTokens
            = new Dictionary<string, int> { { "top", 48 }, { "mid", 36 }, { "lower", 28 } };

        private static readonly Dictionary<string, int> _tierRDRates
            = new Dictionary<string, int> { { "top", 22 }, { "mid", 14 }, { "lower", 8 } };

        // ATR budgets per tier (FIA sliding scale: leaders get less WT time)
        private static readonly Dictionary<string, (int wt, int cfd)> _tierATR
            = new Dictionary<string, (int, int)>
            {
                { "top",   (56, 1120) },   // championship leaders restricted most
                { "mid",   (68, 1480) },
                { "lower", (80, 1840) },   // back-of-grid teams get the most WT/CFD
            };

        // ─────────────────────────────────────────────────────────────────────

        public CarDevelopmentSystem(System.Random rng = null)
        {
            _rng = rng ?? new System.Random();
            GameDataFactory.Initialize();
            InitialiseAllTeams();
        }

        // ── Initialisation ────────────────────────────────────────────────────

        private void InitialiseAllTeams()
        {
            foreach (var team in GameDataFactory.Teams)
            {
                string tier = team.carPerformance >= 90 ? "top"
                            : team.carPerformance >= 83 ? "mid"
                            : "lower";

                var (wt, cfd) = _tierATR[tier];
                var state = new TeamDevelopmentState(
                    team.id, _tierTokens[tier], _tierRDRates[tier], wt, cfd);

                state.Tree = BuildUpgradeTree(team);
                RefreshUnlocks(state);
                _states[team.id] = state;
            }
        }

        public TeamDevelopmentState GetState(int teamID)
            => _states.TryGetValue(teamID, out var s) ? s : null;

        // ── Upgrade tree factory ──────────────────────────────────────────────

        private static List<UpgradeNode> BuildUpgradeTree(TeamInfo team)
        {
            var n = new List<UpgradeNode>();

            // ── POWER UNIT — engine power (3 tiers) ──────────────────────────
            n.Add(new UpgradeNode {
                ID = "PU_T1", Area = UpgradeArea.POWER_UNIT, Tier = 1,
                TokenCost = 4, RDPointsCost = 60, DevelopmentWeeks = 3,
                SuccessProbability = 0.90f, PowerUnitGain = 1.0f, CarPerformanceGain = 0.3f,
            });
            n.Add(new UpgradeNode {
                ID = "PU_T2", Area = UpgradeArea.POWER_UNIT, Tier = 2,
                TokenCost = 6, RDPointsCost = 100, DevelopmentWeeks = 4,
                SuccessProbability = 0.80f, PowerUnitGain = 1.5f, CarPerformanceGain = 0.5f,
                RequiredUpgradeID = "PU_T1",
            });
            n.Add(new UpgradeNode {
                ID = "PU_T3", Area = UpgradeArea.POWER_UNIT, Tier = 3,
                TokenCost = 8, RDPointsCost = 160, DevelopmentWeeks = 6,
                SuccessProbability = 0.65f, PowerUnitGain = 2.0f, CarPerformanceGain = 0.8f,
                RequiredUpgradeID = "PU_T2",
            });

            // ── POWER UNIT — ERS efficiency (parallel path, no PU dependency) ─
            n.Add(new UpgradeNode {
                ID = "ERS_T1", Area = UpgradeArea.POWER_UNIT, Tier = 1,
                TokenCost = 3, RDPointsCost = 50, DevelopmentWeeks = 2,
                SuccessProbability = 0.88f, PowerUnitGain = 0.8f, ReliabilityGain = 0.5f,
            });
            n.Add(new UpgradeNode {
                ID = "ERS_T2", Area = UpgradeArea.POWER_UNIT, Tier = 2,
                TokenCost = 5, RDPointsCost = 90, DevelopmentWeeks = 3,
                SuccessProbability = 0.78f, PowerUnitGain = 1.2f, ReliabilityGain = 1.0f,
                RequiredUpgradeID = "ERS_T1",
            });

            // ── AERO FRONT WING — 3 tiers (research required) ─────────────────
            n.Add(new UpgradeNode {
                ID = "AF_T1", Area = UpgradeArea.AERO_FRONT, Tier = 1,
                ResearchWeeks = 3, ResearchCFDCost = 80, ResearchWindTunnelHours = 4,
                TokenCost = 3, RDPointsCost = 50, DevelopmentWeeks = 2,
                SuccessProbability = 0.92f, ChassisGain = 0.8f, CarPerformanceGain = 0.4f,
            });
            n.Add(new UpgradeNode {
                ID = "AF_T2", Area = UpgradeArea.AERO_FRONT, Tier = 2,
                ResearchWeeks = 4, ResearchCFDCost = 120, ResearchWindTunnelHours = 6,
                TokenCost = 5, RDPointsCost = 90, DevelopmentWeeks = 3,
                SuccessProbability = 0.80f, ChassisGain = 1.2f, CarPerformanceGain = 0.6f,
                RequiredUpgradeID = "AF_T1",
            });
            n.Add(new UpgradeNode {
                ID = "AF_T3", Area = UpgradeArea.AERO_FRONT, Tier = 3,
                ResearchWeeks = 5, ResearchCFDCost = 200, ResearchWindTunnelHours = 10,
                TokenCost = 7, RDPointsCost = 140, DevelopmentWeeks = 5,
                SuccessProbability = 0.70f, ChassisGain = 1.8f, CarPerformanceGain = 0.9f,
                RequiredUpgradeID = "AF_T2",
            });

            // ── AERO REAR WING — 2 tiers (research required) ──────────────────
            n.Add(new UpgradeNode {
                ID = "AR_T1", Area = UpgradeArea.AERO_REAR, Tier = 1,
                ResearchWeeks = 3, ResearchCFDCost = 80, ResearchWindTunnelHours = 4,
                TokenCost = 3, RDPointsCost = 50, DevelopmentWeeks = 2,
                SuccessProbability = 0.90f, ChassisGain = 0.6f, CarPerformanceGain = 0.3f,
            });
            n.Add(new UpgradeNode {
                ID = "AR_T2", Area = UpgradeArea.AERO_REAR, Tier = 2,
                ResearchWeeks = 4, ResearchCFDCost = 120, ResearchWindTunnelHours = 6,
                TokenCost = 5, RDPointsCost = 85, DevelopmentWeeks = 3,
                SuccessProbability = 0.78f, ChassisGain = 1.0f, CarPerformanceGain = 0.5f,
                RequiredUpgradeID = "AR_T1",
            });

            // ── AERO FLOOR — ground effect / underfloor (research required) ───
            n.Add(new UpgradeNode {
                ID = "FL_T1", Area = UpgradeArea.AERO_FLOOR, Tier = 1,
                ResearchWeeks = 4, ResearchCFDCost = 100, ResearchWindTunnelHours = 5,
                TokenCost = 4, RDPointsCost = 70, DevelopmentWeeks = 3,
                SuccessProbability = 0.85f, ChassisGain = 1.0f, CarPerformanceGain = 0.5f,
            });
            n.Add(new UpgradeNode {
                ID = "FL_T2", Area = UpgradeArea.AERO_FLOOR, Tier = 2,
                ResearchWeeks = 5, ResearchCFDCost = 180, ResearchWindTunnelHours = 9,
                TokenCost = 7, RDPointsCost = 130, DevelopmentWeeks = 5,
                SuccessProbability = 0.73f, ChassisGain = 1.6f, CarPerformanceGain = 0.8f,
                RequiredUpgradeID = "FL_T1",
            });

            // ── CHASSIS — 2 tiers ─────────────────────────────────────────────
            n.Add(new UpgradeNode {
                ID = "CH_T1", Area = UpgradeArea.CHASSIS, Tier = 1,
                TokenCost = 5, RDPointsCost = 80, DevelopmentWeeks = 4,
                SuccessProbability = 0.85f, CarPerformanceGain = 1.0f, ChassisGain = 0.5f,
            });
            n.Add(new UpgradeNode {
                ID = "CH_T2", Area = UpgradeArea.CHASSIS, Tier = 2,
                TokenCost = 8, RDPointsCost = 130, DevelopmentWeeks = 5,
                SuccessProbability = 0.72f, CarPerformanceGain = 1.5f, ChassisGain = 1.0f,
                RequiredUpgradeID = "CH_T1",
            });

            // ── SUSPENSION — 2 tiers ──────────────────────────────────────────
            n.Add(new UpgradeNode {
                ID = "SUS_T1", Area = UpgradeArea.SUSPENSION, Tier = 1,
                TokenCost = 3, RDPointsCost = 55, DevelopmentWeeks = 2,
                SuccessProbability = 0.90f, CarPerformanceGain = 0.4f, ReliabilityGain = 1.0f,
            });
            n.Add(new UpgradeNode {
                ID = "SUS_T2", Area = UpgradeArea.SUSPENSION, Tier = 2,
                TokenCost = 5, RDPointsCost = 90, DevelopmentWeeks = 3,
                SuccessProbability = 0.80f, CarPerformanceGain = 0.6f, ReliabilityGain = 1.5f,
                RequiredUpgradeID = "SUS_T1",
            });

            // ── BRAKES — 2 tiers (new) ────────────────────────────────────────
            n.Add(new UpgradeNode {
                ID = "BR_T1", Area = UpgradeArea.BRAKES, Tier = 1,
                TokenCost = 2, RDPointsCost = 40, DevelopmentWeeks = 2,
                SuccessProbability = 0.91f, CarPerformanceGain = 0.2f, ReliabilityGain = 1.5f,
            });
            n.Add(new UpgradeNode {
                ID = "BR_T2", Area = UpgradeArea.BRAKES, Tier = 2,
                TokenCost = 4, RDPointsCost = 75, DevelopmentWeeks = 3,
                SuccessProbability = 0.80f, CarPerformanceGain = 0.4f, ReliabilityGain = 2.0f,
                RequiredUpgradeID = "BR_T1",
            });

            // ── GEARBOX — 2 tiers ─────────────────────────────────────────────
            n.Add(new UpgradeNode {
                ID = "GB_T1", Area = UpgradeArea.GEARBOX, Tier = 1,
                TokenCost = 2, RDPointsCost = 40, DevelopmentWeeks = 2,
                SuccessProbability = 0.92f, ReliabilityGain = 2.0f, CarPerformanceGain = 0.2f,
            });
            n.Add(new UpgradeNode {
                ID = "GB_T2", Area = UpgradeArea.GEARBOX, Tier = 2,
                TokenCost = 4, RDPointsCost = 70, DevelopmentWeeks = 3,
                SuccessProbability = 0.82f, ReliabilityGain = 2.5f, CarPerformanceGain = 0.3f,
                RequiredUpgradeID = "GB_T1",
            });

            // ── COOLING — 1 tier (reliability focus) ──────────────────────────
            n.Add(new UpgradeNode {
                ID = "COOL_T1", Area = UpgradeArea.COOLING, Tier = 1,
                TokenCost = 2, RDPointsCost = 35, DevelopmentWeeks = 2,
                SuccessProbability = 0.93f, ReliabilityGain = 3.0f, CarPerformanceGain = 0.1f,
            });

            return n;
        }

        // ── Player actions ────────────────────────────────────────────────────

        /// <summary>
        /// Unified entry point to advance an upgrade.
        ///
        /// Aero nodes (Phase == None): starts Research, consuming ATR budget.
        /// Aero nodes (Phase == ResearchComplete): starts Development, consuming tokens + R&D.
        /// Non-aero nodes (Phase == None): starts Development directly.
        ///
        /// Returns true on success; errorMessage describes the failure reason.
        /// </summary>
        public bool StartDevelopment(int teamID, string upgradeID, out string errorMessage)
        {
            errorMessage = null;
            var state = GetState(teamID);
            if (state == null)              { errorMessage = "Invalid team.";          return false; }
            var node = state.Tree.FirstOrDefault(n => n.ID == upgradeID);
            if (node == null)               { errorMessage = "Upgrade not found.";     return false; }
            if (!node.IsUnlocked)           { errorMessage = "Prerequisites not met."; return false; }
            if (node.Phase == UpgradePhase.Complete)
                                            { errorMessage = "Already complete.";      return false; }
            if (node.Phase == UpgradePhase.Research || node.Phase == UpgradePhase.Development)
                                            { errorMessage = "Already in progress.";   return false; }

            if (node.Phase == UpgradePhase.None && node.ResearchWeeks > 0)
                return BeginResearch(state, node, out errorMessage);

            return BeginManufacturing(state, node, out errorMessage);
        }

        private bool BeginResearch(TeamDevelopmentState state, UpgradeNode node,
                                    out string errorMessage)
        {
            if (state.CFDUnitsRemaining < node.ResearchCFDCost)
            {
                errorMessage = $"Need {node.ResearchCFDCost} CFD units "
                             + $"({state.CFDUnitsRemaining} remaining this season).";
                return false;
            }
            if (state.WindTunnelHoursRemaining < node.ResearchWindTunnelHours)
            {
                errorMessage = $"Need {node.ResearchWindTunnelHours}h wind tunnel "
                             + $"({state.WindTunnelHoursRemaining}h remaining this season).";
                return false;
            }

            state.CFDUnitsRemaining          -= node.ResearchCFDCost;
            state.WindTunnelHoursRemaining   -= node.ResearchWindTunnelHours;
            node.Phase                        = UpgradePhase.Research;
            node.PhaseWeeksRemaining          = node.ResearchWeeks;
            state.ActiveIDs.Add(node.ID);
            errorMessage = null;
            return true;
        }

        private bool BeginManufacturing(TeamDevelopmentState state, UpgradeNode node,
                                         out string errorMessage)
        {
            float discount   = LearningCurveDiscount(state, node);
            int   tokenCost  = Math.Max(1, (int)(node.TokenCost    * (1f - discount)));
            int   rdCost     = Math.Max(1, (int)(node.RDPointsCost * (1f - discount)));
            int   devWeeks   = Math.Max(1, (int)(node.DevelopmentWeeks * (1f - discount)));

            if (state.Tokens < tokenCost)
            {
                errorMessage = $"Need {tokenCost} tokens ({state.Tokens} available).";
                return false;
            }
            if (state.RDPoints < rdCost)
            {
                errorMessage = $"Need {rdCost} R&D points ({state.RDPoints} available).";
                return false;
            }

            state.Tokens               -= tokenCost;
            state.RDPoints             -= rdCost;
            node.LearningDiscount       = discount;
            node.Phase                  = UpgradePhase.Development;
            node.PhaseWeeksRemaining    = devWeeks;
            state.ActiveIDs.Add(node.ID);
            errorMessage = null;
            return true;
        }

        /// <summary>Adjusts the R&D budget fraction (0.0–1.0), scaling the weekly accrual rate.</summary>
        public void SetBudgetAllocation(int teamID, float allocation)
        {
            var state = GetState(teamID);
            if (state == null) return;
            state.BudgetAllocation  = Math.Max(0f, Math.Min(1f, allocation));
            var team = GameDataFactory.GetTeam(teamID);
            string tier = team != null && team.carPerformance >= 90 ? "top"
                        : team != null && team.carPerformance >= 83 ? "mid"
                        : "lower";
            int baseRate = _tierRDRates[tier];
            state.RDPointsPerWeek = (int)(baseRate * (0.5f + state.BudgetAllocation * 0.5f));
        }

        // ── Weekly tick ───────────────────────────────────────────────────────

        /// <summary>
        /// Advances one team's development by one race week.
        ///
        /// Research phase: when timer hits 0 the node moves to ResearchComplete and is
        /// removed from ActiveIDs (awaiting player/AI decision to start manufacturing).
        /// Development phase: rolls SuccessProbability; on success, applies gains with a
        /// possible 2× breakthrough bonus.
        /// </summary>
        public DevelopmentTickResult TickWeek(int teamID)
        {
            var result = new DevelopmentTickResult();
            var state  = GetState(teamID);
            if (state == null) return result;

            // Accrue R&D
            state.RDPoints       += state.RDPointsPerWeek;
            result.RDPointsEarned = state.RDPointsPerWeek;

            // Small random token bonus (~1 per 6 weeks)
            if (_rng.NextDouble() < 1.0 / 6.0) state.Tokens++;

            var toRemove = new List<string>();
            foreach (string id in state.ActiveIDs.ToList())
            {
                var node = state.Tree.FirstOrDefault(n => n.ID == id);
                if (node == null) continue;

                node.PhaseWeeksRemaining--;
                if (node.PhaseWeeksRemaining > 0) continue;

                if (node.Phase == UpgradePhase.Research)
                {
                    // Research complete — player must confirm manufacturing
                    node.Phase = UpgradePhase.ResearchComplete;
                    result.ResearchCompleted.Add(id);
                    toRemove.Add(id);
                }
                else if (node.Phase == UpgradePhase.Development)
                {
                    node.Phase = UpgradePhase.Complete;
                    toRemove.Add(id);

                    bool success = _rng.NextDouble() < node.SuccessProbability;
                    if (success)
                    {
                        bool breakthrough = _rng.NextDouble() < BreakthroughChance;
                        node.WasBreakthrough = breakthrough;
                        state.CompletedIDs.Add(id);
                        ApplyUpgrade(state, node, breakthrough);
                        result.CompletedUpgrades.Add(id);
                        if (breakthrough) result.Breakthroughs.Add(id);
                    }
                    else
                    {
                        result.FailedUpgrades.Add(id);
                    }
                }
            }

            foreach (var id in toRemove)
                state.ActiveIDs.Remove(id);

            RefreshUnlocks(state);
            result.Summary = BuildSummary(state, result);
            return result;
        }

        /// <summary>Ticks all 10 teams simultaneously.</summary>
        public Dictionary<int, DevelopmentTickResult> TickAllTeams()
        {
            var results = new Dictionary<int, DevelopmentTickResult>();
            foreach (int id in _states.Keys)
                results[id] = TickWeek(id);
            return results;
        }

        // ── AI auto-development ───────────────────────────────────────────────

        /// <summary>
        /// AI teams: auto-start manufacturing for any ResearchComplete nodes,
        /// then kick off the best available research or direct-development upgrade.
        /// </summary>
        public void AutoDevelopAI(int teamID)
        {
            var state = GetState(teamID);
            if (state == null) return;

            // Commit any nodes waiting in ResearchComplete
            foreach (var node in state.Tree
                .Where(n => n.Phase == UpgradePhase.ResearchComplete))
            {
                StartDevelopment(teamID, node.ID, out _);
            }

            // If no active work, start the highest-impact affordable upgrade
            if (state.ActiveIDs.Count > 0) return;

            var candidate = state.Tree
                .Where(n => n.IsUnlocked
                         && n.Phase == UpgradePhase.None
                         && n.TokenCost <= state.Tokens
                         && n.RDPointsCost <= state.RDPoints)
                .OrderByDescending(n => n.TotalImpact)
                .FirstOrDefault();

            if (candidate != null)
                StartDevelopment(teamID, candidate.ID, out _);
        }

        // ── Internal helpers ──────────────────────────────────────────────────

        private void ApplyUpgrade(TeamDevelopmentState state, UpgradeNode node, bool breakthrough)
        {
            float mult = breakthrough ? 2.0f : 1.0f;
            state.CarPerformanceDelta += node.CarPerformanceGain * mult;
            state.PowerUnitDelta      += node.PowerUnitGain      * mult;
            state.ChassisDelta        += node.ChassisGain        * mult;
            state.ReliabilityDelta    += node.ReliabilityGain    * mult;
        }

        private void RefreshUnlocks(TeamDevelopmentState state)
        {
            foreach (var node in state.Tree)
            {
                if (node.Phase != UpgradePhase.None) continue;
                node.IsUnlocked = string.IsNullOrEmpty(node.RequiredUpgradeID)
                    || state.CompletedIDs.Contains(node.RequiredUpgradeID);
            }
        }

        /// <summary>
        /// Learning curve: each completed upgrade in the same Area reduces the next
        /// iteration's token, R&D, and development-time cost.
        /// Gen1 (1 previous complete): 20% cheaper/faster.
        /// Gen2+ (2+ previous complete): 30% cheaper/faster.
        /// </summary>
        private float LearningCurveDiscount(TeamDevelopmentState state, UpgradeNode node)
        {
            int prevDone = state.Tree.Count(
                n => n.Area == node.Area && n.Phase == UpgradePhase.Complete);
            if (prevDone >= 2) return 0.30f;
            if (prevDone == 1) return 0.20f;
            return 0f;
        }

        private static string BuildSummary(TeamDevelopmentState state, DevelopmentTickResult result)
        {
            var parts = new List<string>();
            if (result.CompletedUpgrades.Count > 0)
                parts.Add($"Deployed: {string.Join(", ", result.CompletedUpgrades)}"
                    + (result.Breakthroughs.Count > 0
                        ? $" ⚡ BREAKTHROUGH: {string.Join(", ", result.Breakthroughs)}" : ""));
            if (result.FailedUpgrades.Count > 0)
                parts.Add($"FAILED: {string.Join(", ", result.FailedUpgrades)}");
            if (result.ResearchCompleted.Count > 0)
                parts.Add($"Research done (ready to build): {string.Join(", ", result.ResearchCompleted)}");
            parts.Add($"+{result.RDPointsEarned} R&D → {state.RDPoints} total");
            return string.Join(" | ", parts);
        }

        // ── Effective stats (for lap time calculation) ─────────────────────────

        public float EffectiveCarPerformance(int teamID)
        {
            var team  = GameDataFactory.GetTeam(teamID);
            if (team == null) return 80f;
            var state = GetState(teamID);
            return team.carPerformance + (state?.CarPerformanceDelta ?? 0f);
        }

        public float EffectivePowerUnit(int teamID)
        {
            var team  = GameDataFactory.GetTeam(teamID);
            if (team == null) return 85f;
            var state = GetState(teamID);
            return team.powerUnit + (state?.PowerUnitDelta ?? 0f);
        }

        public float EffectiveChassis(int teamID)
        {
            var team  = GameDataFactory.GetTeam(teamID);
            if (team == null) return 85f;
            var state = GetState(teamID);
            return team.chassis + (state?.ChassisDelta ?? 0f);
        }

        public float EffectiveReliability(int teamID)
        {
            var team  = GameDataFactory.GetTeam(teamID);
            if (team == null) return 80f;
            var state = GetState(teamID);
            return team.reliability + (state?.ReliabilityDelta ?? 0f);
        }

        /// <summary>All unlocked, unstarted upgrades for the given team, in area/tier order.</summary>
        public List<UpgradeNode> AvailableUpgrades(int teamID)
        {
            var state = GetState(teamID);
            if (state == null) return new List<UpgradeNode>();
            return state.Tree
                .Where(n => n.IsUnlocked && n.Phase == UpgradePhase.None)
                .OrderBy(n => n.Area).ThenBy(n => n.Tier)
                .ToList();
        }

        /// <summary>Nodes that have finished research and are ready to manufacture.</summary>
        public List<UpgradeNode> ReadyToManufacture(int teamID)
        {
            var state = GetState(teamID);
            if (state == null) return new List<UpgradeNode>();
            return state.Tree
                .Where(n => n.Phase == UpgradePhase.ResearchComplete)
                .ToList();
        }

        /// <summary>
        /// Resets ATR budgets for all teams at the start of a new season.
        /// The sliding scale means the constructor championship leader gets the least WT time.
        /// </summary>
        public void ResetATRForNewSeason(List<int> constructorRanking)
        {
            // constructorRanking: team IDs ordered P1 → P10 from last season
            for (int i = 0; i < constructorRanking.Count; i++)
            {
                var state = GetState(constructorRanking[i]);
                if (state == null) continue;

                // Linear interpolation: P1 gets minimum ATR, P10 gets maximum
                float t = (float)i / Math.Max(1, constructorRanking.Count - 1);
                state.WindTunnelHoursRemaining = (int)(56 + t * (80 - 56));
                state.CFDUnitsRemaining        = (int)(1120 + t * (1840 - 1120));
            }
        }
    }
}
