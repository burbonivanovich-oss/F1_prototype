"""Rich terminal race display — live standings, telemetry, event log."""

from __future__ import annotations
import os
from typing import Optional, TYPE_CHECKING

from rich.console import Console
from rich.table import Table
from rich.panel import Panel
from rich.columns import Columns
from rich.text import Text
from rich.rule import Rule
from rich import box

from ..core.models import (
    TireCompound, TirePhase, DriverInstruction, WeatherCondition,
    SafetyCarState, RaceState, CarState,
)
from ..core.tire import TIRE_PROFILES, get_tire_phase, optimal_tire_window_remaining
from .track_map import render_track_map
from ..core.ai import pit_stop_projection

if TYPE_CHECKING:
    from ..data.drivers import Driver
    from ..data.teams import Team
    from ..data.circuits import Circuit

console = Console()


# ─── Helpers ─────────────────────────────────────────────────────────────────

def _tire_color(phase: TirePhase, deg_pct: float) -> str:
    if phase == TirePhase.WARM_UP:
        return "cyan"
    if phase == TirePhase.PLATEAU:
        return "green"
    if phase == TirePhase.LINEAR:
        if deg_pct < 0.65:
            return "yellow"
        return "dark_orange"
    if phase == TirePhase.CLIFF:
        return "bold red"
    return "white"


def _compound_badge(compound: TireCompound, age: int, phase: TirePhase, deg: float) -> Text:
    badge_color = _tire_color(phase, deg)
    label = f"{compound.display_name}·{age:02d}"
    return Text(label, style=badge_color)


def _gap_str(gap: float, laps_behind: bool = False) -> str:
    if laps_behind:
        return "LAP"
    if gap == 0.0:
        return "LEADER"
    if gap >= 9000:
        return "LAP"
    return f"+{gap:.3f}"


def _weather_icon(cond: WeatherCondition) -> str:
    icons = {
        WeatherCondition.DRY:         "☀  DRY",
        WeatherCondition.LIGHT_RAIN:  "🌧  RAIN",
        WeatherCondition.HEAVY_RAIN:  "⛈  HEAVY",
        WeatherCondition.DRYING:      "🌤  DRYING",
    }
    return icons.get(cond, "?")


def _sc_badge(sc: SafetyCarState) -> str:
    if sc == SafetyCarState.DEPLOYED:
        return "[bold yellow on red] SC [/]"
    if sc == SafetyCarState.VSC:
        return "[bold yellow] VSC [/]"
    if sc == SafetyCarState.ENDING:
        return "[bold yellow] SC ending [/]"
    return ""


def _instruction_color(instr: DriverInstruction) -> str:
    colors = {
        DriverInstruction.ATTACK:    "bold red",
        DriverInstruction.MANAGE:    "green",
        DriverInstruction.DEFEND:    "yellow",
        DriverInstruction.FUEL_SAVE: "cyan",
    }
    return colors.get(instr, "white")


# ─── Standings Table ──────────────────────────────────────────────────────────

def build_standings_table(
    state: RaceState,
    drivers: dict,
    teams: dict,
) -> Table:
    table = Table(
        box=box.SIMPLE_HEAD,
        show_header=True,
        header_style="bold white",
        padding=(0, 1),
        expand=False,
    )
    table.add_column("P",   width=3, justify="right")
    table.add_column("#",   width=3, justify="center")
    table.add_column("Driver", width=4)
    table.add_column("Team",   width=4)
    table.add_column("Gap",    width=10, justify="right")
    table.add_column("Tyre",   width=6)
    table.add_column("Pit",    width=3, justify="center")
    table.add_column("Instr",  width=5)
    table.add_column("Last",   width=8, justify="right")

    sorted_cars = state.sorted_cars()
    for car in sorted_cars:
        driver = drivers.get(car.driver_id)
        team   = teams.get(car.team_id)
        if not driver or not team:
            continue

        is_player_team = (car.team_id == state.player_team_id)

        pos_str = f"[dim]DNF[/dim]" if car.dnf else str(car.position)

        # Gap
        laps_behind = car.gap_to_leader_s >= 9000
        gap_s = _gap_str(car.gap_to_leader_s, laps_behind)

        # Tire compound badge
        profile = TIRE_PROFILES.get(car.tire_compound)
        phase = car.tire_phase
        tire_text = _compound_badge(car.tire_compound, car.tire_age_laps, phase, car.tire_deg_pct)

        # Last lap time
        if car.last_lap_time_s > 0:
            m = int(car.last_lap_time_s // 60)
            s = car.last_lap_time_s % 60
            last_str = f"{m}:{s:06.3f}"
        else:
            last_str = "—"

        # Instruction
        instr_str = car.instruction.value[:3] if not car.dnf else "DNF"
        instr_color = _instruction_color(car.instruction) if not car.dnf else "dim"

        # Row style: highlight player team
        row_style = "bold" if is_player_team else ""
        dnf_style = "dim" if car.dnf else ""
        final_style = f"{row_style} {dnf_style}".strip() or None

        table.add_row(
            pos_str,
            str(car.car_number),
            f"[{team.color}]{driver.short_name}[/]",
            team.short_name,
            gap_s,
            tire_text,
            str(car.pit_stop_count),
            f"[{instr_color}]{instr_str}[/]",
            last_str,
            style=final_style,
        )

    return table


# ─── Player car telemetry panel ───────────────────────────────────────────────

def build_player_panel(
    state: RaceState,
    drivers: dict,
    teams: dict,
    circuit: "Circuit",
    available_compounds: list | None = None,
) -> Panel:
    player_cars = state.get_player_cars()
    if not player_cars:
        return Panel("[dim]No active player cars[/dim]", title="YOUR TEAM", border_style="dim")

    lines: list[Text] = []
    team = teams.get(state.player_team_id)
    team_name = team.name if team else "Player Team"

    for car in player_cars:
        driver = drivers.get(car.driver_id)
        if not driver:
            continue

        # Header: driver + position
        pos_badge = f"P{car.position}" if not car.dnf else "DNF"
        header = Text()
        header.append(f"  {driver.name} #{car.car_number}  ", style="bold white")
        header.append(f"[{pos_badge}]", style="bold yellow")
        lines.append(header)

        # Gap
        if car.position == 1:
            gap_line = Text("  Gap: RACE LEADER", style="bold green")
        elif car.gap_to_leader_s >= 9000:
            gap_line = Text("  Gap: LAP DOWN", style="dim")
        else:
            gap_line = Text(f"  Gap to leader: +{car.gap_to_leader_s:.3f}s", style="white")
        lines.append(gap_line)

        # Tire status
        profile = TIRE_PROFILES.get(car.tire_compound)
        phase = car.tire_phase
        deg_pct = car.tire_deg_pct
        tire_color = _tire_color(phase, deg_pct)
        phase_label = phase.value
        window = 0
        if profile:
            window = optimal_tire_window_remaining(
                profile, car.tire_age_laps, circuit.tire_deg_multiplier
            )
        tire_line = Text()
        tire_line.append(f"  Tyre: ", style="white")
        tire_line.append(
            f"{car.tire_compound.display_name} [{car.tire_age_laps} laps] — {phase_label}",
            style=tire_color
        )
        if window <= 3 and phase != TirePhase.CLIFF:
            tire_line.append(f"  ⚠ {window} laps to cliff!", style="bold red")
        elif phase == TirePhase.CLIFF:
            tire_line.append("  ‼ CLIFF! PIT NOW!", style="bold red blink")
        else:
            tire_line.append(f"  ({window} laps window)", style="dim")
        lines.append(tire_line)

        # Fuel
        fuel_laps = car.fuel_kg / circuit.fuel_consumption_kg
        laps_left = state.total_laps - state.current_lap
        fuel_color = "green" if fuel_laps > laps_left + 3 else (
            "yellow" if fuel_laps > laps_left else "bold red"
        )
        fuel_line = Text()
        fuel_line.append("  Fuel: ", style="white")
        fuel_line.append(f"{car.fuel_kg:.1f} kg  (~{fuel_laps:.0f} laps)", style=fuel_color)
        lines.append(fuel_line)

        # Instruction
        instr_color = _instruction_color(car.instruction)
        instr_line = Text()
        instr_line.append("  Mode: ", style="white")
        instr_line.append(car.instruction.value, style=instr_color)
        lines.append(instr_line)

        # ERS charge
        ers_bar = "█" * int(car.ers_charge_pct * 10) + "░" * (10 - int(car.ers_charge_pct * 10))
        ers_line = Text()
        ers_line.append("  ERS:  ", style="white")
        ers_line.append(f"[{ers_bar}] {car.ers_charge_pct*100:.0f}%", style="bright_blue")
        lines.append(ers_line)

        # Pit suggestion
        if profile:
            if phase == TirePhase.CLIFF:
                advice = Text("  ⚑ ENGINEER: Box box box!! Tyre is destroyed!", style="bold red")
            elif window <= 4:
                advice = Text(f"  ⚑ ENGINEER: Consider pitting — {window} laps of grip left", style="yellow")
            else:
                advice = Text("  ⚑ ENGINEER: All looks good. Stay out.", style="dim green")
            lines.append(advice)

        # Strategy projection: pit now estimate
        if available_compounds:
            avail_dry = [c for c in available_compounds if c.is_dry]
            laps_left = state.total_laps - state.current_lap
            sorted_cars = state.sorted_cars()
            proj = pit_stop_projection(car, sorted_cars, circuit, avail_dry, laps_left)
            if proj:
                strat_line = Text()
                strat_line.append("  ⚑ Pit now: ", style="white")
                cmpd = proj["compound"]
                pos_after = proj["position_after"]
                recover = proj["laps_to_recover"]
                pos_before = proj["position_before"]
                delta = pos_after - pos_before
                if proj["cars_lost"] == 0:
                    strat_line.append(
                        f"P{pos_before}→P{pos_after} {cmpd.display_name} (clean stop!)",
                        style="bold green"
                    )
                elif proj["can_recover"]:
                    strat_line.append(
                        f"P{pos_before}→P{pos_after} {cmpd.display_name} (+{delta}p), recover ~L{state.current_lap + recover}",
                        style="yellow"
                    )
                else:
                    strat_line.append(
                        f"P{pos_before}→P{pos_after} {cmpd.display_name} (+{delta}p, hard to recover)",
                        style="dim red"
                    )
                lines.append(strat_line)

        lines.append(Text(""))  # spacer between cars

    content = Text("\n").join(lines) if lines else Text("No active cars")

    sc_badge = _sc_badge(state.safety_car)
    title = f"[{team.color}]{team_name}[/]" if team else "YOUR TEAM"

    return Panel(
        content,
        title=title,
        border_style=team.color if team else "white",
        padding=(0, 1),
    )


# ─── Track map panel ─────────────────────────────────────────────────────────

def build_track_map_panel(
    state: RaceState,
    drivers: dict,
    teams: dict,
    circuit: "Circuit",
) -> Panel:
    map_text = render_track_map(circuit, state, drivers, teams)
    return Panel(
        map_text,
        title=f"[dim]{circuit.name.split(' Grand Prix')[0]}[/]",
        border_style="dim",
        padding=(0, 1),
    )


# ─── Weather panel ────────────────────────────────────────────────────────────

def build_weather_panel(state: RaceState) -> Panel:
    cond_str = _weather_icon(state.weather)
    forecast_icons = {
        WeatherCondition.DRY:        "☀",
        WeatherCondition.LIGHT_RAIN: "🌦",
        WeatherCondition.HEAVY_RAIN: "⛈",
        WeatherCondition.DRYING:     "🌤",
    }
    forecast_str = " ".join(forecast_icons.get(c, "?") for c in state.weather_forecast)

    content = (
        f"  Now: {cond_str}   Track: {state.track_temp_c:.0f}°C  Air: {state.air_temp_c:.0f}°C\n"
        f"  Forecast (+5 laps): {forecast_str}"
    )
    return Panel(content, title="Weather", border_style="blue", padding=(0, 1))


# ─── Event log panel ──────────────────────────────────────────────────────────

def build_event_log(state: RaceState, max_events: int = 10) -> Panel:
    recent = state.events[-max_events:]
    lines: list[str] = []
    for ev in reversed(recent):
        style = "bold yellow" if ev.is_player_event else ""
        prefix = "★ " if ev.is_player_event else "  "
        color = {
            "PIT":      "cyan",
            "OVERTAKE": "green",
            "DNF":      "red",
            "SC":       "yellow",
            "WEATHER":  "blue",
            "INFO":     "white",
        }.get(ev.event_type, "white")
        lines.append(f"[{color}]{prefix}[L{ev.lap:02d}] {ev.description}[/]")
    content = "\n".join(lines) if lines else "[dim]No events yet[/dim]"
    return Panel(content, title="Race Events", border_style="dim white", padding=(0, 1))


# ─── Controls help panel ──────────────────────────────────────────────────────

def build_controls_panel(player_cars: list[CarState], drivers: dict) -> Panel:
    cmds: list[str] = []
    for car in player_cars:
        driver = drivers.get(car.driver_id)
        if not driver or car.dnf:
            continue
        n = driver.car_number
        cmds.append(
            f"[bold]b{n}[/]  Box #{n}   "
            f"[bold]a{n}[/]  Attack   "
            f"[bold]m{n}[/]  Manage   "
            f"[bold]d{n}[/]  Defend   "
            f"[bold]f{n}[/]  Fuel Save"
        )
    cmds.append("[bold]ff[/]  Fast-fwd 5 laps   [bold]fff[/]  Fast-fwd 10 laps   [bold]q[/]  Quit")
    return Panel(
        "\n".join(cmds) if cmds else "[dim]No controls available[/dim]",
        title="Commands",
        border_style="dim",
        padding=(0, 1),
    )


# ─── Full race screen ─────────────────────────────────────────────────────────

def render_race_screen(
    state: RaceState,
    drivers: dict,
    teams: dict,
    circuit: "Circuit",
) -> None:
    console.clear()

    # ── Header ────────────────────────────────────────────────────────────────
    sc_tag = _sc_badge(state.safety_car)
    console.print(
        Rule(
            f"[bold white]{state.circuit_name}[/]  "
            f"[yellow]LAP {state.current_lap}/{state.total_laps}[/]  "
            f"{sc_tag}  "
            f"[dim]Fastest: {_fastest_lap_str(state, drivers)}[/]",
            style="dim"
        )
    )

    # ── Main two-column layout ─────────────────────────────────────────────────
    standings = build_standings_table(state, drivers, teams)
    player_panel = build_player_panel(state, drivers, teams, circuit)
    weather_panel = build_weather_panel(state)

    left_items = [Panel(standings, title="Live Standings", border_style="dim white", padding=(0, 0))]
    right_items = [player_panel, weather_panel]

    console.print(Columns([
        "\n".join(str(i) for i in left_items),
        "\n".join(str(i) for i in right_items),
    ], equal=False, expand=True))

    # ── Event log ─────────────────────────────────────────────────────────────
    console.print(build_event_log(state))

    # ── Controls ──────────────────────────────────────────────────────────────
    player_cars = state.get_player_cars()
    if player_cars:
        console.print(build_controls_panel(player_cars, drivers))


def _fastest_lap_str(state: RaceState, drivers: dict) -> str:
    if state.fastest_lap_driver_id < 0:
        return "—"
    driver = drivers.get(state.fastest_lap_driver_id)
    if not driver:
        return "—"
    t = state.fastest_lap_time_s
    m = int(t // 60)
    s = t % 60
    return f"{driver.short_name} {m}:{s:06.3f} (L{state.fastest_lap_number})"


def render_race_screen_rich(
    state: RaceState,
    drivers: dict,
    teams: dict,
    circuit: "Circuit",
) -> None:
    """Full render using rich layout — used for each lap update."""
    console.clear()

    # Header bar
    sc_tag = ""
    if state.safety_car == SafetyCarState.DEPLOYED:
        sc_tag = "  [bold white on red] SAFETY CAR [/] "
    elif state.safety_car == SafetyCarState.VSC:
        sc_tag = "  [bold black on yellow] VSC [/] "
    elif state.safety_car == SafetyCarState.ENDING:
        sc_tag = "  [bold green] SC ENDING [/] "

    weather_icon = _weather_icon(state.weather)
    console.print(
        f"[bold blue]AUTOSPORT MANAGER 2027[/]  "
        f"[white]{state.circuit_name}[/]  "
        f"[yellow]Lap {state.current_lap}/{state.total_laps}[/]  "
        f"{weather_icon}  {state.track_temp_c:.0f}°C"
        f"{sc_tag}"
    )
    console.print()

    # Standings + player telemetry + track map side by side
    from ..core.models import TireCompound
    available_compounds = [TireCompound(n) for n in circuit.available_compounds]
    standings_table = build_standings_table(state, drivers, teams)
    standings_panel = Panel(standings_table, title="[bold]Live Standings[/]",
                            border_style="white", padding=(0, 0))
    player_panel = build_player_panel(state, drivers, teams, circuit, available_compounds)
    weather_panel = build_weather_panel(state)
    track_panel   = build_track_map_panel(state, drivers, teams, circuit)

    right_col = Columns([player_panel, Columns([track_panel, weather_panel], equal=False)],
                        equal=False)
    console.print(Columns(
        [standings_panel, right_col],
        equal=False, expand=True, padding=(0, 2)
    ))

    console.print()
    console.print(build_event_log(state, max_events=8))
    console.print()
    player_cars = state.get_player_cars()
    if player_cars:
        console.print(build_controls_panel(player_cars, drivers))
