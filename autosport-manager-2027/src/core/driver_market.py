"""Driver transfer market: contracts, competitor interest, salary budgets."""

from __future__ import annotations
import random
from dataclasses import dataclass, field
from typing import Optional


# ─── Contract ─────────────────────────────────────────────────────────────────

@dataclass
class DriverContract:
    driver_id:       int
    team_id:         int
    duration_years:  int
    salary_m:        int    # Annual salary in millions €
    season_start:    int
    season_end:      int    # Exclusive
    has_veto:        bool = False

    def is_expired(self, season: int) -> bool:
        return season >= self.season_end

    def years_remaining(self, season: int) -> int:
        return max(0, self.season_end - season)

    def describe(self, season: int) -> str:
        rem   = self.years_remaining(season)
        exp   = "Expiring" if rem == 0 else f"{rem}yr left"
        veto  = " [VETO]" if self.has_veto else ""
        return f"{exp}, €{self.salary_m}M/yr{veto}"


# ─── Transfer offer ───────────────────────────────────────────────────────────

@dataclass
class TransferOffer:
    offering_team_id: int
    driver_id:        int
    salary_m:         int
    duration_years:   int
    is_player_offer:  bool = False
    is_accepted:      bool = False
    is_declined:      bool = False
    decline_reason:   str  = ""


# ─── Market value ─────────────────────────────────────────────────────────────

@dataclass
class DriverMarketValue:
    driver_id:        int
    base_salary_m:    int
    min_salary_m:     int     # Will not sign below this
    is_available:     bool    # No current contract, or expiring
    interest_count:   int     # Number of teams pursuing


# ─── Salary calculation ───────────────────────────────────────────────────────

_SALARY_BANDS: list[tuple[int, int]] = [
    (95, 45),  # Verstappen-tier
    (90, 30),  # Hamilton/Leclerc-tier
    (85, 20),  # Race winner
    (80, 14),  # Mid-field regular
    (75, 10),  # Lower mid-field
    (0,   6),  # Rookie/reserve
]


def _base_salary(driver) -> int:
    avg = (driver.pace + driver.racecraft) // 2
    for min_rating, salary in _SALARY_BANDS:
        if avg >= min_rating:
            return salary
    return 6


# ─── Team budget tiers ────────────────────────────────────────────────────────

_TEAM_BUDGETS: dict[str, int] = {
    "top":   90,   # Combined two-driver budget €M
    "mid":   60,
    "lower": 35,
}


def _team_tier(car_performance: int) -> str:
    if car_performance >= 93:
        return "top"
    if car_performance >= 83:
        return "mid"
    return "lower"


# ─── DriverMarket ─────────────────────────────────────────────────────────────

class DriverMarket:
    """
    Manages driver contracts and the seasonal transfer window.

    At season end:
      market.advance_season()      # opens window, clears expired contracts
      market.run_ai_transfers()    # AI teams sign free agents
      market.close_transfer_window()  # emergency contracts for unsigned drivers

    Player action:
      err = market.make_offer(player_team_id, driver_id, salary, years)
      if err: print("Offer rejected:", err)
    """

    def __init__(self, season: int = 2027, drivers: list = None, teams: list = None,
                 rng: random.Random = None):
        self._rng       = rng or random.Random()
        self.season     = season
        self.is_open    = False

        self._drivers   = {d.id: d for d in (drivers or [])}
        self._teams     = {t.id: t for t in (teams or [])}

        self.contracts: list[DriverContract]  = []
        self.closed_offers: list[TransferOffer] = []

        self.team_budgets: dict[int, int] = {}   # max combined salary
        self.team_spent:   dict[int, int] = {}   # currently committed

        if drivers and teams:
            self._init_contracts()
            self._init_budgets()

    # ── Initialisation ────────────────────────────────────────────────────────

    def _init_contracts(self) -> None:
        for driver in self._drivers.values():
            salary   = _base_salary(driver)
            duration = self._rng.randint(1, 3)
            contract = DriverContract(
                driver_id=driver.id,
                team_id=driver.team_id,
                duration_years=duration,
                salary_m=salary,
                season_start=self.season,
                season_end=self.season + duration,
                has_veto=(salary >= 25),
            )
            self.contracts.append(contract)

    def _init_budgets(self) -> None:
        for team in self._teams.values():
            tier = _team_tier(team.car_performance)
            self.team_budgets[team.id] = _TEAM_BUDGETS[tier]
            self.team_spent[team.id]   = sum(
                c.salary_m for c in self.contracts if c.team_id == team.id)

    # ── Lookup helpers ────────────────────────────────────────────────────────

    def get_contract(self, driver_id: int) -> Optional[DriverContract]:
        return next((c for c in self.contracts if c.driver_id == driver_id), None)

    def get_team_contracts(self, team_id: int) -> list[DriverContract]:
        return [c for c in self.contracts if c.team_id == team_id]

    def remaining_budget(self, team_id: int) -> int:
        return self.team_budgets.get(team_id, 20) - self.team_spent.get(team_id, 0)

    def market_value(self, driver_id: int) -> Optional[DriverMarketValue]:
        driver = self._drivers.get(driver_id)
        if driver is None:
            return None
        base    = _base_salary(driver)
        contract = self.get_contract(driver_id)
        free     = contract is None or contract.is_expired(self.season)

        # Count AI teams interested (have budget, have an open seat)
        interest = sum(
            1 for t in self._teams.values()
            if (t.id != getattr(driver, "team_id", -1)
                and self.remaining_budget(t.id) >= int(base * 0.8)
                and len(self.get_team_contracts(t.id)) < 2)
        )

        return DriverMarketValue(
            driver_id=driver_id,
            base_salary_m=base,
            min_salary_m=int(base * 0.75),
            is_available=free,
            interest_count=min(interest, 3),
        )

    # ── Transfer window control ───────────────────────────────────────────────

    def open_transfer_window(self) -> None:
        self.is_open = True
        self.contracts = [c for c in self.contracts if not c.is_expired(self.season)]
        # Recalculate spent after removals
        for team_id in self._teams:
            self.team_spent[team_id] = sum(
                c.salary_m for c in self.contracts if c.team_id == team_id)

    def close_transfer_window(self) -> None:
        self.is_open = False
        self._sign_emergency_contracts()

    def advance_season(self) -> None:
        self.season += 1
        self.open_transfer_window()

    # ── Player offer ──────────────────────────────────────────────────────────

    def make_offer(self, player_team_id: int, driver_id: int,
                   salary_m: int, duration_years: int) -> Optional[str]:
        """
        Makes a player offer to sign a driver.
        Returns None on success, or an error string on failure.
        """
        if not self.is_open:
            return "Transfer window is closed."

        driver = self._drivers.get(driver_id)
        if driver is None:
            return "Driver not found."

        mv = self.market_value(driver_id)
        if not mv or not mv.is_available:
            return "Driver is under contract."

        if salary_m < mv.min_salary_m:
            return f"Offer too low — driver minimum is €{mv.min_salary_m}M."

        if len(self.get_team_contracts(player_team_id)) >= 2:
            return "Team already has two drivers signed."

        if salary_m > self.remaining_budget(player_team_id):
            rem = self.remaining_budget(player_team_id)
            return f"Insufficient budget — €{rem}M remaining."

        # Roll for acceptance
        accept_p = self._acceptance_probability(driver_id, player_team_id, salary_m)
        accepted = self._rng.random() < accept_p

        offer = TransferOffer(
            offering_team_id=player_team_id,
            driver_id=driver_id,
            salary_m=salary_m,
            duration_years=duration_years,
            is_player_offer=True,
            is_accepted=accepted,
            is_declined=not accepted,
            decline_reason="" if accepted else "Driver chose a different offer.",
        )
        self.closed_offers.append(offer)

        if accepted:
            self._sign_contract(driver_id, player_team_id, salary_m, duration_years)

        return None  # success

    # ── AI transfers ──────────────────────────────────────────────────────────

    def run_ai_transfers(self) -> list[TransferOffer]:
        """Simulates AI team transfer activity. Returns completed offer list."""
        completed: list[TransferOffer] = []

        # Free agents, ordered by quality
        free_agents = [
            d for d in self._drivers.values()
            if self.get_contract(d.id) is None
        ]
        free_agents.sort(key=lambda d: -(d.pace + d.racecraft))

        # Teams with open seats, ordered by car performance (best team first)
        teams_with_seats = [
            t for t in sorted(self._teams.values(), key=lambda t: -t.car_performance)
            if len(self.get_team_contracts(t.id)) < 2
        ]

        for team in teams_with_seats:
            open_seats = 2 - len(self.get_team_contracts(team.id))
            budget     = self.remaining_budget(team.id)

            for _ in range(open_seats):
                if not free_agents:
                    break
                candidate = next(
                    (d for d in free_agents if _base_salary(d) <= budget), None)
                if candidate is None:
                    break

                salary   = int(_base_salary(candidate) * (0.90 + self._rng.random() * 0.20))
                duration = self._rng.randint(1, 3)

                accept_p = self._acceptance_probability(candidate.id, team.id, salary)
                accepted = self._rng.random() < accept_p

                offer = TransferOffer(
                    offering_team_id=team.id,
                    driver_id=candidate.id,
                    salary_m=salary,
                    duration_years=duration,
                    is_accepted=accepted,
                    is_declined=not accepted,
                )
                completed.append(offer)
                self.closed_offers.append(offer)

                if accepted:
                    self._sign_contract(candidate.id, team.id, salary, duration)
                    budget -= salary
                    free_agents.remove(candidate)

        return completed

    # ── Display helpers ───────────────────────────────────────────────────────

    def free_agents(self) -> list:
        return sorted(
            [d for d in self._drivers.values() if self.get_contract(d.id) is None],
            key=lambda d: -(d.pace + d.racecraft)
        )

    def expiring_contracts(self) -> list:
        """Returns drivers whose contracts expire at end of this season."""
        expiring_ids = {c.driver_id for c in self.contracts
                       if c.season_end == self.season + 1}
        return [d for d in self._drivers.values() if d.id in expiring_ids]

    # ── Internal helpers ──────────────────────────────────────────────────────

    def _acceptance_probability(self, driver_id: int, team_id: int, salary_m: int) -> float:
        driver = self._drivers.get(driver_id)
        team   = self._teams.get(team_id)
        if driver is None or team is None:
            return 0.0
        mv = self.market_value(driver_id)
        if mv is None:
            return 0.0

        salary_ratio    = salary_m / mv.base_salary_m if mv.base_salary_m else 1.0
        base            = min(salary_ratio - 0.5, 1.0)  # 0 if half salary, 1 if full
        prestige_bonus  = (team.car_performance - 76) / (97 - 76) * 0.20
        rivalry_penalty = mv.interest_count * 0.05
        return max(0.0, min(1.0, base + prestige_bonus - rivalry_penalty))

    def _sign_contract(self, driver_id: int, team_id: int,
                       salary_m: int, duration_years: int) -> None:
        contract = DriverContract(
            driver_id=driver_id,
            team_id=team_id,
            duration_years=duration_years,
            salary_m=salary_m,
            season_start=self.season,
            season_end=self.season + duration_years,
            has_veto=(salary_m >= 25),
        )
        self.contracts.append(contract)
        self.team_spent[team_id] = self.team_spent.get(team_id, 0) + salary_m

    def _sign_emergency_contracts(self) -> None:
        for driver in self._drivers.values():
            if self.get_contract(driver.id) is not None:
                continue
            salary = int(_base_salary(driver) * 0.80)
            self._sign_contract(driver.id, driver.team_id, salary, 1)
