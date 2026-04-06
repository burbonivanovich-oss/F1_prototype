# TECHNICAL DESIGN DOCUMENT (TDD)
# PART 2: MODULE SPECIFICATIONS

---

## 2.1 PHYSICS ENGINE MODULE

### 2.1.1 Responsibilities

**Input**:
- Car throttle/brake input (0-100%)
- Steering angle (-45° to +45°)
- Current tire compound (soft/medium/hard)
- Fuel level (kg)
- Current gear (1-8)

**Output**:
- Car position (x, y, z in world space)
- Velocity vector (m/s)
- Acceleration vector (m/s²)
- Tire contact forces (for physics simulation)
- Damage state (0-100%)

### 2.1.2 Core Physics Model

**Longitudinal Dynamics** (acceleration/braking):
```
Net Force = Power - Drag - Rolling Resistance
Power = Engine Power × (1 - Gear Loss × 0.05) × Throttle %
Drag = 0.5 × Air Density × Drag Coefficient × Velocity²
Rolling Resistance = Tire Friction Coeff × Weight × 9.81

Acceleration = Net Force / Mass

Maximum Performance (Baseline):
- 0-100 km/h: 2.3 seconds
- Top speed: 330 km/h (limited by downforce balance)
- Fuel consumption: 0.8-1.2 kg/lap depending on driving style
```

**Lateral Dynamics** (cornering):
```
Lateral Force = Tire Grip × Normal Force
Tire Grip = Coefficient of Friction × Downforce Loading

Downforce = 0.5 × Air Density × Downforce Coefficient × Velocity²
Downforce Loading = Downforce / Weight

Lateral Acceleration = Lateral Force / Mass

Handling Characteristics (Setup-dependent):
- Understeer: Front grip < Rear grip → Push into wall
- Oversteer: Rear grip < Front grip → Spin risk
- Balanced: Front grip ≈ Rear grip → Optimal performance
```

**Gear & Engine Model**:
```
Available Gears: 1-8 (8-speed dual-clutch transmission)
Optimal RPM Range: 6,000 - 15,000 RPM

Gear Ratios (illustrative):
Gear 1: 2.50 → max speed 45 km/h at 15k RPM
Gear 2: 1.90 → max speed 60 km/h
...
Gear 8: 0.62 → max speed 330+ km/h (limited by drag)

Automatic Gearbox Logic:
- Upshift at 14,800 RPM (or player override)
- Downshift at 6,000 RPM under braking
- Manual option available for players who want control
```

### 2.1.3 Collision System

**Broad Phase** (spatial partitioning):
- **Quadtree** dividing track into cells (100m × 100m)
- Each frame, cars update their cell
- Only check collisions between cars in neighboring cells

**Narrow Phase** (per-pair collision):
```
For each car pair:
  1. Calculate bounding sphere distance
  2. If distance < 2× car width → check detailed collision
  3. Compute overlap vector and impulse
  4. Apply forces to both cars
```

**Collision Damage Model**:
```
Damage = 0.5 × Relative Velocity² × Impact Factor

Impact Factor (collision angle):
- Head-on (180°): 1.0× (maximum damage)
- Side impact (90°): 0.8×
- Rear impact (0°): 0.3×

Damage Effects (0-100%):
- 0-10%: No performance impact
- 10-30%: -0.1 to -0.3 sec/lap aerodynamic drag increase
- 30-60%: -0.5 to -1.0 sec/lap, plus handling instability
- 60-100%: -2.0+ sec/lap, high crash risk
- 100%: DNF (car unfixable mid-race)
```

### 2.1.4 Performance Optimization Techniques

**Culling**:
- Physics objects outside track bounds are skipped
- Collision checks only for nearby car pairs (quadtree reduces from 24×23÷2=276 pairs to ~20 pairs)

**SIMD Vectorization** (Unity Burst compiler):
- Vector math (position, velocity) compiled to SSE/AVX instructions
- 3-4× speedup for numerical computations

**Integration Method**:
- Use **Semi-Implicit Euler** for stability (better than explicit Euler)
- Consider **Verlet integration** for rigid body physics (more stable)

```csharp
// Pseudocode
public class PhysicsEngine
{
    void FixedStep(float dt)
    {
        foreach (var car in cars)
        {
            car.UpdateTireForces();  // Tire grip, temperature
            car.UpdateDynamics(dt);  // Velocity, position
        }
        
        CollisionDetection();
        ResolveCollisions();
        CheckMarshals();  // Track limit violations, safety car
        
        PublishResults();  // Send to main thread
    }
}
```

---

## 2.2 TIRE SYSTEM MODULE

### 2.2.1 Tire State Model

**Per-Tire Data**:
```
struct TireState
{
    float temperature;           // 0-120°C (optimal 50-80°C)
    float degradation;           // 0-1.0 (0=fresh, 1=unusable)
    float lapsSinceChange;       // 0-50
    float compound;              // Soft/Medium/Hard enum
    float currentGrip;           // 0-1.0 multiplier
    float aquaplaningRisk;       // 0-1.0 (0=safe, 1=certain spin)
}
```

### 2.2.2 Tire Degradation Model

**Three-Phase Degradation** (from GDD):

```
PHASE 1: PLATEAU (Laps 0-10 typically)
  - Grip: 100% nominal
  - Lap time impact: 0.0 sec
  - Formula: degradation += 0.0 per lap

PHASE 2: LINEAR WEAR (Laps 11-22 typically)
  - Grip: 95% → 75% over laps 11-22
  - Lap time impact: +0.05 to +0.10 sec per lap
  - Formula: degradation += 0.015 per lap (track-dependent: 0.010-0.020)

PHASE 3: CLIFF (Final 2-3 laps)
  - Grip: <70% (unstable)
  - Lap time impact: +0.30 to +0.80 sec suddenly
  - Formula: degradation += 0.20 per lap (cliff threshold)
```

**Track-Specific Degradation Rates** (example values):

```
Monaco (High Degradation):
  Soft: Phase1=8 laps, Phase2_rate=0.020/lap, Cliff=lap 17
  Medium: Phase1=12 laps, Phase2_rate=0.015/lap, Cliff=lap 26
  Hard: Phase1=18 laps, Phase2_rate=0.010/lap, Cliff=lap 40

Monza (Low Degradation):
  Soft: Phase1=15 laps, Phase2_rate=0.010/lap, Cliff=lap 32
  Medium: Phase1=22 laps, Phase2_rate=0.008/lap, Cliff=lap 42
  Hard: Phase1=30 laps, Phase2_rate=0.005/lap, Cliff=lap 65
```

**Tire Temperature Model**:

```
Temperature Change Rate (per lap):
  ΔTemp = (Friction Heat × Slip Ratio) - (Ambient Heat Loss)
  
Friction Heat = Tire Lateral Force × Slip
Ambient Heat Loss = Track Temperature - Tire Temperature × Convection Coeff

Optimal Temperature Range (per compound):
  Soft: 65-75°C (works 60-85°C)
  Medium: 70-80°C (works 65-90°C)
  Hard: 75-85°C (works 70-95°C)

Grip Penalty Outside Optimal:
  Below optimal: -0.1% grip per °C below
  Above optimal: -0.2% grip per °C above (overheat risk)

Warm-up Phase (first lap after pit stop):
  Soft: Lap 1 full penalty (-1.0 sec), Lap 2-3 gradual recovery
  Medium: Lap 1 (-1.5 sec), Lap 2-4 recovery
  Hard: Lap 1 (-2.0 sec), Lap 2-5 recovery
  
  Affected by track temperature:
  - Hot track (>30°C): -20% warm-up time
  - Cold track (<15°C): +30% warm-up time
```

### 2.2.3 Aquaplaning Model (Wet Weather)

```
Aquaplaning Risk = (Water Depth) × (Velocity²) / (Tire Treading)

Water Depth (function of precipitation + drying):
  Light rain: 2-5mm depth
  Heavy rain: 8-15mm depth
  Drying: -1mm per lap as sun returns

Tire Treading (wet tires have deeper grooves):
  Wet tire: High treading (-80% aquaplaning risk)
  Intermediate: Medium (-50% aquaplaning risk)
  Dry tire: None (-0% aquaplaning risk, very dangerous in rain)

Spin Probability (when risk > 0.7):
  Per lap in high-aquaplaning condition:
  Spin Chance = 2% + (AquaplaningRisk - 0.7) × 20%
  
  If spin occurs: Car rotates 180°, loses 0.5-1.0 seconds
```

### 2.2.4 Tire Physics Integration

```csharp
public class TireSystem
{
    public void UpdateTires(CarState car, WeatherState weather, float dt)
    {
        for (int i = 0; i < 4; i++)  // 4 tires
        {
            var tire = car.tires[i];
            
            // Temperature update
            float frictionHeat = CalculateFrictionHeat(tire, car);
            float heatLoss = CalculateHeatLoss(tire, weather);
            tire.temperature += (frictionHeat - heatLoss) * dt;
            
            // Degradation update
            float degradationRate = GetDegradationRate(
                tire.compound, 
                tire.lapsSinceChange,
                weather.trackTemp
            );
            tire.degradation += degradationRate * dt;
            
            // Grip calculation
            tire.currentGrip = CalculateGrip(tire, weather);
            
            // Aquaplaning check
            if (weather.precipitation > 0)
            {
                tire.aquaplaningRisk = CalculateAquaplaning(
                    tire, 
                    weather, 
                    car.velocity
                );
            }
        }
    }
}
```

---

## 2.3 WEATHER SYSTEM MODULE

### 2.3.1 Weather State

```
struct WeatherState
{
    float temperature;           // Ambient °C (15-40°C range)
    float trackTemperature;      // Track surface °C
    float humidity;              // 0-100%
    float precipitation;         // 0-100% (0=dry, 100=heavy rain)
    float windSpeed;             // 0-25 km/h
    float windDirection;         // 0-360°
}
```

### 2.3.2 Weather Evolution Model

**Realistic Weather Patterns**:

```
Weather State Transitions:
  Dry → Light Rain: When precipitation > 20%
  Light Rain → Heavy Rain: When precipitation > 60%
  Heavy Rain → Light Rain: When precipitation decreasing < 40%
  Light Rain → Dry: When precipitation < 10%

Precipitation Change (per 5 laps ~ 15 min real time):
  Dry spell: precipitation -= 5% per interval
  Building storm: precipitation += 8% per interval
  Heavy rain: precipitation ±10% per interval (volatile)
  Clearing: precipitation -= 3% per interval

Track Temperature Update:
  ΔTrackTemp = (Ambient Temp - Track Temp) × 0.05
           + (Solar Heat if dry) × 0.02
           - (Rain cooling if wet) × 0.08

Track Temperature Extremes:
  Minimum: Ambient - 15°C (heavy rain)
  Maximum: Ambient + 20°C (sunny, full sun)
```

**Weather Example: Monaco Dry → Rain Transition**:
```
Lap 1-10: Dry, temp 24°C, track 28°C
Lap 11: Clouds build, precipitation → 5%
Lap 12-14: Light rain, precipitation 10-20%, track temp drops to 22°C
Lap 15: Heavy rain arrives! Precipitation 50%, track 18°C
Lap 18-22: Peak rain, precipitation 70%, track 16°C
Lap 25: Rain clearing, precipitation 40%
Lap 28+: Drying, precipitation 10%, track warming to 24°C
```

### 2.3.3 Weather Impact on Gameplay

**Tire Compound Viability** (track temp dependent):

```
Soft tires:
  Dry & warm (>25°C): Excellent grip, short life
  Dry & cold (<18°C): Poor warm-up, 1 sec slower
  Wet: Very dangerous (0.5 sec/lap penalty)

Medium tires:
  Dry & warm: Good balance
  Dry & cold: Acceptable, slight penalty
  Wet: Slower than wets, aquaplaning risk

Hard tires:
  Dry & warm: Low grip (-0.15 sec/lap)
  Dry & cold: Okay, good life
  Wet: Dangerous, high aquaplaning

Wet tires:
  Wet: Optimal grip
  Dry: Blister/overheat risk (-1.0+ sec/lap)
  Transitional: Good option when drying

Intermediate tires:
  Light rain: Good compromise
  Heavy rain: Less effective than wets
  Drying: Excellent balance during transition
```

**Strategic Weather Decisions**:

```
Player Decision at Lap 15 (rain arriving):
  Option A: Pit now for wets (lose 5 seconds to pit, but safe)
  Option B: Stay out, gamble rain stops
  Option C: Pit for intermediates (medium confidence)

AI Driver Feedback:
  "It's getting wet! Box us for wets!"
  or
  "I can stay out, tires are good"
  (morale affects willingness to risk)
```

---

## 2.4 AI DRIVER MODULE

### 2.4.1 AI Architecture

**Per-Driver State**:
```
struct AIDriver
{
    int driverId;
    float skillLevel;            // 0-100 (rating of driver)
    float morale;                // 0-100 (affects aggression)
    float riskTolerance;         // 0-100 (pit vs stay out)
    float mentality;             // Aggressive/Balanced/Conservative
    
    // Decision state
    bool wantsPitStop;
    float desiredTireCompound;   // Soft/Medium/Hard
    bool wantsToAttack;
    float desiredFuelAmount;     // kg to add
}
```

### 2.4.2 AI Decision Tree

**Every 100 ms (~10 physics frames), each AI driver evaluates**:

#### **Step 1: Pit Strategy Assessment**

```
Current pit window for my tires?
  If degradation > 0.9 AND lap > cliff_threshold:
    URGENT PIT REQUIRED (next lap or crash risk)
  Else if degradation > 0.7 AND fuel < 8 laps worth:
    PLAN PIT (next 2-3 laps)
  Else if lap < race_end - 5:
    NORMAL WINDOW (choose optimal gap)
  Else:
    FINAL STINT (push hard, no more pits)

Optimal pit timing:
  Find leader's pit lap
  Calculate: Can I pit same lap and undercut?
  If yes, undercut strategy
  If no, wait and overcut
  If enemy pitting now, react with own pit
```

#### **Step 2: Fuel Management**

```
Remaining fuel = current_fuel
Laps remaining = race_laps - current_lap

Fuel consumption:
  Base: 0.8 kg/lap (cruise)
  Push: +0.2 kg/lap if driver attacking
  Brake regen: -0.05 kg/lap (if ERS/hybrid system)

Decision:
  If fuel/lap_remaining < 0.7:
    MUST PIT SOON (fuel critical)
  If fuel/lap_remaining < 0.9:
    PIT NEXT WINDOW
  If fuel/lap_remaining > 1.2:
    CAN PUSH (extra fuel available)
```

#### **Step 3: Overtaking Opportunity**

```
Is car ahead vulnerable?
  - Tires degraded? (cliff phase risk)
  - Just pitted and cold tires? (+1.5 sec slower)
  - Fuel heavy? (slower pace)

Can I overtake?
  - DRS zone available? (auto +5 km/h)
  - Fuel for push mode? (eat 0.2 kg extra)
  - Skill level advantage vs target? (if <10 pts, risky)

Risk calculation:
  Overtake risk = (Speed gap vs target) / (Driver skill × morale)
  If risk > morale/100:
    Conservative: Stay behind (wait for better opportunity)
    Balanced: Attack if clear weather
    Aggressive: Attack regardless (high spin risk)

Decision:
  If opportunity detected:
    SET wantsToAttack = true
    PLAN overtake for next 1-2 laps
```

#### **Step 4: Morale & Team Dynamics**

```
Morale changes:
  +5: Made a good overtake
  +3: Pit stop executed well
  -5: Lost position to slower car
  -10: Forced to let teammate pass (team orders)
  -8: Tires are bad and falling back
  +10: Moving up positions consistently

If morale < 40:
  Driver becomes risk-averse:
  - Less likely to attack
  - Prefers conservative pit strategy
  - Fuel consumption increases slightly (stressed)

If morale > 80:
  Driver becomes aggressive:
  - More likely to attack
  - Willing to take tire risks
  - Pushes hard even when degraded
```

### 2.4.3 AI Implementation

```csharp
public class AIDriver
{
    private float lastDecisionTime = 0;
    private const float DECISION_INTERVAL = 0.1f;  // 100ms
    
    public void Update(RaceState race, float dt)
    {
        lastDecisionTime += dt;
        
        if (lastDecisionTime >= DECISION_INTERVAL)
        {
            lastDecisionTime = 0;
            
            // Evaluate pit strategy
            EvaluatePitStrategy(race);
            
            // Evaluate overtaking
            EvaluateOvertake(race);
            
            // Evaluate morale
            UpdateMorale(race);
            
            // Generate action command
            var command = GenerateCommand(race);
            race.commandQueue.Enqueue(command);
        }
    }
    
    private void EvaluatePitStrategy(RaceState race)
    {
        var tire = this.tires[0];  // Front left (representative)
        var fuelLapsRemaining = this.fuel / this.fuelConsumption;
        
        bool shouldPit = false;
        if (tire.degradation > 0.9)
            shouldPit = true;  // Urgent
        else if (this.fuel < fuelLapsRemaining * 0.9)
            shouldPit = true;  // Fuel critical
        
        this.wantsPitStop = shouldPit;
    }
}
```

---

## 2.5 UI/UX MODULE

### 2.5.1 Race Monitor Screen Layout

```
┌────────────────────────────────────────────────────┐
│  RACE MONITOR: Monaco Grand Prix - Lap 15/78      │ (Header)
├─────────────────┬──────────────────┬──────────────┤
│   STANDINGS     │   TRACK MAP      │  TELEMETRY   │
│  (Left Panel)   │   (Center)       │  (Right)     │
│                 │                  │              │
│ Pos Driver Team │                  │ Speed: 250   │
│  1  H          │  [Visual race]   │ RPM: 12,000  │
│  2  V          │  [24 cars shown] │ Fuel: 45 kg  │
│  3  L          │  [DRS zones]     │ Tires: Soft  │
│  4  S          │  [Safety car?]   │ Deg: 45%     │
│  ... (scroll)  │  [Weather]       │              │
│                │                  │ Pit Window:  │
│                │                  │ Lap 16-19    │
│                │                  │              │
└────────────────┴──────────────────┴──────────────┘
│ RACE ACTIONS LOG                                   │
│ Lap 15: Sainz pitted for mediums (P3 → P7)        │
│ Lap 15: Weather: Light rain building              │
│ Lap 14: Leclerc fastest lap 1:28.523              │
│ ...                                                │
└────────────────────────────────────────────────────┘
│ CONTROLS: [1x][2x][5x][10x][20x] [Pause] [Settings]│
└────────────────────────────────────────────────────┘
```

### 2.5.2 UI Tech Stack

**Framework**: Unity UIToolkit (not UGUI)
- **Pros**: Retained mode, better performance, UXML markup language
- **Cons**: Steeper learning curve, less documentation

**Rendering Thread**:
- Runs every frame (60 FPS) on main thread
- Reads from last completed physics frame
- Interpolates positions between frames for smooth animation

**Data Binding**:
- Standings panel: Redrawn every 0.5 seconds (not every frame, too many car state updates)
- Telemetry: Updated every physics frame for player car
- Action log: Appended only on events (pit stop, crash, weather)

**Performance Budget**: 3-4 ms per frame for UI rendering

---

## 2.6 DATA PERSISTENCE MODULE

### 2.6.1 Save File Format

**Race Save File** (compressed binary):
- Size: ~8-12 MB (uncompressed 50+ MB)
- Location: `%APPDATA%/AutosportManager/SaveGames/race_<id>.dat`

**Metadata JSON** (alongside .dat file):
```json
{
  "save_version": "1.0",
  "race_id": "monaco_2027_r3",
  "race_name": "Monaco Grand Prix",
  "timestamp": "2026-04-06T14:32:00Z",
  "player_team": "Ferrari",
  "current_lap": 15,
  "weather": "Light rain",
  "player_position": 3
}
```

### 2.6.2 Serialization Details

**Binary Format** (custom serializer):
```
Header (32 bytes):
  - Version: uint32 (0x01000000)
  - Checksum: uint32
  - Timestamp: uint64
  - Compressed size: uint32

Physics State:
  For each car (24):
    - Position (float[3]): 12 bytes
    - Velocity (float[3]): 12 bytes
    - Fuel (float): 4 bytes
    - Tire compound (byte): 1 byte
    - Tire degradation (float[4]): 16 bytes
    - Damage (float): 4 bytes
  Total per car: ~60 bytes
  24 cars: 1,440 bytes

Weather State: 32 bytes
Track State: 8 bytes
AI State (morale, etc): 24 bytes per driver = 576 bytes

Total uncompressed: ~2,050 bytes (minimal!)
```

**Compression**: LZ4 compression
- Uncompressed size: 50 MB (includes full state arrays)
- Compressed: 8-12 MB (typical 6:1 compression ratio)

### 2.6.3 Save/Load Operations

```csharp
public class DataPersistence
{
    public void SaveRace(RaceState race, string filename)
    {
        // Serialize to binary
        var buffer = SerializeRace(race);
        
        // Compress
        var compressed = LZ4Codec.Encode(buffer);
        
        // Write to disk (async)
        File.WriteAllBytesAsync(filename, compressed);
        
        // Create metadata JSON
        var metadata = CreateMetadata(race);
        File.WriteAllText(filename + ".json", JsonConvert.SerializeObject(metadata));
    }
    
    public void LoadRace(string filename, out RaceState race)
    {
        // Read compressed data
        var compressed = File.ReadAllBytes(filename);
        
        // Decompress
        var buffer = LZ4Codec.Decode(compressed);
        
        // Deserialize
        race = DeserializeRace(buffer);
        
        // Validate checksum
        if (!ValidateChecksum(race))
            throw new Exception("Save file corrupted!");
    }
}
```

---

## 2.7 NEXT STEPS

**Remaining TDD Documents to Write**:
1. **Performance Profiling Strategy** (how to measure 24-car sim)
2. **Unity Project Structure** (folder layout, asset organization)
3. **API Reference** (method signatures for each module)
4. **Testing Strategy** (unit tests, integration tests, sim validation)
5. **Prototype Scope** (MVP for first playable)

---

**Document Version**: 1.0  
**Last Updated**: 2026-04-06  
**Status**: Draft - Awaiting Review
