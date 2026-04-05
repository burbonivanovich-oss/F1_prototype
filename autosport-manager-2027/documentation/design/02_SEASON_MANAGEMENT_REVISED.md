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
    - **PARC FERMÉ RULE ACTIVE**: After Q1, when car enters qualifying, car is sealed. No setup changes until race day (except tire pressure/brake balance within strict limits)
    - Consequence: If setup is wrong in Q2, you're stuck with it until warm-up laps Sunday
  
**Sunday - Race Day**:
- **Formation Lap** (30 min before race start): Cars exit pit lane and align on grid, no overtaking allowed
- **RACE** (90-120 minutes simulated, but plays in **20-30 minutes** at 1x speed with acceleration options):
  - See section 2.1.4 for detailed race mechanics
  - **Speed options**: 1x / 2x / 5x / 10x / 20x
  - Average player race experience: 15-20 minutes at 1x (first lap, pit stops, finish) + 5-10 min accelerated (mid-race)

---

### FORMAT B: SPRINT (6 races per season - Friday qualifying, Saturday sprint, Sunday race)

**Real F1 2026**: Only 6 sprint weekends per season (down from 8 in 2025, up from original 3).

**Friday**:
- **FP1** (60 min): Single practice session
- **Qualifying** (60 min, compressed Q1/Q2/Q3): Determines grid for Sunday main race
  - **PARC FERMÉ** activated: No changes until sprint Saturday

**Saturday - Sprint Day**:
- **Sprint Qualifying** (30 min): MANDATORY qualifying session for sprint grid
  - Determines who starts the sprint race (separate grid from Sunday)
- **Sprint Race** (17-21 laps, ~25 minutes real time):
  - Shorter race, 8-10 points available (1st=8pts, 2nd=7pts, 3rd=6pts, etc.)
  - Aggressive driving, high risk of collisions
  - Limited pit stops (usually 1 required)
  - **PARC FERMÉ RESET**: After sprint, teams can make setup changes for Sunday race
  - **No Fastest Lap bonus** (removed 2025)

**Sunday**:
- **Race** (56 laps, ~90 minutes real time, plays in 20-30 min):
  - Full championship points (25-1 for 1st-10th)
  - Standard strategy
  - **No Fastest Lap bonus** (removed 2025)
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
DNF         0         (Did Not Finish)
```

**Note**: Fastest Lap bonus point was **removed in 2025**. No additional points for fastest lap in either sprint or main race.

**Dual Championships**:
- **Drivers' Championship**: Individual points accumulate
- **Constructors' Championship**: Both drivers' points combined

### Financial Rewards (Annual Prize Pool Distribution)

**Changed for 2026**: Prize money now distributed **annually by final championship position** (not per-race), with mid-season advances.

**Annual Prize Pool** (approximately $450M total from FIA):

| Position | Annual Amount | Details |
|----------|--------------|---------|
| **1st Constructor** | ~$100M | 1st place in Constructors' Championship |
| **2nd Constructor** | ~$80M | |
| **3rd Constructor** | ~$60M | |
| **4th Constructor** | ~$45M | |
| **5th Constructor** | ~$35M | |
| **6th Constructor** | ~$25M | |
| **7th Constructor** | ~$18M | |
| **8th Constructor** | ~$12M | |
| **9th Constructor** | ~$8M | |
| **10th Constructor** | ~$5M | |

**Mid-Season Advances**:
- Every 6 races: $5M advance payment to all teams (improves cash flow)
- Sprint race bonuses: $50-100K per 1st place finish (additional)

**Driver Bonuses** (from team's annual pool):
- Win bonus: $500K per race win
- Podium bonus: $250K per podium finish
- Championship bonus: Additional % if top 3 finishers

**Heritage Bonus** (for applicable teams):
- Ferrari: $70-100M/year (historical contribution)
- Mercedes: $25-35M/year
- McLaren: $15-25M/year

**HOME RACE BONUS** (if applicable):
- +$500K bonus
- +10% team morale (local support, media hype)
- Driver morale: +20% if driver is from that country

---

## 2.1.6 COST CAP SYSTEM ($215M Limit - 2026 and onwards)

**Critical Realism Element**: In real F1 2026, the budget cap was increased to **$215 million annually**, strictly enforced by FIA. The cap also adjusts based on calendar size (+$1.8M per race beyond 21 races).

### Cost Cap Breakdown (Realistic Distribution)

**Total Budget**: $215M per team (base, adjustable by calendar)
- **Base**: $215M for 21 races
- **Additional**: +$1.8M per race beyond 21 races
- **2026 Season**: 24 races = $215M + (3 × $1.8M) = **$220.4M maximum**

| Category | Amount | % | Details |
|----------|--------|---|---------|
| **R&D & Development** | $65M | 30% | Aero, chassis, suspension development (IN CAP) |
| **Power Unit** | $35M | 16% | Engine supply, integration, dyno testing (IN CAP for client teams) |
| **Manufacturing & Operations** | $55M | 26% | Factory, production, assembly, tooling (IN CAP) |
| **Team Personnel** | $40M | 19% | Technical staff, engineers, mechanics (IN CAP) |
| **Contingency & Reserves** | $20M | 9% | Emergency repairs, upgrades, reserves (IN CAP) |

### Cost Cap EXCLUSIONS (NOT counted toward limit):

The following major expenses are **excluded from Cost Cap**:
- **Driver Salaries** (2 main drivers): Unlimited
- **Technical Director Salary**: Unlimited
- **Marketing & Commercial**: Unlimited
- **Legal Services & FIA Fees**: Unlimited
- **Logistics (Flights, Hotels)**: Unlimited
- **Support Staff Salaries** (some categories): Can be up to 25% of team personnel budget outside cap

**Impact on Game**: Player focuses on controllable development costs, not unlimited salary spending.

### Cost Cap Penalties (Realistic - 2026)

If team exceeds $215M cap:

| Overage Amount | Penalty |
|----------------|---------|
| **<5% (~$11M)** | Financial fine + warning (FIA can recommend reduction in next season) |
| **5-10% (~$11-21M)** | Fine ($5-15M) + wind tunnel/CFD reduction (5-10% cut) |
| **>10% (>$21M)** | Major fine + wind tunnel reduction (20%) + potential points deduction (10-25 pts) |
| **Systemic Breach** | Team exclusion from season / Constructors' Cup disqualification |

Note: FIA applies discretion based on circumstances. Minor technical breaches treated differently from willful overspend.

### Cost Cap Mechanics in Game

**Player Management**:
- Weekly budget tracker showing spending vs $215M cap (adjusted for calendar)
- Real-time alerts: "You're at $165M spent, $55M remaining" 
- Four quarterly audits (after races 6, 12, 18, 24) verify spending
- Restrictions when exceeding ~90% of cap:
  - Can't hire new engineers
  - Can't upgrade manufacturing facilities
  - Can't pay large signing bonuses
  - Must reduce discretionary spending

**Strategic Decisions**:
- Heavy early investment in R&D → limited flexibility later season
- Conservative spending → miss competitive window
- Mid-season crises (crashes, parts failures) → must manage reserves carefully
- Multi-season planning: overspend this year = reduced next season budget

**Audit System**:
- Q1 Audit (Race 6): Check for issues early
- Q2 Audit (Race 12): Mid-season adjustment period
- Q3 Audit (Race 18): Final opportunity to adjust
- Final Audit (Race 24): End-of-season settlement

---

## 2.1.7 AERODYNAMIC TESTING RESTRICTIONS (ATR) SYSTEM

**Critical Realism**: Real F1 limits wind tunnel time and CFD resources based on previous season's constructors' championship position.

### ATR Basics

**Aerodynamic Testing Resources** include:
- **Wind Tunnel Hours**: Physical aerodynamic testing
- **CFD Units**: Computational fluid dynamics computing hours
- **Dyno Hours**: Engine dynamometer testing (Power Unit development)
- **On-track Testing**: Pre-season tests and practice sessions

**Regulation (2026 F1) - Sliding Scale**:
The ATR system uses a "sliding scale" based on championship position:
- **1st Place Team**: 70% of baseline (most restricted, rewards success)
- **2nd Place Team**: 80% of baseline
- **10th Place Team**: 115% of baseline (most resources, rewards lower teams)
- **Linear scale**: Every position changes by ~4-5% increments

**Baselines**:
- Wind Tunnel: 80 hours/year (baseline)
- CFD: 1840 units/year (baseline)

**Examples**:
- Championship leader: 80 hrs × 70% = 56 hours wind tunnel
- 10th place team: 80 hrs × 115% = 92 hours wind tunnel
- Your 3rd place team: 80 hrs × 87% = 69.6 hours wind tunnel

### Game Implementation

**Weekly ATR Tracking**:

```
AERODYNAMIC RESEARCH BUDGET (2027 Season)
═════════════════════════════════════════
Based on 2026 Constructor Standing: 3rd Place → 87% of baseline

Wind Tunnel Allocation:
- Baseline: 80 hours/year
- Your allocation (87%): 69.6 hours/year
- Used to date: 28.5 hours
- Remaining: 41.1 hours (39.6% remaining)
- Per week average: 0.79 hours/week

CFD Allocation:
- Baseline: 1840 units/year
- Your allocation (87%): 1600.8 units/year
- Used to date: 720 units
- Remaining: 880.8 units (55% remaining)
- Current week: 16.9 units available
```

**Strategic Impact**: Higher championship position = LESS development budget = must prioritize carefully

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
- Pre-season testing (3 days in February, optional but recommended):
  - Run 100+ laps per car
  - Test 2027 car reliability
  - Gather baseline setup data
  - Driver morale boost (+5% for each test)
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
- Sunday: Warm-up (30 min) + Race (90-120 min real, 15-25 min game) - player very active

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

#### EDUO SYSTEM (Engine Development Upgrade Option - Real F1 Mechanism)

**Real F1 2026 Mechanism**: FIA checks power deficit THREE TIMES per season (every 6 races). If engine supplier lags by 2%+ power, FIA allocates extra development hours. Allocation depends on deficit magnitude.

**Game Implementation**:

**Three Power Checks**:
1. **Check 1** (Race 6): First assessment
2. **Check 2** (Race 12): Mid-season evaluation
3. **Check 3** (Race 18): Final check

**EDUO Eligibility & Benefits by Deficit**:

| Power Deficit | EDUO Eligibility | Dyno Hour Bonus | Duration |
|---------------|------------------|-----------------|----------|
| **2.0-4.0%** | 1 authorized PU upgrade | +20% dyno hours | Until next check |
| **4.0%+** | 2 authorized PU upgrades | +30% dyno hours | Until next check |
| **0-2.0%** | No EDUO (not eligible) | None | N/A |

**Important Restrictions**:
- Upgrades must be completed BEFORE next official engine homologation freeze
- No financial compensation (only development resource bonus)
- Upgrades must be applied to the engine (not chassis or aero)
- Teams already leading: No EDUO access

**Example Scenario**:
```
Check 1 (After Race 6):
Leader power: 1050 hp
Your team: 1005 hp
Gap: 45 hp = 4.3% → EDUO ACTIVATED

You get: 2 authorized PU upgrades + 30% dyno hour bonus
Must complete both before Check 2 (Race 12)

Check 2 (After Race 12):
Your team: 1028 hp
Gap: 22 hp = 2.1% → No longer eligible
EDUO expires (even if didn't use all upgrades)
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
| **Driver Injury** (Serious) | 0.1%/race | +0.1% per aggressive instruction per race | Extremely rare, linked to recklessness |
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
| **Normal** | 100% | Standard ($135M) | Baseline | Baseline |
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
- ✅ Cost Cap ($135M) enforcement
- ✅ ATR (wind tunnel/CFD limitations)
- ✅ Parc Fermé rules
- ✅ Sprint format included
- ✅ Home race bonuses
- ✅ Race time: 15-25 min (realistic pacing)
- ✅ Driver injury frequency reduced (0.8% vs 3%)
- ✅ Realistic prize money structure
- ✅ More detailed event systems

