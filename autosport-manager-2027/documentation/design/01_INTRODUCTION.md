# Autosport Manager 2027 - Game Design Document
## PART 1: INTRODUCTION & CONCEPT

**Document Status**: Pre-Production (In Progress)  
**Version**: 1.0 - Foundation Build  
**Last Updated**: April 2026  
**Audience**: Development Team, Stakeholders  

---

## 1.1 ELEVATOR PITCH

**"The Ultimate Motorsports Manager"**

Autosport Manager 2027 is a deep, strategic sports management simulation where players assume the role of Team Principal (CEO) of a professional motorsports team. Build your dynasty across multiple seasons, managing every aspect from driver contracts to engine development, while competing in dynamic championships where every tactical decision matters.

**Core Appeal**: For hardcore motorsports fans and strategy game enthusiasts who want complete control over a team's destiny—not just watching races, but orchestrating championships through intelligent management.

---

## 1.2 HIGH CONCEPT

| Aspect | Details |
|--------|---------|
| **Genre** | Sports Management / Simulation / Strategy |
| **Platform** | PC (Windows/Steam) — V1.0 |
| **Target Audience** | Core-Hardcore (Age 22-55, motorsports fans) |
| **Session Length** | 15-30 hours per season; 200+ hours per career |
| **Estimated Play Sessions** | 60-120 minute sessions (optimal depth) |
| **Single Player / Multiplayer** | Single Player (V1.0); Multiplayer planned for V2.0 |
| **Inspiration** | F1 Manager 2024, Football Manager, iGP Manager 2 |
| **USP (Unique Selling Points)** | • Multi-series support (F1, F2, F3, etc.) <br> • Deeper AI team personalities <br> • Live tactical race management <br> • Emergent narrative through interconnected systems |

---

## 1.3 DESIGN PILLARS (5 Core Principles)

These five principles guide every design decision in Autosport Manager 2027.

### PILLAR 1: DECISIVENESS (Решительность)
**Definition**: Every decision has meaningful consequences. No "perfect" choice exists—only strategic compromises.

**Implementation**:
- Hire top engineer → +15% R&D speed, but salary cuts available budget
- Invest in facility → +5% department efficiency, but -$500K from reserves
- Race conservative strategy → guaranteed points, but lose championship battle
- Push engine performance → +5% speed, but -50% reliability

**Goal**: Players feel the weight of responsibility. Decisions should create emotional investment.

---

### PILLAR 2: EMERGENCE (Эмержентность)
**Definition**: Three interconnected game loops create unpredictable, emergent scenarios. Chain reactions across systems generate unique stories each season.

**Implementation**:
- Crash in Race (Cycle A) → Low morale (Cycle B) → Delayed R&D (Cycle C)
- Hire top engineer (Cycle B) → Unlock new research branches (Cycle C) → Faster car (Cycle A)
- Skip maintenance (Cycle B) → Reliability issues (Cycle C) → Crashes in races (Cycle A)

**Goal**: No two seasons play identically. Complexity emerges from simple interactions.

---

### PILLAR 3: CALCULATED RISK (Расчётливый риск)
**Definition**: Players weigh risk vs. reward in every decision. Spectacular failures and surprising victories create memorable moments.

**Implementation**:
- Gamble on new aerodynamic concept: 60% success, 40% wasted research
- Race aggressive fuel mapping: +3 sec/lap, but 25% chance of DNF
- Sign promising rookie: potential future champion, but might disappoint
- Defer engine upgrade: save money, but lose pace vs. competitors

**Goal**: Create tension and nail-biting moments. Calculated risks should feel thrilling, not reckless.

---

### PILLAR 4: LONG-TERM MASTERY (Долгосрочное мастерство)
**Definition**: Success requires strategic thinking across 200+ gameplay hours. One bad decision doesn't end the game; patience and adaptation matter.

**Implementation**:
- Single bad race = 0-2 lost points, manageable within 23-race season
- Hiring wrong engineer = wasted contracts, learning for next season
- Technology miscalculation = catch-up possible through smart development choices
- Championships built on consistency, not luck

**Goal**: Reward experienced players. Deep systems take time to master, but mastery feels earned.

---

### PILLAR 5: AUTHENTICITY (Аутентичность)
**Definition**: Mechanics reflect real-world motorsports. Players feel like actual team principals, not playing a simplified abstraction.

**Implementation**:
- Pit stop mechanics: realistic 2.0-2.5 second target (vs. fantasy speeds)
- Tire strategies: compound selection, degradation curves based on real data
- R&D timelines: realistic development cycles (not instant upgrades)
- Driver contracts: realistic salary ranges, performance expectations
- Regulatory compliance: DRS, fuel weight limits, engine homologation

**Goal**: Immersion through authenticity. Players think like real team bosses.

---

## 1.4 GAME OVERVIEW: THREE INTERCONNECTED LOOPS

The gameplay revolves around three nested cycles that create depth and emergent complexity:

### **CYCLE A: RACE WEEKEND (Fast Loop - 10-30 minutes)**
**Timescale**: Single race (Friday practice through Sunday race)  
**Player Actions**:
- Analyze track conditions and opponent strategies
- Configure car setup (downforce, fuel load, tire strategy)
- Plan pit stop strategy (tire compounds, gaps, safety margins)
- React to in-race events (crashes, weather, rival moves)
- Monitor race progression via live race monitor

**Feedback/Rewards**:
- Championship points (0-25 per race)
- Prize money ($500K-$15M per race)
- Sponsor satisfaction (contract compliance = $5K-$50K bonuses)
- Driver experience (marginal morale improvement)

---

### **CYCLE B: TEAM MANAGEMENT (Medium Loop - Weekly/Between Races)**
**Timescale**: Off-season, gaps between races  
**Player Actions**:
- **Personnel**: Hire/fire engineers, sign/release drivers, negotiate contracts
- **Infrastructure**: Build/upgrade facilities (wind tunnel, simulator, pit equipment)
- **Finance**: Allocate budget to departments (R&D 40%, Personnel 30%, Operations 20%, Reserve 10%)
- **Crisis Management**: Handle driver injuries, key staff departures, supplier failures

**Feedback/Rewards**:
- Team efficiency metrics (pit stop speed, development velocity)
- Personnel retention (low turnover = stability)
- Financial health (profit/loss per season, sponsor confidence)
- Emerging capabilities (new research options, faster production)

---

### **CYCLE C: R&D & VEHICLE EVOLUTION (Long Loop - Season-Long)**
**Timescale**: Multiple races to entire seasons  
**Player Actions**:
- **Research**: Allocate engineers to study aerodynamics, powertrain, suspension, reliability
- **Development**: Convert research into production car parts
- **Reliability vs. Performance**: Balance top speed with durability (risky gambles)
- **Evolution**: Introduce 4-8 major updates per season (real F1 regulation style)

**Feedback/Rewards**:
- Lap time improvements (0.5-1.5 sec per major upgrade)
- Reliability metric (fewer DNFs = more points)
- Competitive position (relative to rivals' development progress)
- Team morale (successful upgrades boost engineer confidence)

---

## 1.5 HOW THE LOOPS INTERCONNECT

The brilliance of the system lies in how decisions in one loop cascade across others:

| Cycle A Action | → Cycle B Impact | → Cycle C Impact |
|---|---|---|
| Finish in points | +$500K revenue, +morale | Unlock faster research gates |
| Car crash/DNF | -$2M repair cost, -morale | Halt parts production, reassign engineers |
| Set fastest lap | +prestige, +driver morale | Unlock aerodynamic research branch |
| Cycle B Action | → Cycle A Impact | → Cycle C Impact |
| Hire top engineer | Better strategy advice, tactical options | +20% research speed |
| Build wind tunnel | (None directly) | Parts develop 30% faster |
| Poor budget allocation | Reduced competitiveness, worse pit strategy | Delayed R&D, bottleneck |
| Cycle C Action | → Cycle A Impact | → Cycle B Impact |
| Unlock new aero package | +0.5 sec/lap pace | Increased mechanical complexity (more work for crew) |
| Pursue unreliable engine | +8% power, -40% reliability | Risk of major crashes, morale hit |
| Research breakthrough | Competitive advantage for 4 races | Attracts sponsor interest, +$2M revenue |

This interconnection ensures that player actions have felt, visible consequences across the entire team ecosystem.

---

## 1.6 VICTORY CONDITIONS & LONG-TERM GOALS

### PRIMARY VICTORY
**Become World Champion** (Drivers' Championship + Constructors' Championship)
- Lead the championship at season end
- Accumulate points across all 23 races
- Win with multiple viable strategies (not just "build fastest car")

### SECONDARY VICTORIES
- **Legacy Building**: Maintain top-3 position for 5+ consecutive seasons
- **Financial Success**: Achieve $50M+ cumulative profit by end of career
- **Talent Development**: Develop 3 drivers from junior series to world champions
- **Facility Excellence**: Achieve max-level infrastructure (all departments at Level 10)

### LOSS CONDITIONS
- **Bankruptcy**: Operating at -$20M cash reserves
- **Total Reputation Collapse**: All sponsors leave, season-ending sponsors refuse renewal
- **Complete Technical Failure**: 15+ DNFs in single season due to reliability

---

## 1.7 TARGET PLAYER ARCHETYPES

### **THE STRATEGIC MASTER** (Primary)
- Loves complex systems and optimization
- Enjoys long-term planning (multi-season arcs)
- Motivated by outthinking opponents
- Playstyle: Conservative, calculated, data-driven
- Example games: Football Manager, Crusader Kings, Stellaris

### **THE POWER FANTASY** (Secondary)
- Wants to feel in control of a mighty organization
- Enjoys seeing results from decisions
- Motivated by growth and progression
- Playstyle: Aggressive investments, big risks, visible progress
- Example games: Tycoon sims, business sims

### **THE STORYTELLER** (Tertiary)
- Cares about narrative and character arcs
- Enjoys rivalry dynamics and memorable moments
- Motivated by emergent narratives
- Playstyle: Narrative-focused, dramatic decisions, emotional investment
- Example games: Story-driven RPGs, narrative simulations

---

## 1.8 SCOPE STATEMENT (V1.0 - Pre-Production)

### WHAT'S INCLUDED
✅ Full season management system (23 races)  
✅ 3 interconnected game loops  
✅ 10+ team personnel (drivers, engineers, managers)  
✅ Vehicle development system (aero, powertrain, suspension, reliability)  
✅ Financial planning and budget allocation  
✅ Dynamic AI opponent behavior  
✅ Live race monitor (non-playable, text-based or simplified visuals)  
✅ 20 teams × 2 drivers = 40 driveable team configurations  
✅ Multiple championship seasons (3+ season progression)  
✅ Custom team creation / quick-start modes  

### WHAT'S EXPLICITLY NOT INCLUDED (V1.0)
❌ Multiplayer (planned V2.0)  
❌ Dynamic race simulation (AI drivers race; player manages)  
❌ Mobile version (planned V1.5)  
❌ Detailed graphics/animations (UI-focused presentation)  
❌ Cross-platform play (Steam only)  
❌ Licensing (generic teams V1.0; licensing partnerships V1.1+)  

### DEFERRED FEATURES (Post-Launch)
📋 Esports integration  
📋 Streaming overlays  
📋 Mod workshop  
📋 AI-generated custom narratives  
📋 Cross-series management (F1+F2+F3 simultaneous)  

---

## 1.9 DEVELOPMENT PHILOSOPHY

### PILLAR: DEPTH OVER PRESENTATION
We prioritize mechanical depth and systemic complexity over graphical fidelity. A beautiful UI that clearly communicates complex data is our target aesthetic, not AAA graphics.

### PILLAR: EMERGENT > SCRIPTED
We design systems to create emergent stories naturally, rather than scripting narrative moments. A season where your rising star driver crashes out due to your aggressive strategy is more memorable than any pre-written drama.

### PILLAR: ACCESSIBLE COMPLEXITY
New players should be able to play and enjoy the game within 2-3 hours. Complexity reveals itself gradually. We offer difficulty levels and automation options for newer players while preserving depth for veterans.

### PILLAR: DATA-DRIVEN BALANCE
All design decisions informed by metrics: lap times, budgets, reliability percentages. Balancing achieved through tunable parameters, not guess-and-check.

---

## 1.10 SUCCESS METRICS (How We'll Know This Works)

**Design Validation Metrics**:
- Average session length: 60-120 minutes ✓ (target achieved if players commit to this regularly)
- Emergent storytelling: 3+ unique narrative arcs per season ✓ (via decision replay analysis)
- System depth: Player handbook/guide 40+ pages ✓ (complexity captured in documentation)
- Mechanic interdependence: 95%+ of actions affect 2+ systems ✓ (via playtesting analytics)

**Mechanical Balance Metrics**:
- Win distribution: No single strategy dominates (top 5 strategies within 15% of each other)
- Difficulty scaling: Adjustable difficulty ranges from 30% to 120% AI performance
- Player progression: 50-hour mark = player understands 80% of systems
- Replayability: 5+ viable championship strategies exist

---

## 1.11 DESIGN DOCUMENTATION ROADMAP

This GDD is organized as follows:

| Section | Pages | Focus |
|---------|-------|-------|
| **PART 1** | 20 | Concept, Loops, Philosophy |
| **PART 2** | 40 | Game Systems (Team, R&D, Season) |
| **PART 3** | 35 | Mechanics Detail (Contracts, Finance, AI) |
| **PART 4** | 25 | UI/UX & Screen Designs |
| **PART 5** | 15 | Content Specifications |
| **PART 6** | 20 | Balance & Formulas |
| **Total** | ~155 | Complete Game Design |

---

**Next Section**: Part 2 — Game Systems Deep Dive

---

[END OF INTRODUCTION]
