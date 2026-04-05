# PART 6: RACE SIMULATION & MECHANICS

---

## 6.1 RACE SYSTEM OVERVIEW

**Definition**: The Race Simulation System is Cycle A (Race Weekend 10-30 minutes real-time per race). It represents the core gameplay loop where player makes strategic decisions during actual racing: pit timing, tire management, fuel conservation, weather adaptation, and driver instruction adjustments in real-time.

### Race Structure

```
PRE-RACE (2 hours before race):
  - Final setup adjustments (suspension, aero, engine mode)
  - Driver briefing (weather forecast, strategy outline)
  - Pit crew preparation (tire selection, tire selection)
  - Team radio checks

RACE START (Time = 0:00):
  - Parade lap
  - Formation lap
  - Start signal
  - Race timer begins

RACE DURATION:
  - Race distance: 305 km (typically 50-58 laps depending on circuit)
  - Real-world duration: 90-120 minutes
  - Game-time duration: 15-25 minutes (with acceleration options: 1x-20x speed)
  - Player can pause at any time (except during critical moments)

POST-RACE (30 minutes after finish):
  - Cool-down lap
  - Weighing (if finished top-3)
  - Champagne ceremony (if podium)
  - Team radio celebration/debriefing
  - Race result finalized

RACE INTERFACE:
  - Live standings (position, gap to leader, gap to next position)
  - Tire and fuel status (remaining laps)
  - Pit stop timer
  - Weather radar
  - Telemetry panel (lap times, sectors)
  - Team radio (two-way communication with drivers and pit crew)
  - Strategy suggestions (AI-driven recommendations)
```

---

## 6.2 TIRE MANAGEMENT SYSTEM

### 6.2.1 Tire Compounds & Degradation

**Six Tire Compounds Available (C1-C6)** (three selected per race by Pirelli):

| Compound | Grip Level | Base Durability | Track Impact | Notes |
|----------|-----------|----------------|--------------|-------|
| **C6 (Soft Red)** | +0.30 sec/lap | 10-18 laps | High temp: +20% life, low temp: -30% life | Ultra-soft, rare |
| **C5/Soft** | +0.25 sec/lap | 12-25 laps | Track-dependent (see below) | Standard soft |
| **C4/Medium** | 0.0 sec/lap | 18-40 laps | Baseline reference | Standard medium |
| **C3/Hard** | -0.15 sec/lap | 30-50 laps | Baseline reference | Standard hard |
| **C2/Super Hard** | -0.20 sec/lap | 40-60 laps | Rare, very cold climates | Uncommon |
| **C1/Ultra Hard** | -0.25 sec/lap | 50-70 laps | Extremely rare | Almost never used |

**Track-Specific Tire Life Examples**:
```
Monaco (high degradation): C5: 10-15 laps, C4: 20-25 laps, C3: 35-40 laps
Monza (low degradation): C5: 25-30 laps, C4: 35-42 laps, C3: 55-65 laps
Temperature effects:
  - 15°C or below: All compounds -20% to -30% life (harder to heat)
  - 35°C or above: All compounds +15% to +25% life (easier to heat, faster degrade)
```

**Temperature Optimal Windows** (each compound):
```
C5 Soft: Optimal 18-28°C, works 15-30°C, above 30°C overheat quickly, cold penalty 1.0+ sec
C4 Medium: Optimal 20-32°C, works 10-38°C, more forgiving
C3 Hard: Optimal 25-35°C, works 18-40°C, takes 2-3 laps to warm in cool conditions
```

**Mandatory Requirement**: At least TWO DIFFERENT compounds must be used in dry race
**Exception**: Wet/intermediate tires ONLY races don't require different compounds (Safety Car rule applies)
**Minimum requirement exception**: In rain-only races, mandatory compound rule suspended

### 6.2.2 Track Temperature Effects Model

**Track Temperature Impact on Tire Performance**:

```
Track Temperature Range & Tire Effects:

COLD TRACK (<15°C):
  - Tire warm-up: Requires 3-4 laps (not 1-2)
  - Grip level: -15% (all compounds)
  - Degradation: SLOWER (tires last longer in cold)
  - Optimal window: Hard to reach, takes time
  - Example: Monaco in winter (15°C)
    * C5 Soft needs 3 laps to reach optimal grip
    * Performance Lap 1: -1.5 sec vs optimal
    * Performance Lap 2: -0.8 sec vs optimal
    * Performance Lap 3: -0.4 sec vs optimal
    * Performance Lap 4+: Optimal grip reached

COOL TRACK (15-22°C):
  - Tire warm-up: Requires 2 laps
  - Grip level: -8% (medium penalty)
  - Degradation: Normal
  - Example: Silverstone Spring (18°C)
    * C5 Soft needs 2 laps to warm
    * C4 Medium needs 2-3 laps

OPTIMAL TRACK (22-28°C):
  - Tire warm-up: Requires 1 lap
  - Grip level: Baseline (0%)
  - Degradation: Normal
  - Tire life: At design spec
  - Example: Most summer races (24-26°C)
    * Perfect conditions for tire strategy

WARM TRACK (28-32°C):
  - Tire warm-up: Immediate or Lap 1
  - Grip level: +3% (slight improvement)
  - Degradation: FASTER (+8-10% per lap)
  - Tire life: Reduced (-2-3 laps vs baseline)
  - Strategy impact: May need extra pit stop
  - Example: Bahrain Spring (31°C)
    * C5 Soft lasts 22-24 laps instead of 25

HOT TRACK (>32°C):
  - Tire warm-up: Immediate (Lap 1 same pace)
  - Grip level: +5% (better grip)
  - Degradation: VERY FAST (+12-15% per lap)
  - Tire life: Significantly reduced (-4-6 laps)
  - Strategy impact: Likely need 3 stops or harder compounds
  - Example: Abu Dhabi (35°C+)
    * C5 Soft lasts 18-20 laps instead of 25
    * Force harder compound strategy

EXTREME HEAT (>37°C):
  - Degradation: Extreme (+18-20% per lap)
  - Tire blistering risk: 2-5% chance per lap
  - Blister penalty: -0.3 to -1.0 sec/lap
  - Strategy: Must use harder compounds (C4, C3)
  - Example: Abu Dhabi in peak summer (38°C)
    * Softer compounds not viable for long stints
```

**Track Temperature Interaction with Weather**:

```
Rain reduces track temperature significantly:
  - Heavy rain: Track drops 8-12°C instantly
  - Light rain: Track drops 5-8°C
  - Drying track: Temperature rises 2-3°C per lap as sun returns
  
Example: Was 30°C dry, heavy rain arrives
  - Track temp: 30°C → 20°C (instantly)
  - Tire warm-up: Now requires 2-3 laps again
  - Intermediate tires: Much better in cold, less aggressive degradation
  - Shift in strategy: Harder compounds become viable
  
This adds strategic layer: manage tires through temperature transitions
```

### 6.2.3 Tire Degradation Model

**Degradation Formula (Realistic Three-Phase Model):**
```
PHASE 1 - PLATEAU (Laps 1-10/15): Stable Performance
  - Fresh tires perform consistently
  - Lap times: ±0.05 sec variation (setup/fuel load only)
  - Grip level: 100% of nominal
  - Strategy: Maximize pace, build advantage on fresher tires

PHASE 2 - LINEAR DEGRADATION (Laps 11-22): Predictable Loss
  - Tire wear begins affecting grip progressively
  - Lap time loss: +0.05 to +0.10 sec per lap (depending on setup, fuel, track)
  - Example: Lap 15 = +0.25-0.50 sec vs Lap 10
  - Grip level: Decreases from 95% → 75%
  - Strategy: Monitor lap times, prepare pit strategy

PHASE 3 - CLIFF (Final 2-3 Laps): Sudden Performance Drop
  - Tires reach end-of-life threshold
  - Lap time loss: +0.30 to +0.80 sec additional penalty
  - Grip level: Drops below 70% (unstable)
  - Example: Lap 24 (if lifespan is 24 laps) suddenly +1.2 sec slower
  - Risk: High aquaplaning/spin probability if on wet track
  - Strategy: MUST pit before entering cliff phase

Example - Soft Tire Behavior on High-Degradation Track:
  Laps 1-8: 1:23.5 (stable plateau)
  Lap 9: 1:23.5 (still optimal)
  Lap 10: 1:23.6 (+0.1 linear wear begins)
  Lap 12: 1:23.8 (+0.3 cumulative)
  Lap 14: 1:24.1 (+0.6 cumulative)
  Lap 16: 1:24.5 (+1.0 cumulative)
  Lap 17: 1:24.8 (+1.3 cumulative, near end)
  Lap 18: 1:25.5 (+2.0 sec, CLIFF PHASE - must pit now)
  
  Optimal pit window: Laps 16-17 (before cliff)
  Critical pit deadline: Lap 18 (tires undriveable)

Example - Hard Tire Behavior on Low-Degradation Track:
  Laps 1-15: 1:25.0 (extended stable plateau)
  Lap 16: 1:25.1 (+0.1 wear begins)
  Lap 22: 1:25.6 (+0.6 cumulative linear wear)
  Lap 25: 1:26.1 (+1.1 cumulative)
  Lap 26: 1:27.0 (+2.0 sec cliff - end of life)
  
  Tire window: 25-26 laps usable, cliff at lap 27
```

**Track-Specific Degradation Profiles:**
```
HIGH DEGRADATION TRACKS (Monaco, Singapore, Mexico City):
  - C5 Soft: 8 laps plateau, then +0.08-0.10 sec/lap, cliff at lap 15-18
  - C4 Medium: 12 laps plateau, then +0.06-0.08 sec/lap, cliff at lap 22-25
  - C3 Hard: 18 laps plateau, then +0.05-0.07 sec/lap, cliff at lap 35-40
  - Strategy: Frequent pit stops (2-3 mandatory, sometimes 4)

MEDIUM DEGRADATION TRACKS (Silverstone, Spa, Suzuka):
  - C5 Soft: 10 laps plateau, then +0.05-0.07 sec/lap, cliff at lap 20-23
  - C4 Medium: 15 laps plateau, then +0.04-0.06 sec/lap, cliff at lap 30-35
  - C3 Hard: 22 laps plateau, then +0.03-0.05 sec/lap, cliff at lap 45-50
  - Strategy: Standard 2-3 pit stops

LOW DEGRADATION TRACKS (Monza, Montreal, Abu Dhabi):
  - C5 Soft: 15 laps plateau, then +0.03-0.05 sec/lap, cliff at lap 28-32
  - C4 Medium: 20 laps plateau, then +0.03-0.04 sec/lap, cliff at lap 38-42
  - C3 Hard: 28 laps plateau, then +0.02-0.03 sec/lap, cliff at lap 55-65
  - Strategy: Potential for 1-stop or 2-stop strategies

Weather Impact on Degradation:
  - Wet conditions: All compounds degrade +25% faster (shorter plateau, steeper linear phase)
  - Hot conditions (35°C+): Degradation +15% faster (cliff phase arrives earlier)
  - Cold conditions (<10°C): Degradation -20% slower (extended plateau and lifespan)
```

### 6.2.3 Pit Stop Mechanics

**Pit Stop Execution:**

```
Player calls pit stop via radio: "Box this lap"

Pit Stop Timeline (Real-Time):
  Lap N-1 (preparation):
    - Pit crew stands ready at garage
    - Tire selection confirmed (Soft/Medium/Hard)
    - Fuel pump set to desired amount
  
  Lap N (execution):
    - Driver enters pit lane
    - Regulation speed limit: 80 km/h (violation = penalty)
    - Car enters pit box
    - Pit crew action (REAL-TIME, can see as video)
    
    Pit Stop Sequence (simultaneous):
      Front-left tire change: 2.0 sec
      Front-right tire change: 2.0 sec
      Rear-left tire change: 2.0 sec
      Rear-right tire change: 2.0 sec
      Fuel added: 0.5-2.0 sec depending on amount
      Front wing adjustment (if needed): 1.0 sec
      
    Total Pit Stop Time: 1.9-2.5 seconds (realistic modern range)
    
    Pit Stop Variance (Team Morale Based):
      - Perfect execution (morale 95%+): 1.82-1.95 sec (world record territory)
      - Excellent execution (morale 85-94%): 1.95-2.1 sec (top teams typical)
      - Normal execution (morale 70-84%): 2.2-2.5 sec (mid-field baseline)
      - Poor execution (morale <70%): 2.8-4.0 sec (crew errors, fumbles)
      - Critical error (0.5% probability): 4.0-6.0 sec (unsafe release, dropped tire, safety violation)
    
    Reference: Red Bull record 1.82 sec, top teams average 2.0-2.4 sec
  
  Lap N+1:
    - Car exits pit box
    - New tire compound installed and warming up
    - Performance loss during warm-up phase (COMPOUND-DEPENDENT):
      - Soft tires (fresh): -1.0 sec/lap Lap 1 ONLY (warm fastest, ~2-3 full laps to peak grip)
      - Medium tires (fresh): -1.5 sec/lap Lap 1 (warm slower, ~3-4 laps to peak)
      - Hard tires (fresh): -2.0 sec/lap Lap 1, -1.0 sec/lap Lap 2 (warm very slowly, ~4-5 laps needed)
    
    Temperature Effects on Warm-up:
      - Hot asphalt (>30°C): -20% warm-up time (faster to peak grip)
      - Cold asphalt (<15°C): +30% warm-up time (slower to reach optimal window)
    
    After tire warm-up (Lap 2-5 depending on compound): Full grip available
```

**Pit Mechanics Terminology:**

```
Two-Car Stack (Double Stack):
  Definition: Two cars from the same team pit consecutively in the same lap
  Mechanics: 
    - Car A enters pit box, executes pit stop (2.0-2.5 sec)
    - Car B follows immediately behind, waits for Car A to exit
    - Car B then enters pit box for their stop
  
  Time Loss for Second Car:
    - Car A total time: 2.0-2.5 sec (standard)
    - Car B delay: Additional 3-4 sec wait for pit box to clear
    - Car B total pit cycle: 5-7 sec (compared to 2.0-2.5 if pitted separately)
    - Relative loss: 2-4 seconds vs staggered pit timing
  
  Strategic Use:
    - Allowed by FIA regulations (no penalty)
    - Used when both cars need to pit urgently (tire fail, damage)
    - Used when defending team position (both cars pit under safety car)
    - Economical decision if leader not pitting (preserve relative positions)
  
  Risk: High time cost for second car; use only when necessary

Two-Stop Strategy:
  Definition: Standard race strategy using exactly two pit stops
  Mechanics: Pit stop Lap X, then pit stop Lap Y (two total interventions)
  Example: Soft 12 laps → Medium 30 laps → Hard 16 laps (pit stops at Lap 12 and Lap 42)
  Advantage: Balances pit stop time loss (5+ seconds total) vs tire degradation management
```

**Pit Stop Timing Decision:**

```
Player must decide: When to pit?

Scenario: Driver on Soft tires, lap 5 of 58

Analysis:
  - Current tire life: 5 laps used, 13-15 laps remaining
  - Current position: 2nd place, 0.8 sec gap to leader
  - Fuel consumption: 1.6 kg/lap, currently at 85 kg remaining
  - Leader pit stop: Unknown (not yet pitted)
  - Weather: Clear, stable forecast
  
Pit Stop Options:

Option A: Pit THIS lap (Lap 5 end)
  Pros:
    - Fresh tire advantage for 20 laps
    - Can potentially catch leader with fresh tires
    - Tire still has grip, not overly degraded
  Cons:
    - If leader doesn't pit yet, lose position immediately (go to 2nd/3rd)
    - Fresh tire warm-up lap (Lap 6) slower
    - May lose 0.5-1.0 sec on this pit stop cycle
  
  Outcome: Gamble on undercut (pit early, hope undercut works)

Option B: Pit Next lap (Lap 6 end)
  Pros:
    - Wait to see what leader does
    - Current tires still functional
    - Play reactive strategy
  Cons:
    - Tires degrading rapidly (Lap 6 already slow)
    - If leader pits now too, pit stop order same
    - Risk: Tires might be too worn by Lap 7, forcing earlier-than-planned pit
  
  Outcome: Balanced, responsive approach

Option C: Pit in 3 laps (Lap 8 end)
  Pros:
    - Squeeze more life from soft tires (but risky)
    - See where competitors pit before committing
    - Potentially get fresher tires near end of tire life
  Cons:
    - Laps 6-8 on aging tires = -0.5 to -1.5 sec/lap loss
    - May fall back to 4th place during this period
    - If you fall back, harder to recover
  
  Outcome: Aggressive strategy, requires tires to hold

Recommendation from Telemetry Team:
  "Box now (Lap 5) or next lap (Lap 6) max. Leader likely pitting Lap 7-8. 
   Soft tire degradation cliff incoming. Fresh mediums will be 1-2 seconds faster 
   by Lap 10."

Decision: Pit Lap 6 (balanced between aggressive and defensive)
```

**Undercut vs Overcut Strategy:**

```
UNDERCUT STRATEGY (Pit Stop 1-2 Laps Earlier Than Opponent):
  Definition: Complete your pit stop before your rival, gaining time advantage

  How It Works:
    Scenario: You're in 2nd place, 1.2 sec behind leader on Lap 18
    - You pit Lap 20: Exit pit lane, now on fresh soft tires
    - Leader pits Lap 22: Still on old medium tires for 2 more laps
    
    Lap 21 (You on fresh tires vs Leader on old):
      Your pace: 1:23.0 (fresh soft tires, perfect grip)
      Leader pace: 1:24.2 (degraded medium tires, losing grip)
      Gain per lap: -1.2 seconds
    
    Lap 22 (Both pit):
      You pit to 2nd set of tires
      Leader pits, now everyone on similar tire age
      Net result: You gained ~2 seconds during your free advantage laps
  
  Undercut Effectiveness Formula:
    Gain = (Fresh tire speed advantage) × (Laps driven on fresh before opponent pits)
    Example: Fresh tires +0.5 sec/lap × 2 laps = +1.0 sec advantage
  
  When Undercut Works Best:
    - High tire degradation track (tires lose 0.1 sec/lap in cliff phase)
    - When leader is on old tires nearing pit window
    - When opponent won't expect early pit stop
    - When new tires warm up quickly (soft compound, hot track)
  
  When Undercut Fails:
    - Opponent is already on fresh tires (no deficit to overcome)
    - Pit stop loss (2.5 sec) > lap-time gain from fresh tires (1.0-1.5 sec)
    - Opponent defends aggressively, forces position loss
    - You fall back further behind despite fresh tires

OVERCUT STRATEGY (Pit Stop 1-2 Laps Later Than Opponent):
  Definition: Stay out while rival pits, then pit later to gain advantage from longer tire life window

  How It Works:
    Scenario: You're in 2nd place, 0.8 sec behind leader on Lap 25
    - Leader pits Lap 28: Goes to fresh medium tires
    - You pit Lap 30: Stay out 2 extra laps on current tires
    
    Lap 29-30 (Leader on fresh tires vs You on older):
      Leader pace on fresh mediums: 1:23.5
      Your pace on degraded softs: 1:23.8
      Relative loss: -0.3 sec/lap for 2 laps = -0.6 sec
    
    Lap 31+ (You on fresh tires vs Leader on slightly older mediums):
      Your pace on fresh mediums: 1:23.2 (fresh tire advantage)
      Leader pace on older mediums: 1:23.6 (2 laps of degradation)
      Gain per lap: -0.4 seconds
      Over next 5 laps: -2.0 sec advantage builds
  
  Overcut Effectiveness Formula:
    Loss period: (Laps you stayed out) × (pace deficit per lap vs opponent's fresh tires)
    Gain period: (Fresh tire advantage) × (Opponent tire degradation advantage)
    Net gain = Gain period - Loss period
    
    Example: Stay out 2 laps, lose 0.3/lap = -0.6 sec loss
             Then gain 0.4/lap for 5 laps = +2.0 sec gain
             Net: +1.4 sec advantage after 7 total laps
  
  When Overcut Works Best:
    - Tires still have strong performance window (not in cliff phase)
    - Opponent will quickly degrade on fresh tires due to track conditions
    - Following car on fresh tires has better pace (you can attack)
    - Leader can't extend tire life significantly beyond normal strategy
  
  When Overcut Fails:
    - You pit while tires are in cliff phase (lose too much time waiting)
    - Opponent's fresh tires still dominate your pace
    - Safety car/VSC bunches field, negating your advantage

TACTICAL SUMMARY:
  Undercut: Faster reaction, guaranteed time gain during execution, higher pit stop risk
  Overcut: Requires discipline, needs opponent to have worse tire degradation, higher execution risk
  
  Choice depends on: Track tire degradation profile, current tire age, fuel strategy, opponent pit timing prediction
```

---

## 6.3 FUEL MANAGEMENT

### 6.3.1 Fuel Consumption Model

**Consumption Rate & Flow Limit:**

```
FIA RULE: Maximum fuel flow 100 kg/hour at ANY moment (hard limit)

Base Consumption: 1.6 kg/lap (average, varies by track)

Track-Specific Base Consumption:
  - High downforce, slow speed (Monaco, Singapore): 1.7-1.8 kg/lap
  - Low downforce, high speed (Monza, Hungary): 1.3-1.4 kg/lap
  - Average (most tracks): 1.5-1.6 kg/lap

Consumption Modifiers (relative to base):
  + Economy mode: -10% consumption (reduces power ~2-3%)
  + Standard mode: Base consumption
  + Power/Attack mode: +5% consumption (approaches 100 kg/h limit)
  + Aggressive pushing (near power limit): Risk fuel cut-off if exceed 100 kg/h flow

Temperature Impact:
  - Hot conditions (>35°C): +8% consumption (more energy lost to heat)
  - Cold conditions (<10°C): -12% consumption (less energy loss)

Tire Compound Impact:
  - Soft compound: +0.05 kg/lap (more rolling resistance)
  - Medium compound: Baseline
  - Hard compound: -0.05 kg/lap (less rolling resistance)

Fuel Flow Risk:
  - Exceeding 100 kg/h flow = Engine management cuts power temporarily
  - Player must manage throttle/modes to stay within limit during aggressive driving
```

### 6.3.2 Fuel Strategy Decision

**Starting Fuel Load Decision (Pre-Race):**

```
Race distance: 58 laps × 1.6 kg/lap = 92.8 kg fuel needed minimum
Safety margin: +5 kg = 97.8 kg recommended
Maximum allowed: 110 kg

Load Options:

Option A: Minimum Load (97-100 kg)
  Pros:
    - Lighter car (3 kg lighter = +0.03-0.05 sec/lap average gain)
    - Requires precise pit stop scheduling
  Cons:
    - Zero margin for error (safety car, VSC, must pit exactly on time)
    - Cannot adjust fuel strategy mid-race
    - Risk: Running out of fuel before finish (DNF)
  
  Result: Risky, suitable for dry weather, precise one-stop strategy only

Option B: Standard Load (105-107 kg)
  Pros:
    - Adequate safety margin (5-10 km buffer)
    - Flexibility for 1-2 stops
    - Can adjust pit stops reactively
  Cons:
    - Slightly heavier car (-0.02 to -0.03 sec/lap penalty)
    - Standard fuel consumption
  
  Result: Recommended, safe, balanced, reduces tire wear slightly due to weight

Option C: Maximum Load (110 kg, regulatory limit)
  Pros:
    - Maximum flexibility (can pit late or skip second stop)
    - Extra fuel for safety car scenarios
  Cons:
    - Heavy car (-0.05 to -0.08 sec/lap penalty, 10kg loss = ~0.05 sec/lap)
    - Wears tires faster under weight (+5-10% additional tire wear)
    - Fuel management critical to avoid exceeding 100kg/h flow limit
  
  Result: Defensive, suitable for uncertain weather or variable strategy, some competitive cost

Weight Penalty Formula:
  Each additional kg of fuel ≈ 0.005 sec/lap
  Therefore: 10 kg more fuel = ~0.05 sec/lap loss
  Economy mode (power reduced 2-3%) = additional -0.08 to -0.12 sec/lap

Decision: Load 105 kg
  - Provides 5-10 km safety margin
  - Supports 2-stop strategy: ~50 kg first stint, ~30 kg second, ~25 kg final
  - Can be adjusted if safety car scenario occurs
```

### 6.3.3 Mid-Race Fuel Management

**Fuel Consumption Monitoring:**

```
Live Display Shows: "Fuel Remaining: 42.5 kg, Consumption: 1.6 kg/lap"

Calculation: 42.5 kg ÷ 1.6 kg/lap = 26.5 laps remaining fuel

Pit Stop Planning:
  - Current lap: 25 (of 58)
  - Laps until finish: 33
  - Fuel remaining: 42.5 kg
  - Needed: 33 × 1.6 = 52.8 kg
  - Shortfall: 52.8 - 42.5 = 10.3 kg MUST pit

Decision: Pit MUST happen within 10 laps (can't go further)

Options:
  A) Pit in 5 laps (Lap 30): Get 25 kg fuel, run final 28 laps
     - 25 kg remaining would need: 28 × 1.6 = 44.8 kg
     - After pit: 42.5 - (5 × 1.6) + 25 = 45.5 kg (sufficient)
     - Laps until pit: 5 remaining on current fuel
  
  B) Pit in 8 laps (Lap 33): Get 20 kg fuel, run final 25 laps
     - 20 kg remaining would need: 25 × 1.6 = 40 kg
     - After pit: 42.5 - (8 × 1.6) + 20 = 47.7 kg (sufficient)
     - Laps until pit: 8 remaining

  C) Enter fuel-save mode NOW (Lap 25)
     - Reduce consumption to 1.4 kg/lap (-12.5%)
     - Remaining 42.5 kg would last: 42.5 ÷ 1.4 = 30.4 laps
     - Finish without another pit stop possible!
     - Cost: -0.1 sec/lap from fuel-saving mode

Driver Radio: "We can make it to the end on fuel if we go lean now. Cost is 
0.1 seconds per lap performance. Or we pit and maintain pace."

Decision: Go fuel-save mode (only 0.1 sec/lap penalty, saves pit stop and guarantees finish)
```

---

## 6.4 WEATHER SYSTEM

### 6.4.1 Weather Conditions

**Three Weather States:**

```
DRY CONDITIONS:
  - Track temp: 20-35°C
  - Air temp: 15-25°C
  - Visibility: Full
  - Tire performance: Optimal
  - Performance: Baseline
  - Wet tire prohibition: Cannot use wet tires (violation = penalty)

LIGHT RAIN:
  - Track temp: 10-20°C, wet surface
  - Air temp: 10-18°C
  - Visibility: Reduced (80% of dry)
  - Tire performance: Intermediate tires required
  - Performance: All lap times -0.5 to -1.0 sec/lap slower
  - Wet tire risks:
    * Water depth <3mm: Intermediate tires safe (optimal choice)
    * Water depth 3-5mm: Intermediate becoming risky, slicks still bad
    * Water depth >5mm: Risk of aquaplaning on slicks (1-2% per lap; drivers will self-pit)
  - Driver instruction change: "Be careful, wet sections appearing"
  - Pit stop decision: Switch from dry to intermediate tires

HEAVY RAIN:
  - Track temp: 5-15°C, heavy water layer (5-10mm+ standing water)
  - Air temp: 5-12°C
  - Visibility: 50% of dry (dangerous, simulation can be suspended)
  - Tire performance: Wet tires required for safety
  - Performance: All lap times -1.5 to -2.5 sec/lap slower
  - Aquaplaning risk mechanics:
    * Water depth 5-8mm: Aquaplaning on intermediates minimal (0.1-0.3% per lap)
    * Water depth 8-12mm: Aquaplaning on intermediates low risk (0.2-0.5% per lap on int, 1-2% on slicks)
    * Water depth >12mm: Aquaplaning on intermediates moderate (0.3-0.5% per lap)
    * Wet tires: Designed for water dispersion, <0.1% aquaplaning risk
  - Safety car likely deployed
  - Driver instruction: "Safety first, no risks"
  - Pit stop decision: Switch to wet tires or pause/pit early
```

**Aquaplaning Mechanics** (Wet weather hazard):

```
Definition: Loss of tire grip due to water layer between tire and track

Aquaplaning Probability by Combination:
  Intermediate tires + Light rain (3mm water):  0.1-0.3% per lap
  Intermediate tires + Moderate rain (5mm):     0.2-0.5% per lap
  Intermediate tires + Heavy rain (8mm+):       0.3-0.5% per lap
  
  Slick tires + Any rain (3mm+):                1-2% per lap (light rain)
                                                2-3% per lap (moderate/heavy)
                                                (drivers will self-pit)
  
  Wet tires + Any rain:                         <0.1% per lap (designed for this)

Aquaplaning Consequences:
  Hydroplane Event (rare but happens):
    - Sudden loss of grip for 1 corner
    - Driver loses 0.5-1.0 sec
    - Risk: 20% chance of light contact/damage (-0.1 sec/lap)
    - Risk: 5% chance of serious crash (barrier hit, DNF probability)
    - Recovery: Driver regains control after 1-2 corners
  
  Example scenario:
    Lap 24, Eau Rouge (Spa), Heavy rain, Intermediate tires, 7mm water:
    - Aquaplaning check: 3-5% chance this lap
    - Random roll: 67 (out of 100) → No aquaplaning this lap
    - Lap 25: Another aquaplaning check
    - Random roll: 4 (out of 100) → AQUAPLANING EVENT!
    - Impact: Loses grip in Turn 1, recovers Eau Rouge turn, +0.8 sec
    - Minor damage risk: 20% chance → Failed (no damage)
    - Result: Lap 25 time +0.8 sec slower, continue racing
```

### 6.4.2 Weather Forecasting System

**Weather Forecast Accuracy** (90-95% realistic):

```
Real-world baseline: Modern F1 teams use satellite/radar with 85-95% accuracy
Game mechanic: Forecast accuracy by lead time

Forecast Confidence by Range:
  0-2 laps ahead:    95% accuracy (almost certain)
  2-5 laps ahead:    92% accuracy (very reliable)
  5-10 laps ahead:   88% accuracy (good estimate)
  10+ laps ahead:    82% accuracy (rough estimate)
  
  Example at Lap 10:
    "Rain Lap 12-13: 95% confidence ±0 laps" (almost guaranteed)
    "Rain Lap 15-17: 88% confidence ±1 lap" (likely but could shift)
    "Rain Lap 20+: 82% confidence ±2 laps" (estimate only)
```

**Local Rain Surprise Mechanics** (Rare but possible):

```
Occasional random weather events catch teams unprepared:

Surprise Rain (5% of races):
  - Forecast said 88% dry, but local cell moves quickly
  - Driver gets caught on dry tires in sudden downpour
  - Scenario: Lap 22 (sunny), radar said rain Lap 28-30
    → Lap 24: Small cloud appears, light rain Lap 25 (3 laps EARLY)
    → Decision: Pit or risk 3 laps on degrading dry tires?
    → Cost of miscalculation: 0.5-1.0 sec/lap performance loss
  
  Meteorological factor:
    - Local thunderstorms can develop in 10-15 min window
    - Model simulates these as rare deviations from forecast
    - Players who follow forecast strictly can get caught

Rain Window Extension (Rare):
  - Forecast: Rain Lap 15-20 (high confidence)
  - Actual: Rain continues Lap 15-35 (longer than forecast)
  - Strategic impact: Early pit stop strategy now insufficient
  - Must extend fuel/tire strategy mid-race

Surprise Dry Spell (Rare):
  - Forecast: Heavy rain all race
  - Actual: Rain clears by Lap 30, track dries
  - Advantage: Those who pit late for slicks gain advantage
  - Disadvantage: Those who pitted early for wet tires are disadvantaged
```

**Rain Radar Display** (shown in race UI):

```
Race Lap 10 (of 58):
  - Current weather: Dry (Track temp 26°C)
  - Forecast (next 10 minutes / next 5 laps): Light rain approaching
  - Confidence: 92% (very reliable)
  - ETA: Lap 13-14 (±0 laps)
  - Intensity: Light → Moderate progression
  - Duration estimate: 8-12 laps of wet conditions

Strategic Question:
  - Pre-pit for intermediate tires now? (Preventive)
  - Wait and see? (Reactive)
  - Risk: With 92% confidence, very safe to pit

Scenario A: Pre-pit at Lap 11 (RECOMMENDED)
  - Change to intermediate tires (soft compounds, only +0.5 sec/lap penalty)
  - Probability rain arrives: 92% (very likely)
  - If rain doesn't arrive (8% chance): Wasted pit stop, lost time
  - If rain arrives Lap 13: Already on correct tires, perfect!
  - Decision: Mathematically sound (92% > 8% risk)

Scenario B: Stay dry, pit only if rain comes (RISKY)
  - Risk: If rain arrives Lap 13, 3 laps on dry tires on wet track
    - Lap 13: -0.8 sec/lap (dry tires don't grip well)
    - Lap 14: -0.9 sec/lap (aquaplaning risk!)
    - Lap 15: -1.0 sec/lap (spinning high probability)
  - Pit at Lap 15, switch to intermediate
  - Cost: Lost 3 laps on slow tires, risk of spin/crash
  - Decision: 8% chance to save pit stop vs 92% chance to lose time

Recommendation from Telemetry:
  "Rain confidence 92%, very reliable. Pit Lap 11 to pre-empt. Cost: 0.5 
   sec/lap for 1 lap, but gain 5+ seconds when rain arrives."

Decision: Pit Lap 11 for intermediate tires (optimal strategy)
```

### 6.4.3 Safety Car & VSC Dynamics

**Safety Car** (Full Course Yellow):

Triggered by: Crash, debris, severe weather

Effect:
  - All cars line up behind safety car, single-file
  - Speed: 60-80 km/h (controlled pace, prevents relative time loss)
  - DRS disabled: Cannot overtake
  - Pit stops still available and STRATEGICALLY IMPORTANT
  - Duration: 3-8 laps typically
  
**Safety Car Pit Stop Mechanics** (Critical strategy moment):

```
Key advantage: NO TIME LOSS during pit stop
  - Normal pit stop: Lose 15-30 seconds vs cars not pitting
  - Safety car pit stop: NO RELATIVE TIME LOSS (everyone crawling at 60 km/h)
  - Only lose pit stop duration vs staying out
  
Strategic Decisions During Safety Car:

Decision Point: Should we pit under safety car?
  Original plan: Pit stop at Lap 30
  Current: Safety car deployed Lap 22
  
  PIT UNDER SAFETY CAR (LAP 23):
    Advantages:
      ✓ Get required pit stop done with zero relative loss
      ✓ Fresh tires for race restart
      ✓ Reset pit stop debt (no longer owe pit stop later)
      ✓ If leader also pits, relative positions maintained
      ✓ Tire life: ~25 laps left after restart (optimal)
    
    Disadvantages:
      ✗ Lose 2.2-2.5 sec to stop itself (pit time)
      ✗ Risk: If others don't pit, fall back in order
      ✗ Front-of-pack advantage: Might pit from P5 → emerge P7 temporarily
    
    Outcome example:
      Before pit: P5, 1.5s behind P4
      Pit timing: Enter pit from P5, exit pit from P7
      Damage: Lost 2.5s to pit execution, now 4.0s behind P4
      BUT: Race restarts in 3 laps, competitors still in old-tire sequence
      Resume racing: Fresh tires regain positions quickly
  
  DON'T PIT UNDER SAFETY CAR (LAP 25):
    Advantages:
      ✓ Keep current position order (no pit stop gap)
      ✓ See where leaders pit first
      ✓ Reactive strategy (respond to others)
    
    Disadvantages:
      ✗ Still need pit stop later (debt remains)
      ✗ When you pit Lap 30, lose 20-30 sec to competitors already raced ahead
      ✗ Tire age: Will have 30+ laps on same tires by restart
      ✓ Fresh tires advantage: Lost
    
    Outcome example:
      Safety car ends Lap 25, racing resumes
      You still in P5 with old tires (28 laps old)
      Competitors: Some fresh, some old tires mixed
      Lap 30: Finally pit, lose 25 sec to P4 who pit earlier
      Result: Drop to P6 or lower

Recommendation Framework:
  IF pit debt < 3 laps → PIT under safety car (reset debt cheaply)
  IF pit debt > 5 laps → WAIT for safety car to end (pit when everyone stops together)
  IF pit debt 3-5 laps → DEPENDS on positions (aggressive teams pit, conservative wait)
```

Strategic Opportunity:
```
Scenario: You're in 3rd place, Safety Car deployed Lap 20

Your fuel situation:
  - Originally planned Lap 30 pit stop
  - Current fuel: Enough for 15 more laps (Lap 35 limit)
  - Safety car neutralizes time loss (gap to leader maintained)

Pit Stop Decision During Safety Car:
  Option A: Pit under safety car (Lap 21)
    Pros:
      - Get pit stop out of the way while losing no relative time
      - Leaders also pitting, field re-stacks
      - Tire fresh for racing restart
    Cons:
      - Lose grid position order (might go to 5th temporarily)
      - Other drivers might not pit (undercut/overcut risk)
      
  Option B: Don't pit, follow leaders
    Pros:
      - Maintain 3rd place during yellow
      - See where leaders pit first
      - Reactive strategy
    Cons:
      - Still have pit stop debt ahead
      - Leaders may gain advantage by pitting early
      - Miss the "free" pit stop opportunity

Decision: Pit under safety car (Lap 21)
  Result: Both you and leader pit, relative positions same
  Outcome: Safety car ends Lap 24, racing resumes with you in similar position
  Advantage gained: Pit stop "paid for" by safety car (didn't lose time)
```

**Virtual Safety Car (VSC)** (Yellow flag, reduced speed zones):
  - Allows racing to continue
  - Slower than safety car, faster than green flag racing
  - Pit stops possible but timing critical
  - Less strategic impact than full safety car

---

## 6.5 DRIVER INSTRUCTIONS & TEAM RADIO

### 6.5.1 Driver Instruction Types

**Player can radio driver with instructions:**

```
INSTRUCTION: "Defend Against [Car Number]"
  Effect: Driver becomes defensive in upcoming corners
  Cost: -0.15 sec/lap (focusing on defense, not pace)
  Morale impact: +5% if successful defense
  Duration: 3 laps
  Use case: Protecting position from faster car behind

INSTRUCTION: "Push/Attack" (NO TIME LIMIT)
  Effect: Driver maximizes pace, takes risks, uses high fuel flow
  Benefits: +0.2-0.3 sec/lap potential (varies by driver skill)
  Costs:
    - Fuel flow approaches 100 kg/hour limit (risk of power cut-off)
    - Tire wear increases 10-15% per lap
    - Mental load accumulates with sustained pushing
  
  Mental Load Mechanics (Driver Concentration):
    - Laps 1-15: No penalty (driver fresh and focused)
    - Laps 16+: After 15 laps of consecutive attacking, error probability increases
      * Lap 16 of attack: +0.1% error probability
      * Lap 17 of attack: +0.2% cumulative error probability
      * Lap 18 of attack: +0.3% cumulative error probability
      * Each additional lap: +0.1% additional error probability
    - Error types: Missed braking point, off-line corner, slight lock-up
    - Error consequence: -0.05 to -0.15 sec/lap lost performance
    - Recovery: 3-5 laps of "Manage Fuel/Tires" instruction to reset mental load
  
  Morale impact: +10% if overtake successful, -10% if error/off-line
  DNF risk: +0.5% per lap of sustained attack (cumulative from rash mistakes)
  Use case: Attacking leader, attempting overtake, recovering from pit stop disadvantage
  Strategic note: Push hard early, then manage mid-race to avoid mental fatigue

INSTRUCTION: "Manage Fuel/Tires"
  Effect: Driver enters economy mode
  Cost: -0.1 to -0.2 sec/lap (deliberate slower driving)
  Morale impact: -5% (driver frustrated at pace reduction)
  Duration: Until told otherwise
  Use case: Stretching fuel to avoid pit stop, nursing tires

INSTRUCTION: "Caution - Wet Sections"
  Effect: Driver reduces speed through identified wet areas
  Cost: Variable (depends on how severe wet areas)
  Morale impact: +5% (driver feels supported)
  Duration: 1 lap (until situation assessed)
  Use case: Weather change adaptation

INSTRUCTION: "Box This Lap" (Pit Stop Call)
  Effect: Driver enters pit lane next opportunity
  Cost: Pit stop time (2.0-3.2 seconds)
  Morale impact: +5% if pit stop strategic (right decision), -10% if poor timing
  Duration: Immediate
  Use case: Tire change, fuel top-up, setup adjustment
```

### 6.5.2 Driver Response & Reliability

**Driver Performance Under Instruction:**

```
Driver Attribute: "Instruction Following" (part of skill)
  - Pace 85+ drivers: Follow instructions precisely, 100% execution
  - Pace 70-84 drivers: Follow instructions, 85-95% execution
  - Pace <70 drivers: May misunderstand or fight instructions, 70% execution

Example - "Attack" Instruction:

Driver A (Pace 92, follows instructions well):
  - Executes attack perfectly
  - Gains 0.1 sec/lap for 3 laps
  - Overtakes successfully
  - DNF risk: +1% (acceptable)

Driver B (Pace 75, struggles with instructions):
  - Attacks but inconsistently
  - Gains 0.05 sec/lap (half of potential)
  - Close battle but no overtake
  - DNF risk: +2% (higher due to misexecution)

Driver C (Pace 68, difficult driver):
  - Pushes too hard, crashes on turn 2 of attack attempt
  - Lap time: -0.5 sec/lap (off-line recovery)
  - DNF risk: +3% (aggressive error)
```

---

## 6.6 STRATEGIC DECISIONS DURING RACE

### 6.6.1 One-Stop vs Two-Stop Strategy

**Race Lap 1 (Pre-Race Decision):**

```
Race parameters:
  - Distance: 58 laps
  - Tire degradation: Medium (normal track)
  - Weather: Stable dry forecast
  - Starting fuel load: 105 kg

Strategy Analysis:

ONE-STOP STRATEGY:
  Plan: Soft 15 laps → Medium 43 laps (pit stop Lap 15)
  Pit stop size: 48 kg fuel at Lap 15
  
  Pros:
    - Fewer pit stops (only 1)
    - Pit stop "savings" = less time in pit lane
    - Simple strategy to execute
  
  Cons:
    - Medium tires for 43 laps = degradation high on laps 40-58
    - Late-race pace reduced
    - If safety car comes late, stuck on old tires
    - If second-placed car can do two stops, will undercut you
  
  Risk: HIGH (relies on tire holding, no flexibility)
  Best for: High downforce tracks where degradation is extreme early (Monaco-style)

TWO-STOP STRATEGY:
  Plan: Soft 12 laps → Medium 30 laps → Hard 16 laps (pit stops Lap 12 & 42)
  Pit stop 1 size: 55 kg fuel at Lap 12
  Pit stop 2 size: 30 kg fuel at Lap 42
  
  Pros:
    - Fresher tires throughout race
    - Better pace in late race
    - Flexibility if safety car comes
    - Can adjust second pit stop timing
  
  Cons:
    - Extra pit stop = 2.5 seconds time loss (double pit stops)
    - Requires precise timing and fuel management
    - More complex to execute
  
  Risk: MEDIUM (safer, more flexible)
  Best for: Low degradation tracks where tire life is long (Monza-style)

DOUBLE PIT-STOP STRATEGY (Variant of Two-Stop):
  Plan: Medium 20 laps → Hard 18 laps → Medium 20 laps (pit stops Lap 20 & 38)
  Alternative: Soft 8 laps → Hard 25 laps → Soft 25 laps (pit stops Lap 8 & 33)
  Pit stop 1 size: 40-50 kg fuel
  Pit stop 2 size: 35-40 kg fuel
  
  Concept: Two pit stops using similar/repeated compounds for psychological effect
  or tire management optimization (avoid extreme wear from one compound).
  
  Example 1: "Undercut Double-Stop"
    - Start: Soft (aggressive opening)
    - Lap 8: Box → Hard (save tires)
    - Lap 33: Box → Soft (fresh soft for final push)
    - Lap 58: Finish on fresh Soft with good grip
    - Advantage: Soft tires for start (pace) AND finish (overtaking)
    - Risk: Lap 8-33 on Hards might lose position if competitors stay out longer
  
  Example 2: "Hard-Medium Sequence"
    - Start: Medium (balanced)
    - Lap 20: Box → Hard (preserve for final laps)
    - Lap 38: Box → Medium (fresh rubber for late race)
    - Advantage: Medium tires faster than Hard, fresher legs at end
    - Risk: Medium degrades quickly after pit, may need aggressive pace early
  
  Pros:
    - Flexibility: Can pit twice without 3-stop penalty
    - Psychological: Fresh soft tires at finish (looks strong on camera)
    - Tire management: Optimize which compound gets which duration
    - Safety car friendly: Can adjust second stop timing if yellow appears
  
  Cons:
    - Complexity: Four tire changes total (vs two in standard two-stop)
    - Pit crew fatigue: More stops = higher mistake risk if morale low
    - Time loss: Same 2 pit stops = 5 seconds total pit loss
    - No advantage over standard two-stop (same number of stops)
  
  Risk: MEDIUM (same as two-stop, just different compound sequencing)
  Best for: Tracks where mixing compounds is strategic (start with soft, finish with soft)
            or mental edge (fresh tires on camera at finish)

THREE-STOP STRATEGY:
  Plan: Soft 10 laps → Medium 20 laps → Hard 20 laps → Soft 8 laps (pit stops Lap 10, 30, 50)
  Requires: Aggressive fuel management and pit timing
  
  Pros:
    - Freshest tires possible (optimal for extreme degradation circuits)
    - Maximum pace consistency
    - Can recover from safety car scenarios
    - Sometimes optimal (e.g., Singapore 2023 demonstrated 3-stop viability)
  
  Cons:
    - Extra pit stops = time loss (3 × 2.5 sec ≈ 7.5 seconds total)
    - Requires precise fuel and tire management
    - Only competitive on high-degradation tracks (street circuits)
  
  Risk: MEDIUM (viable on right track type with good execution)
  Best for: Extreme degradation tracks (Singapore, Monaco, Mexico) or if leader already committed to 3-stop

Team Radio Decision:
  Telemetry: "Based on fuel and tire modeling, recommend TWO-STOP. Soft 12, 
             Medium 30, Hard 16. Pit stops Lap 12 and 42. This keeps pace 
             consistent and provides flexibility if safety car comes."

Decision: Two-stop strategy (standard, balanced risk)
  Lap 1: Execute Soft tires, monitor degradation
  Lap 11-12: Prepare for first pit stop
  Lap 12: Box, change to Medium, load 55 kg fuel
  Lap 42-43: Prepare for second pit stop
  Lap 42: Box, change to Hard, load final 30 kg fuel
  Lap 43-58: Drive hard on fresh Hards to finish
```

### 6.6.2 MGU-K Energy Management System

**Energy Recovery & Deployment Strategy:**

```
MGU-K (Motor Generator Unit - Kinetic) System Overview:
  Purpose: Capture braking energy, convert to electrical energy for later use
  Battery capacity: ~4 MJ (Megajoules) per lap
  Recovery during braking: 0.8-1.0 MJ per braking event
  Deployment power boost: +0.2-0.5 sec/lap depending on recovery/usage

Driver Control Options (Tactical):
  - High Recovery Mode: Maximum energy recapture during braking
    * Captures: 1.0 MJ per lap (maximum)
    * Penalty: -0.1 sec/lap (increased engine braking, harder to modulate)
    * Benefit: More battery charge available for straights
    * Use case: Low-power tracks, conservation racing
  
  - Medium Recovery Mode (Default): Balanced approach
    * Captures: 0.8 MJ per lap
    * Penalty: None (normal braking feel)
    * Benefit: Standard power boost (+0.2-0.3 sec/lap if used)
    * Use case: Most tracks, strategic situations
  
  - Low Recovery Mode: Minimal energy capture
    * Captures: 0.5 MJ per lap (minimum safe)
    * Penalty: None (loose braking feel, harder to lock)
    * Benefit: Can recover energy from crashes/damage safely
    * Use case: Defensive racing, protecting battery when full

Deployment Strategy (Use of Stored Energy):
  High Deployment Mode: Use all stored energy aggressively
    * Power boost: +0.5 sec/lap on straights (Monza, Baku, Hungary)
    * Battery drain: 4 MJ = 1 lap of maximum boost
    * Tactical use: Overtaking on straights, defending into corners
    * Risk: Battery depleted, no power available for defense
  
  Medium Deployment Mode (Default): Use energy gradually
    * Power boost: +0.2-0.3 sec/lap continuously
    * Battery management: Lasts entire race, sustainable
    * Tactical use: Consistent pace improvement, always available
  
  Economy Deployment Mode: Minimal power usage
    * Power boost: +0.05 sec/lap (barely noticeable)
    * Battery conservation: Can accumulate full charge (8 MJ)
    * Tactical use: Stretching fuel, protecting engine, rain preparation
    * Benefit: Can unleash maximum power late race

Track-Specific Energy Management:
  High-speed tracks (Monza, Spa, Baku):
    - Long straights = more energy recovery opportunity
    - High power boost benefit on straights
    - Recommend: High deployment mode (1-2 lap overtaking windows)
  
  Medium-speed tracks (Silverstone, Hungary, Canada):
    - Balanced recovery and usage
    - Use medium mode for consistent pace
  
  Low-speed/Tight tracks (Monaco, Singapore):
    - Limited braking energy recovery (no long straights)
    - Focus on battery economy
    - Recommend: Low deployment, save energy for critical moments
  
  Technical note: MGU-K system adds tactical layer similar to fuel strategy
```

### 6.6.3 Dirty Air Effect (Following Cars)

**Aerodynamic Wake & Grip Loss:**

```
Dirty Air Definition: Loss of downforce when following closely behind another car

Physics: Car ahead displaces air, creates turbulent wake → following car loses aero grip

Distance-Based Effects:
  <0.5 seconds gap (≈50-80 meters):
    - Downforce loss: -0.1 to -0.2 sec/lap (moderate)
    - Handling: Understeer in corners, harder to maintain line
    - In technical corners: -0.1 sec/lap penalty
    - On straights: No penalty (drafting benefit +0.05 sec/lap)
  
  0.5-1.0 second gap:
    - Downforce loss: -0.15 to -0.3 sec/lap (significant)
    - Handling: Noticeable understeer, must reduce brake pressure
    - All corners: -0.2-0.3 sec/lap penalty
    - Strategic impact: Hard to overtake without clear straight
  
  1.0-1.5 second gap:
    - Downforce loss: -0.05 to -0.15 sec/lap (mild)
    - Handling: Manageable, slight understeer only
    - Overtaking possible if tires fresher
  
  >1.5 seconds gap:
    - Downforce loss: Negligible (<0.05 sec/lap)
    - Handling: Clean air, full grip available

Track Influence on Dirty Air:
  High downforce tracks (Monaco, Singapore):
    - Dirty air very severe (more downforce loss)
    - Gap must be >2 seconds for clean racing
    - Overtaking very difficult without DRS
  
  Low downforce tracks (Monza, Spa):
    - Dirty air less severe (tires matter more than aero)
    - Gap of 1 second usually sufficient
    - Overtaking easier due to straight-line speed
  
  Technical corners (90°+ turns):
    - Dirty air effect worst (maximum grip needed)
    - Understeer compounded by tire degradation
  
  Slow corners (<60° turn):
    - Dirty air effect reduced (brakes more important than aero)

Tactical Implications:
  Following Strategy: 
    - Stay 1.0-1.5 sec behind to conserve fuel/tires
    - Avoid <0.5 sec until ready to commit to overtake
    - Use straights to build confidence for attack
  
  Defending Strategy:
    - Leave >1.5 sec gap in technical sections (difficult to attack)
    - Tighten line in slow corners (increase apex speed)
    - Allow 0.5-1.0 sec in section opponent wants to attack (cover likely line)
  
  Overtaking Strategy:
    - Build confidence at 0.8-1.0 sec gap first
    - Attack on straight with DRS if available
    - Use fresh tires for overtake (overcome dirty air penalty)
    - High-speed corners easier (already losing some grip, attack planned)
```

---

## 6.7 RACE INCIDENT SYSTEM

### 6.7.1 Crash Mechanics

**Crash Probability (REALISTIC MODERN F1):**
```
Per lap base: 0.1-0.2% per lap (if normal racing, realistic ~1 crash per 500-1000 laps)

Modifiers (additive):
  - Wet weather: +0.2% per lap (wet is safer than expected due to grip management)
  - Heavy rain: +0.5% per lap
  - Driver fatigue (late race Lap 40+): +0.15% per lap
  - "Attack" instruction active: +0.3% per lap (accumulated fatigue, higher error rate)
  - Aggressive car setup (high aero): +0.1% per lap
  - Driver skill <60: +0.3% per lap (higher error proneness)
  - Contact with another car: +0.5% (immediate increased risk)

Maximum realistic crash probability: 2-3% per lap even in worst conditions

Example: Driver pushes hard Lap 45 (tired), wet track
  Base: 0.15%
  Wet: +0.2%
  Fatigue: +0.15%
  Attack instruction: +0.3%
  Total: 0.8% chance of crash this lap (high risk, but not guaranteed)
  
Compared to reality: F1 2024 had ~10 DNF crashes total on 480 race starts = ~2% per race, or ~0.1% per lap
```

**Crash Consequences:**

```
Minor Crash (Curb Strike, Light Contact):
  - Damage: Suspension -0.1 to -0.2 sec/lap
  - Repair needed: Yes, pit stop required (or manage with damage)
  - Time impact: Either 2.5 sec pit stop, or -0.2 sec/lap for rest of race
  - Points impact: Minor (no DNF)
  - Morale: Driver -10% (angry at self)

Moderate Crash (Barrier Hit):
  - Damage: Suspension -0.3 to -0.5 sec/lap, wing possibly bent
  - Repair: Front wing replacement needed (pit stop)
  - Time impact: 2.5-3.0 sec pit stop mandatory
  - Points impact: Major (lost 1-2 positions minimum)
  - Morale: Driver -20% (frustrated)

Severe Crash (High-Speed Barrier):
  - Damage: Multiple damage, car extremely unstable
  - Repair: Possible pit stop fix, but car unreliable
  - Risk: High DNF probability (50%+) from further damage
  - Decision: Pit and try to finish, or retire?
  - Morale: Driver -30% (extremely upset)

DNF Crash (Fatal):
  - Impact: Car undriveable, driver exits race
  - Repair: Not repairable mid-race
  - Points: 0 (DNF, no finishing position)
  - Damage cost: $2-5M repair/replacement
  - Morale: Driver -50% (severe morale hit, may request trade)
  - Championship impact: Lost 25 potential points

**Driver Injury System** (Post-crash medical evaluation):

```
Injury Probability by Crash Severity:

Minor Crash → Injury Risk: <0.1% (almost never)
Moderate Crash → Injury Risk: 0.5-1.5%
Severe Crash → Injury Risk: 2-5%
DNF Crash → Injury Risk: 5-15% (depending on speed/forces)

Injury Miss Rate: <1% (modern F1 safety is excellent)
  - Medical check happens immediately after crash
  - FIA doctor evaluation: 99%+ catch rate for injuries
  - Rare case (0.5%): Minor injury not detected until next day (sim resolves before next race)

Medical Evaluation Process:

Lap 45: Driver crashes barrier (Severe crash, -0.5 sec/lap damage)
Immediate: Safety car deployed
Lap 46: Driver in medical tent for evaluation
  - Medical team assesses injuries
  - Doctor questions: "Any pain? Vision OK? Head clear?"
  - Physical checks: reflexes, responsiveness
  - Potential outcomes:
    * Fit to continue: No injury detected (99% of time)
    * Light injury: Bruising, minor soreness (-5% morale penalty next race)
    * Moderate injury: Muscle strain, possible missed next race (50% chance to miss 1 race)
    * Severe injury: Concussion, bone fracture (definitely miss 1-2 races)

Injury Types & Seasons Missed:

Light Injury:
  - Severity: 0.5-1.5% crash severity
  - Symptoms: Bruising, muscle soreness
  - Recovery: No races missed (races same weekend if available)
  - Morale penalty: -5% next race ("sore, low confidence")
  - Financial: Optional injury insurance covers salary continuation

Moderate Injury:
  - Severity: 1.5-3% crash severity
  - Symptoms: Muscle strain, minor bone stress
  - Recovery: Miss 1 race (50% chance) or 2 races (50% chance)
  - Morale penalty: -15% when returning to racing
  - Financial: Injury insurance CRITICAL (covers lost salary + bonuses)
  - Impact: Must bring in reserve driver (skill 65-75)

Severe Injury:
  - Severity: >3% crash severity
  - Symptoms: Concussion, bone fracture, internal assessment
  - Recovery: Miss 2-3 races guaranteed
  - Morale penalty: -25% when returning (confidence severely shaken)
  - Financial: Without injury insurance, lose $2-5M in salary + bonuses
  - With insurance: Covered but still morale hit
  - Impact: Reserve driver stays for 2-3 races, might need external hire

Championship Impact of Injury:
  - Lose 25-75 points per missed race
  - Reserve driver typically scores 0-10 points/race vs primary 15-25
  - Season momentum lost (if missed race during good form)
```

### 6.7.2 Mechanical Failures (2026 MODERN RELIABILITY)

**DNF Probability During Race:**
```
MODERN F1 RELIABILITY (2025-2026):
Average DNF rate per race (per team): 1.5-2.5% (standard reliability)

Baseline Mechanical Failure (per race):
  - Engine failure: 0.5-1.0% per race (depends on power unit tuning)
  - Hydraulic failure: 0.3-0.5% per race
  - Electrical/ECU: 0.2-0.3% per race
  - Suspension failure: 0.3-0.5% per race (from curb strikes, setup aggression)

Total baseline DNF risk: 1.5-2.5% per race (normal reliability)

Reliability Investment Modifiers:
  - Poor reliability (low investment, aggressive tuning): 3.0-5.0% total
    * Engine pushed to maximum → higher failure risk
    * Minimal redundancy systems → component vulnerability
  
  - Standard reliability: 1.5-2.5% total (baseline)
    * Balanced tuning and safety margins
    * Standard maintenance protocols
  
  - Excellent reliability (high investment, conservative setup): 0.8-1.2% total
    * Conservative power mapping → extended engine life
    * Full redundancy systems → component protection
    * Strict maintenance schedules
  
Engine Resource Limit:
  - Each power unit has 6-8 Grand Prix resource limit
  - Risk increases if pushing beyond design parameters
  - Teams typically deploy fresh PU before running out of resources

Example: Lap 35 (of 58)
  Engine resource check: 5/8 used
  Reliability setting: Standard (2.0% DNF probability)
  Reliability roll: 75 (out of 0-100)
  Result: Engine survives if random roll > DNF probability
  
  Scenarios:
    If reliability poor (4.0% DNF): Would fail (only 4% survival chance on this roll)
    If reliability standard (2.0% DNF): Engine survives (75% > 2%)
    If reliability excellent (1.0% DNF): Safe (75% > 1%)
  
  Impact of DNF (rare but consequential):
    - Championship points: 0 points for race
    - Loss of strategic momentum (affects sponsor KPI tracking)
    - Team morale: -20% (reliability failure damage)
    - Sponsor morale: -10% (DNF reduces championship position KPI)
    - Financial impact: Potential $2-5M repair/replacement cost
    - Note: Prize money not affected (paid at season-end based on final championship position)
```

### 6.7.3 Engine Power Mode Impact on Reliability

**Qualifying Mode vs Economy Mode Strategy:**

```
QUALIFYING MODE (Maximum Power Tuning):
  Power output: +3-5% above standard
  Performance gain: +0.3-0.5 sec/lap
  Engine stress: Maximum
  Usage limit: 8-10 laps per race maximum
  
  Reliability Impact:
    Within limit (laps 1-8):
      - DNF probability: Normal (1.5-2.5%)
      - No additional risk
    
    Over limit (lap 9+ in qualifying mode):
      - DNF probability increases: ×3-5 multiplier
      - Example: Normal 2.0% → 6-10% DNF risk per additional lap
      - Risk escalates: Lap 9 = +2% risk, Lap 10 = +5% risk, Lap 11+ = exponential risk
      - Engine component stress: Excessive, approaching structural limits
  
  Tactical Use:
    - Aggressive overtaking: Use 2-3 laps of qualifying mode
    - Final sprint finish: Use 5-6 laps in last 5 laps
    - Never exceed 10 laps total per race (unless desperate situation)
    - Track remaining allocation: Display shows "Qualy mode: 4/8 laps remaining"
  
  Risk-Reward Decision:
    Scenario: In 4th place, 3 laps remaining, 1.2 sec behind 3rd place
    - Use all 4 remaining laps in qualifying mode: +0.4 sec/lap = 1.2 sec gain
    - Probability: 95% catch and pass 3rd place, 5% probability of engine failure
    - Risk: -25 points vs +3 points (DNF if engine fails)
    - Decision: Only use in late race when championship is at stake

ECONOMY MODE (Conservative Power Mapping):
  Power output: -2-3% below standard
  Performance loss: -0.15-0.25 sec/lap
  Engine stress: Minimal
  Usage: Can use entire race
  
  Reliability Benefits:
    - Engine life extension: +2-3 races beyond normal resource limit
    - Example: Normal 6 PU limit → 8-9 PU limit with economy mode
    - DNF probability reduction: -0.5% (from 2.0% → 1.5%)
    - Long-term team cost savings: $5-10M less engine replacement expenses
  
  Tactical Use:
    - Championship-clinching lead: Use to nurse engine safely
    - Multiple resource-limited power units: Stretch life further
    - Mid-field comfort: Secure points without mechanical risk
    - Wet weather: Weather protection (conservative setup reduces risk)
  
  When to Enable Economy Mode:
    - Team is 5+ points clear in championship
    - Power unit has 6+ Grand Prix used (approaching limit)
    - Finished in top-10 (secure points regardless of pace loss)
    - Weather uncertain (safety margin helps)

Long-Term Planning (Season Strategy):
  Example: 24-race season, 3 power units, limited budget
  
  Aggressive Strategy:
    - Races 1-10: Qualifying mode when needed (overtaking, qualify high)
    - Races 11-18: Standard mode (engine resources depleting)
    - Races 19-24: Fresh PU, unlimited power (season finale push)
    - PU usage: 8+8+8 = 24 races (perfect allocation)
    - Cost: 1 PU replacement mid-season ($3-5M)
  
  Conservative Strategy:
    - Races 1-12: Economy mode (preserve engines)
    - Races 13-24: Standard mode (last 12 races, engines still strong)
    - PU usage: 12+12 = 24 races (two PUs exactly)
    - Cost: Save $3-5M (avoid PU replacement)
    - Downside: Lost 0.15-0.25 sec/lap early season (missed point opportunities)

Driver Communication:
  Team radio call: "Switch to Economy mode, save the engine for final races"
  Driver response: Driver feels power loss but understands strategic value
  Morale impact: -5% (driver frustrated at slower pace)
  Recovery: +3% if economy mode results in championship clinch
```

---

## 6.8 RACE RESULT & POST-RACE

### 6.8.1 Final Classification

**Race Finalized After:**
- All drivers finish or DNF
- Top-3 weighing complete
- FIA stewards review (protests period)
- Official result published

**Finishing Position Points:**
```
1st:  25 points
2nd:  18 points
3rd:  15 points
4th:  12 points
5th:  10 points
6th:   8 points
7th:   6 points
8th:   4 points
9th:   2 points
10th:  1 point
11th+: 0 points

**Fastest Lap Bonus: ABOLISHED** (removed in 2025, no longer awards points)
Note: Fastest lap is tracked for driver records/stats but provides no championship points
```

### 6.8.2 Dynamic Driver Rating Updates

**Post-Race Driver Development** (Ratings evolve based on performance):

```
Driver Ratings (5 Categories, 0-100 scale):

1. PACE RATING (Raw speed):
   - Change: ±0-3 points per race
   - Factors:
     * Matched winning pace closely? (+2-3)
     * Lost to clearly slower car? (-1-2)
     * Struggled relative to teammate? (-1)
     * Clearly faster than field? (+2-3)
   
   Race example:
     Before: Pace 88
     Lap times: 0.1 sec/lap slower than winner
     Assessment: Steady, no major gains/losses
     After: Pace 88 (no change)

2. RACECRAFT RATING (Overtaking, defense, strategy execution):
   - Change: ±0-4 points per race
   - Factors:
     * Successful overtake? (+1 per overtake)
     * Defended position well? (+2 if kept faster car behind)
     * Made strategic error? (-2-3)
     * Won crucial position battle? (+3-4)
   
   Race example:
     Before: Racecraft 84
     Performance: Won position at Lap 18, defended against attack Lap 32
     After: Racecraft 88 (+4, two successful tactical moments)

3. CONSISTENCY RATING (Reliable, no errors, clean driving):
   - Change: ±0-2 points per race
   - Factors:
     * Clean race, no incidents? (+1-2)
     * Off-track excursion? (-1)
     * Collision with competitor? (-1-2)
     * Multiple small mistakes? (-1)
   
   Race example:
     Before: Consistency 82
     Performance: Completely clean race, all apexes perfect
     After: Consistency 84 (+2)

4. ADAPTABILITY RATING (Handling weather, tire transitions, setups):
   - Change: ±0-3 points per race
   - Factors:
     * Handled weather transition well? (+2-3)
     * Struggled with new tire compound? (-2)
     * Adapted to safety car perfectly? (+1)
     * Couldn't manage fuel correctly? (-1-2)
   
   Race example:
     Before: Adaptability 76
     Performance: Weather changed Lap 25, driver quickly adapted to intermediate tires
     After: Adaptability 79 (+3)

5. MATURITY RATING (Decision-making, responding to team radio, morale):
   - Change: ±0-3 points per race
   - Factors:
     * Accepted poor strategy gracefully? (+1)
     * Argued with pit crew over radio? (-2)
     * Made mature decisions under pressure? (+2-3)
     * Gave up when behind? (-1-2)

Example: Full Post-Race Rating Update

DRIVER: Lewis (Car #1)
═══════════════════════════════════════

BEFORE RACE:
  Pace: 92  Racecraft: 88  Consistency: 85  Adaptability: 80  Maturity: 87
  
RACE PERFORMANCE ASSESSMENT:

Pace Rating: 92 → 92 (No change)
  - Lap times: 1:47.3 avg, only 0.1 sec off winning pace
  - Relative to leader: Matched pace, car limitation was limiting factor
  - Assessment: World-class pace maintained
  
Racecraft Rating: 88 → 90 (+2)
  - Lap 18: Won position from Red Bull through superior cornering
  - Lap 32: Defended against aggressive Red Bull attack, maintained position
  - Assessment: Two successful tactical moments, excellent racecraft display
  
Consistency Rating: 85 → 86 (+1)
  - Track limits: 1 off-track excursion in Turn 12 (minor, corrected)
  - Collisions: None
  - Mistakes: One small error Lap 45 (minor, did not lose position)
  - Assessment: Mostly clean race with only small errors
  
Adaptability Rating: 80 → 83 (+3)
  - Tire transitions: Soft to Medium change Lap 12 handled perfectly (grip gained immediately)
  - Weather: No major weather transition this race
  - Strategy execution: Perfectly executed two-stop with optimal pit timing
  - Assessment: Excellent execution of strategy and tire management
  
Maturity Rating: 87 → 89 (+2)
  - Pit crew feedback: "Car was perfect today, great strategy" (positive)
  - Response to instructions: Followed team radio suggestions perfectly
  - Pressure handling: Remained calm when defending against Red Bull
  - Assessment: Mature, professional responses throughout

AFTER RACE:
  Pace: 92  Racecraft: 90  Consistency: 86  Adaptability: 83  Maturity: 89
  Overall Driver Rating: 88 → 90 (+2)

MORALE CHANGE:
  Before: 85%
  After: 90% (+5%)
  Reason: Podium finish, excellent racecraft display, team approval
  
LONG-TERM IMPACT:
  - Rating improvements: Drivers with consistent gains become more valuable
  - Championship impact: Higher rated drivers attract better sponsors/teams
  - Reserve driver development: Training shows improvements over time
  - Contract negotiation: Improved ratings can justify salary increases
```

**Rating Distribution** (across team):

```
Primary Driver (Pace 90+):
  - Develops most (±2-4 per race)
  - Can improve toward 95+ by season-end
  - Market value increases significantly

Secondary Driver (Pace 80-85):
  - Develops slower (±0-2 per race)
  - Unlikely to reach 90+ unless major breakthrough
  - May improve specific weaknesses (racecraft, adaptability)

Reserve Driver (Pace 65-75):
  - Develops quickly when racing (±2-3 per race substitute race)
  - Can reach 78-80 by end of season if races frequently
  - Better chance of securing seat next year with improvements
```

### 6.8.3 Prize Money Distribution

**NO RACE-BY-RACE PAYOUTS** (Season-End Payout Only):

```
Prize money is NOT distributed race-by-race in modern F1.
Instead:
  - Race results build your championship position
  - Championship position (1st-10th) determines prize payout AT SEASON END
  - Example: If you finish 4th in championship = $144M payout in December
  
Per-Race Impact:
  - Each race result affects your current championship position
  - Championship position affects sponsor KPI targets
  - Driver morale (+bonus if podium/win, -penalty if DNF/low finish)
  - Sponsor satisfaction (results toward contracted KPIs)

See Part 5 (Finance & Sponsorship) for complete prize distribution model
```

### 6.8.3 Post-Race Analysis

**Automatic Post-Race Report Generated:**

```
RACE REPORT - Race 8 (Belgium)
═════════════════════════════════════════════════════════

RESULT:
  Finishing Position: 3rd
  Championship Points: +15
  Prize Money: $1.5M
  Status: PODIUM ✓

PERFORMANCE SUMMARY:
  Qualifying: P2 (0.2 sec behind pole)
  Race start: P2 (good launch)
  Race pace: -0.05 sec/lap vs winner (competitive)
  Final pit stop: Lap 42, 2.4 sec (excellent execution)
  Tire management: Soft 12 laps, Medium 30 laps, Hard 16 laps (optimal)
  
DRIVER PERFORMANCE:
  Car #1 (Lewis): Pace 92, Racecraft 88
    - Lap-by-lap avg: 1:47.3 (on par with winner)
    - Best lap: 1:47.1 (fastest lap point attempted, failed by 0.1 sec)
    - Racecraft: Won position battle Lap 18, maintained against Lap 32 attack
    - Morale: 85% → 90% (podium bonus +5%)
    - Radio feedback: "Car was perfect today, great strategy."

TEAM PERFORMANCE:
  Pit crew: 2.4 sec pit stop (excellent)
  Strategy: Two-stop optimal for this track (good call)
  Reliability: Zero issues (excellent reliability standing)
  
COMPETITIVE ANALYSIS:
  1st place (Mercedes): 0.3 sec/lap faster (superior car)
  2nd place (Red Bull): 0.1 sec/lap faster (close battle, you won racecraft)
  4th place (McLaren): 0.4 sec/lap slower (clear advantage)
  
  Assessment: You executed strategy perfectly, lost on raw pace
             (car performance issue, not strategy)

CHAMPIONSHIP IMPACT:
  Previous: 4th place, 78 points
  This race: +15 points
  New total: 3rd place, 93 points
  Gap to leader: -25 points (improving)
  Momentum: Positive (podium finish, improving pace)

FINANCIAL IMPACT:
  Prize money: +$1.5M (Q3 cash flow improves)
  Sponsor bonuses: +$250K (podium counts toward TechCorp KPI)
  Driver bonuses: +$250K (Car #1 podium bonus)
  Team morale: +15% (podium celebration boost)
  
NEXT RACE RECOMMENDATIONS:
  - Upgrade aero package (0.1-0.2 sec/lap gap to Mercedes)
  - Maintain reliability focus (perfect record)
  - Continue pit crew excellence
  - Driver #2 needs morale boost (finished 8th, morale -5%)
```

---

**[END OF PART 6: RACE SIMULATION & MECHANICS]**

**Total Pages**: 30-35 pages

This system provides detailed real-time race decision-making with tire management, pit stops, fuel conservation, weather adaptation, strategic choices, and incident handling. All mechanics interconnect with Team Management (pit crew morale affects stop time) and Finance (results affect prize money and sponsor KPIs).

**NEXT SYSTEM**: Part 7 — UI/UX Design (screen layouts, dashboard, race weekend navigation, information hierarchy)
