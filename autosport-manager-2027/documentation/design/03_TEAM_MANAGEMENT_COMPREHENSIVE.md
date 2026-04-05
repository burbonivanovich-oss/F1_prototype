# PART 3: TEAM MANAGEMENT SYSTEM

---

## 3.1 TEAM MANAGEMENT OVERVIEW

**Definition**: The Team Management System encompasses all aspects of human resources, infrastructure, and base operations between race events. This is the Medium Loop (Cycle B) that triggers weekly during inter-race windows and determines the team's capacity to compete throughout the season.

### System Architecture

| Component | Details |
|-----------|---------|
| **Factory Staff** | 500-800 employees (manufacturing, assembly, maintenance, logistics) |
| **Race Team** | 60-80 employees (drivers, engineers, mechanics, strategy, operations) |
| **Managed Roles** | Only key positions: 3 Directors, 7 Department Leads, 2 Drivers, 20-25 core engineers |
| **R&D Centers** | 8 specialized departments (Aero, PU, Chassis, Reliability, etc.) |
| **Base Facilities** | 6 core subsystems (wind tunnel, simulator, maintenance, assembly, data, operations) |
| **Cost Cap** | $215M (FIA hard limit, 2025-2026 regulations) |
| **CapEx Allowance** | $45M/3 years (separate from Cost Cap, facility modernization) |
| **Management Cycle** | Weekly, intensifies during inter-race windows |
| **Critical Events** | Driver injuries, key staff departures, morale crises, poaching attempts |

---

## 3.2 ORGANIZATIONAL STRUCTURE

### 3.2.1 Personnel Hierarchy & Headcount

**Race Team size**: 60-80 core employees (directly managed by player)  
**Factory Staff**: 500-800 support employees (abstracted, managed via Team Base Level)

**KEY MANAGED POSITIONS**:

| Category | Count | Role | Salary Range (Annual) |
|----------|-------|------|---------------------|
| **PRIMARY DRIVER** | 1 | Championship contender | $50M - $55M |
| **SECOND DRIVER** | 1 | Support/teammate | $15M - $30M |
| **RESERVE/TEST DRIVER** | 1 | Emergency + development | $1M - $5M |
| **Top Management** | 3 | Exec Directors (Performance/Engineering/Technical) | $800K - $2.5M |
| **Department Leads** | 7 | R&D center managers (varies by director) | $400K - $700K |
| **Race Engineers** | 2 | One per driver, affects setup feedback | $250K - $400K |
| **Factory Engineers** | 8-10 | Specialists, senior technical staff | $200K - $350K |
| **Mid-Level Engineers** | 8-10 | Core workforce (3-10 years) | $120K - $180K |
| **Junior Engineers** | 5-8 | Graduates/interns (0-3 years) | $70K - $100K |
| **Mechanics & Pit Crew** | 8-12 | Pit crew chief + 7-11 specialists | $80K - $150K |
| **Strategy & Ops** | 3-5 | Chief strategist, engineers, coordinators | $150K - $300K |
| **Managers & Support** | 5-8 | Operations, HR, Finance, logistics | $120K - $250K |

**DRIVER SALARY SCALE REFERENCE** (2025-2026):
- **Top Team** (Pace 90+): $50M - $55M/year (max budget realistic)
- **Leaders** (Pace 85-89): $25M - $40M/year
- **Professionals** (Pace 78-84): $10M - $20M/year
- **Rookies/Reserves** (Pace <78): $1M - $5M/year

**ENGINEER SALARY SCALE**:
- **Junior**: $70K - $100K (0-3 years, fresh graduates)
- **Mid-Level**: $120K - $180K (3-10 years, core engineers)
- **Senior**: $200K - $350K (10+ years, specialists)
- **Department Lead**: $400K - $700K (R&D center manager)
- **Executive Director**: $800K - $2.5M (3 top positions)

**Total annual Race Team personnel budget**: $80M - $120M (~40-55% of Cost Cap)  
**Factory Staff costs**: Included in base operations, not individually tracked

### 3.2.2 Decision-Making Hierarchy (THREE-DIRECTOR MODEL)

```
TEAM PRINCIPAL (Player)
    │
    ├─→ DIRECTOR OF PERFORMANCE ($600K-$1.2M)
    │       ├─→ Chief Race Strategist
    │       ├─→ Performance Coach / Driver Development Lead
    │       ├─→ Telemetry Lead
    │       └─→ Pit Crew Chief
    │
    ├─→ DIRECTOR OF ENGINEERING ($700K-$1.4M)
    │       ├─→ Chief Aerodynamicist
    │       ├─→ Chief Structure Engineer (Chassis)
    │       ├─→ Head of Manufacturing Operations
    │       └─→ Simulator Engineering Lead
    │
    ├─→ DIRECTOR OF TECHNICAL ($800K-$1.5M)
    │       ├─→ Chief Power Unit Engineer (Manufacturers only)*
    │       ├─→ Chief Reliability Engineer
    │       └─→ Chief Data Officer
    │
    ├─→ OPERATIONS MANAGER ($300K-$500K)
    │       └─→ Mechanics/logistics
    │
    └─→ HR MANAGER ($150K-$250K)
            └─→ Contract negotiations & recruitment
```

**Note**: *Non-manufacturer teams (Haas, Alfa Romeo, etc.) do NOT have Chief Power Unit Engineer. Instead they report "Engine Integration Lead" ($200K-300K) who manages supplied engines only.

---

## 3.3 EIGHT R&D CENTERS

### 3.3.1 Specialized Department Structure

Instead of abstract "R&D," the system divides into 8 specialized centers, each with distinct budgets, personnel, and development trajectories.

#### **CENTER 1: AERODYNAMICS**

**Lead Role**: Chief Aerodynamicist  
**Team Size**: 6-8 engineers (wind tunnel, CFD, design)  
**Annual Budget**: $42-54M (40-50% of R&D allocation, ~$108M R&D total)  
**Primary Focus**: Bodywork, wings, floor, DRS development  
**Tools**: Wind tunnel (ATR-limited), CFD clusters, design software  
**Technical Constraint**: ATR system (56-80 wind tunnel hours/year, 1120-1840 CFD units/year)  
**Output**: Aerodynamic coefficient improvements (0.1-0.5 sec/lap per major update)
**Strategic Priority**: Highest impact on lap time performance

**Development Trees:**
- **Front Wing**: Downforce vs. drag trade-off, 3 evolution paths
- **Rear Wing**: DRS deployment optimization, 4 endplate designs
- **Floor Package**: Ground effect tuning, undercarriage flow
- **Sidepods**: Cooling inlet design, brake duct efficiency

**Research Phases:**
- Initial concept (2 weeks, $200K)
- Simulation phase (4 weeks, CFD units: 200)
- Wind tunnel validation (12-16 hours testing, 3 weeks, $100K)
- Production design (2 weeks, ready for manufacturing)
- Total: 11-13 weeks per major update, $400K+ investment

---

#### **CENTER 2: POWER UNIT (Engine)**

**FOR MANUFACTURER TEAMS ONLY** (Mercedes, Ferrari, Red Bull, McLaren):

**Lead Role**: Chief Power Unit Engineer  
**Team Size**: 5-6 engineers (engine, ERS, thermal management)  
**Annual Budget**: $32-42M (30-40% of R&D allocation = ~$108M R&D total)  
**Primary Focus**: Engine development, MGU-K optimization, ERS battery management  
**Tools**: Dynamometer testing rig, thermal CFD, endurance test facilities  
**Technical Constraint**: Homologation freeze (major engine specs locked; only point improvements allowed)  
**Output**: Power increase (0.3-0.8 sec/lap per major update), reliability trade-offs

**Development Trees:**
- **Engine Power**: +5-15 bhp achievable per season (within homologation)
- **ERS Efficiency**: MGU-K recovery optimization, deployment mapping
- **Thermal Management**: Cooling system refinement, radiator sizing
- **Fuel Mapping**: Engine mode tuning for qualifying vs. race

**Homologation Rules:**
- Base engine specs frozen at season start
- Only "evolution" improvements allowed (engine code updates)
- Cannot change cylinder count, displacement, or major components
- New power unit spec allowed every 2-3 years (FIA regulation window)

**Cost of Engine Failure:**
- DNF probability increases 1-2% per aggressive power mapping
- Engine failure mid-season is catastrophic (reliability center must compensate)
- Reliability insurance costs: $500K-$2M per season (if purchased)

---

**FOR CUSTOMER TEAMS** (Haas, Alfa Romeo, Kick Sauber, etc.):

**Lead Role**: Engine Integration Lead  
**Team Size**: 2-3 engineers (supplier liaison, integration only)  
**Annual Budget**: $2M - $4M (within Cost Cap)  
**Primary Focus**: Engine installation, ERS system integration with car, software mapping  
**Tools**: Dyno liaison (at supplier), thermal monitoring  
**No Independent R&D**: Cannot develop power units, must accept manufacturer designs
**Power Unit Cost**: ~$15M/year per power unit supplier (OUTSIDE Cost Cap, like real F1)
**Strategy**: Customer teams negotiate supplier terms, can't influence engine development

**Customer Team Options:**
- Option A: Buy from leading manufacturer (e.g., Mercedes) - high cost ($15-18M), best reliability
- Option B: Buy from mid-tier supplier (e.g., Ferrari customer) - mid cost ($12-15M), medium reliability
- Option C: Budget supplier relationship - low cost ($10-12M), reliability risk

---

#### **CENTER 3: CHASSIS & SUSPENSION**

**Lead Role**: Chief Structure Engineer  
**Team Size**: 3-4 engineers (suspension, geometry, materials)  
**Annual Budget**: $16-22M (15-20% of R&D allocation)  
**Primary Focus**: Suspension kinematics, chassis stiffness, weight optimization  
**Tools**: CAD design, FEA analysis, durability test rig  
**Technical Constraint**: None (within 798kg total car weight)  
**Output**: Handling improvements (+/-0.1-0.3 sec/lap), driver morale modifiers

**Development Trees:**
- **Suspension Geometry**: Front/rear camber, toe-in, ride height optimization
- **Spring Rates**: Stiffness tuning for different track types
- **Anti-Roll Bars**: Lateral stiffness adjustment, corner balancing
- **Damper Tuning**: Compression/rebound settings, driver comfort

**Damage & Repairs:**
- Minor damage (curb strike): -0.1 sec/lap, 4 hours repair
- Suspension damage (crash): -0.5 sec/lap, 24+ hours repair, requires spare parts
- Major structural failure: DNF, car written off, $2-5M repair/replacement

---

#### **CENTER 4: RELIABILITY & DURABILITY**

**Lead Role**: Chief Reliability Engineer  
**Team Size**: 3-4 engineers (failure analysis, materials, testing)  
**Annual Budget**: $11-16M (10-15% of R&D allocation)  
**Primary Focus**: DNF prevention, component longevity, failure mode analysis  
**Tools**: Endurance test rigs, failure databases, stress analysis software  
**Technical Constraint**: Direct trade-off with performance (conservative specs = higher reliability)  
**Output**: DNF probability reduction (baseline 2% → optimized 0.5% with heavy investment)

**Reliability Metrics:**
- **Engine Failures**: 0.3-1.5% per race (depends on power tuning aggressiveness)
- **Hydraulic/Electrical**: 0.2-0.8% per race
- **Suspension**: 0.5-1.2% per race (depends on damage/curbing)
- **Overall DNF Risk**: Sum of all failures, baseline 2% without reliability focus

**Cost of Poor Reliability:**
- Each unplanned DNF: -25 points (1st place value), -$500K prize money, -10% team morale
- Reliability crisis (3+ DNFs in 5 races): Chief Reliability Engineer may quit (-15% R&D speed)
- Sponsor dissatisfaction: Reliability targets trigger sponsor penalties ($1-5M contract fines)

**Reliability Investments:**
- Full durability overhaul: $3M, reduces DNF by 0.5-1.0% for remainder of season
- Component redundancy: $500K, specific component gets backup (hydraulics, electrics)
- Materials upgrade (lighter but less durable): $1M, higher risk but 0.2 sec/lap gain

---

#### **CENTER 5: MANUFACTURING & ASSEMBLY**

**Lead Role**: Head of Manufacturing Operations  
**Team Size**: 2-3 engineers + mechanics (CNC machining, assembly quality control)  
**Annual Budget**: $11-16M (10-15% of R&D allocation)  
**Primary Focus**: Component production, assembly quality, part delivery timelines  
**Tools**: CNC machines, robotic assembly arms, quality control systems  
**Technical Constraint**: Production capacity (max 4 complete component sets/week without overtime)  
**Output**: Component delivery speed, quality assurance (low quality → higher failure rates)

**Production Pipeline:**
- Standard part production: 5 days design → 10 days manufacturing → 2 days QC → ready
- Expedited production: +50% cost, -50% time
- Overtime production: +100% cost for workers (affects morale), can sustain 2 weeks max

**Quality Control Modes:**
- **Standard QC**: 85% chance all parts pass; 15% rework required (adds 1-2 days)
- **Strict QC**: +$200K/week cost; 98% pass rate; prevents failures
- **Minimal QC**: -$100K/week cost; 70% pass rate; higher DNF risk

**Bottleneck Scenarios:**
- Multiple R&D teams request parts simultaneously → manufacturing delays
- New aerodynamic package requires custom machining → 2-3 week delays
- Supply chain disruption (fictional): -30% production capacity, requires $500K supplier fee

---

#### **CENTER 6: TELEMETRY & DATA ANALYTICS**

**Lead Role**: Chief Data Officer (CDO)  
**Team Size**: 3-4 engineers (software engineers, data scientists, IT infrastructure)  
**Annual Budget**: $5-7M (3-5% of Cost Cap)  
**Primary Focus**: Real-time telemetry analysis, competitive intelligence, strategy optimization  
**Tools**: Cloud servers, data visualization software, live telemetry feeds from cars  
**Technical Constraint**: None (IT operations unrestricted by FIA)  
**Output**: Race weekend strategic advice quality, competitor analysis depth

**Telemetry Capabilities:**
- **Live Race Monitor**: Real-time fuel consumption, tire degradation curves, competitor gap analysis
- **Qualifying Analysis**: Lap time breakdowns (Sector 1/2/3), engine mode comparison
- **Sector-by-Sector Comparison**: Telemetry vs. best competitors, weakness identification
- **Tire Strategy Modeling**: Predict pit stop windows, undercut/overcut feasibility

**Strategic Intelligence:**
- **Competitor Pace Analysis**: Estimate rival car performance from public data (telemetry, lap times)
- **Driver Benchmarking**: Compare own drivers' performance vs. rival drivers on same track
- **Weather Prediction Integration**: Real-time weather feeds, strategy pivot recommendations
- **Pit Stop Optimization**: Analyze pit crew performance, identify improvement areas

**Impact on Race Performance:**
- Quality of strategic advice during race: affects pit call accuracy (+/- 0.5 sec/lap via strategy)
- Competitor intelligence: affects qualifying setup choices (+/- 0.1 sec/lap from better understanding)

**Data Scientist Skill Levels:**
- Junior data team (Skill 60-70): Basic telemetry, limited competitor analysis
- Mid-level team (Skill 71-85): Advanced analytics, predictive modeling
- World-class team (Skill 86-100): Cutting-edge AI, real-time race simulations, deep competitive insights

---

#### **CENTER 7: BRAKES & THERMAL MANAGEMENT**

**Lead Role**: Brake Systems Engineer  
**Team Size**: 2 engineers (thermal analysis, brake hardware specialists)  
**Annual Budget**: $4-6M (2-3% of Cost Cap)  
**Primary Focus**: Brake system optimization, cooling efficiency, thermal management  
**Tools**: Thermal CFD, brake dyno testing, coolant circuit design  
**Technical Constraint**: Standard supplier (Brembo for all teams); customization only within supplier offerings  
**Output**: Thermal performance improvements, tire degradation mitigation on hot tracks

**Brake Development Trees:**
- **Cooling Ducts**: Optimize brake air intake, radiator positioning
- **Pad Selection**: Hard/soft pad compounds, thermal fade resistance
- **Rotor Geometry**: Ventilation slots, thickness optimization
- **Hydraulic Systems**: Pressure management, brake balance tuning

**Track-Specific Impact:**
- Hot circuits (Monaco, Singapore, Mexico): Poor brake cooling → 0.2-0.5 sec/lap penalty
- Cold circuits (Montreal, Monza): Brake heat less critical, minimal impact
- Brake-heavy circuits (Suzuka, Silverstone): Proper cooling saves 0.1-0.3 sec/lap

**Thermal Management Cascade:**
- If brakes run hot → tire temps increase
- Higher tire temps → faster degradation (1-2 extra pit stops possible)
- Extra pit stops → race strategy complications, potential loss of position

---

#### **CENTER 8: SIMULATOR & DRIVER DEVELOPMENT**

**Lead Role**: Simulator Engineering Lead  
**Team Size**: 2 engineers (software, hardware maintenance)  
**Annual Budget**: $3-5M (2-3% of Cost Cap)  
**Primary Focus**: Driver training, setup testing, sim-to-reality correlation  
**Tools**: Motion rig simulator (6-axis), race simulation software, hardware controllers  
**Technical Constraint**: Limited testing days (10-12 days/year, FIA restrictions during season)  
**Output**: Driver experience gains, pre-race setup preparation, morale benefits

**Simulator Capabilities:**
- **Full-Circuit Simulation**: Lap-by-lap driver training on upcoming tracks
- **Setup Testing**: Drivers test suspension/aero settings before race weekend
- **Wet Weather Training**: Practice rain driving without real-world risk
- **Pit Stop Simulation**: Practice reaction timing, tire change scenarios
- **Damage Scenarios**: React to brake failures, suspension damage mid-race

**Driver Development Mechanics:**
- Young driver (Age 20-24, Skill 65): +5% improvement per week of simulator training
- Mid-career (Age 25-32, Skill 78): +2% improvement per week (diminishing returns)
- Veteran (Age 35+, Skill 82): +0.5% improvement per week (near-plateau)

**Morale Impact:**
- Simulator training: +5% driver morale per session (max 2 sessions/week)
- Poor simulator correlation (real car behaves differently): -10% driver morale
- Successful setup testing (finds good setup on sim): +15% confidence bonus race day

**Simulator Uptime:**
- Baseline: 95% operational reliability
- With maintenance budget: 98% uptime
- Without maintenance: 85% uptime, risk of simulator failure (1-2 week repair)

---

### 3.3.2 Center Interdependencies

All centers are interconnected. Investment in one center requires support from others.

```
AERODYNAMICS ←→ MANUFACTURING (new wings must be produced)
        ↓
    TELEMETRY (measure wing efficiency)
        ↓
    SIMULATOR (drivers test new aero)

POWER UNIT → RELIABILITY (power tuning increases failure risk)
        ↓
    MANUFACTURING (engine component production)
        ↓
    TELEMETRY (monitor engine performance)

CHASSIS → BRAKES (suspension geometry affects brake cooling)
    ↓
    MANUFACTURING (suspension components)
    ↓
    SIMULATOR (drivers learn new suspension feel)

RELIABILITY → ALL CENTERS (failures cascade through entire system)
        ↓
    Must be backstopped by manufacturing capacity, engineer skill, budget
```

---

## 3.4 ENGINEER MANAGEMENT SYSTEM

### 3.4.1 Engineer Attributes

Every engineer has these characteristic dimensions:

| Attribute | Range | Impact |
|-----------|-------|--------|
| **Skill** | 1-100 | Higher = faster R&D, better race day strategy advice |
| **Specialization** | 1-5 disciplines | Cross-functional; performance modified by primary discipline |
| **Experience** | 0-40 years | Increases base skill with age; affects retention probability |
| **Morale** | 0-100% | Below 30% → reduced productivity (-20% R&D speed) |
| **Age** | 22-65 | Peak: 28-45 years; decline after 50 |
| **Ambition** | Low-High | High = greater departure risk if underutilized |
| **Salary** | $60K - $1.2M | Higher salary = lower departure risk; increases budget pressure |
| **Loyalty** | 0-100% | Affects willingness to stay during poor results; built through shared success |

### 3.4.2 Skill Levels & Economic Model (REALISTIC SALARIES)

| Skill Level | Annual Salary | Description | R&D Multiplier | Market Availability |
|------------|---------|-----------|----------|-----|
| **50-60** | $57K-82K | Junior engineer, new graduate | 0.8x | 20+ candidates available |
| **61-70** | $82K-127K | Mid-level engineer, 3-5 years | 1.0x | 10-15 candidates available |
| **71-80** | $127K-203K | Senior engineer, 8-10 years | 1.15x | 3-5 candidates available |
| **81-90** | $203K-317K | Expert/Department Lead, highly specialized | 1.35x | 1-2 candidates available |
| **91-100** | $317K-600K | World-class (ex-Mercedes, Ferrari, McLaren) | 1.6x | 0-1 candidate/year (extremely rare) |

**Salary Scaling Notes**:
- Engineer salaries are dramatically lower than driver salaries (30:1 ratio)
- Top department leads cap at $317K-$317K; executive directors ($600K+) are separate role category
- Skill growth through successful projects applies multiplier (e.g., Skill 75 senior engineer = baseline $165K + 15% bonus = $190K)

### 3.4.3 Engineer Recruitment & Development

**HIRING WINDOW RESTRICTION**:
- Engineer recruitment only available: November - February (off-season)
- Mid-season hiring blocked except for injury/emergency replacement
- This reflects real F1 transfer window timing

**Hiring Process:**
1. Open vacancy in specific R&D center (Nov-Feb only)
2. System generates 3-5 candidates based on team reputation, engineer reputation system, financial offer
3. Player negotiates salary, signing bonus, contract length
4. Candidate accepts/rejects within 2-3 game days

**Hiring Success Factors:**
- Salary +20% above market: +50% probability
- Top-3 championship position: +30% probability
- Team morale 70%+: +15% probability
- Engineer reputation of target candidate: Varies by past success

**Engineer Reputation System**:
- **World-Class Reputation**: +30% interest in offers; only interested if salary >1.5x current
- **Solid Reputation**: +10% interest; salary >1.3x needed
- **Building Reputation**: Standard interest; salary >1.2x needed
- **Unknown/New**: +5% interest; salary >1.1x needed
- Reputation improves by: Leading successful major projects (+5 points), breakthrough innovations (+10 points), staying 3+ seasons (+5 points)
- Reputation damaged by: Failed projects (-3 points), poaching away to rival (-2 points)

**Engineer Development Through Success:**
- Major R&D update success: +2-3 Skill points for lead engineer
- Minor update success: +0.5 Skill
- Failed project: -1 to -2 Skill
- Promotion to Department Lead: +100% salary increase
- Specialized training programs: +3-5 Skill in specialization (costs $80-120K)

**Morale & Turnover Mechanics:**
- High morale (80%+): Turnover risk -50%
- Low morale (40%): Turnover risk +50%
- Rival team poaching attempts monthly for Skill 75+ engineers
- Successful poaching offer: Requires counter-offer at 1.5x rival salary

**GARDENING LEAVE & TRANSFER RESTRICTIONS**:
- **Junior/Mid-Level Engineers**: Can transfer anytime but require 3-month non-compete (can work at supplier/non-F1)
- **Senior Engineers (Skill 71-80)**: Require 6-month gardening leave post-departure; cannot work for direct rivals until completed
- **Department Leads/Specialists (Skill 81+)**: Require 12-month mandatory gardening leave; 
  - Option to buy out: Pay departing engineer 6 months salary to waive 6-month restriction
  - Affects departing team's season competitiveness if allowed to leave mid-season
- Department lead departure: -50% R&D speed for 2-3 weeks until replacement

### 3.4.4 Race Engineers vs Factory Engineers

**RACE ENGINEERS** (One per driver, follows to each race):
- **Role**: Driver's personal technical liaison, setup engineer, telemetry specialist
- **Salary**: $250K - $400K/year (higher due to specialization)
- **Responsibilities**:
  - Manages driver setup feedback (suspension, wings, brake balance)
  - Analyzes telemetry data race-by-race
  - Provides sector-by-sector performance analysis
  - Improves driver adaptation to car (-0.1 to -0.3 sec/lap setup optimization)
  - Pit stop communication and strategy call
- **Impact**: Better race engineer = faster driver adaptation, better qualifying, better racecraft decisions
- **Turnover Risk**: Moderate (drivers may request specific race engineers)
- **Development**: Skill improves with successful performance improvements per race

**FACTORY ENGINEERS** (Based at HQ, work on R&D and testing):
- **Role**: R&D specialists, development, testing, long-term projects
- **Salary**: $70K - $350K/year (depends on seniority)
- **Responsibilities**:
  - Long-term R&D projects (aero, chassis, reliability)
  - Simulator development and driver coaching
  - Component design and validation
  - Pre-season testing and baseline setup development
  - Post-race failure analysis
- **Impact**: Factory engineers directly contribute to performance via R&D updates
- **Turnover Risk**: High for senior engineers (poaching from rivals)
- **Development**: Skill improves with successful R&D project completions

**Organizational Structure Example**:
```
DRIVER #1 CAR
├─ Race Engineer #1 ($350K) → Follows to races, manages setup/telemetry
├─ Factory Engineers (Aerodynamics, Chassis, Reliability):
│   ├─ Senior Aero Specialist ($280K)
│   ├─ Chassis/Suspension Engineer ($200K)
│   └─ Reliability Engineer ($180K)
└─ Test Driver / Simulator Engineer ($150K)

DRIVER #2 CAR
├─ Race Engineer #2 ($300K) → Follows to races
├─ Shared Factory Engineers (above team contributes 40%)
└─ Development/Reserve Driver use
```

**Race Engineer Skill Affects**:
- Driver adaptation speed: +2-4% faster if excellent race engineer
- Setup optimization per race: -0.05 to -0.15 sec/lap
- Pit stop communication clarity: Better strategy decisions
- Morale: +5% if driver satisfied with race engineer

---

### 3.4.5 Engineer Workload System

**Workload Definition**: Number of simultaneous active projects per engineer (no fatigue health mechanic, only productivity/error effects)

**Workload Levels**:
- **Light (1 project)**: Normal R&D speed (100%), error rate baseline (2%)
- **Moderate (2 projects)**: R&D speed 95%, error rate +1% (3%)
- **Heavy (3 projects)**: R&D speed 85%, error rate +2% (4%)
- **Overloaded (4+ projects)**: R&D speed 70%, error rate +4% (6%), morale -10%

**Error Consequences**:
- Errors cause rework: +1-2 weeks delay on affected project
- Major errors (10% of time): Component failure risk during testing, $500K-$2M replacement cost
- Critical errors (rare, 2%): Entire project scrapped, $3-5M loss, -15% morale for team

**Management Options**:
- Max 3 simultaneous projects per engineer recommended (no penalties)
- Hire additional engineers to distribute workload ($70K-350K salary depending on level)
- Extend project timelines to spread work over longer periods
- Outsource non-critical tasks (CAD design, simulation runs) via contractors (-$100K-500K cost)

**Workload Dynamics**:
- Monthly review of engineer assignments
- High morale increases error tolerance (-1% error rate bonus)
- Low morale increases error chance (+2% error rate penalty)
- Seniority reduces error rate: Senior -2%, Mid-level -1%, Junior +0%

**No Health Mechanic**: Engineers don't get "fatigue-sick" or go on mandatory leave. Instead, overwork affects productivity and error chance only (realistic to modern F1 where engineers work long hours year-round)

---

## 3.5 DRIVER CONTRACT MANAGEMENT

### 3.5.1 Three Driver Categories

#### **DRIVER #1 (Primary/Championship Contender)**
- **Salary Range**: $50M - $55M/year (Elite/Star drivers only, realistic max)
- **Expectations**: Top-3 championship, minimum 3 podiums/season
- **Contract Length**: 2-3 years (typically renewed annually for stars)
- **Status**: Gets strategic priority on race days
- **Bonus Structure**:
  - Per-point bonus: $10K per championship point (e.g., 2nd place 18pts = $180K)
  - Win bonus: $500K per race
  - Podium bonus: $250K per podium (P2/P3)
  - Championship bonus: $3-5M if finishes top-3 in championship
  - Sponsor income share: 15-20% of sponsor bonus pool earned by team

#### **DRIVER #2 (Support/Teammate)**
- **Salary Range**: $15M - $30M/year (Professional/Leader drivers)
- **Expectations**: Top-8 finish, minimum 1 podium/season
- **Contract Length**: 2-3 years
- **Role**: Support team strategy, secondary points scorer
- **Bonus Structure**:
  - Per-point bonus: $8K per point
  - Win bonus: $400K
  - Podium bonus: $200K
  - No championship bonus (typical for #2 drivers)
  - Sponsor income share: 8-12% of sponsor bonus pool
- **Promotion Clause**: Can upgrade to #1 if primary injured or underperforming

#### **RESERVE/TEST DRIVER**
- **Salary Range**: $1M - $5M/year (Rookie/development drivers, realistic budget)
- **Role**: Emergency replacement, 2x mandatory FP1 sessions per car per season
- **Contract Length**: 1-2 years
- **Career Path**: Development driver building experience
- **FP1 Mechanics**: 2 mandatory free practice sessions per calendar year to gain experience
- **Activation Cost**: When primary driver injured, reserve steps in immediately (no replacement period)
- **Performance**: -1.0 to -1.5 sec/lap vs primary (due to lower adaptation to car setup)

### 3.5.2 Driver Attributes

| Attribute | Range | Impact |
|-----------|-------|--------|
| **Pace** | 1-100 | Raw speed; 80+ = excellent, 90+ = world-class |
| **Racecraft** | 1-100 | Overtaking, tactical awareness, consistency in races |
| **Error Proneness** | 1-100 | 65+ = safe, 85+ = very safe (inverted from "Reliability") |
| **Consistency** | 1-100 | Lap variance; 70+ = stable qualifying and race |
| **Adaptation** | 1-100 | Learning curve for new cars; crucial for rookies |
| **Age** | 18-42 | Peak: 26-32 years |
| **Experience** | 0-25 years | +3% performance on repeat circuits |
| **Morale** | 0-100% | Affects performance, crash probability |
| **Salary Demand** | $2M - $70M | Adjusts yearly based on performance |
| **Years at Team** | 0-25 | Affects team chemistry, bonus expectations |

**ROOKIE DRIVER ACCELERATION** (First 2 seasons):
- Season 1: +3-7% performance gain per race as adaptation increases
- Season 2: +2-5% performance gain (slower as early curve exhausted)
- Season 3+: +0-1% per season (mature learning rate)

### 3.5.3 Contract Negotiation & Morale

**CONTRACT NEGOTIATION WINDOW**:
- Contract negotiations only during November-February off-season
- Mid-season salary adjustments blocked except for crisis/retention bonus
- Reflects real F1 contract timing (during winter break)

**Annual Contract Renewal:**
- System calculates "Market Value" based on: championship position, podiums, wins, head-to-head vs. teammate, consistency
- Driver presents salary demand (typically +20% to +140% after strong season, -10% to 0% after poor season)
- Player can accept, counter-offer, release driver, or offer retention bonus
- Salary deficit affects morale: -2% per $1M below market
- Salary growth cap: +30% maximum per year (realistic market constraints)

**Morale Impacts**:
- Podium: +15% morale
- Victory: +40% morale
- DNF (own fault): -20% morale
- Outqualified by teammate: -15% morale
- Home race baseline: +10% morale
- Home race podium: +50% morale
- Contract satisfied (good terms): +5% morale
- Salary dissatisfaction: -5% morale per $5M below market

**Morale Performance Consequences**:
- 80%+: +0.1 sec/lap pace bonus, +2% focus consistency
- 60-80%: Baseline (100% performance)
- 40-60%: -0.1 sec/lap pace, +1% crash probability
- 20-40%: -0.3 sec/lap, +3% crash probability, -10% race strategy effectiveness
- <20%: -0.5 sec/lap, +5% crash probability, driver may threaten breach

### 3.5.4 Driver Injuries & Replacement

**Injury Probability**: <0.5% per race baseline
- Increases with aggressive driving instructions: +0.1% per aggressive "Attack" instruction per race
- Decreases with defensive setup: -0.1% per conservative driving instruction
- Concentrated risk: Multiple "Attack" modes in season can trigger injuries more frequently

**Severity Levels:**
- Minor (50%): 1 race absence
- Moderate (35%): 2-3 races absence
- Severe (12%): 4-6 weeks absence
- Critical (3%): 8+ weeks absence

**Injury Impact:**
- Reserve driver activated automatically
- Initial performance: -0.5 to -1.0 sec/lap
- Adaptation time: 2-3 races to reach -0.5 sec/lap
- Lost championship points: 40-60 points typical
- Morale of primary driver: -15% (frustration)

**Prevention & Insurance:**
- Fitness program: $100K/year, -0.2% injury probability
- Injury insurance: $2M/year, covers salary during absence
- Mandatory test vehicle: $500K one-time, reduces reserve adaptation time

---

## 3.6 TEAM MORALE & CULTURE

### 3.6.1 Morale System Architecture

**Three-Tier System:**
- **Individual Morale** (each person): 0-100%
- **Department Morale** (per center): Average of engineers in that department
- **Company Morale** (overall): Average of all staff + recent results weighting

**Company Morale Formula (EXPANDED)**:
```
Company Morale = (Driver morale avg × 0.30) + (Engineer morale avg × 0.30) 
               + (Recent results × 0.15) + (Budget stability × 0.10) 
               + (Sponsor confidence × 0.10) + (Seasonal goal progress × 0.05)

Where:
- Driver morale: Average of both drivers' individual morale levels
- Engineer morale: Average morale across all 7 department leads
- Recent results: Last 3-race average of point finishes (relative to expectation)
- Budget stability: Current cash vs. quarterly budget targets; <80% = penalty, >100% = bonus
- Sponsor confidence: Sponsor KPI progress vs. contract terms
- Seasonal goal progress: Progress toward announced team championship goal
```

### 3.6.2 Morale Consequences Across All Cycles

**Race Performance Impact:**
- Morale 80%+: Pit crew efficiency +2%, pit stops -0.2 sec
- Morale 60-80%: Baseline
- Morale 40-60%: Pit error risk +10%, pit stops +0.1 sec
- Morale 20-40%: Strategy quality -20%, critical mistakes possible

**Personnel Management Impact:**
- Morale 80%+: Recruitment easier (+30%), turnover -50%
- Morale 40-60%: Normal
- Morale <20%: Mass departures risk, difficult hiring

**R&D Development Impact:**
- Morale 80%+: R&D speed +10%, breakthrough probability +5%
- Morale 40-60%: -5% R&D speed, mistakes +10%
- Morale <20%: Projects likely fail, major delays

### 3.6.3 Morale Management Activities

**Team Building Initiatives:**
- Social Event: $50K, +5% morale
- Championship Celebration: $300K (requires win), +20% morale
- Training Camp: $400K, +15% morale
- Awards Program: $100K/year, +8% morale

**Crisis Management:**
- If morale <30%: Emergency meeting required, costs $50K time/resources
- If morale <15%: Risk of resignations (5-10% of team may leave)

### 3.6.4 Driver Development & Performance Coach

**Role**: Performance Coach / Driver Development Lead (Reports to Director of Performance)
- **Salary Range**: $300K - $600K (depending on championship position)
- **Responsibility**: Coaching drivers on racecraft, setup feedback, data analysis, mental performance
- **Qualifications**: Ex-professional driver or sports psychologist (influences hiring)

**Performance Coach Impact**:
- **Adaptation Bonus**: Rookie drivers +1-3% per season in their first 2 years (accelerates learning)
- **Racecraft Development**: Drivers +0.5-1.0% racecraft improvement per season if coach Skill 75+
- **Morale Multiplier**: +5% team morale bonus if coach retained (stability, confidence)
- **Data Utilization**: Coach skill affects telemetry-to-setup translation (+0.05-0.15 sec/lap potential)
- **Driver Chemistry**: Helps reduce tension between drivers (teammate conflicts lower risk)

**Coach Development Path**:
- Hiring: Usually available Nov-Feb, 0-1 candidates per season
- Retention**: Losing coach mid-season costs -15% driver morale impact
- Multiple coaches: Optional second coach for reserve driver development (+$200K salary, focused on young driver prep)

---

## 3.7 BASE INFRASTRUCTURE & FACILITIES

### 3.7.1 Six Core Facility Systems

Each facility has levels 1-10 with upgrades at increasing cost.

#### **FACILITY 1: WIND TUNNEL** (CapEx-funded)
- **Construction Cost**: $100M-$300M (new build, amortized over 4-5 years = $20-60M/year)
- **Operational Cost**: $6-8M/year (staff, cooling systems, maintenance)
- **Constraint**: ATR system limits hours per season (56-80 based on championship position)
- **Efficiency Levels** (1-5, determined by facility age/investment):
  - Level 1: Older facility, 56 ATR hrs/season, accuracy 95%
  - Level 5: State-of-art, 80 ATR hrs/season, accuracy 99%, +5% R&D speed bonus
- **Benefit**: Higher efficiency levels = faster CFD correlation, better aerodynamic gains

#### **FACILITY 2: SIMULATOR** (CapEx-funded)
- **Construction Cost**: $3M-$10M (motion rig, software, hardware)
- **Operational Cost**: $1.5-2M/year
- **Efficiency Levels** (1-3):
  - Level 1: Basic trainer, drivers +3% adaptation
  - Level 3: State-of-art, drivers +7% adaptation, better correlation to real car
- **Training Limitation**: 10-15 days/year depending on facility level
- **Driver Development**: Performance Coach can boost adaptation gains by +1-3%

#### **FACILITY 3: MAINTENANCE BAY**
- **Construction Cost**: $2M-$4M (CapEx amortized)
- **Operational Cost**: $2M/year
- **Efficiency Levels** (1-3):
  - Level 1: Pit stop 2.8 sec, crew errors 2%
  - Level 3: Pit stop 2.0 sec, crew errors 0.2%
- **Impact**: Reduces pit crew errors, improves reliability

#### **FACILITY 4: DYNO TESTING** (Manufacturers only; see Center 2: Power Unit)
- **Construction Cost**: $5M-$15M (engine testing rig, included in CapEx)
- **Operational Cost**: $3-4M/year
- **Dyno Hours**: 200-1000/year depending on factory setup and budget

#### **FACILITY 5: DATA CENTER & TELEMETRY**
- **Construction Cost**: $1.5M-$3M (servers, software)
- **Operational Cost**: $1.5-2M/year
- **Telemetry Channels**: 100-200 channels depending on level
- **Race Impact**: Good telemetry +0.2-0.3 sec/lap advantage via strategy

#### **FACILITY 6: OPERATIONS & LOGISTICS**
- **Construction Cost**: Minimal (office space)
- **Operational Cost**: $2-3M/year (travel, freight, administration)
- **Logistics Efficiency**: Level 1 = 15% delay risk, Level 3 = 1% delay risk

**Total Facility Operational Cost**: ~$12-16M/year within Cost Cap

---

## 3.8 BUDGET MANAGEMENT & COST CAP

### 3.8.1 $215M FIA Cost Cap Breakdown (2025-2026 REGULATIONS)

**COST CAP IN-SCOPE** ($215M):

| Category | Budget | % | Details |
|----------|--------|-----|----------|
| **Personnel (Salary)** | $120M | 56% | Drivers: $105M, Engineering Directors: $2.4M, Staff: $12.6M |
| **R&D Operations** | $40M | 19% | Aerodynamics, Chassis, Reliability, Manufacturing R&D |
| **Power Unit R&D** | $25M | 12% | Engine development, dyno, optimization (manufacturers only) |
| **Facility Operations** | $20M | 9% | Wind tunnel ops, simulator ops, telemetry, maintenance |
| **Logistics & Travel** | $10M | 5% | Freight, travel, accommodation |

**COST CAP EXCLUSIONS** (Outside $215M cap):

| Category | Details |
|----------|---------|
| **Driver Salaries** | $105M (fully excluded from cap) |
| **Technical Director** | $800K-$1.5M (single role, fully excluded) |
| **Marketing & Brand** | Promotional, advertising, sponsorship activation |
| **Legal Fees** | Regulatory, contracts, disputes |
| **FIA Fees** | Entry fees, licenses, testing permits |
| **Logistics** | Freight for prototype parts (separate allocation) |

**CAPEX ALLOWANCE** (Separate budget, $45M/4 years = $11.25M/year):
- Wind tunnel facility upgrades/construction ($100M-$300M amortized)
- Simulator facility improvements ($3M-$10M)
- Headquarters/factory upgrades
- Equipment replacement (CNC, test rigs, servers)

**Cost Cap Enforcement** (STRICT penalties, like real F1):
- Annual audit (end of season) + Quarterly reviews at Races 6, 12, 18
- **0-5% overage**: $5M fine + 5% ATR reduction (lose wind tunnel hours)
- **5-10% overage**: $5-10M fine + 15-25% ATR reduction (significant performance hit) + possible constructor points deduction
- **>10% overage**: $10-15M fine + 25-35% ATR reduction + mandatory constructor points deduction (like Red Bull 2023)
- **Repeat offense** (2+ breaches in 3 seasons): Points deduction + potential championship invalidation

**Audit Mechanics:**
- Teams submit quarterly spending reports
- FIA spot-checks during race weekends
- Independent accountants verify audits
- Discrepancies found → automatic penalty increase 50%
- Fraud discovered → championship void + constructors' ban

### 3.8.2 Spending Dynamics

**Fixed Personnel Costs** (~$100M-120M, locked):
- Driver #1 (Primary): $50M - $55M (realistic top budget)
- Driver #2 (Secondary): $15M - $30M (support role)
- Reserve Driver: $1M - $5M (emergency backup)
- Exec Directors (3): $2.4M - $7.5M (Performance $0.8-2.5M, Engineering $0.8-2.5M, Technical $0.8-2.5M)
- Department Leads (7): $2.8M - $4.9M ($400K-700K each)
- Senior/Race Engineers (10): $2M - $3.5M (specialists)

**Variable Personnel Budget** ($15M):
- Bonuses, hiring, promotions, retention, morale activities
- Strategic allocation determines competitiveness
- Can be reallocated to bonus pools if drivers outperform

**R&D Budget Allocation** ($100-120M total for competitive team):
- Aerodynamics: $42-54M (40-50% - highest priority, most impact)
- Power Unit: $30-42M (30-35%, manufacturers only; client teams get engine supply contract $10-15M OUTSIDE cap)
- Chassis & Suspension: $16-22M (15-20%)
- Reliability & Manufacturing: $11-18M (10-15%)
- Contingency & Other: $5-10M (5-10%)

**For Customer Teams** (no in-house power unit R&D):
- Aerodynamics: $45-50M (50% of their R&D)
- Chassis & Suspension: $20-25M (25%)
- Reliability & Manufacturing: $15-20M (20%)
- Engine Integration: $2-4M (within Cost Cap)
- Engine Supply Contract: $10-15M/year (OUTSIDE Cost Cap, paid to supplier)

**Facility Operations** ($20M):
- Wind tunnel operational costs: $8M
- Simulator operational costs: $4M
- Data center & telemetry: $3M
- Maintenance & misc: $5M

---

## 3.9 QUARTERLY PERSONNEL REPORT

At end of every 6-race cycle, system generates:

```
PERSONNEL REPORT: Races 1-6 Summary
═══════════════════════════════════════════

DRIVERS:
  Car #1: Morale 85%, Adaptation 95%, 78 points
  Car #2: Morale 62%, Adaptation 88%, 34 points
  Reserve: Morale 90%, Ready, 0 points

ENGINEERS (Aggregate):
  Aerodynamics: Morale 78%, R&D output +0.4 sec/lap
  Power Unit: Morale 65%, Reliability: 1 DNF (preventable)
  Chassis: Morale 82%, Setup quality: +10%

TEAM MORALE: 72% (healthy, -10% from Race 3 DNF, +12% from Race 5 podium)

TURNOVER RISK:
  Carlos (#2): Morale 62% → Monitor closely
  Power Unit Lead: Ferrari offer consideration (-40% acceptance if presented)

RECOMMENDATIONS:
  ✓ Morale activity recommended
  ✓ Loyalty bonus to Power Unit Lead ($200K/year)
  ✓ Address Carlos morale (salary negotiation or promotion)
  ✗ Cannot afford to lose Power Unit Lead before mid-season
```

---

## 3.10 CRISIS MANAGEMENT SCENARIOS

### 3.10.1 Injury Crisis
- Driver #1 injured 4-6 weeks
- Reserve activated, -1.0 sec/lap initial
- Cost: $5-8M + 40-60 championship points
- Prevention: Fitness programs, injury insurance

### 3.10.2 Staff Departure Crisis
- Chief Engineer poached by rival
- Development: -90% speed for 1 week, -50% for 2 weeks
- Cost: $1.5M+ replacement + major competitive gap
- Prevention: Loyalty bonuses, career path clarity

### 3.10.3 Manufacturing Bottleneck
- Multiple R&D projects compete for production capacity
- Expedite option: +50% cost, -50% time
- Decision: Which project gets priority?
- Impact: Delayed upgrades cost 0.2-0.4 sec/lap competitive gap

### 3.10.4 Morale Crisis
- If morale <30%: Emergency meeting required
- If morale <15%: Resignations risk, department collapse
- Recovery: +10% morale from positive race result, +20% from victory
- Cost to recover: $200K+ in immediate investments/bonuses

---

**[END OF PART 3: TEAM MANAGEMENT SYSTEM]**

**Total Pages**: 45-50 pages

This comprehensive system covers all aspects of human resource management, facility operations, budget constraints, and crisis resolution. All systems interconnect realistically with the Season Management and R&D systems.

---

**NEXT SYSTEM**: Part 4 — R&D & Vehicle Development System (8 specialized centers, research vs. development phases, token allocation, reliability vs. performance gambling)
