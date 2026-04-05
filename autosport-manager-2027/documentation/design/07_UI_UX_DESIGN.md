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

**UPDATED FOR NEW GAME SYSTEMS (2024-2026 Regulations)**:
- ✅ Technology Tokens COMPLETELY REMOVED (replaced by Evolution/Major Update)
- ✅ Personnel simplified to 7 key department heads + abstract team structure
- ✅ Wind Tunnel/CFD tracking (no token budgets)
- ✅ Annual prize distribution (no race-by-race payments)
- ✅ Formation Lap + DRS mechanics in race UI
- ✅ 5 tire compounds (C1-C5) instead of 3
- ✅ No FP4 or Warm-Up sessions
- ✅ Cost Cap $135M (updated from $145M)
- ✅ Sprint weekends with proper structure

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
│  UPCOMING RACE: BELGIUM (Race 8 of 24) - SPRINT WEEKEND    │
│  ├─ Friday: FP1 (60 min) + Qualifying (60 min) for Sunday race  │
│  ├─ Saturday: Sprint Qualifying (30 min) + Sprint Race (~25 min)│
│  └─ Sunday: MAIN RACE (305 km, ~90 min real-time)  (in 5 days) │
│                                                                   │
│  QUICK ACTIONS:                                                  │
│  ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐ │
│  │  TEAM MANAGEMENT │ │  R&D PLANNING    │ │  BUDGET REVIEW   │ │
│  │  7 Key Leads     │ │  Wind Tunnel:    │ │  Q3: $45M spent  │ │
│  │  Morale: 72%    │ │  35/80 hrs used  │ │  Reserve: $8.2M  │ │
│  │  [MANAGE]        │ │  [PLAN]          │ │  [DETAILS]       │ │
│  └──────────────────┘ └──────────────────┘ └──────────────────┘ │
│                                                                   │
├─────────────────────────────────────────────────────────────────┤
│  RECENT EVENTS:                                                  │
│  ├─ Race 7 Podium: 3rd place (EXCELLENT) - Annual prize pool    │
│  ├─ Driver #2 morale low (62%) - MONITOR                        │
│  ├─ Aero Evolution deployment confirmed for Race 8              │
│  ├─ TechCorp sponsor satisfied (85%, bonus available)           │
│  └─ Engine Major Update development 60% complete, on track      │
│                                                                   │
├─────────────────────────────────────────────────────────────────┤
│  ALERTS & NOTIFICATIONS:                                         │
│  🔴 URGENT: Driver #2 contract expires Race 12 - NEGOTIATE NOW  │
│  🟡 WARNING: Chief Aerodynamicist has Ferrari offer             │
│  🟢 INFO: Wind tunnel session allocated for this week           │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

### 7.2.2 Dashboard Widgets

**Player can customize by dragging, resizing, hiding/showing, and saving layouts**

| Widget | Default | Data Shown |
|--------|---------|-----------|
| Championship Position | Yes | Points, position, gap |
| Cash Position | Yes | Budget balance, monthly rate |
| Team Morale | Yes | Percentage, trend |
| Upcoming Race | Yes | Schedule, session times |
| R&D Status | Yes | Wind Tunnel/CFD usage, active projects |
| Department Heads | Yes | 7 key leads, morale, retention risk |
| Recent Events | Yes | Last 5 events |
| Alerts | Yes | Critical notifications |
| Sponsor Status | No | Satisfaction scores, KPI tracking |
| Driver Performance | No | Points, morale per driver |
| Financial Forecast | No | Annual distribution projection |
| Reliability Tracker | No | DNF probability |

---

## 7.3 TEAM MANAGEMENT INTERFACE

### 7.3.1 Department Heads Management Screen

**Navigation**: Dashboard → Team → Department Heads

Only 7 key department heads are tracked individually:
- Chief Aerodynamicist (Aerodynamics lead)
- Chief Power Unit Engineer (Engine/ERS lead)
- Chief Structure Engineer (Chassis/Suspension lead)
- Chief Reliability Engineer (Durability lead)
- Head of Manufacturing Operations (Production lead)
- Chief Data Analyst/Strategy Lead (Telemetry/Strategy lead)
- Operations Manager (General team ops lead)

All other team members (45-60 engineers, mechanics, etc.) are represented as **Departmental Base Level** (1-100 scale):
- Higher Base Level = more capable team
- Affects research speed, quality, and morale impact
- Can be improved by hiring better people or team culture investments

**Key Attributes for Each Lead**:
- Skill (1-100): Competence in their specialty
- Loyalty (1-100): Likelihood to stay vs. accept external offers
- Morale (1-100): Current job satisfaction
- Salary: Annual compensation
- Contract: Years remaining
- Specialization: Primary focus area
- Retention Risk: GREEN/YELLOW/RED based on loyalty + external offers

**Actions Available**:
- [RAISE SALARY] - Improve retention (loyalty +5%, cost +$50K)
- [RETENTION BONUS] - One-time boost to loyalty (loyalty +20%, cost $100-500K)
- [NEGOTIATE CONTRACT] - Extend or revise terms
- [TRANSFER REQUEST] - Buy them out/release them

---

## 7.4 R&D PLANNING INTERFACE

### 7.4.1 R&D Dashboard

**Navigation**: Dashboard → R&D Planning

**NO TECHNOLOGY TOKEN SYSTEM** - Instead tracks:
- **Wind Tunnel Hours**: ATR-limited (typically 56-80 hrs/year)
- **CFD Units**: ATR-limited (typically 1120-1840 units/year)
- **Development Budget**: From Cost Cap ($200K-1.5M per research, $500K-3M per development)
- **Dyno Hours**: For Power Unit projects only (limited)

**Project Types**:
1. **Evolution** (Minor Improvement)
   - Research: 4-10 weeks, $200K-600K
   - Development: 6-12 weeks, $500K-1.2M
   - Expected gain: +0.05-0.10 sec/lap
   - Resources: Low to moderate wind tunnel/CFD

2. **Major Update** (Significant Improvement)
   - Research: 6-10 weeks, $600K-1.5M
   - Development: 8-12 weeks, $1.2M-3M
   - Expected gain: +0.15-0.35 sec/lap
   - Resources: High wind tunnel/CFD requirements
   - Risk: Medium to High

**Active Projects Dashboard Shows**:
- Project name and scope
- Current phase (research/development)
- Progress % and timeline
- Resources used vs. allocated (wind tunnel hours, CFD units, budget spent)
- Expected performance gain
- Risk assessment
- Contingency options (expedite, defer, abandon)

---

## 7.5 RACE WEEKEND INTERFACE

### 7.5.1 Race Weekend Schedule

**TRADITIONAL WEEKEND**:
- Friday: FP1 (60 min), FP2 (60 min)
- Saturday: FP3 (45 min), Qualifying (60 min, 3 sessions)
- **PARC FERMÉ**: Activated after Q1 (not Q2) - all setup changes frozen
- Sunday: Formation Lap (30 min before race), RACE (305 km)
- **NO FP4 SESSION** (removed - not in real F1 2023+)
- **NO WARM-UP SESSION** (removed - cancelled 2003)

**SPRINT WEEKEND**:
- Friday: FP1 (60 min), Qualifying (60 min) for Sunday main race
- Saturday: Sprint Qualifying (30 min) for sprint grid, Sprint Race (~25 min)
- **PARC FERMÉ RESET**: After sprint, teams can adjust setup for Sunday race
- Sunday: Formation Lap, MAIN RACE (305 km)

### 7.5.2 Tire Strategy Screen

**Available Compounds** (All 5, three selected per weekend):
- **C1 (Hard)**: Durability 45-55 laps, grip -0.20 sec/lap, for long stints
- **C2 (Medium Hard)**: Durability 35-45 laps, grip -0.10 sec/lap
- **C3 (Medium)**: Durability 25-35 laps, grip baseline, versatile
- **C4 (Soft)**: Durability 15-25 laps, grip +0.15 sec/lap
- **C5 (Softest)**: Durability 10-18 laps, grip +0.25 sec/lap, one-lap pace

**Tire Strategy Rules**:
- Minimum 2 different compounds per race (required)
- Can use same compound twice if separated by another
- Example: C5 → C3 → C5 allowed; C5 → C5 NOT allowed
- Cold tire penalty: -1.5 sec first lap (requires active warm-up)

**Pit Stop Time**: ~2.0-2.8 seconds
- Includes tire change only (no refueling - max 110kg fuel locked)
- Also includes 0.3 second front wing adjustment (done in parallel)
- Random crew error risk 1% (critical pit stop failure = 4-5 sec)

---

## 7.6 RACE INTERFACE

### 7.6.1 Live Race Monitor

**Pre-Race (Formation Lap)**:
- 30 minutes before race start
- No overtaking allowed
- First car appearance on track
- Any off-track excursion = pit lane start penalty

**During Race**:
- **Live Standings**: Current positions, gaps
- **Driver Status**: Tire age, fuel remaining, last lap time
- **Team Radio**: Two-way communication
- **Pit Stop Options**: When to pit, which compound
- **Driver Instructions**: Push / Standard / Defend / Fuel Save
- **Incident Log**: Safety car, red flag, collisions

**Race Mechanics Available**:
- **DRS System**: 1-second gap requirement, disabled first 2 laps/restart
- **Penalty System**: 5-sec, drive-through, stop-and-go
- **Red Flag**: Severe incident stops race, free tire change allowed
- **Safety Car/VSC**: Strategic pit stop opportunities
- **Weather**: Dynamic conditions (rain, dry, temperatures affect tire wear)

### 7.6.2 Tire & Fuel Management

- **No Refueling**: Max 110kg fuel locked at race start
- **Fuel Consumption**: Via engine modes only (Standard/Fuel Save/Attack)
- **Cold Tire Penalty**: -1.5 sec first lap on fresh tires (requires warm-up laps)
- **Tire Degradation**: Dynamic based on driving style, temperature, track condition
- **Compound Changes**: Minimum 2 different required per race

---

## 7.7 FINANCE INTERFACE

### 7.7.1 Financial Dashboard

**Cost Cap Status**:
- Budget: $135M annually (updated from $145M)
- Exclusions: Driver salaries (2x), Technical Director salary, marketing, legal, FIA fees
- Tracked weekly and quarterly
- Penalties: Tiered (0-5% = fine+ATR cut; 5-10% = fine+ATR+points; >10% = exclusion)

**Annual Prize Pool Distribution**:
- 1st Constructor: ~$100M
- 2nd Constructor: ~$80M
- 3rd Constructor: ~$60M
- 4th Constructor: ~$45M
- 5th Constructor: ~$35M
- (continues to 10th place: ~$5M)
- Mid-season advances: $5M every 6 races
- Sprint bonuses: $50-100K per race win

**Driver Bonuses**:
- Win bonus: $500K per race win (paid from annual pool)
- Podium bonus: $250K per podium finish (paid from annual pool)
- Championship bonus: Additional percentage if top 3 finishers

**Sponsor KPI System**:
- Base payment: GUARANTEED (not conditional)
- Bonus pool: Conditional on KPI targets
- Examples:
  - Podium target (8 per season) = +$500K bonus
  - Race wins (3 per season) = +$750K bonus
  - Points total (minimum 100) = +$250K bonus
- Mid-season review: Adjust targets if falling behind
- No sponsor termination mid-season (only off-season review)

**Historical Heritage Bonuses** (for applicable teams):
- **Ferrari**: $70-100M/year (heritage and brand value)
- **Mercedes**: $25-35M/year (dominant history)
- **McLaren**: $15-25M/year (iconic status)

---

## 7.8 UI/UX CONSISTENCY ACROSS ALL SYSTEMS

### Terminology Standards

| Old Term | New Term | Meaning |
|----------|----------|---------|
| Gen 1.5 | Evolution | Minor improvement (~4-10 week research, 6-12 week development) |
| Gen 2 | Major Update | Significant improvement (~6-10 week research, 8-12 week development) |
| Gen 3 | (Removed) | No longer exists - too simplistic for realism |
| Technology Tokens | (Removed) | Replaced by Wind Tunnel/CFD/Budget tracking |
| 52 Individual Engineers | Department Heads (7) + Team Base (1-100) | 7 tracked + abstract team structure |
| FP4 Session | (Removed) | Never existed in real F1 after 2022 |
| Warm-Up Session | (Removed) | Cancelled 2003, no longer in regulations |

### Resource Tracking Standards

All R&D projects track:
- **Wind Tunnel Hours**: Limited by ATR system (56-80 hrs/year typical)
- **CFD Units**: Limited by ATR system (1120-1840 units/year typical)
- **Development Budget**: From $135M Cost Cap
- **Dyno Hours**: For Power Unit projects only (limited allocation)
- **Real-World Time**: Weeks/months (not abstract tokens)

### Race Weekend Standards

All race weekends follow proper F1 schedule:
- **NO FP4** session
- **NO Warm-Up** session
- **Formation Lap**: 30 minutes before race (mandatory)
- **Parc Fermé**: Activated Q1 (not Q2)
- **Sprint Weekends**: Proper structure (FP1 Fri, SQ+Sprint Sat, Race Sun)
- **5 Tire Compounds**: C1-C5 available, 3 selected per race
- **Minimum 2 Different Compounds**: Required per race
- **Cold Tire Penalty**: 1.5 sec first lap (not 0.2-0.4 sec)
- **DRS System**: 1-second gap requirement, disabled first 2 laps
- **Penalty System**: 5-sec, drive-through, stop-and-go available

### Finance Standards

- **Cost Cap**: $135M (not $145M)
- **Exclusions**: Driver salaries, Technical Director, marketing, legal, FIA fees
- **Prize Distribution**: Annual pool (not race-by-race)
- **Mid-Season Advances**: $5M every 6 races
- **Sponsor KPIs**: Bonuses (not penalties)
- **Sponsor Contracts**: Fixed-term (no mid-season termination)

### Visual Indicators

**Project Status Colors**:
- Evolution: 🔵 Blue
- Major Update: 🟠 Orange
- Research Phase: 🟡 Yellow
- Development Phase: 🟢 Green
- Abandoned: ⚫ Gray

**Resource Indicators**:
- Wind Tunnel: 🔷 Cyan
- CFD: 🟣 Purple
- Budget: 💵 Green/Red based on status
- Dyno: ⚙️ Gray

**Alert Levels**:
- Critical: 🔴 Red
- Warning: 🟡 Yellow
- Information: 🔵 Blue
- Neutral: ⚪ Gray
