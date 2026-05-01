"""
Q1/Q2/Q3 qualifying simulation.

Produces a QualifyingResult with full session results and grid order.
Players interact via the UI between sessions (compound choice, track evolution).
"""

from __future__ import annotations
import random
from dataclasses import dataclass, field
from typing import Optional, TYPE_CHECKING

from .models import TireCompound
from .tire import TIRE_PROFILES
from .ai import qualifying_time

if TYPE_CHECKING:
    from ..data.circuits import Circuit
    from ..data.teams import Team
    from ..data.drivers import Driver


@dataclass
class QualifyingLap:
    driver_id: int
    time_s: float
    compound: TireCompound
    traffic_impeded: bool = False  # True if lap was ruined by traffic


@dataclass
class SessionResult:
    session: str        # "Q1", "Q2", "Q3"
    laps: list[QualifyingLap] = field(default_factory=list)
    eliminated: list[int] = field(default_factory=list)  # driver_ids knocked out

    def sorted_laps(self) -> list[QualifyingLap]:
        return sorted(self.laps, key=lambda l: l.time_s)


@dataclass
class QualifyingResult:
    q1: SessionResult
    q2: SessionResult
    q3: SessionResult
    grid_order: list[int]               # driver_id, fastest first
    q2_compound_map: dict[int, TireCompound]  # Top 10 must start on their Q2 compound


# Track evolution: as more cars set laps, grip improves.
# Simulated as a lap-count-based multiplier.
def _track_evo_factor(lap_index: int, total_laps_in_session: int) -> float:
    """Returns time reduction from track rubber. First car = 0, last = ~0.20s faster."""
    if total_laps_in_session <= 1:
        return 0.0
    progress = lap_index / max(1, total_laps_in_session - 1)
    return -0.20 * progress  # Negative = faster


def run_qualifying(
    circuit: "Circuit",
    drivers: dict[int, "Driver"],
    teams: dict[int, "Team"],
    player_team_id: int,
    rng: random.Random,
    player_q1_compounds: Optional[dict[int, TireCompound]] = None,
    player_q2_compounds: Optional[dict[int, TireCompound]] = None,
    player_q3_compounds: Optional[dict[int, TireCompound]] = None,
) -> QualifyingResult:
    """
    Simulate a full Q1/Q2/Q3 qualifying session.
    Player compound choices override AI defaults.
    Returns QualifyingResult with all session data.
    """
    # Softness order: softest = fastest in quali, fastest compound = smallest index in available
    avail_dry = sorted(
        [TireCompound(n) for n in circuit.available_compounds if TireCompound(n).is_dry],
        key=lambda c: c.value  # C3 < C4 < C5; softest = highest number
    )
    # Softest available compound = last in sorted (highest value = highest slip)
    softest = avail_dry[-1]     # e.g. C5
    medium  = avail_dry[-2] if len(avail_dry) >= 2 else softest  # e.g. C4
    # Compound pace bonus vs medium baseline:
    compound_bonus = {
        c: TIRE_PROFILES[c].grip_bonus_s for c in avail_dry if c in TIRE_PROFILES
    }
    # Softer = negative grip_bonus_s = faster. For qualifying, all drivers use softest available
    # unless player overrides.

    active_ids = list(drivers.keys())  # All 20

    def _sim_session(
        session_name: str,
        driver_ids: list[int],
        player_compound_map: Optional[dict[int, TireCompound]],
        default_compound: TireCompound,
        eliminate_n: int,
        track_base_evo: float = 0.0,
    ) -> SessionResult:
        result = SessionResult(session=session_name)
        n = len(driver_ids)

        # Determine compound per driver
        compound_map: dict[int, TireCompound] = {}
        for d_id in driver_ids:
            driver = drivers[d_id]
            team = teams.get(driver.team_id)
            if not team:
                continue
            if player_compound_map and d_id in player_compound_map:
                compound_map[d_id] = player_compound_map[d_id]
            elif team.id == player_team_id:
                compound_map[d_id] = default_compound  # AI default for player team
            else:
                # AI: 80% use softest, 20% use medium (strategic in Q2 for race start)
                if session_name == "Q2" and rng.random() < 0.20:
                    compound_map[d_id] = medium
                else:
                    compound_map[d_id] = softest

        # Shuffle lap order (random run order in session)
        run_order = driver_ids.copy()
        rng.shuffle(run_order)

        for i, d_id in enumerate(run_order):
            driver = drivers.get(d_id)
            team = teams.get(driver.team_id) if driver else None
            if not driver or not team:
                continue

            cmpd = compound_map.get(d_id, default_compound)
            cmpd_bonus = compound_bonus.get(cmpd, 0.0)  # negative = faster

            # Traffic on Monaco/Singapore: chance of impeded lap
            traffic_chance = circuit.overtake_difficulty * 0.15 if circuit.is_street_circuit else 0.05
            traffic_impeded = rng.random() < traffic_chance
            traffic_penalty = rng.uniform(0.2, 0.8) if traffic_impeded else 0.0

            # Track evolution: later laps = faster track
            evo = track_base_evo + _track_evo_factor(i, n)

            t = qualifying_time(
                circuit.base_lap_time_s,
                team.car_performance,
                driver.pace,
                circuit.circuit_length_km,
                rng,
                power_unit=team.power_unit,
                chassis=team.chassis,
                power_sensitivity=circuit.power_sensitivity,
                compound_bonus_s=cmpd_bonus,
            ) + traffic_penalty + evo

            result.laps.append(QualifyingLap(
                driver_id=d_id,
                time_s=t,
                compound=cmpd,
                traffic_impeded=traffic_impeded,
            ))

        # Sort and eliminate slowest
        result.laps.sort(key=lambda l: l.time_s)
        if eliminate_n > 0:
            result.eliminated = [l.driver_id for l in result.laps[-eliminate_n:]]
        return result

    # Q1: all 20 → 5 eliminated
    q1 = _sim_session("Q1", active_ids, player_q1_compounds, softest, eliminate_n=5)
    q1_survivors = [l.driver_id for l in q1.sorted_laps()[:15]]

    # Q2: top 15 → 5 eliminated
    q2 = _sim_session("Q2", q1_survivors, player_q2_compounds, softest, eliminate_n=5, track_base_evo=-0.05)
    q2_survivors = [l.driver_id for l in q2.sorted_laps()[:10]]
    q2_compound_map = {l.driver_id: l.compound for l in q2.laps}

    # Q3: top 10 → final grid positions 1-10
    q3 = _sim_session("Q3", q2_survivors, player_q3_compounds, softest, eliminate_n=0, track_base_evo=-0.10)
    q3_order = [l.driver_id for l in q3.sorted_laps()]

    # Full grid: Q3 order + Q2 eliminees (P11-15 in their Q2 order) + Q1 eliminees (P16-20 in Q1 order)
    q2_positions = [l.driver_id for l in q2.sorted_laps() if l.driver_id in q2.eliminated]
    q1_positions = [l.driver_id for l in q1.sorted_laps() if l.driver_id in q1.eliminated]
    grid_order = q3_order + q2_positions + q1_positions

    return QualifyingResult(
        q1=q1, q2=q2, q3=q3,
        grid_order=grid_order,
        q2_compound_map=q2_compound_map,
    )
