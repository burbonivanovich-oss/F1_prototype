# PART 7: UI/UX DESIGN

**Comprehensive Interface Design for Autosport Manager 2027**
**Aligned with F1 2025-2026 Regulations and Advanced Game Mechanics**

---

## 7.1 DESIGN PHILOSOPHY

**Core UX Principles:**

1. **Information Density**: Deep complexity accessible through progressive disclosure
2. **Real-Time Feedback**: Immediate visual confirmation of all actions
3. **Three-Tier Alerts**: Critical (🔴) vs Attention-needed (🟡) vs Info (🔵)
4. **Context-Sensitive**: UI adapts to season phase (pre-season/in-season/post-race)
5. **Dark Theme Modern F1 Aesthetic**: Black/dark gray with FIA red and team color accents
6. **Performance Optimized**: 150-200ms smooth transitions, instant mode toggle for experienced players

**2025-2026 Regulations Implementation**:
- ✅ Cost Cap $215M with ATR sliding scale (70-115%)
- ✅ Prize pool $1.6B (75% championship-based, 20% heritage, 5% Ferrari legacy)
- ✅ Six tire compounds (C1-C6) with track-dependent degradation
- ✅ Three-phase tire wear: plateau → linear → cliff
- ✅ Realistic pit stops (1.82-2.5 sec, crew morale-dependent)
- ✅ Budget-based R&D (no technology tokens)
- ✅ MGU-K energy management system
- ✅ Dirty air aerodynamic effects
- ✅ Engine power modes (Qualifying/Standard/Economy) with reliability impact
- ✅ Mental load system (driver concentration, not fixed fatigue)
- ✅ Undercut/Overcut strategy visualization
- ✅ No fastest lap bonus (removed 2025)
- ✅ ADUO checks for power units only (3/season)

---

## 7.2 MAIN DASHBOARD ARCHITECTURE

**Layout**: Tabbed interface with 5-7 widgets max per tab to prevent cognitive overload.

### Tab 1: "OVERVIEW" (Always Visible)

**Five core widgets:**

| Widget | Data | Real-time Updates |
|--------|------|------------------|
| **Championship Position** | Standing, points gap to leader, trend arrow | Race finish |
| **Financial Status** | Budget remaining ($XXM/$215M), quarterly burn rate, runway forecast | Weekly |
| **Team Morale** | Overall %, key factors breakdown (results/sponsorship/injuries) | After races |
| **Upcoming Race** | Track name, date, weather forecast (90-95% accuracy), tire recommendations | 3 days before |
| **Quick Alerts** | Three-tier system (see 7.2.1 below) | Real-time |

### Tab 2: "SEASON CALENDAR & EVENTS"

**NEW: Comprehensive schedule and deadline management**

**Left Panel - Mini Calendar:**
```
┌─────────────────────────┐
│ SEASON 2027 (24 Races)  │
├─────────────────────────┤
│  ▶ Race 7  19 Apr (SAT) │ ← Current
│    Race 8  03 May       │
│    Race 9  17 May       │
│ ○ PRE-RACE WEEK         │
│    R&D Deadline: 15 May │
│    Contract Review: 18 May
│ ○ POST-RACE WEEK        │
│    Sponsor KPI Check    │
│    Financial Report (Q2)│
└─────────────────────────┘
```

**Right Panel - Upcoming Deadlines & Notes:**
- Contract expirations (color: red if <2 weeks)
- Sponsor KPI check dates (yellow warnings if trending below)
- R&D project completions (green checkmark when done)
- Cost Cap audit dates (quarterly)
- ADUO checks (3 total per season, marked by Power Unit supplier)

**Interactive Features:**
- Drag-and-drop player notes: "Review aero updates post-Monza"
- Hover-to-expand: Shows full event details
- Color-coded by type: Red (critical), Yellow (action needed), Blue (info), Green (completed)

### Tab 3: "R&D & RELIABILITY"

| Widget | Purpose |
|--------|---------|
| **Active Projects** | List 2-4 current projects: name, progress bar, completion date |
| **Resource Allocation** | Wind Tunnel hours used/available, CFD units budget, manufacturing capacity |
| **Engine Resources** | Power unit #1-#3 usage (X/8 races), DNF risk per unit, resource life remaining |
| **Component Breakthroughs** | Breakthrough innovation tracker: probability %, countdown timer, competitors copying |

### Tab 4: "TEAM & DRIVERS"

**Split into two sections:**

**Section A - Drivers:**
- Driver 1 & 2: Pace rating, morale %, contract status, injury/fitness, mental load indicator
- Performance trends: Last 5 races pace comparison, championship points contribution

**Section B - Personnel:**
- Three Directors: Performance/Engineering/Technical - morale, salary, retention risk
- Key Department Heads (7): Specialization, morale, workload indicator (🟢 Light / 🟡 Normal / 🔴 Overloaded)
- Pit Crew: Morale, next race pit stop confidence, training needs

### Tab 5: "FINANCE & SPONSORS"

| Widget | Purpose |
|--------|---------|
| **Cost Cap Health** | Visual gauge: $XXM / $215M, quarterly breakdown, quarterly freeze dates |
| **Runway Forecast** | Projected spend at current rate vs actual limit (see 7.2.6 below) |
| **Sponsor Status** | Portfolio table: Sponsor name, KPI progress (%), payment schedule |
| **Prize Money Projection** | Estimated Q4 championship payment based on current standings |

### Tab 6: "LIVE RACE MONITOR" (Active during races only)

**Full-screen race interface with multiple panels:**

**Main Panel (60% of screen):**
- Track map with live car positions, gap indicators
- Lap times: Current lap, best lap, delta to leader
- Pit lane status: Queue, pit stop times

**Right Sidebar - Race Strategy (25%):**
- Driver instructions quick menu: Push/Attack, Defend, Manage Fuel/Tires, Caution
- Current settings: MGU-K mode, engine power mode, tire strategy status
- Weather: Live track temp, rain probability, forecast updates

**Bottom Sidebar - Rival Monitor (15%):**
- Top 5 competitors with: tire compound (color icon), lap count on tires, pit stop history
- Projected pit window: "Hamilton → Box Lap 23-25 (soft → medium)"
- When clicked: Expand to show strategy prediction, fuel load estimate, engine mode

### Tab 7: "DECISIONS & HISTORY"

**NEW: Centralized decision log**

**Features:**
- Sortable by: Date (race #), Type (R&D/Contracts/Strategy/Sponsorship), Status (Success/In-Progress/Failed)
- Example entries:
  ```
  Race 12 | R&D Start       | Begin "Rear Wing Gen 3"     | In-Progress
  Race 11 | Contract Signed | Hire Sr. Aerodynamicist     | Success ✅
  Race 10 | Strategy        | Two-stop (Monza)            | Success ✅
  Race 9  | Sponsorship     | Negotiated +$5M bonus       | Success ✅
  Race 8  | R&D Complete    | "Brake Duct Update"         | Success ✅
  Race 7  | Engineer Leave  | Chief Chassis left team    | Failed ❌ (morale impact)
  ```
- Export to CSV: For analytical post-season review
- Search/filter: By team member, project type, outcome

---

## 7.3 ALERT SYSTEM (Three-Tier)

**NEW: Hierarchical notification system replacing flat "Quick Alerts"**

### Tier 1: 🔴 URGENT (Red Bell Icon)
**Must resolve before next race:**
- Contract expiring in <7 days (with renewal cost/options)
- Cash position below payroll (projected shortfall in X days)
- Key director/driver at risk of departure (morale <20%)
- Cost Cap approaching limit (>90% spent with races remaining)
- Power unit critical failure risk (DNF >4% remaining races)

Action required: Pop-up on login, cannot dismiss without action plan

### Tier 2: 🟡 ATTENTION (Yellow Bell Icon)
**Monitor and plan response:**
- Sponsor KPI trending below target (championship position slipping, podiums missed)
- Engineer morale declining (3+ races of low performance, approaching departure)
- Driver mental load high (15+ laps of sustained attack, error rate increasing)
- R&D project at risk of delay (manufacturing issues, complexity exceeded estimate)
- Pit crew morale low (2-3 recent slow stops, team cohesion issue)

Action optional: Can schedule meeting/intervention or monitor

### Tier 3: 🔵 INFO (Blue Bell Icon)
**Informational, no action required:**
- Training completed (skill improvement available)
- R&D milestone reached (Gen 1 complete, 50% of Gen 2 done)
- Sponsor bonus earned (KPI exceeded, +$X million)
- Driver form improving (last 3 races pace +0.2 sec avg)

No action required, dismissed by reading

**Implementation:**
- Bell icon in top-right corner shows count of unread alerts by tier
- Clicking opens sidebar with three sections stacked (Urgent top, scrollable)
- Mobile-style "swipe to dismiss" for Info tier only
- Weekly digest email with summary for player convenience

---

## 7.4 ENGINEER WORKLOAD & ALLOCATION

**NEW: Resource Allocation Matrix (replaces vague "team status")**

```
┌─────────────────────────────────────────────────────────────────────┐
│ DEPARTMENT RESOURCE ALLOCATION (Live during development)            │
├─────────────────────┬────────────┬────────────┬──────────┬──────────┤
│ ENGINEER            │ Aero Gen 3 │ Chassis    │ Driveability   │ FREE  │
├─────────────────────┼────────────┼────────────┼──────────┼──────────┤
│ Chief Aero          │ █████████░ │ ░░░░░░░░░░ │ ░░░░░░░░░░     │ 10%  │
│ Senior Aero #1      │ █████████░ │ ██░░░░░░░░ │ ░░░░░░░░░░     │ 5%   │
│ Senior Aero #2      │ █████████░ │ ░░░░░░░░░░ │ ░░░░░░░░░░     │ 10%  │
│ Aerodynamics Intern │ ██████░░░░ │ ░░░░░░░░░░ │ ░░░░░░░░░░     │ 40%  │
│ Chief Reliability   │ ░░░░░░░░░░ │ ██████░░░░ │ ██████░░░░     │ 20%  │
│ Chassis Lead        │ ░░░░░░░░░░ │ █████████░ │ ░░░░░░░░░░     │ 10%  │
│ Drivetrain Spec.    │ ░░░░░░░░░░ │ ░░░░░░░░░░ │ ██████████     │ 0%   │
├─────────────────────┼────────────┼────────────┼──────────┼──────────┤
│ TEAM TOTAL LOAD     │ 65%        │ 40%        │ 50%        │ -       │
└─────────────────────┴────────────┴────────────┴──────────┴──────────┘

🟢 GREEN (20-70% load): Optimal productivity, normal error rate (1-2%)
🟡 YELLOW (70-90% load): Slight fatigue, error rate up to 3-4%
🔴 RED (>100% load): OVERLOADED, error rate 6%+, may leave team

Click engineer row to:
- View full details (specialization, salary, development projects, skill modifiers)
- Transfer to different project (drag-and-drop, respects skill matching)
- Request training upgrade (costs money, takes time)
- Schedule 1-on-1 meeting (morale boost, but takes 1 game week)
```

**Features:**
- Real-time updates as projects progress
- Color-coded by load percentage (green/yellow/red)
- Hover to see error rate impact: "Current 3.2% error → accelerate project 20% = 4.1% error rate"
- Drag engineers between projects if skills allow (system warns if skill mismatch)
- Auto-distribution option: System recommends optimal allocation based on project priority

---

## 7.5 RACE INTERFACE: RIVAL MONITOR

**NEW: Side panel showing competitor strategies during live race**

```
┌──────────────────────────────────────────────────────────────────┐
│ RIVAL MONITOR (Expandable/collapsible)                           │
├──────────────────────────────────────────────────────────────────┤
│ POS │ DRIVER      │ TIRES  │ LAP STINT │ PIT HISTORY │ FORECAST │
├─────┼─────────────┼────────┼──────────┼─────────────┼──────────┤
│ 1   │ L.Hamilton  │🔴Soft │ 8/20 lap │ Lap 10      │ Box 23-26│
│     │ Mercedes   │ -0.3s  │ Temp:45°C│ Med+Fuel 55kg│ -Pit:2.3s
│     │            │ Fresh  │ Degr: +1.2s          │ Est Gap: -2s
├─────┼─────────────┼────────┼──────────┼─────────────┼──────────┤
│ 2   │ M.Verstap.  │🟡Hard │ 14/25 lap│ Lap 20      │ Box 32-35│
│     │ Red Bull   │ -0.1s  │ Temp:44°C│ Soft+Fuel 40kg│ -Pit:2.1s
│     │            │ Worn   │ Degr: +2.0s          │ Est Gap: -0.8s
├─────┼─────────────┼────────┼──────────┼─────────────┼──────────┤
│ 3   │ C.Leclerc   │🔵Med   │ 5/18 lap │ Pit Lap 10  │ Box 15-18│
│     │ Ferrari    │ +0.1s  │ Temp:43°C│ Hard+Fuel 60kg│ -Pit:2.2s
│     │            │ Peak   │ Degr: +0.5s          │ Est Gap: +1.2s
├─────┼─────────────┼────────┼──────────┼─────────────┼──────────┤
│ 4   │ YOU        │🟢Soft  │ 12/22 lap│ (Player)    │ Box 24-27│
│     │ Your Team  │ FOCUS  │ Temp:45°C│ Lap 12      │ -Pit:2.4s
│     │            │ Fresh  │ Degr: +0.8s          │ Est Gap: BASE
└──────────────────────────────────────────────────────────────────┘
```

**Information Breakdown:**
- **Tire Color Icon**: 🔴 Soft / 🟡 Medium / 🔵 Hard / 🟢 Intermediate / ⚫ Wet
- **Tire Age**: "Fresh" (1-3 laps) / "Peak" (optimal window) / "Worn" (cliff approaching)
- **Lap Stint**: Current lap vs expected tire lifespan (e.g., "8/20 lap")
- **Track Temp**: Affects degradation rate, tire warm-up
- **Degradation**: Time loss per lap as tires wear
- **Pit History**: When they pitted, what they pitted to
- **Forecast**: Predicted pit window (range), expected pit stop time, time gap after pit

**Interactive Features:**
- Click row to expand: Full strategy details, fuel calculation, MGU-K mode, engine power mode
- Hover on "Forecast" to see AI prediction confidence (70-95% accuracy)
- Color-coded pit stop times based on crew morale (🟢 green = fast, 🔴 red = slow)
- Auto-update every lap or on major events (pit stop, crash, safety car)

**Data Sources:**
- Pit stop data: Observed from replay
- Strategy prediction: Based on fuel load, tire degradation curve, team's typical strategy pattern
- MGU-K/Engine mode: Inferred from pace changes and fuel consumption rates

---

## 7.6 PIT CREW MANAGEMENT INTEGRATION

**NEW: Pit crew status linked to live race performance**

**In Crew Management Screen (before race):**
```
┌────────────────────────────────────────────┐
│ PIT CREW #1 READINESS                      │
├────────────────────────────────────────────┤
│ Morale: 85% (🟢 Excellent)                │
│ Pit Stop Avg: 2.15 sec (league avg 2.3)  │
│                                            │
│ Mechanics Assigned: 24/24 (Full)           │
│  ├─ Wheel Operators: 4/4 ✅                │
│  ├─ Fuel Attendants: 2/2 ✅                │
│  ├─ Jack Operator: 1/1 ✅                  │
│  └─ Tire Carriers: 4/4 ✅                  │
│                                            │
│ Expected Pit Stop Time (this race):        │
│  ├─ Soft to Medium change: 2.10 sec       │
│  ├─ Soft to Hard change: 2.15 sec         │
│  └─ Refuel 40kg: +0.2 sec (parallel)      │
│                                            │
│ 🎯 Training Focus: Fuel accuracy (+0.05s) │
│ 💰 Monthly cost: $150K                    │
└────────────────────────────────────────────┘
```

**During Live Race (in Rival Monitor):**
- Pit stop times show expected vs actual (if competitor pitted)
- Player's pit crew shown with: "Your crew: 🟢 2.15s (ready)" 
- If crew at <70% morale: "⚠️ Pit stop +0.3s penalty expected"
- Allows player to make tactical pit timing decisions based on crew condition

**Post-Race Feedback:**
- Actual pit stop time logged
- Crew morale adjusts +/-5% based on performance
- Individual mechanic ratings updated (faster/slower than baseline)

---

## 7.7 FINANCIAL INTERFACE: RUNWAY FORECAST

**NEW: Predictive spending dashboard**

```
┌──────────────────────────────────────────────────────┐
│ COST CAP HEALTH & RUNWAY FORECAST                   │
├──────────────────────────────────────────────────────┤
│ CURRENT SPENDING (Races 1-8)                        │
│ ├─ Spent YTD: $112M / $215M (52%)                  │
│ ├─ Monthly run rate: $14M/month                     │
│ └─ Remaining: $103M for 16 races                    │
│                                                      │
│ PROJECTION AT CURRENT RATE:                         │
│ ├─ If spending continues: $168M by Race 24         │
│ ├─ Status: ✅ ON TRACK (safe $47M buffer)          │
│ └─ Risk: 🟢 LOW                                    │
│                                                      │
│ SCENARIO MODELING (Click to explore):               │
│ ├─ IF accelerate R&D +20%:                         │
│ │  └─ Projected spend: $196M → ✅ Still safe      │
│ │                                                   │
│ ├─ IF hire 2 more senior engineers:                │
│ │  └─ Projected spend: $224M → 🔴 OVER CAP -$9M  │
│ │                                                   │
│ └─ IF slow-down manufacturing (save $500K/week):   │
│    └─ Projected spend: $156M → ✅ Safe (+$59M)   │
│                                                      │
│ QUARTERLY BREAKDOWN:                                │
│ Q1: $28M | Q2: $29M | Q3 (Est): $32M | Q4: $30M   │
│         (bars show actual/projected)                │
└──────────────────────────────────────────────────────┘
```

**Interactive Features:**
- Slider: "Adjust R&D budget -/+ 30%" → recalculates projection
- Slider: "Hiring pace" → projects salary impact
- Slider: "Manufacturing acceleration" → projects production cost
- All sliders show real-time impact on "Runway" (months of operation remaining)
- Red threshold line at $215M for visual reference
- Hovering over quarter shows breakdown: Personnel / R&D / Manufacturing / Operations

**Alerts:**
- 🟢 Safe (>$20M buffer): No action needed
- 🟡 Caution (5-20M buffer): Monitor Q3-Q4, consider cost cuts
- 🔴 Critical (<5M buffer or >$215M): Urgent action required, suggest specific cuts

---

## 7.8 RACE STRATEGY SETUP

**Pre-race pit stop strategy planner:**

```
┌──────────────────────────────────────────────────────────┐
│ RACE STRATEGY: Monza (24 Laps to pit stop window)       │
├──────────────────────────────────────────────────────────┤
│ DRIVER: Car #44 (Your Primary)                          │
│                                                          │
│ TIRE STRATEGY: TWO-STOP                                │
│ ├─ Stint 1: Soft (Laps 1-11)                           │
│ │  └─ Condition at 11 laps: Worn, -1.3 sec/lap        │
│ │                                                       │
│ ├─ Pit Stop 1: Lap 12 (expected)                       │
│ │  ├─ Soft → Medium change: 2.1 sec                    │
│ │  ├─ Fuel: 50 kg (lasts 13 laps)                      │
│ │  └─ Risk: Standard pit crew, weather OK             │
│ │                                                       │
│ ├─ Stint 2: Medium (Laps 13-26) [3+ extra laps]      │
│ │  └─ Condition at 26 laps: Cliff phase, -2.0 sec/lap│
│ │                                                       │
│ └─ Pit Stop 2: Lap 27 (expected)                       │
│    ├─ Medium → Hard change: 2.1 sec                    │
│    ├─ Fuel: 30 kg (last 4 laps to finish)            │
│    └─ Risk: High fuel flow approaching limit          │
│                                                        │
│ ENGINE POWER MODE:                                     │
│ ├─ Laps 1-5: Standard (preserve power units)          │
│ ├─ Laps 6-22: Standard (normal pace)                  │
│ └─ Laps 23-24: Qualifying mode (2/8 laps) if needed  │
│                                                        │
│ MGU-K STRATEGY:                                        │
│ ├─ Recovery: Medium mode (0.8 MJ per lap)             │
│ └─ Deployment: Medium (continuous +0.2 sec/lap)      │
│                                                        │
│ RISK ASSESSMENT:                                       │
│ ├─ Weather: ✅ Dry (high confidence)                  │
│ ├─ Safety Car: Yellow flag risk 15% (normal)         │
│ └─ Competitor Strategy: Undercut possible Lap 10-12  │
│                                                        │
│ [VIEW COMPETITOR STRATEGIES] [SAVE TEMPLATE]         │
└──────────────────────────────────────────────────────────┘
```

**Features:**
- Visual tire degradation curve showing when cliff phase begins
- Fuel calculation: Shows fuel remaining after each pit stop
- Engine power budget tracker (Qualifying mode limited to 8-10 laps/race)
- MGU-K mode selector with expected lap time gain/loss
- Risk flags for weather changes, safety car probability
- Compare to competitor strategies (undercut/overcut warnings)
- Save template: Reuse similar strategies for comparable tracks

---

## 7.9 ONBOARDING & TUTORIALS

**NEW: Visual guided tour system (replaces text-heavy tutorial)**

**Mode: "Interactive First-Time Setup"**
1. Game loads → Guided tour offered (skip option available)
2. Hover highlighting: Pink glow around suggested element
3. Floating tooltip: "Click the R&D tab to set up your development projects" (with pointer arrow)
4. After action: Confirmation checkmark, "✅ Great! Now let's..."
5. Auto-advance after 10 player actions (or manual "Next" click)

**Tour Sequence (5-7 minutes):**
1. Overview dashboard: Where to find key info
2. R&D setup: Choose first project (e.g., "Aero Gen 1")
3. Team management: Assign engineers to projects
4. Race strategy: Set up tire/fuel plan for Race 1
5. Finance: View Cost Cap status and sponsorship
6. Race monitor: Simulate live race with paused time (practice)
7. Results: View post-race report

**Advanced Features:**
- Replay any tour section from "Help" menu
- Context-sensitive help (F1 key) on any screen shows relevant tutorial segment
- "Learning mode" toggle: Shows helpful hints on unusual actions ("Assigning 5+ engineers to one project will cause overload →")

---

## 7.10 KEYBOARD SHORTCUTS & HOTKEYS

**Power user optimizations:**

```
┌──────────────────────────────────┐
│ PRIMARY HOTKEYS (Available)      │
├──────────────────────────────────┤
│ T    → Team Management           │
│ R    → R&D Projects              │
│ B    → Budget/Finance            │
│ P    → Race Strategy / Pit setup  │
│ H    → Decision History           │
│ N    → Notifications (Alerts)    │
│ ?    → Help / Hotkey menu        │
│ Esc  → Close popup/sidebar       │
│ Ctrl+S → Save game               │
│ Ctrl+Z → Undo last decision      │
│ Ctrl+Y → Redo                    │
├──────────────────────────────────┤
│ IN-RACE HOTKEYS (During race)    │
├──────────────────────────────────┤
│ Space   → Pause race             │
│ A       → Attack instruction     │
│ D       → Defend instruction     │
│ F       → Manage Fuel/Tires      │
│ W       → Caution (weather)      │
│ X       → Box (pit stop)         │
│ M       → MGU-K mode selector    │
│ E       → Engine power mode      │
│ R       → Rival Monitor toggle   │
│ V       → View strategy      │
│ Spacebar → Pause/resume          │
└──────────────────────────────────┘
```

**Customization:**
- Players can reassign hotkeys in Settings
- Profiles for different playstyles (aggressive/conservative)
- Macro creation: "Aggressive attack" = Ctrl+1 → A+Engine Qualy+MGU-K high

---

## 7.11 PERFORMANCE & ACCESSIBILITY

**Technical Optimization:**
- Animations: 150-200ms transitions (toggle "instant mode" for 0ms for competitive play)
- Rendering: Optimized for 1440p@60fps, scales to 4K
- Save/load: <2 second state transition
- Data updates: Real-time for race data, batched for other metrics (weekly/race frequency)

**Accessibility:**
- WCAG AA compliant (min 4.5:1 contrast ratio)
- Color-blind modes: Deuteranopia, Protanopia (red-green) + Monochromacy
- Text scaling: 100%-200% (all UI responsive)
- Screen reader support: Semantic HTML, ARIA labels
- Dyslexia-friendly font option (OpenDyslexic available)

---

## 7.12 ERROR HANDLING & CONFIRMATIONS

**Smart confirmation system:**

**Automatic (no confirmation needed):**
- Assigning engineer to project (reversible via undo)
- Changing team meeting schedule
- Adjusting R&D budget sliders

**One-click confirmation (yellow warning):**
- Offering contract extension (locks salary, can renegotiate later)
- Starting major R&D project (significant cost, long timeline)
- Changing pit strategy before race day

**Two-step confirmation (red critical):**
- Firing engineer/director (permanent, morale hit)
- Accepting sponsor that requires high KPI (breach = financial penalty)
- Selling power unit to competitor (confidentiality risk)

**Undo System:**
- Last 20 actions can be undone
- Undo stack shown in Decision History
- Undoing a race result resets that race's progression (warns player)

---

## 7.13 VISUAL DESIGN SPECIFICATIONS

**Color Palette:**
- Dark background: #0A0E27 (night sky)
- Accent primary: #DC143C (FIA red)
- Success: #00D084 (bright green)
- Warning: #FFB800 (amber)
- Critical: #FF1744 (deep red)
- Neutral: #808080 (gray)
- Team colors: Primary team color + secondary (respect for visual identity)

**Typography:**
- Headers: Inter Bold, 18-24pt
- Body: Inter Regular, 14pt
- Data: IBM Plex Mono (numbers/stats), 12pt
- Line height: 1.5 (readability)

**Icons:**
- Tire compounds: Color-coded circles (red=soft, yellow=medium, blue=hard)
- Status indicators: 🟢 Good / 🟡 Caution / 🔴 Critical (emoji for clarity)
- Actions: "+" (add), "=" (adjust), "×" (remove), "→" (navigate)

**Micro-interactions:**
- Button hover: Background darkens 10%, slight scale 1.05x
- Click feedback: 50ms press animation
- Loading: Pulsing spinner with "Loading race data..." text
- Toast notifications: Slide in from top, auto-dismiss after 3 seconds
- Tooltips: Appear on 300ms hover, fade out on mouse leave

---

## 7.14 MOBILE/RESPONSIVE DESIGN NOTES

**Note**: Primary design is desktop-focused. Tablet responsiveness achievable but mobile app explicitly out of scope per requirements.

**Desktop breakpoints:**
- 1920px+ (4K): Full-width panels, additional context sidebars
- 1440px (QHD): Standard view, optimized layout
- 1024px (iPad landscape): Collapsible sidebars, stacked widgets

---

## 7.15 FUTURE INTERFACE EXTENSIBILITY

**Plugin/mod readiness:**
- CSS variable system for complete theme customization
- Plugin API for custom widgets (e.g., "Driver market analysis", "Component cost optimizer")
- Import/export game data in JSON format for custom tools
- Webhook support for third-party integrations (spreadsheet sync, Discord notifications)

---

**END OF PART 7: UI/UX DESIGN**

---

**Summary of F1 2025-2026 Compliance:**
This UI design comprehensively integrates all game mechanics from Parts 2-6:
- Cost Cap ($215M) with Runway Forecast
- Prize distribution ($1.6B annual)
- 6-tire compound system with visual wear tracking
- MGU-K energy management with driver controls
- Dirty air effects (shown in Rival Monitor)
- Engine power modes (Qualifying/Standard/Economy) with reliability linkage
- Pit crew morale impact on pit stop times
- Mental load system for drivers (concentration tracking)
- Undercut/Overcut strategy visualization
- ADUO checks and Power Unit resource management
- Three-tier alert system for critical vs information alerts
- Decision history logging for post-race analysis

**Key Improvements Over Previous Version:**
1. ✅ Added "Season Calendar & Events" tab (deadline management)
2. ✅ Split alerts into 3 tiers (Urgent/Attention/Info)
3. ✅ Engineer Resource Allocation Matrix (visual workload tracking)
4. ✅ Rival Monitor side panel (competitor strategy tracking)
5. ✅ Pit crew status integration (live race performance link)
6. ✅ Financial Runway Forecast (predictive spending model)
7. ✅ Visual guided tours (onboarding without text walls)
8. ✅ Decision history log (central decision repository)
9. Removed mobile app discussion (desktop primary)
