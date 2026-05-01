// DriverMarket.cs — Transfer window: contracts, competitor interest, salary budget.
// Pure C#, no Unity dependencies.
using System;
using System.Collections.Generic;
using System.Linq;

namespace F1Manager
{
    // ─────────────────────────────────────────────────────────────────────────
    // Contract
    // ─────────────────────────────────────────────────────────────────────────

    public class DriverContract
    {
        public int    DriverID;
        public int    TeamID;
        public int    DurationYears;    // 1–3 years
        public int    SalaryMillions;   // Annual salary (M€)
        public int    SeasonStart;      // Season when contract began
        public int    SeasonEnd;        // Season when contract expires (exclusive)
        public bool   HasVetoClause;    // Driver can block a transfer

        public bool IsExpired(int currentSeason) => currentSeason >= SeasonEnd;
        public int  YearsRemaining(int currentSeason) => Math.Max(0, SeasonEnd - currentSeason);

        public string Describe(int currentSeason)
        {
            string veto = HasVetoClause ? " [VETO]" : "";
            int remaining = YearsRemaining(currentSeason);
            string exp = remaining == 0 ? "Expiring" : $"{remaining}yr remaining";
            return $"{exp}, €{SalaryMillions}M/yr{veto}";
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Transfer offer
    // ─────────────────────────────────────────────────────────────────────────

    public class TransferOffer
    {
        public int    OfferingTeamID;
        public int    DriverID;
        public int    ProposedSalaryMillions;
        public int    ProposedDurationYears;
        public bool   IsPlayerOffer;   // True = human player sent this

        // Outcome
        public bool   IsAccepted;
        public bool   IsDeclined;
        public string DeclineReason;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Driver market value
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Computed market value for one driver at a given point in time.</summary>
    public class DriverMarketValue
    {
        public int   DriverID;
        public int   BaseValueMillions;     // Annual market rate
        public int   MinAcceptableMillions; // Will not sign below this
        public bool  IsMarketAvailable;     // Not under contract or contract expiring
        public int   InterestingTeamCount;  // How many teams are actively pursuing
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Transfer window state
    // ─────────────────────────────────────────────────────────────────────────

    public class TransferWindowState
    {
        public int  Season;
        public bool IsOpen;  // Transfer window opens at season end / mid-season

        public List<DriverContract>  Contracts = new List<DriverContract>();
        public List<TransferOffer>   PendingOffers = new List<TransferOffer>();
        public List<TransferOffer>   ClosedOffers  = new List<TransferOffer>();

        // Team salary budgets (M€ per year)
        public Dictionary<int, int>  TeamBudgets = new Dictionary<int, int>();
        public Dictionary<int, int>  TeamSpent   = new Dictionary<int, int>(); // Committed salary

        /// <summary>Returns contracts for a specific team.</summary>
        public List<DriverContract> GetTeamContracts(int teamID)
            => Contracts.Where(c => c.TeamID == teamID).ToList();

        /// <summary>Returns the contract for a driver, or null if free agent.</summary>
        public DriverContract GetDriverContract(int driverID)
            => Contracts.FirstOrDefault(c => c.DriverID == driverID);

        /// <summary>Budget remaining for a team after committed salary.</summary>
        public int RemainingBudget(int teamID)
        {
            int budget  = TeamBudgets.TryGetValue(teamID, out int b) ? b : 20;
            int spent   = TeamSpent.TryGetValue(teamID, out int s)   ? s : 0;
            return budget - spent;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DriverMarket
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Manages driver contracts and the transfer window.
    /// At season end, expired contracts free drivers for transfer.
    /// AI teams bid on available drivers based on their ratings and budget.
    /// Players can make offers within their team's salary budget.
    /// </summary>
    public class DriverMarket
    {
        public TransferWindowState State { get; private set; }
        private readonly System.Random _rng;

        // Salary table (millions per year) indexed by driver pace/racecraft average
        // High-rated drivers demand more; top teams pay premium
        private static readonly (int minRating, int baseSalary)[] _salaryBands =
        {
            (95, 45), // Verstappen-tier: €45M+
            (90, 30), // Hamilton/Leclerc-tier: €30M
            (85, 20), // Solid race winner: €20M
            (80, 14), // Mid-field regular: €14M
            (75, 10), // Lower mid-field: €10M
            (0,   6), // Rookie/lower grid: €6M
        };

        // Team salary budgets (M€ per season — combined for two drivers)
        private static readonly Dictionary<string, int> _teamBudgetBands = new Dictionary<string, int>
        {
            { "top",   90 },   // RedBull, Mercedes, Ferrari
            { "mid",   60 },   // McLaren, Aston
            { "lower", 35 },   // Williams, Haas, Sauber, Alpine
        };

        public DriverMarket(int season = 2027, System.Random rng = null)
        {
            _rng  = rng ?? new System.Random();
            State = new TransferWindowState { Season = season };
            GameDataFactory.Initialize();
            InitialiseContracts();
            InitialiseBudgets();
        }

        // ── Initialisation ────────────────────────────────────────────────────

        private void InitialiseContracts()
        {
            // Give all active drivers a 1–3 year contract at season start
            foreach (var driver in GameDataFactory.Drivers)
            {
                int salary   = CalculateBaseSalary(driver);
                int duration = _rng.Next(1, 4);  // 1–3 years
                bool veto    = salary >= 25;       // Only top drivers have veto clauses

                var contract = new DriverContract
                {
                    DriverID        = driver.id,
                    TeamID          = driver.teamID,
                    DurationYears   = duration,
                    SalaryMillions  = salary,
                    SeasonStart     = State.Season,
                    SeasonEnd       = State.Season + duration,
                    HasVetoClause   = veto,
                };
                State.Contracts.Add(contract);
            }
        }

        private void InitialiseBudgets()
        {
            foreach (var team in GameDataFactory.Teams)
            {
                string tier = team.carPerformance >= 90 ? "top"
                            : team.carPerformance >= 83 ? "mid"
                            : "lower";
                State.TeamBudgets[team.id] = _teamBudgetBands[tier];

                // Committed salary = sum of all driver salaries on this team
                int spent = State.Contracts
                    .Where(c => c.TeamID == team.id)
                    .Sum(c => c.SalaryMillions);
                State.TeamSpent[team.id] = spent;
            }
        }

        // ── Market value helpers ──────────────────────────────────────────────

        /// <summary>Computes market value for a driver at current time.</summary>
        public DriverMarketValue GetMarketValue(int driverID)
        {
            var driver   = GameDataFactory.GetDriver(driverID);
            if (driver == null) return null;

            int   base_  = CalculateBaseSalary(driver);
            var   contract = State.GetDriverContract(driverID);
            bool  free   = contract == null || contract.IsExpired(State.Season);

            // AI interest: teams with budget and an expiring seat
            int interest = State.Contracts.Count(c =>
                c.DriverID != driverID
                && c.IsExpired(State.Season)
                && State.RemainingBudget(c.TeamID) >= base_ * 8 / 10);

            return new DriverMarketValue
            {
                DriverID               = driverID,
                BaseValueMillions      = base_,
                MinAcceptableMillions  = (int)(base_ * 0.75f),
                IsMarketAvailable      = free,
                InterestingTeamCount   = Math.Min(interest, 3),
            };
        }

        private int CalculateBaseSalary(DriverInfo driver)
        {
            int avg = (driver.pace + driver.racecraft) / 2;
            foreach (var (minRating, baseSalary) in _salaryBands)
                if (avg >= minRating) return baseSalary;
            return _salaryBands[_salaryBands.Length - 1].baseSalary;
        }

        // ── Transfer window actions ───────────────────────────────────────────

        /// <summary>
        /// Opens the transfer window (call at end of season).
        /// Expired contracts are cleared; drivers become free agents.
        /// </summary>
        public void OpenTransferWindow()
        {
            State.IsOpen = true;
            // Remove expired contracts
            State.Contracts.RemoveAll(c => c.IsExpired(State.Season));
        }

        /// <summary>
        /// Closes the transfer window (call before new season starts).
        /// Any unsigned drivers get emergency 1-year contracts.
        /// </summary>
        public void CloseTransferWindow()
        {
            State.IsOpen = false;
            SignEmergencyContracts();
        }

        /// <summary>
        /// Advances the season counter by one year.
        /// Call after the season is complete.
        /// </summary>
        public void AdvanceSeason()
        {
            State.Season++;
            OpenTransferWindow();
        }

        // ── Player offer ──────────────────────────────────────────────────────

        /// <summary>
        /// Makes an offer to sign a driver to the player's team.
        /// Returns an error string if the offer is not valid, otherwise null.
        /// The offer is resolved immediately if the driver is a free agent;
        /// contract-buyouts require AI acceptance.
        /// </summary>
        public string MakeOffer(int playerTeamID, int driverID, int salaryMillions, int durationYears)
        {
            if (!State.IsOpen) return "Transfer window is closed.";

            var driver   = GameDataFactory.GetDriver(driverID);
            if (driver == null) return "Driver not found.";

            var mv = GetMarketValue(driverID);
            if (!mv.IsMarketAvailable) return $"Driver is under contract (has veto clause).";

            if (salaryMillions < mv.MinAcceptableMillions)
                return $"Offer too low — driver minimum is €{mv.MinAcceptableMillions}M.";

            // Check team budget
            var teamContracts = State.GetTeamContracts(playerTeamID);
            if (teamContracts.Count >= 2)
                return "Team already has 2 signed drivers.";

            int remaining = State.RemainingBudget(playerTeamID);
            if (salaryMillions > remaining)
                return $"Insufficient budget — €{remaining}M remaining, €{salaryMillions}M requested.";

            // Calculate acceptance probability
            float accept = AcceptanceProbability(driverID, playerTeamID, salaryMillions);
            bool accepted = _rng.NextDouble() < accept;

            var offer = new TransferOffer
            {
                OfferingTeamID         = playerTeamID,
                DriverID               = driverID,
                ProposedSalaryMillions = salaryMillions,
                ProposedDurationYears  = durationYears,
                IsPlayerOffer          = true,
                IsAccepted             = accepted,
                IsDeclined             = !accepted,
                DeclineReason          = accepted ? null : "Driver chose a different offer.",
            };

            State.ClosedOffers.Add(offer);

            if (accepted)
            {
                var contract = new DriverContract
                {
                    DriverID       = driverID,
                    TeamID         = playerTeamID,
                    DurationYears  = durationYears,
                    SalaryMillions = salaryMillions,
                    SeasonStart    = State.Season,
                    SeasonEnd      = State.Season + durationYears,
                    HasVetoClause  = salaryMillions >= 25,
                };
                State.Contracts.Add(contract);

                if (!State.TeamSpent.ContainsKey(playerTeamID)) State.TeamSpent[playerTeamID] = 0;
                State.TeamSpent[playerTeamID] += salaryMillions;
            }

            return null; // No error
        }

        // ── AI transfer simulation ────────────────────────────────────────────

        /// <summary>
        /// Simulates AI team transfer activity for one transfer window.
        /// Call after OpenTransferWindow() and before CloseTransferWindow().
        /// </summary>
        public List<TransferOffer> RunAITransfers()
        {
            var completed = new List<TransferOffer>();

            // Collect free agents
            var freeAgents = GameDataFactory.Drivers
                .Where(d => State.GetDriverContract(d.id) == null)
                .OrderByDescending(d => (d.pace + d.racecraft) / 2)
                .ToList();

            // AI teams with open seats bid on free agents
            foreach (var team in GameDataFactory.Teams.OrderByDescending(t => t.carPerformance))
            {
                var teamContracts = State.GetTeamContracts(team.id);
                int openSeats     = 2 - teamContracts.Count;
                if (openSeats <= 0) continue;

                int budget = State.RemainingBudget(team.id);

                for (int seat = 0; seat < openSeats && freeAgents.Count > 0; seat++)
                {
                    // Pick best affordable candidate
                    var candidate = freeAgents
                        .FirstOrDefault(d => CalculateBaseSalary(d) <= budget);
                    if (candidate == null) break;

                    int salary   = (int)(CalculateBaseSalary(candidate) * (0.90f + (float)_rng.NextDouble() * 0.20f));
                    int duration = _rng.Next(1, 4);

                    float accept = AcceptanceProbability(candidate.id, team.id, salary);
                    bool  ok     = _rng.NextDouble() < accept;

                    var offer = new TransferOffer
                    {
                        OfferingTeamID         = team.id,
                        DriverID               = candidate.id,
                        ProposedSalaryMillions = salary,
                        ProposedDurationYears  = duration,
                        IsPlayerOffer          = false,
                        IsAccepted             = ok,
                        IsDeclined             = !ok,
                    };
                    completed.Add(offer);
                    State.ClosedOffers.Add(offer);

                    if (ok)
                    {
                        var contract = new DriverContract
                        {
                            DriverID       = candidate.id,
                            TeamID         = team.id,
                            DurationYears  = duration,
                            SalaryMillions = salary,
                            SeasonStart    = State.Season,
                            SeasonEnd      = State.Season + duration,
                            HasVetoClause  = salary >= 25,
                        };
                        State.Contracts.Add(contract);
                        if (!State.TeamSpent.ContainsKey(team.id)) State.TeamSpent[team.id] = 0;
                        State.TeamSpent[team.id] += salary;
                        budget -= salary;
                        freeAgents.Remove(candidate);
                    }
                }
            }

            return completed;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private float AcceptanceProbability(int driverID, int teamID, int salaryMillions)
        {
            var  driver  = GameDataFactory.GetDriver(driverID);
            var  team    = GameDataFactory.GetTeam(teamID);
            if  (driver == null || team == null) return 0f;

            var  mv      = GetMarketValue(driverID);
            if  (mv == null) return 0f;

            // Base: does the salary meet expectations?
            float salaryRatio = (float)salaryMillions / mv.BaseValueMillions;
            float base_       = Math.Min(salaryRatio - 0.5f, 1.0f); // 0 if half, 1 if full market

            // Team prestige bonus: top teams more attractive
            float prestigeBonus = (team.carPerformance - 76f) / (97f - 76f) * 0.20f;

            // Competition: other interested teams reduce acceptance
            float rivalry = mv.InterestingTeamCount * 0.05f;

            return Math.Max(0f, Math.Min(1f, base_ + prestigeBonus - rivalry));
        }

        private void SignEmergencyContracts()
        {
            foreach (var driver in GameDataFactory.Drivers)
            {
                if (State.GetDriverContract(driver.id) != null) continue;

                // Emergency 1-year deal at 80% market value
                int salary = (int)(CalculateBaseSalary(driver) * 0.80f);
                State.Contracts.Add(new DriverContract
                {
                    DriverID      = driver.id,
                    TeamID        = driver.teamID,  // Return to home team
                    DurationYears = 1,
                    SalaryMillions = salary,
                    SeasonStart   = State.Season,
                    SeasonEnd     = State.Season + 1,
                    HasVetoClause = false,
                });
            }
        }

        // ── Display helpers ───────────────────────────────────────────────────

        /// <summary>Returns all free agents (drivers with no current contract).</summary>
        public List<DriverInfo> GetFreeAgents()
        {
            return GameDataFactory.Drivers
                .Where(d => State.GetDriverContract(d.id) == null)
                .OrderByDescending(d => (d.pace + d.racecraft) / 2)
                .ToList();
        }

        /// <summary>
        /// Returns drivers whose contracts expire at end of the current season.
        /// Useful for planning ahead.
        /// </summary>
        public List<DriverInfo> GetExpiringContracts(int currentSeason)
        {
            var expiringIDs = State.Contracts
                .Where(c => c.SeasonEnd == currentSeason + 1)
                .Select(c => c.DriverID)
                .ToHashSet();

            return GameDataFactory.Drivers
                .Where(d => expiringIDs.Contains(d.id))
                .ToList();
        }
    }
}
