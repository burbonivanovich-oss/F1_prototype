# F1Manager2027-Unity — Codebase Guide

## What This Is

A Unity 2022.3 LTS port of the `autosport-manager-2027` Python prototype.
All simulation logic lives in pure C# (no `UnityEngine` imports) and is identical
to the Python reference implementation. Unity only handles rendering and input
via UI Toolkit (UXML + USS).

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
      Enums.cs                 TireCompound, DriverInstruction, WeatherCondition, TirePhase
                               + extension methods: IsDry(), DisplayName(), GripAdvantageS()
      Models.cs                TireProfile, TireProfiles (all 8 compounds), CarState,
                               RaceEvent, PitStopResult, RaceState
      TireSystem.cs            4-phase degradation: GetPhase(), DegPenaltyS(),
                               WindowRemaining(), BestCompoundForStint()
      WeatherSystem.cs         Rain event planner, lap-by-lap Advance(), LapTimeWeatherPenaltyS(),
                               AquaplaningChance(), RecommendedCompound()
      OvertakeSystem.cs        Sigmoid overtake formula, DRS gating, dirty-air penalty
      AIStrategyEngine.cs      ShouldPit(), ChooseCompound(), ChooseInstruction(),
                               PitStopDurationS(), FuelToAddKg(), PitStopProjection()
      QualifyingEngine.cs      Q1/Q2/Q3 simulation, Q2 compound rule, RunQualifyingLapForDriver()
      RaceEngine.cs            17-step per-lap loop; CommandPit/Instruction/TeamOrder public API
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
| `CarState` | `Compound` | TireCompound (NOT TireCompound field) |

## Tire Degradation Model

4-phase, mirroring Python `core/tire.py`:

| Phase | Trigger | Effect |
|-------|---------|--------|
| WARM_UP | age ≤ warmUpLaps | Fixed penalty (1.0–2.5s) |
| PLATEAU | age ≤ plateauLaps / degMult | ~0 degradation |
| LINEAR | age ≤ (plateau+linear) / degMult | +0.022–0.080s/lap |
| CLIFF | beyond linear | Large per-lap penalty; pit immediately |

All durations scale by `CircuitInfo.tireDegMultiplier` (Monza 0.60, Monaco 1.40).

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

Tests are pure C# and require no scene — they test simulation classes directly.

## Relationship to Python Prototype

`autosport-manager-2027/` is the design reference. All simulation constants
(overtake weights, tire profiles, lap time formula, AI thresholds) are identical
between the Python and C# implementations. When changing a mechanic, update
both. Python is faster to iterate on for balance changes.

## Known Gaps vs Python Prototype

1. **No season management**: Single race only (same as Python)
2. **No ASCII track map**: RaceScreen has a placeholder Label; needs implementation
3. **No player qualifying input**: QualifyingViewController shows AI results only;
   player lap simulation via `RunQualifyingLapForDriver()` is not yet wired to UI
4. **No sprint format**: Traditional weekend only
5. **No save/load**: Session state lives in GameManager singleton only
