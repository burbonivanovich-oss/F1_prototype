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

## Tuning Parameters To Revisit

| Parameter | Current | Flag | Location |
|-----------|---------|------|----------|
| W4_DRS (DRS logit weight) | 1.50 | Flag for playtest | overtake.py |
| W5_TRACK (track difficulty) | 3.00 | Flag for playtest | overtake.py |
| BASE_CRASH_PROB | 0.0015/lap | Monitor DNF rate | engine.py |
| Safety car trigger probability | per-circuit | Monitor SC frequency | circuits.py |
| Tire CLIFF penalty multiplier | 0.3/lap in cliff | Tune for drama | tire.py |

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
- **H3 (track projection)**: Not yet tested (2D/ASCII track map pending)
- **H2 (24-car budget)**: Python is well within budget for 20-car sim (sub-ms per lap)
- **H4 (overtake quality)**: Sigmoid formula produces realistic ~15-70 overtakes per race by circuit type
- **H5 (SnapshotBuffer)**: RaceState acts as the snapshot; clean separation between sim and display
