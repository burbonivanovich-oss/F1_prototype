# Autosport Manager 2027 — Codebase Guide

## What This Is
A Python terminal-based F1 management simulation prototype. Players manage a constructor team through race weekends: choosing tire strategy, giving driver instructions, timing pit stops, and reacting to safety cars and weather. The prototype validates the core race simulation mechanics before Unity production begins.

**Run the game:**
```bash
cd autosport-manager-2027
pip install rich
python -m src.main
```

## Directory Layout

```
autosport-manager-2027/
  src/
    main.py                 Entry point + interactive race loop
    data/
      circuits.py           24 F1 circuits with authentic track data
      teams.py              10 constructor teams (car performance, reliability, pit skill)
      drivers.py            20 drivers + 2 reserves with individual ratings
    core/
      models.py             All enums and dataclasses (TireCompound, CarState, RaceState, etc.)
      tire.py               3-phase tire degradation model (PLATEAU → LINEAR → CLIFF)
      weather.py            Weather state machine with 5-lap forecast
      overtake.py           Sigmoid overtake formula from GDD §08_TUNING_OVERTAKE_TIRE_v0
      ai.py                 AI pit decisions, compound selection, driver instructions
      engine.py             Main race simulation (RaceEngine.simulate_lap())
    ui/
      display.py            Rich TUI: standings table, player telemetry, event log
      menu.py               Pre-race menus: team/circuit selection, strategy setup
  documentation/            Design documents (GDD, TDD, ADRs)
  requirements.txt
```

## Core Simulation Loop

`RaceEngine.simulate_lap()` is called once per displayed lap. Each call:
1. Advances weather state, generates weather events
2. Manages safety car countdown
3. Executes pit stops (player commands + AI decisions)
4. Computes each car's lap time: `base + car_gap + fuel_penalty + tire_deg + dirty_air + instruction + weather + noise`
5. Resolves overtake attempts (sigmoid formula, DRS gating)
6. Checks mechanical DNF and crash probabilities
7. Updates positions and gaps
8. Updates AI driver instructions for next lap

## Tire Degradation Model (`core/tire.py`)

Three phases — matched to GDD §6.2.3 target lap counts:

| Phase | When | Effect |
|-------|------|--------|
| WARM_UP | First 1-2 laps after pit | Fixed penalty (1.0–2.0s for hard compounds) |
| PLATEAU | Laps 1–(plateau_laps / deg_mult) | Near-zero degradation |
| LINEAR | Beyond plateau | +0.028–0.080s per additional lap |
| CLIFF | Beyond (plateau+linear) / deg_mult | Large per-lap penalty; MUST pit |

Reference: C3 Hard at Silverstone (deg_mult=1.0): cliff at ~44 laps.  
All values scale by `circuit.tire_deg_multiplier` — Monza (0.60) nearly doubles tire life; Monaco (1.40) cuts it by ~30%.

## Overtake Formula (`core/overtake.py`)

From design doc `08_TUNING_OVERTAKE_TIRE_v0.md`:
```
logit = 0.04*(skill_atk-skill_def) + 1.80*pace_delta + 0.05*(-tire_age_delta)
      + 1.50*DRS - 3.00*track_difficulty - 2.00*defender_aggression
P(success) = min(sigmoid(logit), 0.85)
```

Attempt filter: attacker must be within 1.2s AND have at least 0.15s/lap pace advantage (0.05s if DRS active). This prevents ghost-overtakes between similarly-paced cars.

## AI Strategy (`core/ai.py`)

`AIStrategyEngine.should_pit()` returns True when:
- Tire enters CLIFF phase
- Tire window ≤ 2 laps remaining AND race is not nearly over
- Fuel will run out before finish
- Safety car is deployed AND tyre age ≥ 8 laps (85% probability)
- Mandatory two-compound rule not yet fulfilled with ≤ 10 laps remaining

`AIStrategyEngine.choose_compound()` selects fastest compound that won't cliff before race end; always picks a different compound if mandatory rule needs satisfying.

## Player Commands (During Race)

```
b{N}      Box car #{N} with auto compound  (e.g. b4, b81)
b{N}s/m/h  Box car #{N} on Soft/Medium/Hard (e.g. b4s)
a{N}       Attack instruction for #{N}
mn{N}      Manage (economy) for #{N}
d{N}       Defend for #{N}
f{N}       Fuel-save for #{N}
ff         Fast-forward 5 laps
fff        Fast-forward 10 laps
q          Quit race
```

## Key Data Sources

- **24 circuits**: base lap time calibrated to 2024/2025 pole benchmarks; deg_multiplier, overtake difficulty, and DRS zones sourced from GDD §6.2 target tables and real race statistics
- **Driver ratings**: 0–100 scale; VER=98 pace, ALO=97 racecraft, HAM=98 wet_skill. Spread follows ~15s lap-time gap between top and bottom teams
- **Team performance**: 97 (RBR) to 76 (Audi). Each 1-point gap = ~0.055s/lap. Spread ≈ 1.2s from best to worst at same circuit

## Design Documents

- `documentation/01_GAME_OVERVIEW.md` — What the game is; player experience; five interconnected systems
- `documentation/design/06_RACE_SIMULATION_MECHANICS.md` — Tire, fuel, weather, pit stop, overtake, incident formulas
- `documentation/design/08_TUNING_OVERTAKE_TIRE_v0.md` — Sigmoid weights and v0 calibration
- `documentation/technical/01_ARCHITECTURE_OVERVIEW_REVISED.md` — Lap-tick architecture, SnapshotBuffer, Unity integration plan
- `documentation/ARCHITECTURE_DECISION_PHYSICS_VS_SIMPLIFIED.md` — Why hybrid sim was chosen

## New Features Since Initial Build

### ASCII Track Map (`ui/track_map.py`)
All 24 circuits have normalized waypoint silhouettes rendered in a 40×15 ASCII minimap.
Cars appear at their fractional track position (estimated from gap_to_leader / base_lap_time).
Player cars shown as ★, all others as single car-number digit, colored by team.

### Engineer Radio Messages (`core/engine.py` `_engineer_radio()`)
Rate-limited contextual radio messages fire for player cars at key moments:
tire warnings (4/2/1 laps before cliff, CLIFF itself), SC pit window, DRS attack/hunt,
defend warning, fuel caution, final countdown (5/3/1 laps), podium/points encouragement,
fastest lap within reach. ~0.75 messages per driver per lap.

### Pit Stop Strategy Projection (`core/ai.py` `pit_stop_projection()`)
Player panel shows real-time undercut analysis: if pitting now → estimated position,
positions lost to cars within 22s gap, recovery lap. Green/yellow/red by recoverability.

### Lap Time Sparkline (`ui/display.py` `_sparkline()`)
Unicode block chars (▁▂▃▄▅▆▇█) visualize the last 20 lap times in the player panel,
colored by tire phase. Shows `+Xs vs best` delta so tire fall-off is immediately visible.

### Gap Trend Arrows
Standings table shows ▲/▼/= per car based on 3-lap gap trend.
Full gap_history[] stored for all 20 cars throughout the race.

### Strategy Window Calculator (`core/tire.py` `compute_strategy_windows()`)
Computes optimal 1-stop and 2-stop pit lap ranges per circuit using tire physics.
Displayed in pre-race strategy screen. Circuit-specific: Monaco forces 2-stop (1.40x),
Monza enables 1-stop hards (0.60x), Qatar requires 2-stop (1.45x).

### Team Orders System
`TeamOrder` enum: FREE_RACE, HOLD_GAP, SWAP_DRIVERS, PUSH_BOTH.
Commands: `tohold`, `toswap`, `topush`, `tofree` during race.
HOLD_GAP slows following car when within 1.5s of teammate.
SWAP_DRIVERS executes position swap when tire delta ≥5 laps and gap <0.5s.

### Driver Personality Differentiation
- **Wet skill**: Scales weather lap-time penalty (HAM wet=98: 15% less penalty; STR wet=71: 6% more)
- **Race start simulation**: Lap 1 start scoring from racecraft + experience + noise;
  positions clamped to ±3 from grid. ~4 notable start events per race.
- Tire management affects degradation rate (already active since initial build).

### Sector Times (`data/circuits.py`, `core/engine.py`, `ui/display.py`)
Each circuit has `sector_splits: Tuple[float, float, float]` calibrated from real F1 pole data.
S1/S2/S3 computed each lap with ±0.06s independent noise per sector.
Player telemetry shows sectors colored: magenta=personal best, green=clean, yellow=slower, red=degrading.
Monaco S2 = 40% of lap (tunnel), Hungary S2 = 44%, Monza S3 = 36% (Parabolica) — track identity is visible.

### Race-Relative Compound Labels
Standings table and telemetry show H/M/S based on which compound is hardest/softest for THIS circuit.
At Monza (C2/C3/C4): the C4 column shows "S" not "M". At Monaco (C4/C5/C6): C4 shows "H".
Pre-race screen shows "C4 — SOFT this weekend" style labels.

### Circuit Power Sensitivity Model (`data/circuits.py`, `data/teams.py`, `core/engine.py`)
Each team has `power_unit` (engine power, 84–97) and `chassis` (aero package, 73–99) sub-ratings.
Each circuit has `power_sensitivity` (1.0=Monza/Baku, 0.10=Monaco/Singapore).
Lap time formula: `car_gap -= (power_unit - 90) * ps * 0.015 + (chassis - 90) * (1-ps) * 0.015`
Effect: ~±0.10s swing at extreme circuits. Ferrari wins Monza, Red Bull dominates Monaco.
Circuit type badges (PWR/DF/BAL) shown in circuit selector and qualifying screen.

### Driver Momentum System (`core/engine.py`)
Successful overtake gives attacker a -0.10s pace bonus; defender gets +0.08s penalty.
Exponential decay (×0.82/lap) returns to neutral in ~5 laps.
Fastest lap holder sustains a -0.02s/lap sustained benefit.
"Momentum" line in player telemetry: green "on form", bold green "IN THE ZONE", red "struggling".

### Interactive Q1/Q2/Q3 Qualifying (`core/qualifying.py`, `ui/menu.py`)
Full qualifying simulation before each race:
- Q1 (20→15), Q2 (15→10), Q3 (10 cars, pole)
- Track rubber evolution: +0.20s faster for last car in session vs first
- Street circuit traffic: 15% chance of impeded lap at Monaco/Singapore
- **Q2 compound rule**: Top 10 finishers must start race on their Q2 compound
  (key strategic decision: use mediums in Q2 for better race start option)
- Player compound choice per driver per session with grip delta shown
- Timing table shows H/M/S labels, gaps, eliminated markers

### Fastest Lap Bonus Point
Real F1 rule: holder of fastest race lap scores +1 point, but only if they finish P1–P10.
Magenta "FL" badge shown next to driver name in race results. Included in constructor totals.

## Tuning Parameters To Revisit

| Parameter | Current | Flag | Location |
|-----------|---------|------|----------|
| W4_DRS (DRS logit weight) | 1.50 | Flag for playtest | overtake.py |
| W5_TRACK (track difficulty) | 3.00 | Flag for playtest | overtake.py |
| BASE_CRASH_PROB | 0.0015/lap | Monitor DNF rate | engine.py |
| Safety car trigger probability | per-circuit | Monitor SC frequency | circuits.py |
| Tire CLIFF penalty multiplier | 0.3/lap in cliff | Tune for drama | tire.py |
| Start score noise sigma | 6.0 | Tune for drama vs realism | engine.py |
| Wet skill penalty multiplier | 0.0075 | Balance HAM advantage | engine.py |
| Momentum decay rate | 0.82/lap | Balance drama vs stability | engine.py |
| power_sensitivity lap time scale | 0.015s/pt | Circuit type gap width | engine.py |

## Known Prototype Limitations

1. **No season management**: Only single-race prototype; no R&D, contracts, or budget system yet
2. **No save/load**: Each run is fresh
3. **20 cars not 24**: Prototype uses 20 drivers (2 per 10 teams)
4. **Pit lane time fixed**: 20s pit lane loss is a constant approximation
5. **No qualifying replay**: Qualifying is simulated procedurally (no player control)
6. **No sprint format**: Only traditional race weekend
7. **Weather forecast shows condition only**: No rain intensity forecast

## Development Notes

The prototype answers the spike questions from `prototypes/race-sim-spike/PLAN.md`:
- **H1 (UI smoothness)**: Lap-by-lap tick with rich TUI renders cleanly at terminal width
- **H3 (track projection)**: ✅ 40×15 ASCII minimap for all 24 circuits with live car positions
- **H2 (24-car budget)**: Python is well within budget for 20-car sim (sub-ms per lap)
- **H4 (overtake quality)**: Sigmoid formula produces realistic ~15-70 overtakes per race by circuit type
- **H5 (SnapshotBuffer)**: RaceState acts as the snapshot; clean separation between sim and display
