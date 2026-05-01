"""
ASCII track map — renders car positions on simplified circuit silhouettes.

Each circuit has a normalized (x,y) waypoint list forming a closed loop.
Cars are placed at their estimated fractional position along the track,
derived from gap_to_leader_s / base_lap_time_s.
"""

from __future__ import annotations
import math
from typing import TYPE_CHECKING

from rich.text import Text

if TYPE_CHECKING:
    from ..core.models import RaceState, CarState
    from ..data.circuits import Circuit

MAP_W = 40   # characters wide
MAP_H = 15   # characters tall

# ─── Track shapes ─────────────────────────────────────────────────────────────
# Normalized (x, y) waypoints, 0.0–1.0 range, forming a closed loop.
# (0,0) = top-left when rendered; start/finish is the first waypoint.

TRACK_SHAPES: dict[str, list[tuple[float, float]]] = {

    "Bahrain Grand Prix": [
        (0.50, 0.05), (0.72, 0.05), (0.88, 0.12), (0.95, 0.28),
        (0.90, 0.45), (0.75, 0.55), (0.58, 0.60), (0.42, 0.65),
        (0.28, 0.68), (0.15, 0.60), (0.05, 0.48), (0.10, 0.35),
        (0.20, 0.28), (0.28, 0.22), (0.18, 0.12), (0.32, 0.07),
        (0.50, 0.05),
    ],

    "Saudi Arabian Grand Prix": [
        (0.50, 0.02), (0.72, 0.02), (0.88, 0.06), (0.96, 0.15),
        (0.98, 0.28), (0.94, 0.42), (0.85, 0.56), (0.70, 0.66),
        (0.58, 0.72), (0.55, 0.82), (0.62, 0.92), (0.75, 0.96),
        (0.88, 0.96), (0.88, 0.82), (0.78, 0.72), (0.62, 0.62),
        (0.46, 0.58), (0.32, 0.55), (0.18, 0.50), (0.08, 0.40),
        (0.04, 0.28), (0.10, 0.16), (0.22, 0.07), (0.38, 0.02),
        (0.50, 0.02),
    ],

    "Australian Grand Prix": [
        (0.50, 0.05), (0.68, 0.05), (0.80, 0.10), (0.86, 0.20),
        (0.82, 0.32), (0.76, 0.40), (0.82, 0.50), (0.86, 0.60),
        (0.80, 0.70), (0.68, 0.76), (0.54, 0.80), (0.38, 0.80),
        (0.22, 0.74), (0.12, 0.62), (0.10, 0.48), (0.16, 0.36),
        (0.26, 0.28), (0.36, 0.22), (0.28, 0.12), (0.40, 0.06),
        (0.50, 0.05),
    ],

    "Japanese Grand Prix": [   # Suzuka — figure-8
        (0.50, 0.05), (0.65, 0.05), (0.78, 0.10), (0.85, 0.22),
        (0.82, 0.34), (0.72, 0.40), (0.60, 0.45), (0.48, 0.48),
        (0.34, 0.48), (0.22, 0.44), (0.14, 0.34), (0.18, 0.22),
        (0.28, 0.16), (0.40, 0.18), (0.48, 0.28),  # crossover approach
        (0.52, 0.38), (0.56, 0.50), (0.55, 0.62),  # 130R area
        (0.48, 0.70), (0.40, 0.74), (0.30, 0.72),
        (0.20, 0.66), (0.14, 0.56), (0.14, 0.44),
        (0.22, 0.36), (0.34, 0.30),                # cross under
        (0.46, 0.24), (0.50, 0.14), (0.50, 0.05),
    ],

    "Chinese Grand Prix": [
        (0.50, 0.05), (0.65, 0.05), (0.80, 0.10), (0.90, 0.22),
        (0.94, 0.36), (0.90, 0.50), (0.80, 0.58), (0.64, 0.62),
        (0.48, 0.62), (0.30, 0.62), (0.14, 0.60), (0.06, 0.50),
        (0.08, 0.36), (0.16, 0.26), (0.28, 0.18), (0.38, 0.15),
        (0.42, 0.24), (0.40, 0.34), (0.46, 0.42), (0.56, 0.38),
        (0.58, 0.26), (0.52, 0.14), (0.50, 0.05),
    ],

    "Miami Grand Prix": [
        (0.50, 0.05), (0.68, 0.05), (0.82, 0.10), (0.90, 0.20),
        (0.92, 0.32), (0.88, 0.44), (0.78, 0.52), (0.64, 0.58),
        (0.52, 0.62), (0.40, 0.64), (0.28, 0.60), (0.18, 0.52),
        (0.10, 0.42), (0.08, 0.30), (0.14, 0.20), (0.24, 0.12),
        (0.38, 0.07), (0.50, 0.05),
    ],

    "Emilia Romagna Grand Prix": [
        (0.50, 0.05), (0.66, 0.05), (0.80, 0.12), (0.88, 0.24),
        (0.86, 0.36), (0.78, 0.46), (0.66, 0.54), (0.54, 0.60),
        (0.42, 0.64), (0.30, 0.64), (0.18, 0.58), (0.10, 0.46),
        (0.12, 0.34), (0.22, 0.24), (0.34, 0.18), (0.42, 0.12),
        (0.50, 0.05),
    ],

    "Monaco Grand Prix": [
        (0.50, 0.05), (0.64, 0.05), (0.74, 0.10), (0.80, 0.20),
        (0.78, 0.32), (0.72, 0.42), (0.62, 0.50), (0.52, 0.58),
        (0.44, 0.64), (0.32, 0.66), (0.18, 0.66), (0.08, 0.58),
        (0.06, 0.46), (0.10, 0.36), (0.18, 0.30), (0.26, 0.28),
        (0.30, 0.36), (0.28, 0.46), (0.34, 0.56), (0.44, 0.60),
        (0.50, 0.54), (0.54, 0.46), (0.52, 0.36), (0.50, 0.24),
        (0.50, 0.12), (0.50, 0.05),
    ],

    "Canadian Grand Prix": [
        (0.50, 0.05), (0.66, 0.05), (0.80, 0.10), (0.90, 0.22),
        (0.92, 0.36), (0.88, 0.50), (0.78, 0.62), (0.64, 0.70),
        (0.50, 0.74), (0.36, 0.70), (0.22, 0.62), (0.14, 0.50),
        (0.10, 0.36), (0.14, 0.22), (0.26, 0.12), (0.40, 0.07),
        (0.50, 0.05),
    ],

    "Spanish Grand Prix": [
        (0.50, 0.05), (0.66, 0.05), (0.80, 0.10), (0.90, 0.20),
        (0.92, 0.32), (0.88, 0.44), (0.78, 0.52), (0.66, 0.60),
        (0.54, 0.66), (0.42, 0.70), (0.30, 0.68), (0.18, 0.62),
        (0.10, 0.50), (0.08, 0.38), (0.12, 0.26), (0.22, 0.18),
        (0.36, 0.10), (0.50, 0.05),
    ],

    "Austrian Grand Prix": [
        (0.50, 0.05), (0.64, 0.05), (0.76, 0.14), (0.82, 0.28),
        (0.80, 0.42), (0.72, 0.54), (0.58, 0.64), (0.46, 0.74),
        (0.34, 0.80), (0.24, 0.74), (0.18, 0.62), (0.22, 0.48),
        (0.30, 0.38), (0.36, 0.28), (0.42, 0.18), (0.50, 0.05),
    ],

    "British Grand Prix": [
        (0.50, 0.05), (0.66, 0.05), (0.80, 0.08), (0.90, 0.16),
        (0.94, 0.30), (0.88, 0.42), (0.78, 0.48), (0.68, 0.52),
        (0.60, 0.62), (0.48, 0.66), (0.32, 0.66), (0.18, 0.60),
        (0.08, 0.50), (0.06, 0.36), (0.10, 0.22), (0.18, 0.16),
        (0.26, 0.10), (0.38, 0.05), (0.50, 0.05),
    ],

    "Hungarian Grand Prix": [
        (0.50, 0.05), (0.64, 0.05), (0.76, 0.14), (0.82, 0.26),
        (0.84, 0.38), (0.78, 0.48), (0.68, 0.56), (0.56, 0.62),
        (0.44, 0.66), (0.34, 0.64), (0.22, 0.58), (0.14, 0.48),
        (0.12, 0.36), (0.18, 0.26), (0.26, 0.18), (0.36, 0.12),
        (0.50, 0.05),
    ],

    "Belgian Grand Prix": [
        (0.50, 0.05), (0.64, 0.05), (0.78, 0.08), (0.90, 0.14),
        (0.96, 0.24), (0.92, 0.36), (0.84, 0.46), (0.72, 0.52),
        (0.62, 0.58), (0.52, 0.65), (0.42, 0.72), (0.32, 0.76),
        (0.22, 0.72), (0.14, 0.64), (0.10, 0.54), (0.10, 0.42),
        (0.14, 0.30), (0.22, 0.20), (0.34, 0.12), (0.50, 0.05),
    ],

    "Dutch Grand Prix": [
        (0.50, 0.05), (0.66, 0.05), (0.80, 0.10), (0.90, 0.20),
        (0.94, 0.34), (0.90, 0.48), (0.80, 0.58), (0.66, 0.65),
        (0.52, 0.70), (0.38, 0.67), (0.26, 0.60), (0.18, 0.50),
        (0.14, 0.38), (0.18, 0.26), (0.28, 0.16), (0.42, 0.08),
        (0.50, 0.05),
    ],

    "Italian Grand Prix": [   # Monza — fast oval with chicanes
        (0.50, 0.05), (0.66, 0.05), (0.82, 0.05), (0.92, 0.10),
        (0.96, 0.22), (0.94, 0.34), (0.86, 0.44), (0.76, 0.50),
        (0.68, 0.54), (0.62, 0.62), (0.60, 0.72), (0.50, 0.80),
        (0.38, 0.82), (0.28, 0.76), (0.22, 0.66), (0.20, 0.54),
        (0.22, 0.42), (0.30, 0.30), (0.38, 0.18), (0.50, 0.05),
    ],

    "Azerbaijan Grand Prix": [
        (0.50, 0.05), (0.66, 0.05), (0.82, 0.05), (0.94, 0.10),
        (0.98, 0.20), (0.96, 0.32), (0.90, 0.44), (0.80, 0.54),
        (0.70, 0.62), (0.58, 0.68), (0.46, 0.74), (0.36, 0.70),
        (0.26, 0.64), (0.18, 0.54), (0.12, 0.42), (0.10, 0.30),
        (0.14, 0.20), (0.24, 0.12), (0.38, 0.07), (0.50, 0.05),
    ],

    "Singapore Grand Prix": [
        (0.50, 0.05), (0.64, 0.05), (0.76, 0.10), (0.84, 0.18),
        (0.90, 0.28), (0.88, 0.40), (0.82, 0.52), (0.72, 0.62),
        (0.60, 0.70), (0.50, 0.76), (0.40, 0.70), (0.30, 0.62),
        (0.20, 0.54), (0.12, 0.44), (0.08, 0.34), (0.12, 0.22),
        (0.22, 0.13), (0.36, 0.07), (0.50, 0.05),
    ],

    "United States Grand Prix": [
        (0.50, 0.05), (0.58, 0.05), (0.68, 0.10), (0.80, 0.16),
        (0.90, 0.24), (0.94, 0.36), (0.90, 0.48), (0.82, 0.56),
        (0.72, 0.62), (0.60, 0.66), (0.50, 0.70), (0.38, 0.74),
        (0.28, 0.72), (0.18, 0.64), (0.12, 0.52), (0.10, 0.40),
        (0.14, 0.28), (0.24, 0.18), (0.38, 0.10), (0.50, 0.05),
    ],

    "Mexico City Grand Prix": [
        (0.50, 0.05), (0.66, 0.05), (0.80, 0.10), (0.88, 0.20),
        (0.90, 0.34), (0.84, 0.46), (0.74, 0.54), (0.62, 0.60),
        (0.50, 0.64), (0.38, 0.66), (0.26, 0.62), (0.14, 0.54),
        (0.08, 0.42), (0.08, 0.30), (0.14, 0.20), (0.24, 0.12),
        (0.38, 0.07), (0.50, 0.05),
    ],

    "São Paulo Grand Prix": [  # Interlagos — anti-clockwise
        (0.50, 0.05), (0.36, 0.05), (0.22, 0.10), (0.12, 0.20),
        (0.08, 0.32), (0.10, 0.44), (0.18, 0.54), (0.30, 0.60),
        (0.44, 0.64), (0.56, 0.68), (0.68, 0.72), (0.78, 0.66),
        (0.86, 0.56), (0.90, 0.44), (0.90, 0.32), (0.84, 0.20),
        (0.74, 0.12), (0.62, 0.06), (0.50, 0.05),
    ],

    "Las Vegas Grand Prix": [
        (0.50, 0.05), (0.70, 0.05), (0.86, 0.05), (0.94, 0.12),
        (0.96, 0.24), (0.94, 0.36), (0.90, 0.50), (0.80, 0.60),
        (0.70, 0.68), (0.60, 0.72), (0.50, 0.70), (0.40, 0.64),
        (0.30, 0.56), (0.20, 0.48), (0.12, 0.38), (0.10, 0.26),
        (0.16, 0.15), (0.26, 0.08), (0.40, 0.05), (0.50, 0.05),
    ],

    "Qatar Grand Prix": [
        (0.50, 0.05), (0.66, 0.05), (0.82, 0.08), (0.92, 0.16),
        (0.96, 0.28), (0.94, 0.40), (0.88, 0.52), (0.78, 0.60),
        (0.64, 0.65), (0.50, 0.67), (0.36, 0.64), (0.24, 0.56),
        (0.16, 0.46), (0.12, 0.34), (0.14, 0.22), (0.22, 0.12),
        (0.36, 0.06), (0.50, 0.05),
    ],

    "Abu Dhabi Grand Prix": [
        (0.50, 0.05), (0.66, 0.05), (0.80, 0.10), (0.90, 0.20),
        (0.94, 0.32), (0.92, 0.44), (0.86, 0.54), (0.74, 0.60),
        (0.62, 0.64), (0.50, 0.67), (0.40, 0.70), (0.28, 0.64),
        (0.18, 0.56), (0.12, 0.44), (0.10, 0.32), (0.14, 0.22),
        (0.22, 0.14), (0.34, 0.08), (0.50, 0.05),
    ],
}

_FALLBACK_OVAL: list[tuple[float, float]] = [
    (0.50, 0.05), (0.66, 0.05), (0.82, 0.10), (0.92, 0.22),
    (0.94, 0.40), (0.90, 0.58), (0.80, 0.70), (0.64, 0.80),
    (0.50, 0.85), (0.36, 0.80), (0.20, 0.70), (0.10, 0.58),
    (0.06, 0.40), (0.08, 0.22), (0.18, 0.10), (0.34, 0.05),
    (0.50, 0.05),
]


# ─── Drawing helpers ──────────────────────────────────────────────────────────

def _bresenham(
    x0: int, y0: int, x1: int, y1: int
) -> list[tuple[int, int]]:
    pts = []
    dx = abs(x1 - x0); dy = abs(y1 - y0)
    sx = 1 if x1 > x0 else -1
    sy = 1 if y1 > y0 else -1
    err = dx - dy
    while True:
        pts.append((x0, y0))
        if x0 == x1 and y0 == y1:
            break
        e2 = 2 * err
        if e2 > -dy:
            err -= dy; x0 += sx
        if e2 < dx:
            err += dx; y0 += sy
    return pts


def _normalize(waypoints: list[tuple[float, float]], pad: float = 0.06) -> list[tuple[float, float]]:
    """Remap waypoints so the bounding box fills [pad, 1-pad] in both axes."""
    xs = [p[0] for p in waypoints]
    ys = [p[1] for p in waypoints]
    x0, x1 = min(xs), max(xs)
    y0, y1 = min(ys), max(ys)
    rx = (x1 - x0) or 1.0
    ry = (y1 - y0) or 1.0
    inner = 1.0 - 2 * pad
    return [
        (pad + (x - x0) / rx * inner, pad + (y - y0) / ry * inner)
        for x, y in waypoints
    ]


def _arc_lengths(waypoints: list[tuple[float, float]]) -> list[float]:
    lengths = [0.0]
    for i in range(1, len(waypoints)):
        x0, y0 = waypoints[i - 1]
        x1, y1 = waypoints[i]
        lengths.append(lengths[-1] + math.hypot(x1 - x0, y1 - y0))
    return lengths


def _point_at(
    waypoints: list[tuple[float, float]],
    arc_lens: list[float],
    frac: float,
) -> tuple[float, float]:
    total = arc_lens[-1]
    if total == 0:
        return waypoints[0]
    target = max(0.0, min(1.0, frac)) * total
    for i in range(len(arc_lens) - 1):
        if arc_lens[i] <= target <= arc_lens[i + 1]:
            seg = arc_lens[i + 1] - arc_lens[i]
            t = (target - arc_lens[i]) / seg if seg > 0 else 0.0
            x0, y0 = waypoints[i]
            x1, y1 = waypoints[i + 1]
            return x0 + t * (x1 - x0), y0 + t * (y1 - y0)
    return waypoints[-1]


# ─── Public API ───────────────────────────────────────────────────────────────

def render_track_map(
    circuit: "Circuit",
    state: "RaceState",
    drivers_by_id: dict,
    teams_by_id: dict,
) -> Text:
    """
    Return a Rich Text object containing the ASCII track map with car markers.
    Width = MAP_W chars, Height = MAP_H lines.
    """
    raw_waypoints = TRACK_SHAPES.get(circuit.name, _FALLBACK_OVAL)
    waypoints = _normalize(raw_waypoints)
    arc_lens = _arc_lengths(waypoints)

    # ── Build grid ────────────────────────────────────────────────────────────
    # Each cell: (' ', None) = empty; ('·', None) = track; (char, color) = car
    grid: list[list[tuple[str, str | None]]] = [
        [(' ', None)] * MAP_W for _ in range(MAP_H)
    ]

    def to_col(x: float) -> int:
        return max(0, min(MAP_W - 1, int(round(x * (MAP_W - 1)))))

    def to_row(y: float) -> int:
        return max(0, min(MAP_H - 1, int(round(y * (MAP_H - 1)))))

    # Draw track outline
    n = len(waypoints)
    for i in range(n - 1):
        x0c = to_col(waypoints[i][0]);     y0r = to_row(waypoints[i][1])
        x1c = to_col(waypoints[i + 1][0]); y1r = to_row(waypoints[i + 1][1])
        for col, row in _bresenham(x0c, y0r, x1c, y1r):
            if grid[row][col][0] == ' ':
                grid[row][col] = ('·', 'bright_black')

    # ── Mark SF line ─────────────────────────────────────────────────────────
    sf_x, sf_y = waypoints[0]
    sf_col, sf_row = to_col(sf_x), to_row(sf_y)
    grid[sf_row][sf_col] = ('|', 'white')

    # ── Compute car fractional positions ─────────────────────────────────────
    base_t = circuit.base_lap_time_s
    sorted_cars = state.sorted_cars()

    car_fracs: list[tuple["CarState", float]] = []
    for car in sorted_cars:
        if car.dnf:
            continue
        gap = car.gap_to_leader_s
        if gap >= 9000:
            frac = max(0.02, 0.45 - (car.position - 15) * 0.02)
        else:
            frac = max(0.01, min(0.99, 1.0 - gap / base_t))
        car_fracs.append((car, frac))

    # Enforce minimum visual separation (2% of track) so cars don't pile up
    MIN_SEP = 0.022
    for i in range(1, len(car_fracs)):
        prev_frac = car_fracs[i - 1][1]
        car, frac = car_fracs[i]
        if prev_frac - frac < MIN_SEP:
            car_fracs[i] = (car, prev_frac - MIN_SEP)

    # Place cars on grid — player team last so they draw over others
    player_fracs = [(c, f) for c, f in car_fracs if c.team_id == state.player_team_id]
    other_fracs  = [(c, f) for c, f in car_fracs if c.team_id != state.player_team_id]

    for car, frac in other_fracs + player_fracs:
        nx, ny = _point_at(waypoints, arc_lens, frac)
        col = to_col(nx)
        row = to_row(ny)

        team = teams_by_id.get(car.team_id)
        driver = drivers_by_id.get(car.driver_id)

        if car.team_id == state.player_team_id:
            char = '★'
            color = team.color if team else 'white'
        else:
            # Use last 1-2 chars of car number; single digit looks cleanest
            char = str(car.car_number % 10)
            color = team.color if team else 'dim'

        grid[row][col] = (char, color)

    # ── Render to Rich Text ───────────────────────────────────────────────────
    out = Text()
    for row_idx, row in enumerate(grid):
        for char, color in row:
            if color:
                out.append(char, style=color)
            else:
                out.append(char)
        if row_idx < MAP_H - 1:
            out.append('\n')

    return out
