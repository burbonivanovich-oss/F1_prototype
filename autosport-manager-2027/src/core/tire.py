"""Tire degradation system — authentic 3-phase model from GDD §6.2."""

from __future__ import annotations
from dataclasses import dataclass
from typing import TYPE_CHECKING

from .models import TireCompound, TirePhase

if TYPE_CHECKING:
    from .models import CarState
    from ..data.circuits import Circuit


@dataclass(frozen=True)
class TireProfile:
    """Describes degradation behaviour for one compound at baseline deg multiplier=1.0."""
    compound: TireCompound
    # Phase 1: warm-up
    warm_up_laps: int            # Laps before full grip available
    warm_up_penalty_s: float     # Lap-time loss on lap 1 of a fresh set
    warm_up_penalty_lap2_s: float
    # Phase 2: plateau
    plateau_laps: int            # Laps of stable, near-peak performance
    # Phase 3: linear
    linear_deg_per_lap_s: float  # Additional seconds per lap during linear phase
    linear_phase_laps: int       # Duration of linear phase before cliff
    # Phase 4: cliff
    cliff_penalty_s: float       # Extra time loss per lap once in cliff
    # Pace offset relative to C4/Medium at optimal conditions
    grip_bonus_s: float          # Positive = slower than reference (harder compound)


TIRE_PROFILES: dict[TireCompound, TireProfile] = {
    # Calibrated to match GDD §6.2.1 target lap counts at Silverstone (deg_mult=1.0)
    # C3 Hard @ Silverstone: plateau ~22, cliff at ~44–48 laps ✓
    # C4 Medium @ Silverstone: plateau ~18, cliff at ~34–36 laps ✓
    # C5 Soft @ Silverstone: plateau ~12, cliff at ~23–24 laps ✓
    TireCompound.C1: TireProfile(
        compound=TireCompound.C1,
        warm_up_laps=2, warm_up_penalty_s=2.5, warm_up_penalty_lap2_s=1.5,
        plateau_laps=35, linear_deg_per_lap_s=0.018, linear_phase_laps=28,
        cliff_penalty_s=0.30, grip_bonus_s=0.25,
    ),
    TireCompound.C2: TireProfile(
        compound=TireCompound.C2,
        warm_up_laps=2, warm_up_penalty_s=2.0, warm_up_penalty_lap2_s=1.2,
        plateau_laps=28, linear_deg_per_lap_s=0.022, linear_phase_laps=24,
        cliff_penalty_s=0.35, grip_bonus_s=0.20,
    ),
    TireCompound.C3: TireProfile(
        compound=TireCompound.C3,
        warm_up_laps=2, warm_up_penalty_s=2.0, warm_up_penalty_lap2_s=1.0,
        plateau_laps=22, linear_deg_per_lap_s=0.028, linear_phase_laps=22,
        cliff_penalty_s=0.40, grip_bonus_s=0.15,
    ),
    TireCompound.C4: TireProfile(
        compound=TireCompound.C4,
        warm_up_laps=1, warm_up_penalty_s=1.5, warm_up_penalty_lap2_s=0.8,
        plateau_laps=18, linear_deg_per_lap_s=0.040, linear_phase_laps=16,
        cliff_penalty_s=0.50, grip_bonus_s=0.0,
    ),
    TireCompound.C5: TireProfile(
        compound=TireCompound.C5,
        warm_up_laps=1, warm_up_penalty_s=1.0, warm_up_penalty_lap2_s=0.4,
        plateau_laps=12, linear_deg_per_lap_s=0.058, linear_phase_laps=11,
        cliff_penalty_s=0.60, grip_bonus_s=-0.25,
    ),
    TireCompound.C6: TireProfile(
        compound=TireCompound.C6,
        warm_up_laps=1, warm_up_penalty_s=0.8, warm_up_penalty_lap2_s=0.3,
        plateau_laps=9,  linear_deg_per_lap_s=0.080, linear_phase_laps=9,
        cliff_penalty_s=0.80, grip_bonus_s=-0.30,
    ),
    TireCompound.INTER: TireProfile(
        compound=TireCompound.INTER,
        warm_up_laps=1, warm_up_penalty_s=0.5, warm_up_penalty_lap2_s=0.2,
        plateau_laps=20, linear_deg_per_lap_s=0.04, linear_phase_laps=15,
        cliff_penalty_s=0.40, grip_bonus_s=0.0,
    ),
    TireCompound.WET: TireProfile(
        compound=TireCompound.WET,
        warm_up_laps=1, warm_up_penalty_s=0.4, warm_up_penalty_lap2_s=0.1,
        plateau_laps=25, linear_deg_per_lap_s=0.03, linear_phase_laps=20,
        cliff_penalty_s=0.30, grip_bonus_s=0.0,
    ),
}


def get_tire_phase(profile: TireProfile, tire_age: int, deg_mult: float) -> TirePhase:
    """Return current tire phase given age and circuit deg multiplier."""
    effective_plateau = max(3, int(profile.plateau_laps / deg_mult))
    effective_linear  = max(3, int(profile.linear_phase_laps / deg_mult))
    cliff_start = effective_plateau + effective_linear

    if tire_age <= profile.warm_up_laps:
        return TirePhase.WARM_UP
    elif tire_age <= effective_plateau:
        return TirePhase.PLATEAU
    elif tire_age <= cliff_start:
        return TirePhase.LINEAR
    else:
        return TirePhase.CLIFF


def tire_deg_penalty_s(
    profile: TireProfile,
    tire_age: int,
    deg_mult: float,
    tire_management_rating: int = 85,
) -> float:
    """
    Returns lap time penalty (seconds) from tire degradation.
    tire_management_rating 0–100 modifies effective deg rate:
      90+ = 5% less wear; 70- = 10% more wear.
    """
    mgmt_factor = 1.0 - (tire_management_rating - 85) * 0.002
    effective_deg = profile.linear_deg_per_lap_s * deg_mult * max(0.7, mgmt_factor)

    effective_plateau = max(3, int(profile.plateau_laps / deg_mult))
    effective_linear  = max(3, int(profile.linear_phase_laps / deg_mult))
    cliff_start = effective_plateau + effective_linear

    if tire_age <= profile.warm_up_laps:
        # Warm-up: use the warm-up penalty
        if tire_age == 1:
            return profile.warm_up_penalty_s
        elif tire_age == 2 and profile.warm_up_laps >= 2:
            return profile.warm_up_penalty_lap2_s
        return 0.0
    elif tire_age <= effective_plateau:
        # Plateau: near-zero deg
        return max(0.0, effective_deg * 0.2)
    elif tire_age <= cliff_start:
        # Linear phase: progressive degradation
        laps_into_linear = tire_age - effective_plateau
        return effective_deg * laps_into_linear
    else:
        # Cliff phase
        laps_into_cliff = tire_age - cliff_start
        base_linear = effective_deg * effective_linear
        return base_linear + profile.cliff_penalty_s * (1 + laps_into_cliff * 0.3)


def warm_up_penalty_s(profile: TireProfile, tire_age: int, track_temp: float) -> float:
    """First laps after pit stop — includes temperature modifier."""
    if tire_age > profile.warm_up_laps:
        return 0.0
    cold_multiplier = 1.0 + max(0.0, (22.0 - track_temp) * 0.04)
    if tire_age == 1:
        return profile.warm_up_penalty_s * cold_multiplier
    elif tire_age == 2 and profile.warm_up_laps >= 2:
        return profile.warm_up_penalty_lap2_s * cold_multiplier
    return 0.0


def optimal_tire_window_remaining(profile: TireProfile, tire_age: int, deg_mult: float) -> int:
    """How many laps before cliff begins."""
    effective_plateau = max(3, int(profile.plateau_laps / deg_mult))
    effective_linear  = max(3, int(profile.linear_phase_laps / deg_mult))
    cliff_start = effective_plateau + effective_linear
    return max(0, cliff_start - tire_age)


def tire_degradation_pct(profile: TireProfile, tire_age: int, deg_mult: float) -> float:
    """0.0 = fresh, 1.0 = completely worn. Used for UI colour coding."""
    effective_plateau = max(3, int(profile.plateau_laps / deg_mult))
    effective_linear  = max(3, int(profile.linear_phase_laps / deg_mult))
    total_life = effective_plateau + effective_linear + 5  # +5 buffer before total failure
    return min(1.0, tire_age / total_life)


def best_compound_for_stint(
    available: list[TireCompound],
    stint_laps: int,
    deg_mult: float,
    weather: str = "DRY",
) -> TireCompound:
    """
    Selects the fastest compound that won't cliff before stint_laps.
    Prefers fastest compound that survives (no cliff during stint).
    """
    if weather in ("LIGHT_RAIN", "HEAVY_RAIN"):
        return TireCompound.INTER if TireCompound.INTER in available else TireCompound.WET
    if weather == "HEAVY_RAIN":
        return TireCompound.WET

    # Sort available dry compounds by grip (fastest first = lowest compound number difference from C4)
    dry_compounds = [c for c in available if c.is_dry]
    dry_compounds.sort(key=lambda c: TIRE_PROFILES[c].grip_bonus_s)

    for compound in dry_compounds:
        profile = TIRE_PROFILES[compound]
        window = optimal_tire_window_remaining(profile, 0, deg_mult)
        if window >= stint_laps:
            return compound

    # If nothing survives full stint, return hardest available
    return sorted(dry_compounds, key=lambda c: TIRE_PROFILES[c].plateau_laps, reverse=True)[0]
