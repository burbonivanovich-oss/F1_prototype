# PART 7: UI/UX DESIGN

**Updated for F1 2024-2026 Game Mechanics Realism**

---

## 7.1 DESIGN PHILOSOPHY

**Core UX Principles for Autosport Manager 2027:**

1. **Information Density**: Deep complexity accessible through progressive disclosure
2. **Accessibility**: Multiple information paths (summary + detailed views)
3. **Real-Time Feedback**: Immediate visual confirmation of all actions
4. **Visual Hierarchy**: Championship > Drivers > Deltas; Red > Yellow > Blue > Gray
5. **Context-Sensitive**: UI changes based on season phase (pre-season/in-season/post-race)

**Design Style**: Modern F1 aesthetic
- Dark theme (black/dark gray), bright accents (FIA red, team colors)
- Clean typography, minimalist icons, 400-600ms transitions

**CRITICAL UPDATES FOR 2024-2026 REGULATIONS**:
- ✅ Technology Tokens COMPLETELY REMOVED (Evolution/Major Update only)
- ✅ 7 key department heads tracked + abstract Team Base Level
- ✅ Wind Tunnel/CFD hours tracking (no token budgets)
- ✅ Annual prize distribution model
- ✅ Formation Lap + DRS mechanics
- ✅ 5 tire compounds (C1-C5), no mandatory change rule
- ✅ No FP4/Warm-Up sessions
- ✅ Cost Cap $135M with proper exclusions
- ✅ Sprint weekends (FP1+Q Friday, SQ+Sprint Saturday, Race Sunday)

---

## 7.2 MAIN DASHBOARD

**At-a-glance team status** showing championship position, cash, morale, upcoming race details.

Key Widget Updates:
- Championship Position (yes, no customize)
- Cash Position (yes, no customize)
- Team Morale (yes, no customize)
- Upcoming Race (yes, yes)
- **R&D Status**: Shows Wind Tunnel hrs / CFD units (not tokens!)
- **Department Heads**: 7 key leads shown with morale/retention risk
- Recent Events (yes, yes)
- Alerts (yes, no customize)
- Sponsor Status (no, yes)
- Driver Performance (no, yes)
- Financial Forecast (no, yes)
- Reliability Tracker (no, yes)

**Quick Actions Cards Update**:
- Team Management: "7 Key Leads | Morale: 72%"
- R&D Planning: "Wind Tunnel: 35/80 hrs | CFD: 450/1400 units"
- Budget Review: "Q3: $45M spent | Reserve: $8.2M"

---

## 7.3 TEAM MANAGEMENT INTERFACE

### Personnel System (SIMPLIFIED)

**Only 7 Department Heads tracked individually**:
1. Chief Aerodynamicist (Aerodynamics lead)
2. Chief Power Unit Engineer (Engine/ERS lead)
3. Chief Structure Engineer (Chassis lead)
4. Chief Reliability Engineer (Durability lead)
5. Head of Manufacturing Operations (Production lead)
6. Chief Data Analyst/Strategy Lead (Telemetry lead)
7. Operations Manager (General ops lead)

**All other 45-60 team members** represented as **Departmental Base Level (1-100)**:
- Higher = more capable team
- Affects R&D speed, quality, morale
- Improves via hiring/team culture

**Attributes per Lead**: Skill, Loyalty, Morale, Salary, Contract, Specialization, Retention Risk

**Actions**: [RAISE SALARY], [RETENTION BONUS], [NEGOTIATE CONTRACT], [TRANSFER REQUEST]

---

## 7.4 R&D PLANNING INTERFACE

### NO TECHNOLOGY TOKEN SYSTEM

**Resource Tracking Instead**:
- **Wind Tunnel Hours**: ATR-limited (56-80 hrs/year typical)
- **CFD Units**: ATR-limited (1120-1840 units/year typical)
- **Development Budget**: From $135M Cost Cap
- **Dyno Hours**: Power Unit projects only

**Project Types**:

**EVOLUTION** (Minor Improvement):
- Research: 4-10 weeks, $200K-600K
- Development: 6-12 weeks, $500K-1.2M
- Gain: +0.05-0.10 sec/lap
- Resources: Low-moderate wind tunnel/CFD

**MAJOR UPDATE** (Significant Improvement):
- Research: 6-10 weeks, $600K-1.5M
- Development: 8-12 weeks, $1.2M-3M
- Gain: +0.15-0.35 sec/lap
- Resources: High wind tunnel/CFD
- Risk: Medium-High

**Dashboard Shows**:
- Project name & scope
- Phase (research/development)
- Progress % & timeline
- Resources used vs allocated (hours, units, budget)
- Expected performance gain
- Risk assessment
- Contingency options (expedite, defer, abandon)

---

## 7.5 RACE WEEKEND INTERFACE

### Race Schedule (CORRECTED FOR REALISM)

**TRADITIONAL WEEKEND**:
- Friday: FP1 (60 min), FP2 (60 min)
- Saturday: FP3 (45 min), Qualifying (60 min, 3 sessions)
- **PARC FERMÉ**: Activated after Q1 (frozen until race)
- Sunday: Formation Lap (30 min before), RACE (305 km)
- **REMOVED**: FP4 session, Warm-Up session

**SPRINT WEEKEND**:
- Friday: FP1 (60 min), Qualifying (60 min) for Sunday race
- Saturday: Sprint Qualifying (30 min), Sprint Race (~25 min)
- **PARC FERMÉ RESET**: After sprint, setup changes allowed
- Sunday: Formation Lap (30 min before), MAIN RACE (305 km)

### Tire Strategy Screen (5 COMPOUNDS)

**Available C1-C5** (3 selected per weekend):
- C1: 45-55 laps, -0.20 sec/lap
- C2: 35-45 laps, -0.10 sec/lap
- C3: 25-35 laps, baseline
- C4: 15-25 laps, +0.15 sec/lap
- C5: 10-18 laps, +0.25 sec/lap

**Rules**:
- Minimum 2 different compounds (required)
- Can reuse same compound if separated by another
- Example: C5→C3→C5 ✓, C5→C5 ✗

**Pit Stop**: ~2.0-2.8 seconds (tire only, no refueling, 110kg locked)
- Cold tire penalty: -1.5 sec first lap
- Random error risk: 1% (4-5 sec critical failure)

### Live Race Monitor (UPDATED)

**Pre-Race**:
- Formation Lap: 30 min before race, no overtaking

**During Race Shows**:
- Standings, gaps
- Tire age/fuel remaining
- **DRS**: 1-second gap requirement, disabled first 2 laps/in rain
- **Penalties Available**: 5-sec, drive-through, stop-and-go
- **Red Flag Mechanic**: Severe incident stops race, free tire change
- **Safety Car/VSC**: Strategic pit opportunities
- **Weather**: Dynamic rain/temp affecting tire wear

**Driver Instructions**: Push / Standard / Defend / Fuel Save
- No Qualifying Mode (removed - banned 2020)
- Attack mode: ERS battery-based (not unlimited)
- Defend mode: Dynamic loss 0.1-0.4 sec/lap

---

## 7.6 FINANCE INTERFACE

### Financial Dashboard

**Cost Cap Status**:
- Budget: $135M annually (updated from $145M)
- Exclusions: Driver salaries (2x), Technical Director, marketing, legal, FIA fees
- Tracked weekly/quarterly
- Penalties Tiered: 0-5% = fine+ATR cut; 5-10% = fine+ATR+points; >10% = exclusion

**Annual Prize Pool Distribution**:
- 1st: ~$100M | 2nd: ~$80M | 3rd: ~$60M | 4th: ~$45M | 5th: ~$35M
- (continues to 10th: ~$5M)
- Mid-season advances: $5M every 6 races
- Sprint bonuses: $50-100K per race win

**Driver Bonuses**:
- Win: $500K (from annual pool)
- Podium: $250K (from annual pool)
- Championship: Additional % if top 3

**Sponsor KPIs** (AS BONUSES, NOT PENALTIES):
- Base payment: GUARANTEED
- Bonus pool: Conditional on targets
- Examples: Podium target = +$500K, Win target = +$750K
- **No mid-season termination** (only off-season review)

**Heritage Bonuses** (if applicable):
- Ferrari: $70-100M/year
- Mercedes: $25-35M/year
- McLaren: $15-25M/year

---

## 7.7 CONSISTENCY STANDARDS

### Terminology

| Old | New | Meaning |
|-----|-----|---------|
| Gen 1.5 | Evolution | Minor (4-10 weeks research, 6-12 dev) |
| Gen 2 | Major Update | Significant (6-10 weeks research, 8-12 dev) |
| Gen 3 | (Removed) | No longer exists |
| Tokens | (Removed) | Replaced by Wind Tunnel/CFD/Budget |
| 52 Engineers | 7 Leads + Team Base | 7 tracked + abstract team |
| FP4 | (Removed) | Never in real F1 post-2022 |
| Warm-Up | (Removed) | Cancelled 2003 |

### Resources Tracked
- **Wind Tunnel**: Hours (ATR limit 56-80/year)
- **CFD**: Units (ATR limit 1120-1840/year)
- **Budget**: Dollars from $135M Cost Cap
- **Dyno**: Hours (PU projects only)
- **Real Time**: Weeks/months

### Race Weekend Standards
- NO FP4, NO Warm-Up
- Formation Lap: 30 min before (mandatory)
- Parc Fermé: Q1 (not Q2)
- Sprint weekends: proper structure
- 5 tire compounds available
- Min 2 different required per race
- Cold penalty: 1.5 sec first lap
- DRS: 1-sec gap, disabled first 2 laps
- Penalties: 5-sec, drive-through, stop-and-go

### Finance Standards
- Cost Cap: $135M
- Exclusions: Driver salaries, TD, marketing, legal, FIA fees
- Distribution: Annual pool (not per-race)
- Mid-season advances: $5M every 6 races
- Sponsor KPIs: Bonuses (not penalties)
- Contracts: Fixed-term (no mid-season termination)

### Visual Indicators
- Evolution: 🔵 Blue
- Major Update: 🟠 Orange
- Research: 🟡 Yellow
- Development: 🟢 Green
- Abandoned: ⚫ Gray
- Wind Tunnel: 🔷 Cyan
- CFD: 🟣 Purple
- Budget: 💵 Green/Red
- Alerts: 🔴 Red, 🟡 Yellow, 🔵 Blue, ⚪ Gray
