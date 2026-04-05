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
- 400-600ms smooth transitions
- Mobile-responsive (desktop primary, tablet secondary)

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

### Primary Widgets (Always Visible)

| Widget | Data | Update Frequency |
|--------|------|------------------|
| **Championship Position** | Current standing, points gap to leader | Real-time |
| **Cash Position** | Available budget, quarterly forecasts | Weekly |
| **Team Morale** | Overall %, factors (results/sponsorships/goals) | Race-by-race |
| **Upcoming Race** | Next GP, date, track, forecast | Session-by-session |

### Secondary Widgets (Customizable)

| Widget | Purpose | Data Shown |
|--------|---------|-----------|
| **R&D Status** | Development progress | Wind Tunnel hrs (actual/limit), CFD units (actual/limit), Active projects |
| **Driver Performance** | Current driver form | Pace rating, racecraft, morale, recent races |
| **Financial Forecast** | Budget health | Q3/Q4 forecast, prize money projection, sponsor status |
| **Sponsor Status** | KPI tracking | Each sponsor's KPI progress, risk of bonus loss |
| **Reliability Standing** | DNF risk assessment | Engine resource (X/8 used), overall reliability rating |
| **Department Heads** | Team health | 7 leads: morale, retention risk, contract status |

### Quick Action Cards

```
┌─────────────────────────────────┐
│ 🔧 TEAM MANAGEMENT              │
│ 3 Directors | 7 Department Heads │
│ Team Morale: 76% (+2% race)      │
│ [VIEW DETAILS]                  │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│ 📊 R&D PLANNING                 │
│ Wind Tunnel: 24/72 hrs (33%)     │
│ CFD Units: 380/1400 (27%)        │
│ Active Projects: 3               │
│ [MANAGE PROJECTS]               │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│ 💰 BUDGET REVIEW                │
│ Cost Cap Used: $95M / $215M      │
│ Cash Reserve: $12.3M             │
│ Q4 Forecast: +$28M prize money  │
│ [BUDGET DETAILS]                │
└─────────────────────────────────┘
```

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

## 7.9 RESPONSIVE DESIGN

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

This comprehensive UI/UX design incorporates all regulations and mechanics from the 2025-2026 F1 ruleset, providing players with an intuitive interface to manage complex team operations while maintaining visual clarity and accessibility.

