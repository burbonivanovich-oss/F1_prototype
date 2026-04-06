# TECHNICAL DESIGN DOCUMENT (TDD) - REVISED
# PART 1: ARCHITECTURE OVERVIEW

**Version**: 2.0 (Revised for Management Game)  
**Date**: 2026-04-06  
**Status**: Final Draft - Management Sim Approach  
**Target Platform**: PC (Windows/Steam)  
**Target Engine**: Unity 2022.3 LTS  

---

## 1.1 PROJECT SCOPE (Corrected)

### Core Requirements
- **Management game** where player manages team (not drives car)
- **Strategic race simulation**: 24 AI-driven cars, lap-by-lap progression
- **Player decisions**: Pit timing, tire compounds, driver instructions (push/defend/save)
- **Real-time UI**: Live standings, telemetry graphs, pit strategy advisor
- **Season progression**: 24 races with R&D, team management, driver morale
- **Deterministic** (save/load must produce identical results)
- **Performance**: 30 FPS target, 20 Hz simulation tick (NOT 60 FPS)

### What This Game IS:
✅ **Like Football Manager**: Turn-based team management between races  
✅ **Like Motorsport Manager**: Strategic pit decisions during live race  
✅ **Like F1 Manager 2024**: Real teams/drivers, season-long progression  

### What This Game IS NOT:
❌ **Racing simulator** (player never steers, accelerates, brakes)  
❌ **Real-time action** (doesn't need 60 FPS physics)  
❌ **Physics-heavy** (tire temperatures are formulas, not Euler integration)  

---

## 1.2 ARCHITECTURE PHILOSOPHY: "Lap Tick" Simulation

Instead of frame-by-frame physics, the race progresses in discrete **lap ticks** (every 1-2 seconds real-time):

```
TRADITIONAL PHYSICS APPROACH:
Frame 1 (16.67 ms) → Update position 0.1 meters
Frame 2 (16.67 ms) → Update position 0.1 meters
...
Frame 60 (1000 ms) → 1 lap completed

MANAGEMENT GAME APPROACH (Better):
Lap Tick 1 (2 seconds) → Lap 1 completed, position updated
Lap Tick 2 (2 seconds) → Lap 2 completed, position updated
...
Race completes in 58 ticks × 2 sec = ~2 minutes (with UI rendering at 30 FPS)
```

**Benefits**:
- Simple (lap outcome = formula, not 1000 physics calculations)
- Deterministic (same input = same lap time, always)
- Performant (CPU usage negligible)
- Debuggable (race progression visible in logs)

---

## 1.3 HIGH-LEVEL ARCHITECTURE

### Core Modules (5 Systems, Not 8)

```
┌─────────────────────────────────────────────────┐
│         UI RENDERER (Main Thread, 30 FPS)        │
│  Standings | Telemetry | Pit Dialog | Track Map  │
└─────────────────────────────────────────────────┘
         ↑                    ↓
         └────────┬───────────┘
                  ↓
    ┌─────────────────────────────────────┐
    │   RACE SIMULATOR (Logic Thread)      │
    │  20 Hz lap-tick progression          │
    │                                      │
    │  ┌──────────────────────────────┐   │
    │  │ Lap Calculator               │   │
    │  │ (speed, tire deg, weather)   │   │
    │  └──────────────────────────────┘   │
    │           ↓                          │
    │  ┌──────────────────────────────┐   │
    │  │ Position Updater             │   │
    │  │ (overtake logic, collisions) │   │
    │  └──────────────────────────────┘   │
    │           ↓                          │
    │  ┌──────────────────────────────┐   │
    │  │ Event Generator              │   │
    │  │ (pit, crash, weather change) │   │
    │  └──────────────────────────────┘   │
    └─────────────────────────────────────┘
         ↑           ↑           ↑
    ┌────────┐  ┌────────┐  ┌────────┐
    │  TIRE  │  │WEATHER │  │ AI     │
    │ SYSTEM │  │ SYSTEM │  │DRIVER  │
    │        │  │        │  │        │
    └────────┘  └────────┘  └────────┘
         ↓           ↓           ↓
    ┌──────────────────────────────────┐
    │   DATA PERSISTENCE               │
    │   (Save/Load, Season Progress)   │
    └──────────────────────────────────┘
```

### Module Responsibilities

| Module | Purpose | Update Rate |
|--------|---------|-------------|
| **Race Simulator** | Master: lap progression, car positions, pit stops, incidents | 20 Hz (50 ms ticks) |
| **Tire System** | Degradation formula, temperature model, compound effects | Per-lap calculation |
| **Weather System** | Dynamic weather evolution, track temp, grip modifiers | Per-lap update |
| **AI Driver** | 24 drivers: pit strategy decisions, morale updates | Per-lap (not every frame) |
| **UI Renderer** | Display standings, graphs, telemetry, pit dialogs | 30 FPS (33 ms) |
| **Data Persistence** | Save/load race state, season progression, telemetry logs | On demand |

**Key Difference**: No physics thread, no collision detection thread. Single simulation loop, simple and clear.

---

## 1.4 PERFORMANCE TARGETS

### FPS & Frame Budget

**Target**: 30 FPS on mid-range PC (sufficient for management game)
- **Frame time**: 33.33 ms per frame
- **Simulation tick**: 50 ms (20 Hz) → Runs ~1.5× per frame

**Frame Budget Allocation** (per 33.33 ms frame at 30 FPS):
- UI rendering: 15-18 ms (standings, telemetry, track map)
- Simulation: 5-8 ms (calculate 1 lap for all 24 cars)
- Events/logic: 3-5 ms (pit decisions, crashes, weather)
- Overhead: 5 ms

**Hardware Target** (Minimum):
- CPU: Intel i5-8400 or AMD Ryzen 5 2600 (any 6-core CPU)
- GPU: Integrated graphics OK (GTX 1050 recommended for UI smoothness)
- RAM: 4-6 GB
- Storage: SSD with 8-10 GB free

**Scalability** (if performance issues):
1. **Ultra**: 30 FPS, 24 AI cars, full weather detail
2. **High**: 30 FPS, 18 AI cars, simplified weather
3. **Medium**: 30 FPS, 12 AI cars, basic weather
4. **Low**: Forced 20 FPS, 8 AI cars, minimal effects

No need for 60 FPS option (not real-time action game).

---

## 1.5 ENGINE & TECHNOLOGY CHOICES

### Unity 2022.3 LTS (Recommended)

**Why Unity?**
1. **UIToolkit**: Fast, performant UI for real-time data (graphs, standings)
2. **No physics needed**: We don't use PhysX (we use formulas instead)
3. **Steam integration**: Steamworks SDK compatibility
4. **Development speed**: Quick iteration, large asset ecosystem
5. **Data visualization**: Built-in for telemetry graphs

**Alternative NOT recommended**:
- Unreal: Overkill for management game, slower UI iteration
- Custom engine: Unnecessary (Unity sufficient)
- Godot: Less mature for commercial Steam release

### Programming Languages
- **Primary**: C# (Unity scripting)
- **Data Formats**: JSON (config, car setups), CSV (telemetry logs)

### Key Dependencies
1. **Newtonsoft JSON** (NuGet) — robust JSON parsing
2. **Steamworks.NET** (free) — Steam API integration
3. **Dotween** (free) — UI animations
4. **Cinemachine** (free) — optional camera management

---

## 1.6 CORE SIMULATION LOOP (Pseudocode)

```csharp
// SINGLE-THREADED MAIN LOOP (Simple & Clear)
public class RaceSimulator
{
    private float simulationTickTimer = 0;
    private const float SIMULATION_TICK_RATE = 0.05f;  // 20 Hz = 50 ms
    
    void Update()  // Called every frame (30 FPS = 33.3 ms per frame)
    {
        // Input handling
        HandlePlayerInput();
        
        // Accumulate time
        simulationTickTimer += Time.deltaTime;
        
        // Run simulation ticks as needed (usually ~1.5 ticks per frame)
        while (simulationTickTimer >= SIMULATION_TICK_RATE)
        {
            simulationTickTimer -= SIMULATION_TICK_RATE;
            RunSimulationTick();
        }
        
        // Render UI based on current race state
        RenderUI();
    }
    
    void RunSimulationTick()  // Called 20× per second
    {
        currentLap++;
        
        // For each car (24 cars):
        foreach (var car in cars)
        {
            // 1. Calculate lap time for this car
            float lapTime = CalculateLapTime(car);
            car.lapTimes.Add(lapTime);
            car.totalTime += lapTime;
            
            // 2. Update position (based on total time vs competitors)
            UpdateCarPosition(car);
            
            // 3. Update tire state (simple formula)
            UpdateTireState(car);
            
            // 4. Update fuel consumption
            car.fuel -= CalculateFuelConsumption(car);
            
            // 5. Check pit stop condition
            if (ShouldPit(car))
            {
                ExecutePitStop(car);
            }
            
            // 6. Check for crash/damage
            if (CheckCollision(car))
            {
                HandleCrash(car);
            }
        }
        
        // 7. Update weather
        weatherSystem.UpdateWeather();
        
        // 8. Check for race end condition
        if (currentLap >= raceLaps)
        {
            FinalizeRace();
        }
        
        // 9. Publish events (UI listens for these)
        eventManager.PublishUpdates(cars, weather);
    }
}
```

**Key advantage**: All simulation on main thread, no threading complexity.

---

## 1.7 RACE STATE STRUCTURE

**Minimal state needed for deterministic save/load**:

```csharp
[Serializable]
public class RaceState
{
    public int currentLap;
    public float elapsedTimeSeconds;
    public bool isRaceActive;
    
    [Serializable]
    public class CarState
    {
        public int carId;
        public int position;  // 1-24
        public float totalTimeSeconds;  // Sum of all lap times
        public float fuel;  // kg remaining
        public Tire[] tires;  // 4 tires
        public float damage;  // 0-100%
        public int consecutiveLapsPushed;  // For tire deg calculation
        public bool hasPitted;  // Track pit history
    }
    
    [Serializable]
    public class Tire
    {
        public string compound;  // "soft", "medium", "hard"
        public float degradation;  // 0-1.0
        public float lapsSinceChange;  // Integer
        public float temperature;  // 60-100°C
    }
    
    public List<CarState> cars = new List<CarState>(24);
    public WeatherState weather;
    public List<string> eventLog;  // Race events (pit, crash, weather)
}
```

**Size**: ~2 KB per lap snapshot (vs 50 MB with full physics state)

---

## 1.8 SAVE/LOAD SYSTEM (Simplified)

**Not complex like physics sim** - we can save the entire race state.

```csharp
public class RacePersistence
{
    public void SaveRace(RaceState state, string filename)
    {
        // Serialize to JSON (simple, human-readable)
        string json = JsonConvert.SerializeObject(state, Formatting.Indented);
        
        // Compress (optional, but nice for Steam Cloud)
        byte[] compressed = CompressString(json);
        
        // Write to disk
        File.WriteAllBytes(filename, compressed);
    }
    
    public RaceState LoadRace(string filename)
    {
        byte[] data = File.ReadAllBytes(filename);
        string json = DecompressString(data);
        return JsonConvert.DeserializeObject<RaceState>(json);
    }
}
```

**Advantages**:
- Can debug by reading JSON directly
- No complex binary serialization bugs
- Fast (2 KB → <1 KB compressed)
- Deterministic (JSON always parses the same way)

---

## 1.9 THREADING MODEL (Minimal)

**Option A: Single-threaded (RECOMMENDED for management game)**
- Everything on main thread (simple, no race conditions)
- Performance sufficient (simulation is cheap, ~5-8 ms per tick)
- Debugging trivial (no threading bugs)

**Option B: Two-threaded (if needed later)**
- Main thread: UI rendering (30 FPS)
- Sim thread: Race logic (20 Hz)
- Communication: Thread-safe queue for events
- Benefit: UI never stutters if simulation spikes

**Recommendation**: Start with Option A (single-threaded). If profiling shows UI lag, migrate to Option B without changing simulation logic.

---

## 1.10 DEVELOPMENT WORKFLOW

### Week-by-Week Approach

**Week 1**: Foundation
- [ ] Unity project setup (Git, folder structure)
- [ ] Data structures (RaceState, CarState, TireState)
- [ ] Mock race data (3 cars, hard-coded)

**Week 2-3**: Core simulation
- [ ] Lap time calculator (speed formula)
- [ ] Position updater (overtake logic)
- [ ] Tire degradation formula
- [ ] Weather system
- [ ] Test: 3-car race works, deterministic save/load

**Week 4**: AI & pit strategy
- [ ] AI pit decision tree
- [ ] Morale system
- [ ] Test: 24 cars, realistic pit timing

**Week 5-6**: UI & player interaction
- [ ] Race monitor (standings, graphs)
- [ ] Pit dialog (tire choice, fuel)
- [ ] Driver instructions (push/defend/save)
- [ ] Playtesting: is it fun?

**Week 7-8**: Polish
- [ ] Save/load system
- [ ] Incident handling (crashes, DNF)
- [ ] Qualifying session
- [ ] Bug fixes, optimization

**Week 9-10**: Testing & handoff
- [ ] Full 24-race season playable
- [ ] Profiling (ensure 30 FPS stable)
- [ ] Playtesting feedback integration

---

## 1.11 RISK MITIGATION

### Primary Risks (Management Game Context)

| Risk | Mitigation |
|------|-----------|
| **AI makes dumb pit decisions** | Test AI decisions extensively in W4. Have player review AI pit timing before race starts. |
| **Race feels "ticky" (not smooth)** | Add visual smoothing: interpolate car positions between ticks for display. Race logic is discrete, visual is smooth. |
| **Player bored (strategy not deep)** | Add morale system, tire choice consequences, weather surprises. Test in W6 playtesting. |
| **Save file corruption** | Use JSON (human-readable, easy to debug). Add checksum validation. Keep 3 auto-backups. |
| **UI lags during race** | Profile in W5. If needed, move simulation to separate thread (Option B). |
| **Physics determinism (if we added physics later)** | Not a risk for management game (no physics). Only relevant if pivoting to racing sim. |

---

## 1.12 SUCCESS CRITERIA (End of Pre-Production)

**Technical**:
- [ ] Stable 30 FPS with 24 cars on reference hardware
- [ ] Save/load works (100 cycles without corruption)
- [ ] Deterministic (save at lap 10, load, race produces identical lap 11+)
- [ ] No memory leaks over 2-hour session

**Gameplay**:
- [ ] 1 complete race (Monaco) playable end-to-end (5-10 minutes)
- [ ] Player makes strategic decisions (pit timing, tire choice, push/defend)
- [ ] AI pit decisions are reasonable (within 1-2 lap window of optimal)
- [ ] Race feels exciting (overtakes, close finishes, pit strategy matters)

**Player Experience**:
- [ ] Non-gamer completes 1st race without help
- [ ] Player wants to play race 2 ("just one more race")
- [ ] Race doesn't feel broken/unbalanced

---

## 1.13 NEXT DOCUMENT

**Part 2**: Module Specifications (simplified for management game)
- Lap calculator formula (speed based on setup + driver skill)
- Tire degradation (3-phase, but formula-based)
- Weather evolution
- AI pit strategy decision tree
- UI architecture

---

## SUMMARY: WHY THIS APPROACH?

**Old TDD (Physics-Based)**:
- 40 weeks development
- Complex threading (main/physics/AI/render)
- High performance risk
- Overkill for management game

**New TDD (Lap-Tick Based)**:
- 10-12 weeks development
- Simple single-threaded design
- Deterministic, debuggable
- Perfect for management game
- Can still look good (smooth visual interpolation)

**Bottom line**: We're building Football Manager (strategy/tactics game), not iRacing (racing simulator). Different architecture for different goals.

---

**Document Version**: 2.0  
**Status**: FINAL for Management Game  
**Last Updated**: 2026-04-06  
**Author**: Claude Assistant (Revised based on genre clarification)
