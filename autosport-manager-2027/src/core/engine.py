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
    TeamOrder,
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
        # Engineer radio cooldown: maps (driver_id, category) → last lap sent
        self._radio_last: dict[tuple[int, str], int] = {}

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

    def command_team_order(self, order: TeamOrder) -> None:
        """Player issues a team-wide order. Applied until next command_team_order call."""
        self.race_state.team_order = order
        self.race_state.events.append(RaceEvent(
            self.race_state.current_lap, "Team Principal", "INFO",
            f"📻 Team orders: {order.value.replace('_', ' ').title()}",
            is_player_event=True,
        ))

    def _simulate_race_start(self, state: "RaceState") -> None:
        """
        Simulate lights-out launch. Each car gets a tiny grid-spread starting offset
        so race times differ. Then we apply start scores to shuffle positions.
        """
        grid_order = state.sorted_cars()
        grid_positions = {c.driver_id: c.position for c in grid_order}

        # Apply small starting offsets so gap computation makes sense from lap 1
        # Each grid position = 0.15s further back (realistic for a 200m grid spread)
        for car in grid_order:
            car.total_race_time_s = (car.position - 1) * 0.15

        # Compute start score per car
        start_scores: list[tuple[float, "CarState"]] = []
        for car in grid_order:
            driver = self.drivers.get(car.driver_id)
            if not driver:
                continue
            score = (
                driver.racecraft * 0.45
                + min(driver.experience, 15) * 1.8
                + self.rng.gauss(0, 6)  # Tighter noise → fewer extreme starts
            )
            start_scores.append((score, car, car.position))

        # Sort by score (higher = gained more at start)
        start_scores.sort(key=lambda x: -x[0])

        # Assign new time offsets based on start rank (with tiebreaker noise)
        for new_rank, (score, car, original_pos) in enumerate(start_scores, 1):
            # Clamp: max 3 positions gain at start
            clamped_rank = max(original_pos - 3, min(original_pos + 3, new_rank))
            # Small noise prevents exact ties while preserving order
            car.total_race_time_s = (clamped_rank - 1) * 0.15 + self.rng.uniform(0, 0.03)

        # Update positions
        self._update_positions(state)
        self._compute_gaps(state)

        # Hard-clamp actual deltas to ±3 (resolve cascading effects)
        needs_recheck = False
        for car in state.cars:
            old_q = grid_positions.get(car.driver_id, car.position)
            if abs(car.position - old_q) > 3:
                car.total_race_time_s = (old_q - 1 + self.rng.uniform(-2, 2)) * 0.15
                needs_recheck = True
        if needs_recheck:
            self._update_positions(state)
            self._compute_gaps(state)

        # Log notable starts (≥2 position change)
        for car in state.sorted_cars():
            driver = self.drivers.get(car.driver_id)
            if not driver:
                continue
            old_q = grid_positions.get(car.driver_id, car.position)
            delta = old_q - car.position
            is_player = car.team_id == self.player_team_id
            if delta >= 3:
                state.events.append(RaceEvent(
                    1, driver.name, "INFO",
                    f"🟢 {driver.short_name} rocket start! +{delta} (P{old_q}→P{car.position})",
                    is_player_event=is_player,
                ))
            elif delta <= -3:
                state.events.append(RaceEvent(
                    1, driver.name, "INFO",
                    f"🔴 {driver.short_name} poor start: P{old_q}→P{car.position}",
                    is_player_event=is_player,
                ))

    def _apply_team_orders(self, state: "RaceState") -> None:
        order = state.team_order
        if order is None or order == TeamOrder.FREE_RACE:
            return

        player_cars = [c for c in state.cars if c.team_id == self.player_team_id and not c.dnf]
        if len(player_cars) < 2:
            return

        player_cars_sorted = sorted(player_cars, key=lambda c: c.position)
        lead_car   = player_cars_sorted[0]
        follow_car = player_cars_sorted[1]

        if order == TeamOrder.HOLD_GAP:
            if follow_car.gap_to_ahead_s < 1.5 and follow_car.team_id == lead_car.team_id:
                follow_car.instruction = DriverInstruction.MANAGE

        elif order == TeamOrder.SWAP_DRIVERS:
            if (follow_car.gap_to_ahead_s < 0.5
                    and follow_car.team_id == lead_car.team_id
                    and lead_car.tire_age_laps > follow_car.tire_age_laps + 3):
                # Actually execute swap: adjust race times
                lead_car.total_race_time_s, follow_car.total_race_time_s = (
                    follow_car.total_race_time_s - 0.1,
                    lead_car.total_race_time_s + 0.1,
                )
                driver1 = self.drivers.get(lead_car.driver_id)
                driver2 = self.drivers.get(follow_car.driver_id)
                if driver1 and driver2:
                    state.events.append(RaceEvent(
                        state.current_lap, "Team Principal", "INFO",
                        f"📻 {driver2.short_name} passes {driver1.short_name} — team order executed.",
                        is_player_event=True,
                    ))
                state.team_order = TeamOrder.FREE_RACE  # Reset after swap

        elif order == TeamOrder.PUSH_BOTH:
            for car in player_cars:
                car.instruction = DriverInstruction.ATTACK

    # ── Main lap simulation ───────────────────────────────────────────────────

    def simulate_lap(self) -> RaceState:
        """Advance the race by one lap. Returns updated RaceState."""
        state = self.race_state
        state.current_lap += 1
        lap = state.current_lap

        if state.is_race_complete:
            return state

        # ── 0. Lap 1 race start ───────────────────────────────────────────────
        if lap == 1:
            self._simulate_race_start(state)

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

        # ── 4b. Apply team orders ─────────────────────────────────────────────
        self._apply_team_orders(state)

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

            # Safety car: all cars drive slowly, field compresses
            if state.safety_car in (SafetyCarState.DEPLOYED, SafetyCarState.VSC):
                sc_mult = SC_PACE_MULTIPLIER if state.safety_car == SafetyCarState.DEPLOYED else 1.30
                base_time = self.circuit.base_lap_time_s * sc_mult
                # Under full SC: compress field — cars behind close up toward the train.
                # Each SC lap, gap_to_leader shrinks by 18% (bounded at 1.0s minimum).
                if state.safety_car == SafetyCarState.DEPLOYED and car.gap_to_leader_s > 1.0:
                    compression = car.gap_to_leader_s * 0.18
                    car.total_race_time_s -= compression
                car.last_lap_time_s = base_time + self.rng.uniform(-0.1, 0.1)
                car.total_race_time_s += car.last_lap_time_s
                car.laps_completed += 1
                continue

            lap_time = self._compute_lap_time(car, driver, team, state, i, sorted_cars)
            car.last_lap_time_s = lap_time
            car.total_race_time_s += lap_time
            car.laps_completed += 1

            # Record history for sparkline display
            car.lap_times.append(lap_time)

            # Compute sector times: circuit-weighted split with independent noise
            s = self.circuit.sector_splits
            n1 = self.rng.gauss(0, 0.06)
            n2 = self.rng.gauss(0, 0.06)
            s1 = lap_time * s[0] + n1
            s2 = lap_time * s[1] + n2
            s3 = max(0.1, lap_time - s1 - s2)
            car.last_sector_times = (s1, s2, s3)
            car.best_sector_times = (
                min(car.best_sector_times[0], s1),
                min(car.best_sector_times[1], s2),
                min(car.best_sector_times[2], s3),
            )

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

        # ── 7b. Update driver momentum after overtakes ────────────────────────
        self._update_driver_momentum(state)

        # ── 8. Check incidents / crashes ──────────────────────────────────────
        self._check_incidents(state, lap)

        # ── 9. Recompute positions & gaps ──────────────────────────────────────
        self._update_positions(state)
        self._compute_gaps(state)

        # ── 9b. Record gap history for all cars (sparkline + trend arrows) ───
        for car in state.cars:
            if not car.dnf:
                car.gap_history.append(car.gap_to_leader_s)

        # ── 10. AI: choose next instruction ──────────────────────────────────
        self._update_ai_instructions(state)

        # ── 10b. Engineer radio for player cars ───────────────────────────────
        self._engineer_radio(state, lap)

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

        # ── Car performance: overall baseline, then adjusted for circuit type ──
        car_gap = (100 - team.car_performance) * 0.055
        # Circuit-sensitive performance: power_unit helps on power tracks, chassis on df tracks.
        # Scale: 1-point delta = 0.015s when sensitivity=1.0; effect ≈ ±0.10s max at extreme circuits.
        ps = self.circuit.power_sensitivity
        pu_adv  = (team.power_unit - 90) * ps * 0.015
        aero_adv = (team.chassis - 90) * (1.0 - ps) * 0.015
        car_gap -= (pu_adv + aero_adv)
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

        # ── Weather penalty — scaled by driver wet skill ─────────────────────
        weather_pen = self.weather_sys.lap_time_weather_penalty_s(car.tire_compound)
        if weather_pen > 0:
            # wet_skill 80 = baseline; 98 (HAM) = 15% less penalty; 70 (STR) = 6% more
            wet_mod = 1.0 - (driver.wet_skill - 80) * 0.0075
            weather_pen *= max(0.60, min(1.40, wet_mod))
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

            # Momentum: attacker on a high, defender momentarily rattled
            attacker.morale_modifier_s = max(-0.30, attacker.morale_modifier_s - 0.10)
            defender.morale_modifier_s  = min( 0.25, defender.morale_modifier_s  + 0.08)

    def _update_driver_momentum(self, state: "RaceState") -> None:
        """Decay all drivers' morale modifiers toward 0 each lap (~halved every 5 laps)."""
        for car in state.cars:
            if car.dnf:
                continue
            # Fastest-lap holder gets a sustained small boost
            if car.driver_id == state.fastest_lap_driver_id:
                car.morale_modifier_s = max(-0.12, car.morale_modifier_s - 0.02)
            # Exponential decay toward 0
            car.morale_modifier_s *= 0.82
            # Clamp to ±0.3s
            car.morale_modifier_s = max(-0.30, min(0.25, car.morale_modifier_s))

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

    # ── Engineer radio messages ───────────────────────────────────────────────

    def _engineer_radio(self, state: "RaceState", lap: int) -> None:
        """
        Generate engineer radio messages for player cars at key moments.
        Rate-limited per (driver_id, category) to avoid spam.
        """
        from .tire import TIRE_PROFILES, optimal_tire_window_remaining

        player_cars = state.get_player_cars()

        for car in player_cars:
            if car.dnf or car.is_pitting_this_lap:
                continue

            driver = self.drivers.get(car.driver_id)
            if not driver:
                continue

            laps_left = state.total_laps - lap

            def _radio(category: str, cooldown: int, msg: str) -> None:
                key = (car.driver_id, category)
                if lap - self._radio_last.get(key, -999) >= cooldown:
                    self._radio_last[key] = lap
                    state.events.append(RaceEvent(
                        lap, driver.name, "INFO", msg, is_player_event=True
                    ))

            # ── Tire window warnings ──────────────────────────────────────────
            profile = TIRE_PROFILES.get(car.tire_compound)
            if profile and not car.is_pitting_this_lap:
                window = optimal_tire_window_remaining(
                    profile, car.tire_age_laps, self.circuit.tire_deg_multiplier
                )
                if car.tire_phase == TirePhase.CLIFF:
                    _radio("tire_cliff", 2,
                           f"📻 {driver.short_name}: Box box box! Tyre is gone — pit NOW!")
                elif window == 4:
                    _radio("tire_4", 5,
                           f"📻 Engineer: {driver.short_name}, you have four laps of tyre left.")
                elif window == 2:
                    _radio("tire_2", 3,
                           f"📻 Engineer: Two laps, {driver.short_name}. Start thinking about the box.")
                elif window == 1:
                    _radio("tire_1", 2,
                           f"📻 Engineer: Last lap of the tyre, {driver.short_name}. Box next lap.")

            # ── Safety car pit window ─────────────────────────────────────────
            if (state.safety_car in (SafetyCarState.DEPLOYED, SafetyCarState.VSC)
                    and car.tire_age_laps >= 6 and laps_left > 8):
                _radio("sc_window", 4,
                       f"📻 Engineer: Safety car — free stop available! Box this lap, {driver.short_name}!")

            # ── DRS / gap battle ahead ────────────────────────────────────────
            if 0 < car.gap_to_ahead_s <= 1.0 and car.position > 1:
                sorted_cars = state.sorted_cars()
                pos_idx = next((i for i, c in enumerate(sorted_cars) if c.driver_id == car.driver_id), -1)
                if pos_idx > 0:
                    target = sorted_cars[pos_idx - 1]
                    target_driver = self.drivers.get(target.driver_id)
                    target_name = target_driver.short_name if target_driver else "car ahead"
                    if car.gap_to_ahead_s <= 0.5:
                        _radio("drs_attack", 3,
                               f"📻 Engineer: DRS engaged — go for it on {target_name}, {driver.short_name}!")
                    else:
                        _radio("drs_hunt", 4,
                               f"📻 Engineer: Gap to {target_name}: {car.gap_to_ahead_s:.1f}s — keep pushing.")

            # ── Car closing from behind ───────────────────────────────────────
            sorted_cars = state.sorted_cars()
            pos_idx = next((i for i, c in enumerate(sorted_cars) if c.driver_id == car.driver_id), -1)
            if pos_idx >= 0 and pos_idx < len(sorted_cars) - 1:
                behind = sorted_cars[pos_idx + 1]
                if 0 < behind.gap_to_ahead_s <= 1.2 and not behind.dnf:
                    behind_driver = self.drivers.get(behind.driver_id)
                    behind_name = behind_driver.short_name if behind_driver else "car behind"
                    _radio("defend_warning", 4,
                           f"📻 Engineer: {behind_name} is {behind.gap_to_ahead_s:.1f}s behind — watch your mirrors.")

            # ── Fuel warning ──────────────────────────────────────────────────
            fuel_laps = car.fuel_kg / max(0.1, self.circuit.fuel_consumption_kg)
            if fuel_laps < laps_left - 1 and laps_left > 3:
                _radio("fuel_warn", 5,
                       f"📻 Engineer: Fuel is short, {driver.short_name} — switch to fuel save mode.")
            elif 1 < laps_left <= 5 and fuel_laps >= laps_left:
                _radio("fuel_ok", 99,
                       f"📻 Engineer: Fuel is fine to the end. Push if you need to.")

            # ── Final laps sprint ─────────────────────────────────────────────
            if laps_left == 5:
                _radio("5_to_go", 99,
                       f"📻 Engineer: Five laps to go, {driver.short_name}. Give it everything.")
            elif laps_left == 3:
                _radio("3_to_go", 99,
                       f"📻 Engineer: Three laps remaining. P{car.position} — keep it clean.")
            elif laps_left == 1:
                _radio("last_lap", 99,
                       f"📻 Engineer: Final lap, {driver.short_name}! You're P{car.position}.")

            # ── New position gained (podium / points) ────────────────────────
            if car.position <= 3:
                _radio("podium", 10,
                       f"📻 Engineer: P{car.position} — you're on the podium! Keep it together.")
            elif car.position <= 10 and laps_left <= 10:
                _radio("points", 8,
                       f"📻 Engineer: P{car.position} — you're in the points. Bring it home.")

            # ── Fastest lap within reach (last 10 laps) ───────────────────────
            fl = state.fastest_lap_time_s
            ll = car.last_lap_time_s
            if (laps_left <= 10 and laps_left >= 2 and ll > 0 and fl > 0
                    and ll - fl < 0.8 and car.tire_phase != TirePhase.CLIFF):
                _radio("fl_attempt", 5,
                       f"📻 Engineer: You're {ll - fl:.2f}s off fastest lap — push for the bonus point!")

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
