"""
Overtake resolution using the sigmoid formula from GDD §08_TUNING_OVERTAKE_TIRE_v0.md.

P_raw    = 1 / (1 + exp(-logit))
P(success) = min(P_raw, 0.85)

logit = w1*(skill_atk - skill_def)
      + w2*(pace_delta)
      + w3*(tire_age_delta)
      + w4*(DRS_flag)
      - w5*(track_overtake_difficulty)
      - w6*(defender_aggression)
"""

from __future__ import annotations
import math
import random
from dataclasses import dataclass
from typing import TYPE_CHECKING

from .models import TireCompound, DriverInstruction, OvertakeResult

if TYPE_CHECKING:
    from .models import CarState, RaceState
    from ..data.drivers import Driver
    from ..data.circuits import Circuit


# v0 weights — see tuning doc §1.3
W1_SKILL   = 0.04   # skill advantage contribution
W2_PACE    = 1.80   # pace delta (s/lap) contribution
W3_TIRE    = 0.05   # tire age delta (laps fresher) contribution
W4_DRS     = 1.50   # DRS bonus (flat)
W5_TRACK   = 3.00   # track overtake difficulty penalty
W6_DEFEND  = 2.00   # defender aggression penalty

P_CAP = 0.85        # Hard cap: preserved by producer decision 2026-04-25


def _defender_aggression(car: "CarState", driver_defending: "Driver") -> float:
    """
    0.0 = completely passive; 1.0 = all-out defense.
    Derived from instruction + morale.
    """
    base = 0.3
    if car.instruction == DriverInstruction.DEFEND:
        base = 0.85
    elif car.instruction == DriverInstruction.ATTACK:
        base = 0.15
    # Morale modifier: low morale reduces willingness to fight
    morale_factor = driver_defending.morale / 100.0
    return min(1.0, base * morale_factor * 1.1)


def _pace_delta(attacker: "CarState", defender: "CarState") -> float:
    """
    Positive = attacker faster (in seconds per lap).
    Computed from last lap times, capped ±3s.
    """
    if attacker.last_lap_time_s <= 0 or defender.last_lap_time_s <= 0:
        return 0.0
    delta = defender.last_lap_time_s - attacker.last_lap_time_s
    return max(-3.0, min(3.0, delta))


def _drs_available(
    attacker: "CarState",
    gap_s: float,
    circuit: "Circuit",
    lap: int,
    total_laps: int,
) -> bool:
    """DRS: attacker must be within 1 second behind, circuit must have DRS zones, lap >= 2."""
    return (
        circuit.drs_zones > 0
        and lap >= 2
        and 0.0 < gap_s <= 1.0
    )


def compute_overtake_probability(
    attacker_car: "CarState",
    defender_car: "CarState",
    attacker_driver: "Driver",
    defender_driver: "Driver",
    circuit: "Circuit",
    gap_s: float,
    lap: int,
    total_laps: int,
    rng: random.Random,
) -> tuple[float, bool]:
    """
    Returns (P_success, drs_active).
    Only called when cars are within ~1.5s of each other.
    """
    skill_delta  = attacker_driver.racecraft - defender_driver.defending
    pace_d       = _pace_delta(attacker_car, defender_car)
    tire_delta   = attacker_car.tire_age_laps - defender_car.tire_age_laps  # positive = attacker older
    drs_on       = _drs_available(attacker_car, gap_s, circuit, lap, total_laps)
    track_diff   = circuit.overtake_difficulty
    agg_def      = _defender_aggression(defender_car, defender_driver)

    logit = (
        W1_SKILL  * skill_delta
        + W2_PACE  * pace_d
        + W3_TIRE  * (-tire_delta)   # negative tire_delta = attacker has fresher tires (+ve logit)
        + W4_DRS   * (1.0 if drs_on else 0.0)
        - W5_TRACK * track_diff
        - W6_DEFEND * agg_def
    )

    p_raw = 1.0 / (1.0 + math.exp(-logit))
    p_success = min(p_raw, P_CAP)
    return p_success, drs_on


def dirty_air_penalty_s(gap_to_ahead_s: float, circuit: "Circuit") -> float:
    """
    Additional lap-time penalty from following in aerodynamic wake.
    GDD §6.6.3 — diminishes with distance.
    High-downforce tracks (Monaco, Hungary) amplify this penalty.
    """
    if gap_to_ahead_s <= 0 or gap_to_ahead_s > 2.0:
        return 0.0

    df_multiplier = 1.0
    if circuit.overtake_difficulty > 0.7:  # High-downforce circuit
        df_multiplier = 1.3
    elif circuit.overtake_difficulty < 0.2:  # Low-downforce (Monza)
        df_multiplier = 0.6

    if gap_to_ahead_s <= 0.5:
        base = 0.30
    elif gap_to_ahead_s <= 1.0:
        base = 0.20
    else:
        base = 0.10

    return base * df_multiplier


def resolve_overtakes(
    race_state: "RaceState",
    drivers_by_id: dict,
    circuit: "Circuit",
    rng: random.Random,
) -> list[OvertakeResult]:
    """
    For every car that is within 1.5 seconds of the car ahead, resolve overtake attempt.
    Returns a list of OvertakeResult objects (for UI + event log).
    """
    sorted_cars = race_state.sorted_cars()
    results: list[OvertakeResult] = []

    for i in range(1, len(sorted_cars)):
        attacker = sorted_cars[i]
        defender = sorted_cars[i - 1]

        if attacker.dnf or defender.dnf:
            continue
        if attacker.is_pitting_this_lap or defender.is_pitting_this_lap:
            continue
        # Only attempt when within DRS range or very close
        if attacker.gap_to_ahead_s <= 0 or attacker.gap_to_ahead_s > 1.2:
            continue
        # Must be same lap (don't overtake lap-down cars trivially)
        if attacker.laps_completed != defender.laps_completed:
            continue
        # Require attacker to have a meaningful pace advantage or DRS
        pace_d = _pace_delta(attacker, defender)
        drs_on = _drs_available(attacker, attacker.gap_to_ahead_s, circuit, race_state.current_lap, race_state.total_laps)
        # Without DRS: must be at least 0.15s/lap faster to attempt
        # With DRS: 0.05s/lap advantage enough (straight-line speed boost)
        min_pace = 0.05 if drs_on else 0.15
        if pace_d < min_pace:
            continue

        att_driver = drivers_by_id.get(attacker.driver_id)
        def_driver = drivers_by_id.get(defender.driver_id)
        if not att_driver or not def_driver:
            continue

        p_success, drs_on = compute_overtake_probability(
            attacker, defender, att_driver, def_driver,
            circuit, attacker.gap_to_ahead_s,
            race_state.current_lap, race_state.total_laps, rng,
        )

        success = rng.random() < p_success
        result = OvertakeResult(
            attacker_id=attacker.driver_id,
            defender_id=defender.driver_id,
            lap=race_state.current_lap,
            success=success,
            p_success=p_success,
            was_drs=drs_on,
            new_position_attacker=attacker.position - 1 if success else attacker.position,
        )
        results.append(result)

    return results
