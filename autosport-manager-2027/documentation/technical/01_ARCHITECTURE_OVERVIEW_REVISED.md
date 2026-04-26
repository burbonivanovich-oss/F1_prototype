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
- **Non-deterministic by design** (save/load may produce different outcomes — see ADR 2026-04-25). Dev-only seed override for QA bug reproduction.
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

Instead of frame-by-frame physics, the race progresses in discrete **lap ticks** every 50 ms wall-clock (20 Hz simulation rate). The amount of in-game time advanced per tick is controlled by the player's speed multiplier (see §1.15). The simulation tick rate itself stays constant so the frame budget does not flex with player speed choice.

```
TRADITIONAL PHYSICS APPROACH:
Frame 1 (16.67 ms) → Update position 0.1 meters
Frame 2 (16.67 ms) → Update position 0.1 meters
...
Frame 60 (1000 ms) → 1 lap completed

MANAGEMENT GAME APPROACH (Better):
Sim tick fires every 50 ms wall-clock (20 Hz, fixed).
In-game time advanced per tick = base_step * speed_multiplier
Player chooses speed multiplier (pause / 1x / 2x / 4x / 8x / instant).
Race wall-clock duration is therefore player-controlled, not fixed.
```

**Benefits**:
- Simple (lap outcome = formula, not 1000 physics calculations)
- Performant (CPU usage negligible)
- Debuggable (race progression visible in logs; dev-seed override allows reproducible runs in QA builds)

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

**Hybrid hooks**: Step 2 (`UpdateCarPosition`) calls `ResolveOvertakeAttempt()` defined in §1.6.1 when two cars are within overtake range. Step 6 (`CheckCollision`) calls `RollIncident()` defined in §1.6.2 only when an overtake attempt fails. Neither hook adds physics — both are formula-driven probability rolls.

---

## 1.6.1 OVERTAKE RESOLUTION (Hybrid)

When the gap between attacker and defender closes below a track-specific threshold during a tick, the simulation rolls an overtake outcome rather than instantly swapping positions.

```
gap_closed = (speed_attacker - speed_defender) * tick_dt
if gap_closed > 0 AND distance < OVERTAKE_THRESHOLD:
    P(success) = sigmoid(
          w1 * (skill_attacker - skill_defender)
        + w2 * (pace_attacker - pace_defender)
        + w3 * tire_age_delta
        + w4 * DRS_flag
        + w5 * track_overtake_difficulty
        - w6 * defender_aggression
    )
    if RNG.NextDouble(seed_for_attempt) < P(success):
        SwapPositions(attacker, defender)
    else:
        RollIncident(attacker, defender)   // see §1.6.2
```

**Inputs** (variables only — tuning numbers belong in design docs, not TDD):
- Driver skill: `overtaking` and `defending` sub-stats (0-100)
- Pace delta: current lap-time potential difference
- Tire age delta: laps since last pit per car
- DRS flag: boolean from track + race rules
- `track_overtake_difficulty`: per-circuit constant (Monaco high, Monza low)
- `defender_aggression`: derived from current driver instruction (push/defend/save) and morale

**RNG seeding**: Per the ADR 2026-04-25 non-determinism decision, the per-attempt seed is derived from a non-stable source (e.g. wall-clock + monotonic counter). In dev/QA builds with `AUTOSPORT_RNG_SEED` set, the seed is derived deterministically from `(seedOverride, lap, attackerId)` for reproducible bug investigation.

---

## 1.6.2 INCIDENT MODEL (Hybrid)

When an overtake attempt fails, the simulation rolls an incident outcome:

```
P(incident) = (1 - P(success)) * driver_risk_appetite * track_risk_factor
roll = RNG.NextDouble(seed_for_incident)

if roll > P(incident):
    outcome = CLEAN          // no penalty
elif roll > P(incident) * 0.7:
    outcome = MINOR_CONTACT  // pace penalty +0.2-0.5s/lap, repairable at pit
else:
    outcome = MAJOR_INCIDENT
    car.aeroDamage += rand(0.3, 0.6)
    car.mechanicalDamage += rand(0.2, 0.5)
    if car.aeroDamage + car.mechanicalDamage >= DNF_THRESHOLD:
        car.status = DNF
```

**Inputs**:
- `driver_risk_appetite` ∈ {conservative, balanced, aggressive} — see §1.7 `CarState.driverRiskAppetite`
- `track_risk_factor`: per-circuit constant (street circuits high, modern tracks low)
- `incidentRiskAccumulator`: increases on near-misses, resets at pit stop or after a clean lap (prevents runaway incident chains)
- `lastIncidentLap`: cooldown to prevent double-incidents within N laps

**Outcomes feed back into**: `CarState.aeroDamage`, `CarState.mechanicalDamage`, `CarState.status` (see §1.7).

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
        public int consecutiveLapsPushed;  // For tire deg calculation
        public bool hasPitted;  // Track pit history

        // Hybrid additions (see §1.6.2 incident model):
        public float aeroDamage;             // 0-1, multiplier on cornering pace
        public float mechanicalDamage;       // 0-1, multiplier on top speed
        public float incidentRiskAccumulator;// resets at pit / clean lap
        public byte lastIncidentLap;         // cooldown to prevent double-incidents
        public RiskProfile driverRiskAppetite; // conservative/balanced/aggressive — derived from morale + instruction
    }

    public enum RiskProfile { Conservative, Balanced, Aggressive }
    
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

**Mandatory from day 1 (per ADR 2026-04-25)**: a `SnapshotBuffer` abstraction. The simulator publishes immutable per-tick snapshots (car positions, lap times, events) into a double-buffered store; the UI reads from the buffer rather than from live sim state. This keeps Option A simple while making the future migration to Option B a thread-boundary swap rather than a rewrite. See §1.14 for how the UI consumes snapshots.

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
- [ ] Dev-seed override (`AUTOSPORT_RNG_SEED`) reproduces identical race outcomes in QA builds; release builds are non-deterministic by design (see ADR 2026-04-25)
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

## 1.14 VISUAL INTERPOLATION LAYER

**Layer**: UI (`RaceMonitorView`), not simulation. The simulation never reads interpolated values — its state remains tick-discrete.

**Goal**: smooth movement of 2D track-map dots at 30 FPS even though the sim updates every 50 ms (20 Hz). Without interpolation, dots would snap-step ~1.5× per frame.

**Mechanism**:
1. Each simulation tick, the sim writes an authoritative `Snapshot` (per-car track-distance scalar `position_t ∈ [0, 1]`, per-car lap-time, events) to the immutable `SnapshotBuffer` from §1.9.
2. The UI keeps the latest two snapshots: `S_prev` (tick `t`) and `S_curr` (tick `t+1`).
3. Each frame, the UI computes `alpha = (now - S_curr.tickTime) / 50ms` clamped to `[0, 1]`.
4. Per car: `displayPosition = lerp(S_prev.position, S_curr.position, alpha)`.
5. The display position is a track-distance scalar; the UI projects it onto the per-track polyline (see content pipeline below) via arc-length lookup to get screen-space `(x, y)`.

**Track polyline asset** (per-circuit content, 24 circuits for v1):
- Source: open GPS traces (e.g. OpenStreetMap relations for permanent circuits, GPX exports for street circuits).
- Format: ordered list of (lat, lon) points, normalized to local 2D coordinates and sampled to ~200-400 points per lap.
- Cached metadata: total arc length (meters), per-segment cumulative length for fast `position ∈ [0,1] → (x, y)` lookup.
- Pipeline risk: per-circuit licensing must be validated before content lock-in (see `project-config.json` `risks.medium_priority`).

**Why UI-side, not sim-side**: keeping interpolation in the UI preserves a clean contract — the simulation produces discrete ground-truth events and positions; the renderer is responsible only for visual smoothness. This also means a future replay/spectator mode can be a pure consumer of snapshot streams.

---

## 1.15 RACE SPEED MODEL

**Producer requirement (2026-04-25)**: race wall-clock duration is player-controlled with a speed multiplier (pause / 1x / 2x / 4x / 8x / instant), not fixed at ~2 minutes as the original §1.2 implied.

**Implementation contract**:

- Simulation tick rate is fixed at 20 Hz (50 ms wall-clock per tick) regardless of speed multiplier. Frame budget per TDD §1.4 does not flex.
- Each tick advances `in_game_time_step = base_step * speed_multiplier`. At 1x, `base_step` is calibrated so that one race lap occupies a producer-chosen real-time duration (target TBD via playtest; see open question below).
- Speed multipliers ≤ 1x apply by reducing in-game time per tick — visually smoother because more snapshots per in-game lap.
- Speed multipliers > 1x apply by advancing more in-game time per tick — fewer interpolation samples, but UI still renders at 30 FPS so motion stays smooth.
- "Instant" mode: simulation runs in a tight loop without `Time.deltaTime` gating until race end or next interactive prompt; UI shows progress indicator. Used when player wants to skip ahead.
- "Pause" mode: simulator does not advance in-game time. UI remains fully interactive (player can browse standings, plan strategy, change speed, issue instructions).

**Why simulation rate is constant (not scaled)**: scaling sim Hz with speed multiplier would push 4×–8× modes outside the §1.4 frame budget and risk variable physics behaviour. Holding sim rate constant and varying time-per-tick keeps performance predictable; the tradeoff is fewer interpolation samples per in-game lap at high speeds, which is acceptable for management-sim viewing.

**Open question (needs systems-designer + playtest)**: real-time-per-lap calibration at 1x. Two reference points to choose from:
- Real F1 lap times (~80–120 seconds) → 60-lap race ≈ 80–120 minutes at 1x. Realistic but very long.
- Compressed "engineer time" (e.g., 10–20 seconds per lap at 1x) → 60-lap race ≈ 10–20 minutes at 1x. Better fit for "watch + decide" loop.

Recommend the compressed model (~15s/lap at 1x) pending playtest. Speed multipliers then give 7.5s/lap at 2x, 3.75s/lap at 4x, sub-2s at 8x, immediate at instant.

**Status**: needs technical-director sign-off on the constant-sim-rate-with-variable-time-step model before implementation; calibration target needs playtest.

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
