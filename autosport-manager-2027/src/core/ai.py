"""AI strategy engine — pit timing, compound selection, driver instructions."""

from __future__ import annotations
import random
from typing import TYPE_CHECKING

from .models import TireCompound, DriverInstruction, TirePhase, WeatherCondition
from .tire import TIRE_PROFILES, get_tire_phase, optimal_tire_window_remaining, best_compound_for_stint

if TYPE_CHECKING:
    from .models import CarState, RaceState
    from ..data.circuits import Circuit
    from ..data.drivers import Driver


class AIStrategyEngine:
    """
    Decides when and how each AI-controlled car should pit, plus per-lap instructions.
    Called once per lap for each non-player car.
    """

    def __init__(self, circuit: "Circuit", available_compounds: list[TireCompound], rng: random.Random):
        self.circuit = circuit
        self.available_dry = [c for c in available_compounds if c.is_dry]
        self.rng = rng

    # ── Pit decision ─────────────────────────────────────────────────────────

    def should_pit(
        self,
        car: "CarState",
        race: "RaceState",
        driver: "Driver",
    ) -> bool:
        laps_remaining = race.total_laps - race.current_lap
        profile = TIRE_PROFILES.get(car.tire_compound)
        if profile is None:
            return False

        deg_mult = self.circuit.tire_deg_multiplier
        phase = get_tire_phase(profile, car.tire_age_laps, deg_mult)
        window = optimal_tire_window_remaining(profile, car.tire_age_laps, deg_mult)

        # Must pit: tire is in cliff
        if phase == TirePhase.CLIFF:
            return True

        # Mandatory compound rule: must use 2 different dry compounds in a dry race.
        # Only trigger if there IS an alternative compound to switch to.
        if len(set(car.compounds_used)) < 2 and laps_remaining <= 10:
            alternatives = [c for c in self.available_dry if c not in set(car.compounds_used)]
            if alternatives:
                return True

        # Pit if we're about to enter cliff and won't finish on current tires
        if window <= 2 and laps_remaining > 5:
            return True

        # Safety-car opportunity: pit if it's strategically beneficial
        if race.safety_car.value == "DEPLOYED" and car.tire_age_laps >= 8:
            # Pit under SC: free stop, always good if we haven't pitted yet this stint
            if laps_remaining > 8:
                return self.rng.random() < 0.85  # 85% chance AI takes the SC window

        # Fuel check: can we make it to the end?
        fuel_needed = laps_remaining * self.circuit.fuel_consumption_kg
        if car.fuel_kg < fuel_needed * 0.92:
            # Going to run dry before finish — MUST pit
            return True

        # Weather-driven pit: switch to correct compound
        if race.weather != WeatherCondition.DRY and car.tire_compound.is_dry:
            # On dry tires in rain: pit immediately
            return True
        if race.weather == WeatherCondition.DRY and not car.tire_compound.is_dry:
            # On wet tires in dry: window to pit if track dry long enough
            drying_laps = race.current_lap - (
                getattr(race, '_rain_end_lap', race.current_lap)
            )
            if drying_laps >= 2:
                return True

        return False

    # ── Compound selection ────────────────────────────────────────────────────

    def choose_compound(
        self,
        car: "CarState",
        race: "RaceState",
    ) -> TireCompound:
        laps_remaining = race.total_laps - race.current_lap

        # Weather overrides
        if race.weather in (WeatherCondition.HEAVY_RAIN,):
            return TireCompound.WET
        if race.weather in (WeatherCondition.LIGHT_RAIN, WeatherCondition.DRYING):
            return TireCompound.INTER

        compounds_used = set(car.compounds_used)

        # Mandatory rule: if haven't satisfied two-compound requirement, pick something different
        if len(compounds_used) < 2:
            alternatives = [c for c in self.available_dry if c not in compounds_used]
            if alternatives and laps_remaining <= 15:
                # Pick hardest alternative (most durable for remaining laps)
                return sorted(alternatives, key=lambda c: TIRE_PROFILES[c].plateau_laps, reverse=True)[0]

        # Use best compound that won't cliff before the end
        deg_mult = self.circuit.tire_deg_multiplier
        best = best_compound_for_stint(self.available_dry, laps_remaining, deg_mult)
        # Safety: never return current compound if we still need to fulfil mandatory rule
        if len(compounds_used) < 2 and best in compounds_used:
            alternatives = [c for c in self.available_dry if c not in compounds_used]
            if alternatives:
                return alternatives[0]
        return best

    # ── Pit stop duration ─────────────────────────────────────────────────────

    def pit_stop_duration_s(self, team_pit_skill: int, rng: random.Random) -> float:
        """
        GDD §6.2.3: realistic pit stop time based on team crew skill.
        Morale-based model:
          skill 90+: 1.92–2.05s
          skill 80-89: 2.10–2.40s
          skill 70-79: 2.40–2.80s
          skill <70: 2.80–4.00s
        0.5% chance of critical error (4.0–6.0s).
        """
        if rng.random() < 0.005:
            return rng.uniform(4.0, 6.0)  # Critical error

        if team_pit_skill >= 90:
            return rng.uniform(1.92, 2.05)
        elif team_pit_skill >= 80:
            return rng.uniform(2.10, 2.40)
        elif team_pit_skill >= 70:
            return rng.uniform(2.40, 2.80)
        else:
            return rng.uniform(2.80, 4.00)

    # ── Driver instruction ────────────────────────────────────────────────────

    def choose_instruction(
        self,
        car: "CarState",
        race: "RaceState",
        driver: "Driver",
    ) -> DriverInstruction:
        laps_remaining = race.total_laps - race.current_lap
        deg_mult = self.circuit.tire_deg_multiplier
        profile = TIRE_PROFILES.get(car.tire_compound)
        phase = get_tire_phase(profile, car.tire_age_laps, deg_mult) if profile else TirePhase.PLATEAU
        window = optimal_tire_window_remaining(profile, car.tire_age_laps, deg_mult) if profile else 20

        # Aggressive final push when no fuel/tire concerns
        if laps_remaining <= 5 and phase != TirePhase.CLIFF:
            return DriverInstruction.ATTACK

        # Save fuel if running tight
        fuel_needed = laps_remaining * self.circuit.fuel_consumption_kg
        if car.fuel_kg < fuel_needed * 0.98 and car.pit_stop_count >= 1:
            return DriverInstruction.FUEL_SAVE

        # Manage tires when approaching cliff
        if window <= 3:
            return DriverInstruction.MANAGE

        # Attack when on fresh tires with pace advantage
        if car.tire_age_laps <= 3 and car.position > 1:
            return DriverInstruction.ATTACK

        # Defend when being closely followed
        if car.gap_to_ahead_s < 0.8 and car.position < 5:
            return DriverInstruction.DEFEND

        return DriverInstruction.MANAGE

    # ── Fuel load at pit stop ─────────────────────────────────────────────────

    def fuel_to_add_kg(self, car: "CarState", race: "RaceState") -> float:
        """
        Calculates how much fuel to add at the pit stop.
        Adds enough for remaining laps plus a small safety buffer.
        """
        laps_remaining = race.total_laps - race.current_lap
        needed = laps_remaining * self.circuit.fuel_consumption_kg
        # Safety margin + account for fuel already on board
        to_add = max(0.0, needed - car.fuel_kg + 3.0)
        return min(30.0, to_add)  # Max realistic top-up is ~30 kg


def qualifying_time(
    base_lap_time_s: float,
    car_performance: int,
    driver_pace: int,
    circuit_len_km: float,
    rng: random.Random,
) -> float:
    """
    Simulates a qualifying lap time for grid position.
    Faster cars/drivers = smaller time = higher grid slot.
    """
    # Car performance penalty: 97-rated car is ~0.12s off absolute best
    car_gap  = (100 - car_performance) * 0.05
    # Driver skill contributes ~0.5s across full range (50-100)
    drv_gain = (driver_pace - 75) * 0.012
    # Small random variation ±0.10s (tyre prep, traffic, perfect lap variance)
    noise = rng.uniform(-0.10, 0.10)
    return base_lap_time_s + car_gap - drv_gain + noise
