"""
Autosport Manager 2027 — Race Prototype Entry Point

Run with:  python -m src.main   (from autosport-manager-2027/)
           python src/main.py   (from autosport-manager-2027/)
"""

from __future__ import annotations
import sys
import os

# Make sure imports resolve from the project root
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from rich.console import Console
from rich.prompt import Prompt, IntPrompt

from src.data.circuits import CIRCUITS
from src.data.teams import TEAMS, TEAM_MAP
from src.data.drivers import DRIVERS, DRIVER_MAP
from src.core.models import TireCompound, DriverInstruction, TeamOrder
from src.core.engine import RaceEngine
from src.ui.display import render_race_screen_rich, console
from src.ui.menu import (
    splash_screen, select_team, select_circuit,
    pre_race_strategy, show_race_result, run_qualifying_screen,
)
from src.core.season import SeasonEngine, CALENDAR, RaceResultRecord
from src.core.car_development import CarDevelopmentSystem
from src.core.driver_market import DriverMarket
from src.ui.development_screen import (
    run_development_screen, render_season_standings
)

_console = Console()


# ─── Command parser ───────────────────────────────────────────────────────────

def parse_player_commands(
    raw: str,
    engine: RaceEngine,
    player_drivers: list,
) -> int:
    """
    Parse a player command string and apply to the engine.
    Returns:
      0  = normal (simulate 1 lap)
      5  = fast forward 5 laps
      10 = fast forward 10 laps
     -1  = quit
    """
    cmd = raw.strip().lower()

    if cmd in ("q", "quit", "exit"):
        return -1

    if cmd in ("ff", "5"):
        return 5

    if cmd in ("fff", "10"):
        return 10

    # Team order commands
    team_order_map = {
        "to0": TeamOrder.FREE_RACE,
        "tofree": TeamOrder.FREE_RACE,
        "tohold": TeamOrder.HOLD_GAP,
        "toswap": TeamOrder.SWAP_DRIVERS,
        "topush": TeamOrder.PUSH_BOTH,
    }
    if cmd in team_order_map:
        engine.command_team_order(team_order_map[cmd])
        _console.print(f"[bold yellow]▶ Team order: {team_order_map[cmd].value}[/]")
        return 0

    # Car-specific commands: b1, b44, a4, m81, d18, f63, etc.
    compound_shortcuts = {
        "s": TireCompound.C5,  # soft
        "m": TireCompound.C4,  # medium
        "h": TireCompound.C3,  # hard
        "i": TireCompound.INTER,
        "w": TireCompound.WET,
    }
    instr_shortcuts = {
        "a": DriverInstruction.ATTACK,
        "d": DriverInstruction.DEFEND,
        "mn": DriverInstruction.MANAGE,
        "fs": DriverInstruction.FUEL_SAVE,
    }

    for driver in player_drivers:
        n = str(driver.car_number)
        # Box command: b{n} or b{n}{compound}
        if cmd.startswith("b" + n):
            suffix = cmd[len("b" + n):]
            compound = compound_shortcuts.get(suffix)  # None = auto-choose
            engine.command_pit(driver.id, compound)
            _console.print(f"[cyan]▶ Pit command set for #{n} — {compound.display_name if compound else 'auto compound'}[/]")
            return 0

        # Instruction commands
        for key, instr in instr_shortcuts.items():
            if cmd == key + n or cmd == instr.value[0].lower() + n:
                engine.command_instruction(driver.id, instr)
                _console.print(f"[green]▶ {instr.value} instruction set for #{n}[/]")
                return 0

        # Short instruction by full name prefix
        if cmd.startswith("attack") and n in cmd:
            engine.command_instruction(driver.id, DriverInstruction.ATTACK)
            return 0
        if cmd.startswith("manage") and n in cmd:
            engine.command_instruction(driver.id, DriverInstruction.MANAGE)
            return 0
        if cmd.startswith("defend") and n in cmd:
            engine.command_instruction(driver.id, DriverInstruction.DEFEND)
            return 0
        if cmd.startswith("save") and n in cmd:
            engine.command_instruction(driver.id, DriverInstruction.FUEL_SAVE)
            return 0

    return 0  # Default: advance 1 lap


# ─── Race loop ────────────────────────────────────────────────────────────────

def run_race(
    engine: RaceEngine,
    player_drivers: list,
) -> None:
    """Main interactive race loop."""
    state = engine.race_state

    _console.print("[dim]Race starting in 3... 2... 1... LIGHTS OUT![/]")
    try:
        Prompt.ask("[dim]Press Enter[/]", default="")
    except EOFError:
        pass

    while not state.is_race_complete:
        render_race_screen_rich(state, DRIVER_MAP, TEAM_MAP, engine.circuit)

        laps_remaining = state.total_laps - state.current_lap
        _console.print(
            f"\n[dim]{laps_remaining} laps remaining. "
            "Enter command (or press Enter to advance 1 lap):[/]"
        )

        try:
            raw = input("> ").strip()
        except (EOFError, KeyboardInterrupt):
            _console.print("\n[yellow]Exiting race...[/]")
            return

        advance = parse_player_commands(raw, engine, player_drivers)

        if advance == -1:
            _console.print("[yellow]Quitting race.[/]")
            return

        # Simulate the requested number of laps
        laps_to_sim = advance if advance > 0 else 1
        laps_to_sim = min(laps_to_sim, laps_remaining)

        for _ in range(laps_to_sim):
            if state.is_race_complete:
                break
            engine.simulate_lap()

    # Final render
    render_race_screen_rich(state, DRIVER_MAP, TEAM_MAP, engine.circuit)
    _console.print()
    _console.print("[bold yellow]🏁 Race complete![/]")


# ─── Single-race mode ────────────────────────────────────────────────────────

def run_single_race(player_team, circuit, all_drivers, all_teams, rng) -> None:
    """Runs one standalone race weekend (qualifying + race)."""
    player_drivers = [d for d in DRIVERS if d.team_id == player_team.id]

    qualifying_result = run_qualifying_screen(
        player_team, circuit, all_drivers, all_teams, player_drivers, rng
    )
    overrides = pre_race_strategy(player_team, circuit, player_drivers)

    engine = RaceEngine(
        circuit=circuit,
        teams=all_teams,
        drivers=all_drivers,
        player_team_id=player_team.id,
        player_car_states=overrides,
        qualifying_result=qualifying_result,
    )

    for car in engine.race_state.cars:
        if car.driver_id in overrides:
            car.fuel_kg = overrides[car.driver_id].get("fuel_kg", 105.0)

    run_race(engine, player_drivers)
    show_race_result(engine.race_state, DRIVER_MAP, TEAM_MAP, player_team.id)

    return engine.race_state


# ─── Season mode ─────────────────────────────────────────────────────────────

def run_season_mode(player_team) -> None:
    """
    Manages a full 24-race season: car development, driver market, standings.
    After each race, the player can view standings and develop the car.
    """
    import random
    rng = random.Random()

    all_drivers = {d.id: d for d in DRIVERS if not d.is_reserve}
    all_teams   = {t.id: t for t in TEAMS}
    driver_map  = {d.id: d for d in DRIVERS}
    team_map    = {t.id: t for t in TEAMS}

    # Initialise season systems
    season   = SeasonEngine(player_team_id=player_team.id,
                            drivers=list(all_drivers.values()),
                            teams=TEAMS)
    dev_sys  = CarDevelopmentSystem(teams=TEAMS, rng=rng)
    market   = DriverMarket(drivers=list(all_drivers.values()),
                            teams=TEAMS, rng=rng)

    _console.print(f"\n[bold yellow]🏆 SEASON {season.state.season} — BEGIN[/]")
    _console.print(f"Team: [bold]{player_team.name}[/]  Car performance: {player_team.car_performance}\n")

    while not season.state.is_complete:
        entry   = season.current_entry()
        if entry is None:
            break

        # Clamp circuit index to available CIRCUITS list length
        circuit_idx = min(entry.circuit_idx, len(CIRCUITS) - 1)
        circuit     = CIRCUITS[circuit_idx]

        _console.rule(
            f"[bold cyan]Round {entry.round}/24 — {circuit.name}"
            f"{'  ★ SPRINT' if entry.is_sprint else ''}[/]  {entry.date_label}",
            style="cyan"
        )

        # Pre-race menu
        _console.print("\n[dim]Options before this race:[/]")
        _console.print("  [bold]r[/] — race weekend")
        _console.print("  [bold]d[/] — car development screen")
        _console.print("  [bold]s[/] — season standings")
        _console.print("  [bold]q[/] — quit season")

        action = Prompt.ask("[cyan]Action[/]", default="r").strip().lower()

        if action == "q":
            _console.print("[yellow]Season mode exited.[/]")
            break

        if action == "s":
            render_season_standings(season, driver_map, team_map)
            Prompt.ask("[dim]Press Enter to continue[/]", default="")
            continue  # Re-show same round

        if action == "d":
            run_development_screen(
                dev_sys, player_team.id, player_team.name, player_team.car_performance
            )
            continue  # Re-show same round

        # Tick development for all teams
        tick_result = dev_sys.tick_week(player_team.id)
        if tick_result.completed or tick_result.failed:
            from src.ui.development_screen import render_tick_result
            _console.print("\n[bold cyan]── Development Update ──[/]")
            render_tick_result(tick_result)
            _console.print()

        # AI teams auto-develop
        for t in TEAMS:
            if t.id != player_team.id:
                dev_sys.auto_develop_ai(t.id)
                dev_sys.tick_week(t.id)

        # Run the race weekend
        race_state = run_single_race(player_team, circuit, all_drivers, all_teams, rng)

        # Record result in season standings
        sorted_cars = sorted(
            race_state.cars,
            key=lambda c: (-c.laps_completed, c.total_race_time_s)
        )
        finishing_order = [c.driver_id for c in sorted_cars if not c.dnf]
        finishing_order += [c.driver_id for c in sorted_cars if c.dnf]

        record = RaceResultRecord(
            round=entry.round,
            circuit_idx=entry.circuit_idx,
            is_sprint=entry.is_sprint,
            finishing_order=finishing_order,
            dnf_ids={c.driver_id for c in race_state.cars if c.dnf},
            fastest_lap_id=getattr(race_state, "fastest_lap_driver_id", -1),
        )
        season.record_result(record, driver_map=driver_map)

        # Show brief standings update
        _console.print()
        render_season_standings(season, driver_map, team_map)
        Prompt.ask("[dim]Press Enter for next round[/]", default="")

    _console.print("\n[bold yellow]🏆 SEASON COMPLETE![/]")
    render_season_standings(season, driver_map, team_map)


# ─── Main entry point ─────────────────────────────────────────────────────────

def main() -> None:
    splash_screen()

    # Top-level mode selection
    _console.print("\n[bold]Select mode:[/]")
    _console.print("  [bold cyan]1[/] — Single Race")
    _console.print("  [bold yellow]2[/] — Season Mode (24 rounds)")
    mode = Prompt.ask("[cyan]Mode[/]", choices=["1", "2"], default="1")

    # Team selection (shared)
    player_team = select_team()
    all_drivers = {d.id: d for d in DRIVERS if not d.is_reserve}
    all_teams   = {t.id: t for t in TEAMS}

    import random
    rng = random.Random()

    if mode == "2":
        run_season_mode(player_team)
    else:
        # Single race — pick a circuit
        circuit = select_circuit(player_team)
        run_single_race(player_team, circuit, all_drivers, all_teams, rng)


if __name__ == "__main__":
    main()
