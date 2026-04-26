# PART 7: UI/UX DESIGN

**Comprehensive Interface Design for Autosport Manager 2027**
**Aligned with F1 2025-2026 Regulations and Advanced Game Mechanics**

---

## 7.1 DESIGN PHILOSOPHY

**Core UX Principles:**

1. **Information Density**: Deep complexity accessible through progressive disclosure
2. **Real-Time Feedback**: Immediate visual confirmation of all actions
3. **Three-Tier Alerts**: Critical / Attention-needed / Info, differentiated by **shape, text label, AND color** (color is never the sole signal — see §7.11 accessibility)
4. **Context-Sensitive**: UI adapts to season phase (pre-season/in-season/post-race)
5. **Dark Theme Modern F1 Aesthetic**: Black/dark gray with FIA red and team color accents
6. **Performance Within 30 FPS Budget**: All transitions and updates must fit the 15-18 ms UI budget per frame defined in TDD §1.4. Transitions are 100-150 ms (was 150-200 ms). "Instant mode" is reframed as an accessibility option (motion sensitivity) rather than a power-user toggle.
7. **Accessibility-First**: WCAG 2.1 AA baseline from day 1. Non-color differentiators, keyboard-only navigability, screen-reader semantics, configurable motion. See §7.11.
8. **Two-Car Team Management**: Player commands two cars (Primary + Teammate). Race-time decisions and panels accommodate concurrent calls for both cars.

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

This is the screen the player spends ~80% of game time on. Information architecture is organized by **attention demand**, not by spatial column.

**Visual contract**: 2D track map with cars rendered as **interpolated dots** on a per-circuit polyline outline (Visual Tier Level 2 per ADR 2026-04-25, see TDD §1.14). No 3D in v1. Dot color encodes tire compound; dot shape encodes player's two cars vs rivals (filled = your team, hollow = rival).

**Always-visible (peripheral attention — never requires a click)**:
- 2D track map with all 24 dots (your two cars distinctly marked)
- Lap counter: `current / total`
- Both your cars: position, gap to ahead/behind (two compact rows)
- Both your cars: tire compound + stint lap count (the clock the player watches)
- Weather indicator: single icon + track temp
- **Speed control** (top-right): Pause / 1x / 2x / 4x / 8x / Instant — see §7.16 and TDD §1.15

**On-demand (player drills in to answer a forming question)**:
- Full 24-car standings table — collapsed to top-8 by default; expand for full grid; sort by gap (more useful mid-race than position number)
- Rival strategy detail: tire age, pit-window prediction, fuel load estimate (this is the §7.5 Rival Monitor — repositioned as a panel, not a permanent sidebar, to reduce visual noise during the 90% of time when no rival is in a critical window)
- Lap-time graph for each of your cars: last 10 laps showing degradation curve
- Pit strategy advisor: pre-planned strategy with current deviation ("you planned lap 22, now lap 20 recommended"). Has an event-driven trigger — see below.
- MGU-K / engine power mode / driver instruction quick menu (Push / Defend / Save / Caution)

**Event-driven (interrupts the player — something changed)**:
- **Pit window alert** (most important interrupt): non-modal banner anchored to bottom of track map per car: "Box this lap — undercut window open". Sim does NOT pause. See §7.16.
- Incident notification: crash, safety car, yellow flag — instant strategy impact, must interrupt.
- Rival pit completed: toast, "Verstappen pitted — now on Mediums"
- Weather change warning: "Rain likely in 4 laps" — time-sensitive but not instant-action.
- Driver radio message: crew-chief comment (cliff imminent, rival closing). Text + audio cue.

**Two-car principle**: every "your car" element above appears twice (Primary + Teammate). Shared elements (track map, weather, speed control) are single. Pit-window alerts can fire independently for each car — see §7.16 for the dual-banner case.

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

**On-demand panel showing competitor strategy estimates during live race.**

Repositioned from a permanent sidebar to an on-demand panel (per §7.2 Tab 6 IA). Reading a 24-row table while watching for a pit-window moment creates attentional conflict; rival detail belongs one click away from the always-visible track map, not in continuous peripheral vision.

**Important — display semantics**: every value below is a **model estimate**, not a sensor reading. The simulation is formula-based (TDD §1.2, §1.7), so showing "Temp: 45°C" implies precision the model does not produce. All rival numbers carry an explicit confidence qualifier (e.g., "Pit window: 23-26 (75% conf.)") to align display with model fidelity and preserve player trust when values shift.

```
┌──────────────────────────────────────────────────────────────────┐
│ RIVAL MONITOR (on-demand panel)                                  │
├──────────────────────────────────────────────────────────────────┤
│ POS │ DRIVER      │ TIRES   │ LAP STINT │ PIT HISTORY │ FORECAST│
├─────┼─────────────┼─────────┼-----------┼-------------┼---------┤
│ 1   │ L.Hamilton  │ Soft ●  │ 8 / ~20   │ Lap 10 → M  │ Box 23-26
│     │ Mercedes    │ Fresh   │ Pace est. │             │ 75% conf.
│     │             │         │ -0.3 s    │             │
├─────┼─────────────┼─────────┼-----------┼-------------┼---------┤
│ 2   │ M.Verstap.  │ Hard ▼  │ 14 / ~25  │ Lap 20 → S  │ Box 32-35
│     │ Red Bull    │ Worn    │ Pace est. │             │ 70% conf.
│     │             │         │ -0.1 s    │             │
├─────┼─────────────┼─────────┼-----------┼-------------┼---------┤
│ 3   │ C.Leclerc   │ Med ◆   │ 5 / ~18   │ Pit Lap 10  │ Box 15-18
│     │ Ferrari     │ Peak    │ Pace est. │ → H         │ 80% conf.
│     │             │         │ +0.1 s    │             │
├─────┼─────────────┼─────────┼-----------┼-------------┼---------┤
│ 4   │ YOU (Pri.)  │ Soft ●  │ 12 / ~22  │ (Player)    │ Plan 24-27
│     │ Your Team   │ Fresh   │ Confirmed │ Lap 12 (S)  │
└──────────────────────────────────────────────────────────────────┘
```

**Tire compound differentiation** (color is supplementary, not sole signal — see §7.11 accessibility):
- Soft: filled circle ● (red) + label "S"
- Medium: diamond ◆ (yellow) + label "M"
- Hard: triangle-down ▼ (blue) + label "H"
- Intermediate: square ■ (green) + label "I"
- Wet: filled triangle-up ▲ (dark blue) + label "W"

**Tire Age semantic** (model bucket, not measured): Fresh (early stint) / Peak (optimal) / Worn (cliff approaching).

**Pace estimate**: model output of expected lap delta vs your current pace. Sign convention: negative = rival faster than you.

**Forecast**: predicted pit window range with model confidence percentage (drawn from 06_RACE_SIMULATION_MECHANICS.md degradation projection).

**Interactive Features:**
- Click row to expand: estimated fuel load, MGU-K usage pattern, engine power mode (all marked "estimated")
- Confidence bar visible under every forecast — players learn to discount low-confidence rows
- Color-coded pit stop time prediction tied to rival crew morale model
- Auto-update on snapshot tick when row state changes (no per-frame redraw)

**UIToolkit performance note** (mitigates `risks.high_priority` "UI performance degrades with 24-car standings"): the rival panel must use **diff-only dirty-checking**. The presenter compares incoming snapshot to last-rendered snapshot and marks only changed cells dirty. During safety-car periods most rows are static — zero dirty cells means zero render cost. See §7.11.

**Data Sources** (all model estimates):
- Pit history: observed from race event log (deterministic source, high confidence)
- Tire age: known from event log
- Pit-window forecast: derived from tire degradation model + fuel load estimate + team's typical strategy pattern
- Pace estimate: derived from recent lap times + tire age model
- MGU-K / engine mode: inferred from pace deltas (lower confidence)

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
- Competitor pit stop times: model-estimated time predicted before pit + observed time after pit (both shown so player can see when their model was wrong)
- Player's pit crew status: "Your crew: 2.15 s (ready)" with status icon (shape + label, not color alone — see §7.11)
- If crew at <70% morale: "Pit stop +0.3 s penalty expected" warning with attention-tier visual (icon + text label)
- Allows player to make tactical pit timing decisions based on crew condition
- All time estimates marked as "estimated"; observed times marked separately so the difference is visible

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
│ Space   → Pause / Resume         │
│ 1       → Speed 1× (normal)      │
│ 2       → Speed 2×               │
│ 3       → Speed 4×               │
│ 4       → Speed 8×               │
│ 0       → Instant (skip ahead)   │
│ Tab     → Switch active car      │
│         (Primary ↔ Teammate)     │
│ A       → Attack instruction     │
│ D       → Defend instruction     │
│ F       → Manage Fuel/Tires      │
│ W       → Caution (weather)      │
│ X       → Confirm pit (active car)│
│ Shift+X → Confirm pit (teammate) │
│ M       → MGU-K mode selector    │
│ E       → Engine power mode      │
│ R       → Rival Monitor toggle   │
│ V       → View strategy          │
└──────────────────────────────────┘

**Two-car shortcuts**: All single-car commands act on the currently active car (default = Primary). Use Tab to cycle. Shift-modifier always targets the other car. This means a player can issue different instructions to both cars without leaving the keyboard.

**Accessibility — full keyboard navigability**: every interactive element in the live race monitor (banner buttons, pit-call panel, rival-row expansion) is reachable by Tab / Shift+Tab and activatable by Enter or Space. No mouse-only interactions in race mode. See §7.11.
```

**Customization:**
- Players can reassign hotkeys in Settings
- Profiles for different playstyles (aggressive/conservative)
- Macro creation: "Aggressive attack" = Ctrl+1 → A+Engine Qualy+MGU-K high

---

## 7.11 PERFORMANCE & ACCESSIBILITY

### Performance (aligned with TDD §1.4 frame budget)

- **Target**: 1440p @ 30 FPS (was 60 FPS — corrected per ADR 2026-04-25). Scales to 4K. No 60 FPS option in v1; this is a management sim, not a real-time action game.
- **Frame budget**: 33.3 ms total per frame; UI gets 15-18 ms (per TDD §1.4). All transitions and updates must fit inside that.
- **Transitions**: 100-150 ms (was 150-200 ms — must be validated against the 15-18 ms UI budget; a 200 ms transition that blocks a panel update can lag displayed race state behind sim state).
- **Save/load**: <2 s state transition.
- **Data updates**: snapshot-driven for race data (one update per 50 ms tick, diff-only); event-driven for graphs (only redraw on new data point); batched for non-race metrics (weekly/race frequency).

**UIToolkit performance mitigations** (addresses `risks.high_priority` "UI performance degrades with 24-car standings + live telemetry graphs"):

1. **Standings table — diff-only dirty-checking.** Presenter compares incoming snapshot to last-rendered; only marks changed cells dirty. During safety-car periods most positions are frozen → zero dirty cells, zero render cost. Convention, not architecture.
2. **Lap-time graphs — event-driven redraw, not per-frame.** Use UIToolkit's `MeshGenerationContext` only when a new data point arrives (~once per sim tick = 50 ms, not per 33 ms frame). Rival graphs only redraw when the on-demand panel is expanded.
3. **Polyline arc-length lookup — binary search, not linear scan.** Pre-cache the arc-length table as a sorted array; use `System.Array.BinarySearch` for the segment lookup. Negligible per car, but compounds with 24 cars per frame.

### Accessibility (first-class from day 1, per producer decision 2026-04-25)

Baseline: **WCAG 2.1 AA** across all screens. Accessibility features ship in v1, not as a post-launch patch.

**Visual**:
- **Color is never the sole signal.** Every alert, status, and tire compound carries a non-color differentiator: shape (circle / diamond / triangle / square), text label ("S/M/H/I/W"), or icon glyph. The three-tier alert system in §7.3 uses shape + text label in addition to color.
- Contrast ratio ≥ 4.5:1 for all text and meaningful UI; ≥ 3:1 for large text and graphical objects.
- Color-blind modes: Deuteranopia, Protanopia, Tritanopia, Monochromacy. All test against the tire compound differentiation in §7.5.
- Text scaling 100-200% with full reflow (no clipping, no horizontal scroll).
- Dyslexia-friendly font option (OpenDyslexic); optional sentence-spacing increase.
- "Reduced motion" toggle that disables non-essential transitions and replaces them with instant state changes — repurposes the previous "instant mode" toggle as an accessibility control rather than a competitive-play tweak.

**Auditory** (every event-driven interrupt has an audio cue):
- Pit-window alert: distinct two-tone radio chirp (Primary: high pitch; Teammate: low pitch — separable by ear)
- Incident notification: longer attention-grabbing tone
- Rival pit completed: subtle one-tone notification
- All audio cues are independent of music volume and mappable to specific frequencies for hearing-impaired players who use vibration / visual alternatives

**Motor / input**:
- Full keyboard navigability — every interactive element reachable via Tab / Shift+Tab. Enter / Space activates. No mouse-only interactions in race mode.
- Configurable hotkeys (already in §7.10) — including alternative bindings for one-handed play.
- No time-pressure decisions: pit-call banner has no countdown timer; default-action fallback handles inaction (see §7.16).

**Cognitive**:
- Configurable simulation speed (see §7.16) — players can drop to 1× or pause to absorb information at their own pace.
- Information density toggle: "compact" (default) vs "comfortable" (more padding, larger touch targets).
- Plain-language toggle for technical telemetry (e.g., "tire grip falling" instead of "tire degradation +0.8 s/lap").

**Screen reader**:
- Semantic UIToolkit element types (button, list, table) with descriptive ARIA-equivalent labels.
- Live region announcements for event-driven interrupts (pit window, incident, weather change).
- Race state queryable via screen-reader summary command (hotkey ?): "Lap 12 of 60. Primary P3, +1.2 s ahead. Teammate P7. Soft tires, 8 laps in."

**Validation**:
- WCAG 2.1 AA conformance audit before each milestone.
- Keyboard-only smoke test as part of CI for the live race monitor.
- Color-blind playthroughs in QA test plan (see qa-lead).

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

## 7.16 IN-RACE DECISION FLOW (Pit Call)

**The most critical interaction in the game.** The producer's core experience is "watching AND deciding." A modal that pauses the sim breaks immersion and undermines the race-engineer fantasy. A pop-up that interrupts without pausing creates panic. The right model is neither — a non-modal banner with diegetic time pressure (race state context, not countdown timers) and an automatic default-action fallback.

### 7.16.1 Single-Car Pit Call Sequence

1. **Banner appears** (non-modal) anchored to the bottom of the track map, identifying the car: "Box recommended — Lap 20 (Primary). Confirm? [X] [Not yet]". Sim continues running. Player can read the banner while watching the map.
2. **Soft escalation** (after 2 ignored laps): banner increases visual weight (brighter outline, persistent shape change — not just color), and the crew chief fires a second radio message with audio cue. Sim still runs.
3. **Player presses X (or hotkey from §7.10)**: a compact in-place panel slides up from the banner — compound selector (3 options with shape+label per §7.5), fuel delta slider, confirm button. Total interaction time 3-5 seconds. Race continues.
4. **Default-action fallback** (if player ignores past the planned pit lap): the AI engineer executes the pre-race plan (§7.8) automatically. Player sees a passive notification: "Pitted on Lap 22 — plan executed." This preserves the "watch AND decide" fantasy for distracted players without catastrophic punishment for inaction.

**No countdown timer on the banner.** Pressure is **diegetic** — the player can see rivals pitting on the map, can see their own tire-stint counter advancing, can hear the crew chief escalate. Mechanical countdowns add anxiety without strategic depth.

### 7.16.2 Two-Car Variant (Concurrent Pit Calls)

Both cars can have open pit-window banners simultaneously. Three distinct cases:

**Case A — sequential**: pit-window for one car opens 3+ laps before the other. Standard single-car flow runs twice, no special UI. Most common case.

**Case B — overlapping windows (within 2 laps)**: two banners stack vertically at the bottom of the track map, top = Primary, bottom = Teammate. Each independently confirmable (X for Primary, Shift+X for Teammate per §7.10). Audio cues differentiated by pitch (high/low).

**Case C — same-lap conflict (both want to box this lap)**: pit lane can only accept one car per lap (real F1 constraint). UI surfaces this as a prioritization prompt: "Only one stop possible this lap. Box Primary or Teammate?" with two big buttons. The non-pitted car's banner persists into next lap, AI engineer rolls a delay decision based on tire condition and gap risk. This is the only situation where a decision becomes mandatory; the prompt remains until resolved but the sim continues running around it.

### 7.16.3 Strategy Override Mid-Race

Outside pit windows, the player can open the pit strategy panel from §7.8 at any time and adjust the plan (next compound, target lap, fuel delta). Changes apply to the next pit window — the banner sequence above then reflects the new plan. Sim never pauses for strategy editing.

### 7.16.4 Acceptance Criteria

This section is fully implemented when:
- A first-time player can complete a 60-lap race without ever having to pause to make a pit decision (default-action fallback covers them).
- An experienced player can manage two cars on different strategies without leaving the keyboard (Tab + X / Shift+X / hotkeys per §7.10).
- The pit-window banner does not occlude the track map's center 60% (the always-visible region).
- Audio cues are differentiable by hearing-impaired players via vibration / visual replacement (§7.11 accessibility).

---

## 7.17 RACE SPEED CONTROL

**Producer requirement (2026-04-25)**: race wall-clock duration is player-controlled, not fixed at ~2 minutes. See TDD §1.15 for the simulation contract; this section covers the UI.

### 7.17.1 Control Surface

Top-right of the live race monitor (always-visible per §7.2 Tab 6). Six states laid out as a compact pill with the active state highlighted by **shape and label**, not color alone:

```
┌─────────────────────────────────────────────────┐
│ [⏸ Pause] [1×] [2×] [4×] [8×] [⏭ Instant]      │
└─────────────────────────────────────────────────┘
```

Hotkeys: Space (Pause/Resume), 1/2/3/4 (1×/2×/4×/8×), 0 (Instant). See §7.10.

### 7.17.2 Per-State Behaviour

| State | Sim advance | UI interaction | Use case |
|---|---|---|---|
| **Pause** | Frozen | Fully interactive — browse standings, plan strategy, change speed, issue instructions | Look around, plan, take a breath |
| **1×** | Calibrated baseline (~15 s/lap, TBD per TDD §1.15 open question) | Fully interactive | Default "watch + decide" pace |
| **2×** | 2× baseline | Fully interactive | Steady stints, no urgent decisions |
| **4×** | 4× baseline | Fully interactive | Long boring stints, gap to next event |
| **8×** | 8× baseline | Fully interactive but visual interpolation has fewer samples per in-game lap | Skip through neutral race phases |
| **Instant** | Tight loop until next interactive prompt or race end | Read-only progress indicator with cancel button | Skip to result or to next decision moment |

### 7.17.3 Auto-Pause and Auto-Slow Triggers

To support "watch + decide" without forcing the player to manually slow down for every event, the game **proposes** speed changes on key events. Player can disable each trigger individually in settings (cognitive accessibility, §7.11).

| Event | Default action | Reason |
|---|---|---|
| Pit-window banner opens (yours) | Slow to 1× if currently >2× | Decision moment requires attention |
| Incident on track (your cars or top-3 rivals) | Pause if currently >1× | Strategic impact, player needs to assess |
| Weather change confirmed | Slow to 1× | Strategy may need adjustment |
| Safety car deployed | Pause | Always strategy-changing |
| Race end approaching (final 5 laps) | Slow to 1× if currently >2× | Result drama |

Triggers fire as a **soft suggestion banner** ("Slowing to 1× — incident detected"), not an instant override the player can't cancel. The player can immediately tap a higher speed to dismiss.

### 7.17.4 Instant Mode Safety

Instant mode skips ahead, which can race past decision moments the player cared about. Two safeguards:
- Instant mode **always halts** at the same triggers that would auto-pause in other modes (table above).
- Player can set a "stop at lap N" target before entering instant — useful for quickly advancing to a known decision point (e.g., "I want to be at lap 25 for my pit window").

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
