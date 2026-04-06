# TECHNICAL DESIGN DOCUMENT (TDD)
# PART 1: ARCHITECTURE OVERVIEW

**Version**: 1.0  
**Date**: 2026-04-06  
**Status**: Draft for Review  
**Target Platform**: PC (Windows/Steam)  
**Target Engine**: Unity 2022.3 LTS or later  

---

## 1.1 PROJECT SCOPE (Technical)

### Core Requirements Summary
- **24 simultaneous cars** in real-time race simulation (50-58 laps)
- **Full physics simulation** (tire degradation, fuel consumption, damage)
- **Dynamic weather** (rain, track temperature, grip changes)
- **AI agents** (24 drivers making tactical decisions)
- **Real-time UI** showing live standings, telemetry, strategy
- **Acceleration options** (1x, 2x, 5x, 10x, 20x) without breaking simulation
- **Single-player only** (no multiplayer)
- **Target FPS**: 60 FPS minimum on mid-range PC (GTX 1060, i5-8400 equivalent)

### What This Means Architecturally
1. **Performance is critical** — 24 physics objects + AI logic every frame = ~1440+ physics calculations per second
2. **Determinism required** — simulation must be reproducible (save/load states)
3. **Decoupled rendering** — visual FPS can differ from sim step rate (sim runs at fixed timestep)
4. **Heavy use of multithreading** — physics on separate thread, AI on another, rendering on main thread

---

## 1.2 HIGH-LEVEL ARCHITECTURE

### Core Modules (8 Major Systems)

```
┌─────────────────────────────────────────────────────────────┐
│                    GAME LOOP (Main Thread)                   │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │ Input Handler | UI Renderer | Camera Controller          │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
         ↕                    ↕                    ↕
    ┌─────────┐         ┌──────────┐      ┌──────────────┐
    │  EVENT  │         │   RACE   │      │   STRATEGY   │
    │ MANAGER │────────→│SIMULATOR │←─────│   ADVISOR    │
    │         │         │          │      │              │
    └─────────┘         └──────────┘      └──────────────┘
                             ↕
        ┌────────────────────┬─────────────────┬──────────────┐
        ↓                    ↓                 ↓              ↓
    ┌─────────┐      ┌──────────┐      ┌──────────┐   ┌──────────┐
    │ PHYSICS │      │   TIRE   │      │ WEATHER  │   │   AI     │
    │ ENGINE  │      │ SYSTEM   │      │ SYSTEM   │   │ SYSTEM   │
    │         │      │          │      │          │   │          │
    └─────────┘      └──────────┘      └──────────┘   └──────────┘
        ↓                    ↓                 ↓              ↓
    ┌─────────────────────────────────────────────────────────┐
    │             DATA PERSISTENCE LAYER                       │
    │   (Save/Load states, player progression, telemetry)     │
    └─────────────────────────────────────────────────────────┘
```

### Module Responsibilities

| Module | Purpose | Threading |
|--------|---------|-----------|
| **Race Simulator** | Master coordinator: manages lap progression, car states, marshal events | Main thread |
| **Physics Engine** | Longitudinal/lateral dynamics, collision detection, damage model | Physics thread (fixed-step) |
| **Tire System** | Tire degradation, temperature, compound selection, aquaplaning | Physics thread |
| **Weather System** | Temperature dynamics, precipitation, track grip changes | Async updates |
| **AI System** | 24 driver agents: pit strategy, overtaking, fuel management, risk assessment | AI thread + Physics thread |
| **Strategy Advisor** | Recommendations to player: pit timing, tire compound, driver instructions | Main thread |
| **Event Manager** | Broadcasts events (crash, DNF, pit stop, weather change) to all listeners | Main thread |
| **Data Persistence** | Save/load race states, season progress, telemetry logs | Async I/O |

---

## 1.3 PERFORMANCE CONSTRAINTS & TARGETS

### FPS & Frame Budget

**Target**: 60 FPS on mid-range PC
- **Frame time**: 16.67 ms per frame
- **Simulation tick**: 20 ms (50 Hz) OR 10 ms (100 Hz) — TBD during prototype
  - At 20 ms: Sim runs at 50 Hz, rendering at 60 FPS (rendering interpolates between sim frames)
  - At 10 ms: Sim runs at 100 Hz, rendering at 60 FPS (better responsiveness)

**Frame Budget Allocation** (per 16.67 ms frame at 60 FPS):
- Physics simulation: 4-5 ms (24 cars, collision detection)
- AI decision-making: 2-3 ms (pathfinding, pit strategy evaluation)
- UI rendering: 3-4 ms (standings, telemetry panels)
- Camera/effects: 2-3 ms
- Overhead/misc: 2 ms

**Hardware Target** (Minimum):
- CPU: Intel i5-8400 or AMD Ryzen 5 2600 (6-core, 3.6+ GHz)
- GPU: NVIDIA GTX 1060 or AMD RX 580 (3GB+)
- RAM: 8-12 GB
- Storage: SSD with 15-20 GB free

### Scalability Strategy

**Quality Settings** (player-configurable):
1. **Ultra (60 FPS target)**: All physics detail, 24 cars, full AI
2. **High (stable 60 FPS)**: Reduced collision detail, 18 AI cars (6 static)
3. **Medium (stable 30 FPS)**: Simplified physics, 12 AI cars, no advanced weather
4. **Low (forced 30 FPS)**: Basic physics, text-only race monitor

---

## 1.4 ENGINE & TECHNOLOGY CHOICES

### Recommended: Unity 2022.3 LTS

**Why Unity?**
1. **Physics**: Built-in PhysX engine (adequate for 24-car simulation with optimization)
2. **UI**: UIToolkit for responsive, performance-conscious UI (better than UGUI for dense data)
3. **Multithreading**: Good support via Job System and Burst compiler
4. **Cross-platform**: Easy Steam integration (Steamworks SDK)
5. **Development speed**: Large community, asset ecosystem

**Alternative Considered: Unreal Engine 5**
- Pros: Better graphics, superior physics engine (Chaos)
- Cons: Steeper learning curve, overkill for management game (performance waste on rendering)
- Decision: Unity preferred for management-focused game with simpler visuals

### Programming Languages

**Primary**: C# (Unity scripting)
**Data Format**: JSON (GDD data, car setups) + CSV (telemetry logs)

### Key Plugins/Assets (License: Check for Steam availability)

1. **Cinemachine** (free) — camera management
2. **Dotween** (free/pro) — UI animations
3. **Newtonsoft JSON** (free) — robust JSON parsing
4. **Steamworks.NET** (free) — Steam integration
5. **Vehicle Physics Pro** (optional, $50) — if custom physics becomes bottleneck

### Data Serialization

**Race State Serialization** (for save/load):
- Binary format (custom serializer) for performance
- JSON for human-readable logs/debugging
- Compression: LZ4 compression on save files

---

## 1.5 CORE SIMULATION LOOP (Pseudocode)

```
MAIN THREAD:
  While game running:
    FrameStart = now()
    
    1. Input Handler
       - Get player input (pause, UI clicks, speed controls)
    
    2. Enqueue Physics Sim Commands
       - Send current race state snapshot to Physics thread
       - Include: car positions, velocities, inputs
    
    3. Strategy Advisor Update
       - Based on last physics frame output, calculate recommendations
       - Pit window warnings, tire strategy analysis
    
    4. AI Decision Layer (Main Thread)
       - Each AI agent evaluates pit timing, fuel, morale
       - Queue commands for next physics sim
    
    5. Wait for Physics Thread
       - Block until physics frame is complete (if frame time allows)
       - Or use frame N-1 results if physics not ready
    
    6. UI Rendering
       - Render standings, telemetry panels, track map
       - Use interpolated physics data between frames
    
    7. Camera & Effects
       - Update camera following active car
       - Render weather effects (rain shader)
    
    FrameEnd = now()
    Sleep until next frame (60 FPS target)


PHYSICS THREAD (Fixed Timestep: 20 ms or 10 ms):
  While simulation running:
    1. Read race state from main thread
    
    2. For each car (24):
       a. Update fuel consumption based on throttle
       b. Update tire temperature & degradation
       c. Apply chassis forces (gravity, downforce)
       d. Apply motor forces (power unit output)
       e. Integrate velocity & position (Euler or RK4)
    
    3. Collision Detection
       - Broad phase: spatial partitioning (quadtree)
       - Narrow phase: car-to-car, car-to-environment
       - Damage calculation
    
    4. Constraint Solving (if collisions detected)
       - Resolve overlaps, apply impulses
    
    5. Tire Physics Update
       - Check aquaplaning condition (wet track)
       - Update grip level based on tire temp & degradation
    
    6. Marshal & Safety Checks
       - Detect crashes, activate safety car if needed
       - Check track limits (penalties)
    
    7. Output: Publish updated car states to main thread


AI THREAD (Variable timestep, ~100 ms):
  Every 100 ms (or every 10 physics frames at 100 Hz):
    1. For each AI driver:
       a. Evaluate pit strategy
          - Current fuel level vs. remaining laps
          - Tire degradation vs. pit window
          - Competitor pit timing
       
       b. Evaluate overtaking opportunity
          - DRS zone entry? Can we follow this car?
          - Fuel available for push?
          - Driver morale/risk tolerance
       
       c. Morale adjustment
          - Losing to teammate? Unhappy
          - Long stint on damaged tires? Frustrated
          - Strong performance? Confident
       
       d. Queue next action (pit/push/defend/save)
    
    2. Publish decisions to physics thread for next frame
```

---

## 1.6 THREAD SAFETY & SYNCHRONIZATION

### Thread Model

```
Main Thread (60 Hz):
  ├─ Receives: Physics results from frame N-1
  ├─ Sends: Race state to Physics thread for frame N
  └─ Renders frame based on interpolation

Physics Thread (50-100 Hz, fixed step):
  ├─ Receives: Command queue from main thread
  ├─ Computes: All 24-car physics in fixed timestep
  └─ Sends: Frame results back to main thread

AI Thread (~10 Hz variable):
  ├─ Receives: Telemetry snapshots
  ├─ Analyzes: Pit strategy, morale, tactical decisions
  └─ Sends: Decisions to physics thread via command queue
```

### Synchronization Mechanisms

**Double Buffering**: Two snapshots of race state
- Frame N-1 (stable, ready to read)
- Frame N (being written by physics thread)
- Main thread always reads N-1 while physics writes N

**Lock-Free Queues**: Command queues between threads
- No mutex locks (too slow for 60 FPS)
- Use atomic operations for enqueue/dequeue

**Memory Barriers**: Volatile fields for shared state flags
- Pause flag, speed multiplier, simulation state

---

## 1.7 SAVE/LOAD SYSTEM ARCHITECTURE

### Race State Serialization

**What Gets Saved**:
1. Car states (position, velocity, fuel, tire compound, damage)
2. Weather state (current temp, precipitation, wind)
3. Lap history (lap times, pit stop data, incidents)
4. AI decisions log (pit stops executed, overtakes attempted)
5. Player UI state (camera focus, pause position)

**Serialization Format**:
```json
{
  "race_id": "monaco_2027_round_3",
  "lap": 15,
  "elapsed_time_ms": 1234567,
  "weather": {
    "temperature": 24.5,
    "humidity": 65,
    "precipitation": 0.0
  },
  "cars": [
    {
      "car_id": 1,
      "position": { "x": 100, "y": 50, "z": 0 },
      "velocity": { "x": 85, "y": -2, "z": 0 },
      "fuel_kg": 45.2,
      "tire_compound": "soft",
      "tire_degradation": 0.65,
      "damage": 0.0
    },
    // ... 23 more cars
  ]
}
```

**Performance Optimization**:
- Binary format for in-memory snapshots (fast serialization)
- JSON only for manual debugging/inspection
- Compression: LZ4 on save files (reduces 50 MB state to ~8 MB)

---

## 1.8 DEVELOPMENT TOOLS & PIPELINE

### Version Control & Collaboration
- **Git**: Primary VCS (GitHub for code)
- **Art Assets**: Kept in separate folder (not version controlled, stored on Drive/Dropbox)
- **Documentation**: Markdown in Git with this TDD

### Development Environment
- **IDE**: Visual Studio 2022 Community (free)
- **Editor**: Unity 2022.3 LTS
- **Debugger**: VS2022 + Unity Debugger
- **Version tracking**: Semantic versioning (0.1.0 = alpha, 1.0.0 = release)

### Build Pipeline

```
Local Development
    ↓
    Code commit → Git
    ↓
    CI/CD (Optional: GitHub Actions)
    ↓
    Build to Standalone EXE
    ↓
    Automated testing (smoke tests)
    ↓
    Manual QA (race simulation verification)
    ↓
    Steam distribution (when ready)
```

---

## 1.9 RISK MITIGATION (Technical)

### High-Risk Areas

| Risk | Impact | Mitigation |
|------|--------|-----------|
| **24-car physics bottleneck** | Game unplayable at target FPS | Prototype with 8 cars first, profile, optimize |
| **AI decision lag** | Unrealistic driver behavior | Parallel AI thread, separate from physics |
| **Save/load corruption** | Player progress lost | Binary format validation, auto-backups |
| **Memory leaks** | Frame rate degrades over time | Regular profiling, Unity Memory Profiler |
| **Physics determinism loss** | Save/load produces different results | Fixed timestep discipline, avoid floating-point divergence |

---

## 1.10 SUCCESS CRITERIA (For TDD Phase)

By end of TDD refinement:
- [ ] Detailed module APIs defined (next document)
- [ ] Performance budget allocated per system
- [ ] Threading model finalized and documented
- [ ] Save/load format designed
- [ ] CI/CD pipeline sketched
- [ ] Unity project structure documented (folder layout)

**Next**: Technical specifications for each module (Physics, AI, UI, Weather)

---

**TDD Author**: Claude Assistant  
**Review Status**: Awaiting feedback  
**Last Updated**: 2026-04-06
