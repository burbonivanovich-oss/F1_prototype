# PART 2: SEASON MANAGEMENT SYSTEM (REVISED)
## Complete Realistic Implementation

---

## 2.1 SEASON STRUCTURE & CALENDAR

### 2.1.1 Season Overview

**Real F1 Calendar Parameters**:

| Parameter | Real F1 2024-2025 | Game Implementation |
|-----------|------------------|-------------------|
| **Total Races** | 24 | 24 ✓ |
| **Calendar Duration** | March - December | March - December ✓ |
| **Typical Gap Between Races** | 7-21 days (variable) | 7-21 days (variable) ✓ |
| **Summer Break** | 2-3 weeks (August) | 2-3 weeks (August) ✓ |
| **Pre-Season Testing** | 3 days (February) | 2-3 days ✓ |
| **In-Season Test Dates** | Mid-season (2-3 dates) | Mid-season ✓ |

**Game Advantage**: Unlike real F1, we can optimize calendar for gameplay pacing while maintaining realism.

---

## 2.1.2 RACE WEEKEND STRUCTURE (DUAL FORMAT)

The game supports **two realistic weekend formats** that alternate throughout the season:

### FORMAT A: TRADITIONAL (Most Races - 16 events)

**Friday - Practice Day**:
- **FP1** (60 minutes): Early setup exploration
  - Player actions: Test different wing configurations, brake balance
  - Driver feedback: "Front understeer in Turns 3-5"
  - Outcomes: Baseline telemetry, setup direction
  
- **FP2** (60 minutes): Longer runs, tire degradation analysis
  - Player actions: Run qualifying simulations, race simulation (5-8 laps)
  - Tire management: Monitor thermal envelopes
  - Competitor intel: Analyze rivals' pace progression
  
- **FP3** (45 minutes): Final practice before qualifying
  - Setup fine-tuning
  - Qualify simulation run 1
  - Decision: Conservative vs aggressive setup for quali

**Saturday - Qualification Day**:
- **Qualifying** (60 minutes, 3 sessions):
  - **Q1** (18 min): All drivers. Bottom 5 eliminated.
    - Player decision: Aggressive fuel load or conservative?
    - DRS activation: Only after 3rd lap
    - Strategic timing: When to run hot lap (early/late in session)
  
  - **Q2** (15 min): 15 drivers remain
    - Player decision: One lap vs two laps?
    - Risk: New tires vs used tires?
  
  - **Q3** (12 min): Top 10 fighters
    - Player decision: Qualify on race fuel or light fuel?
    - **PARC FERMÉ RULE ACTIVE**: After Q1 (when car enters qualifying), car setup is sealed. No major setup changes until race day (only tire pressure/brake balance micro-adjustments within strict limits allowed)
    - Consequence: If setup is wrong in Q1-Q3, you're stuck with it for the race
  
**Sunday - Race Day**:
- **Formation Lap & Grid**: 30 minutes before race start
  - Cars exit pit lane and align on grid
  - Tire/brake warm-up procedures
  - Driver briefing
  - No overtaking allowed on formation lap
  - Any off-track excursion = pit lane restart penalty
  
- **RACE** (90-120 minutes simulated, but plays in **20-30 minutes** at 1x speed with acceleration options):
  - See section 2.1.4 for detailed race mechanics
  - **Speed options**: 1x / 2x / 5x / 10x / 20x
  - Average player race experience: 15-20 minutes at 1x (first lap, pit stops, finish) + 5-10 min accelerated (mid-race)

---

### FORMAT B: SPRINT (8 races per season - Friday qualifying, Saturday sprint, Sunday race)

**Introduced 2024 in real F1, now alternates with traditional format in our game.**

**Friday**:
- **FP1** (60 min): Single practice session
- **Qualifying** (60 min, compressed Q1/Q2/Q3): Determines grid for Sunday MAIN race (not sprint!)
  - **PARC FERMÉ** activated: Car sealed until Sunday race

**Saturday - Sprint Day**:
- **Sprint Qualifying** (30 min): MANDATORY qualifying session for sprint grid
  - Condensed format: SQ1 (all cars, 3 eliminated), SQ2 (remaining, 2 eliminated), SQ3 (top drivers)
  - Determines Saturday sprint grid (different from Sunday race grid!)
  - **PARC FERMÉ RESET**: After sprint qualifying, teams can adjust setup for sprint
- **Sprint Race** (17-21 laps, ~25 minutes real time):
  - Shorter race, 8-10 points available (1st=8pts, 2nd=7pts, 3rd=6pts, etc.)
  - Aggressive driving, high risk of collisions
  - Limited pit stops (usually 1 required)
  - **PARC FERMÉ RESET**: After sprint, teams can make setup changes for Sunday race
  - Fastest Lap bonus: +1 point (if finish in top 8 on sprint)

**Sunday**:
- **Race** (56 laps, ~90 minutes real time, plays in 20-30 min):
  - Full championship points (25-1 for 1st-10th)
  - Standard strategy
  - Fastest Lap bonus: +1 point (if finish in top 10 on race)
  - **Total points available that weekend**: Sprint (8pts) + Race (25pts) = 33pts max

**Strategic Impact**:
- Sprint races favor aggressive drivers (high risk/reward)
- Conservative drivers can save tires for Sunday race
- Teams must choose: Win sprint and risk Sunday? Or save for main race?

---

## 2.1.3 RACE SIMULATION & PLAYER EXPERIENCE

### Race Timeline (Real vs Game Time)

| Stage | Real Time | Game Time (1x) | Player Interaction |
|-------|-----------|----------------|-------------------|
| **Pre-race prep** | 20 min | 2 min | Automatic |
| **Formation lap** | 3 min | 30 sec | Automatic |
| **Lap 1-5** (critical) | 7-8 min | 2-3 min | **ACTIVE** - Monitor pit wall |
| **Mid-race** (Laps 6-40) | 40-50 min | 8-12 min | **CAN ACCELERATE** (5x-20x) |
| **Final laps** (safety car, overtakes) | 10-15 min | 2-3 min | **ACTIVE** - Critical decisions |
| **Cool-down lap** | 2 min | 20 sec | Automatic |
| **TOTAL** | 90-120 min | 15-25 min | ~8-12 min active |

**Player Experience**:
- Watch Lap 1 at 1x (real-time tension, drama)
- Accelerate to 5x during predictable mid-race phases
- Drop back to 1x for pit stops, overtakes, safety cars
- Game never forces auto-skip (player has full control of speed)

---

## 2.1.4 RACE VISUALIZATION & CONTROLS

### Race Screen (Football Manager Style)

**Visual Layout**:
- **Top**: Championship positions live ticker (20-30 drivers visible)
- **Left**: Real-time standings (Pos | Driver | Car | Laps | Gap | Pit | Tires)
- **Center**: Track map (bird's eye view)
  - Small circles representing cars (color-coded by team)
  - DRS zones highlighted (green zones on straights)
  - Safety car position (when active)
  - Yellow/red flags (accident locations)
- **Right**: Telemetry panel
  - Your drivers' data: Speed, tire temps, fuel, brake balance
  - Pit stop timer (if active)
  - Engineer radio (text snippets)
- **Bottom**: Timeline + Action log
  - "Lap 15: Hamilton pitted for mediums"
  - "Lap 18: Safety car deployed (Alonso crash Turn 3)"
  - "Lap 32: P2 driver tires warming up"

### Player Controls During Race

**Real-time Decisions** (paused-time menu):

1. **Pit Stop Decision**:
   - "Pit now?" → triggers pit entrance next lap
   - Tire compound choice (soft/medium/hard)
   - Fuel level (affects weight, pace)
   - Estimated pit time: 2.1 seconds + 0.3s safety margin
   - **Risk**: Gap to competitors (other cars pass during stop)

2. **Driver Instructions**:
   - "Push harder" → +2% fuel consumption, +1-2% pace
   - "Save tires" → -3% pace, extend tire life +20%
   - "Defend position" → defensive line, risk collision
   - "Attack" → overtake mode, higher fuel use
   - **Driver might disagree**: "I have good tires, let me attack!" (morale factor)

3. **Weather Reaction** (if rain/dry changes):
   - Pit for wets? (1-lap delay, but wet tires 2+ sec faster in rain)
   - Stay out? (gamble it stays dry)
   - Switch to inters? (mid-option)
   - **Dynamic**: AI makes recommendations, you decide

4. **Strategy Adjustment**:
   - One-stop vs two-stop (recalculate based on current pace)
   - Undercut opportunity? ("P3 pitted, we can undercut if we pit now")
   - Overcut? ("Stay out, let them pit, we pit later")

5. **Driver Conflict Resolution**:
   - **Scenario**: "I'm faster, let me overtake P2 internally"
     - Your choice: "Defend" / "Let him pass" / "Ignore, focus on race"
     - Morale impact: -15% if forced to let faster teammate pass
     - Speed impact: -0.5 sec if forced to defend

---

## 2.1.5 RACE OUTCOMES & POINTS

### Points System (Real F1 2024)

```
Position    Points    Example: P1 Driver
─────────────────────────────────
1st         25        +25 pts
2nd         18        
3rd         15        
4th         12        
5th         10        
6th         8         
7th         6         
8th         4         
9th         2         
10th        1         
Fastest Lap +1        (only if P1-P10)
DNF         0         (Did Not Finish)
```

**Dual Championships**:
- **Drivers' Championship**: Individual points accumulate
- **Constructors' Championship**: Both drivers' points combined

### Financial Rewards (Realistic Prize Money)

Prize money in real F1 depends on:
1. **Race finishing position** (primary)
2. **Constructors' Championship final standing** (at season end, bonus pool distribution)
3. **"Sporting income"** (appearance fees, historical payouts - Ferrari, Mercedes get extra %)

**Game Simplification** (acceptable for playability):

Per-race prize money structure:

| Position | Base Reward | Team with Good Reputation | Team in Crisis |
|----------|-------------|-------------------------|-----------------|
| **1st** | $5M | $5.5M | $4.5M |
| **2nd** | $3.5M | $3.75M | $3.25M |
| **3rd** | $2.5M | $2.75M | $2.25M |
| **4th** | $1.5M | $1.75M | $1.25M |
| **5th** | $1M | $1.2M | $0.8M |
| **6-10th** | $500K | $600K | $400K |
| **Fastest Lap Bonus** | $200K | $200K | $200K |

**End-of-Season Bonus Pool** (distributed based on Constructors' Championship position):
- 1st place constructor: +$15M bonus
- 2nd: +$10M
- 3rd: +$8M
- 4th: +$5M
- 5th-10th: +$1-2M

**HOME RACE BONUS** (if applicable):
- +$500K bonus
- +10% team morale (local support, media hype)
- Driver morale: +20% if driver is from that country

---

## 2.1.6 COST CAP SYSTEM ($135M Limit)

**Critical Realism Element**: In real F1, teams operate under a **$135 million annual budget cap** (2024-2025), strictly enforced by FIA.

### Cost Cap Breakdown (Realistic Distribution)

**Total Budget**: $135M per team

**Cost Cap Exclusions** (NOT counted toward limit):
- Driver salaries (2 main drivers)
- Technical Director salary
- Marketing & hospitality expenses
- Legal services & FIA fees
- Young driver programs (VLN, F2 sponsorship)

| Category | Amount | % | Details |
|----------|--------|---|---------|
| **Personnel Costs** | $45M | 31% | Driver salaries, engineers, staff |
| **Power Unit (Engine)** | $20M | 14% | Engine supply, cooling, MGU-K/MGU-H development |
| **Chassis Development** | $30M | 21% | Aero, suspension, carbon manufacturing |
| **Operations & Logistics** | $20M | 14% | Transport, facilities, pit crew |
| **Contingency & Reserves** | $20M | 14% | Damage repairs, emergency upgrades, reserves |
| **IT & Infrastructure** | $10M | 7% | Software, simulators, CFD computers |

### Cost Cap Penalties (Realistic)

If team exceeds $135M:

| Overage Amount | Penalty |
|----------------|---------|
| **<5% ($6.75M)** | Financial fine + 5-10% ATR reduction (wind tunnel/CFD cut) |
| **5-10% ($6.75-13.5M)** | Fine + 10-20% ATR reduction + possible 10-25 point deduction |
| **>10% ($13.5M+)** | Season exclusion from championship OR complete point deduction |
| **Systematic Fraud** | Team exclusion + Super License revocation of leadership |

### Cost Cap Mechanics in Game

**Player Management**:
- Weekly budget tracker showing spending vs $135M cap
- Real-time alerts: "You're at $138M spent, $7M remaining"
- Restrictions trigger when approaching cap:
  - Can't hire new engineers (except free agency)
  - Can't upgrade facilities
  - Can't pay signing bonuses
  - Can't invest in reserve driver development

**Strategic Decisions**:
- Spend aggressively early season → limited flexibility later
- Spend conservatively → miss development opportunities
- Mid-season crises (parts failures, injuries) → must manage emergency fund carefully

---

## 2.1.7 AERODYNAMIC TESTING RESTRICTIONS (ATR) SYSTEM

**Critical Realism**: Real F1 limits wind tunnel time and CFD resources based on previous season's constructors' championship position.

### ATR Basics

**Aerodynamic Testing** includes:
- Wind tunnel runs (physical aerodynamic tests)
- CFD computing hours (computational fluid dynamics)
- On-track aero measurement (goniometers, pressure sensors)

**Regulation (2024 F1)**:
- Limits vary by championship position
- Champ team: 56 wind tunnel hours/year (baseline: 224 runs or ~56 hours)
- 10th place team: ~80 hours/year (15% more)
- CFD: Similar proportional distribution (~1120-1840 units of computer hours)

### Game Implementation

**Weekly ATR Tracking**:

```
AERODYNAMIC RESEARCH BUDGET (2027 Season)
═════════════════════════════════════════
Based on 2026 Constructor Standing: 3rd Place → 90% of baseline

Wind Tunnel Allocation:
- Baseline (56 hrs/yr): 56 hours
- Your allocation (90%): 50.4 hours/year
- Remaining: 45.2 hours (80.7% used)
- Per week average: 0.97 hours/week (need to prioritize)

CFD Allocation:
- Baseline (1120 units/yr): 1120 units
- Your allocation (90%): 1008 units/year
- Remaining: 897 units (89% used)
- Current week: 19.4 units available

Next week resets: Check back Monday for new allocation
```

### ATR Impact on R&D Speed

**Wind Tunnel Time**:
- Each aero project requires X wind tunnel hours
- "New front wing concept": 8 hours (can't proceed without time allocated)
- "Aero refinement": 3 hours
- **Consequence**: Limited aero projects per season (2-4 major updates vs competitors' 4-6)

**CFD Allocation**:
- Each aero optimization run costs Y CFD units
- Limited CFD = less iteration = slower convergence to optimal solution
- Example: "Endplate winglet redesign needs 20 units CFD" (takes 1 week to accumulate)

### Strategic Decisions

**ATR Strategy**:
1. **Allocate to priority areas**: Aero strength → use full wind tunnel time on wings
2. **Defer low-priority work**: Suspension tweaks → less wind tunnel needed
3. **High-risk projects**: New concept aero might need 12+ wind tunnel hours (gamble)
4. **Use CFD to extend wind tunnel results**: Cheaper CFD can supplement tunnel work

### Penalties for Exceeding ATR

If team exceeds allocated wind tunnel/CFD:

| Overage | Penalty |
|---------|---------|
| **<5%** | Warning, deduction from next 2 weeks |
| **5-10%** | 1-month wind tunnel ban (0 hours), -50% CFD next 4 weeks |
| **>10%** | 3-month wind tunnel ban, -75% CFD for rest of season |

---

## 2.1.8 SEASON PHASES IN DETAIL

### PHASE 1: PRE-SEASON (Weeks -2 to 0)

**Duration**: 2-3 hours gameplay

**Actions**:
- Driver contract confirmation
- Base setup review (carry over from last season)
- **Pre-season testing (3 days in February, MANDATORY for all teams)**:
  - Three consecutive days at designated test track
  - Can be split as 3+3 days with 1-week break if needed
  - Run 100+ laps per car
  - Test 2027 car reliability
  - Gather baseline setup data
  - Driver morale boost (+5% for each day completed)
  - **FIA PENALTY for skipping**: -$500K budget + -10% morale + -5% R&D speed
- Facility check (all operational?)
- Budget allocation review (ensure <$135M)
- ATR planning (which projects fit in wind tunnel/CFD allocation?)

**Outcomes**:
- Good pre-season testing: +10% R&D speed to first upgrade, drivers +5% confidence
- Poor/skipped pre-season: -10% morale, higher reliability risk

---

### PHASE 2: CHAMPIONSHIP (Weeks 1-22)

**Standard 1-2 Week Cycle**:

**RACE WEEK**:
- Friday: FP1 (60 min) + FP2 (60 min) + FP3 (45 min) - player active
- Saturday: Qualifying (60 min, 3 sessions) - player active
- Sunday: Formation Lap (30 min before race) + Race (90-120 min real, 15-25 min game) - player very active

**OFF-WEEK (1-3 races between each weekend)**:
- Team management (1-2 hours gameplay):
  - Review driver performance (stats, complaints)
  - Check engineer morale
  - Process race earnings
  - Manage crisis events (injuries, departures, technical issues)
  - Plan next race strategy
- R&D work continues (automatic in background):
  - Wind tunnel testing progresses
  - CFD computing continues
  - Parts manufacturing advances

---

### PHASE 3: MID-SEASON (Race 12, Usually July)

**Critical Juncture**: Technical regulation updates, ADUO eligibility checks

#### REGULATION UPDATE (Real F1 style)

FIA might issue mid-season technical directives:
- "New floor design regulations effective Race 15"
- "DRS activation timing changed to Lap 4"
- "Fuel flow limit reduced to 105 kg/h" (hypothetical)

**Game Impact**:
- All teams scramble to comply
- Teams lagging in development get 2-week grace period (ADUO advantage)
- Dominant teams must conform immediately (BoP penalty equivalent)

#### ADUO SYSTEM (Aerodynamic Development Upgrade Option) → Renamed: EDUO (Engine Development Upgrade Option)

**Real F1 Mechanism**: If engine supplier lags by >2% power, FIA allocates extra engine dyno hours.

**Game Implementation**:

1. **Power Assessment** (calculated at mid-season):
   - Compare your power unit to championship leader
   - If you're >2% behind in peak power: Eligible for EDUO
   
2. **EDUO Benefits** (if qualified):
   - +20-30% extra engine dyno hours for remainder of season
   - No aero or financial bonuses
   - Power unit development only
   - Window: Available until next homologation freeze

3. **Teams Already Leading**:
   - No EDUO (can't use it to further increase lead)
   - Slight dyno hour reduction (-10%) to slow dominance

**Example Scenario**:
```
Mid-Season Power Check (Race 12, July 15):
══════════════════════════════════════════
Leader power: 1050 hp (Mercedes-style engine)
Your power: 1022 hp (client team with engine contract)
Gap: 28 hp = 2.67%

Result: EDUO ACTIVATED
- Engine dyno hours: +30 hours (20% bonus)
- Window: Next 8 races for development testing
- Can focus on MGU-K optimization, combustion efficiency, thermal management
```

---

### PHASE 4: END-OF-SEASON SPRINT (Races 21-24)

**Final 4 Races**: High-stakes, limited development

#### Resource Constraints

**Development Tokens Depleted**:
- Wind tunnel budget depleted (last 2-3 races = zero new development)
- CFD hours near exhaustion
- Manufacturing capacity strained (last-minute repairs only)

**Engine Reliability Risk**:
- Power units aging (some teams on 3rd or 4th engine of season)
- Higher failure rate (+30-50% DNF risk from reliability)

**Strategic Options**:
1. **Conservative**: Use old, reliable setups → predictable, safe points
2. **Aggressive**: Push reliability limits → risk DNF, potential breakthrough
3. **Balanced**: One safe driver, one aggressive driver

#### Championship Scenarios

**Scenario A - Leader Consolidating**:
- Leader focuses on reliability, safe points
- Risk: 2nd place driver gets lucky, new development, overtakes

**Scenario B - Close Fight**:
- Both leaders push hard
- Final races are nail-biters
- Safety cars often decide championship

**Scenario C - Underdog Surge**:
- Mid-field team that invested heavily in ADUO/development
- Makes unexpected climb with fresh upgrades
- Final races dramatic comebacks

---

## 2.1.9 DRAMATIC EVENTS SYSTEM

### Event Generation (3 Layers)

#### Layer 1: Deterministic Events (Triggered by Decisions)

**Your Decision** → **Guaranteed Consequence**

Examples:
- **Aggressive R&D strategy** (push engine to max):
  - 60% success: +5% power, looks great
  - 40% failure: Engine DNF Races 8-10, 3 races lost development, $2M repair
  
- **Skip pre-season testing** (save budget):
  - Risk: -10% reliability Race 1-3, higher setup mistakes
  - Reward: +$500K saved, usable for something else
  
- **Overspend on personnel early** (max budget month 1):
  - Constraint: Can't hire mid-season replacements, can't afford upgrades later
  - Consequence: Forced to use mediocre reserve when injury occurs

#### Layer 2: AI-Driven Events (Rival Actions)

**Rival Team's Action** → **Ripple Effect on Your Race**

Examples:
- **Rival team develops breakthrough aero**:
  - You discover via telemetry (faster on data runs)
  - Emergency meeting: "Need response package, costs $3M, 4 weeks"
  - Decision: Go for it, or accept performance gap?
  
- **Rival's driver crashes out** (Race 8):
  - Safety car deployed, strategy chaos
  - Opportunity: Undercut while they're pitless, gain 2-3 positions
  
- **Rival team poaches your chief engineer**:
  - Loss: -20% R&D speed for 2 weeks (until replacement
  - Retaliation option: Poach their technical director back (expensive)
  - Consequence: Drama, media scrutiny, morale hit

#### Layer 3: Stochastic (Random Environmental Events)

**Base Probability** (varies by your decisions)

| Event | Base % | Modifier | Example |
|-------|--------|----------|---------|
| **Driver Injury** | 0.8%/race | -0.3% if aggressive tactics | Rare but impacts season |
| **Reliability DNF** | 1.5%/race | +1% if aggressive R&D | Every 15-20 races |
| **Aero Breakthrough** | 0.5%/week | +0.2% if high R&D budget | 1-2 per season |
| **Sponsor Ultimatum** | 2%/month | +1% if poor results | Negotiation needed |
| **Key Staff Departure** | 0.7%/month | +0.5% if low morale | Predictable if monitored |
| **Wind Tunnel Malfunction** | 0.1%/month | - | 1-2 per season, 1-week downtime |

### Example Event Narratives

**Narrative A: "Crisis & Recovery"**
- Race 5: Your primary driver crashes in free practice
- Medical: 2-race absence (Races 6-7)
- Activate reserve (70% speed, morale -20%)
- Race 6: Finish 12th (bad), lose 18 points
- Race 7: Reserve driver scores points (confidence boost)
- Race 8: Primary driver returns, morale recovery
- **Lesson**: Reserve drivers can win surprising points

**Narrative B: "The Undercut"**
- Race 13: Close battle for P2
- Rival pits on Lap 18 for fresh tires
- Your driver is faster on old tires
- Decision: Pit now (undercut) or stay out?
  - Pit now: Gain fresh tires, hope to gain time before Lap 18+4 (rival's pit)
  - Stay out: Extend lead, but tires degrade fast
- If undercut works: +3-4 positions gained, race-deciding
- If it fails: Lose position, demoralize team

**Narrative C: "Regulation Shock"**
- Race 12: FIA announces floor design change (Race 15 onwards)
- Impact: Your current design becomes illegal
- Emergency development: 2-week sprint to adapt
- Budget: -$8M for accelerated engineering
- Risk: Miss opportunity on other development
- Consequence: Competitive shock to all teams, order reshuffled

---

## 2.1.10 CHAMPIONSHIP SCORING & SEASON FINALE

### Drivers' & Constructors' Championships

**Drivers' Championship**:
- Individual point accumulation across 24 races
- Highest score = World Champion

**Constructors' Championship** (equally important in F1):
- Both drivers' points combined
- Affects prize money distribution end-of-year
- Crucial for sponsor satisfaction

### Tie-Breaking (Real F1 Rules)

If drivers tied on points at season end:

1. **Head-to-Head Record**: Most 1st place finishes wins
2. **Fastest Lap Count**: More fastest laps breaks tie
3. **Most 2nd Place Finishes**: Extends h2h
4. **Race-Counting Rule**: Better results in final 5 races matter more

### Season Finale Drama

**Final Race Scenarios**:

1. **Champion Decided Already**: 2nd place fights for title (anti-climactic)
2. **Champion Decided in Final Lap**: Maximum drama, rewatch moments
3. **Championship Tied, Final Race Decides**: Ultimate tension (e.g., 2016 F1 championship)

**Game Encourages Scenario 2-3** through:
- AI championship management (leaders play defensively, underdogs push hard)
- Safety cars in final races (randomness, not guaranteed)
- Emerging technologies rewarding aggressive strategies

---

## 2.1.11 SEASON DIFFICULTY & REPLAYABILITY

### Difficulty Modifiers

| Setting | AI Strength | Budget | ATR Generosity | Event Probability |
|---------|-----------|--------||----------------|
| **Easy** | 70% | +20% ($174M) | +20% tunnel time | -50% bad events |
| **Normal** | 100% | Standard ($145M) | Baseline | Baseline |
| **Hard** | 130% | -10% ($130.5M) | -10% tunnel/CFD | +50% bad events |
| **Expert** | 150% | -20% ($116M) | -20% | +100% |

### Why Each Season Varies

1. **Procedural Event Generation**: Different injuries, breakthroughs, departures per season
2. **AI Rivalry Variation**: Rival teams take different strategic paths (aggressive vs conservative)
3. **Regulation Mutations**: Technical directives vary by season
4. **Weather Randomness**: Rain timing, intensity differs
5. **Driver Talent Pool**: Different junior driver talent available each season
6. **Mid-season Drama**: Unpredictable rule changes, competitor collapses

**Result**: High replay value, no two seasons identical.

---

**[END OF REVISED SEASON MANAGEMENT SYSTEM]**

**Next Section**: Part 3 - Team Management System (Revised)

**Key Changes Made**:
- ✅ ADUO system (realistic, not BoP)
- ✅ Cost Cap ($145M) enforcement
- ✅ ATR (wind tunnel/CFD limitations)
- ✅ Parc Fermé rules
- ✅ Sprint format included
- ✅ Home race bonuses
- ✅ Race time: 15-25 min (realistic pacing)
- ✅ Driver injury frequency reduced (0.8% vs 3%)
- ✅ Realistic prize money structure
- ✅ More detailed event systems

