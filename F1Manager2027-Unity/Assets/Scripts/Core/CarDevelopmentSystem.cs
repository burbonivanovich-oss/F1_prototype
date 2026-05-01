// CarDevelopmentSystem.cs — Car upgrade tree: tokens, R&D points, weekly performance growth,
// and upgrade success/failure risk. Pure C#, no Unity dependencies.
using System;
using System.Collections.Generic;
using System.Linq;

namespace F1Manager
{
    // ─────────────────────────────────────────────────────────────────────────
    // Upgrade areas
    // ─────────────────────────────────────────────────────────────────────────

    public enum UpgradeArea
    {
        POWER_UNIT,     // Affects powerUnit rating + power-sensitive circuits
        AERO_FRONT,     // Affects chassis rating + downforce-sensitive circuits
        AERO_REAR,      // Affects chassis rating + stability
        CHASSIS,        // Affects overall carPerformance
        SUSPENSION,     // Affects tire wear rates (tireDegMultiplier improvement)
        GEARBOX,        // Affects reliability
        COOLING,        // Affects reliability + hot-circuit performance
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Upgrade node
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// One node in the upgrade tree.
    /// Each area has multiple tiers; Tier 1 must be unlocked before Tier 2, etc.
    /// </summary>
    public class UpgradeNode
    {
        public string      ID;             // Unique identifier e.g. "PU_T1", "AERO_F_T2"
        public UpgradeArea Area;
        public int         Tier;           // 1-based tier within the area

        // Cost
        public int   TokenCost;            // R&D tokens (hard cap ~40/season for mid-teams)
        public int   RDPointsCost;         // R&D points accumulated by spending budget

        // Development time
        public int   WeeksToDevelop;       // Rounds until upgrade is ready to deploy

        // Risk
        public float SuccessProbability;   // 0.0–1.0; failure = no performance gain but cost paid

        // Reward
        public float CarPerformanceGain;   // Added to TeamInfo.carPerformance (fractional)
        public float PowerUnitGain;        // Added to TeamInfo.powerUnit
        public float ChassisGain;          // Added to TeamInfo.chassis
        public float ReliabilityGain;      // Added to TeamInfo.reliability

        // Tree dependency
        public string RequiredUpgradeID;   // Null = root node

        // State
        public bool IsUnlocked      = false;  // Prerequisites met
        public bool IsInDevelopment = false;
        public bool IsComplete      = false;
        public int  WeeksRemaining  = 0;

        /// <summary>Total performance units gained across all stats.</summary>
        public float TotalImpact => CarPerformanceGain + PowerUnitGain * 0.5f
                                 + ChassisGain * 0.5f + ReliabilityGain * 0.2f;

        public string Describe()
        {
            string eff = IsComplete ? "[DONE]" : IsInDevelopment ? $"[{WeeksRemaining}w]" : "";
            return $"{ID} | {Area} T{Tier} | {TokenCost}T/{RDPointsCost}R&D | "
                + $"{SuccessProbability*100:F0}% | +{TotalImpact:F1} perf {eff}";
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Development state per team
    // ─────────────────────────────────────────────────────────────────────────

    public class TeamDevelopmentState
    {
        public int TeamID;

        // Resources
        public int   Tokens;           // Finite season budget (typ. 30–50 per team)
        public int   RDPoints;         // Accumulated; spent to develop upgrades
        public int   RDPointsPerWeek;  // Accrual rate based on budget allocation (5–25)
        public float BudgetAllocation; // 0.0–1.0 fraction of budget spent on R&D

        // Applied performance deltas (cumulative from completed upgrades)
        public float CarPerformanceDelta;
        public float PowerUnitDelta;
        public float ChassisDelta;
        public float ReliabilityDelta;

        // In-progress and completed upgrades
        public List<UpgradeNode> Tree          = new List<UpgradeNode>();
        public List<string>      CompletedIDs  = new List<string>();
        public List<string>      ActiveIDs     = new List<string>(); // In development

        public TeamDevelopmentState(int teamID, int initialTokens, int rdPerWeek)
        {
            TeamID         = teamID;
            Tokens         = initialTokens;
            RDPointsPerWeek = rdPerWeek;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Development result (per-week tick)
    // ─────────────────────────────────────────────────────────────────────────

    public class DevelopmentTickResult
    {
        public List<string> CompletedUpgrades  = new List<string>(); // IDs finished this week
        public List<string> FailedUpgrades     = new List<string>(); // IDs that failed
        public int          RDPointsEarned;
        public string       Summary;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CarDevelopmentSystem
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Manages the car upgrade tree for all teams. Each round, <see cref="TickWeek"/>
    /// is called once to advance development timers and accrue R&D points.
    /// Players allocate tokens and R&D points to start development of new upgrades.
    /// </summary>
    public class CarDevelopmentSystem
    {
        private readonly Dictionary<int, TeamDevelopmentState> _states
            = new Dictionary<int, TeamDevelopmentState>();

        private readonly System.Random _rng;

        // Token allocations per team tier (tokens available for season)
        private static readonly Dictionary<string, int> _tierTokens = new Dictionary<string, int>
        {
            { "top",    48 },   // Red Bull, Mercedes, Ferrari
            { "mid",    36 },   // McLaren, Aston, Alpine
            { "lower",  28 },   // Williams, Haas, Sauber, Audi
        };

        // R&D points per week by budget allocation tier
        private static readonly int[] _rdRates = { 5, 8, 12, 16, 20, 25 }; // index 0=lowest, 5=top

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

                int tokens   = _tierTokens[tier];
                int rdRate   = team.carPerformance >= 90 ? _rdRates[5]
                             : team.carPerformance >= 86 ? _rdRates[4]
                             : team.carPerformance >= 83 ? _rdRates[3]
                             : team.carPerformance >= 80 ? _rdRates[2]
                             : _rdRates[1];

                var state = new TeamDevelopmentState(team.id, tokens, rdRate);
                state.Tree = BuildUpgradeTree(team);
                RefreshUnlocks(state);
                _states[team.id] = state;
            }
        }

        /// <summary>Returns development state for the given team.</summary>
        public TeamDevelopmentState GetState(int teamID)
        {
            return _states.TryGetValue(teamID, out var s) ? s : null;
        }

        // ── Upgrade tree factory ──────────────────────────────────────────────

        private static List<UpgradeNode> BuildUpgradeTree(TeamInfo team)
        {
            var nodes = new List<UpgradeNode>();

            // Power Unit — 3 tiers
            nodes.Add(new UpgradeNode {
                ID = "PU_T1", Area = UpgradeArea.POWER_UNIT, Tier = 1,
                TokenCost = 4, RDPointsCost = 60, WeeksToDevelop = 3,
                SuccessProbability = 0.90f, PowerUnitGain = 1.0f, CarPerformanceGain = 0.3f,
            });
            nodes.Add(new UpgradeNode {
                ID = "PU_T2", Area = UpgradeArea.POWER_UNIT, Tier = 2,
                TokenCost = 6, RDPointsCost = 100, WeeksToDevelop = 4,
                SuccessProbability = 0.80f, PowerUnitGain = 1.5f, CarPerformanceGain = 0.5f,
                RequiredUpgradeID = "PU_T1",
            });
            nodes.Add(new UpgradeNode {
                ID = "PU_T3", Area = UpgradeArea.POWER_UNIT, Tier = 3,
                TokenCost = 8, RDPointsCost = 160, WeeksToDevelop = 6,
                SuccessProbability = 0.65f, PowerUnitGain = 2.0f, CarPerformanceGain = 0.8f,
                RequiredUpgradeID = "PU_T2",
            });

            // Aero Front — 3 tiers
            nodes.Add(new UpgradeNode {
                ID = "AF_T1", Area = UpgradeArea.AERO_FRONT, Tier = 1,
                TokenCost = 3, RDPointsCost = 50, WeeksToDevelop = 2,
                SuccessProbability = 0.92f, ChassisGain = 0.8f, CarPerformanceGain = 0.4f,
            });
            nodes.Add(new UpgradeNode {
                ID = "AF_T2", Area = UpgradeArea.AERO_FRONT, Tier = 2,
                TokenCost = 5, RDPointsCost = 90, WeeksToDevelop = 3,
                SuccessProbability = 0.80f, ChassisGain = 1.2f, CarPerformanceGain = 0.6f,
                RequiredUpgradeID = "AF_T1",
            });
            nodes.Add(new UpgradeNode {
                ID = "AF_T3", Area = UpgradeArea.AERO_FRONT, Tier = 3,
                TokenCost = 7, RDPointsCost = 140, WeeksToDevelop = 5,
                SuccessProbability = 0.70f, ChassisGain = 1.8f, CarPerformanceGain = 0.9f,
                RequiredUpgradeID = "AF_T2",
            });

            // Aero Rear — 2 tiers
            nodes.Add(new UpgradeNode {
                ID = "AR_T1", Area = UpgradeArea.AERO_REAR, Tier = 1,
                TokenCost = 3, RDPointsCost = 50, WeeksToDevelop = 2,
                SuccessProbability = 0.90f, ChassisGain = 0.6f, CarPerformanceGain = 0.3f,
            });
            nodes.Add(new UpgradeNode {
                ID = "AR_T2", Area = UpgradeArea.AERO_REAR, Tier = 2,
                TokenCost = 5, RDPointsCost = 85, WeeksToDevelop = 3,
                SuccessProbability = 0.78f, ChassisGain = 1.0f, CarPerformanceGain = 0.5f,
                RequiredUpgradeID = "AR_T1",
            });

            // Chassis — 2 tiers
            nodes.Add(new UpgradeNode {
                ID = "CH_T1", Area = UpgradeArea.CHASSIS, Tier = 1,
                TokenCost = 5, RDPointsCost = 80, WeeksToDevelop = 4,
                SuccessProbability = 0.85f, CarPerformanceGain = 1.0f, ChassisGain = 0.5f,
            });
            nodes.Add(new UpgradeNode {
                ID = "CH_T2", Area = UpgradeArea.CHASSIS, Tier = 2,
                TokenCost = 8, RDPointsCost = 130, WeeksToDevelop = 5,
                SuccessProbability = 0.72f, CarPerformanceGain = 1.5f, ChassisGain = 1.0f,
                RequiredUpgradeID = "CH_T1",
            });

            // Suspension — 2 tiers
            nodes.Add(new UpgradeNode {
                ID = "SUS_T1", Area = UpgradeArea.SUSPENSION, Tier = 1,
                TokenCost = 3, RDPointsCost = 55, WeeksToDevelop = 2,
                SuccessProbability = 0.90f, CarPerformanceGain = 0.4f, ReliabilityGain = 1.0f,
            });
            nodes.Add(new UpgradeNode {
                ID = "SUS_T2", Area = UpgradeArea.SUSPENSION, Tier = 2,
                TokenCost = 5, RDPointsCost = 90, WeeksToDevelop = 3,
                SuccessProbability = 0.80f, CarPerformanceGain = 0.6f, ReliabilityGain = 1.5f,
                RequiredUpgradeID = "SUS_T1",
            });

            // Gearbox — 2 tiers
            nodes.Add(new UpgradeNode {
                ID = "GB_T1", Area = UpgradeArea.GEARBOX, Tier = 1,
                TokenCost = 2, RDPointsCost = 40, WeeksToDevelop = 2,
                SuccessProbability = 0.92f, ReliabilityGain = 2.0f, CarPerformanceGain = 0.2f,
            });
            nodes.Add(new UpgradeNode {
                ID = "GB_T2", Area = UpgradeArea.GEARBOX, Tier = 2,
                TokenCost = 4, RDPointsCost = 70, WeeksToDevelop = 3,
                SuccessProbability = 0.82f, ReliabilityGain = 2.5f, CarPerformanceGain = 0.3f,
                RequiredUpgradeID = "GB_T1",
            });

            // Cooling — 1 tier (reliability focus)
            nodes.Add(new UpgradeNode {
                ID = "COOL_T1", Area = UpgradeArea.COOLING, Tier = 1,
                TokenCost = 2, RDPointsCost = 35, WeeksToDevelop = 2,
                SuccessProbability = 0.93f, ReliabilityGain = 3.0f, CarPerformanceGain = 0.1f,
            });

            return nodes;
        }

        // ── Player actions ────────────────────────────────────────────────────

        /// <summary>
        /// Starts development on a specific upgrade for a team.
        /// Returns false if insufficient tokens, R&D points, or prerequisites not met.
        /// </summary>
        public bool StartDevelopment(int teamID, string upgradeID, out string errorMessage)
        {
            errorMessage = null;
            var state = GetState(teamID);
            if (state == null) { errorMessage = "Invalid team."; return false; }

            var node = state.Tree.FirstOrDefault(n => n.ID == upgradeID);
            if (node == null) { errorMessage = "Upgrade not found."; return false; }
            if (!node.IsUnlocked) { errorMessage = "Prerequisites not met."; return false; }
            if (node.IsInDevelopment || node.IsComplete) { errorMessage = "Already developed."; return false; }
            if (state.Tokens < node.TokenCost) { errorMessage = $"Need {node.TokenCost} tokens ({state.Tokens} available)."; return false; }
            if (state.RDPoints < node.RDPointsCost) { errorMessage = $"Need {node.RDPointsCost} R&D points ({state.RDPoints} available)."; return false; }

            state.Tokens     -= node.TokenCost;
            state.RDPoints   -= node.RDPointsCost;
            node.IsInDevelopment = true;
            node.WeeksRemaining  = node.WeeksToDevelop;
            state.ActiveIDs.Add(upgradeID);
            return true;
        }

        /// <summary>
        /// Adjusts the R&D budget allocation for a team (0.0=none → 1.0=maximum).
        /// Affects R&D points accrual per week.
        /// </summary>
        public void SetBudgetAllocation(int teamID, float allocation)
        {
            var state = GetState(teamID);
            if (state == null) return;
            state.BudgetAllocation = Math.Max(0f, Math.Min(1f, allocation));
            // Recalculate RD rate
            int idx = (int)(state.BudgetAllocation * (_rdRates.Length - 1));
            var team = GameDataFactory.GetTeam(teamID);
            int baseRate = team != null && team.carPerformance >= 90 ? _rdRates[5]
                         : team != null && team.carPerformance >= 83 ? _rdRates[3]
                         : _rdRates[1];
            state.RDPointsPerWeek = (int)(baseRate * (0.5f + state.BudgetAllocation * 0.5f));
        }

        // ── Weekly tick ───────────────────────────────────────────────────────

        /// <summary>
        /// Advances all teams' development by one week (one race round).
        /// Call once per CalendarEntry before the race weekend begins.
        /// </summary>
        public DevelopmentTickResult TickWeek(int teamID)
        {
            var result = new DevelopmentTickResult();
            var state  = GetState(teamID);
            if (state == null) return result;

            // Accrue R&D points
            int earned = state.RDPointsPerWeek;
            state.RDPoints        += earned;
            result.RDPointsEarned  = earned;

            // AI teams also gain tokens slowly (1 per 6 weeks)
            if (_rng.NextDouble() < 1.0 / 6.0)
                state.Tokens += 1;

            // Tick active upgrades
            var toRemove = new List<string>();
            foreach (string id in state.ActiveIDs.ToList())
            {
                var node = state.Tree.FirstOrDefault(n => n.ID == id);
                if (node == null) continue;

                node.WeeksRemaining--;
                if (node.WeeksRemaining > 0) continue;

                // Development complete — roll for success
                bool success = _rng.NextDouble() < node.SuccessProbability;
                node.IsInDevelopment = false;
                node.IsComplete      = success;
                toRemove.Add(id);

                if (success)
                {
                    result.CompletedUpgrades.Add(id);
                    state.CompletedIDs.Add(id);
                    ApplyUpgrade(state, node);
                }
                else
                {
                    result.FailedUpgrades.Add(id);
                }
            }

            foreach (var id in toRemove)
                state.ActiveIDs.Remove(id);

            RefreshUnlocks(state);

            result.Summary = BuildSummary(state, result);
            return result;
        }

        /// <summary>Ticks all 10 teams simultaneously (e.g. for AI teams).</summary>
        public Dictionary<int, DevelopmentTickResult> TickAllTeams()
        {
            var results = new Dictionary<int, DevelopmentTickResult>();
            foreach (int teamID in _states.Keys)
                results[teamID] = TickWeek(teamID);
            return results;
        }

        // ── AI auto-development ───────────────────────────────────────────────

        /// <summary>
        /// AI teams automatically start development on the highest-value available upgrade
        /// if they have sufficient resources and no active development.
        /// </summary>
        public void AutoDevelopAI(int teamID)
        {
            var state = GetState(teamID);
            if (state == null || state.ActiveIDs.Count > 0) return;

            // Pick best-value unlocked undeveloped upgrade affordable within budget
            var candidate = state.Tree
                .Where(n => n.IsUnlocked && !n.IsInDevelopment && !n.IsComplete)
                .Where(n => n.TokenCost <= state.Tokens && n.RDPointsCost <= state.RDPoints)
                .OrderByDescending(n => n.TotalImpact)
                .FirstOrDefault();

            if (candidate != null)
                StartDevelopment(teamID, candidate.ID, out _);
        }

        // ── Internal helpers ──────────────────────────────────────────────────

        private void ApplyUpgrade(TeamDevelopmentState state, UpgradeNode node)
        {
            state.CarPerformanceDelta += node.CarPerformanceGain;
            state.PowerUnitDelta      += node.PowerUnitGain;
            state.ChassisDelta        += node.ChassisGain;
            state.ReliabilityDelta    += node.ReliabilityGain;
        }

        private void RefreshUnlocks(TeamDevelopmentState state)
        {
            foreach (var node in state.Tree)
            {
                if (node.IsComplete || node.IsInDevelopment) continue;

                node.IsUnlocked = string.IsNullOrEmpty(node.RequiredUpgradeID)
                    || state.CompletedIDs.Contains(node.RequiredUpgradeID);
            }
        }

        private static string BuildSummary(TeamDevelopmentState state, DevelopmentTickResult result)
        {
            var parts = new List<string>();
            if (result.CompletedUpgrades.Count > 0)
                parts.Add($"Completed: {string.Join(", ", result.CompletedUpgrades)}");
            if (result.FailedUpgrades.Count > 0)
                parts.Add($"FAILED: {string.Join(", ", result.FailedUpgrades)}");
            parts.Add($"+{result.RDPointsEarned} R&D pts → {state.RDPoints} total");
            return string.Join(" | ", parts);
        }

        // ── Effective stats ───────────────────────────────────────────────────

        /// <summary>
        /// Returns effective carPerformance for a team, including all applied upgrades.
        /// Use this in race/qualifying lap time calculation instead of TeamInfo.carPerformance.
        /// </summary>
        public float EffectiveCarPerformance(int teamID)
        {
            var team  = GameDataFactory.GetTeam(teamID);
            if (team == null) return 80f;
            var state = GetState(teamID);
            if (state == null) return team.carPerformance;
            return team.carPerformance + state.CarPerformanceDelta;
        }

        public float EffectivePowerUnit(int teamID)
        {
            var team  = GameDataFactory.GetTeam(teamID);
            if (team == null) return 85f;
            var state = GetState(teamID);
            return (state == null) ? team.powerUnit : team.powerUnit + state.PowerUnitDelta;
        }

        public float EffectiveChassis(int teamID)
        {
            var team  = GameDataFactory.GetTeam(teamID);
            if (team == null) return 85f;
            var state = GetState(teamID);
            return (state == null) ? team.chassis : team.chassis + state.ChassisDelta;
        }

        /// <summary>Lists available (unlocked, not started, affordable) upgrades for the player.</summary>
        public List<UpgradeNode> AvailableUpgrades(int teamID)
        {
            var state = GetState(teamID);
            if (state == null) return new List<UpgradeNode>();
            return state.Tree
                .Where(n => n.IsUnlocked && !n.IsInDevelopment && !n.IsComplete)
                .OrderBy(n => n.Area).ThenBy(n => n.Tier)
                .ToList();
        }
    }
}
