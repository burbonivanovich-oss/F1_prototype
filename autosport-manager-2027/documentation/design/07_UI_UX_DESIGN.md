# PART 7: UI/UX DESIGN

**Comprehensive Interface Design for Autosport Manager 2027**
**Aligned with F1 2025-2026 Regulations**

---

## 7.1 DESIGN PHILOSOPHY

**Core UX Principles:**

1. **Information Density**: Deep complexity accessible through progressive disclosure
2. **Real-Time Feedback**: Immediate visual confirmation of all actions
3. **Visual Hierarchy**: Championship > Drivers > Performance gaps
4. **Context-Sensitive**: UI adapts to season phase (pre-season/in-season/post-race)
5. **Color Coding**: Red=critical, Yellow=warning, Blue=info, Gray=neutral

**Modern F1 Aesthetic**:
- Dark theme (black/dark gray backgrounds)
- Bright accents (FIA red, team colors)
- Clean typography, minimalist icons
- **150-200ms smooth transitions** (optimized for manager responsiveness)
- Optional instant mode (toggle for experienced players, no animations)
- Mobile-responsive (desktop primary, tablet secondary)
- Micro-interactions: hover states, click feedback, loading spinners

**2025-2026 Regulations Implementation**:
- ✅ Cost Cap $215M (not $135M)
- ✅ Three-tier prize distribution ($1.6B total)
- ✅ ATR sliding scale (70-115% by position)
- ✅ 6 tire compounds with track-dependent life
- ✅ Realistic pit stop times (1.82-2.5 sec)
- ✅ No technology token system (budget-based R&D)
- ✅ Minimum weight 768kg (not 798kg)
- ✅ No fastest lap bonus (removed 2025)
- ✅ ADUO for power units only (3 checks/season)
- ✅ CapEx allowance separate from Cost Cap

---

## 7.2 MAIN DASHBOARD

**At-a-glance team status and upcoming priorities**

**Layout**: Tabbed interface with 5-7 primary widgets maximum per tab to avoid cognitive overload.

### Primary Tab: "OVERVIEW" (Always Visible)

Max 5 core widgets:

| Widget | Data | Update Frequency |
|--------|------|------------------|
| **Championship Position** | Current standing, points gap to leader | Real-time |
| **Cash Position** | Available budget, quarterly forecasts | Weekly |
| **Team Morale** | Overall %, key factors (results/sponsorships) | Race-by-race |
| **Upcoming Race** | Next GP, date, track, weather forecast | Session-by-session |
| **Quick Alerts** | Critical items: contract ending, KPI at risk, engine resource low | Real-time |

### Secondary Tabs (Accessible but not overwhelming)

**TAB: "R&D & RELIABILITY"**
| Widget | Purpose |
|--------|---------|
| **R&D Status** | Wind Tunnel hrs, CFD units, Active projects count |
| **Reliability Standing** | Engine resource (X/8 used), DNF risk, power unit aging |
| **Active Projects** | Overview of 3-5 current development projects |

**TAB: "TEAM & DRIVERS"**
| Widget | Purpose |
|--------|---------|
| **Driver Performance** | Both drivers: pace rating, morale, contract status |
| **Department Heads** | 7 leads summary: morale, retention risk, salary info |
| **Pit Crew Status** | Crew morale, pit stop accuracy avg, training focus |

**TAB: "FINANCE & SPONSORS"**
| Widget | Purpose |
|--------|---------|
| **Cost Cap Health** | $215M limit, current usage %, quarter breakdown |
| **Sponsor Portfolio** | KPI progress per sponsor, risk indicators |
| **Prize Money Projection** | Estimated Q4 payment, season-end forecast |

### Quick Action Cards (Overview Tab)

Three primary action cards for fast access to frequent tasks:

```
┌─────────────────────────────────┐
│ 🔧 TEAM MANAGEMENT              │
│ 3 Directors | 7 Department Heads │
│ Team Morale: 76% (+2% race)      │
│ Retention Risk: 1 MEDIUM 🟡      │
│ [T] VIEW | [G] MANAGE            │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│ 📊 R&D PLANNING                 │
│ Wind Tunnel: 24/72 hrs (33%)     │
│ CFD Units: 380/1400 (27%)        │
│ Active Projects: 3               │
│ [R] PROJECTS | [Ctrl+R] QUICK   │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│ 💰 BUDGET REVIEW                │
│ Cost Cap: $95M / $215M (44%)     │
│ Cash Reserve: $12.3M             │
│ Next Payment: $28M (Dec)        │
│ [B] BUDGET | [Ctrl+B] FORECAST   │
└─────────────────────────────────┘
```

**Keyboard Shortcuts** (shown in tooltips, can be customized):
- `T` = Team Management
- `R` = R&D Planning  
- `B` = Budget / Finance
- `Ctrl+T`, `Ctrl+R`, `Ctrl+B` = Quick filter toggle
- `Escape` = Back to Overview

---

## 7.3 TEAM MANAGEMENT INTERFACE

### Organizational Structure View

**Three-Director Hierarchy** (Not traditional CTO model):

```
TEAM PRINCIPAL
├─ DIRECTOR OF PERFORMANCE ($600K-$1.2M)
│  ├─ Chief Race Strategist
│  ├─ Performance Coach / Driver Development Lead
│  ├─ Telemetry Lead
│  └─ Pit Crew Chief
├─ DIRECTOR OF ENGINEERING ($700K-$1.4M)
│  ├─ Chief Aerodynamicist
│  ├─ Chief Structure Engineer (Chassis)
│  ├─ Head of Manufacturing Operations
│  └─ Simulator Engineering Lead
├─ DIRECTOR OF TECHNICAL ($800K-$1.5M)
│  ├─ Chief Power Unit Engineer (Manufacturers only)*
│  ├─ Chief Reliability Engineer
│  └─ Chief Data Officer
├─ Operations Manager ($300K-$500K)
└─ HR Manager ($150K-$250K)

* Non-manufacturer teams: "Engine Integration Lead" ($200K-$300K) instead
```

### Department Heads Screen

**7 Key Leaders Tracked**:
- Chief Aerodynamicist (Aerodynamics research lead)
- Chief Power Unit Engineer (Engine/ERS development)
- Chief Structure Engineer (Chassis/suspension)
- Chief Reliability Engineer (Durability focus)
- Head of Manufacturing (Production schedule)
- Chief Data Officer (Telemetry/strategy)
- Simulator Engineering Lead (Driver development)

**Per-Head Display**:
```
┌─────────────────────────────────┐
│ Chief Aerodynamicist            │
│ Skill: 82/100 | Age: 47         │
│                                 │
│ Morale: 78% ▓▓▓▓▓░░░░ -5% DNF  │
│ Loyalty: 62% ▓▓▓░░░░░░░░       │
│ Salary: $245K/year              │
│ Contract: 2024-2026 (expires)   │
│ Retention Risk: MEDIUM 🟡       │
│                                 │
│ [RAISE SALARY] [BONUS] [RETAIN] │
└─────────────────────────────────┘
```

### Team Base Level

**Abstract representation of supporting 45-60 engineers**:
- Base Level: 1-100 scale
- Affects: R&D speed, quality, morale
- Improves via: Hiring reputation, successful projects, morale activities
- Degrades via: Key departures, failed projects, low morale

### Driver Management Screen

**Driver Salary Scales**:
| Tier | Pace Range | Salary Range | Examples |
|------|-----------|--------------|----------|
| Elite/Stars | 90+ | $50M - $70M | Verstappen, Leclerc |
| Leaders | 85-89 | $25M - $40M | Norris, Hamilton |
| Professionals | 78-84 | $10M - $20M | Mid-grid drivers |
| Rookies | <78 | $2M - $8M | Newcomers/reserves |

**Injury Insurance Option** ($2-3M/season):
- ✅ Covers driver salary during absence
- ✅ Covers 75% of lost bonus pool
- ✅ Reduces championship impact from injuries
- Display: [PURCHASE INSURANCE] button with cost/benefit analysis

### Pit Crew Management Interface

**New Screen**: Crew composition and training between races

```
┌──────────────────────────────────────────────┐
│ PIT CREW MANAGEMENT                          │
│                                              │
│ Pit Crew Chief: Marcus (Skill 84, Morale 71%│
│ Performance: 1.97 sec avg pit stop           │
│                                              │
│ CREW COMPOSITION (20 members):                │
│ ┌─ Car #1 Crew (11 members) ─┐              │
│ │ Wheel Operators: 4/4 (Morale 72%)          │
│ │ Fuel / Gun Operators: 2/2 (Morale 68%)     │
│ │ Support (Jacks, etc): 5/5 (Morale 75%)     │
│ │ Avg Pit Stop: 1.94 sec                     │
│ │ [REASSIGN] [TRAIN] [MOTIVATE]              │
│ └─────────────────────────────────────────┘
│                                              │
│ ┌─ Car #2 Crew (9 members) ──┐              │
│ │ Wheel Operators: 3/4 ⚠️ (Need hire)        │
│ │ Fuel / Gun Operators: 2/2 (Morale 65%)     │
│ │ Support (Jacks, etc): 4/5 (Morale 72%)     │
│ │ Avg Pit Stop: 2.15 sec (suboptimal)        │
│ │ [HIRE CREW] [REASSIGN] [TRAIN]             │
│ └─────────────────────────────────────────┘
│                                              │
│ TRAINING PROGRAM (between races):            │
│ [ ] Acceleration drills (-0.05 sec pit)      │
│ [ ] Coordination training (morale +3%)       │
│ [SCHEDULE TRAINING] (costs $50K, 3 days)    │
└──────────────────────────────────────────────┘
```

**Key Features**:
- Crew morale affects pit stop time (+/-0.2-0.3 sec)
- Hiring window: Nov-Feb only
- Training reduces pit time by 0.05-0.10 sec
- Reassign crew between cars for races
- Pit crew bonuses tied to race performance

---

## 7.4 R&D PLANNING INTERFACE

### Budget Allocation (No Token System)

**Cost Cap Breakdown**:
```
Total In-Cap Budget: $215M
├─ Personnel (non-drivers): $120M
├─ R&D Operations: $45M
│  ├─ Aerodynamics: $15-18M
│  ├─ Chassis/Suspension: $10-12M
│  ├─ Reliability: $8-10M
│  └─ Manufacturing R&D: $8-10M
├─ Power Unit R&D: $28M (manufacturers only)
├─ Facility Operations: $12M
└─ Contingency: $10M

CAPEX (Separate, $45M/4 years = $11.25M/year):
├─ Wind tunnel upgrades/construction
├─ Simulator facility improvements
└─ Equipment replacement
```

### ATR Allocation Display

**Aerodynamic Testing Resources** (sliding scale by championship position):

```
Championship  Wind Tunnel    CFD Units    Status
Position      Hours/Year     /Year
─────────────────────────────────────────────
1st place     56 hrs (70%)    1120 (70%)   ⚠️ RESTRICTED
2nd place     60 hrs (75%)    1200 (75%)   ⚠️ RESTRICTED
3rd place     64 hrs (80%)    1280 (80%)   ⚠️ RESTRICTED
4th place     68 hrs (85%)    1360 (85%)   LIMITED
5th place     72 hrs (90%)    1400 (90%)   LIMITED
6th-7th       76 hrs (95%)    1540 (95%)   STANDARD
8th-10th      80 hrs (115%)   1840 (115%)  BONUS ✓
```

**Visual allocation bar for current season**:
```
Wind Tunnel Budget: ████████░░░░░░░░ 24/72 hours (33% used)
CFD Units Budget:  ███░░░░░░░░░░░░░░ 380/1400 units (27% used)
```

### R&D Project Dashboard

**Active Projects Display**:
```
┌────────────────────────────────────┐
│ PROJECT: Aero Gen 2 (Floor Package)│
│ Phase: DEVELOPMENT (Week 6/8)      │
│ Progress: ████████░░ 75%           │
│                                    │
│ Resources Allocated:               │
│ • Wind Tunnel: 12/20 hours         │
│ • CFD Units: 200/500 units         │
│ • Budget: $1.2M / $1.5M (80%)      │
│                                    │
│ Expected Gain: +0.18-0.22 sec/lap │
│ Risk Assessment: MEDIUM 🟡         │
│ Impact by Track Type:              │
│   - High speed (Monza): +0.15 sec  │
│   - Medium (Silverstone): +0.20 sec│
│   - Technical (Monaco): +0.08 sec  │
│                                    │
│ [ACCELERATE] [DEFER] [ABANDON]    │
└────────────────────────────────────┘

┌────────────────────────────────────┐
│ PROJECT: ADUO Engine Update (PU)   │
│ Eligibility: Power Deficit 3.2%    │
│ Check Deadline: Race 6 (ACTIVE)    │
│ Status: ✓ APPROVED for development │
│                                    │
│ Development: +4 bhp potential      │
│ Dyno hours: 40/60 allocated        │
│ Timeline: Ready Race 8             │
│                                    │
│ [DEPLOY AT RACE 8]                 │
└────────────────────────────────────┘
```

---

## 7.5 RACE WEEKEND INTERFACE

### Pre-Race Setup Screen

**Race Schedule (Track-dependent)**:

**Traditional Weekend Format**:
```
FRIDAY
  09:00-10:00  FP1 (60 min)
  11:30-12:30  FP2 (60 min)
  
SATURDAY
  10:00-10:45  FP3 (45 min)
  12:00-13:00  Qualifying (Q1, Q2, Q3)
  📌 PARC FERMÉ ACTIVATED
  
SUNDAY
  09:00 Formation Lap (30 min before race)
  14:00 RACE START (305 km)
```

**Sprint Weekend Format** (6 events per season):
```
FRIDAY
  10:00-11:00  FP1 (60 min)
  12:00-13:00  Qualifying (for Sunday race)
  
SATURDAY
  11:00-11:30  Sprint Qualifying (30 min)
  14:00 Sprint Race (~25 min)
  📌 PARC FERMÉ RESET for main race
  
SUNDAY
  09:00 Formation Lap (30 min before race)
  15:00 RACE START (305 km)
```

### Tire Strategy Planner

**6 Available Compounds** (3 selected per weekend):

```
Compound  Grip Level  Degradation  Typical Life  Temp Window
─────────────────────────────────────────────────────────────
C6 Ultra-Soft  +0.30 sec  Very high  10-18 laps    Extreme heat
C5 Soft        +0.25 sec  High       15-25 laps    25-35°C
C4 Medium      Baseline   Medium     20-35 laps    20-32°C
C3 Hard        -0.15 sec  Low        30-50 laps    18-28°C
C2 Super Hard  -0.20 sec  Very low   40-60 laps    <20°C
C1 Ultra Hard  -0.25 sec  Minimal    50-70 laps    Cold climates
```

**Track-Specific Degradation Examples**:
```
MONACO (High degradation):
  C5 Soft: 10-15 laps → suggests 2-3 stops
  C4 Medium: 20-25 laps
  C3 Hard: 35-40 laps
  ⚠️ High tire wear circuits

MONZA (Low degradation):
  C5 Soft: 25-30 laps → suggests 1-2 stops
  C4 Medium: 35-42 laps
  C3 Hard: 55-65 laps
  ✓ Low tire wear circuits
```

**Strategy Options**:
- ONE-STOP: C5 → C4 (fast but risky late-race)
- TWO-STOP: C5 → C4 → C3 (balanced, typical)
- THREE-STOP: C5 → C4 → C3 → C5 (for extreme degradation, viable at Singapore)
- RAIN: Intermediate/Wet (ignores compound rule)

### Strategy Templates & Presets

**Quick-Load Strategy Presets** (save/load between races):

```
┌────────────────────────────────────────────┐
│ RACE STRATEGY PRESETS                      │
│                                            │
│ ⭐ AGGRESSIVE 2-STOP                       │
│    Tire plan: C5 (8-12L) → C3 (25-30L)    │
│    Pit Lap: 15 & 32                       │
│    Driver mode: PUSH until pit, then SAVE │
│    Risk: High degradation gamble          │
│    [LOAD] [EDIT] [DELETE]                 │
│                                            │
│ ⭐ CONSERVATIVE 2-STOP                     │
│    Tire plan: C4 (15-20L) → C4 (20-25L)  │
│    Pit Lap: 20 & 38                       │
│    Driver mode: STANDARD, late PUSH       │
│    Risk: Low, relies on tire advantage    │
│    [LOAD] [EDIT] [DELETE]                 │
│                                            │
│ ⭐ UNDERCUT SPECIALIST (1-STOP)            │
│    Tire plan: C5 (18-22L) → C4            │
│    Pit Lap: 22 (early, catch undercut)    │
│    Driver mode: PUSH lap 1-21, DEFEND 22+ │
│    Risk: Medium, depends on pace delta    │
│    [LOAD] [EDIT] [DELETE]                 │
│                                            │
│ [+ NEW PRESET] [SAVE CURRENT]              │
└────────────────────────────────────────────┘
```

**Custom Preset Creation**:
- Save current race plan as new template
- Name it (e.g., "Monaco Wet Contingency")
- Mark as track-specific or all-purpose
- Can be auto-loaded for future races at same track

**Keyboard shortcut**: `P` = Show presets, `P+1/2/3` = Load template 1/2/3

### Live Race Monitor

**On-Track Display**:

```
Position 2/20  |  Gap to 1st: 0.8s  |  Gap to 3rd: +1.2s
──────────────────────────────────────────────────────

Lap 18/58      |  15:34 elapsed     |  Est. finish: 17:12

DRIVER #1 (Lewis):
  Tire: C4 Medium (Lap 8/32 remaining)  │ Status: ✓ HEALTHY
  Fuel: 28.4 kg (16 laps remaining)     │ Consumption: 1.6 kg/lap
  Engine: 4/8 PU used (healthy)         │ DRS: ✓ AVAILABLE (0.9s gap)
  Instruction: STANDARD                 │ Morale: 82%

Next Pit Stop: Lap 26 (8 laps)
  Plan: Change to C3 Hard, 25 kg fuel
  Est. time: 2.2-2.4 seconds
  
[PUSH] [STANDARD] [DEFEND] [FUEL SAVE] [BOX THIS LAP]
```

**Race Incidents & Strategy Options**:
- 🟡 YELLOW FLAG: Sector 1, debris
- 🔴 SAFETY CAR: Deploy cost analysis
- ⚠️ WEATHER ALERT: Rain arriving Lap 22-24 (90% confidence ±1 lap)
- 📊 STRATEGY SUGGESTION: "Pit Lap 25, gain 0.5 sec on undercut"

### Pit Stop Mechanics

**Pit Stop Time Display**:
```
Pit Crew Morale: 88% → Excellent execution

Expected pit stop times:
  Perfect (95%+ morale):    1.82-1.95 sec (record territory)
  Excellent (85-94%):       1.95-2.1 sec (top team level)
  Normal (70-84%):          2.2-2.5 sec (standard mid-field)
  Poor (<70%):              2.8-4.0 sec (errors, fumbles)

Tire Warm-up Penalties:
  C5 Soft (fresh):    -1.0 sec/lap Lap 1 only
  C4 Medium (fresh):  -1.5 sec/lap Lap 1
  C3 Hard (fresh):    -2.0 sec Lap 1, -1.0 sec Lap 2
  
  Track temp: 28°C (optimal) = faster warm-up
  Track temp: 12°C (cold) = +30% warm-up time
```

### Driver Instructions

**Available Instructions** (No 3-lap Attack limit):

```
[STANDARD] - Baseline pace, consistent

[PUSH/ATTACK] - Maximize pace, accumulate fatigue
  • Gain: +0.2-0.3 sec/lap potential
  • Fatigue accumulates: Lap 4-6 (-0.05 sec), Lap 7+ (-0.10 sec)
  • Fuel flow risk: Approaches 100 kg/hour limit
  • Tire wear: +10-15% per lap
  • Recovery: 5 laps normal pace reduces fatigue 50%
  
[DEFEND] - Conservative, block overtakes
  • Cost: -0.15 sec/lap (focus on defense)
  • Morale bonus: +5% if successful
  
[FUEL SAVE] - Economy mode
  • Cost: -0.1-0.2 sec/lap + morale -5%
  • Benefit: Stretches fuel, avoid pit stop
  
[CAUTION - WET] - Reduce speed in rain
  • Cost: Variable based on rain intensity
  • Morale bonus: +5% (driver feels supported)
```

### HUD Profile Selection (Customizable Race Display)

**Three Display Modes** (toggle with `H` key during race):

```
┌─────────────────────────────────────────────────┐
│ HUD PROFILE SELECTOR                            │
│                                                 │
│ ⭐ BEGINNER (Information minimized)             │
│   Shows: Position, time gap, tire life, fuel   │
│   Hides: DRS status, morale, engine resource   │
│   Use for: Learning, relaxed play              │
│   [ACTIVATE]                                    │
│                                                 │
│ ⭐ STANDARD (Balanced - DEFAULT)               │
│   Shows: Everything below                      │
│   Use for: Competitive standard F1 experience  │
│   [ACTIVATE] ✓ ACTIVE                          │
│                                                 │
│ ⭐ EXPERT (Full telemetry)                      │
│   Shows: All data + telemetry, fuel flow rate, │
│           tire temperature, brake balance      │
│   Use for: Simulationists, pit crew view       │
│   [ACTIVATE]                                    │
│                                                 │
│ [CUSTOM] → Save current HUD layout as preset   │
│ [SETTINGS] → Reorder/hide individual panels    │
│ [INSTANT MODE] ☐ → Toggle animations off      │
└─────────────────────────────────────────────────┘
```

**Individual Panel Visibility** (via settings):
- Pit Stop Timer (always show before pit)
- Tire Degradation Graph
- Fuel Usage Calculator
- Weather Forecast (mini)
- Driver Morale Indicator
- Engine Status

---

## 7.6 FINANCE INTERFACE

### Financial Dashboard (Updated)

**Cost Cap Status**:
```
╔════════════════════════════════╗
║  COST CAP TRACKER 2026         ║
╠════════════════════════════════╣
║ Total Limit:      $215M        ║
║ Spent YTD (Races 1-6): $95M    ║
║ Quarterly Rate:   $15.8M/race  ║
║ Q3 Forecast:      $110M        ║
║ Status:           ✓ ON TRACK   ║
║                                ║
║ Penalties if over-cap:         ║
║ • 0-5%:  $0.5-2M fine + 5% ATR║
║ • 5-10%: $3-5M fine + 10-20%  ║
║ • >10%:  $10-15M + 25-35% ATR ║
╚════════════════════════════════╝
```

### Prize Money Distribution (3-Tier Model)

**$1.6B Annual Prize Pool**:

```
Tier 1: Championship Position (75% = $1.2B)
├─ 1st: $168M (14%)
├─ 2nd: $162M (13.5%)
├─ 3rd: $156M (13%)
├─ 4th: $144M (12%)
├─ 5th: $132M (11%)
├─ 6th: $108M (9%)
├─ 7th: $96M (8%)
├─ 8th: $90M (7.5%)
├─ 9th: $84M (7%)
└─ 10th: $72M (6%)

Tier 2: Heritage/Historical Bonus (20% = $320M)
├─ Ferrari: $80M (most titles)
├─ Mercedes: $60M (recent dominance)
├─ Red Bull: $40M (recent success)
├─ McLaren: $40M
└─ Others: Combined $100M

Tier 3: Ferrari Legacy Bonus (5% = $80M)
└─ Ferrari only: $80M (FIA regulation)

PAID: December (season-end only, not race-by-race)
```

### Sponsorship Status

**Sponsor Portfolio Display**:
```
┌──────────────────────────────────┐
│ ORACLE ENERGY (Title Sponsor)     │
│ Contract Value: $65M/year         │
│ Status: Active (2024-2026)        │
│                                  │
│ KPI PROGRESS (Target: Top 3):    │
│ Championship Position: 4th 🟡    │
│ Podium Target: 5 podiums         │
│ ├─ Achieved: 3 (60%)             │
│ Wins Target: 2 wins              │
│ ├─ Achieved: 1 (50%)             │
│ Brand Exposure: 78% of target     │
│                                  │
│ Year-End Adjustment:             │
│ • Base: $65M (full payment)      │
│ • KPI Adjustments: -$4M (4th)    │
│ • Final: $61M expected           │
│                                  │
│ Risk Level: MEDIUM 🟡            │
└──────────────────────────────────┘

Championship Series Sponsor:
  📊 Nestlé (KitKat): $6.4M share
  (40% equal + 30% position + 30% home market)
```

**Market Growth Mechanic**:
- Baseline: 5-10% annual sponsor value increase
- Championship winners: +15-20% increase next year
- New entrants: Ramp up over 3 years
- Market reference: Grew $677M → $2.04B (2020-2024)

### Injury Insurance Option

```
┌──────────────────────────────────┐
│ DRIVER INJURY INSURANCE           │
│ Cost: $2.5M / driver / season    │
│                                  │
│ Coverage:
│ ✓ Salary during absence          │
│ ✓ 75% bonus pool replacement     │
│ ✓ Reduced championship impact    │
│                                  │
│ Without Insurance:
│ - 4-race injury = -$9M+ impact   │
│ - Next season: -$24M prize impact│
│                                  │
│ ROI: Recommended for top drivers │
│ [PURCHASE] [DECLINE]             │
└──────────────────────────────────┘
```

---

## 7.7 RACE RESULTS & POST-RACE

### Race Result Summary

```
╔════════════════════════════════╗
║  RACE 8 RESULT - BELGIUM       ║
╠════════════════════════════════╣
║ Position:     3rd              ║
║ Championship Points: +15       ║
║ Prize Money (season-end):      ║
║   • 4th place in championship  ║
║   • Estimated total: $144M     ║
║ Podium Bonus:    $250K         ║
║ Sponsor KPI:     Podium count  ║
║ Driver Morale:   +5% (podium)  ║
║ Team Morale:     +8%           ║
╚════════════════════════════════╝
```

### Driver Performance Breakdown

```
CAR #1 (Lewis)
Pace Rating: 92/100
Racecraft Rating: 88/100 (+2 from race decisions)
Lap Time Avg: 1:47.3 (0.1 sec off winning pace)
Best Lap: 1:47.1
Tire Management: ✓ Excellent
Fuel Management: ✓ Excellent
Attack Mode Usage: 6 laps (within limits)
Driver Fatigue: 22% (accumulated)
Morale: 85% → 90% (+5% podium)
```

### Dynamic Rating Updates

```
DRIVER DEVELOPMENT (Post-race):
• Lewis - Pace: 92 → 92 (stable)
         Racecraft: 88 → 90 (+2 from 3 successful overtakes)
         Consistency: 85 → 86 (+1 from clean race)

• Partner - Pace: 78 → 78 (stable)
           Racecraft: 72 → 71 (-1 from early mistake)
           Consistency: 80 → 79 (-1 from off-track)
```

---

## 7.8 CONSISTENCY STANDARDS

### Updated Terminology

| Old Term | New Term | Notes |
|----------|----------|-------|
| Tokens | Budget Allocation | Wind tunnel hours/CFD units from $215M cap |
| Gen 1.5 | Evolution | Minor improvement (4-10 weeks R) |
| Gen 2 | Major Update | Significant improvement (6-10 weeks R) |
| Gen 3 | (Removed) | No longer exists in game |
| 52 Engineers | 7 Leads + Team Base | Realistic org structure |
| FP4 | (Removed) | Doesn't exist in real F1 post-2022 |
| Warm-Up | (Removed) | Cancelled in 2003 real F1 |
| Race-by-race prizes | Season-end payout | All paid in December |
| Fastest lap bonus | (Removed) | Abolished 2025 |
| $135M Cost Cap | $215M Cost Cap | Updated for 2026 |
| 5 tire compounds | 6 compounds (C1-C6) | Track-dependent life |

### Visual Indicators

```
R&D Status:
  🔵 Blue    = Evolution project
  🟠 Orange  = Major Update project
  🟡 Yellow  = Research phase
  🟢 Green   = Development phase
  ⚫ Gray    = Abandoned/paused

Resource Tracking:
  🔷 Cyan   = Wind tunnel hours used
  🟣 Purple = CFD units used
  💵 Green  = Budget available
  💵 Red    = Over budget alert

Driver/Team Status:
  🔴 Red    = Critical (morale <30%, injury, etc.)
  🟡 Yellow = Warning (morale 30-60%, medium risk)
  🔵 Blue   = Information (normal activity)
  ⚪ Gray   = Neutral (no action needed)

Performance:
  ↑ Green arrow   = Improved
  ↓ Red arrow     = Declined
  → Gray arrow    = Stable
```

---

## 7.9 EVENTS & NOTIFICATIONS SYSTEM

**Unified Notification Center** (accessible via `N` key or inbox icon):

```
┌──────────────────────────────────────────┐
│ INBOX (5 UNREAD MESSAGES)                │
├──────────────────────────────────────────┤
│                                          │
│ 🔴 CRITICAL                             │
│ └─ Contract Expiration (3 days)          │
│    Director of Engineering expires       │
│    Race 8. Offer renewal? +$50K/year     │
│    [RENEW NOW] [NEGOTIATE] [DISMISS]     │
│                                          │
│ 🟠 WARNING                               │
│ └─ KPI At Risk (Sponsor: TechCorp)       │
│    Current: 45% / Target: 60%            │
│    Deadline: Race 9 (2 races left)       │
│    [REVIEW KPI] [STRATEGY] [DISMISS]     │
│                                          │
│ ├─ ADUO Development Window Opens         │
│    Your team eligible for power check    │
│    3 checks per season, next: Race 6     │
│    [APPROVE DEVELOPMENT] [DISMISS]       │
│                                          │
│ 🔵 INFORMATIONAL                        │
│ └─ Driver Morale Update: Lewis +8%       │
│    Reason: Podium celebration            │
│                                          │
│ └─ Pit Crew Training Completed           │
│    Average pit stop: 1.96 sec (-0.15s)   │
│                                          │
│ [MARK ALL AS READ]                       │
└──────────────────────────────────────────┘
```

**Event Calendar** (accessible from sidebar):
- Contract renewals (color-coded by department)
- ADUO check deadlines
- Sponsor KPI milestones
- R&D project completions
- Hiring windows (Nov-Feb highlighted)

**Notification Settings**:
- Toggle critical alerts: On/Off
- Email digest frequency: Daily/Weekly/Off
- Sound effects: On/Off
- Desktop notifications: On/Off

---

## 7.10 ONBOARDING & TOOLTIP SYSTEM

**Interactive Tutorial (First Race)**:

```
┌──────────────────────────────────────────┐
│ WELCOME TO AUTOSPORT MANAGER 2027        │
│                                          │
│ 📚 TUTORIAL STEPS (3/8)                  │
│                                          │
│ ✓ 1. Dashboard overview (5 min)          │
│ ✓ 2. Team structure (3 min)              │
│ ▶ 3. R&D Planning (current)              │
│    [HIGHLIGHT] [SKIP] [DETAILS]         │
│                                          │
│ Next steps:                              │
│  4. Race weekend setup                   │
│  5. Finance management                   │
│  6. Live race control                    │
│  7. Post-race analysis                   │
│  8. Season progression                   │
│                                          │
│ [CONTINUE] [EXIT TUTORIAL] [RESTART]     │
└──────────────────────────────────────────┘
```

**Contextual Tooltips** (Hover on any unfamiliar term):
- ADUO: "Aerodynamic Development Upgrade Option - 3 annual power unit checks for manufacturers"
- ATR: "Aerodynamic Testing Resources - sliding scale 70-115% based on championship position"
- CFD: "Computational Fluid Dynamics - simulations using allocated units (1400 max/year)"
- CapEx: "Capital Expenditure - separate $45M/4 years for facility improvements"
- Cost Cap: "$215M annual limit for personnel, R&D, power unit R&D, facilities"

**Glossary Screen** (accessible via `?` key):
- Full alphabetical reference of all game terms
- Cross-links to related concepts
- Search function to find definitions quickly
- Print-friendly PDF option

**Learning Mode** (Optional):
- AI hints suggest optimal decisions ("Consider early pit stop to gain undercut")
- Explanations for system interactions
- Disable for experienced players

---

## 7.11 UNDO/REDO & ACTION CONFIRMATION

**Confirmation Dialogs** (for expensive/irreversible actions):

```
For FIRING an engineer:
┌─────────────────────────────────────┐
│ ⚠️  CONFIRM DISMISSAL               │
│                                     │
│ You are about to terminate:         │
│ Chief Aerodynamicist ($245K/year)   │
│                                     │
│ Consequences:                       │
│ • Loss of expertise (-5 R&D speed)  │
│ • Severance cost: $200K             │
│ • Team Base Level: -10%             │
│ • Replacement hiring: 3 months      │
│                                     │
│ Is this correct?                    │
│                                     │
│ [CONFIRM] [CANCEL] [ALTERNATIVE]    │
└─────────────────────────────────────┘
```

**Action History** (Between races only):
- Last action only can be undone with `Ctrl+Z`
- Redo with `Ctrl+Y`
- Action list shows: "Spent $2.5M on research", "Hired Chief Reliability Engineer", "Dismissed pit crew member"

**Undo Limitations**:
- Cannot undo during active race (to prevent exploit)
- Can undo between-race decisions during pause mode
- Cannot undo past 1 action (prevents save-scumming)

**Cancellations**:
- In-progress R&D projects can be paused/stopped with cost
- Contracts can be broken (penalties apply)
- Hiring offers can be withdrawn

---

## 7.12 SMART FILTERS & ADVANCED VIEWS

**R&D Projects View with Filters**:

```
FILTER CONTROLS:
[PROJECT TYPE] [RISK] [TIMELINE] [ROI] [STATUS]

Current filters: TYPE=Aero, RISK=Medium, STATUS=Active

Sort by:  ⬇ Deadline | Expected Gain | Risk/Reward | Budget

RESULTS (2 matching projects):
────────────────────────────────────────────
1. Aero Package Update
   Status: Development (60% complete)
   Timeline: 6 weeks | Risk: Medium 🟡
   Budget: $1.2M / $1.5M allocated
   Expected gain: +0.20 sec/lap
   ROI: Good (completes before Race 8)
   [VIEW DETAILS] [ACCELERATE] [DEFER]

2. Slot Gap Optimization
   Status: Research (30% complete)
   Timeline: 4 weeks | Risk: Medium 🟡
   Budget: $400K / $800K allocated
   Expected gain: +0.05 sec/lap (low impact)
   ROI: Poor (low gain, late delivery)
   [VIEW DETAILS] [ACCELERATE] [ABANDON]
────────────────────────────────────────────
```

**Finance View Smart Filters**:
- Filter by department (Personnel, R&D, Facilities, etc.)
- Sort by spend (highest to lowest)
- Show only Over-Budget items
- Group by cost tier

**Driver Market Filters** (when hiring):
- Filter by skill range (70-90)
- Sort by salary demand
- Show only available drivers
- Aging/Contract status indicators

---

## 7.13 ACCESSIBILITY IMPROVEMENTS

**Color-Blind Friendly Design**:
- All color indicators ALSO have icons/symbols
  - Red → ⚠️ (warning/critical)
  - Yellow → ⚡ (caution/medium)
  - Green → ✓ (success/good)
  - Blue → ℹ️ (information)

**Text Contrast**:
- WCAG AA standard minimum (4.5:1)
- Dark text on light backgrounds in tutorials
- Light text on dark backgrounds in main UI

**Keyboard Navigation**:
- Tab to navigate all elements
- Enter to confirm, Escape to cancel
- Arrow keys to scroll lists
- Alt+key shortcuts for menu items (Alt+T = Team, Alt+R = R&D, Alt+B = Budget)

**Font & Size Options**:
- Adjustable UI scaling (90%-130%)
- Dyslexia-friendly font option (Comic Sans MS or OpenDyslexic)
- Line-height adjustment for readability

**Audio Cues**:
- Optional sound effects for notifications
- Distinct sounds for different alert types
- Volume control in settings

---

## 7.14 GAME DESIGN EVALUATION & IMPROVEMENTS

### Current Implementation vs Game Dev Standards

| Principle | Rating | Status | Improvement |
|-----------|--------|--------|-------------|
| **Clarity** | ✅ Good | All metrics labeled & color-coded | Add glossary tooltips (DONE) |
| **Feedback** | ⚠️ Medium → **✅ Good** | Added micro-interactions, loading states | Reduce animation to 150-200ms (DONE) |
| **Consistency** | ✅ Good | Repeated patterns maintained | Added visual icon+color redundancy (DONE) |
| **Efficiency** | ❌ Poor → **⚠️ Medium** | Hotkeys & presets added | Need drag-drop widget reorder |
| **Error Forgiveness** | ❌ Weak → **✅ Good** | Added undo/redo & confirmations | Prevent 1-click disasters (DONE) |
| **Accessibility** | ⚠️ Medium → **✅ Good** | Color + icons, keyboard nav, text sizing | Full WCAG AA compliance (DONE) |
| **PC Optimization** | ✅ Good | Retained for mouse/keyboard | Added HUD customization (DONE) |

### Implementation Priorities

**Phase 1 (Core - IMPLEMENTED):**
- ✅ Reduce animation timing (150-200ms)
- ✅ Add quick command system (hotkeys)
- ✅ Tabbed dashboard (5-7 blocks max)
- ✅ Strategy templates/presets
- ✅ Events/notifications system
- ✅ Pit crew management interface
- ✅ HUD profiles (beginner/standard/expert)
- ✅ Undo/redo & confirmations
- ✅ Tooltips & onboarding
- ✅ Accessibility (color + icons + keyboard)
- ✅ Smart filters

**Phase 2 (Polish - FUTURE):**
- Drag-drop widget customization
- Video tutorials for complex systems
- AI assistant suggestions during play
- Advanced telemetry visualizations
- Replay system for analyzing decisions
- Achievement/challenge system

### Feedback Loop

**Player Telemetry to Track**:
- Average time per menu navigation
- Hotkey adoption rate (% of players using shortcuts)
- Preset usage (save/load frequency)
- Undo/redo usage (detect frustration patterns)
- Tooltip click-through (identify unclear terms)
- Accessibility setting usage

---

## 7.15 RESPONSIVE DESIGN

**Desktop (Primary)**:
- Full-width dashboard with all widgets visible
- Drag-and-drop widget rearrangement
- Multi-panel sidebar navigation

**Tablet**:
- 2-column layout
- Scrollable dashboard sections
- Simplified sidebar (hamburger menu)

**Mobile** (Limited):
- Single-column layout
- Essential widgets only
- Touch-optimized buttons

---

**[END OF PART 7: UI/UX DESIGN]**

## COMPREHENSIVE SUMMARY

This refined UI/UX design incorporates all 2025-2026 F1 regulations with professional game design improvements addressing 12 major design principles. Implementation prioritizes:

**Phase 1 Complete (11 Core Improvements)**:
- ✅ Optimized animation timing (150-200ms with instant mode)
- ✅ Intelligent dashboard layout (tabbed, 5-7 blocks max)
- ✅ Complete hotkey system for rapid navigation
- ✅ Strategy template system with save/load presets
- ✅ Unified notification and event center
- ✅ Pit crew management and training interface
- ✅ HUD customization profiles for all skill levels
- ✅ Full undo/redo with intelligent confirmations
- ✅ Interactive tutorial and comprehensive tooltips
- ✅ Full accessibility compliance (icons, keyboard, text sizing)
- ✅ Smart filtering and sorting across all views

**Result**: Balances authentic F1 complexity with intuitive player control, suitable for casual fans and hardcore simulationists alike.

