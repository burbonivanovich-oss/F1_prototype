# F1Manager2027-Unity — Codebase Guide

## What This Is

A Unity 2022.3 LTS port of the `autosport-manager-2027` Python prototype.
All simulation logic lives in pure C# (no `UnityEngine` imports) and is fully
testable in NUnit EditMode without a running scene. Unity handles rendering and
input via UI Toolkit (UXML + USS).

## How To Open

1. Install **Unity 2022.3.20f1** (or any 2022.3.x LTS)
2. Open this folder as a Unity project
3. Create a Scene with two GameObjects:
   - `GameManager` → attach `GameManager.cs`, assign `MainMenu.uxml` to `menuDocument`
     and `RaceScreen.uxml` to `raceDocument`
   - `UIRoot` → attach a `UIDocument` for each document above
4. Hit Play

## Directory Layout

```
Assets/
  Scripts/
    Core/                      Pure C# simulation — no Unity deps
      Enums.cs                 TireCompound, DriverInstruction, WeatherCondition, TirePhase,
                               TeamOrder + extension methods: IsDry(), DisplayName(), GripAdvantageS()
      Models.cs                TireProfile, TireProfiles (all 8 compounds), CarState,
                               RaceEvent, PitStopResult, RaceState
      TireSystem.cs            4-phase degradation: GetPhase(), DegPenaltyS(),
                               WindowRemaining(), BestCompoundForStint()
      WeatherSystem.cs         Rain event planner, lap-by-lap Advance(), LapTimeWeatherPenaltyS(),
                               AquaplaningChance(), RecommendedCompound()
      OvertakeSystem.cs        Sigmoid overtake formula, DRS gating, dirty-air penalty.
                               ComputeSuccessProbability() static helper exposes raw sigmoid inputs
                               without CarState objects (used by tests and balance tooling).
      AIStrategyEngine.cs      ShouldPit(), ChooseCompound(), ChooseInstruction(),
                               PitStopDurationS(), FuelToAddKg(), static PitStopProjection()
      QualifyingEngine.cs      Q1/Q2/Q3 simulation, Q2 compound rule, RunQualifyingLapForDriver()
      RaceEngine.cs            17-step per-lap loop; CommandPit/Instruction/TeamOrder public API
      SeasonEngine.cs          24-race calendar, points standings, EDUO power-unit tracking,
                               end-of-season prize money distribution (see Season Management below)
      PracticeEngine.cs        FP1/FP2/FP3 simulation: setup programs, tyre runs, strategy
                               recommendations (1-stop vs 2-stop), setup gain up to 0.35s
      CarDevelopmentSystem.cs  Two-phase upgrade tree (Research → Development), ATR budget,
                               learning-curve discounts, breakthrough rolls (see R&D below)
      DriverMarket.cs          Seasonal driver contracts, transfer window, AI transfers,
                               salary bands, team budget tiers
      SprintEngine.cs          SQ1/SQ2/SQ3 qualifying + 100km sprint race simulation
    Data/                      Static game data — no Unity deps
      StaticGameData.cs        CircuitInfo, TeamInfo, DriverInfo plain C# classes
                               + StaticGameData.CreateData() with all 24 circuits, 10 teams,
                               20 active drivers + 2 reserves
      GameDataFactory.cs       Static singleton: Initialize(), GetCircuit/Team/Driver(),
                               GetTeamDrivers(), GetAllDrivers(), GetTeamsDict()
      CircuitData.cs           ScriptableObject wrapper (Editor only, optional)
      TeamData.cs              ScriptableObject wrapper (Editor only, optional)
      DriverData.cs            ScriptableObject wrapper (Editor only, optional)
      GameDatabase.cs          ScriptableObject container for Editor asset wiring
    UI/                        MonoBehaviour + ViewController layer
      GameManager.cs           Singleton; session state; screen routing:
                               TeamSelect → CircuitSelect → Qualifying → Strategy → Race → Results
      MenuViewController.cs    Team list, circuit list (with type filters), strategy screen,
                               results screen with podium + constructor points
      QualifyingViewController.cs  Q1/Q2/Q3 timing tower with tab navigation; elimination
                               cut-line; Q2 tyre rule notice; pole sitter banner
      RaceViewController.cs    Live standings table, player telemetry panels (sparkline,
                               sectors, tire delta vs rivals, pit projection), event log,
                               command input
  UI/
    MainMenu.uxml / .uss       Team select, circuit select, strategy, qualifying, results
    RaceScreen.uxml / .uss     Race HUD: standings, player panels, event log, track map
  Tests/
    EditMode/
      F1Manager.Tests.asmdef   Assembly def (references Assembly-CSharp, NUnit)
      TireSystemTests.cs       18 tests — phase transitions, deg penalties, compound selection
      WeatherSystemTests.cs     9 tests — construction, penalties, rain events, forecast
      QualifyingEngineTests.cs 10 tests — session counts, grid completeness, determinism
      RaceEngineTests.cs       14 tests — lap simulation, pit commands, DNF, team orders,
                               qualifying grid injection, safety car
      AIStrategyEngineTests.cs 13 tests — ShouldPit() triggers, ChooseCompound() logic,
                               ChooseInstruction() modes, PitStopDurationS(), PitStopProjection()
      OvertakeSystemTests.cs   11 tests — sigmoid probability bounds, DRS effect, Monaco
                               suppression, attempt-filter gap/pace thresholds
Packages/
  manifest.json               Unity 2022.3 package dependencies
ProjectSettings/
  ProjectVersion.txt          2022.3.20f1
```

## Core Simulation Loop

`RaceEngine.SimulateLap()` — called once per displayed lap:
1. Advance weather, collect event messages
2. Safety car countdown
3. Record `PittedLastLap` flags (undercut tracking)
4. Set pit flags from queued commands
5. Evaluate AI pit decisions (`AIStrategyEngine.ShouldPit()`)
6. Evaluate AI instructions (`AIStrategyEngine.ChooseInstruction()`)
7. Apply team orders (HOLD_GAP, SWAP_DRIVERS, PUSH_BOTH)
8. Execute pit stops, update tire age and compound
9. Compute lap times: `base + carGap + fuelPenalty + tireDeg + dirtyAir + instruction + weather + noise`
10. Resolve overtakes (sigmoid formula, DRS gating)
11. Check DNF probability
12. Update positions and gaps
13. Update tire phases
14. Generate engineer radio messages
15. Check race complete (all laps done or all cars DNF)
16. Finalize: sort, build `RaceState`

## Key Field Names (DriverInfo / TeamInfo / CircuitInfo)

Common source of bugs — always use these exact names:

| Class | Field | Type |
|-------|-------|------|
| `DriverInfo` | `driverName` | string |
| `DriverInfo` | `shortName` | string |
| `DriverInfo` | `carNumber` | int |
| `DriverInfo` | `teamID` | int |
| `DriverInfo` | `isReserve` | bool |
| `TeamInfo` | `colorHex` | string (`#RRGGBB`) |
| `TeamInfo` | `shortName` | string |
| `CircuitInfo` | `totalLaps` | int |
| `CircuitInfo` | `tireDegMultiplier` | float |
| `CarState` | `Compound` | TireCompound |
| `CarState` | `DNFReason` | string (empty = no DNF; "mechanical" triggers EDUO change) |

## Tire Degradation Model

4-phase, mirroring Python `core/tire.py`:

| Phase | Trigger | Effect |
|-------|---------|--------|
| WARM_UP | age ≤ warmUpLaps | Fixed penalty (1.0–2.5s) |
| PLATEAU | age ≤ plateauLaps / degMult | ~0 degradation |
| LINEAR | age ≤ (plateau+linear) / degMult | +0.022–0.080s/lap |
| CLIFF | beyond linear | Large per-lap penalty; pit immediately |

All durations scale by `CircuitInfo.tireDegMultiplier` (Monza 0.60, Monaco 1.40).

## Season Management (`SeasonEngine.cs`)

Design reference: `documentation/design/02_SEASON_MANAGEMENT_REVISED.md`

### Calendar
24 rounds; sprint weekends at rounds **5, 6, 11, 19, 21, 23**.
`SeasonEngine.BuildCalendar()` returns the full list as `List<CalendarEntry>`.

### Points
- Race: `[25, 18, 15, 12, 10, 8, 6, 4, 2, 1]`
- Sprint: `[8, 7, 6, 5, 4, 3, 2, 1]`
- **No fastest-lap bonus** — rule was removed in 2025 regs (§2.1.5).
  `DriverStanding.FastestLaps` is tracked for statistics only.

### Prize Money
`DistributePrizeMoney()` runs automatically when `IsSeasonComplete` is true.
Total pool ≈ $450M: P1=$69M … P10=$27M (`PointsTables.PrizeMoney[]`).
Result stored in `ConstructorStanding.PrizeMoney`.

### EDUO — Power Unit Tracking
`DriverPUUsage` tracks ICE/MGU-K/MGU-H/TC/ES/CE components per driver.
FIA 2027 limits: max 3 ICE, 2 of each ERS component.

Key methods:

| Method | When to call |
|--------|-------------|
| `RecordPUUpgrade(teamID, includeERS)` | After a PU-area upgrade completes in `CarDevelopmentSystem` |
| `RecordEmergencyPUChange(driverID)` | After a mechanical DNF in `RaceEngine` |
| `CheckEDUOPenalties(round)` | Auto-called at rounds 6, 12, 18 by `RecordRaceResult()` |
| `GetGridPenaltyForDriver(driverID, round)` | Before `RaceEngine` sets the starting grid |
| `ConsumeGridPenalties(round)` | After the grid is locked, so penalties don't carry over |

Penalty scale: new ICE = 10 grid places; each other component = 5 places.

## Car Development System (`CarDevelopmentSystem.cs`)

Design reference: `documentation/design/04_RND_VEHICLE_DEVELOPMENT_SYSTEM.md`

### Upgrade areas (9)
`POWER_UNIT`, `AERO_FRONT`, `AERO_REAR`, `AERO_FLOOR`, `CHASSIS`,
`SUSPENSION`, `BRAKES`, `GEARBOX`, `COOLING`

### Two-phase workflow
Aero nodes (`AERO_FRONT`, `AERO_REAR`, `AERO_FLOOR`) require a **Research phase**
before manufacturing can begin. Non-aero nodes skip straight to Development.

```
None ──► Research ──► ResearchComplete ──► Development ──► Complete
         (ATR cost)    (player commits       (tokens + R&D)
                        tokens + R&D)
```

`StartDevelopment(teamID, upgradeID)` routes automatically based on current
`UpgradePhase`: call it once to begin research, once more to begin manufacturing.

### ATR — Aerodynamic Testing Resources
Each team has a seasonal budget of wind-tunnel hours and CFD units (reset via
`ResetATRForNewSeason()`). FIA sliding scale: championship leader gets least time.

| Tier | Wind Tunnel hrs/yr | CFD units/yr |
|------|--------------------|--------------|
| Top (≥90 perf) | 56 | 1 120 |
| Mid (83–89) | 68 | 1 480 |
| Lower (<83) | 80 | 1 840 |

Research cost per node tier: T1 = 4h/80 CFD, T2 = 6h/120 CFD, T3 = 10h/200 CFD.

### Learning curve
For each subsequent upgrade in the same area, cost and development time reduce:
- Gen 1 (1 previous complete in area): **−20%**
- Gen 2+ (2+ previous): **−30%**

### Breakthrough
10% chance on any successful development roll → **×2 performance gains**.
`UpgradeNode.WasBreakthrough` is set; `DevelopmentTickResult.Breakthroughs` lists IDs.

### Upgrade tree (21 nodes)

| Area | Nodes |
|------|-------|
| POWER_UNIT | PU_T1 → PU_T2 → PU_T3; ERS_T1 → ERS_T2 (parallel) |
| AERO_FRONT | AF_T1 → AF_T2 → AF_T3 (research-gated) |
| AERO_REAR | AR_T1 → AR_T2 (research-gated) |
| AERO_FLOOR | FL_T1 → FL_T2 (research-gated) |
| CHASSIS | CH_T1 → CH_T2 |
| SUSPENSION | SUS_T1 → SUS_T2 |
| BRAKES | BR_T1 → BR_T2 |
| GEARBOX | GB_T1 → GB_T2 |
| COOLING | COOL_T1 |

### Token / R&D tiers

| Tier | Tokens/season | R&D pts/week |
|------|---------------|--------------|
| Top | 48 | 22 |
| Mid | 36 | 14 |
| Lower | 28 | 8 |

### AI auto-development
`AutoDevelopAI(teamID)` auto-commits any `ResearchComplete` nodes to manufacturing,
then starts the highest-impact affordable upgrade if no work is active.

## Driver Market (`DriverMarket.cs`)

Design reference: `documentation/design/03_TEAM_MANAGEMENT_COMPREHENSIVE.md` §driver contracts

Seasonal transfer window: `OpenTransferWindow()` → player makes offers via
`MakeOffer(teamID, driverID, salaryM, years)` → `RunAITransfers()` → `CloseTransferWindow()`.

Salary bands (by average pace+racecraft):

| Rating | Base salary (M€/yr) |
|--------|---------------------|
| ≥ 95 | 45 |
| ≥ 90 | 30 |
| ≥ 85 | 20 |
| ≥ 80 | 14 |
| ≥ 75 | 10 |
| < 75 | 6 |

Team budget caps: top=$90M, mid=$60M, lower=$35M (combined two-driver payroll).

## Sprint Engine (`SprintEngine.cs`)

Design reference: `documentation/design/02_SEASON_MANAGEMENT_REVISED.md` §2.3

Sprint weekend flow:
1. `RunSprintQualifying()` → SQ1 (cut 20→15), SQ2 (cut 15→10), SQ3 (10 cars, pole)
2. `RunSprintRace()` → 100 km, `SprintLapCount()` = `round(100 / circuitLengthKm)`, min 10
3. No mandatory two-compound rule in sprint race
4. Sprint points `[8,7,6,5,4,3,2,1]` for P1–P8 only

`CloneCircuitWithLaps()` creates a modified `CircuitInfo` with reduced lap count for
use in the standard `RaceEngine` — sprint simulation reuses all race logic.

## Practice Engine (`PracticeEngine.cs`)

Three sessions: FP1 (setup baseline), FP2 (long-run + strategy projection), FP3 (qualifying sim).

FP2 generates a `StrategyRecommendation` comparing 1-stop vs 2-stop windows using
tire physics. Setup gain accumulates across sessions, capped at **0.35s** total.

## Screen Flow

```
GameManager.Start()
    └─ MenuViewController.ShowTeamSelect()

OnTeamSelected() → ShowCircuitSelect()
OnCircuitSelected() → RunQualifying() → QualifyingViewController.Show()
QualifyingViewController (Q1→Q2→Q3→STRATEGY) → OnQualifyingComplete()
OnQualifyingComplete() → MenuViewController.ShowStrategyScreen()
OnStrategyConfirmed() → switch to RaceScreen → RaceViewController.StartRace()
RaceViewController (race loop) → GameManager.OnRaceComplete()
OnRaceComplete() → switch to MenuScreen → ShowResults()
OnNewRace() → ShowTeamSelect()
```

## Running Tests

Unity → Window → General → Test Runner → EditMode tab → Run All

Tests are pure C# and require no scene. Total: **75 tests** across 6 fixtures.

## Relationship to Python Prototype

`autosport-manager-2027/` is the design reference. Core simulation constants
(overtake weights, tire profiles, lap time formula, AI thresholds) are identical
between Python and C#. When changing a mechanic, update both. Python is faster
to iterate on for balance changes.

## Known Gaps vs Design Documents

| Gap | Source | Notes |
|-----|--------|-------|
| Cost Cap ($215M) | doc 02 §2.1.6 | No budget tracking; quarterly audit not implemented |
| Sprint PARC FERMÉ reset | doc 02 §2.3 | Setup changes after sprint not enforced |
| ADUO (aero concept carryover) | doc 04 §4.5 | Upgrade unlock rules are tier-only, no concept lock |
| Component versioning + rollback | doc 04 §4.6 | No V1.0→V1.1 naming or rollback |
| Three-director model | doc 03 §3.2 | Personnel system not implemented |
| Base facility upgrades | doc 03 §3.4 | Wind tunnel / simulator facility tier not tracked |
| Sponsorship contracts + KPIs | doc 05 §5.2 | No financial income layer |
| No save/load | — | Session state in memory only |
| No ASCII track map | — | RaceScreen has placeholder Label |
| Player qualifying input | — | QualifyingViewController shows AI laps only |
