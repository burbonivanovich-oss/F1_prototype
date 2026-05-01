"""Season management: championship points, standings, 24-race calendar, save state."""

from __future__ import annotations
from dataclasses import dataclass, field
from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from ..data.circuits import Circuit
    from ..data.teams import Team
    from ..data.drivers import Driver
    from .engine import RaceEngine

# ─── Points tables ────────────────────────────────────────────────────────────

RACE_POINTS   = [25, 18, 15, 12, 10, 8, 6, 4, 2, 1]
SPRINT_POINTS = [8, 7, 6, 5, 4, 3, 2, 1]


def points_for_position(position: int, is_sprint: bool = False) -> int:
    """Returns championship points for a 1-based finishing position."""
    table = SPRINT_POINTS if is_sprint else RACE_POINTS
    idx   = position - 1
    return table[idx] if 0 <= idx < len(table) else 0


# ─── Calendar ─────────────────────────────────────────────────────────────────

@dataclass
class CalendarEntry:
    round: int          # 1-indexed
    circuit_idx: int    # Index into CIRCUITS list
    is_sprint: bool     # Sprint weekend (6 per season)
    date_label: str     # Human-readable date string


# Sprint weekends: rounds 5, 6, 11, 19, 21, 23 (matches C# SeasonEngine)
CALENDAR: list[CalendarEntry] = [
    CalendarEntry(1,  0,  False, "Mar 2"),    # Bahrain GP
    CalendarEntry(2,  1,  False, "Mar 16"),   # Saudi Arabian GP
    CalendarEntry(3,  2,  False, "Mar 30"),   # Australian GP
    CalendarEntry(4,  3,  False, "Apr 6"),    # Japanese GP
    CalendarEntry(5,  4,  True,  "Apr 20"),   # Chinese GP ★ Sprint
    CalendarEntry(6,  5,  True,  "May 4"),    # Miami GP ★ Sprint
    CalendarEntry(7,  6,  False, "May 18"),   # Emilia Romagna GP
    CalendarEntry(8,  7,  False, "May 25"),   # Monaco GP
    CalendarEntry(9,  8,  False, "Jun 1"),    # Spanish GP
    CalendarEntry(10, 9,  False, "Jun 15"),   # Canadian GP
    CalendarEntry(11, 10, True,  "Jun 29"),   # Austrian GP ★ Sprint
    CalendarEntry(12, 11, False, "Jul 6"),    # British GP
    CalendarEntry(13, 12, False, "Jul 27"),   # Belgian GP
    CalendarEntry(14, 13, False, "Aug 3"),    # Hungarian GP
    CalendarEntry(15, 14, False, "Aug 24"),   # Dutch GP
    CalendarEntry(16, 15, False, "Sep 7"),    # Italian GP
    CalendarEntry(17, 16, False, "Sep 21"),   # Azerbaijan GP
    CalendarEntry(18, 17, False, "Oct 5"),    # Singapore GP
    CalendarEntry(19, 18, True,  "Oct 19"),   # United States GP ★ Sprint
    CalendarEntry(20, 19, False, "Oct 26"),   # Mexican GP
    CalendarEntry(21, 20, True,  "Nov 9"),    # São Paulo GP ★ Sprint
    CalendarEntry(22, 21, False, "Nov 22"),   # Las Vegas GP
    CalendarEntry(23, 22, True,  "Nov 30"),   # Qatar GP ★ Sprint
    CalendarEntry(24, 23, False, "Dec 7"),    # Abu Dhabi GP
]


# ─── Standings rows ───────────────────────────────────────────────────────────

@dataclass
class DriverStanding:
    driver_id: int
    points:    int = 0
    wins:      int = 0
    podiums:   int = 0
    poles:     int = 0
    fastest_laps: int = 0
    races:     int = 0


@dataclass
class ConstructorStanding:
    team_id: int
    points:  int = 0
    wins:    int = 0
    podiums: int = 0
    races:   int = 0


# ─── Race result record ───────────────────────────────────────────────────────

@dataclass
class RaceResultRecord:
    """Compact result stored after each race weekend."""
    round:           int
    circuit_idx:     int
    is_sprint:       bool
    finishing_order: list[int] = field(default_factory=list)  # driver IDs, leader first
    dnf_ids:         set[int]  = field(default_factory=set)
    fastest_lap_id:  int       = -1
    pole_sitter_id:  int       = -1


# ─── Season state ─────────────────────────────────────────────────────────────

@dataclass
class SeasonState:
    season:           int   = 2027
    current_round:    int   = 1       # 1-indexed, next race to run
    player_team_id:   int   = 0
    driver_standings: list[DriverStanding]      = field(default_factory=list)
    constructor_standings: list[ConstructorStanding] = field(default_factory=list)
    history:          list[RaceResultRecord]    = field(default_factory=list)

    @property
    def is_complete(self) -> bool:
        return self.current_round > len(CALENDAR)


# ─── SeasonEngine ─────────────────────────────────────────────────────────────

class SeasonEngine:
    """
    Manages the full 24-round championship.

    Usage:
        engine = SeasonEngine(player_team_id=0)
        # ... run a race ...
        engine.record_result(result)
        print(engine.sorted_driver_standings())
    """

    def __init__(self, player_team_id: int = 0, season: int = 2027,
                 drivers: list = None, teams: list = None,
                 saved_state: SeasonState = None):
        if saved_state:
            self.state = saved_state
        else:
            self.state = SeasonState(season=season, player_team_id=player_team_id)
            self._init_standings(drivers or [], teams or [])

    # ── Initialisation ────────────────────────────────────────────────────────

    def _init_standings(self, drivers: list, teams: list) -> None:
        for d in drivers:
            self.state.driver_standings.append(DriverStanding(driver_id=d.id))
        for t in teams:
            self.state.constructor_standings.append(ConstructorStanding(team_id=t.id))

    # ── Calendar helpers ──────────────────────────────────────────────────────

    def current_entry(self) -> CalendarEntry | None:
        if self.state.is_complete:
            return None
        return next((e for e in CALENDAR if e.round == self.state.current_round), None)

    def upcoming_rounds(self, count: int = 5) -> list[CalendarEntry]:
        return [e for e in CALENDAR if e.round >= self.state.current_round][:count]

    def remaining_rounds(self) -> int:
        return len(CALENDAR) - self.state.current_round + 1

    # ── Result recording ──────────────────────────────────────────────────────

    def record_result(self, result: RaceResultRecord, driver_map: dict = None) -> None:
        """Records a race result and updates standings. Advances the round counter."""
        self.state.history.append(result)
        self._apply_points(result, driver_map or {})
        self.state.current_round += 1

    def _apply_points(self, result: RaceResultRecord, driver_map: dict) -> None:
        fl_id = result.fastest_lap_id
        fl_in_points = (fl_id >= 0
                        and fl_id in result.finishing_order[:10]
                        and fl_id not in result.dnf_ids)

        for i, driver_id in enumerate(result.finishing_order):
            dnf = driver_id in result.dnf_ids
            pos = i + 1
            pts = 0 if dnf else points_for_position(pos, result.is_sprint)

            # Fastest lap bonus: +1 for non-sprint races, top 10 only
            if not result.is_sprint and driver_id == fl_id and fl_in_points:
                pts += 1

            # Update driver standing
            ds = next((d for d in self.state.driver_standings if d.driver_id == driver_id), None)
            if ds:
                ds.points += pts
                ds.races  += 1
                if not dnf and pos == 1: ds.wins += 1
                if not dnf and pos <= 3: ds.podiums += 1
                if not result.is_sprint and driver_id == fl_id and fl_in_points:
                    ds.fastest_laps += 1
                if not result.is_sprint and driver_id == result.pole_sitter_id:
                    ds.poles += 1

            # Update constructor standing
            driver = driver_map.get(driver_id)
            if driver is None:
                continue
            cs = next((c for c in self.state.constructor_standings if c.team_id == driver.team_id), None)
            if cs:
                cs.points += pts
                cs.races  += 1
                if not dnf and pos == 1: cs.wins += 1
                if not dnf and pos <= 3: cs.podiums += 1

    # ── Sorted standings ──────────────────────────────────────────────────────

    def sorted_driver_standings(self) -> list[DriverStanding]:
        return sorted(self.state.driver_standings,
                      key=lambda d: (-d.points, -d.wins, -d.podiums))

    def sorted_constructor_standings(self) -> list[ConstructorStanding]:
        return sorted(self.state.constructor_standings,
                      key=lambda c: (-c.points, -c.wins))

    # ── Championship gap ──────────────────────────────────────────────────────

    def championship_gap(self, driver_id: int) -> int:
        """Points behind the championship leader. 0 if the driver is leading."""
        sorted_ = self.sorted_driver_standings()
        if not sorted_:
            return 0
        leader = sorted_[0].points
        my_pts = next((d.points for d in sorted_ if d.driver_id == driver_id), leader)
        return leader - my_pts

    def max_points_remaining(self, is_sprint: bool = False) -> int:
        """Maximum points a driver could still score in the remaining races."""
        per_race = SPRINT_POINTS[0] if is_sprint else RACE_POINTS[0] + 1
        return self.remaining_rounds() * per_race

    # ── Utility ───────────────────────────────────────────────────────────────

    def status_line(self) -> str:
        if self.state.is_complete:
            return f"Season {self.state.season} — FINAL"
        entry = self.current_entry()
        sprint = " [SPRINT]" if entry and entry.is_sprint else ""
        return f"Season {self.state.season} — Round {self.state.current_round}/24{sprint}"

    @staticmethod
    def build_record_from_race_state(
        round_: int,
        circuit_idx: int,
        is_sprint: bool,
        race_state,             # RaceState object from engine
        pole_sitter_id: int = -1,
    ) -> RaceResultRecord:
        """Convenience factory: builds a RaceResultRecord from an engine RaceState."""
        sorted_cars = sorted(
            race_state.cars,
            key=lambda c: (-c.laps_completed, c.total_race_time_s)
        )
        dnf_cars = [c for c in race_state.cars if c.dnf]

        finishing = [c.driver_id for c in sorted_cars if not c.dnf]
        finishing += [c.driver_id for c in dnf_cars]

        fl_id = getattr(race_state, "fastest_lap_driver_id", -1)

        return RaceResultRecord(
            round=round_,
            circuit_idx=circuit_idx,
            is_sprint=is_sprint,
            finishing_order=finishing,
            dnf_ids={c.driver_id for c in dnf_cars},
            fastest_lap_id=fl_id,
            pole_sitter_id=pole_sitter_id,
        )
