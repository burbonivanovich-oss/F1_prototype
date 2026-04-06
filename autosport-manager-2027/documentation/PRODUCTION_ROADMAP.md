# PRODUCTION ROADMAP
## Autosport Manager 2027 - Pre-Production Phase

**Duration**: 10 weeks (Weeks 1-10)  
**Target Completion**: 2026-06-15  
**Deliverables at End**: Playable prototype, TDD, risks document, GDD finalization  

---

## EXECUTIVE SUMMARY

### Pre-Production Goals
1. **Validate core gameplay** → Build 1-race prototype (playable)
2. **Finalize architecture** → TDD complete and reviewed
3. **De-risk major technical challenges** → Prototype 24-car physics
4. **Prepare for full production** → Roadmap for 24-36 weeks development

### Team Structure (Estimated)
- 1 Producer (you)
- 1 Technical Lead (architect physics/AI)
- 1-2 Gameplay Programmers (mechanics implementation)
- 1 AI Specialist (driver behavior trees)
- 1 UI/UX Designer
- 1 QA Engineer (testing & validation)
- 1 Artist/Animator (optional, defer to production)

---

## WEEK-BY-WEEK BREAKDOWN

### **WEEK 1: Foundation & Planning** 
**Theme**: Setup, tools, first prototype concept

**Deliverables**:
- [ ] Project initialized in Unity 2022.3 LTS
- [ ] Development environment set up (Git, CI/CD, build scripts)
- [ ] Initial project structure (folders, namespaces)
- [ ] TDD Part 1 & 2 reviewed and signed off
- [ ] Team kickoff meeting (roles, responsibilities)

**Tasks**:
- **Technical Lead**: 
  - Set up Unity project with recommended settings (physics, input, graphics)
  - Configure Git repo structure
  - Create physics engine skeleton
  
- **UI Designer**:
  - Sketch UI layouts (race monitor, pit strategy screen)
  - Define color scheme, typography
  - Start UIToolkit learning/setup

- **Gameplay Programmers**:
  - Set up data structures (CarState, RaceState, TireState)
  - Implement basic lap timing system
  - Create mock race data (hard-coded 3 cars for testing)

- **QA Engineer**:
  - Set up test plan document
  - Identify success criteria for prototype

**Risk Check**: No major risk events expected

**Milestone**: "Alpha Zero" — Unity project builds, 3 cars visible on screen

---

### **WEEK 2-3: Physics Engine Prototype**
**Theme**: Get realistic physics working (8 cars first)

**Deliverables**:
- [ ] Longitudinal dynamics (acceleration, braking, max speed)
- [ ] Lateral dynamics (cornering, understeer/oversteer)
- [ ] Collision detection working (quadtree spatial partitioning)
- [ ] Basic tire model (grip, temperature)
- [ ] 8-car race simulation running

**Tasks**:
- **Technical Lead** (40 hours/week):
  - Implement semi-implicit Euler integration
  - Physics thread setup (fixed timestep: 20 ms)
  - Quadtree implementation for broad-phase collision
  - Vehicle setup data (power, drag, weight)
  
- **Gameplay Programmers** (30 hours/week):
  - Implement CarController (throttle, steering input)
  - Tire temperature calculations
  - Simple tire degradation model (linear, not 3-phase yet)
  - Damage model (basic collision → speed loss)

- **UI Designer** (20 hours/week):
  - Implement standings panel (position, gap, tire info)
  - Basic track map (bird's eye view)
  - Speed controls (1x, 2x, 5x) UI

- **QA Engineer** (15 hours/week):
  - Test physics stability (no cars sinking through track)
  - Verify collision detection working
  - Performance profiling (target: 60+ FPS with 8 cars)

**Risk Check**: 
- ⚠️ RISK #1 — Physics performance
  - Prototype with 8 cars shows if scaling to 24 is feasible
  - If FPS < 40 with 8 cars → need optimization before scaling
  - Success: 60+ FPS with 8 cars on reference hardware

**Milestone**: "8-Car Race" — Full race simulation with 8 AI cars (simple AI: just drive around)

---

### **WEEK 4: Tire System & Weather**
**Theme**: Realistic tire behavior and weather effects

**Deliverables**:
- [ ] Tire degradation model (3-phase: plateau, linear, cliff)
- [ ] Tire temperature model (warm-up, optimal window, overheat)
- [ ] Track temperature effects on grip
- [ ] Weather system (dry → rain → dry transition)
- [ ] Dynamic pit stop mechanics

**Tasks**:
- **Gameplay Programmers** (40 hours/week):
  - Implement 3-phase degradation (formula from GDD)
  - Tire temperature calculations (heat generation, ambient cooling)
  - Track temperature integration
  - Pit stop sequence (enter pit lane, change tires, warm-up penalty)

- **UI Designer** (20 hours/week):
  - Implement tire telemetry panel (temperature, degradation %)
  - Pit stop UI (when to pit, which compound, how much fuel)
  - Weather indicator (precipitation %, track temp)

- **Technical Lead** (15 hours/week):
  - Integrate tire system into physics (grip calculations)
  - Weather state evolution algorithm

**Risk Check**:
- Verify pit stop timing is strategic (not instant laps)
- AI should make pit decisions based on tire state

**Milestone**: "Tire Strategy" — Player can strategically pit, weather affects race

---

### **WEEK 5-6: AI Driver System**
**Theme**: 24 intelligent drivers making tactical decisions

**Deliverables**:
- [ ] Scale physics to 24 cars (performance validated)
- [ ] AI decision tree (pit strategy, overtaking, morale)
- [ ] AI thread implementation (100 ms decision cycles)
- [ ] Morale system affecting driver behavior
- [ ] Test full 24-car race simulation

**Tasks**:
- **AI Specialist** (40 hours/week):
  - Implement pit strategy evaluation (fuel, degradation, gap to competitors)
  - Overtaking decision logic (DRS, skill check, risk assessment)
  - Morale system (performance bonuses, penalties, team orders)
  - AI thread architecture (separate from physics thread)

- **Gameplay Programmers** (30 hours/week):
  - Scale physics engine to 24 cars
  - Performance optimization (SIMD, spatial partitioning tuning)
  - Implement command queues (thread communication)

- **Technical Lead** (20 hours/week):
  - Monitor performance: profiling with 24 cars
  - Bottleneck analysis & optimization
  - Thread safety validation

- **QA Engineer** (20 hours/week):
  - Performance profiling (FPS with 24 cars)
  - AI behavior validation (pit timing reasonable, overtakes realistic)
  - Stress test (10+ races without crashes/hangs)

**Risk Check**:
- ⚠️ RISK #1 (VALIDATION) — Is 24-car sim achievable?
  - If FPS < 45 with 24 cars → activate fallback plan (16 cars + 8 static)
  - If FPS > 55 → green light for production

- ⚠️ RISK #2 (AI LAG) — Are pit decisions responsive?
  - AI should pit within 1-2 lap window of optimal
  - If AI lags, simplify decision tree

**Milestone**: "24-Car Grand Prix" — Full race with all 24 drivers AI-controlled

---

### **WEEK 7: Race Simulation Refinement**
**Theme**: Polish core race loop, make it fun

**Deliverables**:
- [ ] Safety car mechanics (triggered by crash)
- [ ] Pit crew animations/sounds (feel of pit stop)
- [ ] Driver radio system (text-based feedback)
- [ ] Race incident system (crashes, penalties, DNFs)
- [ ] Qualifying session (FP1/FP2/FP3/Q1/Q2/Q3)

**Tasks**:
- **Gameplay Programmers** (40 hours/week):
  - Implement safety car logic (detection, deployment, conditions)
  - Crash damage escalation (minor → major → DNF)
  - Penalty system (track limit violations, unsafe release from pit)
  - Qualifying stages (different time limits, elimination rules)

- **UI Designer** (30 hours/week):
  - Driver radio UI (speech bubbles, pit engineer suggestions)
  - Incident log (crash notifications, yellow flags)
  - Qualifying session screens (standings by Q1/Q2/Q3)

- **Technical Lead** (10 hours/week):
  - Marshal system architecture (event-based incident triggering)

**Risk Check**:
- Qualifying system should feel realistic but fast (not 1:1 simulation)

**Milestone**: "Full Race Weekend" — Qualifying + Race with realistic structure

---

### **WEEK 8: UI Polish & Strategy Advisor**
**Theme**: Complete race monitor UI, add strategy suggestions

**Deliverables**:
- [ ] Full race monitor UI complete (standings, telemetry, track map)
- [ ] Strategy advisor system (recommendations for pit/push/defend)
- [ ] Telemetry visualization (lap times, tire wear graph)
- [ ] Settings screen (difficulty, graphics quality, language)
- [ ] Tutorial/help system

**Tasks**:
- **UI Designer** (40 hours/week):
  - Implement telemetry graphs (lap time trend, tire degradation over laps)
  - Full race monitor layout (everything visible at once)
  - Settings screen (quality levels, controls remapping)
  - Tutorial mode (interactive first race)

- **Gameplay Programmers** (20 hours/week):
  - Strategy advisor logic (when to pit, which tire compound)
  - Recommendations for driver instructions (push/defend/save)
  - Real-time data processing (standings, gaps, pit window calculation)

- **QA Engineer** (30 hours/week):
  - Usability testing (can player find all features?)
  - Playtesting with 5-10 testers (non-developers)
  - Bug list from playtesting

**Risk Check**:
- ⚠️ RISK #7 (UI COMPLEXITY) — Is UI overwhelming?
  - Playtesting feedback: understand race in 1st race?
  - If yes → proceed. If no → simplify UI or add onboarding

**Milestone**: "Full UI" — Complete race experience, playable race 1-10

---

### **WEEK 9: Save/Load & Data Persistence**
**Theme**: Implement season persistence, save files, auto-saves

**Deliverables**:
- [ ] Save/load system (race state serialization)
- [ ] Auto-save every 5 laps
- [ ] Rollback/recovery (3 backup saves)
- [ ] Telemetry logging (for post-race analysis)
- [ ] Checksum validation (detect corrupted saves)

**Tasks**:
- **Gameplay Programmers** (35 hours/week):
  - Binary serialization/deserialization (efficient format)
  - LZ4 compression integration
  - Auto-save triggers (every 5 laps, before pit decision)
  - Checksum calculation & validation

- **Tools Programmer** (15 hours/week):
  - Save file recovery tools (restore from backups)
  - Telemetry log viewer (debug tool)

- **QA Engineer** (20 hours/week):
  - Serialize/deserialize stress test (100 cycles)
  - Corruption testing (corrupt bytes, verify rejection)
  - Load/save consistency test (should produce same race results)

**Risk Check**:
- ⚠️ RISK #3 (SAVE CORRUPTION) — Are saves reliable?
  - 100 save/load cycles without data loss = success
- ⚠️ RISK #5 (DETERMINISM) — Do saves produce same results?
  - Save at lap 10, load, compare lap 11+ vs continuous sim

**Milestone**: "Persistent Season" — Can play 5 race season, save/resume

---

### **WEEK 10: Prototype Finalization & Testing**
**Theme**: Polish, test, document everything for handoff to production

**Deliverables**:
- [ ] Prototype build (Windows .exe, distributable)
- [ ] Bug fixing pass (critical & high bugs)
- [ ] Performance optimization final pass
- [ ] Documentation (TDD complete, API reference, build instructions)
- [ ] Production kickoff readiness

**Tasks**:
- **All Team** (40 hours/week):
  - Bug fixing sprint (highest priority issues)
  - Performance profiling final pass
  - Code cleanup (comments, refactoring)

- **Technical Lead** (20 hours/week):
  - Performance optimization final review
  - Architecture validation for handoff
  - Build pipeline testing

- **QA Engineer** (40 hours/week):
  - Comprehensive testing (regression, edge cases)
  - Stability testing (5+ hour race sessions)
  - Performance validation (FPS consistency)
  - Bug report compilation

- **Producer (You)** (20 hours/week):
  - Test gameplay (play 3 full races yourself)
  - Gather feedback from testers
  - Document lessons learned for production phase
  - Schedule production kickoff

**Milestone**: "Prototype Shipped" — Game is playable, fun, and stable

---

## DELIVERABLES BY WEEK

| Week | Code | Design | Art | Docs | Status |
|------|------|--------|-----|------|--------|
| 1 | ✓ Project setup | ✓ UI sketches | - | ✓ TDD, roles | Complete |
| 2-3 | ✓ Physics engine | ✓ Basic UI | - | ✓ Physics spec | Physics validation |
| 4 | ✓ Tire/weather | ✓ Telemetry UI | - | ✓ Tire spec | Tire validation |
| 5-6 | ✓ AI system (24 cars) | ✓ Radio UI | - | ✓ AI spec | AI validation |
| 7 | ✓ Race mechanics | ✓ Incident UI | - | ✓ Race spec | Gameplay feel |
| 8 | ✓ Strategy advisor | ✓ Full UI | - | ✓ API docs | Usability test |
| 9 | ✓ Save/load system | - | - | ✓ Data spec | Reliability test |
| 10 | ✓ Bug fixes | ✓ Polish | - | ✓ Build guide | Final validation |

---

## SUCCESS CRITERIA (End of Week 10)

**Gameplay**:
- [ ] 1 complete race (Monaco) playable end-to-end
- [ ] Player makes meaningful strategic decisions (pit timing, tire compound)
- [ ] AI drives realistically (overtakes, pit strategy, crashes)
- [ ] Race completes in 20-30 minutes (with acceleration options)

**Technical**:
- [ ] Stable 55+ FPS with 24 cars on reference hardware
- [ ] No memory leaks over 1-hour session
- [ ] Save/load works reliably (100 cycles without corruption)
- [ ] Physics simulation deterministic (save/load produces same results)

**Player Experience**:
- [ ] Tutorial explains basic mechanics in <5 minutes
- [ ] Non-gamer can complete 1st race without help
- [ ] Race feels exciting (close finishes, dramatic pit stops)

**Documentation**:
- [ ] TDD complete (5 parts) and finalized
- [ ] Risk assessment with mitigation strategies
- [ ] Production roadmap for 24-36 weeks development
- [ ] API documentation for all modules

---

## RESOURCE ALLOCATION

### Person-Weeks per Role

| Role | W1 | W2-3 | W4 | W5-6 | W7 | W8 | W9 | W10 | Total |
|------|----|----|----|----|----|----|----|----|-------|
| **Producer** | 5 | 5 | 5 | 5 | 5 | 5 | 5 | 10 | 45 |
| **Tech Lead** | 10 | 20 | 5 | 10 | 5 | 5 | 5 | 10 | 70 |
| **Gameplay Prog** | 10 | 30 | 30 | 30 | 40 | 20 | 35 | 30 | 225 |
| **AI Specialist** | 5 | 5 | 5 | 40 | 10 | 5 | 5 | 10 | 85 |
| **UI Designer** | 5 | 5 | 20 | 5 | 30 | 40 | 5 | 10 | 120 |
| **QA Engineer** | 5 | 15 | 15 | 20 | 10 | 30 | 20 | 40 | 155 |
| **TOTAL** | 40 | 80 | 80 | 110 | 100 | 105 | 75 | 110 | **700 hours** |

**Average team size**: 5-6 people  
**Weekly commitment**: 40-110 hours (spikes Week 5-6 for 24-car scaling)

---

## RISK CHECKPOINTS

**Week 3 End**: Physics performance with 8 cars
- If FPS > 60 → proceed with 24 cars
- If FPS < 50 → activate optimization task force

**Week 6 End**: 24-car AI system
- If FPS > 45 → pass prototype phase
- If FPS < 40 → activate fallback plan (16 cars)

**Week 8 End**: Playtesting feedback
- If 80% understand game → proceed
- If <60% → redesign UI, extend timeline

**Week 10 End**: Stability validation
- If zero crashes in 5-hour session → pass
- If instability detected → extend testing week

---

## CONTINGENCY OPTIONS

**If falling behind schedule**:
1. **Reduce scope**: Remove qualifying (just race), simplify AI morale
2. **Extend timeline**: Shift production start from Week 11 → Week 12
3. **Defer features**: Save system, telemetry logging move to Week 1 of production

**If physics doesn't scale to 24 cars**:
- **Option A**: Reduce to 16 AI cars + 8 simplified/ghost cars (still playable)
- **Option B**: Accept 30 FPS instead of 60 FPS (turn-based game is less sensitive to FPS)
- **Option C**: Delay 24-car feature to Post-Launch patch (launch with 12 cars)

---

## HANDOFF TO PRODUCTION

**At Week 10 end, production team receives**:
1. Working prototype (playable, fun)
2. TDD documentation (complete technical design)
3. Build pipeline (reproducible builds, version control)
4. Asset list (what needs art, 3D models, animations)
5. Feature roadmap (priority order for full game)
6. Known issues log (bugs to fix, optimizations to do)

**Production phase begins Week 11**: Full team (10-15 people) ramps up for 24-36 weeks development

---

**Document Version**: 1.0  
**Last Updated**: 2026-04-06  
**Prepared by**: Claude Assistant  
**Status**: Ready for team review and approval
