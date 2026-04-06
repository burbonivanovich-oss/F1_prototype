# GAME OVERVIEW: Autosport Manager 2027
## Complete Vision Document

**Version**: 1.0  
**Date**: 2026-04-06  
**Purpose**: Define what the game IS, what player DOES, main loops, what makes it fun  

---

## 1. WHAT IS THIS GAME?

### Genre Definition
- **Type**: Sports Management Simulation (like Football Manager, Motorsport Manager)
- **NOT**: Racing simulator (player never steers a car)
- **Focus**: Strategic decision-making, team building, resource management
- **Player Role**: Team Principal / Manager

### Core Loop (What player does)

```
MACRO LOOP (Season = 9 months, 24 races):
    Week 1: Plan strategy for Race 1
    Week 2: Manage R&D, hire/fire staff
    Week 3: Race 1 (active, 10 min)
    Week 4: Review results, manage morale
    Week 5: Plan strategy for Race 2
    ... repeat ...
    Week 237: Race 24, season complete
    
MICRO LOOP (Race day = 3 hours, compressed to 30 min):
    1. Qualifying (30 min simulation, player makes setup choices)
    2. Race (20 min active, pit decisions every 2-3 laps)
    3. Results (3 min review, post-race analysis)
```

---

## 2. FIVE MAIN GAME SYSTEMS

These are NOT separate games — they are interconnected.

### SYSTEM 1: TEAM MANAGEMENT (Daily/Weekly)
**What**: Manage 60-80 core employees (3 directors, 7 department leads, 2 drivers, 20-25 engineers, mechanics, strategists, operations)

**Player decisions**:
- Hire/fire drivers (contracts, salaries, performance management)
- Manage 3 executive directors (Performance, Engineering, Technical)
- Assign department leads to 8 R&D centers
- Negotiate salaries (within $215M Cost Cap)
- Manage team morale (affects pit crew speed, engineer productivity)
- Handle crises (driver injury, engineer departure poaching, morale collapse)
- Manage workload (overloaded engineers make mistakes, miss deadlines)

**Affects**: 
- Race performance (engineer skill → setup quality)
- Driver pace (morale → +/- 0.5 sec/lap)
- Pit stop speed (team morale → 1.8-4.0 sec per stop)
- R&D progress (engineer workload → delays, errors)
- Reliability (overworked engineers → more DNFs)

**Time scale**: Weekly updates (morale changes, salary costs), critical decisions during inter-race windows

---

### SYSTEM 2: R&D / VEHICLE DEVELOPMENT (Weekly/Monthly)
**What**: Research and develop car components across 8 specialized research centers

**8 R&D Centers** (each has own budget, personnel, development tree):
1. **Aerodynamics** — wing designs, floor package, DRS optimization
   - Budget: $42-54M/year (40-50% of R&D)
   - Development: 12-14 weeks per major upgrade (+0.3-0.5 sec/lap gain)
   
2. **Power Unit** — engine power, ERS, thermal management (manufacturer teams only)
   - Budget: $32-42M/year (non-manufacturers buy engines, ~$15M/year outside cost cap)
   - Development: homologation locked (only evolution improvements allowed)
   
3. **Chassis & Suspension** — suspension geometry, stiffness, materials
   - Budget: $16-22M/year (15-20% of R&D)
   - Development: 8-10 weeks per upgrade (+0.1-0.3 sec/lap gain)
   
4. **Reliability & Durability** — failure prevention, testing, endurance
   - Budget: $12-18M/year
   - Development: ongoing (reduces DNF chance)
   
5. **Manufacturing & Assembly** — production efficiency, quality control, tooling
   - Budget: $10-15M/year
   - Development: affects how quickly upgrades can be produced
   
6. **Telemetry & Data Analytics** — data analysis, performance optimization, AI insights
   - Budget: $8-12M/year
   - Development: affects pit strategy quality, setup optimization
   
7. **Brakes & Thermal Management** — brake system, cooling efficiency, temperature control
   - Budget: $8-12M/year
   - Development: 10-12 weeks per major upgrade
   
8. **Simulator & Driver Development** — sim fidelity, driver training, talent development
   - Budget: $6-10M/year
   - Development: affects driver skill improvement rate

**Research & Development Timeline**:
- **Research phase**: 4-8 weeks (theoretical exploration, CFD, competitor analysis)
- **Development phase**: 8-16 weeks (prototype, testing, refinement)
- **Total**: 12-24 weeks per major upgrade
- **Deployment**: Next 2-3 races after completion

**ATR (Aerodynamic Testing Restrictions)**:
- Based on championship position: leader gets fewer wind tunnel hours
- Restrictions: 56-80 hours/year wind tunnel, 1120-1840 CFD units/year
- Strategic: use budget wisely, can't simulate everything

**ADUO (Additional Development & Upgrade Opportunities)**:
- Non-leading manufacturers (non-title contenders) get extra development time
- Catch-up mechanic: allows underdogs to develop faster upgrades

**Player decisions**:
- Allocate budget to each of 8 centers ($108M R&D budget from $215M cost cap)
- Choose which upgrades to develop (e.g., "new floor" vs "suspension refinement")
- Prioritize: long-term (complex, high-gain) vs short-term (simple, quick)
- Track development trees for each center (multiple upgrade paths)
- Manage ATR hours (aero testing is limited, plan carefully)

**Affects**: 
- Car speed (0.1-0.5 sec/lap per upgrade)
- Reliability (engineering depth → lower DNF chance)
- Competitiveness (upgrades cumulative through season)

**Time scale**: Monthly planning, 12-24 week development cycles, upgrades deployed every 3-5 races

---

### SYSTEM 3: RACE MANAGEMENT (Race weekend)
**What**: Manage single race weekend (2 days: qualifying + race)

**Race Weekend Structure** (Two formats, alternating):
- **Traditional Format** (16 races/season): Friday (FP1 + FP2 + FP3) → Saturday (Qualifying Q1/Q2/Q3) → Sunday (Formation Lap + Race)
- **Sprint Format** (6 races/season): Friday (FP1 + Qualifying) → Saturday (Sprint Qualifying + Sprint Race) → Sunday (Race)

**Friday/Saturday - Qualifying (Player actions)**:
- **Setup decisions**: wing angle, brake balance, fuel load, suspension stiffness
- **Tire strategy for quali**: aggressive (light fuel, push hard) vs conservative (heavy fuel, safe pace)
- **PARC FERMÉ rule**: after Q1, car is sealed — no setup changes until race day (only tire pressure/brake balance adjustments allowed)
- **Consequence**: wrong setup in quali means stuck with it for Sunday unless you pit for fresh tires

**Sunday - Race (Active 15-25 minutes at 1x speed)**:
- **Pit strategy**: 1-stop vs 2-stop vs 3-stop (based on tire degradation, fuel consumption, competitor pace)
- **Tire compound choice**: Soft/Medium/Hard (mandatory: use 2+ different compounds in dry race)
- **Pit stop timing**: Call pit at optimal lap (too early = waste fresh tires, too late = cliff phase)
- **Driver instructions** (every lap, paused-time decisions):
  - "Push harder" → +2% pace, +20% fuel consumption, morale depends on driver
  - "Save tires" → -3% pace, extend tire life +20%
  - "Defend position" → defensive racing line, collision risk
  - "Attack" → overtake mode, high fuel use
  - "Fuel save" → economy mode, -5% pace but extend fuel range
- **Weather adaptation**: pit for wets when rain arrives, switch to inters during drying
- **In-race incidents**: crashes, safety car, collisions (damage impacts pace)

**Pit Stop Mechanics**:
- Duration: 1.9-2.5 seconds (based on team morale)
  - Perfect execution (morale 95%+): 1.82-1.95 sec
  - Good execution (morale 85-94%): 1.95-2.1 sec
  - Normal (morale 70-84%): 2.2-2.5 sec
  - Poor (morale <70%): 2.8-4.0 sec
  - Critical error (0.5% chance): 4.0-6.0 sec (unsafe release, dropped tire)
- Tire warm-up penalty (post-pit):
  - Soft tires: -1.0 sec lap 1, -0.3 sec lap 2, normal by lap 3
  - Medium: -1.5 sec lap 1, -0.8 lap 2, -0.3 lap 3, normal lap 4
  - Hard: -2.0 sec lap 1, -1.0 lap 2, gradual recovery, normal lap 4-5

**Tire Degradation System** (3-phase model):
- **Phase 1 (Plateau)**: Laps 1-8/15 (depending on track) = stable performance, no wear
- **Phase 2 (Linear Wear)**: Laps increase gradually, lap time loss +0.05-0.10 sec per lap
- **Phase 3 (Cliff)**: Final 2-3 laps = sudden -0.5 to -1.0 sec penalty, must pit before cliff
- **Track-specific examples**:
  - Monaco (high deg): Soft 10-15 laps, Medium 20-25 laps, Hard 35-40 laps
  - Monza (low deg): Soft 25-30 laps, Medium 35-42 laps, Hard 55-65 laps

**Weather Effects**:
- Rain → track temp drops 8-12°C, tire warm-up takes longer, wet tires 1.5-2.5 sec/lap faster
- Light rain: +1.0-2.0 sec lap time penalty (dry tires in light rain)
- Heavy rain: +3.0-5.0 sec lap time penalty (must use wet tires)
- Drying: intermediate tires best during transitions

**Driver Mental Fatigue**:
- Long stints without pit stop → driver concentration decreases → error chance increases
- Driver morale affects risk tolerance (aggressive vs conservative)

**Safety Car & Marshal Rules**:
- Safety car deploys on major crash/incident → pit window disruption
- Yellow flags on sector → no DRS, single file
- Red flags → race stopped, cars in pit lane, resumes later

**Player decisions**:
- Setup (wing, brakes, suspension) before qualifying
- Qualifying strategy (aggressive vs safe fuel load)
- Race pit plan (1-stop? 2-stop? 3-stop?)
- In-race pit timing (call pit when degradation > 0.75 or fuel running low)
- Driver instructions (every lap: push/defend/save/fuel save)
- Weather adaptation (pit for wets? Stay out? Switch to inters?)

**Affects**: Race result (1st-10th points), championship, driver morale, team reputation

**Time scale**: Qualifying ~5 min, Race 15-25 min active (accelerable to 1x/2x/5x/10x/20x speed)

---

### SYSTEM 4: SEASON PROGRESSION (Long-term, 9 months)
**What**: Manage entire 24-race championship season (March-December)

**Season Calendar**:
- **Total duration**: 9-10 months (40-45 weeks, not 237!)
- **24 races** spread across season
- **Inter-race gaps**: 1-3 weeks between races (varies by calendar)
- **Summer break**: 2-3 weeks in August (no races, team maintenance)
- **Pre-season testing**: 3 days in February (before season start)

**Cost Cap System** ($215M Hard Limit):
- Total annual budget: $215 million
- Breakdown: ~40-55% salaries, ~25-30% R&D, ~15-20% operations, ~5-10% contingency
- Enforcement: FIA audits, penalties for exceeding (loss of points, fines, banned upgrades)
- Loopholes: Some costs outside cap (power units for non-manufacturers, hospitality facilities)
- Strategic: teams can shift spending (e.g., defer upgrades to save for driver salaries)

**Sponsorship & Prize Money**:
- Constructors' prize pool distributed: 75% by championship position, 20% heritage bonus, 5% Ferrari legacy
- Sponsorships negotiated annually (bonuses for podiums, wins, championships)
- Example: $50-100M season revenue from sponsorships (varies by team prestige)

**Player decisions**:
- Contract drivers (2 primary, 1 reserve; salaries $1-55M/year)
- Allocate budget: R&D ($108M) vs salaries ($80-120M) vs operations ($20-30M)
- Plan development roadmap (which upgrades for which races)
- Sponsor management (pursue bonuses, maintain relationships, negotiate renewals)
- Team goals (championship? Financial profit? Driver development?)
- Trade-offs: spend now on upgrades OR save for next season? Pay top driver OR invest R&D?

**Season Milestones**:
- **March**: Season starts, 1st race, upgrades deployed
- **April-May**: Early season, establish competitive baseline
- **June-July**: Mid-season development focus, upgrades deployment #2-3
- **August**: Summer break, major R&D decisions for late season
- **September-November**: Final races, prove upgrades work
- **December**: Season finale, awards, contract negotiations for next year

**Affects**: Championship position, financial results, reputation, driver retention

**Time scale**: Seasonal (9-month arcs), monthly budget decisions, weekly management updates

---

### SYSTEM 5: DRIVER MANAGEMENT (Weekly)
**What**: Manage 2 primary drivers + 1 reserve driver (career development, contracts, performance)

**Driver Attributes**:
- **Skill rating**: 0-100 (rookie 40-60, world-class 90+)
- **Morale**: 0-100 (affects pace, aggressiveness, willingness to follow team orders)
- **Mental fatigue**: Increases during long seasons, decreases on break
- **Consistency**: 0-100 (high = predictable pace, low = wild variance)
- **Wet weather rating**: Special skill for rain (some drivers +5, some -5)
- **Experience**: Years in F1 (affects salary, prestige, negotiation power)

**Contract System**:
- Multi-year contracts (1-3 years typical)
- Annual salary: $1M-55M depending on skill and prestige
- Signing bonus: 10-50% of salary
- Bonus clauses: podium bonuses ($100K-500K), championship bonus
- Release clauses: early exit cost
- Risk: driver poaching (rival teams try to sign your driver mid-contract)

**Player decisions**:
- **Driver selection**: hire top driver (expensive, morale issues) vs develop rookie (cheap, slower)
- **Team structure**: Primary driver championship focus vs balanced team vs driver development program
- **Training programs**: invest in simulator (improve skill +1-2/season), coaching sessions
- **Morale management**:
  - Bonus payments ($500K-5M per race) for good performance
  - Assign better engineer (boost morale +10-15)
  - Public criticism (morale -20, but signals frustration to team)
  - Team orders (let faster teammate win, other driver unhappy -15)
- **Contract negotiations**: renewals, salary increases, release clauses
- **Injury management**: driver crashes, injured (sidelined 1-8 races), need reserve driver

**In-Season Driver Dynamics**:
- **Morale changes**: +5 per win, +3 per podium, -5 per DNF, -10 if losing to teammate
- **Performance changes**: morale directly affects pace (-0.5 to +0.5 sec/lap)
- **Rivalry**: teammates compare performance, can escalate to team orders
- **Burnout risk**: long seasons without break → mental fatigue → mistakes increase

**Driver Career Arc** (Multi-season):
- Year 1: Rookie learning, skill +5/season if well-managed
- Year 2-5: Peak performance, incremental improvements
- Year 6+: Veteran status, skill plateaus, can decline if unmotivated

**Affects**: Race performance, championship results, team harmony, sponsorship image

**Time scale**: Weekly morale updates, monthly salary costs, yearly contract negotiations, multi-year career arcs

---

## 3. HOW SYSTEMS INTERCONNECT

### Example: R&D Upgrade affects Race Performance

```
Week 1-4: Aerodynamics center develops new floor design
  Cost: $500K
  Development time: 4 weeks
  Expected gain: +0.3 sec/lap

Week 4 Race: New floor deployed
  Car speed improved by +0.3 sec → move from P5 to P3
  Sponsorship bonus: +$200K for top-5 finish
  Driver morale: +10 (winning is good)

Week 5: Manufacturing center optimizes floor production
  Cost: $100K
  Reliability check: makes sure floor won't fail
  Result: floor reliable for entire season

Week 8 Race: Same floor used, no reliability issues
  Continues to provide +0.3 sec/lap advantage
  Car speed advantage compounds over season
```

### Example: Driver Morale affects Race Performance

```
Week 1: Driver A (skill 85) unhappy
  Lost internal championship to teammate
  Morale: 45/100 (low)
  
Race week:
  Driver A pace penalty: -0.2 sec/lap
  Pit strategy advice: driver ignores team, crashes
  Result: DNF, morale falls to 20

Week 2: Manager handles morale
  Give driver bonus: +$1M
  Assign better engineer: morale +15
  Driver A confidence returns

Race week 2:
  Driver A pace back to normal: 0.0 sec/lap
  Race successfully, morale +10
  Player keeps driver for next contract
```

---

## 4. PLAYER JOURNEY (Full Season = 40-45 weeks, March-December)

### Pre-Season (February, ~3 days)
- Pre-season testing: 3 days, test baseline car performance
- Hire/finalize driver contracts
- Allocate R&D budget to 8 centers
- Plan car development roadmap (which upgrades for which races)
- Set team goals (championship? Financial? Driver development?)

### Season (March-November, 24 races over ~36 weeks)

**Typical 2-week cycle** (inter-race window):
1. **Days 1-3**: Post-race analysis (morale management, medical check-ups, sponsor updates)
2. **Days 4-7**: R&D oversight (check progress on current projects, approve new research)
3. **Days 8-10**: Team management (handle contract negotiations, engineer workload, morale crises)
4. **Days 11-14**: Pre-race preparation (setup planning, driver briefing, tire selection)
5. **Days 15-16**: Race weekend
   - Qualifying: 30 min simulation (player: setup car, pit strategy draft)
   - Race: 15-25 min active gameplay (player: pit timing, driver instructions, weather adaptation)
   - Post-race: 3 min results and celebration/commiseration

**Development Progression**:
- **Week 1-4**: Deploy first round upgrades (small aero tweaks, suspension refinements)
- **Week 5-12**: R&D centers show progress on major projects (floor design, power unit optimization)
- **Week 13-16**: Deploy upgraded components (expect +0.2-0.5 sec/lap gain if successful)
- **Week 17-20**: Mid-season planning (major upgrades or consolidate advantages?)
- **Week 21-28**: Final upgrades deployed (late-season surprise improvements or reliability focus)
- **Week 29-36**: Finish season, evaluate what worked

### Season End (December, ~1-2 weeks)
- Final championship points tallied, awards ceremony
- Financial report (profit/loss, prize money, sponsorship bonuses)
- Evaluate R&D performance (which upgrades worked? Which failed?)
- Driver evaluations (keep/sell/trade drivers)
- Plan next season (contracts, new goals, major changes)

---

## 5. CORE GAMEPLAY LOOPS (By Time Scale)

### DAILY (Simulated, background)
- Driver training sessions (skill +0.1-0.5/day if well-coached)
- Engineer productivity (R&D progress accumulates)
- Facility maintenance costs, energy usage

### INTER-RACE WEEK (Player makes explicit decisions)
- **Morale changes**: Driver morale shifts based on recent race result (+5 win, -5 DNF)
- **R&D progress**: Show milestones on ongoing projects (30% complete, 60% complete, etc.)
- **Team management**: 
  - Hire/fire mechanics based on performance
  - Reassign engineers between centers (workload balancing)
  - Award bonuses to morale-critical staff
  - Handle discipline (driver penalties, engineer disputes)
- **Workload monitoring**: Check if engineers overloaded (>100% workload = mistakes, delays)
- **Sponsor relationships**: Update weekly (morale drops if team performing poorly)
- **Injury updates**: Medical checks on injured drivers, expected return date

### RACE WEEKEND (Active, 20-30 min real-time per race)
- **Friday/Saturday Prep** (5 min):
  - Qualifying: Player sets car setup (wing, brake, suspension, fuel load)
  - Choose aggressive vs conservative qualifying strategy
  
- **Sunday Race** (15-25 min):
  - Formation lap (automatic)
  - Race start (automatic)
  - **Active decision points** every lap:
    - Pit stop timing (when tires degraded or fuel low)
    - Driver instructions (push/defend/save/fuel-save)
    - Weather adaptation (pit for wets? Switch to inters?)
  - Incidents happen (crashes, safety car, mechanical DNF)
  - Final result (points awarded)
  
- **Post-Race** (3 min):
  - Review results, morale impact
  - Driver analysis (who drove well? Who crashed?)
  - Celebrate podium or analyze failure

### MONTHLY (Strategic Planning)
- **R&D Reviews**: Evaluate milestones, check if on track
- **Upgrade Deployment**: If ready, deploy next component upgrade
- **Contract Negotiations**: Discuss salary, bonuses, contract terms with drivers
- **Budget Reallocation**: Move money between centers (e.g., "shift funds from chassis to reliability")
- **Sponsor Updates**: Bonuses claimed, sponsor relationship changes

### SEASONAL (Long-term Arcs, Consequences)
- **Championship Results**: Cumulative points, determines prestige/prize money
- **Team Reputation**: Improved if winning, decreased if losing/DNFing
- **Financial Results**: Profit/loss calculated, affects next-year budget ceiling
- **Sponsor Renewals**: Sponsors decide to continue/exit based on team performance
- **Driver Market**: Top drivers become available if rivals performing poorly
- **Driver Retention**: Own drivers can demand trades/salary increases after strong seasons

---

## 6. WHAT MAKES IT FUN?

### Tension Points (Strategic Decisions Matter)

1. **Limited Budget** ($215M cost cap for 60-80 person team)
   - Can't hire best driver AND fund major R&D upgrade AND pay all engineers
   - Must choose: expensive WDC-caliber driver ($50M) OR invest $30M in R&D OR beef up reliability team
   - Trade-off: win now (hire Verstappen, go for championship) OR build long-term (develop rookies, gradual upgrades)
   - Consequence: every budget decision has 9-month ripple effects

2. **Car Development Uncertainty**
   - Spend 16 weeks + $500K developing new floor design
   - Prediction: +0.3 sec/lap gain
   - Reality: might be +0.1 (disappointing) or +0.5 (amazing) or broken on track (reliability failure)
   - Consequence: R&D is long bet, might not pay off by end of season

3. **Pit Stop Tension** (1.9-2.5 second window, not 5 seconds)
   - Pit now (tires fresh, lose 2 seconds to competitors) OR stay out 2 more laps (tires degrading, might cliff)?
   - Pit stop speed depends on team morale (1.82 sec at 95% morale vs 4.0 sec at 50% morale)
   - Safety car changes pit window entirely (was lap 20, now must pit lap 25)
   - Consequence: pit timing is critical, 2-second difference is huge in competitive race

4. **Driver Morale & Performance**
   - Win with strong driver but cost $55M/year
   - Switch to cheap rookie (save $40M) but lose 1.5 sec/lap
   - Unhappy driver (-20 morale) → -0.5 sec/lap + higher crash risk
   - Consequence: people management is as important as car setup

5. **Weather Adaptation**
   - Track temp 22°C, tires cold, soft compound won't warm up (1.5 sec/lap penalty)
   - Switch to hard tires now? Or wait 3 laps for ambient heating?
   - Rain arriving lap 15? Pit for wets (lose 3-5 seconds) or gamble (risk aquaplaning)?
   - Consequence: every weather change is a strategic fork

6. **Parc Fermé Constraint**
   - Wrong setup choice in qualifying? Stuck with it until Sunday race
   - Can only adjust tire pressure/brake balance by small amounts
   - Consequence: qualifying setup is permanent gamble (aggressive vs safe)

7. **Emergent Moments** (Plans break)
   - Driver crashes lap 3 (car damaged, -1.0 sec/lap, might DNF)
   - Rival's engine failure (championship battle changes)
   - Unexpected safety car (pit window destroyed, must re-plan)
   - Tire blistering in extreme heat (have to pit early)
   - Consequence: player must improvise, rare moment of unscripted challenge

### Reward Loops

- **Win race** → +25 points → championship progress → sponsorship bonus → more budget
- **Develop upgrade** → +0.3 sec/lap → car faster → win more races → reputation
- **Develop driver** → +5 skill over 2 years → faster pace → attract sponsorship
- **Happy team** → +morale → better performance → better results → win more

---

## 7. VICTORY CONDITIONS (Multiple ways to win)

Player chooses season goal at start:

1. **Championship Victory**
   - Goal: Win more points than competitors
   - Reward: prestige, sponsorship, salary boost
   - Challenge: competitors also developing cars

2. **Financial Success**
   - Goal: Maximize profit (revenue - costs)
   - Reward: larger budget next season
   - Challenge: racing costs money

3. **Driver Development**
   - Goal: Take rookie (skill 50) to world class (skill 95)
   - Reward: player can sell driver to other team for profit
   - Challenge: driver might leave for better team

4. **Team Building**
   - Goal: Assemble "dream team" (best drivers + engineers)
   - Reward: powerful team for later seasons
   - Challenge: expensive, time-consuming

5. **Technical Innovation**
   - Goal: Deploy 5 major upgrades in season
   - Reward: technological reputation
   - Challenge: expensive, development time

---

## 8. GAME FEEL (Tone & Pacing)

### Tone
- **Serious**: Real F1 mechanics, realistic consequences
- **Strategic**: Think multiple steps ahead
- **Dynamic**: Plans often fail (weather, crashes, injuries)
- **Social**: Team dynamics matter, not just car speed

### Pacing
- **Slow burn**: Season unfolds over 9 months (60-100 hours play)
- **Race focus**: Every 2-3 weeks is a tense 20-minute race
- **Meaningful consequences**: Decision in Week 5 affects Season 24
- **Replayability**: Different driver, different team structure = different season

---

## 9. SESSION LENGTHS

### Casual Session (30-40 min)
- Play one race (qualifying 5 min + race 15-25 min)
- Make immediate post-race management decisions
- Save and quit

### Medium Session (1.5-2 hours)
- Inter-race management window (1 hour: team decisions, R&D oversight, contracts)
- Play one race (30 min: qualifying + race)
- Brief post-race wrap-up

### Long Session (3-4 hours)
- Complete 2-3 race weekends
- Full inter-race management between each
- Strategic planning updates

### Campaign (60-100 hours)
- Complete full 24-race season (40-45 weeks of gameplay)
- Multiple seasons with career progression
- Example: 24 races × 0.5 hour average = 12 hours racing, + 48 hours management/strategy = 60 hours per season

---

## 10. TARGET AUDIENCE

**Primary**:
- F1 fans (know racing rules, want deep strategy)
- Management game fans (Football Manager, Civilization players)
- Age 16-50, PC/Steam
- Core/Hardcore gamers (willing to read manual, complex systems)

**Secondary**:
- Racing sim fans looking for something different
- Strategy game fans new to motorsports

---

## 11. CRITICAL MECHANICS (Often Overlooked But Essential)

These mechanics create realistic constraints and prevent "optimal solutions":

1. **ATR (Aerodynamic Testing Restrictions)**
   - Championship leader: fewest wind tunnel hours (56 hours/year)
   - Mid-field: 68 hours/year
   - Back-marker: 80 hours/year
   - Constraint: You can't simulate every aero concept, must choose wisely
   - Strategy: Leaders innovate less because resources limited, underdogs catch up

2. **ADUO (Additional Development & Upgrade Opportunities)**
   - Non-title-contending teams get extra development time on major upgrades
   - Catch-up mechanic: prevents runaway leaders
   - Strategic: might be worth NOT winning mid-season if big upgrade almost ready

3. **Cost Cap Enforcement**
   - $215M hard limit
   - FIA audits annually
   - Penalties: loss of points, fines, banned upgrades for next season
   - Loopholes: some costs outside cap (power units, hospitality)
   - Consequence: can't just "spend money to win," budget management is core

4. **Parc Fermé Rule**
   - Car sealed after Q1, no setup changes until race day
   - Can adjust tire pressure/brake balance only (tiny margins)
   - Consequence: qualifying setup choice is permanent, must be conservative or bold

5. **Tire Degradation (3-Phase Model)**
   - Plateau → Linear → Cliff (not smooth linear decay)
   - Different for each compound/track combo
   - Consequence: pit window is not flexible, must pit before cliff (unpredictable)

6. **Dirty Air Effect**
   - Following car loses 5-10% grip due to aerodynamic wash
   - Makes overtaking difficult (realistic)
   - Consequence: track position matters, not just pace

7. **Engine Reliability Trade-offs**
   - Aggressive power mapping (+10 bhp) increases DNF chance by 1-2% per mode
   - Conservative mapping ensures reliability (no power gain)
   - Consequence: push hard and risk DNF, or play safe

8. **MGU-K Energy Management**
   - Harvest kinetic energy on braking (limited by FIA rules)
   - Can deploy +50 kW for ~33 seconds per lap
   - Strategic: save ERS for overtaking or manage throughout lap
   - Consequence: fuel + power management intertwined

9. **Driver Mental Fatigue**
   - Long stints without breaks increase error chance
   - Morale affects willingness to follow team orders
   - Consequence: can't just tell unhappy driver to defend forever

10. **Heritage Bonus in Prize Money**
   - 75% of prize pool by championship position
   - 20% heritage bonus (teams with history get money)
   - 5% Ferrari legacy (Ferrari always gets this)
   - Consequence: new teams need massive performance advantage to overcome financial gap

11. **Engineer Workload & Errors**
   - Each engineer has capacity (100% max)
   - Overload (>100%) causes delays, mistakes in R&D
   - Consequence: can't assign all engineers to all projects simultaneously

12. **Sprint Race Format** (6 races/season)
   - Friday quali → Saturday sprint (17-21 laps, 8 points) → Sunday race (56 laps, 25 points)
   - Total points available: 33/weekend (vs 25 in traditional)
   - Sprint format favors aggressive drivers, risky strategies
   - Consequence: some weekends are higher-stakes, different strategy considerations

---

## 10. KEY DESIGN PILLARS

1. **Strategic Depth**
   - Multiple viable strategies (aggressive vs conservative pit plans)
   - No single "optimal path" (weather changes, accidents disrupt plans)
   - Emergent gameplay (unexpected rain, crashes, engine failures)

2. **Authenticity**
   - Real F1 2026-2027 rules (Cost Cap, ATR, sprint races, tire compounds)
   - Real consequences (DNF costs 25 points, damage -1 sec/lap, morale -0.5 sec/lap)
   - Real drama (rival driver injured, team principal fired, driver trade deadline)

3. **Accessibility with Depth**
   - Easy mode: hide advanced options (ATR, ADUO, engineer workload)
   - Normal mode: full systems, but guided
   - Expert mode: full complexity (every decision visible and impactful)

4. **Replayability**
   - 24 different cars with different pace levels (simulate realistic grid)
   - Weather procedural (random rain, hot days, cold days)
   - Reliability procedural (random DNFs, failures)
   - Career mode (multiple seasons, trade drivers, build team empire)

---

## 12. SYSTEMS SUMMARY TABLE

| System | Player Role | Time Scale | Budget Impact | Typical Duration |
|--------|------------|-----------|---------------|-----------------|
| **Team Management** | Hire/fire, morale, contracts | Weekly decisions | $80-120M salaries | Ongoing (9 months) |
| **R&D (8 Centers)** | Budget allocation, upgrade selection | Monthly planning | $108M R&D budget | 12-24 weeks per upgrade |
| **Race Management** | Setup, pit strategy, driver instructions | Active (per race) | None (included) | 20-30 min per race |
| **Driver Management** | Career development, contracts, mental fatigue | Weekly morale updates | Part of salary budget | Multi-season arcs |
| **Season Progression** | Budget reallocation, sponsor management, team goals | Seasonal planning | $215M cost cap | 40-45 weeks (Mar-Dec) |

---

## 13. INTERCONNECTION EXAMPLES

**Scenario 1: Budget Crunch**
- Spent $30M too much on driver contracts
- Must reduce R&D budget (from $108M to $78M)
- Cancel 2 planned upgrades
- Result: car slower by midseason (-0.4 sec/lap), lose championship battle

**Scenario 2: R&D Payoff**
- Develop new floor over 16 weeks (-$500K)
- Deploy for race 12
- +0.3 sec/lap gain
- Win race 12 (+25 pts, +$1M sponsorship bonus)
- Reputation +10, can attract better drivers next season

**Scenario 3: Driver Morale Crisis**
- Lose championship lead (morale drops -20)
- Driver demands trade (contract clause)
- Release driver to rival (lose team's best car driver)
- Must develop new driver (rookie, 1.5 sec/lap slower)
- Miss championship, but save salary for next season

**Scenario 4: Emergent Race Drama**
- Planned 2-stop strategy
- Rain arrives lap 15 (pit for wets)
- Safety car lap 20 (pit stops stack, destroy pit window)
- Rival crashes lap 28 (championship contention changes)
- Player must adapt on the fly (switch to 1-stop? Stay out? Pit again?)

---

## NEXT DOCUMENT

**File #2: Management Systems Deep Dive**
- Detailed team structure (who manages whom)
- Budget system (cost cap, how money flows)
- Salary & contract mechanics
- Morale system with detailed formulas
- Hiring/firing mechanics
- Inter-team politics (poaching, trades, rivalries)

---

**Document Status**: CORRECTED & COMPLETE  
**Incorporated Feedback**: 
- ✅ Corrected R&D centers (8 actual centers, no "Strategy")
- ✅ Fixed development time (12-24 weeks, not 4-16)
- ✅ Fixed pit stop time (1.9-2.5 sec, not 5 sec)
- ✅ Fixed season length (40-45 weeks, not 237!)
- ✅ Added critical mechanics (ATR, ADUO, Parc Fermé, Sprint format, Cost Cap, tire phases, MGU-K, Dirty Air, engine modes, mental fatigue)
- ✅ Fixed race time (15-25 min, not 30 min)
- ✅ Added engineer workload system
- ✅ Added heritage bonus structure
- ✅ Corrected all terminology (Telemetry & Data vs Data Analysis)

**Ready for**: Approval before File #2 (Management Systems)
