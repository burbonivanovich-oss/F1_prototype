"""Car upgrade tree: tokens, R&D points, weekly performance growth, upgrade failure risk."""

from __future__ import annotations
import random
from dataclasses import dataclass, field
from enum import Enum, auto
from typing import Optional


# ─── Upgrade areas ────────────────────────────────────────────────────────────

class UpgradeArea(Enum):
    POWER_UNIT = auto()
    AERO_FRONT = auto()
    AERO_REAR  = auto()
    CHASSIS    = auto()
    SUSPENSION = auto()
    GEARBOX    = auto()
    COOLING    = auto()


# ─── Upgrade node ─────────────────────────────────────────────────────────────

@dataclass
class UpgradeNode:
    id:                  str
    area:                UpgradeArea
    tier:                int
    token_cost:          int
    rd_points_cost:      int
    weeks_to_develop:    int
    success_probability: float

    # Reward deltas
    car_perf_gain:    float = 0.0
    power_unit_gain:  float = 0.0
    chassis_gain:     float = 0.0
    reliability_gain: float = 0.0

    # Tree dependency
    requires: Optional[str] = None

    # Mutable state
    is_unlocked:      bool = False
    is_in_dev:        bool = False
    is_complete:      bool = False
    weeks_remaining:  int  = 0

    @property
    def total_impact(self) -> float:
        return (self.car_perf_gain
                + self.power_unit_gain * 0.5
                + self.chassis_gain    * 0.5
                + self.reliability_gain * 0.2)

    def describe(self) -> str:
        status = ""
        if self.is_complete:
            status = "[DONE]"
        elif self.is_in_dev:
            status = f"[{self.weeks_remaining}w left]"
        return (f"{self.id:<10} {self.area.name:<12} T{self.tier} "
                f"| {self.token_cost}T / {self.rd_points_cost}R&D "
                f"| {self.success_probability*100:.0f}% "
                f"| +{self.total_impact:.1f} perf {status}")


# ─── Development state per team ───────────────────────────────────────────────

@dataclass
class TeamDevState:
    team_id:            int
    tokens:             int  = 36
    rd_points:          int  = 0
    rd_per_week:        int  = 12

    car_perf_delta:    float = 0.0
    power_unit_delta:  float = 0.0
    chassis_delta:     float = 0.0
    reliability_delta: float = 0.0

    tree:          list[UpgradeNode] = field(default_factory=list)
    completed_ids: list[str]         = field(default_factory=list)
    active_ids:    list[str]         = field(default_factory=list)


@dataclass
class DevTickResult:
    completed: list[str] = field(default_factory=list)
    failed:    list[str] = field(default_factory=list)
    rd_earned: int       = 0
    summary:   str       = ""


# ─── Static upgrade tree ─────────────────────────────────────────────────────

def build_upgrade_tree() -> list[UpgradeNode]:
    """Returns the full upgrade tree for any team. Performance gains are team-agnostic."""
    return [
        # ── Power Unit ────────────────────────────────────────────────────────
        UpgradeNode("PU_T1",  UpgradeArea.POWER_UNIT, 1, 4, 60,  3, 0.90,
                    car_perf_gain=0.3, power_unit_gain=1.0),
        UpgradeNode("PU_T2",  UpgradeArea.POWER_UNIT, 2, 6, 100, 4, 0.80,
                    car_perf_gain=0.5, power_unit_gain=1.5, requires="PU_T1"),
        UpgradeNode("PU_T3",  UpgradeArea.POWER_UNIT, 3, 8, 160, 6, 0.65,
                    car_perf_gain=0.8, power_unit_gain=2.0, requires="PU_T2"),

        # ── Aero Front ────────────────────────────────────────────────────────
        UpgradeNode("AF_T1",  UpgradeArea.AERO_FRONT, 1, 3, 50,  2, 0.92,
                    car_perf_gain=0.4, chassis_gain=0.8),
        UpgradeNode("AF_T2",  UpgradeArea.AERO_FRONT, 2, 5, 90,  3, 0.80,
                    car_perf_gain=0.6, chassis_gain=1.2, requires="AF_T1"),
        UpgradeNode("AF_T3",  UpgradeArea.AERO_FRONT, 3, 7, 140, 5, 0.70,
                    car_perf_gain=0.9, chassis_gain=1.8, requires="AF_T2"),

        # ── Aero Rear ────────────────────────────────────────────────────────
        UpgradeNode("AR_T1",  UpgradeArea.AERO_REAR,  1, 3, 50,  2, 0.90,
                    car_perf_gain=0.3, chassis_gain=0.6),
        UpgradeNode("AR_T2",  UpgradeArea.AERO_REAR,  2, 5, 85,  3, 0.78,
                    car_perf_gain=0.5, chassis_gain=1.0, requires="AR_T1"),

        # ── Chassis ───────────────────────────────────────────────────────────
        UpgradeNode("CH_T1",  UpgradeArea.CHASSIS,    1, 5, 80,  4, 0.85,
                    car_perf_gain=1.0, chassis_gain=0.5),
        UpgradeNode("CH_T2",  UpgradeArea.CHASSIS,    2, 8, 130, 5, 0.72,
                    car_perf_gain=1.5, chassis_gain=1.0, requires="CH_T1"),

        # ── Suspension ───────────────────────────────────────────────────────
        UpgradeNode("SUS_T1", UpgradeArea.SUSPENSION, 1, 3, 55,  2, 0.90,
                    car_perf_gain=0.4, reliability_gain=1.0),
        UpgradeNode("SUS_T2", UpgradeArea.SUSPENSION, 2, 5, 90,  3, 0.80,
                    car_perf_gain=0.6, reliability_gain=1.5, requires="SUS_T1"),

        # ── Gearbox ──────────────────────────────────────────────────────────
        UpgradeNode("GB_T1",  UpgradeArea.GEARBOX,    1, 2, 40,  2, 0.92,
                    car_perf_gain=0.2, reliability_gain=2.0),
        UpgradeNode("GB_T2",  UpgradeArea.GEARBOX,    2, 4, 70,  3, 0.82,
                    car_perf_gain=0.3, reliability_gain=2.5, requires="GB_T1"),

        # ── Cooling ──────────────────────────────────────────────────────────
        UpgradeNode("COOL_T1",UpgradeArea.COOLING,    1, 2, 35,  2, 0.93,
                    car_perf_gain=0.1, reliability_gain=3.0),
    ]


# ─── CarDevelopmentSystem ─────────────────────────────────────────────────────

class CarDevelopmentSystem:
    """
    Manages R&D and upgrade trees for all teams.

    Call tick_week(team_id) once per race round to advance development.
    Call start_development(team_id, upgrade_id) to begin an upgrade.
    """

    # Token allocations by performance tier
    _TOKEN_TIERS = {"top": 48, "mid": 36, "lower": 28}
    # R&D rate by tier (points per week)
    _RD_RATES    = {"top": 22, "mid": 14, "lower": 8}

    def __init__(self, teams: list = None, rng: random.Random = None):
        self._rng    = rng or random.Random()
        self._states: dict[int, TeamDevState] = {}
        if teams:
            for team in teams:
                self._init_team(team)

    # ── Initialisation ────────────────────────────────────────────────────────

    def _init_team(self, team) -> None:
        perf = team.car_performance
        tier = "top" if perf >= 93 else "mid" if perf >= 83 else "lower"

        state = TeamDevState(
            team_id    = team.id,
            tokens     = self._TOKEN_TIERS[tier],
            rd_per_week= self._RD_RATES[tier],
        )
        state.tree = build_upgrade_tree()
        self._refresh_unlocks(state)
        self._states[team.id] = state

    def get_state(self, team_id: int) -> TeamDevState | None:
        return self._states.get(team_id)

    # ── Player actions ────────────────────────────────────────────────────────

    def start_development(self, team_id: int, upgrade_id: str) -> tuple[bool, str]:
        """
        Starts development on an upgrade.
        Returns (success, error_message). error_message is empty on success.
        """
        state = self.get_state(team_id)
        if state is None:
            return False, "Invalid team."

        node = next((n for n in state.tree if n.id == upgrade_id), None)
        if node is None:
            return False, "Upgrade not found."
        if not node.is_unlocked:
            return False, "Prerequisites not met."
        if node.is_in_dev or node.is_complete:
            return False, "Already in progress or completed."
        if state.tokens < node.token_cost:
            return False, f"Need {node.token_cost} tokens ({state.tokens} available)."
        if state.rd_points < node.rd_points_cost:
            return False, f"Need {node.rd_points_cost} R&D points ({state.rd_points} available)."

        state.tokens    -= node.token_cost
        state.rd_points -= node.rd_points_cost
        node.is_in_dev       = True
        node.weeks_remaining = node.weeks_to_develop
        state.active_ids.append(upgrade_id)
        return True, ""

    def set_rd_rate(self, team_id: int, points_per_week: int) -> None:
        """Override R&D accrual rate (e.g. based on budget reallocation)."""
        state = self.get_state(team_id)
        if state:
            state.rd_per_week = max(1, points_per_week)

    # ── Weekly tick ───────────────────────────────────────────────────────────

    def tick_week(self, team_id: int) -> DevTickResult:
        """Advances one team's development by one race week."""
        result = DevTickResult()
        state  = self.get_state(team_id)
        if state is None:
            return result

        # Accrue R&D points
        state.rd_points   += state.rd_per_week
        result.rd_earned   = state.rd_per_week

        # Small chance of receiving a bonus token
        if self._rng.random() < 1 / 6:
            state.tokens += 1

        # Advance in-progress upgrades
        to_remove: list[str] = []
        for uid in list(state.active_ids):
            node = next((n for n in state.tree if n.id == uid), None)
            if node is None:
                continue
            node.weeks_remaining -= 1
            if node.weeks_remaining > 0:
                continue

            # Development timer expired — roll for success
            node.is_in_dev = False
            success = self._rng.random() < node.success_probability

            if success:
                node.is_complete = True
                state.completed_ids.append(uid)
                self._apply_upgrade(state, node)
                result.completed.append(uid)
            else:
                result.failed.append(uid)

            to_remove.append(uid)

        for uid in to_remove:
            state.active_ids.remove(uid)

        self._refresh_unlocks(state)
        result.summary = self._build_summary(state, result)
        return result

    def tick_all_teams(self) -> dict[int, DevTickResult]:
        return {tid: self.tick_week(tid) for tid in self._states}

    # ── AI auto-development ───────────────────────────────────────────────────

    def auto_develop_ai(self, team_id: int) -> None:
        """AI teams automatically start the best available upgrade."""
        state = self.get_state(team_id)
        if state is None or state.active_ids:
            return
        candidates = [
            n for n in state.tree
            if (n.is_unlocked and not n.is_in_dev and not n.is_complete
                and n.token_cost <= state.tokens
                and n.rd_points_cost <= state.rd_points)
        ]
        if not candidates:
            return
        best = max(candidates, key=lambda n: n.total_impact)
        self.start_development(team_id, best.id)

    # ── Effective stats ───────────────────────────────────────────────────────

    def effective_car_performance(self, team_id: int, base_perf: int) -> float:
        state = self.get_state(team_id)
        return base_perf + (state.car_perf_delta if state else 0.0)

    def available_upgrades(self, team_id: int) -> list[UpgradeNode]:
        state = self.get_state(team_id)
        if not state:
            return []
        return sorted(
            [n for n in state.tree
             if n.is_unlocked and not n.is_in_dev and not n.is_complete],
            key=lambda n: (n.area.value, n.tier)
        )

    # ── Internal helpers ──────────────────────────────────────────────────────

    def _apply_upgrade(self, state: TeamDevState, node: UpgradeNode) -> None:
        state.car_perf_delta    += node.car_perf_gain
        state.power_unit_delta  += node.power_unit_gain
        state.chassis_delta     += node.chassis_gain
        state.reliability_delta += node.reliability_gain

    def _refresh_unlocks(self, state: TeamDevState) -> None:
        for node in state.tree:
            if node.is_complete or node.is_in_dev:
                continue
            node.is_unlocked = (node.requires is None
                                or node.requires in state.completed_ids)

    @staticmethod
    def _build_summary(state: TeamDevState, result: DevTickResult) -> str:
        parts: list[str] = []
        if result.completed:
            parts.append(f"Completed: {', '.join(result.completed)}")
        if result.failed:
            parts.append(f"FAILED: {', '.join(result.failed)}")
        parts.append(f"+{result.rd_earned} R&D → {state.rd_points} total")
        return " | ".join(parts)
