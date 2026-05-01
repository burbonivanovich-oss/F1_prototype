"""
Race simulation engine — lap-tick architecture.

Each call to simulate_lap() advances the race by exactly one lap for all cars.
The engine produces a RaceState snapshot after every lap for the UI to render.
"""

from __future__ import annotations
import random
import math
from dataclasses import dataclass
from typing import Optional, TYPE_CHECKING

from .models import (
    TireCompound, TirePhase, DriverInstruction, WeatherCondition,
    SafetyCarState, CarState, RaceState, RaceEvent, PitStopResult, OvertakeResult,
)
from .tire import (
    TIRE_PROFILES, get_tire_phase, tire_deg_penalty_s,
    tire_degradation_pct, warm_up_penalty_s,
)
from .weather import WeatherSystem
from .overtake import resolve_overtakes, dirty_air_penalty_s
from .ai import AIStrategyEngine

if TYPE_CHECKING:
    from ..data.circuits import Circuit
    from ..data.teams import Team
    from ..data.drivers import Driver


# Fuel weight penalty: 0.005s per kg above 0 (GDD §6.3)
FUEL_WEIGHT_PENALTY_PER_KG = 0.005

# Base crash probability per lap (GDD §6.7.1)
BASE_CRASH_PROB = 0.0015
# Safety car duration range (laps)
SC_MIN_LAPS = 3
SC_MAX_LAPS = 6

# Safety car lap time multiplier: everyone drives at ~60% normal pace
SC_PACE_MULTIPLIER = 1.65


class RaceEngine:
    """
    Central simulation object. Created once per race, then simulate_lap() is called
    repeatedly until race_state.is_race_complete.
    """

    def __init__(
        self,
        circuit: "Circuit",
        teams: dict[int, "Team"],
        drivers: dict[int, "Driver"],
        player_team_id: int,
        player_car_states: Optional[dict[int, dict]] = None,
        seed: Optional[int] = None,
    ):
        self.circuit = circuit
        self.teams = teams
        self.drivers = drivers
        self.player_team_id = player_team_id
        self.seed = seed or random.randint(0, 2**31)
        self.rng = random.Random(self.seed)

        # Determine available compounds for this circuit
        self.available_compounds = [
            TireCompound(n) for n in circuit.available_compounds
        ]

        # Weather subsystem
        self.weather_sys = WeatherSystem(
            circuit=circuit,
            rng=random.Random(self.rng.randint(0, 2**31)),
            total_laps=circuit.total_laps,
        )

        # AI strategy engine (shared; tracks circuit context)
        self.ai = AIStrategyEngine(circuit, self.available_compounds, random.Random(self.rng.randint(0, 2**31)))

        # Queue of player-commanded pit stops (driver_id: compound or None=auto)
        self._player_pit_commands: dict[int, Optional[TireCompound]] = {}
        # Player driver instructions (may be overridden each lap)
        self._player_instructions: dict[int, DriverInstruction] = {}

        # Build initial race state (qualifying grid + car states)
        self.race_state = self._build_initial_state(player_car_states)

    # ── Setup ────────────────────────────────────────────────────────────────

    def _build_initial_state(
        self, player_overrides: Optional[dict[int, dict]]
    ) -> RaceState:
        state = RaceState(
            circuit_name=self.circuit.name,
            total_laps=self.circuit.total_laps,
            current_lap=0,
            weather=self.weather_sys.condition,
            track_temp_c=self.weather_sys.track_temp_c,
            air_temp_c=self.weather_sys.air_temp_c,
            player_team_id=self.player_team_id,
        )

        # Qualifying simulation for grid order
        from .ai import qualifying_time
        grid_times: list[tuple[float, int]] = []
        for driver in self.drivers.values():
            if driver.team_id not in self.teams:
                continue
            team = self.teams[driver.team_id]
            q_time = qualifying_time(
                self.circuit.base_lap_time_s,
                team.car_performance,
                driver.pace,
                self.circuit.circuit_length_km,
                self.rng,
            )
            grid_times.append((q_time, driver.id))

        grid_times.sort(key=lambda x: x[0])

        # Create car states in grid order
        for grid_pos, (q_time, driver_id) in enumerate(grid_times, start=1):
            driver = self.drivers[driver_id]
            team   = self.teams[driver.team_id]

            # Default starting compound (player override possible)
            default_compound = self.available_compounds[-1]  # Softest available
            if player_overrides and driver_id in player_overrides:
                ov = player_overrides[driver_id]
                default_compound = ov.get("compound", default_compound)

            car = CarState(
                driver_id=driver_id,
                team_id=driver.team_id,
                car_number=driver.car_number,
                position=grid_pos,
                tire_compound=default_compound,
                fuel_kg=105.0,  # Max starting fuel
                compounds_used=[default_compound],
                instruction=DriverInstruction.MANAGE,
            )
            car.morale_modifier_s = -(driver.morale - 80) * 0.01  # ±0.2s range

            state.cars.append(car)

        # Initial weather forecast
        state.weather_forecast = self.weather_sys.forecast

        return state

    # ── Player commands ───────────────────────────────────────────────────────

    def command_pit(self, driver_id: int, compound: Optional[TireCompound] = None) -> None:
        """Player issues a 'box this lap' order. Compound=None = auto-select."""
        self._player_pit_commands[driver_id] = compound

    def command_instruction(self, driver_id: int, instruction: DriverInstruction) -> None:
        """Player sets driver instruction for next lap."""
        self._player_instructions[driver_id] = instruction

    # ── Main lap simulation ───────────────────────────────────────────────────

    def simulate_lap(self) -> RaceState:
        """Advance the race by one lap. Returns updated RaceState."""
        state = self.race_state
        state.current_lap += 1
        lap = state.current_lap

        if state.is_race_complete:
            return state

        # ── 1. Weather update ────────────────────────────────────────────────
        weather_msgs = self.weather_sys.advance(lap)
        state.weather = self.weather_sys.condition
        state.track_temp_c = self.weather_sys.track_temp_c
        state.air_temp_c = self.weather_sys.air_temp_c
        state.weather_forecast = self.weather_sys.forecast
        for msg in weather_msgs:
            state.events.append(RaceEvent(lap, "Weather", "WEATHER", msg))

        # ── 2. Safety car management ─────────────────────────────────────────
        self._update_safety_car(state)

        # ── 3. Clear pitting flags from last lap ─────────────────────────────
        for car in state.cars:
            car.is_pitting_this_lap = False
            car.pit_stop_duration_s = 0.0

        # ── 4. Apply player instructions ─────────────────────────────────────
        for driver_id, instr in self._player_instructions.items():
            for car in state.cars:
                if car.driver_id == driver_id and not car.dnf:
                    car.instruction = instr

        # ── 5. Execute pit stops (player commands + AI decisions) ─────────────
        pit_results = self._execute_pit_stops(state, lap)

        # ── 6. Simulate lap times ─────────────────────────────────────────────
        sorted_cars = state.sorted_cars()
        for i, car in enumerate(sorted_cars):
            if car.dnf:
                continue

            car.tire_age_laps += 1

            driver = self.drivers[car.driver_id]
            team   = self.teams[car.team_id]

            # Fuel consumption this lap
            fuel_multiplier = 1.0
            if car.instruction == DriverInstruction.FUEL_SAVE:
                fuel_multiplier = 0.88
            elif car.instruction == DriverInstruction.ATTACK:
                fuel_multiplier = 1.05
            fuel_consumed = self.circuit.fuel_consumption_kg * fuel_multiplier
            car.fuel_kg = max(0.1, car.fuel_kg - fuel_consumed)

            # Safety car: all cars drive slowly, no real racing
            if state.safety_car in (SafetyCarState.DEPLOYED, SafetyCarState.VSC):
                sc_mult = SC_PACE_MULTIPLIER if state.safety_car == SafetyCarState.DEPLOYED else 1.30
                base_time = self.circuit.base_lap_time_s * sc_mult
                car.last_lap_time_s = base_time + self.rng.uniform(-0.1, 0.1)
                car.total_race_time_s += car.last_lap_time_s
                car.laps_completed += 1
                continue

            lap_time = self._compute_lap_time(car, driver, team, state, i, sorted_cars)
            car.last_lap_time_s = lap_time
            car.total_race_time_s += lap_time
            car.laps_completed += 1

            # Track fastest lap
            if lap_time < state.fastest_lap_time_s:
                state.fastest_lap_time_s = lap_time
                state.fastest_lap_driver_id = car.driver_id
                state.fastest_lap_number = lap

            # Update tire phase for display
            profile = TIRE_PROFILES.get(car.tire_compound)
            if profile:
                car.tire_phase = get_tire_phase(
                    profile, car.tire_age_laps, self.circuit.tire_deg_multiplier
                )
                car.tire_deg_pct = tire_degradation_pct(
                    profile, car.tire_age_laps, self.circuit.tire_deg_multiplier
                )

        # ── 7. Resolve overtakes ──────────────────────────────────────────────
        self._compute_gaps(state)
        ot_results = resolve_overtakes(
            state, self.drivers, self.circuit, self.rng
        )
        self._apply_overtakes(state, ot_results)

        # ── 8. Check incidents / crashes ──────────────────────────────────────
        self._check_incidents(state, lap)

        # ── 9. Recompute positions & gaps ──────────────────────────────────────
        self._update_positions(state)
        self._compute_gaps(state)

        # ── 10. AI: choose next instruction ──────────────────────────────────
        self._update_ai_instructions(state)

        # ── 11. Add pit stop events ───────────────────────────────────────────
        for pit in pit_results:
            driver = self.drivers[pit.driver_id]
            team   = self.teams[self.drivers[pit.driver_id].team_id]
            is_player = (team.id == self.player_team_id)
            msg = (
                f"{driver.short_name} pits → {pit.new_compound.display_name} "
                f"({pit.duration_s:.2f}s stop). +{pit.fuel_added_kg:.0f} kg fuel."
            )
            state.events.append(RaceEvent(lap, driver.name, "PIT", msg, is_player_event=is_player))

        # ── 12. Add overtake events (only notable ones) ───────────────────────
        for ot in ot_results:
            if ot.success:
                att_d = self.drivers[ot.attacker_id]
                def_d = self.drivers[ot.defender_id]
                drs_tag = " [DRS]" if ot.was_drs else ""
                att_team = self.teams[att_d.team_id]
                is_player = (att_team.id == self.player_team_id)
                msg = f"{att_d.short_name} overtakes {def_d.short_name} for P{ot.new_position_attacker}{drs_tag}"
                state.events.append(RaceEvent(lap, att_d.name, "OVERTAKE", msg, is_player_event=is_player))

        # ── 13. Safety car trigger events ─────────────────────────────────────
        self._maybe_trigger_safety_car(state, lap)

        # ── 14. Race completion check ─────────────────────────────────────────
        active_cars = [c for c in state.cars if not c.dnf]
        if all(c.laps_completed >= state.total_laps for c in active_cars):
            state.is_race_complete = True
            self._finalize_race(state)

        # Clear consumed commands
        self._player_pit_commands.clear()

        return state

    # ── Lap time calculation ──────────────────────────────────────────────────

    def _compute_lap_time(
        self,
        car: "CarState",
        driver: "Driver",
        team: "Team",
        state: "RaceState",
        position_index: int,
        sorted_cars: list["CarState"],
    ) -> float:
        """
        Returns realistic lap time for this car on this lap.
        All penalties/bonuses are additive to base circuit lap time.
        """
        base = self.circuit.base_lap_time_s

        # ── Car performance: relative to the absolute fastest car (97-rated) ──
        car_gap = (100 - team.car_performance) * 0.055
        base += car_gap

        # ── Driver pace: ±0.6s spread from 50–100 skill ───────────────────────
        drv_bonus = (driver.pace - 75) * 0.012
        base -= drv_bonus

        # ── Morale modifier ───────────────────────────────────────────────────
        base += car.morale_modifier_s

        # ── Fuel weight: each kg = 0.005s ─────────────────────────────────────
        fuel_penalty = car.fuel_kg * FUEL_WEIGHT_PENALTY_PER_KG
        base += fuel_penalty

        # ── Tire degradation (3-phase) ─────────────────────────────────────────
        profile = TIRE_PROFILES.get(car.tire_compound)
        if profile:
            deg_mult = self.circuit.tire_deg_multiplier
            if car.is_pitting_this_lap:
                # Pitting this lap: old tire was used up to pit point (no cliff penalty added)
                tire_pen = tire_deg_penalty_s(
                    profile, max(1, car.tire_age_laps - 1), deg_mult,
                    driver.tire_management
                ) * 0.5
            else:
                tire_pen = tire_deg_penalty_s(
                    profile, car.tire_age_laps, deg_mult,
                    driver.tire_management
                )
            # Warm-up after new set
            warm_pen = warm_up_penalty_s(profile, car.tire_age_laps, state.track_temp_c)
            base += tire_pen + warm_pen

        # ── Tire compound grip relative to C4/Medium ─────────────────────────
        if profile and not car.is_pitting_this_lap:
            # grip_bonus_s is negative for softs (faster) positive for hards (slower)
            base -= profile.compound.grip_advantage_s  # subtract: faster=more negative

        # ── Weather penalty ───────────────────────────────────────────────────
        weather_pen = self.weather_sys.lap_time_weather_penalty_s(car.tire_compound)
        base += weather_pen

        # ── Driver instruction modifier ───────────────────────────────────────
        instr = car.instruction
        if instr == DriverInstruction.ATTACK:
            base -= 0.25
        elif instr == DriverInstruction.FUEL_SAVE:
            base += 0.12
        elif instr == DriverInstruction.MANAGE:
            base += 0.05
        # DEFEND: no pace change (focused on blocking)

        # ── Dirty air: following closely behind another car ───────────────────
        if position_index > 0:
            base += dirty_air_penalty_s(car.gap_to_ahead_s, self.circuit)

        # ── Car damage ────────────────────────────────────────────────────────
        base += car.damage_s

        # ── ERS / MGU-K deployment ────────────────────────────────────────────
        if car.ers_deploy_mode == "HIGH" and car.ers_charge_pct > 0.3:
            base -= 0.35
            car.ers_charge_pct = max(0.0, car.ers_charge_pct - 0.15)
        elif car.ers_deploy_mode == "MEDIUM":
            base -= 0.15
            car.ers_charge_pct = max(0.0, car.ers_charge_pct - 0.05)
        # Recharge from braking
        car.ers_charge_pct = min(1.0, car.ers_charge_pct + 0.08)

        # ── Small random variation: driver + track conditions ─────────────────
        # Consistency skill reduces variance
        variance = 0.12 * (1.0 - (driver.consistency - 60) / 80.0)
        base += self.rng.gauss(0, max(0.02, variance))

        # ── Pit stop time is added as full extra time to this lap ─────────────
        if car.is_pitting_this_lap:
            base += car.pit_stop_duration_s + 20.0  # 20s avg pit lane loss (speed limiter)

        return max(base, self.circuit.base_lap_time_s * 0.90)  # physical floor

    # ── Pit stops ─────────────────────────────────────────────────────────────

    def _execute_pit_stops(self, state: "RaceState", lap: int) -> list[PitStopResult]:
        results: list[PitStopResult] = []

        for car in state.cars:
            if car.dnf or car.laps_completed >= state.total_laps:
                continue

            driver = self.drivers[car.driver_id]
            team   = self.teams[car.team_id]
            is_player = (team.id == self.player_team_id)

            should_pit_flag = False
            chosen_compound: Optional[TireCompound] = None

            if is_player and car.driver_id in self._player_pit_commands:
                should_pit_flag = True
                chosen_compound = self._player_pit_commands[car.driver_id]
            elif not is_player:
                should_pit_flag = self.ai.should_pit(car, state, driver)

            if not should_pit_flag:
                continue

            # Double-stacking: if teammate already pitting this lap, add 3s delay
            extra_delay = 0.0
            same_team_pitting = [
                c for c in state.cars
                if c.team_id == car.team_id and c.driver_id != car.driver_id
                and c.is_pitting_this_lap
            ]
            if same_team_pitting:
                extra_delay = self.rng.uniform(2.8, 4.0)

            # Determine new compound
            if chosen_compound is None:
                chosen_compound = self.ai.choose_compound(car, state)

            # Calculate stop duration
            pit_dur = self.ai.pit_stop_duration_s(team.pit_crew_skill, self.rng) + extra_delay

            # Fuel to add
            fuel_add = self.ai.fuel_to_add_kg(car, state)

            old_compound = car.tire_compound

            # Apply pit stop
            car.is_pitting_this_lap = True
            car.pit_stop_duration_s = pit_dur
            car.tire_compound = chosen_compound
            car.tire_age_laps = 0
            car.pit_stop_count += 1
            car.fuel_kg = min(110.0, car.fuel_kg + fuel_add)
            car.compounds_used.append(chosen_compound)

            results.append(PitStopResult(
                driver_id=car.driver_id,
                lap=lap,
                old_compound=old_compound,
                new_compound=chosen_compound,
                duration_s=pit_dur,
                fuel_added_kg=fuel_add,
                is_player_car=is_player,
            ))

        return results

    # ── Position & gap management ─────────────────────────────────────────────

    def _update_positions(self, state: "RaceState") -> None:
        sorted_cars = state.sorted_cars()
        for pos, car in enumerate(sorted_cars, start=1):
            car.position = pos

    def _compute_gaps(self, state: "RaceState") -> None:
        sorted_cars = state.sorted_cars()
        if not sorted_cars:
            return
        leader = sorted_cars[0]
        for i, car in enumerate(sorted_cars):
            if i == 0:
                car.gap_to_leader_s = 0.0
                car.gap_to_ahead_s = 0.0
            else:
                ahead = sorted_cars[i - 1]
                # Gap = difference in total race time (same lap) or "LAP DOWN"
                if car.laps_completed < ahead.laps_completed:
                    car.gap_to_ahead_s = 9999.0  # lap down
                    car.gap_to_leader_s = 9999.0
                else:
                    gap_ahead  = car.total_race_time_s - ahead.total_race_time_s
                    gap_leader = car.total_race_time_s - leader.total_race_time_s
                    car.gap_to_ahead_s  = max(0.0, gap_ahead)
                    car.gap_to_leader_s = max(0.0, gap_leader)

    # ── Overtake application ──────────────────────────────────────────────────

    def _apply_overtakes(self, state: "RaceState", results: list[OvertakeResult]) -> None:
        """Swap positions and adjust race times to reflect successful overtakes."""
        sorted_cars = state.sorted_cars()
        car_by_id = {c.driver_id: c for c in sorted_cars}

        for ot in results:
            if not ot.success:
                continue
            attacker = car_by_id.get(ot.attacker_id)
            defender = car_by_id.get(ot.defender_id)
            if not attacker or not defender:
                continue

            # Swap total race times so attacker overtakes
            att_time = attacker.total_race_time_s
            def_time = defender.total_race_time_s
            # Give attacker 0.05s gap ahead (clean pass)
            attacker.total_race_time_s = def_time - 0.05
            defender.total_race_time_s = max(def_time, att_time + 0.01)

    # ── Safety car ────────────────────────────────────────────────────────────

    def _update_safety_car(self, state: "RaceState") -> None:
        if state.safety_car == SafetyCarState.DEPLOYED:
            state.safety_car_laps_remaining -= 1
            if state.safety_car_laps_remaining <= 0:
                state.safety_car = SafetyCarState.ENDING
                state.events.append(RaceEvent(
                    state.current_lap, "Race Control", "SC",
                    "🟢 Safety Car is coming in. Green flag next lap!"
                ))
        elif state.safety_car == SafetyCarState.ENDING:
            state.safety_car = SafetyCarState.NONE
            state.events.append(RaceEvent(
                state.current_lap, "Race Control", "SC",
                "🏁 GREEN FLAG — Racing resumes!"
            ))
        elif state.safety_car == SafetyCarState.VSC:
            state.safety_car_laps_remaining -= 1
            if state.safety_car_laps_remaining <= 0:
                state.safety_car = SafetyCarState.NONE
                state.events.append(RaceEvent(
                    state.current_lap, "Race Control", "SC",
                    "🟢 Virtual Safety Car period has ended."
                ))

    def _maybe_trigger_safety_car(self, state: "RaceState", lap: int) -> None:
        """Probabilistic safety car trigger (separate from crash-triggered SC)."""
        if state.safety_car != SafetyCarState.NONE:
            return
        if lap <= 2 or lap >= state.total_laps - 3:
            return

        trigger_prob = self.circuit.safety_car_probability / state.total_laps
        if self.rng.random() < trigger_prob:
            is_vsc = self.rng.random() < 0.35  # 35% chance it's a VSC not full SC
            if is_vsc:
                state.safety_car = SafetyCarState.VSC
                state.safety_car_laps_remaining = self.rng.randint(2, 4)
                state.events.append(RaceEvent(
                    lap, "Race Control", "SC",
                    "🟡 VIRTUAL SAFETY CAR deployed — debris on track."
                ))
            else:
                state.safety_car = SafetyCarState.DEPLOYED
                state.safety_car_laps_remaining = self.rng.randint(SC_MIN_LAPS, SC_MAX_LAPS)
                state.events.append(RaceEvent(
                    lap, "Race Control", "SC",
                    f"🔴 SAFETY CAR deployed! (~{state.safety_car_laps_remaining} laps)"
                ))

    # ── Incidents & DNFs ──────────────────────────────────────────────────────

    def _check_incidents(self, state: "RaceState", lap: int) -> None:
        for car in state.cars:
            if car.dnf:
                continue

            driver = self.drivers[car.driver_id]
            team   = self.teams[car.team_id]

            # ── Aquaplaning check ─────────────────────────────────────────────
            aqua_chance = self.weather_sys.aquaplaning_chance(car.tire_compound)
            if aqua_chance > 0 and self.rng.random() < aqua_chance:
                penalty = self.rng.uniform(0.5, 1.2)
                car.last_lap_time_s += penalty
                car.total_race_time_s += penalty
                is_player = team.id == self.player_team_id
                state.events.append(RaceEvent(
                    lap, driver.name, "INFO",
                    f"💦 {driver.short_name} aquaplanes! Loses {penalty:.1f}s",
                    is_player_event=is_player
                ))

            # ── Mechanical reliability check ──────────────────────────────────
            # Target: ~1.5-2.5% DNF rate per race for standard reliability.
            # Scale per lap: total_rate / total_laps.
            base_mech_rate = (1.0 - team.reliability) * 0.22  # ~2% for reliability=0.91
            dnf_prob = base_mech_rate / max(1, self.circuit.total_laps)
            if car.instruction == DriverInstruction.ATTACK:
                dnf_prob *= 1.5
            if self.rng.random() < dnf_prob:
                self._retire_car(car, driver, team, state, lap, reason="Mechanical failure")
                continue

            # ── Crash probability check ───────────────────────────────────────
            # Base: ~0.15% per lap = ~8% over 53 laps for all 20 cars combined.
            crash_prob = BASE_CRASH_PROB
            if state.weather in (WeatherCondition.LIGHT_RAIN, WeatherCondition.HEAVY_RAIN):
                crash_prob *= 2.2
            if car.instruction == DriverInstruction.ATTACK:
                crash_prob *= 1.6
            if driver.consistency < 75:
                crash_prob *= 1.2

            if self.rng.random() < crash_prob:
                severity = self.rng.random()
                is_player = team.id == self.player_team_id
                if severity > 0.90:   # 10% of crash events = full DNF
                    self._retire_car(car, driver, team, state, lap, reason="Crash")
                    # Trigger safety car
                    if state.safety_car == SafetyCarState.NONE:
                        state.safety_car = SafetyCarState.DEPLOYED
                        state.safety_car_laps_remaining = self.rng.randint(SC_MIN_LAPS, SC_MAX_LAPS)
                        state.events.append(RaceEvent(
                            lap, "Race Control", "SC",
                            f"🔴 SAFETY CAR — {driver.short_name} has crashed out!"
                        ))
                elif severity > 0.65:  # 25% of crash events = damage
                    dmg = self.rng.uniform(0.15, 0.50)
                    car.damage_s += dmg
                    state.events.append(RaceEvent(
                        lap, driver.name, "INFO",
                        f"⚠ {driver.short_name} has car damage: +{dmg:.1f}s/lap",
                        is_player_event=is_player
                    ))

    def _retire_car(
        self,
        car: "CarState",
        driver: "Driver",
        team: "Team",
        state: "RaceState",
        lap: int,
        reason: str,
    ) -> None:
        car.dnf = True
        car.dnf_reason = reason
        is_player = team.id == self.player_team_id
        state.events.append(RaceEvent(
            lap, driver.name, "DNF",
            f"💥 {driver.short_name} ({team.short_name}) RETIRES — {reason}",
            is_player_event=is_player
        ))

    # ── AI instruction update ─────────────────────────────────────────────────

    def _update_ai_instructions(self, state: "RaceState") -> None:
        for car in state.cars:
            if car.dnf or car.team_id == self.player_team_id:
                continue
            driver = self.drivers[car.driver_id]
            car.instruction = self.ai.choose_instruction(car, state, driver)

    # ── Race finalisation ─────────────────────────────────────────────────────

    def _finalize_race(self, state: "RaceState") -> None:
        """Assign final classifications and compute points."""
        pts_map = {1: 25, 2: 18, 3: 15, 4: 12, 5: 10,
                   6: 8,  7: 6,  8: 4,  9: 2,  10: 1}
        active = [c for c in state.cars if not c.dnf]
        dnfd   = [c for c in state.cars if c.dnf]
        active_sorted = sorted(active, key=lambda c: (-c.laps_completed, c.total_race_time_s))
        for pos, car in enumerate(active_sorted, 1):
            car.position = pos
        for i, car in enumerate(dnfd, len(active_sorted) + 1):
            car.position = i
        state.events.append(RaceEvent(
            state.total_laps, "Race Control", "INFO",
            f"🏁 CHEQUERED FLAG — {self.drivers[active_sorted[0].driver_id].short_name} wins!"
        ))
