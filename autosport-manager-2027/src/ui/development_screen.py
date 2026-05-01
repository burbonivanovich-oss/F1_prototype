"""Car Development Screen — Rich TUI for the upgrade tree and R&D management."""

from __future__ import annotations
from typing import Optional
from rich.console import Console
from rich.table import Table
from rich.panel import Panel
from rich.columns import Columns
from rich.text import Text
from rich.prompt import Prompt, IntPrompt
from rich import box

from ..core.car_development import (
    CarDevelopmentSystem, TeamDevState, UpgradeNode, UpgradeArea, DevTickResult
)

_console = Console()


# ─── Colours ──────────────────────────────────────────────────────────────────

_AREA_COLORS: dict[UpgradeArea, str] = {
    UpgradeArea.POWER_UNIT: "red",
    UpgradeArea.AERO_FRONT: "cyan",
    UpgradeArea.AERO_REAR:  "blue",
    UpgradeArea.CHASSIS:    "yellow",
    UpgradeArea.SUSPENSION: "green",
    UpgradeArea.GEARBOX:    "magenta",
    UpgradeArea.COOLING:    "white",
}

_AREA_ICONS: dict[UpgradeArea, str] = {
    UpgradeArea.POWER_UNIT: "⚡",
    UpgradeArea.AERO_FRONT: "🔵",
    UpgradeArea.AERO_REAR:  "🔷",
    UpgradeArea.CHASSIS:    "🏎",
    UpgradeArea.SUSPENSION: "🔧",
    UpgradeArea.GEARBOX:    "⚙",
    UpgradeArea.COOLING:    "❄",
}


# ─── Node status helpers ──────────────────────────────────────────────────────

def _node_status_text(node: UpgradeNode) -> Text:
    if node.is_complete:
        return Text("✓ DONE", style="bold green")
    if node.is_in_dev:
        return Text(f"⏳ {node.weeks_remaining}w", style="bold yellow")
    if not node.is_unlocked:
        return Text("🔒 LOCKED", style="dim")
    return Text("▶ AVAILABLE", style="bold cyan")


def _impact_bar(impact: float, max_impact: float = 3.0, width: int = 10) -> str:
    filled = min(width, int(impact / max_impact * width))
    return "█" * filled + "░" * (width - filled)


# ─── Upgrade tree table ───────────────────────────────────────────────────────

def render_upgrade_tree(state: TeamDevState, team_name: str) -> None:
    """Renders the full upgrade tree in a Rich table."""
    _console.print()
    _console.print(Panel(
        f"[bold white]🔬 CAR DEVELOPMENT — {team_name}[/]",
        subtitle=(f"[cyan]Tokens: {state.tokens}[/]  "
                  f"[yellow]R&D Points: {state.rd_points}[/]  "
                  f"[green]In dev: {len(state.active_ids)}[/]"),
        border_style="bright_blue",
    ))

    table = Table(
        show_header=True, header_style="bold white",
        box=box.SIMPLE_HEAD, min_width=90, expand=False,
    )
    table.add_column("ID",       style="bold", width=10)
    table.add_column("Area",     width=14)
    table.add_column("Tier",     width=5,  justify="center")
    table.add_column("Cost",     width=14, justify="center")
    table.add_column("Weeks",    width=6,  justify="center")
    table.add_column("Success%", width=8,  justify="center")
    table.add_column("Impact",   width=14)
    table.add_column("Status",   width=14)

    # Group by area for visual clarity
    from itertools import groupby
    sorted_nodes = sorted(state.tree, key=lambda n: (n.area.value, n.tier))
    for area, nodes in groupby(sorted_nodes, key=lambda n: n.area):
        color = _AREA_COLORS.get(area, "white")
        icon  = _AREA_ICONS.get(area, "•")
        for node in nodes:
            impact_bar = _impact_bar(node.total_impact)
            success_str = f"{node.success_probability*100:.0f}%"
            if node.success_probability < 0.75:
                success_str = f"[red]{success_str}[/]"
            elif node.success_probability >= 0.90:
                success_str = f"[green]{success_str}[/]"

            cost_str = f"{node.token_cost}T / {node.rd_points_cost}R"

            table.add_row(
                f"[{color}]{node.id}[/]",
                f"{icon} [{color}]{area.name}[/]",
                str(node.tier),
                cost_str,
                str(node.weeks_to_develop),
                success_str,
                f"[bold]{impact_bar}[/] +{node.total_impact:.1f}",
                _node_status_text(node),
            )

    _console.print(table)


# ─── In-progress panel ───────────────────────────────────────────────────────

def render_active_developments(state: TeamDevState) -> None:
    if not state.active_ids:
        _console.print("[dim]No upgrades currently in development.[/]\n")
        return

    panels = []
    for uid in state.active_ids:
        node = next((n for n in state.tree if n.id == uid), None)
        if node is None:
            continue
        color = _AREA_COLORS.get(node.area, "white")
        bar   = "█" * (node.weeks_to_develop - node.weeks_remaining)
        bar  += "░" * node.weeks_remaining
        content = Text()
        content.append(f"  Area:    ", style="dim")
        content.append(f"{node.area.name}\n", style=color)
        content.append(f"  Impact:  ", style="dim")
        content.append(f"+{node.total_impact:.1f} perf\n")
        content.append(f"  Progress: [{bar}] {node.weeks_remaining}w left\n")
        content.append(f"  Success:  {node.success_probability*100:.0f}%\n")
        panels.append(Panel(content, title=f"[bold {color}]{uid}[/]",
                            border_style=color, width=30))

    _console.print(Columns(panels, equal=True, expand=False))
    _console.print()


# ─── Tick result summary ─────────────────────────────────────────────────────

def render_tick_result(result: DevTickResult) -> None:
    if result.completed:
        for uid in result.completed:
            _console.print(f"  [bold green]✓ UPGRADE COMPLETE:[/] {uid}")
    if result.failed:
        for uid in result.failed:
            _console.print(f"  [bold red]✗ UPGRADE FAILED:[/] {uid} — prototype scrapped, cost paid")
    if not result.completed and not result.failed:
        _console.print(f"  [dim]+{result.rd_earned} R&D points accrued.[/]")


# ─── Available upgrades panel ─────────────────────────────────────────────────

def render_available_upgrades(available: list[UpgradeNode]) -> None:
    if not available:
        _console.print("[dim]No upgrades available to start.[/]\n")
        return

    _console.print(Panel("[bold cyan]AVAILABLE UPGRADES[/]", border_style="cyan"))
    for i, node in enumerate(available):
        color = _AREA_COLORS.get(node.area, "white")
        _console.print(
            f"  [{i+1}] [{color}]{node.id:<10}[/] "
            f"T{node.token_cost} / {node.rd_points_cost}R  "
            f"{node.weeks_to_develop}w  "
            f"{node.success_probability*100:.0f}%  "
            f"+{node.total_impact:.1f} perf"
        )
    _console.print()


# ─── Performance delta summary ────────────────────────────────────────────────

def render_performance_summary(state: TeamDevState, base_perf: int) -> None:
    effective = base_perf + state.car_perf_delta

    table = Table(box=box.SIMPLE, show_header=False, min_width=40)
    table.add_column("Stat",  style="dim")
    table.add_column("Value", justify="right")
    table.add_column("Delta", justify="right")

    def _delta(d: float) -> str:
        if d > 0: return f"[green]+{d:.1f}[/]"
        if d < 0: return f"[red]{d:.1f}[/]"
        return "[dim]—[/]"

    table.add_row("Car Performance", f"[bold white]{effective:.1f}[/]",
                  _delta(state.car_perf_delta))
    table.add_row("Power Unit",      "—", _delta(state.power_unit_delta))
    table.add_row("Chassis",         "—", _delta(state.chassis_delta))
    table.add_row("Reliability",     "—", _delta(state.reliability_delta))

    _console.print(Panel(table, title="[bold]Season Progress[/]", border_style="blue"))


# ─── Main interactive screen ──────────────────────────────────────────────────

def run_development_screen(
    dev_system: CarDevelopmentSystem,
    team_id:    int,
    team_name:  str,
    base_perf:  int,
) -> None:
    """
    Full interactive car development screen.
    Shows the upgrade tree, lets the player start/review developments.
    Exits when the player types 'done' or 'back'.
    """
    state = dev_system.get_state(team_id)
    if state is None:
        _console.print(f"[red]No development data found for team {team_id}.[/]")
        return

    while True:
        _console.clear()
        render_upgrade_tree(state, team_name)

        _console.print()
        _console.rule("[bold cyan]IN DEVELOPMENT[/]", style="cyan")
        render_active_developments(state)

        _console.rule("[bold cyan]PERFORMANCE DELTAS[/]", style="cyan")
        render_performance_summary(state, base_perf)

        _console.print()
        available = dev_system.available_upgrades(team_id)
        render_available_upgrades(available)

        # Commands
        _console.print("[dim]Commands: [bold]start <ID>[/] — begin upgrade  "
                       "| [bold]done[/] — exit development screen[/]")
        cmd = Prompt.ask("\n[cyan]Development command[/]", default="done").strip().lower()

        if cmd in ("done", "back", "exit", "q"):
            break

        if cmd.startswith("start "):
            upgrade_id = cmd[6:].strip().upper()
            ok, err = dev_system.start_development(team_id, upgrade_id)
            if ok:
                _console.print(f"[bold green]✓ Development started: {upgrade_id}[/]")
            else:
                _console.print(f"[bold red]✗ Cannot start {upgrade_id}: {err}[/]")
            Prompt.ask("[dim]Press Enter to continue[/]", default="")
        else:
            _console.print("[red]Unknown command.[/]")
            Prompt.ask("[dim]Press Enter to continue[/]", default="")


# ─── Season standings screen ──────────────────────────────────────────────────

def render_season_standings(season_engine, driver_map: dict, team_map: dict) -> None:
    """Renders driver and constructor standings as Rich tables."""
    from ..core.season import SeasonEngine

    _console.print()
    _console.print(Panel(
        f"[bold white]{season_engine.status_line()}[/]",
        border_style="yellow",
    ))

    # Driver standings
    drv_table = Table(title="[bold]DRIVER STANDINGS[/]", box=box.SIMPLE_HEAD,
                      header_style="bold white", min_width=60)
    drv_table.add_column("Pos", width=4,  justify="center")
    drv_table.add_column("Driver",        width=22)
    drv_table.add_column("Team",          width=18, style="dim")
    drv_table.add_column("Pts",  width=6, justify="right", style="bold yellow")
    drv_table.add_column("W",    width=4, justify="center")
    drv_table.add_column("Pd",   width=4, justify="center")
    drv_table.add_column("Poles",width=5, justify="center")

    for pos, ds in enumerate(season_engine.sorted_driver_standings(), 1):
        driver = driver_map.get(ds.driver_id)
        dname  = driver.name if driver else f"ID{ds.driver_id}"
        team   = team_map.get(getattr(driver, "team_id", -1))
        tname  = team.short_name if team else "???"
        style  = "bold" if pos == 1 else ""
        drv_table.add_row(
            str(pos), dname, tname,
            str(ds.points), str(ds.wins), str(ds.podiums), str(ds.poles),
            style=style,
        )

    # Constructor standings
    con_table = Table(title="[bold]CONSTRUCTOR STANDINGS[/]", box=box.SIMPLE_HEAD,
                      header_style="bold white", min_width=40)
    con_table.add_column("Pos", width=4, justify="center")
    con_table.add_column("Team",          width=24)
    con_table.add_column("Pts", width=6,  justify="right", style="bold yellow")
    con_table.add_column("W",   width=4,  justify="center")

    for pos, cs in enumerate(season_engine.sorted_constructor_standings(), 1):
        team  = team_map.get(cs.team_id)
        tname = team.name if team else f"ID{cs.team_id}"
        tcolor= getattr(team, "color", "white") if team else "white"
        style = "bold" if pos == 1 else ""
        con_table.add_row(
            str(pos), f"[{tcolor}]{tname}[/]",
            str(cs.points), str(cs.wins),
            style=style,
        )

    _console.print(Columns([drv_table, con_table], equal=False, expand=False))
    _console.print()
