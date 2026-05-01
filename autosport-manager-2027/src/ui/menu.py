"""Pre-race menus: team selection, circuit selection, strategy setup."""

from __future__ import annotations
import os
from typing import Optional

from rich.console import Console
from rich.table import Table
from rich.panel import Panel
from rich.prompt import Prompt, IntPrompt
from rich.text import Text
from rich import box

from ..data.circuits import CIRCUITS, Circuit
from ..data.teams import TEAMS, Team
from ..data.drivers import DRIVERS, Driver
from ..core.models import TireCompound
from ..core.tire import compute_strategy_windows

console = Console()

# Maps compound number → short name + color
COMPOUND_DISPLAY = {
    1: ("C1-UH", "white"),
    2: ("C2-H",  "white"),
    3: ("C3-H",  "white"),
    4: ("C4-M",  "yellow"),
    5: ("C5-S",  "red"),
    6: ("C6-US", "bright_red"),
}


def clear() -> None:
    console.clear()


def splash_screen() -> None:
    clear()
    console.print()
    console.print(
        Panel(
            Text.from_markup(
                "[bold white]AUTOSPORT MANAGER 2027[/]\n\n"
                "[dim]A realistic Formula 1 team management simulation[/]\n"
                "[dim]24 drivers · 10 teams · 24 circuits · deep strategy[/]\n\n"
                "[cyan]v0.1 Prototype — Pre-Production Build[/]"
            ),
            border_style="blue",
            padding=(2, 8),
        )
    )
    console.print()
    try:
        Prompt.ask("[dim]Press Enter to continue[/]", default="")
    except EOFError:
        pass


def select_team() -> Team:
    """Display team selection screen and return chosen Team."""
    while True:
        clear()
        console.print("[bold]SELECT YOUR TEAM[/]")
        console.print()

        table = Table(box=box.SIMPLE_HEAD, show_header=True, header_style="bold white")
        table.add_column("#",          width=3,  justify="center")
        table.add_column("Team",       width=28)
        table.add_column("Car Perf",   width=9,  justify="center")
        table.add_column("Reliability",width=12, justify="center")
        table.add_column("Pit Crew",   width=9,  justify="center")
        table.add_column("Budget $M",  width=10, justify="right")
        table.add_column("Drivers",    width=25)

        for i, team in enumerate(TEAMS, 1):
            team_drivers = [d for d in DRIVERS if d.team_id == team.id]
            drv_str = " / ".join(f"{d.short_name}({d.pace})" for d in team_drivers)
            perf_bar = "█" * (team.car_performance // 10) + "░" * (10 - team.car_performance // 10)
            table.add_row(
                str(i),
                f"[{team.color}]{team.name}[/]",
                f"[{team.color}]{perf_bar}[/] {team.car_performance}",
                f"{team.reliability*100:.0f}%",
                str(team.pit_crew_skill),
                f"{team.budget_m:.0f}",
                drv_str,
            )

        console.print(table)
        console.print()
        console.print("[dim]Your car performance and budget determine your competitive position.[/]")
        console.print("[dim]Hint: Red Bull/Ferrari/McLaren are championship contenders. Williams/Haas are underdogs.[/]")
        console.print()

        try:
            choice = IntPrompt.ask("Choose team", default=1)
            if 1 <= choice <= len(TEAMS):
                selected = TEAMS[choice - 1]
                console.print(f"\n✅ Selected: [{selected.color}]{selected.name}[/]")
                return selected
            console.print("[red]Invalid choice. Try again.[/]")
        except (ValueError, KeyboardInterrupt):
            console.print("[red]Invalid input.[/]")


def select_circuit(team: Team) -> Circuit:
    """Display circuit selection and return chosen Circuit."""
    while True:
        clear()
        console.print(f"[bold]SELECT RACE CIRCUIT[/]  [dim]— Managing: [{team.color}]{team.name}[/][/dim]")
        console.print()

        table = Table(box=box.SIMPLE_HEAD, show_header=True, header_style="bold white")
        table.add_column("#",       width=3,  justify="center")
        table.add_column("Circuit", width=28)
        table.add_column("Country", width=14)
        table.add_column("Laps",    width=5,  justify="center")
        table.add_column("Tyre Deg",width=9,  justify="center")
        table.add_column("Overtake",width=9,  justify="center")
        table.add_column("Rain%",   width=6,  justify="center")
        table.add_column("Compounds",width=12)
        table.add_column("Type",    width=8,  justify="center")

        for i, c in enumerate(CIRCUITS, 1):
            deg_label = ("LOW" if c.tire_deg_multiplier < 0.8
                        else "HIGH" if c.tire_deg_multiplier > 1.2 else "MED")
            deg_color = ("green" if c.tire_deg_multiplier < 0.8
                        else "red" if c.tire_deg_multiplier > 1.2 else "yellow")
            ov_label = ("EASY" if c.overtake_difficulty < 0.3
                       else "HARD" if c.overtake_difficulty > 0.7 else "MED")
            ov_color = ("green" if c.overtake_difficulty < 0.3
                       else "red" if c.overtake_difficulty > 0.7 else "yellow")
            compounds = "/".join(
                COMPOUND_DISPLAY[n][0] for n in c.available_compounds
            )
            street_tag = " [dim]⬛[/dim]" if c.is_street_circuit else ""
            ps = c.power_sensitivity
            if ps >= 0.75:
                type_str = "[red]PWR[/]"
            elif ps <= 0.25:
                type_str = "[cyan]DF[/]"
            elif ps >= 0.60:
                type_str = "[yellow]PWR±[/]"
            elif ps <= 0.40:
                type_str = "[bright_cyan]DF±[/]"
            else:
                type_str = "[dim]BAL[/]"
            table.add_row(
                str(i),
                f"{c.name}{street_tag}",
                c.country,
                str(c.total_laps),
                f"[{deg_color}]{deg_label}[/]",
                f"[{ov_color}]{ov_label}[/]",
                f"{c.rain_probability*100:.0f}%",
                compounds,
                type_str,
            )

        console.print(table)
        console.print()
        console.print("[dim]⬛ = Street circuit. Deg: tyre degradation rate. Overtake: difficulty passing.[/]")
        console.print()

        try:
            choice = IntPrompt.ask("Choose circuit", default=12)  # Default: Silverstone
            if 1 <= choice <= len(CIRCUITS):
                selected = CIRCUITS[choice - 1]
                console.print(f"\n✅ Selected: {selected.name}")
                return selected
            console.print("[red]Invalid choice. Try again.[/]")
        except (ValueError, KeyboardInterrupt):
            console.print("[red]Invalid input.[/]")


def run_qualifying_screen(
    team: Team,
    circuit: Circuit,
    all_drivers: dict,
    all_teams: dict,
    player_drivers: list[Driver],
    rng,
) -> "QualifyingResult":
    """
    Interactive Q1/Q2/Q3 qualifying screen.
    Player chooses compound for their driver(s) before each session.
    Returns QualifyingResult for use in race engine.
    """
    from ..core.qualifying import run_qualifying
    from ..core.tire import TIRE_PROFILES

    available_compounds = [TireCompound(n) for n in circuit.available_compounds]
    avail_dry = [c for c in available_compounds if c.is_dry]
    race_labels = ("HARD", "MEDIUM", "SOFT")
    race_colors = ("white", "yellow", "red")

    def _cmpd_label(c: TireCompound) -> str:
        idx = avail_dry.index(c) if c in avail_dry else 0
        return f"[{race_colors[idx]}]{c.name} ({race_labels[idx]})[/]"

    def _fmt_time(t: float) -> str:
        m = int(t // 60)
        s = t % 60
        return f"{m}:{s:06.3f}"

    def _show_session(session, session_result, player_ids: set[int]) -> None:
        """Display timing screen for a session."""
        clear()
        console.print(f"\n[bold]QUALIFYING — {session}[/]  {circuit.name}")
        console.print()

        table = Table(box=box.SIMPLE_HEAD, show_header=True, header_style="bold white")
        table.add_column("P",      width=3, justify="right")
        table.add_column("Driver", width=20)
        table.add_column("Team",   width=6)
        table.add_column("Tyre",   width=5)
        table.add_column("Time",   width=10, justify="right")
        table.add_column("Gap",    width=9, justify="right")
        table.add_column("",       width=12)  # Status

        laps = session_result.sorted_laps()
        fastest = laps[0].time_s if laps else 0

        for i, lap in enumerate(laps, 1):
            d = all_drivers.get(lap.driver_id)
            t = all_teams.get(d.team_id) if d else None
            if not d or not t:
                continue
            is_player = lap.driver_id in player_ids
            is_elim = lap.driver_id in session_result.eliminated

            gap_str = "POLE" if i == 1 else f"+{lap.time_s - fastest:.3f}"
            status = ""
            row_style = ""

            if is_elim:
                status = "[red]ELIMINATED[/]"
                row_style = "dim"
            elif is_player:
                status = f"[bold yellow]★ YOUR DRIVER[/]"
                row_style = "bold"

            if lap.traffic_impeded:
                status += " [dim](traffic)[/]"

            cmpd_idx = avail_dry.index(lap.compound) if lap.compound in avail_dry else 0
            cmpd_str = f"[{race_colors[cmpd_idx]}]{race_labels[cmpd_idx][0]}[/]"  # H/M/S

            table.add_row(
                str(i),
                f"[{t.color}]{d.short_name}[/]",
                t.short_name,
                cmpd_str,
                _fmt_time(lap.time_s),
                gap_str,
                status,
                style=row_style,
            )

        console.print(table)

    player_ids = {d.id for d in player_drivers}

    # ── Q1 ──────────────────────────────────────────────────────────────────────
    clear()
    ps = circuit.power_sensitivity
    if ps >= 0.75:
        circ_type = "[bold red]POWER CIRCUIT[/] — PU advantage (Ferrari/Mercedes)"
    elif ps <= 0.25:
        circ_type = "[bold green]DOWNFORCE CIRCUIT[/] — Chassis advantage (Red Bull)"
    elif ps >= 0.60:
        circ_type = "[yellow]POWER-BIASED[/]"
    elif ps <= 0.40:
        circ_type = "[cyan]DOWNFORCE-BIASED[/]"
    else:
        circ_type = "[white]BALANCED[/]"

    console.print(f"\n[bold]QUALIFYING[/]  {circuit.name}  {circ_type}")
    console.print(f"[dim]Q1: {len(all_drivers)} cars → 5 eliminated | Q2: 15 → 5 eliminated | Q3: 10 for pole[/]")
    console.print()

    # Player compound choice for Q1
    q1_compounds: dict[int, TireCompound] = {}
    for driver in player_drivers:
        console.print(f"[bold]{driver.name}[/] — Q1 compound:")
        for idx, c in enumerate(avail_dry):
            profile = TIRE_PROFILES.get(c)
            bonus = f"[dim]({-profile.grip_bonus_s:+.2f}s grip)[/]" if profile else ""
            console.print(f"  [{idx+1}] {_cmpd_label(c)}  {bonus}")
        while True:
            try:
                choice = IntPrompt.ask(f"  Choice", default=len(avail_dry))
                if 1 <= choice <= len(avail_dry):
                    q1_compounds[driver.id] = avail_dry[choice - 1]
                    break
                console.print("[red]Invalid.[/]")
            except (ValueError, KeyboardInterrupt):
                break

    try:
        Prompt.ask("[dim]Press Enter to run Q1[/]", default="")
    except EOFError:
        pass

    # Run Q1
    partial_result = run_qualifying(
        circuit, all_drivers, all_teams, team.id, rng,
        player_q1_compounds=q1_compounds,
    )
    _show_session("Q1", partial_result.q1, player_ids)

    # Show who was eliminated
    elim_names = [all_drivers[did].short_name for did in partial_result.q1.eliminated
                  if did in all_drivers]
    console.print(f"\n[red]Q1 ELIMINATED: {', '.join(elim_names)}[/]")
    console.print()

    q1_survivors_set = {l.driver_id for l in partial_result.q1.sorted_laps()[:15]}
    player_survived_q1 = any(d.id in q1_survivors_set for d in player_drivers)
    if not player_survived_q1:
        console.print("[bold red]❌ Your driver was ELIMINATED in Q1![/]")
        try:
            Prompt.ask("[dim]Press Enter to continue[/]", default="")
        except EOFError:
            pass
        # Still run full qualifying for grid
        full = run_qualifying(circuit, all_drivers, all_teams, team.id, rng,
                              player_q1_compounds=q1_compounds)
        return full

    # ── Q2 ──────────────────────────────────────────────────────────────────────
    console.print("[bold]Q2 COMPOUND STRATEGY[/]")
    console.print("[dim]⚑ Top 10 finishers must start the race on their Q2 compound![/]")
    console.print()

    q2_compounds: dict[int, TireCompound] = {}
    for driver in player_drivers:
        if driver.id not in q1_survivors_set:
            continue
        console.print(f"[bold]{driver.name}[/] — Q2 compound:")
        console.print("  [dim]Using SOFT = fastest in Q2, but must start race on softs[/]")
        console.print("  [dim]Using MEDIUM = slightly slower but better race strategy (P6-10 only)[/]")
        for idx, c in enumerate(avail_dry):
            profile = TIRE_PROFILES.get(c)
            bonus = f"[dim]({-profile.grip_bonus_s:+.2f}s grip)[/]" if profile else ""
            console.print(f"  [{idx+1}] {_cmpd_label(c)}  {bonus}")
        while True:
            try:
                choice = IntPrompt.ask(f"  Choice", default=len(avail_dry))
                if 1 <= choice <= len(avail_dry):
                    q2_compounds[driver.id] = avail_dry[choice - 1]
                    break
                console.print("[red]Invalid.[/]")
            except (ValueError, KeyboardInterrupt):
                break

    try:
        Prompt.ask("[dim]Press Enter to run Q2[/]", default="")
    except EOFError:
        pass

    # Re-run qualifying with Q2 compound choices
    partial_q2 = run_qualifying(
        circuit, all_drivers, all_teams, team.id, rng,
        player_q1_compounds=q1_compounds,
        player_q2_compounds=q2_compounds,
    )
    _show_session("Q2", partial_q2.q2, player_ids)

    elim_names_q2 = [all_drivers[did].short_name for did in partial_q2.q2.eliminated
                     if did in all_drivers]
    console.print(f"\n[red]Q2 ELIMINATED: {', '.join(elim_names_q2)}[/]")
    console.print()

    q2_survivors_set = {l.driver_id for l in partial_q2.q2.sorted_laps()[:10]}
    player_survived_q2 = any(d.id in q2_survivors_set for d in player_drivers)
    if not player_survived_q2:
        console.print("[bold yellow]⚠ Your driver was ELIMINATED in Q2 — starts P11-15[/]")
        try:
            Prompt.ask("[dim]Press Enter to continue[/]", default="")
        except EOFError:
            pass
        full = run_qualifying(circuit, all_drivers, all_teams, team.id, rng,
                              player_q1_compounds=q1_compounds,
                              player_q2_compounds=q2_compounds)
        return full

    # ── Q3 ──────────────────────────────────────────────────────────────────────
    q3_compounds: dict[int, TireCompound] = {}
    console.print("[bold]Q3 — SHOOTOUT FOR POLE[/]")
    for driver in player_drivers:
        if driver.id not in q2_survivors_set:
            continue
        console.print(f"[bold]{driver.name}[/] — Q3 compound (free choice, no race start restriction):")
        for idx, c in enumerate(avail_dry):
            profile = TIRE_PROFILES.get(c)
            bonus = f"[dim]({-profile.grip_bonus_s:+.2f}s grip)[/]" if profile else ""
            console.print(f"  [{idx+1}] {_cmpd_label(c)}  {bonus}")
        while True:
            try:
                choice = IntPrompt.ask(f"  Choice", default=len(avail_dry))
                if 1 <= choice <= len(avail_dry):
                    q3_compounds[driver.id] = avail_dry[choice - 1]
                    break
                console.print("[red]Invalid.[/]")
            except (ValueError, KeyboardInterrupt):
                break

    try:
        Prompt.ask("[dim]Press Enter to run Q3[/]", default="")
    except EOFError:
        pass

    # Full qualifying run with all compound choices
    full_result = run_qualifying(
        circuit, all_drivers, all_teams, team.id, rng,
        player_q1_compounds=q1_compounds,
        player_q2_compounds=q2_compounds,
        player_q3_compounds=q3_compounds,
    )
    _show_session("Q3", full_result.q3, player_ids)
    console.print()

    # Final grid announcement
    top3 = full_result.grid_order[:3]
    pole = all_drivers.get(top3[0]) if top3 else None
    if pole:
        pole_t = all_teams.get(pole.team_id)
        console.print(
            f"[bold yellow]POLE POSITION: [{pole_t.color if pole_t else 'white'}]{pole.name}[/] "
            f"({pole_t.name if pole_t else '?'})[/bold yellow]"
        )
    for driver in player_drivers:
        grid_pos = (full_result.grid_order.index(driver.id) + 1
                    if driver.id in full_result.grid_order else "?")
        start_cmpd = full_result.q2_compound_map.get(driver.id)
        cmpd_str = _cmpd_label(start_cmpd) if start_cmpd else "?"
        console.print(f"[bold]{driver.name}[/] starts P{grid_pos} on {cmpd_str}")

    console.print()
    try:
        Prompt.ask("[dim]Press Enter to continue to strategy[/]", default="")
    except EOFError:
        pass

    return full_result


def pre_race_strategy(
    team: Team,
    circuit: Circuit,
    drivers: list[Driver],
) -> dict[int, dict]:
    """
    Pre-race strategy screen: choose starting tire compound for each driver.
    Returns {driver_id: {"compound": TireCompound}}.
    """
    clear()
    console.print(f"[bold]PRE-RACE STRATEGY[/]  [{team.color}]{team.name}[/]  —  {circuit.name}")
    console.print()

    # Show circuit strategy context
    available_compounds = [TireCompound(n) for n in circuit.available_compounds]
    race_labels = ("HARD", "MEDIUM", "SOFT")
    race_colors = ("white", "yellow", "red")

    def _compound_display(c: TireCompound, idx_in_alloc: int) -> str:
        """e.g. 'C4 (SOFT this weekend)' at Monza where C4 is the softest option."""
        label = race_labels[idx_in_alloc]
        color = race_colors[idx_in_alloc]
        return f"[{color}]{c.name} — {label}[/]"

    console.print(f"[yellow]Available compounds this weekend:[/]")
    for idx, c in enumerate(available_compounds):
        console.print(f"  [{idx+1}] {_compound_display(c, idx)}")

    console.print()
    console.print(f"[dim]Circuit: {circuit.name}  Tyre deg: {circuit.tire_deg_multiplier:.2f}x  "
                  f"Rain probability: {circuit.rain_probability*100:.0f}%[/]")
    console.print()

    # Strategy advice
    if circuit.tire_deg_multiplier > 1.2:
        console.print("[yellow]⚑ High degradation — consider harder compounds or plan 2-3 stops.[/]")
    elif circuit.tire_deg_multiplier < 0.8:
        console.print("[green]⚑ Low degradation — soft start viable, 1-stop possible.[/]")
    else:
        console.print("[white]⚑ Medium degradation — typical 2-stop strategy recommended.[/]")

    if circuit.rain_probability > 0.25:
        console.print("[cyan]⚑ High rain probability — be ready to switch to Inters quickly.[/]")

    # Show computed strategy windows
    avail_dry = [c for c in available_compounds if c.is_dry]
    strategies = compute_strategy_windows(avail_dry, circuit.total_laps, circuit.tire_deg_multiplier)
    if strategies:
        console.print("[bold]Recommended strategies:[/]")
        for i, strat in enumerate(strategies[:4], 1):
            stops_str = f"{strat['stops']}-stop"
            star = "[yellow]★[/]" if i == 1 else " "
            console.print(f"  {star} [{i}] {stops_str:8s}  {strat['label']}")
        console.print()


    overrides: dict[int, dict] = {}
    for driver in drivers:
        console.print(f"[bold]{driver.name}[/] (Pace {driver.pace} | Tyre Mgmt {driver.tire_management})")
        console.print("  Starting compound:")
        for idx, c in enumerate(available_compounds):
            console.print(f"    [{idx+1}] {_compound_display(c, idx)}")

        while True:
            try:
                choice = IntPrompt.ask(
                    f"  Choice for {driver.short_name}",
                    default=len(available_compounds)  # Default: softest
                )
                if 1 <= choice <= len(available_compounds):
                    chosen = available_compounds[choice - 1]
                    chosen_idx = available_compounds.index(chosen)
                    console.print(f"  → {_compound_display(chosen, chosen_idx)}")
                    overrides[driver.id] = {"compound": chosen}
                    break
                console.print("[red]Invalid choice.[/]")
            except (ValueError, KeyboardInterrupt):
                pass

        console.print()

    # Fuel load selection (applies to both cars)
    console.print("[bold]Fuel load strategy[/]")
    console.print("  [1] Minimum (97 kg) — lightest, risky")
    console.print("  [2] Standard (105 kg) — recommended")
    console.print("  [3] Maximum (110 kg) — heaviest, safest")
    fuel_choice = IntPrompt.ask("  Fuel load", default=2)
    fuel_map = {1: 97.0, 2: 105.0, 3: 110.0}
    fuel_kg = fuel_map.get(fuel_choice, 105.0)
    for did in overrides:
        overrides[did]["fuel_kg"] = fuel_kg

    console.print()
    console.print(f"[green]✅ Strategy confirmed. Fuel load: {fuel_kg:.0f} kg[/]")
    try:
        Prompt.ask("[dim]Press Enter to start race[/]", default="")
    except EOFError:
        pass

    return overrides


def show_race_result(
    state,
    drivers: dict,
    teams: dict,
    player_team_id: int,
) -> None:
    """Post-race result screen."""
    clear()
    console.print()
    console.print(Panel(
        f"[bold white]RACE RESULT — {state.circuit_name}[/]",
        border_style="yellow", padding=(0, 4),
    ))
    console.print()

    table = Table(box=box.SIMPLE_HEAD, show_header=True, header_style="bold white")
    table.add_column("Pos",    width=4,  justify="center")
    table.add_column("Driver", width=20)
    table.add_column("Team",   width=25)
    table.add_column("Laps",   width=5, justify="center")
    table.add_column("Time/Gap", width=14, justify="right")
    table.add_column("Pts",    width=4, justify="center")

    pts_map = {1:25,2:18,3:15,4:12,5:10,6:8,7:6,8:4,9:2,10:1}
    # Fastest lap bonus: +1 pt if holder finishes in top 10
    fl_holder_id = state.fastest_lap_driver_id
    fl_car = next((c for c in state.cars if c.driver_id == fl_holder_id), None)
    fl_bonus_eligible = (fl_car and not fl_car.dnf and fl_car.position <= 10)

    sorted_cars = state.sorted_cars()
    leader_time = sorted_cars[0].total_race_time_s if sorted_cars else 0

    for car in sorted_cars[:20]:
        driver = drivers.get(car.driver_id)
        team   = teams.get(car.team_id)
        if not driver or not team:
            continue

        is_player = (team.id == player_team_id)
        pts = pts_map.get(car.position, 0) if not car.dnf else 0
        if fl_bonus_eligible and car.driver_id == fl_holder_id:
            pts += 1  # Fastest lap bonus point

        if car.dnf:
            time_str = f"DNF ({car.dnf_reason})"
        elif car.position == 1:
            t = car.total_race_time_s
            h = int(t // 3600)
            m = int((t % 3600) // 60)
            s = t % 60
            time_str = f"{h}:{m:02d}:{s:06.3f}" if h > 0 else f"{m}:{s:06.3f}"
        else:
            gap = car.total_race_time_s - leader_time
            time_str = f"+{gap:.3f}s"

        row_style = "bold" if is_player else ""
        fl_tag = " [bold magenta]FL[/]" if (fl_bonus_eligible and car.driver_id == fl_holder_id) else ""
        table.add_row(
            str(car.position),
            f"[{team.color}]{driver.name}[/]{fl_tag}",
            team.name,
            str(car.laps_completed),
            time_str,
            str(pts),
            style=row_style,
        )

    console.print(table)
    console.print()

    # ── Race statistics ────────────────────────────────────────────────────
    stats_table = Table(box=box.SIMPLE, show_header=False, padding=(0, 2))
    stats_table.add_column("Stat", width=22)
    stats_table.add_column("Value", width=30)

    # Fastest lap
    fl_driver = drivers.get(state.fastest_lap_driver_id)
    if fl_driver and state.fastest_lap_time_s < 9000:
        fl_t = state.fastest_lap_time_s
        m, s = int(fl_t // 60), fl_t % 60
        fl_team = teams.get(fl_driver.team_id)
        fl_color = fl_team.color if fl_team else "white"
        stats_table.add_row(
            "⚡ Fastest Lap",
            f"[{fl_color}]{fl_driver.short_name}[/]  {m}:{s:06.3f}  (L{state.fastest_lap_number})"
        )

    # Total overtakes
    ot_count = sum(1 for e in state.events if e.event_type == "OVERTAKE" and "overtakes" in e.description)
    stats_table.add_row("🔄 Overtakes", str(ot_count))

    # DNFs
    dnf_count = sum(1 for c in state.cars if c.dnf)
    stats_table.add_row("💥 Retirements", str(dnf_count))

    # Safety car laps
    sc_events = [e for e in state.events if e.event_type == "SC" and "SAFETY CAR deployed" in e.description.upper() or "SAFETY CAR" in e.description]
    sc_count = len([e for e in state.events if "SAFETY CAR deployed" in e.description or "🔴 SAFETY CAR" in e.description])
    stats_table.add_row("🚨 Safety Cars", str(sc_count))

    # Driver of the race: most overtakes made (on-track passes)
    overtake_counts: dict[int, int] = {}
    for ev in state.events:
        if ev.event_type == "OVERTAKE" and "overtakes" in ev.description:
            overtake_counts[ev.driver_name] = overtake_counts.get(ev.driver_name, 0) + 1

    dotr_driver = None
    dotr_team = None
    best_ot = 0
    for d_id, d in drivers.items():
        count = overtake_counts.get(d.name, 0)
        if count > best_ot:
            best_ot = count
            dotr_driver = d
            dotr_team = teams.get(d.team_id)

    if dotr_driver and dotr_team:
        fin_pos = next((c.position for c in state.cars if c.driver_id == dotr_driver.id), "?")
        stats_table.add_row(
            "🏆 Driver of Race",
            f"[{dotr_team.color}]{dotr_driver.name}[/]  P{fin_pos}  ({best_ot} overtakes)"
        )

    console.print(Panel(stats_table, title="Race Statistics", border_style="dim", padding=(0, 2)))
    console.print()

    # ── Constructor points this race ──────────────────────────────────────
    team_pts: dict[int, int] = {}
    for car in state.sorted_cars():
        t = teams.get(car.team_id)
        if t:
            car_pts = pts_map.get(car.position, 0) if not car.dnf else 0
            if fl_bonus_eligible and car.driver_id == fl_holder_id:
                car_pts += 1
            team_pts[t.id] = team_pts.get(t.id, 0) + car_pts

    pts_table = Table(box=box.SIMPLE, show_header=True, header_style="bold white", padding=(0, 1))
    pts_table.add_column("Team", width=28)
    pts_table.add_column("Points", width=7, justify="right")
    for t_id, pts in sorted(team_pts.items(), key=lambda x: -x[1]):
        team = teams.get(t_id)
        if not team or pts == 0:
            continue
        is_player = (t_id == player_team_id)
        row_style = "bold" if is_player else ""
        pts_table.add_row(f"[{team.color}]{team.name}[/]", str(pts), style=row_style)

    console.print(Panel(pts_table, title="Constructor Points This Race", border_style="dim yellow", padding=(0, 1)))
    console.print()

    # ── Player summary ────────────────────────────────────────────────────
    player_cars = [c for c in state.cars if c.team_id == player_team_id]
    total_pts = sum(pts_map.get(c.position, 0) for c in player_cars if not c.dnf)
    player_team = teams.get(player_team_id)
    team_str = f"[{player_team.color}]{player_team.name}[/]" if player_team else "Your team"
    console.print(f"[bold]{team_str} scored [yellow]{total_pts} points[/] this race.[/]")

    # DNF warning
    player_dnfs = [c for c in player_cars if c.dnf]
    for car in player_dnfs:
        driver = drivers.get(car.driver_id)
        if driver:
            console.print(f"[red]  ⚠ {driver.name} retired — {car.dnf_reason}[/]")

    console.print()
    try:
        Prompt.ask("[dim]Press Enter to continue[/]", default="")
    except EOFError:
        pass
