# F1Manager2027-Unity — Unity/C# Implementation Guide

**Mechanics reference**: `../autosport-manager-2027/CLAUDE.md`
**Design documents**: `../autosport-manager-2027/documentation/design/`

This file covers only Unity/C#-specific concerns: class APIs, correspondence
to Python modules, test coverage, and implementation gaps.

## How To Open

1. Install **Unity 2022.3.20f1** (or any 2022.3.x LTS)
2. Open `F1Manager2027-Unity/` as a Unity project
3. Hit Play (scene setup: `GameManager.cs` + `UIRoot` with UXML documents)

## Directory Layout

```
Assets/Scripts/Core/          Pure C# — zero UnityEngine dependencies
  Enums.cs                    TireCompound, DriverInstruction, WeatherCondition,
                              TirePhase, TeamOrder + extension methods
  Models.cs                   TireProfile, TireProfiles, CarState, RaceState,
                              RaceEvent, PitStopResult
  TireSystem.cs               ← mirrors core/tire.py
  WeatherSystem.cs            ← mirrors core/weather.py
  OvertakeSystem.cs           ← mirrors core/overtake.py
  AIStrategyEngine.cs         ← mirrors core/ai.py
  QualifyingEngine.cs         ← mirrors core/qualifying.py
  RaceEngine.cs               ← mirrors src/core/engine.py
  SeasonEngine.cs             ← mirrors src/core/season.py  (+EDUO, +prize money)
  PracticeEngine.cs           (no Python equivalent yet)
  CarDevelopmentSystem.cs     ← mirrors src/core/car_development.py  (+two-phase, +ATR)
  DriverMarket.cs             ← mirrors src/core/driver_market.py
  SprintEngine.cs             (no Python equivalent yet)

Assets/Scripts/Data/          Static game data — no Unity deps
  StaticGameData.cs           CircuitInfo, TeamInfo, DriverInfo + CreateData()
  GameDataFactory.cs          Initialize(), GetCircuit/Team/Driver(), GetTeamDrivers()

Assets/Scripts/UI/            MonoBehaviour + ViewController layer
  GameManager.cs              Screen routing: TeamSelect→Qualifying→Strategy→Race→Results
  MenuViewController.cs       Team list, circuit list, strategy screen, results
  QualifyingViewController.cs Q1/Q2/Q3 timing tower, Q2 tyre rule notice
  RaceViewController.cs       Standings table, player telemetry, event log, command input

Assets/Tests/EditMode/
  TireSystemTests.cs          18 tests
  WeatherSystemTests.cs        9 tests
  QualifyingEngineTests.cs    10 tests
  RaceEngineTests.cs          14 tests
  AIStrategyEngineTests.cs    13 tests
  OvertakeSystemTests.cs      11 tests
```

## C# API Quick Reference

### Key field names — common source of bugs

| Class | Field | Type |
|-------|-------|------|
| `DriverInfo` | `driverName` | string |
| `DriverInfo` | `shortName` | string |
| `DriverInfo` | `carNumber` | int |
| `DriverInfo` | `teamID` | int |
| `DriverInfo` | `isReserve` | bool |
| `TeamInfo` | `colorHex` | string (`#RRGGBB`) |
| `CircuitInfo` | `totalLaps` | int |
| `CircuitInfo` | `tireDegMultiplier` | float |
| `CarState` | `DNFReason` | string (empty = OK; `"mechanical"` → EDUO emergency change) |

### OvertakeSystem — extra static helper

`ComputeSuccessProbability(skillDelta, paceDelta, tireAgeDelta, drsActive, trackDifficulty, defenderAggression)`
exposes the raw sigmoid inputs without `CarState` objects. Used by tests and balance tooling.

### SeasonEngine — EDUO integration

```csharp
// After CarDevelopmentSystem completes a PU-area upgrade:
season.RecordPUUpgrade(teamID, includeERS: false);

// After a mechanical DNF in RaceEngine:
season.RecordEmergencyPUChange(driverID);

// Before setting the starting grid:
int penalty = season.GetGridPenaltyForDriver(driverID, currentRound);
season.ConsumeGridPenalties(currentRound);
```
EDUO checks fire automatically at rounds 6, 12, 18 inside `RecordRaceResult()`.

### CarDevelopmentSystem — two-phase workflow

Aero nodes (`AERO_FRONT`, `AERO_REAR`, `AERO_FLOOR`) require a Research phase
before manufacturing. Call `StartDevelopment()` twice: first starts Research
(consumes ATR budget), second starts Development (consumes tokens + R&D).

```csharp
dev.StartDevelopment(teamID, "AF_T1", out string err); // → enters Research
// ... TickWeek() until ResearchComplete ...
dev.StartDevelopment(teamID, "AF_T1", out err);        // → enters Development
```

Non-aero nodes skip Research; a single call goes straight to Development.

`AutoDevelopAI(teamID)` handles both steps automatically for AI teams.

### CarDevelopmentSystem — upgrade tree (21 nodes, 9 areas)

| Area | Nodes | Research? |
|------|-------|-----------|
| POWER_UNIT | PU_T1→T2→T3; ERS_T1→T2 (parallel) | No |
| AERO_FRONT | AF_T1→T2→T3 | Yes |
| AERO_REAR | AR_T1→T2 | Yes |
| AERO_FLOOR | FL_T1→T2 | Yes |
| CHASSIS | CH_T1→T2 | No |
| SUSPENSION | SUS_T1→T2 | No |
| BRAKES | BR_T1→T2 | No |
| GEARBOX | GB_T1→T2 | No |
| COOLING | COOL_T1 | No |

## C# vs Python — implementation gaps

| Feature | Python (`car_development.py`) | C# (`CarDevelopmentSystem.cs`) |
|---------|-------------------------------|-------------------------------|
| R&D phases | Single `weeks_to_develop` | Two-phase: Research + Development |
| ATR budget | Not tracked | `WindTunnelHoursRemaining`, `CFDUnitsRemaining` |
| Learning curve | Not implemented | −20% Gen1, −30% Gen2+ per area |
| Breakthroughs | Not implemented | 10% chance → ×2 gains |
| AERO_FLOOR area | Not present | FL_T1, FL_T2 |
| BRAKES area | Not present | BR_T1, BR_T2 |
| ERS sub-path | Not present | ERS_T1, ERS_T2 |

| Feature | Python (`season.py`) | C# (`SeasonEngine.cs`) |
|---------|----------------------|------------------------|
| Fastest lap bonus | Not awarded (removed 2025) | Not awarded (removed 2025) |
| EDUO tracking | Not implemented | `DriverPUUsage`, checkpoint rounds |
| Prize money | Not distributed | `DistributePrizeMoney()` at season end |

## Running Tests

Unity → Window → General → Test Runner → EditMode → Run All

75 tests total. All tests are pure C# and require no scene.

## Known Implementation Gaps (vs design docs)

| Gap | Design doc | Notes |
|-----|------------|-------|
| Cost Cap ($215M) | doc 02 §2.1.6 | No budget tracking |
| Sprint PARC FERMÉ reset | doc 02 §2.3 | Setup changes after sprint not enforced |
| ADUO (aero concept carryover) | doc 04 §4.5 | Upgrade unlocks are tier-only |
| Component versioning + rollback | doc 04 §4.6 | No V1.0→V1.1 naming |
| Three-director model | doc 03 §3.2 | Personnel system not implemented |
| Base facility upgrades | doc 03 §3.4 | WT/simulator tier not tracked |
| Sponsorship contracts + KPIs | doc 05 §5.2 | No financial income layer |
| No save/load | — | Session state in memory only |
| No ASCII track map | — | RaceScreen has placeholder Label |
| Player qualifying input | — | QualifyingViewController shows AI laps only |
