# TECHNICAL DESIGN DOCUMENT (TDD) - REVISED
# PART 2: MODULE SPECIFICATIONS

**Version**: 2.0 (Management Game - Lap-Tick Based)  
**Date**: 2026-04-06  
**Status**: Detailed Implementation Guide  

---

## 2.1 RACE SIMULATOR MODULE

### 2.1.1 Responsibilities

**Input**:
- Player pit decisions (tire compound, fuel, pit timing)
- Player driver instructions (push/defend/save)
- Current race state (lap 1-58, weather, all car positions)

**Output**:
- Updated race state (all car positions, lap times, incidents)
- Events (pit executed, crash occurred, weather changed)

### 2.1.2 Lap Time Calculator (Core Formula)

**Lap time depends on**:
1. **Base speed** (car setup: aero, chassis, power unit)
2. **Driver skill** (driver rating 0-100)
3. **Tire degradation** (older tires = slower)
4. **Weather** (rain = slower)
5. **Damage** (crashes = penalty)
6. **Fuel load** (heavier car = slower, but only slight)

**Formula**:
```
baseLapTime = 80.0 seconds (reference for Monaco, example)

lapTime = baseLapTime
        - (carSetupPerformance * driverSkill / 100) * 0.15  // Fast driver, good setup
        + (tiresDegradation * 0.5)                         // Worn tires slower
        + (weatherPenalty)                                  // Rain = +1.5 to +3 seconds
        + (damagePenalty)                                   // Crash = +0.5 to +2 seconds
        + (fuelLoad * 0.0005)                              // Heavy fuel = slight penalty
        + (randomVariance * 0.1)                           // Driver consistency (-0.2 to +0.2 sec)

Example lap times:
- Fresh soft tires, optimal setup, dry: 1:23.5
- Fresh soft tires, optimal setup, light rain: 1:25.2 (+1.7 sec)
- Degraded soft tires (80%), optimal setup, dry: 1:24.8 (+1.3 sec)
- Degraded soft tires (95%, cliff phase): 1:26.5 (+3.0 sec, must pit!)
```

### 2.1.3 Position Calculator (Overtake Logic)

**Every lap, determine positions**:

```
1. Calculate each car's lap time (using formula above)
2. Add lap time to cumulative time:
   car.totalTime += lapTime

3. Sort cars by totalTime (ascending = faster = higher position)
4. Check for overtakes:
   If (car_A speed > car_B speed) AND (distance small):
      Overtake chance = (speed_gap) * (car_A skill) / 100
      Random check: if random < overtake_chance:
         car_A position++
         car_B position--
         log event: "Hamilton overtakes Verstappen"
      Else:
         No overtake this lap
         
5. Check for collisions (low probability):
   If aggressive_overtake AND (tire_quality_bad OR rain):
      Collision chance = 5-15%
      If collision:
         Both cars take damage (0.5-1.0 sec/lap penalty)
         Slower car drops 1-2 positions
         log event: "Crash! Alonso and Norris collide Turn 3"
```

**Example overtake sequence**:
```
Lap 15:
- Hamilton (P2) speed 1:23.0
- Leclerc (P1) speed 1:23.8
- Speed gap: 0.8 sec/lap
- Hamilton attempts overtake: (0.8 * 92 skill / 100) = ~0.74 chance (74%)
- Random: 0.63 < 0.74 → SUCCESS
- Hamilton now P1, Leclerc P2
- Log: "Lap 15: Hamilton overtakes Leclerc for the lead"
```

---

## 2.2 TIRE SYSTEM MODULE

### 2.2.1 Tire State

```csharp
[Serializable]
public class TireState
{
    public string compound;           // "soft", "medium", "hard"
    public float degradation;         // 0-1.0 (0=fresh, 1.0=unusable)
    public float lapsSinceChange;     // 0-60 (for formula tracking)
    public float temperature;         // 60-100°C (cosmetic, affects formula)
    public float currentGripModifier; // 0-1.0 (for lap time calculations)
}
```

### 2.2.2 Three-Phase Degradation Model (Formula-Based)

**Not physics simulation — just a formula that matches real F1 behavior:**

```
PHASE 1: PLATEAU (Laps 0-10, typical)
  degradation += 0.00 per lap  // Fresh tires, no wear
  grip = 1.0 (full grip)
  
PHASE 2: LINEAR WEAR (Laps 11-22, typical)
  degradation += 0.015 per lap  // Progressive wear
  grip = 1.0 - degradation
  Effect: Lap time increases 0.05-0.10 sec per lap
  
PHASE 3: CLIFF (Lap 23+)
  degradation += 0.25 per lap  // Rapid cliff
  grip = max(0.0, 1.0 - degradation)
  Effect: Lap time jumps +0.5 to +1.0 sec
  Player MUST pit before entering cliff phase
```

**Track-specific parameters** (soft tires example):

```
Monaco (high degradation):
  Phase1_laps: 8
  Phase2_rate: 0.020/lap
  Phase2_duration: 12 laps (laps 9-20)
  Cliff_trigger: lap 21

Monza (low degradation):
  Phase1_laps: 15
  Phase2_rate: 0.010/lap
  Phase2_duration: 15 laps (laps 16-30)
  Cliff_trigger: lap 31
```

**Implementation**:

```csharp
public float CalculateTireGrip(TireState tire, WeatherState weather)
{
    float grip = 1.0f;
    
    // Base degradation formula
    if (tire.lapsSinceChange <= phase1_duration)
    {
        grip = 1.0f;  // Plateau
    }
    else if (tire.lapsSinceChange <= phase1_duration + phase2_duration)
    {
        // Linear degradation
        float phase2_progress = (tire.lapsSinceChange - phase1_duration) / phase2_duration;
        grip = 1.0f - (phase2_progress * 0.25f);
    }
    else
    {
        // Cliff phase
        grip = 0.2f + (1.0f - tire.degradation) * 0.5f;
    }
    
    // Temperature modifier (optional, for immersion)
    if (tire.temperature < 60.0f)
        grip *= 0.7f;  // Cold tires
    else if (tire.temperature > 95.0f)
        grip *= 0.9f;  // Overheated
    
    // Weather modifier (see 2.3)
    grip *= weather.gripMultiplier;
    
    return grip;
}
```

### 2.2.3 Tire Warm-Up (Post-Pit Stop)

**After pit stop, new tires need warm-up:**

```
Lap 1 after pit: -1.5 seconds (tires cold, not yet optimal temp)
Lap 2 after pit: -0.8 seconds (warming up)
Lap 3 after pit: -0.3 seconds (nearly optimal)
Lap 4 after pit: 0.0 seconds (fully warmed, normal grip)

Formula:
warmupPenalty = max(0.0f, 1.5f * (1.0f - lapsSincePit / 4.0f))
adjustedLapTime = baseLapTime + warmupPenalty
```

### 2.2.4 Tire Compounds

**Three options per race**:

| Compound | Base Speed | Durability | Notes |
|----------|-----------|-----------|-------|
| **Soft** | +0.25 sec advantage | 15-25 laps | High grip, wear fast |
| **Medium** | Baseline (0.0) | 25-40 laps | Balanced |
| **Hard** | -0.15 sec slower | 40-60 laps | Low grip, long life |

**Mandatory requirement** (real F1): Use at least 2 different compounds in dry race (except rain races).

---

## 2.3 WEATHER SYSTEM MODULE

### 2.3.1 Weather State

```csharp
[Serializable]
public class WeatherState
{
    public float precipitation;    // 0-100% (0=dry, 100=heavy rain)
    public float trackTemperature; // 15-40°C
    public float ambientTemp;      // 10-35°C
    public float gripMultiplier;   // Derived value: 1.0 (dry) to 0.6 (wet)
}
```

### 2.3.2 Weather Evolution

**Realistic weather changes**:

```
WEATHER STATES:
Dry (0-15% precipitation):
  gripMultiplier = 1.0
  
Light rain (15-50% precipitation):
  gripMultiplier = 0.85
  Lap times +1.0 to +2.0 seconds
  Wet tires 0.5-1.0 sec/lap FASTER than dry tires
  
Heavy rain (50-100% precipitation):
  gripMultiplier = 0.60
  Lap times +3.0 to +5.0 seconds
  Wet tires 1.5-2.5 sec/lap faster than soft/medium
```

**Precipitation evolution** (over 5-10 laps):

```
Dry spell: precipitation -= 3% per lap
Building storm: precipitation += 5% per lap
Clearing: precipitation -= 4% per lap

Example: Monaco, dry condition
Lap 1-10: 0% (dry)
Lap 11: Clouds → 5%
Lap 12-15: Light rain building → 20-40%
Lap 16-20: Heavy rain → 60-80%
Lap 21-25: Peak rain → 70%
Lap 26-30: Clearing → 40-20%
Lap 31+: Dry again → 0%
```

**Track temperature** (affects tire warm-up):

```
Base formula:
trackTemp = ambientTemp + (sunHeat * 10.0f) - (rain * 15.0f)

If raining:
  trackTemp = ambientTemp - 10 to -15°C
  
If dry and sunny:
  trackTemp = ambientTemp + 5 to +15°C
  
Example:
  Ambient: 22°C
  Heavy rain: trackTemp = 22 - 12 = 10°C (cold)
  Clearing up: trackTemp = 22 + 5 = 27°C (warming)
```

---

## 2.4 AI DRIVER MODULE

### 2.4.1 AI Decision Tree (Pit Strategy)

**Every 5 laps (or when player-triggered pit window), each AI driver evaluates:**

```
Decision tree:

IF (current_lap > 5):
    // Check pit urgency
    IF (tire_degradation > 0.90):
        DECISION = "URGENT_PIT" (next lap)
    ELSE IF (tire_degradation > 0.75 AND fuel_remaining < laps_left):
        DECISION = "NORMAL_PIT" (within 2 laps)
    ELSE:
        // Can complete race without pit
        IF (current_lap < race_end - 5):
            DECISION = "NO_PIT_NEEDED"
        ELSE:
            DECISION = "FINAL_STINT_PUSH"

IF (DECISION == "NORMAL_PIT"):
    // Choose tire compound
    IF (weather == "heavy_rain"):
        COMPOUND = "wet"
    ELSE IF (weather == "light_rain"):
        COMPOUND = "intermediate"
    ELSE:
        // Dry conditions: choose based on strategy
        IF (current_position < 5):
            COMPOUND = "soft"  // Leader pushes
        ELSE:
            COMPOUND = "medium"  // Chase, balance pace & life

    // Decide fuel amount
    fuel_to_add = (race_laps - current_lap) * avg_fuel_consumption + 5.0f
```

**Example decision sequence**:

```
Lap 12 (Ferrari #16):
- Tire degradation: 0.45 (still good)
- Fuel: 45 kg (enough for ~22 laps)
- Position: P3 (behind Hamilton P1, Sainz P2)
- Weather: Dry

Decision: NO_PIT_NEEDED (can run to lap 28+)
Strategy: Save tires, undercut opponent when they pit

Lap 18 (same car):
- Tire degradation: 0.72 (entering wear phase)
- Fuel: 30 kg (enough for ~15 laps)
- Position: Still P3
- Weather: Dry

Decision: NORMAL_PIT (within 2 laps)
Compound: Medium (balanced)
Fuel: 25 kg (for final ~15 laps)
```

### 2.4.2 Morale System

**AI drivers have morale (0-100) affecting risk tolerance:**

```
Morale increases:
  +5: Successful overtake
  +10: Moved up position
  +3: Pit stop executed well
  +5: Leading race
  
Morale decreases:
  -5: Lost position
  -10: Stuck behind slower car
  -8: Tires are terrible (cliff phase)
  -15: Forced to let teammate pass (team orders)
  -20: Engine failure / DNF

Morale effects:
  < 40: Conservative (won't risk overtakes, safer pit strategy)
  40-70: Balanced (normal risk)
  > 70: Aggressive (more risky overtakes, willing to push degraded tires)
```

---

## 2.5 UI/UX MODULE

### 2.5.1 Race Monitor Layout

```
┌─────────────────────────────────────────────────────────────┐
│ Monaco Grand Prix - Lap 18/78 - 1:52:34 elapsed              │
├──────────────────┬───────────────────┬──────────────────────┤
│ STANDINGS        │ TRACK MAP         │ TELEMETRY (Driver 1) │
│ (Left: standings)│ (Center: visual)  │ (Right: data)        │
│                  │                   │                      │
│ P1 Hamilton      │  ╔═════╗          │ Lap Time: 1:23.521   │
│    Gap: -        │  ║ ●▲▼ ║          │ Tire: Soft (70%)     │
│ P2 Sainz         │  ║█████║          │ Fuel: 35 kg          │
│    Gap: +0.8     │  ║●  ●║          │ Damage: 0%           │
│ P3 Leclerc       │  ╚═════╝          │                      │
│    Gap: +3.2     │  Pit window: OK   │ Pit Window: Next lap │
│ P4 Verstappen    │                   │ Suggestion: Pit lap  │
│    Gap: +5.1     │ [1x] [2x] [5x]    │ 19-21, Medium tires  │
│ P5 Alonso        │                   │                      │
│    ...           │ [Pause] [Settings]│ [Pit Now] [Push]     │
│                  │                   │ [Defend] [Save]      │
└──────────────────┴───────────────────┴──────────────────────┘
│ RACE EVENT LOG                                               │
│ Lap 18: Leclerc pitted for mediums (P2 → P4, +3 sec gap)   │
│ Lap 17: Safety car deployed - Alonso crash Turn 8          │
│ Lap 16: Sainz fastest lap 1:23.421                         │
└─────────────────────────────────────────────────────────────┘
```

### 2.5.2 UI Tech Stack

**Framework**: Unity UIToolkit (not UGUI)
- Retained-mode rendering (efficient updates)
- Data binding for live updates
- Performance: <5 ms per frame for UI

**Update rates**:
- Standings panel: Every 1 second (not every frame)
- Telemetry graphs: Every 2-3 seconds
- Pit dialog: Updates in real-time when pit available

**Performance budget**: 5 ms per frame for UI (out of 33 ms total)

---

## 2.6 DATA PERSISTENCE MODULE

### 2.6.1 Save File Format

**JSON-based** (simple, human-readable, debuggable):

```json
{
  "race_id": "monaco_2027_r3",
  "current_lap": 18,
  "elapsed_seconds": 6754,
  "weather": {
    "precipitation": 0.0,
    "track_temp": 28.5
  },
  "cars": [
    {
      "id": 1,
      "driver": "Lewis Hamilton",
      "position": 1,
      "total_time": 1845.32,
      "fuel_kg": 32.5,
      "tire_compound": "soft",
      "tire_degradation": 0.45,
      "damage": 0.0,
      "laps_since_pit": 6
    },
    // ... 23 more cars
  ],
  "event_log": [
    "Lap 16: Safety car deployed",
    "Lap 17: Sainz fastest lap 1:23.421"
  ]
}
```

**Size**: ~3-5 KB per save (compressed ~1 KB)

### 2.6.2 Save/Load Implementation

```csharp
public void SaveRace(RaceState state, string filename)
{
    // Convert to JSON
    string json = JsonConvert.SerializeObject(state, Formatting.Indented);
    
    // Compress (optional)
    byte[] data = Compress(Encoding.UTF8.GetBytes(json));
    
    // Write atomically
    string tempFile = filename + ".tmp";
    File.WriteAllBytes(tempFile, data);
    
    // Atomic rename (replace old with new)
    File.Delete(filename);
    File.Move(tempFile, filename);
}

public RaceState LoadRace(string filename)
{
    byte[] data = File.ReadAllBytes(filename);
    string json = Decompress(data);
    return JsonConvert.DeserializeObject<RaceState>(json);
}
```

**Safety**:
- Write to temp file first, then atomic rename
- No corruption risk (old save preserved until new is complete)
- Keep 3 auto-backup versions (rollback if needed)

---

## 2.7 IMPLEMENTATION ROADMAP

### Week 1: Foundation
- [ ] Data structures (RaceState, CarState, TireState)
- [ ] Mock race data (3 cars)
- [ ] Basic UI skeleton

### Week 2-3: Core Simulation
- [ ] Lap calculator (formula)
- [ ] Position updater (overtake logic)
- [ ] Tire degradation (3-phase formula)
- [ ] Weather system
- [ ] Test: 3-car race deterministic

### Week 4: AI & Pit Strategy
- [ ] AI pit decision tree
- [ ] Morale system
- [ ] Test: 24 cars, realistic pit timing

### Week 5-6: UI & Interaction
- [ ] Race monitor complete
- [ ] Pit dialog (tire choice)
- [ ] Driver instructions (push/defend/save)
- [ ] Playtesting

### Week 7-8: Polish
- [ ] Save/load system
- [ ] Qualifying session
- [ ] Incident handling
- [ ] Bug fixes

### Week 9-10: Testing & Validation
- [ ] Full season playable
- [ ] Profiling (30 FPS target)
- [ ] Playtesting feedback
- [ ] Final polish

---

## APPENDIX: FORMULAS REFERENCE

**Lap Time Base Formula**:
```
lapTime = baseLapTime
        - (carSetup * driverSkill * 0.15 / 100)  // Performance
        + (tireDegradation * 0.5)                // Wear
        + weatherPenalty                         // Rain, temp
        + damagePenalty                          // Crashes
        + fuelLoad * 0.0005                      // Weight
```

**Tire Grip Calculation**:
```
if (lapsSincePit <= phase1):
    grip = 1.0
elif (lapsSincePit <= phase1 + phase2):
    grip = 1.0 - ((lapsSincePit - phase1) / phase2) * 0.25
else:
    grip = 0.2 + (1.0 - degradation) * 0.5
    
// Apply modifiers
grip *= weatherGripMult
grip *= tempModifier (0.7 to 1.0)
```

**Overtake Chance**:
```
overtakeChance = (leadingLapTime - trailingLapTime) * trailingDriverSkill / 100
```

---

**Document Version**: 2.0  
**Status**: FINAL for Management Game  
**Last Updated**: 2026-04-06
