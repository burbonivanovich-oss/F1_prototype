# PART 7: UI/UX DESIGN

---

## 7.1 DESIGN PHILOSOPHY

**Core UX Principles for Autosport Manager 2027:**

1. **Information Density**: Deep complexity accessible through progressive disclosure
   - Main screen simple and scannable
   - Detailed information available with one click
   - Avoid overwhelming with options

2. **Accessibility**: Multiple paths to same information
   - Dashboard summary view AND detailed view for every system
   - Keyboard shortcuts for power users
   - Mobile-responsive design (tablet play possible)

3. **Real-Time Feedback**: Immediate visual confirmation of actions
   - Confirmation dialogs for irreversible actions
   - Toast notifications for important events
   - Subtle animations for state changes (not distracting)

4. **Visual Hierarchy**: Importance determines prominence
   - Championship position > Driver names > Time deltas
   - Critical alerts (red) > warnings (yellow) > info (blue) > neutral (gray)
   - Scannable typography (headings, bold, colors)

5. **Context-Sensitive**: UI changes based on season phase
   - Pre-season: Setup focus (R&D, contracts, facilities)
   - In-season: Race weekend focus (strategy, morale, incidents)
   - Post-race: Analysis focus (results, financials, adjustments)

**Design Style**: Modern F1 aesthetic
- Dark theme (black/dark gray background)
- Bright accent colors (FIA red, team colors, neon accents)
- Clean typography (sans-serif, excellent readability)
- Minimalist iconography (clear, recognizable)
- Smooth transitions (400-600ms animation duration)

---

## 7.2 MAIN DASHBOARD

### 7.2.1 Dashboard Layout (Default Home Screen)

**Screen Purpose**: At-a-glance overview of team status across all three cycles

**Visual Layout** (16:9 aspect ratio):
```
┌─────────────────────────────────────────────────────────────────┐
│ AUTOSPORT MANAGER 2027  |  Season 6, Week 15  |  Team Name      │  (Header)
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │  CHAMPIONSHIP   │  │  CASH POSITION  │  │  TEAM MORALE    │  │
│  │                 │  │                 │  │                 │  │
│  │  3RD PLACE      │  │  $8.2M          │  │  72%            │  │ (Top row summary)
│  │  93 Points      │  │  +$1.2M (+17%)  │  │  ↑ +5% this week│  │
│  │  -25 vs Leader  │  │  Monthly: $1.8M │  │  Healthy        │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
│                                                                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  UPCOMING RACE: BELGIUM (Race 8 of 24)              [SCHEDULE]   │
│  └─ Qualifying: Friday 14:00 UTC                                 │
│  └─ Race: Sunday 14:00 UTC  (in 5 days)                         │
│                                                                   │
│  QUICK ACTIONS:                                                  │
│  ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐ │
│  │  TEAM MANAGEMENT │ │  R&D PLANNING    │ │  BUDGET REVIEW   │ │ (Action shortcuts)
│  │  Morale: 72%     │ │  Tokens: 3.5/8   │ │  Q3: $45M spent  │ │
│  │  15 Personnel    │ │  Aero: In Dev    │ │  Reserve: $8.2M  │ │
│  │  [MANAGE]        │ │  [PLAN]          │ │  [DETAILS]       │ │
│  └──────────────────┘ └──────────────────┘ └──────────────────┘ │
│                                                                   │
├─────────────────────────────────────────────────────────────────┤
│  RECENT EVENTS:                                                  │
│  ├─ Race 7 Podium: 3rd place (EXCELLENT) +$1.5M prize money    │
│  ├─ Driver #2 morale low (62%) - MONITOR                        │
│  ├─ Aero Gen 2 deployment confirmed for Race 8                  │
│  ├─ TechCorp sponsor checking in on performance (85% satisfied)  │
│  └─ Engine Gen 2 development 60% complete, on track             │
│                                                                   │
├─────────────────────────────────────────────────────────────────┤
│  ALERTS & NOTIFICATIONS:                                         │
│  🔴 URGENT: Driver #2 contract expires Race 12 - NEGOTIATE NOW  │
│  🟡 WARNING: Power Unit Lead considering Ferrari offer          │
│  🟢 INFO: Wind tunnel hours allocated for this week             │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

### 7.2.2 Dashboard Widgets (Customizable)

**Player can customize dashboard by:**
- Dragging widgets to reorder
- Resizing widgets (small/medium/large)
- Hiding/showing specific widgets
- Saving custom layouts

**Available Widgets:**

| Widget | Default | Customizable | Data Shown |
|--------|---------|------------|-----------|
| Championship Position | Yes | No | Points, position, gap |
| Cash Position | Yes | No | Budget balance, monthly rate |
| Team Morale | Yes | No | Percentage, trend |
| Upcoming Race | Yes | Yes | Next race date/time |
| R&D Status | Yes | Yes | Token usage, active projects |
| Personnel Summary | Yes | Yes | Headcount, morale avg |
| Recent Events | Yes | Yes | Last 5 events |
| Alerts | Yes | No | Critical notifications |
| Sponsor Status | No | Yes | Satisfaction scores |
| Driver Performance | No | Yes | Points, morale per driver |
| Financial Forecast | No | Yes | Quarter projections |
| Reliability Tracker | No | Yes | DNF probability |

---

## 7.3 TEAM MANAGEMENT INTERFACE

### 7.3.1 Personnel Management Screen

**Navigation**: Dashboard → Team → Personnel Management

**Layout** (Three-Column Design):

```
┌─────────────────────────────────────────────────────────────────────┐
│ TEAM MANAGEMENT / PERSONNEL                    [MORALE] [BUDGET] [X]│
├──────────────────────┬────────────────────────────────────────────┤
│ FILTERS & SORT:      │ PERSONNEL LIST (15/52 employees shown)    │
│                      │                                             │
│ Department:          │ NAME           | POSITION    | SKILL | PAY │
│ ☑ All              │                                             │
│ ☐ Aerodynamics     │ John Smith     | Chief Aero  | 86    | $280K│
│ ☐ Power Unit       │ Maria Garcia   | Power Lead  | 81    | $450K│
│ ☐ Chassis          │ David Chen     | Sr Engineer | 75    | $180K│
│ ☐ Reliability      │ Sarah Johnson  | Junior Eng  | 62    | $80K │
│ ☐ Manufacturing    │ [scroll down for more]                      │
│ ☐ Telemetry        │                                             │
│                      │ [Pagination: 1-15 of 52]                   │
│ Skill Level:        │                                             │
│ ○ All              │ SORT BY: Name ▼ | FILTER: All ▼            │
│ ○ 80+ (World Class)│                                             │
│ ○ 70-79 (Senior)   │                                             │
│ ○ <70 (Junior)     │                                             │
│                      │                                             │
│ Morale:            │                                             │
│ ○ All             │                                             │
│ ○ 80%+ (Excellent) │                                             │
│ ○ 60-80% (Good)    │                                             │
│ ○ <60% (Concerning)│                                             │
│                      │                                             │
│ [APPLY FILTERS]    │                                             │
├──────────────────────┼────────────────────────────────────────────┤
│                      │ DETAIL VIEW (Click person to expand):     │
│                      │                                             │
│                      │ John Smith (Chief Aerodynamicist)         │
│                      │ ├─ Skill: 86/100 (World-Class)            │
│                      │ ├─ Morale: 78% (Good, trending ↓)        │
│                      │ ├─ Salary: $280K/year                     │
│                      │ ├─ Contract: 2 years remaining            │
│                      │ ├─ Experience: 15 years (F1 veteran)      │
│                      │ ├─ Specializations: Aerodynamics (primary)│
│                      │ │                   Telemetry (secondary) │
│                      │ ├─ Recent Projects: Aero Gen 2 (success)  │
│                      │ └─ Retention Risk: MEDIUM (Ferrari offer) │
│                      │                                             │
│                      │ [PROMOTE] [RAISE SALARY] [RETAIN BONUS]  │
│                      │ [VIEW HISTORY] [NEGOTIATE CONTRACT]       │
│                      │                                             │
└──────────────────────┴────────────────────────────────────────────┘
```

### 7.3.2 Driver Management Screen

**Navigation**: Dashboard → Team → Drivers

**Layout** (Two Driver Cards + Contract Details):

```
┌────────────────────────────────────────────────────────────────┐
│ DRIVER MANAGEMENT                                  [MORALE] [X] │
├────────────────────────────────────────────────────────────────┤
│                                                                  │
│  CAR #1: DRIVER #1 (PRIMARY)                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Lewis (Age 28, Pace 92, Racecraft 88)                   │  │
│  │ Morale: 85% ████████░ Excellent                          │  │
│  │ Performance: 3 wins, 8 podiums, 78 points this season   │  │
│  │ Contract: 3 years (2 remaining), $18M/year base         │  │
│  │                                                           │  │
│  │ YTD Earnings:                                            │  │
│  │  ├─ Base salary: $13.5M (9 months paid)                 │  │
│  │  ├─ Win bonuses: $1.5M (3 wins × $500K)                 │  │
│  │  ├─ Podium bonuses: $2M (8 podiums × $250K)            │  │
│  │  └─ YTD Total: $17M                                      │  │
│  │                                                           │  │
│  │ Morale Factors:                                          │  │
│  │  ✓ Recent podiums (+15% each, cumulative)              │  │
│  │  ✓ Excellent car performance (+10%)                    │  │
│  │  ○ Salary on market (+0%)                              │  │
│  │                                                           │  │
│  │ [VIEW FULL STATS] [RENEGOTIATE CONTRACT] [MEDICAL CHECK]│  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  CAR #2: DRIVER #2 (SUPPORT)                                    │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Carlos (Age 26, Pace 84, Racecraft 82)                  │  │
│  │ Morale: 62% █████░░░░ Concerning ⚠️                      │  │
│  │ Performance: 0 wins, 1 podium, 34 points this season    │  │
│  │ Contract: 2 years (2 remaining), $5M/year base          │  │
│  │                                                           │  │
│  │ YTD Earnings:                                            │  │
│  │  ├─ Base salary: $3.75M (9 months paid)                 │  │
│  │  ├─ Podium bonuses: $250K (1 podium)                    │  │
│  │  └─ YTD Total: $4M                                       │  │
│  │                                                           │  │
│  │ Morale Factors:                                          │  │
│  │  ✗ Losing to teammate (-15% per race)                   │  │
│  │  ✗ Only 1 podium vs 3 target (-5%)                      │  │
│  │  ✗ Below market salary (-2%)                             │  │
│  │  ○ Recent pace improvement (+5%)                        │  │
│  │                                                           │  │
│  │ Actions Available:                                       │  │
│  │ [SALARY INCREASE] [PROMOTION TO #1] [SIMULATOR TRAINING]│  │
│  │ [MEDICAL CHECK] [RENEGOTIATE CONTRACT] [FIRE DRIVER]    │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  RESERVE DRIVER:                                                │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ George (Age 23, Pace 78, Racecraft 75, Reserve Status) │  │
│  │ Status: Ready for emergency activation                  │  │
│  │ Salary: $400K/year (minimal, on-call)                   │  │
│  │ [CHECK READINESS] [SIMULATOR HOURS] [CONTRACT DETAILS]  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
└────────────────────────────────────────────────────────────────┘
```

---

## 7.4 R&D PLANNING INTERFACE

### 7.4.1 R&D Dashboard

**Navigation**: Dashboard → R&D Planning

**Layout** (Project Tree + Budget View):

```
┌────────────────────────────────────────────────────────────────┐
│ R&D PLANNING & DEVELOPMENT                    [TOKENS] [BUDGET]│
├────────────────────────────────────────────────────────────────┤
│                                                                  │
│ TOKEN BUDGET:  [████████░░] 3.5 / 8.0 tokens used (44%)        │
│ Available: 4.5 tokens  |  Burn Rate: 0.4 tokens/race           │
│                                                                  │
├────────────────────────────────────────────────────────────────┤
│                                                                  │
│ ACTIVE PROJECTS:                                               │
│                                                                  │
│ 📊 AERODYNAMICS - Aero Gen 2 (Floor Package)                   │
│    Status: DEVELOPMENT (Week 4 of 6)  |  Cost: 1.5 tokens      │
│    Research: ✓ Complete               |  Budget: $600K         │
│    Development: ████████░░ 80% done   |  Wind tunnel: 8/16h   │
│    Expected Deployment: Race 8        |  Gain: +0.18 sec/lap   │
│    Risk: LOW (conservative design)    |  [DETAILS] [ADJUST]    │
│                                                                  │
│ ⚙️ POWER UNIT - Engine Gen 2                                     │
│    Status: DEVELOPMENT (Week 2 of 4)  |  Cost: 1.0 tokens      │
│    Research: ✓ Complete               |  Budget: $400K         │
│    Development: ██████░░░░ 60% done   |  Dyno: 20/60 hours    │
│    Expected Deployment: Race 7        |  Gain: +0.08 sec/lap   │
│    Risk: MEDIUM (aggressive tuning)   |  [DETAILS] [ADJUST]    │
│                                                                  │
│ 🔧 CHASSIS - Suspension Gen 2                                   │
│    Status: RESEARCH (Week 2 of 3)     |  Cost: 1.0 tokens      │
│    Research: ██████░░░░ 65% done      |  Budget: $250K         │
│    Go/No-Go Decision: Week 3          |  Potential: +0.15 sec  │
│    Risk: MEDIUM                       |  [DETAILS] [ADJUST]    │
│                                                                  │
│ 🚦 RELIABILITY - Redundant Systems                              │
│    Status: RESEARCH (Week 1 of 2)     |  Cost: 0.5 tokens      │
│    Research: ███░░░░░░░ 30% done      |  Budget: $150K         │
│    Focus: Hydraulic & electrical      |  Benefit: -0.3% DNF    │
│    Risk: LOW                          |  [DETAILS] [ADJUST]    │
│                                                                  │
│ 📍 ABANDONED PROJECTS:                                          │
│    ✗ Aero Gen 3 (2.5 tokens freed)    |  Reason: Too risky    │
│    ✗ Suspension Gen 3 (2.5 tokens)    |  Complexity excessive │
│                                                                  │
├────────────────────────────────────────────────────────────────┤
│ [START NEW PROJECT] [PAUSE PROJECT] [ABANDON PROJECT]          │
│ [REALLOCATE TOKENS] [VIEW SCHEDULE] [FORECASTING]              │
│                                                                  │
└────────────────────────────────────────────────────────────────┘
```

### 7.4.2 R&D Project Detail Screen

**When clicking on a project (e.g., "Aero Gen 2"):**

```
┌────────────────────────────────────────────────────────────────┐
│ R&D PROJECT: AERODYNAMICS GEN 2 (Floor Package)          [X]   │
├────────────────────────────────────────────────────────────────┤
│                                                                  │
│ PROJECT OVERVIEW:                                              │
│ ├─ Current Phase: DEVELOPMENT                                  │
│ ├─ Progress: 80% (20 days complete, 5 days remaining)         │
│ ├─ Token Cost: 1.5 tokens (research 0.5, development 1.0)     │
│ ├─ Budget Spent: $380K / $600K allocated                       │
│ ├─ Expected Deployment: Race 8 (Belgium)                       │
│ └─ Expected Performance: +0.18 sec/lap                         │
│                                                                  │
│ RESEARCH PHASE (COMPLETED):                                    │
│ ├─ Concept: Explore new floor design for low-downforce tracks  │
│ ├─ Duration: 2 weeks                                           │
│ ├─ Cost: $200K                                                 │
│ ├─ Output: Research report, CFD analysis, manufacturing review │
│ └─ Decision: GO (proceed to development)                       │
│                                                                  │
│ DEVELOPMENT PHASE (IN PROGRESS):                               │
│ ├─ Duration: 6 weeks (Week 4 of 6)                            │
│ ├─ Team Lead: John Smith (Chief Aerodynamicist, Skill 86)     │
│ ├─ Supporting Engineers: 2 senior, 3 junior                    │
│ │                                                               │
│ ├─ Milestones:                                                │
│ │  ✓ Week 1: Detailed CAD design                              │
│ │  ✓ Week 2: Manufacturing feasibility review                 │
│ │  ✓ Week 3: Prototype production                             │
│ │  ✓ Week 4: Wind tunnel testing (6 hours used, 2 remaining) │
│ │  ⏳ Week 5: Final refinement (THIS WEEK)                     │
│ │  ⏳ Week 6: Production design (NEXT WEEK)                    │
│ │                                                               │
│ ├─ Manufacturing Plan:                                         │
│ │  ├─ Prototype: ✓ Complete (uses 2 CNC machines, 1 week)    │
│ │  ├─ Production tooling: Scheduled Week 6                     │
│ │  └─ Car assembly: Ready Race 7 (for Race 8 deployment)     │
│ │                                                               │
│ ├─ Risk Assessment:                                            │
│ │  ├─ Technical: LOW (conservative design, proven concept)    │
│ │  ├─ Manufacturing: LOW (tooling on schedule)                │
│ │  ├─ Reliability: LOW (extensive durability testing done)    │
│ │  └─ Overall: LOW RISK                                        │
│ │                                                               │
│ ├─ Contingency Actions Available:                              │
│ │  [EXPEDITE PRODUCTION] (+50% cost, -50% time)              │
│ │  [SINGLE CAR FIRST] (test Car #1, Car #2 Race 9)          │
│ │  [DEFER DEPLOYMENT] (push to Race 9)                       │
│ │  [ABANDON PROJECT] (frees 1.5 tokens)                      │
│ │                                                               │
│ └─ Next Action: Monitor Week 5 development, finalize design   │
│                                                                  │
│ [SAVE CHANGES] [CLOSE]                                         │
│                                                                  │
└────────────────────────────────────────────────────────────────┘
```

---

## 7.5 RACE WEEKEND INTERFACE

### 7.5.1 Race Weekend Hub

**Navigation**: Dashboard → Schedule → [Select Race]

**Pre-Race View** (Friday):

```
┌────────────────────────────────────────────────────────────────┐
│ RACE 8: BELGIUM (SPA-FRANCORCHAMPS)                       [X]   │
├────────────────────────────────────────────────────────────────┤
│                                                                  │
│  RACE SCHEDULE:                                                │
│  ├─ Friday 10:00 UTC: Free Practice 1 (60 min)                │
│  ├─ Friday 14:00 UTC: Free Practice 2 (90 min)                │
│  ├─ Saturday 11:00 UTC: Free Practice 3 (60 min)              │
│  ├─ Saturday 14:00 UTC: Qualifying Session (45 min)           │
│  ├─ Saturday 17:00 UTC: Parc Fermé (car sealed)              │
│  └─ Sunday 14:00 UTC: RACE (305 km)                           │
│                                                                  │
│  QUICK ACTIONS:                                               │
│  [SETUP PREFERENCES] [DRIVER BRIEFING] [PIT STRATEGY] [...]  │
│                                                                  │
├────────────────────────────────────────────────────────────────┤
│                                                                  │
│  SETUP OPTIMIZATION:                                            │
│  ┌──────────────────┬──────────────────┬──────────────────┐   │
│  │ CAR #1 SETUP     │ CAR #2 SETUP     │ WEATHER FORECAST │   │
│  │                  │                  │                  │   │
│  │ FP1 Testing:     │ FP1 Testing:     │ Friday: 18°C,    │   │
│  │ ○ Aggressive     │ ○ Balanced       │ Dry, Light wind  │   │
│  │ ○ Balanced       │ ○ Conservative   │                  │   │
│  │ ○ Conservative   │ ○ Experimental   │ Saturday: 15°C   │   │
│  │                  │                  │ Dry, increasing  │   │
│  │ [APPLY]          │ [APPLY]          │ wind 20 km/h     │   │
│  │                  │                  │                  │   │
│  │ Telemetry Data:  │ Telemetry Data:  │ Sunday: 12°C,    │   │
│  │ Optimal Setup    │ Experimental 20% │ Rain expected    │   │
│  │ Confidence: 95%  │ Confidence: 60%  │ 70% probability  │   │
│  │                  │                  │ [RAIN PLAN]      │   │
│  └──────────────────┴──────────────────┴──────────────────┘   │
│                                                                  │
│ [FINALIZE SETUP] [CONFIRM STRATEGY] [CHECK FUEL PLAN]         │
│                                                                  │
└────────────────────────────────────────────────────────────────┘
```

### 7.5.2 Live Race Monitor

**During Race (Real-Time View):**

```
┌────────────────────────────────────────────────────────────────┐
│ RACE 8: BELGIUM - LIVE MONITOR                    LAP 23 / 58  │
├────────────────────────────────────────────────────────────────┤
│                                                                  │
│  STANDINGS:                                                    │
│  POS | DRIVER         | GAP        | LAP TIME | TIRES | FUEL   │
│  ───┼────────────────┼────────────┼──────────┼───────┼────── │
│  1  │ Mercedes #1    | -          | 1:47.3   | MED   | 62kg  │
│  2  │ Red Bull #1    | +0.8s      | 1:48.1   | MED   | 65kg  │
│  3  │ YOUR CAR #1    | +2.1s      | 1:47.8   | MED   | 58kg  │
│      └─ Pit Stop: Lap 29 planned (Medium → Hard)             │
│  4  │ McLaren #1     | +4.5s      | 1:49.1   | SOFT  | 48kg  │
│  5  │ Alpine #1      | +6.2s      | 1:49.4   | MED   | 72kg  │
│  ... [6-10 omitted for brevity]                              │
│  12 │ YOUR CAR #2    | +28.5s     | 1:50.2   | SOFT  | 41kg  │
│                                                                  │
│ SECTOR TIMES (Current Lap):                                   │
│ Sector 1: 43.2s (⚠️ -0.1s vs best)                            │
│ Sector 2: 49.1s (✓ +0.0s vs best)                             │
│ Sector 3: 35.0s (⏳ in progress)                               │
│                                                                  │
├────────────────────────────────────────────────────────────────┤
│ DRIVER #1 CURRENT STATUS:                                      │
│                                                                  │
│ Position: 3rd | Pace: Good | Fuel: 58kg (remaining ~29 laps)  │
│ Tire: Medium (lap 8 of 30 estimated life) | Morale: 85%       │
│                                                                  │
│ STRATEGY:                                                      │
│ Plan: Two-stop (Soft 12 → Medium 30 → Hard 16)               │
│ First Pit Stop: Planned Lap 29 (in 6 laps)                    │
│  ├─ Tire change: Medium → Hard (5 tires)                      │
│  ├─ Fuel: Top up 30 kg                                        │
│  └─ Estimated pit time: 2.4 seconds                           │
│                                                                  │
│ ACTION PANEL:                                                  │
│ ┌─────────────────────────────────────────────────────────┐  │
│ │ [RADIO: "Box this lap?"] → Y / N / MAYBE                │  │
│ │ [INSTRUCTION: Defend/Attack/Manage/Caution] → [APPLY]  │  │
│ │ [PIT STRATEGY: Adjust timing?] → [EARLY/LATE/KEEP]     │  │
│ │ [WEATHER: Rain approaching (70% confidence, ETA: Lap 35)│  │
│ │           Prepare intermediate tires? → Y / N]          │  │
│ │ [TEAM MESSAGE: "Mercedes pulling away, pressure from   │  │
│ │  McLaren behind. Strategy is working."]                 │  │
│ └─────────────────────────────────────────────────────────┘  │
│                                                                  │
│ TELEMETRY PANEL:                                              │
│ [LAP CHART] [TIRE WEAR] [FUEL BURN] [COMPARATIVE ANALYSIS]   │
│                                                                  │
├────────────────────────────────────────────────────────────────┤
│ TEAM RADIO LOG:                                                │
│ Lap 20: "Tire wear moderate, maintaining pace"               │
│ Lap 18: "Mercedes pit stop, we'll undercut next"             │
│ Lap 15: "Good job defending Lap 14, Mercedes very close"     │
│                                                                  │
│ [PAUSE] [SETTINGS] [SPEED: 1x ▼] [CONTINUE RACE]            │
│                                                                  │
└────────────────────────────────────────────────────────────────┘
```

---

## 7.6 FINANCIAL MANAGEMENT INTERFACE

### 7.6.1 Budget Dashboard

**Navigation**: Dashboard → Finance → Budget Overview

```
┌────────────────────────────────────────────────────────────────┐
│ FINANCIAL MANAGEMENT - BUDGET OVERVIEW           [FORECAST] [X]│
├────────────────────────────────────────────────────────────────┤
│                                                                  │
│ CASH POSITION:                                                 │
│ Reserve Balance: $8.2M  |  Monthly Rate: $1.8M/month         │
│ Status: HEALTHY ✓  |  Burn Rate: Sustainable                │
│                                                                  │
├────────────────────────────────────────────────────────────────┤
│                                                                  │
│ Q3 BUDGET ALLOCATION & TRACKING:                              │
│                                                                  │
│ INCOME:                                                        │
│ ├─ Sponsorship Q3: $19.5M ✓ Received                          │
│ ├─ Prize Money (Races 13-18): $8M ✓ Received                  │
│ ├─ TV Rights Q3: $1.5M ✓ Received                             │
│ └─ Total Q3 Income: $29M                                       │
│                                                                  │
│ EXPENSES:                                                      │
│ ├─ Personnel: $22.5M / $45M total
│ │  └─ Details: Salaries $21M, Bonuses $1.5M                   │
│ │                                                               │
│ ├─ R&D: $15M / $50M total
│ │  └─ Details: Aero $6M, Power Unit $5M, Chassis $4M         │
│ │                                                               │
│ ├─ Operations: $12M / $25M total
│ │  └─ Details: Logistics $5M, Travel $5M, Facilities $2M     │
│ │                                                               │
│ ├─ IT & Telemetry: $2.5M / $10M total
│ │  └─ Details: Servers $1.2M, Software $1.3M                  │
│ │                                                               │
│ └─ Total Q3 Expenses YTD: $52M                                 │
│                                                                  │
│ NET POSITION:                                                  │
│ YTD Income: $57.5M                                             │
│ YTD Expenses: $54.5M                                           │
│ YTD Net: +$3M ✓ POSITIVE                                       │
│                                                                  │
├────────────────────────────────────────────────────────────────┤
│                                                                  │
│ SEASON FORECAST (if current performance continues):            │
│                                                                  │
│ Total Income Projection: $111M                                 │
│ (Sponsorship adjusted for position: $75M, Prizes: $30M, TV: $6M)│
│                                                                  │
│ Total Expense Limit: $145M (Cost Cap hard limit)              │
│                                                                  │
│ PROJECTED SHORTFALL: -$34M ⚠️ ACTION REQUIRED                  │
│                                                                  │
│ Options:                                                       │
│ [REDUCE R&D SPENDING] [SALARY CUTS] [EMERGENCY LOAN]         │
│ [RENEGOTIATE SPONSORSHIPS] [IMPROVE CHAMPIONSHIP POSITION]    │
│                                                                  │
│ [DETAILS] [HISTORICAL] [FORECAST ADJUSTMENTS]                 │
│                                                                  │
└────────────────────────────────────────────────────────────────┘
```

---

## 7.7 SETTINGS & PREFERENCES

### 7.7.1 Settings Menu

**Navigation**: Menu → Settings

```
┌────────────────────────────────────────────────────────────────┐
│ SETTINGS                                                   [X]   │
├────────────────────────────────────────────────────────────────┤
│                                                                  │
│ GAME SETTINGS:                                                 │
│ ├─ Difficulty: ○ Easy ○ Normal ◉ Hard ○ Realistic             │
│ ├─ Auto-Pit Suggestions: ◉ On ○ Off                            │
│ ├─ AI Aggressiveness: [████░░░░░░] 4/10                       │
│ ├─ Realistic Mechanics: ◉ On ○ Off                             │
│ │  └─ Includes: Cost Cap, ATR, Tire degradation, Fuel         │
│ └─ Tutorial Hints: ◉ On ○ Off                                  │
│                                                                  │
│ INTERFACE:                                                     │
│ ├─ Theme: ◉ Dark ○ Light                                      │
│ ├─ Text Size: ○ Small ○ Normal ◉ Large                       │
│ ├─ Animation Speed: [████░░░░░░] 5/10 (medium)                │
│ ├─ Notifications: ◉ On ○ Off                                  │
│ │  └─ Alert types: 🔴 Critical, 🟡 Warning, 🟢 Info          │
│ └─ Dashboard Layout: ◉ Customizable ○ Default                 │
│                                                                  │
│ AUDIO:                                                         │
│ ├─ Music Volume: [██████████] 100%                            │
│ ├─ Effects Volume: [████████░░] 80%                            │
│ ├─ Team Radio: ◉ On ○ Off                                     │
│ └─ Ambient Sound: ◉ On ○ Off                                  │
│                                                                  │
│ GAMEPLAY:                                                      │
│ ├─ Simulation Speed: ◉ Real-Time (1x) ○ 2x ○ 5x ○ 10x ○ 20x  │
│ ├─ Pause During Race: ◉ Always ○ Only setups ○ Never         │
│ ├─ Autosave: ◉ Every race ○ Every day ○ Manual only          │
│ └─ Save/Load: [QUICK SAVE] [LOAD SAVE] [DELETE SAVE]         │
│                                                                  │
│ ACCESSIBILITY:                                                 │
│ ├─ Colorblind Mode: ○ None ◉ Deuteranopia ○ Protanopia      │
│ ├─ High Contrast: ○ Off ◉ On                                  │
│ ├─ Screen Reader: ◉ On ○ Off                                  │
│ └─ Keyboard Shortcuts: [CUSTOMIZE]                            │
│                                                                  │
│ [RESTORE DEFAULTS] [APPLY CHANGES] [CLOSE]                   │
│                                                                  │
└────────────────────────────────────────────────────────────────┘
```

---

## 7.8 KEYBOARD SHORTCUTS & CONTROLS

### 7.8.1 Default Shortcuts

**Common Navigation:**
```
ESC          = Return to previous screen / Main menu
H            = Help / Tutorials
D            = Dashboard (home)
T            = Team Management
R            = R&D Planning
F            = Finance / Budget
S            = Schedule / Races
P            = Pause (during race)
```

**During Race:**
```
SPACE        = Pause/Resume
1            = Driver #1 focused view
2            = Driver #2 focused view
T            = Tire strategy panel
F            = Fuel management panel
W            = Weather/Radar panel
R            = Team radio / Communication
P            = Pit stop planner
L            = Lap chart / Standings
TAB          = Cycle through information panels
+ / -        = Increase/Decrease simulation speed (1x to 20x)
```

**All shortcuts customizable in Settings**

---

## 7.9 MOBILE/TABLET RESPONSIVE DESIGN

### 7.9.1 Tablet Layout (iPad, 12.9" landscape)

**Dashboard adapts to smaller screen:**
- Single-column layout (instead of multi-column)
- Larger touch targets (buttons 48px minimum)
- Horizontal swipe navigation between major sections
- Collapsible panels for detail views
- Landscape orientation required for race monitor (critical data density)

### 7.9.2 Mobile Layout (Not Officially Supported)

**Limited support for phones (6"+):**
- Dashboard only (simplified)
- Team management (personnel list only)
- Finance overview (basic budget)
- Schedule view
- Cannot run live race (too complex for small screen)

---

## 7.10 ACCESSIBILITY & LOCALIZATION

### 7.10.1 Language Support

**Supported Languages:**
- English (primary)
- French
- German
- Spanish
- Italian
- Portuguese
- Japanese
- Chinese (Simplified)
- Russian
- Dutch

**Localization includes:**
- All UI text
- Tooltips and help
- Team names and driver names (local variations)
- Currency (€, £, ¥, ₽, etc.)
- Date/time formatting

### 7.10.2 Accessibility Features

**Inclusive Design:**
- WCAG 2.1 AA compliance (minimum standard)
- Screen reader support (NVDA, JAWS, VoiceOver)
- High contrast mode (for visually impaired)
- Colorblind modes (Deuteranopia, Protanopia, Tritanopia)
- Keyboard-only navigation (no mouse required)
- Text scaling (up to 200% comfortable reading)
- Audio descriptions for critical race events

---

## 7.11 ANIMATION & VISUAL FEEDBACK

### 7.11.1 Key Transitions

**All animations 400-600ms duration (responsive, not sluggish):**

```
Screen Transitions:     Fade + slide (200ms fade, 400ms slide)
Button Clicks:         Slight scale (0.95x) + color change (instant)
Data Updates:          Subtle color flash (200ms) then fade to normal
Notifications:         Slide in from top (300ms), auto-dismiss (5s)
Toggle Switches:       Smooth slide animation (300ms)
Menu Expansion:        Height animation (200ms ease-in-out)
```

**Visual Feedback Principles:**
- Immediate response to input (no delay > 100ms)
- Color change + animation (reinforces state change)
- Sound effects optional (can be disabled in settings)
- Subtle particles/effects (celebratory podium, pit stop success)
- No jarring transitions (smooth ease functions, not linear)

---

## 7.12 ERROR HANDLING & CONFIRMATIONS

### 7.12.1 Destructive Action Confirmations

**When player attempts actions that cannot be undone:**

```
Example: Firing an engineer

Dialog Box:
┌──────────────────────────────────────────┐
│ ⚠️ CONFIRM ACTION                  [X]   │
├──────────────────────────────────────────┤
│                                          │
│ You are about to fire John Smith        │
│ (Chief Aerodynamicist, Skill 86)        │
│                                          │
│ Consequences:                            │
│ • Loses $280K/year salary (savings)     │
│ • Loses ~0.8x R&D multiplier (loss)     │
│ • Aerodynamics dept morale -20%         │
│ • Team morale -10%                      │
│ • Retention risk for other engineers    │
│                                          │
│ This action CANNOT be undone.           │
│                                          │
│ [CANCEL] [CONFIRM FIRING]              │
│                                          │
└──────────────────────────────────────────┘
```

**All critical actions have:**
- Clear description of consequences
- Undo option if possible (within 60 seconds)
- Confirmation requirement
- Warning color (red) for danger

---

**[END OF PART 7: UI/UX DESIGN]**

**Total Pages**: 20-25 pages

This design document provides comprehensive UI/UX covering all major screens, navigation patterns, accessibility requirements, and visual design principles. All screens are optimized for information density and intuitive navigation across three major game cycles.

---

## APPENDIX: GDD COMPLETION STATUS

**Complete Game Design Document: Autosport Manager 2027**

| Part | Title | Pages | Status |
|------|-------|-------|--------|
| 1 | Introduction & Design Pillars | 20 | ✅ Complete |
| 2 | Season Management System | 100+ | ✅ Complete |
| 3 | Team Management System | 45-50 | ✅ Complete |
| 4 | R&D & Vehicle Development | 40-45 | ✅ Complete |
| 5 | Finance & Sponsorship | 35-40 | ✅ Complete |
| 6 | Race Simulation & Mechanics | 30-35 | ✅ Complete |
| 7 | UI/UX Design | 20-25 | ✅ Complete |

**Total GDD**: 290-315 pages

**Next Phases (Beyond GDD):**
- Technical Design Document (TDD): Systems architecture, data models, code structure
- Prototype Development: First race weekend playable demo
- Risk Assessment & Mitigation Strategies
- Production Roadmap & Development Schedule (8-10 weeks pre-production)
