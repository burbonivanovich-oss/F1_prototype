"""Core runtime state models for the race simulation."""

from __future__ import annotations
from dataclasses import dataclass, field
from enum import Enum, auto
from typing import Optional


# ─────────────────────────────────────────────────────────────────────────────
# Enumerations
# ─────────────────────────────────────────────────────────────────────────────

class TireCompound(Enum):
    """C1=ultra-hard, C6=ultra-soft. Only 3 selected per race by circuit rules."""
    C1 = 1   # Ultra-Hard (rare; Monza/Baku only)
    C2 = 2   # Hard
    C3 = 3   # Hard (standard)
    C4 = 4   # Medium
    C5 = 5   # Soft
    C6 = 6   # Ultra-Soft (Monaco, Singapore, Hungary)
    INTER = 7   # Intermediate (wet)
    WET = 8     # Full wet

    @property
    def display_name(self) -> str:
        names = {
            self.C1: "UH", self.C2: "H", self.C3: "H",
            self.C4: "M",  self.C5: "S", self.C6: "US",
            self.INTER: "INT", self.WET: "WET",
        }
        return names[self]

    @property
    def color(self) -> str:
        colors = {
            self.C1: "white", self.C2: "white", self.C3: "white",
            self.C4: "yellow", self.C5: "red",  self.C6: "red",
            self.INTER: "green", self.WET: "blue",
        }
        return colors[self]

    @property
    def is_dry(self) -> bool:
        return self not in (TireCompound.INTER, TireCompound.WET)

    @property
    def grip_advantage_s(self) -> float:
        """Lap time advantage relative to C4/Medium at optimal temperature (seconds)."""
        adv = {
            self.C1: 0.25, self.C2: 0.20, self.C3: 0.15,
            self.C4: 0.0,  self.C5: -0.25, self.C6: -0.30,
            self.INTER: 0.0, self.WET: 0.0,
        }
        return adv[self]


class DriverInstruction(Enum):
    MANAGE = "MANAGE"    # Economy – save tires and fuel; -0.15s lap, -15% wear
    ATTACK = "ATTACK"    # Push for position; -0.25s lap, +15% wear
    DEFEND = "DEFEND"    # Hold position; +0.05s, activates defender aggression
    FUEL_SAVE = "FUEL_SAVE"  # Conservation; -0.10s lap, -12% fuel


class TeamOrder(Enum):
    FREE_RACE    = "FREE_RACE"    # Both drivers race freely
    HOLD_GAP     = "HOLD_GAP"    # Driver 1 holds gap; driver 2 not to overtake
    SWAP_DRIVERS = "SWAP_DRIVERS" # Request position swap between teammates
    PUSH_BOTH    = "PUSH_BOTH"   # Both drivers on ATTACK mode


class WeatherCondition(Enum):
    DRY = "DRY"
    LIGHT_RAIN = "LIGHT_RAIN"
    HEAVY_RAIN = "HEAVY_RAIN"
    DRYING = "DRYING"   # Post-rain, track still damp


class SafetyCarState(Enum):
    NONE = "NONE"
    DEPLOYED = "DEPLOYED"  # Full safety car
    VSC = "VSC"            # Virtual Safety Car
    ENDING = "ENDING"      # Final lap before restart


class TirePhase(Enum):
    WARM_UP = "WARM_UP"    # First 1-2 laps after pit; warm-up penalty active
    PLATEAU = "PLATEAU"    # Peak performance, minimal degradation
    LINEAR = "LINEAR"      # Progressive wear, calculable time loss
    CLIFF = "CLIFF"        # Rapid performance collapse — MUST pit soon


# ─────────────────────────────────────────────────────────────────────────────
# Per-car race state
# ─────────────────────────────────────────────────────────────────────────────

@dataclass
class CarState:
    driver_id: int
    team_id: int
    car_number: int

    # Race progress
    position: int = 0
    laps_completed: int = 0
    total_race_time_s: float = 0.0
    last_lap_time_s: float = 0.0
    best_lap_time_s: float = 0.0
    gap_to_leader_s: float = 0.0
    gap_to_ahead_s: float = 0.0

    # Tire state
    tire_compound: TireCompound = TireCompound.C5
    tire_age_laps: int = 0        # Laps on this set
    tire_phase: TirePhase = TirePhase.PLATEAU
    tire_deg_pct: float = 0.0     # 0.0 = fresh, 1.0 = destroyed

    # Fuel state
    fuel_kg: float = 105.0
    fuel_mode: str = "STANDARD"   # STANDARD | ECONOMY | ATTACK

    # Strategy
    pit_stop_count: int = 0
    laps_on_softs: int = 0        # Mandatory compound rule tracking
    laps_on_hards: int = 0
    compounds_used: list[TireCompound] = field(default_factory=list)
    planned_pit_laps: list[int] = field(default_factory=list)

    # Instruction & morale
    instruction: DriverInstruction = DriverInstruction.MANAGE
    morale_modifier_s: float = 0.0  # Pace penalty/bonus from driver morale

    # Incident state
    dnf: bool = False
    dnf_reason: str = ""
    damage_s: float = 0.0         # Lap time penalty from car damage

    # In-pit flag
    is_pitting_this_lap: bool = False
    pit_stop_duration_s: float = 0.0

    # ERS / MGU-K
    ers_charge_pct: float = 0.80  # 0–1; deployed on overtake/defend
    ers_deploy_mode: str = "MEDIUM"  # LOW | MEDIUM | HIGH

    # Historical data (for sparkline charts)
    lap_times: list[float] = field(default_factory=list)    # every lap time logged
    gap_history: list[float] = field(default_factory=list)  # gap to leader each lap


# ─────────────────────────────────────────────────────────────────────────────
# Race-level state
# ─────────────────────────────────────────────────────────────────────────────

@dataclass
class RaceState:
    circuit_name: str
    total_laps: int
    current_lap: int = 0
    is_race_complete: bool = False

    cars: list[CarState] = field(default_factory=list)

    # Weather
    weather: WeatherCondition = WeatherCondition.DRY
    track_temp_c: float = 28.0
    air_temp_c: float = 22.0
    water_depth_mm: float = 0.0   # Relevant only in rain
    weather_forecast: list[WeatherCondition] = field(default_factory=list)  # next 5 laps

    # Safety car
    safety_car: SafetyCarState = SafetyCarState.NONE
    safety_car_laps_remaining: int = 0
    safety_car_trigger_car: int = -1

    # Event log (displayed in UI)
    events: list[RaceEvent] = field(default_factory=list)

    # Fastest lap tracking
    fastest_lap_time_s: float = 9999.0
    fastest_lap_driver_id: int = -1
    fastest_lap_number: int = 0

    # Player's team id
    player_team_id: int = 0

    # Team orders
    team_order: Optional["TeamOrder"] = None

    def get_car(self, driver_id: int) -> Optional[CarState]:
        for car in self.cars:
            if car.driver_id == driver_id:
                return car
        return None

    def get_player_cars(self) -> list[CarState]:
        return [c for c in self.cars if c.team_id == self.player_team_id and not c.dnf]

    def sorted_cars(self) -> list[CarState]:
        """Returns cars ordered by race position (leader first)."""
        active = [c for c in self.cars if not c.dnf]
        dnfd   = [c for c in self.cars if c.dnf]
        active_sorted = sorted(active, key=lambda c: (-c.laps_completed, c.total_race_time_s))
        return active_sorted + dnfd


# ─────────────────────────────────────────────────────────────────────────────
# Events
# ─────────────────────────────────────────────────────────────────────────────

@dataclass
class RaceEvent:
    lap: int
    driver_name: str
    event_type: str    # "PIT" | "OVERTAKE" | "DNF" | "SC" | "WEATHER" | "INFO"
    description: str
    is_player_event: bool = False

    def __str__(self) -> str:
        prefix = "★ " if self.is_player_event else "  "
        return f"{prefix}[L{self.lap:02d}] {self.description}"


@dataclass
class PitStopResult:
    driver_id: int
    lap: int
    old_compound: TireCompound
    new_compound: TireCompound
    duration_s: float
    fuel_added_kg: float
    is_player_car: bool


@dataclass
class OvertakeResult:
    attacker_id: int
    defender_id: int
    lap: int
    success: bool
    p_success: float
    was_drs: bool
    new_position_attacker: int
