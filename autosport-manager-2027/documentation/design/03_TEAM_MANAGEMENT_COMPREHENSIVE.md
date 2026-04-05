# PART 3: TEAM MANAGEMENT SYSTEM

---

## 3.1 TEAM MANAGEMENT OVERVIEW

**Definition**: The Team Management System encompasses all aspects of human resources, infrastructure, and base operations between race events. This is the Medium Loop (Cycle B) that triggers weekly during inter-race windows and determines the team's capacity to compete throughout the season.

### System Architecture

| Component | Details |
|-----------|---------|
| **Personnel** | 45-60 employees (drivers, engineers, mechanics, administration) |
| **R&D Centers** | 8 specialized departments |
| **Base Facilities** | 6 core subsystems (wind tunnel, simulator, maintenance, assembly, data, operations) |
| **Annual Budget** | $135M (FIA Cost Cap hard limit) |
| **Management Cycle** | Weekly, intensifies during inter-race windows |
| **Critical Events** | Driver injuries, key staff departures, morale crises, poaching attempts |

---

## 3.2 ORGANIZATIONAL STRUCTURE

### 3.2.1 Personnel Hierarchy & Headcount

**Target team size**: 52 full-time employees

| Category | Count | Role | Salary Range (Annual) |
|----------|-------|------|---------------------|
| **Drivers** | 3 | Race drivers (2 primary + 1 reserve/test) | $5M - $25M |
| **Chief Engineer** | 1 | Technical strategy coordinator | $800K - $1.2M |
| **Department Leads** | 7 | R&D center managers | $350K - $550K |
| **Senior Engineers** | 12 | Specialists (10+ years experience) | $200K - $350K |
| **Mid-Level Engineers** | 15 | Core workforce (3-10 years) | $120K - $200K |
| **Junior Engineers** | 8 | Graduates/interns (0-3 years) | $60K - $120K |
| **Mechanics** | 4 | Pit crew & base maintenance | $80K - $140K |
| **Workshop Manager** | 1 | Equipment & logistics lead | $150K - $200K |
| **HR Manager** | 1 | Contracts & personnel management | $100K - $150K |

**Total annual personnel budget**: $25M - $35M (45% of Cost Cap)

### 3.2.2 Decision-Making Hierarchy

```
TEAM PRINCIPAL (Player)
    │
    ├─→ CHIEF ENGINEER (CTO equivalent)
    │       │
    │       ├─→ Aerodynamics Lead
    │       │       ├─→ External bodywork engineer
    │       │       ├─→ Floor/undercarriage engineer
    │       │       └─→ DRS/front wing engineer
    │       │
    │       ├─→ Power Unit Lead
    │       │       ├─→ Engine engineer
    │       │       └─→ Hybrid systems engineer
    │       │
    │       ├─→ Chassis Lead
    │       │       ├─→ Suspension engineer
    │       │       └─→ Brake systems engineer
    │       │
    │       ├─→ Reliability Lead
    │       │       └─→ Durability specialists
    │       │
    │       ├─→ Manufacturing Lead
    │       │       └─→ Assembly/production engineers
    │       │
    │       └─→ Telemetry Lead
    │               └─→ Data analysts
    │
    ├─→ WORKSHOP MANAGER (Operations)
    │       └─→ Mechanics/pit crew
    │
    └─→ HR MANAGER
            └─→ Contract negotiations
```

---

## 3.3 EIGHT R&D CENTERS

### 3.3.1 Specialized Department Structure

Instead of abstract "R&D," the system divides into 8 specialized centers, each with distinct budgets, personnel, and development trajectories.

#### **CENTER 1: AERODYNAMICS**

**Lead Role**: Chief Aerodynamicist  
**Team Size**: 5-6 engineers (wind tunnel, CFD, design)  
**Annual Budget**: $18-22M (14% of Cost Cap)  
**Primary Focus**: Bodywork, wings, floor, DRS development  
**Tools**: Wind tunnel (ATR-limited), CFD clusters, design software  
**Technical Constraint**: ATR system (56-80 wind tunnel hours/year, 1120-1840 CFD units/year)  
**Output**: Aerodynamic coefficient improvements (0.1-0.5 sec/lap per major update)

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

**Lead Role**: Chief Power Unit Engineer  
**Team Size**: 4-5 engineers (engine, ERS, thermal management)  
**Annual Budget**: $20-25M (14-17% of Cost Cap)  
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


**IMPORTANT - Non-Manufacturer Teams**: Client teams (Haas, Alfa Romeo, etc.) do NOT have independent Power Unit R&D centers. Instead, they purchase complete power units from manufacturers and have only a small "Engine Integration" department (2-3 engineers).

---

#### **CENTER 3: CHASSIS & SUSPENSION**

**Lead Role**: Chief Structure Engineer  
**Team Size**: 3-4 engineers (suspension, geometry, materials)  
**Annual Budget**: $12-15M (8-10% of Cost Cap)  
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
**Team Size**: 3 engineers (failure analysis, materials, testing)  
**Annual Budget**: $8-12M (5-8% of Cost Cap)  
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
**Annual Budget**: $6-8M (4-5% of Cost Cap)  
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

### 3.4.2 Skill Levels & Economic Model

| Skill Level | Annual Salary | Description | R&D Multiplier | Market Availability |
|------------|---------|-----------|----------|-----|
| **50-60** | $60K-90K | Junior engineer, new graduate | 0.8x | 20+ candidates available |
| **61-70** | $90K-140K | Mid-level engineer, 3-5 years | 1.0x | 10-15 candidates available |
| **71-80** | $140K-200K | Senior engineer, 8-10 years | 1.15x | 3-5 candidates available |
| **81-90** | $200K-350K | Expert level, highly specialized | 1.35x | 1-2 candidates available |
| **91-100** | $350K-600K | World-class (ex-Mercedes, Ferrari, McLaren) | 1.6x | 0-1 candidate/year (extremely rare) |

### 3.4.3 Engineer Recruitment & Development

**Hiring Process:**
1. Open vacancy in specific R&D center
2. System generates 3-5 candidates based on team reputation, financial offer
3. Player negotiates salary, signing bonus, contract length
4. Candidate accepts/rejects within 2-3 game days

**Hiring Success Factors:**
- Salary +20% above market: +50% probability
- Top-3 championship position: +30% probability
- Team morale 70%+: +15% probability
- Recent race win: +15% probability

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
- Department lead departure: -50% R&D speed for 2-3 weeks until replacement

---

## 3.5 DRIVER CONTRACT MANAGEMENT

### 3.5.1 Three Driver Categories

#### **DRIVER #1 (Primary/Championship Contender)**
- **Salary Range**: $10M - $25M/year
- **Expectations**: Top-3 championship, minimum 3 podiums/season
- **Contract Length**: 3-5 years
- **Status**: Gets strategic priority on race days
- **Bonuses**: Win +$500K, Podium +$250K, Points +$50K, Fastest Lap +$100K

#### **DRIVER #2 (Support/Teammate)**
- **Salary Range**: $3M - $8M/year
- **Expectations**: Top-8 finish, minimum 1 podium/season
- **Contract Length**: 2-3 years
- **Role**: Support team strategy, secondary points scorer
- **Promotion Clause**: Can upgrade to #1 if primary injured

#### **RESERVE/TEST DRIVER**
- **Salary Range**: $200K - $800K/year
- **Role**: Emergency replacement, simulator work
- **Contract Length**: 1-2 years
- **Career Path**: Development driver building experience

### 3.5.2 Driver Attributes

| Attribute | Range | Impact |
|-----------|-------|--------|
| **Pace** | 1-100 | Raw speed; 80+ = excellent, 90+ = world-class |
| **Racecraft** | 1-100 | Overtaking, tactical awareness |
| **Reliability** | 1-100 | Crash avoidance; 65+ = safe, 85+ = very safe |
| **Consistency** | 1-100 | Lap variance; 70+ = stable |
| **Adaptation** | 1-100 | Learning curve for new cars |
| **Age** | 18-42 | Peak: 26-32 years |
| **Experience** | 0-25 years | +3% performance on repeat circuits |
| **Morale** | 0-100% | Affects performance, crash probability |
| **Salary Demand** | $200K - $25M | Adjusts yearly +/-20% based on performance |

### 3.5.3 Contract Negotiation & Morale

**Annual Contract Renewal:**
- System calculates "Market Value" based on championship position, podiums, wins, head-to-head results
- Driver presents salary demand (typically +20% to +140% after strong season)
- Player can accept, counter-offer, or release driver
- Salary deficit affects morale: -2% per $1M below market

**Morale Impacts:**
- Podium: +15% morale
- Victory: +40% morale
- DNF (own fault): -20% morale
- Outqualified by teammate: -15% morale
- Home race: +15% morale baseline
- Home race podium: +60% morale

**Morale Consequences:**
- 80%+: +0.1 sec/lap pace bonus
- 60-80%: Baseline
- 40-60%: -0.1 sec/lap pace, +1% crash probability
- 20-40%: -0.3 sec/lap, +3% crash probability
- <20%: Driver may breach contract, loss imminent

### 3.5.4 Driver Injuries & Replacement

**Injury Probability**: 0.8% per race

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

**Company Morale Formula:**
```
Company Morale = (Driver morale avg × 0.4) + (Engineer morale avg × 0.4) + (Recent results × 0.2)
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

---

## 3.7 BASE INFRASTRUCTURE & FACILITIES

### 3.7.1 Six Core Facility Systems

Each facility has levels 1-10 with upgrades at increasing cost.

#### **FACILITY 1: WIND TUNNEL**
- **Baseline Cost**: Existing facility
- **Upgrade Cost**: $2M-5M per level
- **Operational Cost**: $400K/month
- **Constraint**: ATR system limits hours per season (56-80 based on championship)
- **Benefit**: Level 10 allows faster testing, better accuracy, wind tunnel hours scaling

#### **FACILITY 2: SIMULATOR**
- **Baseline Cost**: Existing facility
- **Upgrade Cost**: $1.5M-3M per level
- **Operational Cost**: $200K/month
- **Benefit**: Level 1 drivers +3% adaptation, Level 10 drivers +12% adaptation per session
- **Training Limitation**: 10-15 days/year depending on facility level

#### **FACILITY 3: MAINTENANCE BAY**
- **Baseline Cost**: Existing facility
- **Upgrade Cost**: $1M-2M per level
- **Operational Cost**: $150K/month
- **Pit Stop Performance**: Level 1 = 2.8 sec, Level 10 = 2.0 sec
- **Reliability**: Better maintenance reduces pit crew errors

#### **FACILITY 4: ENGINE FACILITY**
- **Baseline Cost**: Existing facility
- **Upgrade Cost**: $3M-6M per level
- **Operational Cost**: $300K/month
- **Testing Capacity**: Level 1 = 200 dyno hours/year, Level 10 = 1000 hours/year
- **Engine Production**: Level 1 = 2 new engines/season, Level 10 = 6 per season

#### **FACILITY 5: DATA CENTER**
- **Baseline Cost**: Existing facility
- **Upgrade Cost**: $1.5M-2.5M per level
- **Operational Cost**: $100K/month
- **Channels**: Level 1 = 50 channels, Level 10 = 200 channels
- **Race Impact**: Good telemetry +0.3 sec/lap advantage through strategy

#### **FACILITY 6: OPERATIONS CENTER**
- **Baseline Cost**: Existing facility
- **Upgrade Cost**: $800K-1.5M per level
- **Operational Cost**: $80K/month
- **Logistics Impact**: Level 1 = 15% delay risk, Level 10 = 1% delay risk
- **Budget Efficiency**: Level 10 saves $5.8M/year in administrative overhead

**Total Baseline Operational Cost**: ~$1.4M/month ($16.8M/year)

---

## 3.8 BUDGET MANAGEMENT & COST CAP

### 3.8.1 $135M FIA Cost Cap Breakdown

| Category | Budget | % | Details |
|----------|--------|-----|----------|
| **Personnel** | $45M | 31% | Drivers: $25M, Engineers: $15M, Support: $5M |
| **Power Unit** | $20.3M | 14% | R&D, dyno, supplier fees |
| **Chassis & Aero** | $30M | 21% | Combined R&D, manufacturing |
| **Operations** | $20M | 14% | Logistics, travel, facility ops |
| **Contingency** | $20M | 14% | Emergency repairs, unexpected costs |
| **IT & Data** | $10M | 7% | Servers, software, telemetry |

**Cost Cap Enforcement:**
- Quarterly audits (25%, 50%, 75%, 100% spending targets)
- 0-5% overage: $500K fine + warning
- 5-10% overage: $2M fine + -25 constructors' points + 4-race wind tunnel ban
- >10% overage: $5M fine + -50 points + potential disqualification

### 3.8.2 Spending Dynamics

**Fixed Personnel Costs** (~$32.5M):
- Drivers: $28M (salaries locked)
- Chief Engineer: $900K
- Department Leads: $2.8M
- Support staff: $800K

**Variable Personnel Budget** ($12.5M):
- Bonuses, hiring, promotions, retention
- Strategic allocation determines morale management flexibility

**R&D Budget Allocation** ($60.3M total):
- Aerodynamics: $18-22M (30-37%)
- Power Unit: $15-20M (25-33%)
- Chassis: $10-15M (17-25%)
- Reliability: $8-12M (13-20%)
- Manufacturing: $6-8M (10-13%)
- Other: $8-12M

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
