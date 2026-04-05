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

**Five Tire Compounds (C1-C5)** (mandatory in race):

| Compound | Grip Level | Durability | Optimal Window | Typical Life |
|----------|-----------|-----------|----------------|-------------|
| **Soft (Red)** | +0.25 sec/lap | 15-20 laps | Laps 1-5, 25-30 | 18 laps |
| **Medium (Yellow)** | 0.0 sec/lap (baseline) | 25-35 laps | Laps 1-20, any time | 32 laps |
| **Hard (White)** | -0.15 sec/lap | 40-50 laps | Laps 20+ | 45 laps |

**Mandatory Requirement**: At least one pit stop with different compound change (cannot race on same compound entire race)

### 6.2.2 Tire Degradation Model

**Degradation Formula:**
```
Lap Time Loss = Base Lap Time + (Degradation Rate × Laps Used)

Example - Soft Tire Behavior:
  Lap 1: 1:23.5 (fresh, optimal)
  Lap 2: 1:23.6 (+0.1 sec degradation begins)
  Lap 3: 1:23.8 (+0.2 cumulative)
  Lap 4: 1:24.1 (+0.5 cumulative)
  Lap 5: 1:24.5 (+0.9 cumulative, near end of life)
  Lap 6: 1:25.2 (+1.6 cumulative, degrading rapidly)
  
  Soft tire window: Laps 1-5 are prime, Lap 6 is already slow
  Pit stop timing: Should stop by end of Lap 5 before tire goes off cliff
```

**Track-Specific Degradation:**
```
High Degradation Tracks (street circuits, high-temp):
  - Monaco, Singapore, Mexico City
  - C5 life: 10-18 laps only
  - Medium tire life: 20-25 laps
  - Hard tire life: 35-40 laps
  - Strategy: More frequent pit stops needed (2-3 stops mandatory)

Low Degradation Tracks (smooth, cool):
  - Monza, Montreal, Silverstone
  - Soft tire life: 20-25 laps
  - Medium tire life: 35-40 laps
  - Hard tire life: 50+ laps
  - Strategy: Fewer pit stops (potentially 1 stop possible)

Weather Impact:
  - Wet conditions: All tire compounds degrade 25% faster
  - Hot conditions (35°C+): Degradation +15%
  - Cold conditions (<10°C): Degradation -20% (tires harder to warm up)
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
      
    Total Pit Stop Time: 2.0-2.5 seconds (baseline, best teams)
    
    Pit Stop Variance:
      - Excellent execution (morale 85%+): 2.0-2.2 sec
      - Normal execution (morale 70%): 2.3-2.5 sec
      - Poor execution (morale <60%): 2.6-3.2 sec (equipment fumbles, tire mounting errors)
      - Critical error (1% probability): 4-5 sec (dropped tire, unsafe release)
  
  Lap N+1:
    - Car exits pit box
    - New tire compound installed and warming up
    - Performance loss during lap 1 on new tires: Fresh tires cold
      - Soft tires (fresh): -1.5 sec/lap Lap 1 only (cold tires)
      - Medium tires (fresh): -1.2 sec/lap Lap 1
      - Hard tires (fresh): -1.8 sec/lap Laps 1-2 (take longer to warm up)
    
    After tire warm-up (Lap 2+): Full grip available
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

---

## 6.3 FUEL MANAGEMENT

### 6.3.1 Fuel Consumption Model

**Consumption Rate:**
```
Base Consumption: 1.6 kg/lap (average)

Fuel Economy Modifiers:
  + Slow lap (fuel-saving mode): -0.2 kg/lap (-12.5% consumption)
  + Fast lap (aggressive): +0.1 kg/lap (+6% consumption)
  + Engine Mode mapping: Standard vs Power vs Qualifying
    - Qualifying mode: +0.3 kg/lap (maximum power)
    - Standard race mode: 1.6 kg/lap
    - Economy mode: 1.4 kg/lap (-12.5%)
  + Temperature impact: Hot conditions +0.1 kg/lap, cold -0.05 kg/lap
  + Tire compound (energy loss): Soft tires waste 0.05 kg/lap more energy

Track-Specific Consumption:
  - High downforce, slow speed (Monaco, Singapore): 1.8 kg/lap
  - Low downforce, high speed (Monza, Hungary): 1.4 kg/lap
  - Average (most tracks): 1.6 kg/lap
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
    - Lighter car (1-2 kg lighter = +0.01 sec/lap)
    - Lower fuel consumption rate
    - Requires precise pit stop scheduling
  Cons:
    - Zero margin for error (safety car, VSC, must pit exactly on time)
    - Cannot adjust fuel strategy mid-race
    - Risk: Running out of fuel before finish (DNF)
  
  Result: Risky, suitable for dry weather, one-stop strategy only

Option B: Standard Load (105-107 kg)
  Pros:
    - Adequate safety margin (5-10 km buffer)
    - Flexibility for 1-2 stops
    - Can adjust pit stops reactively
  Cons:
    - Slightly heavier car (-0.02 sec/lap)
    - Standard fuel consumption
  
  Result: Recommended, safe, balanced

Option C: Maximum Load (110 kg, refueling limit)
  Pros:
    - Maximum flexibility (can pit late or skip second stop)
    - Extra fuel for safety car scenarios
  Cons:
    - Heavy car (-0.1 sec/lap), significant performance penalty
    - Wears tires faster under weight
    - Waste fuel if don't need full amount
  
  Result: Defensive, suitable for uncertain weather, high DNF risk strategies

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
  - Driver instruction change: "Be careful, wet sections appearing"
  - Pit stop decision: Switch from dry to intermediate tires

HEAVY RAIN:
  - Track temp: 5-15°C, heavy water layer
  - Air temp: 5-12°C
  - Visibility: 50% of dry (dangerous, simulation can be suspended)
  - Tire performance: Wet tires required
  - Performance: All lap times -1.5 to -2.5 sec/lap slower
  - Safety car likely deployed
  - Driver instruction: "Safety first, no risks"
  - Pit stop decision: Switch to wet tires or pause/pit early
```

### 6.4.2 Weather Transitions

**Rain Radar** (shown in race UI):

```
Race Lap 10 (of 58):
  - Current weather: Dry
  - Forecast (next 10 minutes / next 5 laps): Light rain approaching
  - Confidence: 80% (likely)
  - ETA: Lap 13-15

Strategic Question:
  - Pre-pit for intermediate tires now? (Preventive)
  - Wait and see? (Reactive)
  - Risk getting caught out by sudden downpour

Scenario A: Pre-pit at Lap 11
  - Change to intermediate tires (soft compounds, only +0.5 sec/lap penalty)
  - If rain doesn't arrive for 10 more laps: Wasted pit stop, lost time
  - If rain arrives Lap 13: Already on correct tires, perfect!
  - Decision: Gamble on forecast

Scenario B: Stay dry, pit only if rain comes
  - Risk: If rain sudden (Lap 13), 3 laps on dry tires on wet track
    - Lap 13: -0.8 sec/lap (dry tires don't grip well)
    - Lap 14: -0.9 sec/lap (even worse)
    - Lap 15: -1.0 sec/lap (spinning risk!)
  - Pit at Lap 15, switch to intermediate
  - Cost: Lost 3 laps on slow tires, potentially in wrong position
  - Decision: More risk, but saves pit stop if no rain

Recommendation from Telemetry:
  "Rain probability 80%, hit Lap 13-14. Pit Lap 12 to pre-empt. Cost: 0.5 
   sec/lap for 1 lap, but save 5+ seconds if rain arrives."

Decision: Pit Lap 12 for intermediate tires (proactive strategy)
```

### 6.4.3 Safety Car & VSC Dynamics

**Safety Car** (Full Course Yellow):

Triggered by: Crash, debris, severe weather

Effect:
  - All cars line up behind safety car, single-file
  - Speed: 60-80 km/h (controlled pace)
  - DRS disabled: Cannot overtake
  - Pit stops still available (but risky, easy to lose position)
  - Duration: 3-8 laps typically

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

INSTRUCTION: "Push/Attack"
  Effect: Driver maximizes pace, takes risks
  Cost: -0.05 sec/lap (pushing hard increases risk)
  Morale impact: +10% if overtake successful, -10% if crash
  Reliability: +1% DNF probability from aggressive driving
  Duration: 3 laps maximum (driver fatigue)
  Use case: Attacking leader, attempting overtake

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

THREE-STOP STRATEGY:
  Plan: Soft 10 laps → Medium 20 laps → Hard 20 laps → Soft 8 laps (pit stops Lap 10, 30, 50)
  Requires: Aggressive fuel management and pit timing
  
  Pros:
    - Freshest tires possible
    - Maximum pace consistency
    - Can recover from safety car scenarios
  
  Cons:
    - Extra pit stops = massive time loss (3 × 2.5 sec = 7.5 seconds)
    - Only worth if degradation is extreme
    - Requires perfect execution
  
  Risk: VERY HIGH (only viable in extreme conditions)

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

---

## 6.7 RACE INCIDENT SYSTEM

### 6.7.1 Crash Mechanics

**Crash Probability:**
```
Per lap base: 0.5% per lap (if normal racing)
Increases if:
  - Wet weather: +2% probability
  - Driver fatigue (late race): +1% probability
  - "Attack" instruction active: +1% probability
  - Aggressive car setup (aero): +0.5% probability
  - Driver skill <60: +2% additional

Example: Driver pushes hard Lap 45 (tired), wet track
  Base: 0.5%
  Wet: +2%
  Fatigue: +1%
  Attack instruction: +1%
  Total: 4.5% chance of crash this lap
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
```

### 6.7.2 Mechanical Failures

**DNF Probability During Race:**
```
Engine failure: 0.3-1.5% per race (depends on power tuning aggressiveness)
Hydraulic failure: 0.2-0.8% per race
Electrical/ECU: 0.2-0.5% per race
Suspension: 0.5-1.2% per race (depends on damage from curbs)

Total DNF risk: 1.2-3.5% per race (varies by team reliability investment)

Example: Lap 35 (of 58)
  Random event triggers: Engine failure check
  Roll: 47 (out of 0-100)
  Result: Engine fails if probability >47%
  
  If reliability investment adequate (DNF 0.5%): Engine survives
  If reliability poor (DNF 2%): Engine fails, DNF Lap 35
  
  Impact of DNF:
    - Lost 23 laps remaining (potential 23 × $500K = $11.5M prize money)
    - Championship points: 0
    - Team morale: -20% (reliability failure anger)
    - Sponsor morale: -10% (DNF hurts their KPIs)
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

Fastest Lap Bonus: +1 point (available to any finisher, not just podium)
Note: Fastest lap giver loses 1 point if overtaken late race (risk/reward)
```

### 6.8.2 Financial Distribution

**Prize Money Per Race:**
```
Race distributes $25M total:

1st place: $4M
2nd place: $2.5M
3rd place: $1.5M
4th place: $1M
5th place: $800K
6th place: $600K
7th place: $400K
8th place: $300K
9th place: $200K
10th place: $100K

Finishing position affects:
  - Individual race payment (above)
  - Constructor championship running total (affects season prize money)
  - Driver morale (+bonus if podium/win, -penalty if missed expected finish)
  - Sponsor KPI tracking (affects sponsor satisfaction)
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
