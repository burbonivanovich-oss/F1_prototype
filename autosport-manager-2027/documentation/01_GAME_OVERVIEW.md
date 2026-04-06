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
**What**: Manage 60-80 employees (drivers, engineers, mechanics, strategists)

**Player decisions**:
- Hire/fire staff (drivers, chief engineer, race strategist)
- Negotiate salaries (cost cap limited)
- Set team structure (who reports to whom)
- Handle morale crises (driver unhappy, engineer wants to leave)
- Manage injuries/medical leave

**Affects**: Race performance (good engineers = better setups, morale = driver aggression)

**Time scale**: Weekly updates, player makes decisions during "Tuesday afternoon" screens

---

### SYSTEM 2: R&D / VEHICLE DEVELOPMENT (Weekly/Monthly)
**What**: Research and develop car components (8 research centers)

**8 R&D Centers**:
1. **Aerodynamics** — wing designs, floor, DRS optimization
2. **Power Unit** — engine power, ERS efficiency (manufacturer teams only)
3. **Chassis** — suspension, geometry, materials
4. **Reliability** — durability, failure prevention
5. **Manufacturing** — production efficiency, quality
6. **Simulation** — CFD, wind tunnel usage allocation
7. **Data Analysis** — telemetry, performance optimization
8. **Strategy** — pit strategy algorithms, race prediction

**Player decisions**:
- Allocate budget to each center ($50M+ budget per year)
- Choose which upgrades to develop (e.g., "new floor design")
- Prioritize: long-term (complex, slow) vs short-term (simple, fast) projects
- Trade-off: cost vs performance vs reliability

**Affects**: Car speed (0.1-0.5 sec/lap per upgrade), reliability (DNF chance)

**Time scale**: 4-16 weeks per major upgrade, monthly milestones

---

### SYSTEM 3: RACE MANAGEMENT (Race weekend)
**What**: Manage single race (qualify + race)

**Player decisions**:
- Car setup (wing angle, brake balance, tire pressure, fuel load)
- Strategy (1-stop vs 2-stop vs 3-stop pit plan)
- Driver instructions (during race: push/defend/save tires)
- Tire choice (soft/medium/hard, when to pit)
- Pit crew management (which mechanics work pit stop)

**Affects**: Race result, championship points, driver morale, sponsor satisfaction

**Time scale**: Active 20-30 minutes per race

---

### SYSTEM 4: SEASON PROGRESSION (Long-term)
**What**: Manage entire 9-month season (24 races)

**Player decisions**:
- Contract drivers (salaries, contract length)
- Plan development roadmap (which upgrades for which races)
- Budget allocation (R&D vs salaries vs facilities)
- Team goals (championship, highest paid driver, best engineer team)
- Sponsor management (fulfill bonuses, maintain relationships)

**Affects**: Team reputation, financial stability, driver retention

**Time scale**: Monthly planning, season-long consequences

---

### SYSTEM 5: DRIVER MANAGEMENT (Weekly)
**What**: Manage 2-3 drivers (primary, second, reserve)

**Player decisions**:
- Driver development (training programs, confidence building)
- Driver assignment (who races in which car)
- Team orders (Hamilton faster, tell Sainz to defend/let pass)
- Contract negotiations (sign, renew, release)
- Morale management (bonus for good race, discipline for mistakes)

**Affects**: Driver performance (10-20 year career development), team harmony

**Time scale**: Weekly morale updates, multi-year career arcs

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

## 4. PLAYER JOURNEY (Full Season)

### Pre-Season (Weeks 1-4)
- Hire drivers, engineers, mechanics
- Allocate R&D budget to projects
- Plan car development roadmap
- Set team goals (win championship? Earn money?)

### Season (Weeks 5-230)
- **Week 1-3**: R&D progress, team management, prepare strategy
- **Week 4**: Race weekend (active)
  - Qualifying: setup car, player makes decisions
  - Race: 20 min active, pit stops, driver instructions
  - Results: analysis, points, prize money
- **Week 5**: Manage fallout (injuries, morale, sponsor complaints)
- **Repeat** for 24 races

### Season End (Weeks 231-237)
- Final championship points, celebrate/mourn
- Fire/rehire drivers for next season
- Evaluate R&D performance (did upgrades work?)
- Plan next season

---

## 5. CORE GAMEPLAY LOOPS (By Time Scale)

### DAILY (Implicit, not shown in UI)
- Driver training effects
- Engineer productivity
- Facility maintenance

### WEEKLY (Explicit, player makes decisions)
- Driver morale changes
- Sponsor relationship changes
- R&D progress updates
- Team management decisions (hire/fire, reassign roles)

### RACE WEEKEND (Active gameplay)
- Qualifying (player: setup car)
- Race (player: pit strategy, driver instructions)
- Post-race (results, morale impact)

### MONTHLY (Planning & Strategy)
- R&D milestone reviews
- Major upgrade deployments
- Contract negotiations with drivers
- Budget reallocation

### SEASONAL (Long-term Consequences)
- Championship results
- Team reputation changes
- Financial results (profit/loss)
- Sponsor renewal/loss
- Driver retention/departure

---

## 6. WHAT MAKES IT FUN?

### Tension Points (Player wants to optimize)

1. **Limited Budget** ($215M cost cap)
   - Can't do everything
   - Must choose: expensive driver OR expensive R&D?
   - Consequence: long-term pain for short-term gain

2. **Car Development Trade-offs**
   - New floor = faster but unreliable?
   - Spend time testing or risk DNF?
   - Consequence: race-day surprises

3. **Driver Morale & Performance**
   - Best driver expensive + unhappy
   - Cheap driver fast but inconsistent
   - Consequence: unpredictable race results

4. **Race Day Tactics**
   - Pit now (lose 5 seconds) or stay out (risk tire cliff)?
   - Push driver hard (win race but crash risk)?
   - Consequence: every pit stop is decision point

5. **Emergent Moments**
   - Driver crashes mid-race (damaged car, DNF)
   - Safety car changes pit window
   - Unexpected rain (tire strategy fails)
   - Consequence: player must adapt on the fly

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

### Casual Session (20-30 min)
- Play one race + qualifying
- Make post-race management decisions
- Save and quit

### Medium Session (1-2 hours)
- Plan strategy for next 2-3 races
- Manage R&D priorities
- Play 1-2 races

### Long Session (3-5 hours)
- Play 3-5 races consecutively
- Full season management update
- Multiple team decisions

### Campaign (20-100 hours)
- Complete full season
- Multiple seasons with career progression

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

## 11. KEY DESIGN PILLARS

1. **Strategic Depth**
   - Multiple viable strategies (aggressive vs conservative)
   - Decisions matter (no "optimal path")
   - Emergent gameplay (plans often break)

2. **Authenticity**
   - Real F1 mechanics (pit stops, tire compounds, cost cap)
   - Real consequences (DNF, reliability, financial impact)
   - Real drama (comebacks, upsets, rivalries)

3. **Accessibility**
   - Tutorial explains basics
   - Advanced features can be ignored
   - Settings for difficulty/complexity

4. **Replayability**
   - Many viable strategies
   - Procedural elements (weather, reliability)
   - Career mode (10+ seasons possible)

---

## 12. SYSTEMS SUMMARY TABLE

| System | Player Role | Time Scale | Budget Impact | Fun Factor |
|--------|------------|-----------|---------------|-----------|
| **Management** | Hire/manage team | Weekly | Salaries | People management |
| **R&D** | Choose upgrades | Monthly | $50M+/year | Long-term planning |
| **Race** | Pit strategy, setup | Race day | None direct | Tactical tension |
| **Drivers** | Train, motivate | Weekly | Salaries + bonuses | Relationship building |
| **Season** | Overall strategy | 9 months | Cost cap | Long-term rewards |

---

## NEXT DOCUMENT

**Part 2: Management Systems Architecture**
- Team structure (who manages whom)
- Budget system (cost cap, how money works)
- Hiring/firing mechanics
- Morale system details

---

**Document Status**: READY FOR REVIEW  
**Next Step**: Approve vision, then move to Management Systems details

Would you like me to adjust ANYTHING in this overview before I write the Management Systems document?
