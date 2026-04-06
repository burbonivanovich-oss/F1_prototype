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

### **WEEK 1: Foundation & Architecture Decision** 
**Theme**: Setup, decide on approach (CRITICAL DECISION POINT)

**Deliverables**:
- [ ] Project initialized in Unity 2022.3 LTS
- [ ] Development environment (Git, build scripts)
- [ ] Initial project structure (folders, data structures)
- [ ] **CRITICAL: Architecture decision finalized** (Approach A/B/C)
- [ ] Team kickoff & roles assigned
- [ ] TDD REVISED Part 1 & 2 reviewed and signed off

**Tasks**:
- **Producer** (YOU):
  - Lead architecture decision meeting (read ARCHITECTURE_DECISION_PHYSICS_VS_SIMPLIFIED.md)
  - Facilitate team discussion: which approach matches vision?
  - Make final call: Approach A (full physics) / B (distance-based) / C (hybrid)
  - Document decision in writing
  
- **Technical Lead**:
  - Set up Unity project (minimal settings, no heavy physics)
  - Configure Git structure
  - Create data structures (RaceState, CarState, TireState)
  - Create 3-car mock race data

- **Gameplay Programmers**:
  - Implement RaceSimulator skeleton (lap calculator placeholder)
  - Create UI skeleton (race monitor layout)
  
- **QA Engineer**:
  - Create test plan document
  - Outline success criteria for prototype

**Risk Check**: 
- ⚠️ RISK #1 (CRITICAL) — Architecture decision
  - If wrong approach chosen → weeks of wasted effort mid-project
  - **Must decide by Apr 13** (end of Week 1)
  - Get tech lead + producer agreement in writing

**Milestone**: "Architecture Locked" — decision made, team aligned, project structure ready

---

### **WEEK 2-3: Lap-Tick Simulation Core**
**Theme**: Get lap-based race simulation working (3 cars first, then scale to 24)

**Deliverables**:
- [ ] Lap time calculator working (formula-based, not physics)
- [ ] Position updater working (overtake logic)
- [ ] Tire degradation model (3-phase formula)
- [ ] Weather system (precipitation evolution)
- [ ] 3-car race simulation deterministic end-to-end
- [ ] Save/load produces identical results

**Tasks**:
- **Gameplay Programmers** (40 hours/week):
  - Implement lap time calculator (formula from TDD Part 2)
  - Implement position updater (distance/time based)
  - Implement 3-phase tire degradation formula
  - Weather state evolution
  - Test: 3-car race 5 laps, save at lap 3, load, race continues identically
  
- **Technical Lead** (15 hours/week):
  - Review code architecture (ensure clean data flow)
  - Validate determinism (no floating-point drift)
  - Performance check (should be negligible for 3 cars)

- **UI Designer** (15 hours/week):
  - Implement standings panel (position, gap, tire info)
  - Basic track map (bird's eye view)
  - Test UI updates smoothly (30 FPS target)

- **QA Engineer** (20 hours/week):
  - Test lap formula (realistic times? setup affects times?)
  - Test overtake logic (fast cars pass slow cars?)
  - Test determinism (100 save/load cycles)

**Risk Check**: 
- ⚠️ RISK #5 (DETERMINISM) — Can we achieve save/load consistency?
  - Success: lap times match within 0.01% after load
  - Failure: investigate floating-point or RNG issues

**Milestone**: "Deterministic 3-Car Race" — works identically after save/load

---

### **WEEK 4: Scale to 24 Cars & AI Pit Strategy**
**Theme**: Expand to full grid, implement AI pit decisions

**Deliverables**:
- [ ] 24-car race simulation running (not just 3)
- [ ] AI pit strategy decision tree
- [ ] Morale system affecting driver behavior
- [ ] Pit stop mechanics (tire choice, fuel selection)
- [ ] Performance validated (should be fast, CPU not bottleneck)

**Tasks**:
- **Gameplay Programmers** (35 hours/week):
  - Scale simulation to 24 cars (should be trivial, formula-based)
  - Implement pit stop logic (tire change, fuel load, warm-up penalty)
  - Implement pit decision tree (when to pit, which compound)
  - Morale system (increase/decrease based on performance)

- **AI Specialist** (30 hours/week):
  - Implement pit strategy evaluation (fuel, tire degradation, position)
  - Implement morale dynamics
  - Test: AI pit timing reasonable? (within 1-2 lap window of optimal)

- **UI Designer** (20 hours/week):
  - Implement pit dialog (tire choice, fuel amount UI)
  - Implement driver instructions (push/defend/save buttons)
  - Implement pit window indicator ("pit now" suggestion)

- **QA Engineer** (25 hours/week):
  - Test AI pit timing (is it logical?)
  - Test pit stop execution (UI responsive?)
  - Test morale effects (do drivers behave differently?)
  - Playtesting: watch 2-3 full races, note AI decisions

**Risk Check**: 
- ⚠️ RISK #2 (AI LOGIC) — Do AI pit decisions make sense?
  - Play 3 full races, evaluate AI pit timing
  - If illogical: simplify decision tree
  - Success: player says "AI strategy makes sense" after 1 race

**Milestone**: "24-Car Grid Ready" — full race with all drivers, pit strategy working

---

### **WEEK 5: Race Mechanics & Incidents**
**Theme**: Add realism: crashes, safety car, qualifying, penalties

**Deliverables**:
- [ ] Collision detection & damage model
- [ ] Safety car mechanics (triggered by crash)
- [ ] Qualifying session (FP1/FP2/FP3/Q1/Q2/Q3)
- [ ] Incident logging (crashes, penalties, DNFs)
- [ ] Track limit violation detection

**Tasks**:
- **Gameplay Programmers** (35 hours/week):
  - Implement simple collision logic (probability-based, not physics)
  - Damage model (speed loss from crashes)
  - Safety car deployment (red flag, FCY period)
  - Qualifying stages (time limits, elimination)
  - Track limit penalties

- **UI Designer** (25 hours/week):
  - Implement qualifying UI (3 sessions, live timing)
  - Incident log display (crash notifications, yellow flags)
  - Safety car indicator

- **QA Engineer** (20 hours/week):
  - Test crash probability (realistic? not too often?)
  - Test qualifying format (realistic F1 rules?)
  - Test incident log clarity

**Risk Check**: 
- None major (all features formula-based, not risky)

**Milestone**: "Full Race Weekend" — Qualifying + Race structure complete

---

### **WEEK 6: UI Polish & Strategy Advisor**
**Theme**: Complete race monitor, add pit recommendations, telemetry graphs

**Deliverables**:
- [ ] Complete race monitor UI (all data visible at once)
- [ ] Telemetry graphs (lap time trend, tire degradation)
- [ ] Strategy advisor (pit window recommendations)
- [ ] Driver instructions UI (push/defend/save/hold position)
- [ ] Settings screen (graphics quality, language, controls)

**Tasks**:
- **UI Designer** (40 hours/week):
  - Implement telemetry graphs (lap time trend over race)
  - Tire wear visualization (% degradation)
  - Pit window indicator (lap 16-19 recommendation)
  - Full race monitor layout (everything visible, no scrolling)
  - Settings menu (quality levels, language, keybinds)

- **Gameplay Programmers** (20 hours/week):
  - Implement strategy advisor logic (pit window calculation)
  - Driver instructions system (queue push/defend/save)
  - Live telemetry data generation

- **QA Engineer** (30 hours/week):
  - Usability testing (can player find all features?)
  - Playtesting with 5-10 non-developers (does UI make sense?)
  - Bug tracking (everything responsive?)

**Risk Check**: 
- ⚠️ RISK #7 (UI COMPLEXITY) — Is UI overwhelming?
  - Playtesting feedback: understand race in 1st race?
  - If yes → proceed. If no → simplify or add tutorial

**Milestone**: "Full UI" — Complete race experience, playtesting begins

---

### **WEEK 7: Save/Load System & Season Persistence**
**Theme**: Implement save/load, season progression, telemetry logging

**Deliverables**:
- [ ] Save/load system (race state serialization)
- [ ] Auto-save every 5 laps
- [ ] 3-backup rollback system
- [ ] Telemetry logging (lap times, pit stops, incidents)
- [ ] Checksum validation (detect corrupted saves)

**Tasks**:
- **Gameplay Programmers** (35 hours/week):
  - Implement JSON serialization (human-readable)
  - Implement LZ4 compression (save file size small)
  - Auto-save triggers (every 5 laps, before pit decision)
  - Checksum calculation & validation

- **Tools Programmer** (15 hours/week):
  - Save file recovery tools (restore from backups)
  - Telemetry log viewer (debug tool)

- **QA Engineer** (20 hours/week):
  - Serialize/deserialize stress test (100 cycles)
  - Corruption testing (corrupt bytes, verify rejection)
  - Determinism test (save/load consistency)

**Risk Check**:
- ⚠️ RISK #3 (SAVE CORRUPTION) — Are saves reliable?
  - 100 save/load cycles without data loss = success
- ⚠️ RISK #5 (DETERMINISM) — Do saves produce same results?
  - Save at lap 10, load, compare lap 11+ vs continuous sim

**Milestone**: "Persistent Season" — Can play 5 races, save/resume

---

### **WEEK 8-9: Bug Fixes & Polish**
**Theme**: Fix bugs, optimize, playtesting integration, final tweaks

**Deliverables**:
- [ ] All critical bugs fixed (no crashes, no data loss)
- [ ] Performance optimization (maintain 30 FPS)
- [ ] Playtesting feedback integration
- [ ] Tutorial system or on-screen help
- [ ] Full 5-race season playable end-to-end

**Tasks**:
- **All Team** (40 hours/week):
  - Bug fixing sprint (prioritize critical issues)
  - Code cleanup (comments, refactoring)
  - Performance profiling (ensure 30 FPS stable)

- **Producer** (YOU):
  - Run internal playtest (play 5 races yourself)
  - Gather feedback from QA team
  - Document lessons learned
  - Prioritize fixes by severity

- **QA Engineer** (40 hours/week):
  - Comprehensive testing (regression, edge cases)
  - Stability testing (5-hour play session)
  - Performance validation (FPS consistency)
  - Bug report compilation & prioritization

**Risk Check**: 
- ⚠️ RISK #4 (MEMORY LEAKS) — Monitor heap during long play session
- ⚠️ RISK #2 (AI LOGIC) — Validate AI still makes sense after code changes

**Milestone**: "Feature Complete" — All systems working, bugs minor

---

### **WEEK 10: Final Testing & Validation**
**Theme**: Final QA, documentation, handoff to production team

**Deliverables**:
- [ ] Prototype build (Windows .exe, distributable on Steam)
- [ ] Final bug pass (zero critical bugs)
- [ ] Documentation (TDD complete, API reference, build guide)
- [ ] Performance profiling report
- [ ] Production kickoff plan

**Tasks**:
- **All Team** (20 hours/week):
  - Final bug fixes (critical only)
  - Performance final validation
  
- **Technical Lead** (15 hours/week):
  - Build pipeline testing (automated builds)
  - Architecture review for production team handoff
  - API documentation

- **QA Engineer** (35 hours/week):
  - Final regression testing (all features)
  - Stress test: 8-hour play session (multiple races)
  - Performance validation (30 FPS maintained)
  - Build verification (exe runs on clean machine)

- **Producer (You)** (25 hours/week):
  - Play 5 full races yourself
  - Gather playtest feedback from external testers
  - Compile lessons learned doc
  - Create production team handoff package
  - Schedule production kickoff meeting

**Milestone**: "Prototype Shipped" — Ready for production team, docs complete

---

## DELIVERABLES BY WEEK

| Week | Code | Design | Docs | Status |
|------|------|--------|------|--------|
| 1 | ✓ Project setup, data structures | ✓ UI sketches | ✓ TDD, architecture decision | CRITICAL: decide approach |
| 2-3 | ✓ Lap calculator, 3-car sim, determinism | ✓ Standings UI | ✓ Core formula specs | Determinism validated |
| 4 | ✓ 24-car scale, pit strategy, morale | ✓ Pit dialog | ✓ AI decision tree | AI logic validated |
| 5 | ✓ Crashes, safety car, qualifying | ✓ Incident UI | ✓ Race mechanics spec | Full weekend ready |
| 6 | ✓ Strategy advisor, telemetry | ✓ Full UI complete | ✓ UI architecture | Playtesting begins |
| 7 | ✓ Save/load system | - | ✓ Data persistence spec | Determinism validated |
| 8-9 | ✓ Bug fixes, polish, optimization | ✓ Polish | ✓ Integration notes | Feature complete |
| 10 | ✓ Final bugs, build testing | - | ✓ Build guide, API docs | Production handoff |

---

## SUCCESS CRITERIA (End of Week 10)

**Gameplay**:
- [ ] 1 complete race (Monaco) playable end-to-end
- [ ] Player makes meaningful strategic decisions (pit timing, tire compound, driver instructions)
- [ ] AI pit strategy reasonable (within 1-2 lap window of sensible timing)
- [ ] Race completes in 5-10 minutes real-time (or faster with acceleration options)
- [ ] 5-race season playable consecutively

**Technical**:
- [ ] Stable 30 FPS with 24 cars on reference hardware
- [ ] No memory leaks over 2-hour session (5 races)
- [ ] Save/load works reliably (100 cycles without corruption)
- [ ] Race simulation deterministic (save/load produces same lap times ±0.01%)
- [ ] CPU/GPU usage well below reference hardware capacity

**Player Experience**:
- [ ] Non-gamer completes 1st race without help
- [ ] Player wants to play race 2 ("just one more race")
- [ ] Race feels strategic (pit decisions matter, UI makes sense)
- [ ] No crashes, no data loss, no obvious bugs

**Documentation**:
- [ ] TDD complete (5 parts) and finalized
- [ ] Risk assessment with mitigation strategies
- [ ] Production roadmap for 24-36 weeks development
- [ ] API documentation for all modules

---

## RESOURCE ALLOCATION (Simplified Approach)

### Person-Weeks per Role

| Role | W1 | W2-3 | W4 | W5 | W6 | W7 | W8-9 | W10 | Total |
|------|----|----|----|----|----|----|-----|-----|-------|
| **Producer** | 8 | 5 | 5 | 5 | 5 | 5 | 8 | 15 | 56 |
| **Tech Lead** | 10 | 15 | 8 | 5 | 5 | 5 | 10 | 10 | 68 |
| **Gameplay Prog** | 12 | 40 | 35 | 35 | 20 | 35 | 25 | 15 | 217 |
| **AI Specialist** | 5 | 8 | 30 | 5 | 5 | 5 | 8 | 5 | 71 |
| **UI Designer** | 8 | 15 | 20 | 25 | 40 | 10 | 15 | 10 | 143 |
| **QA Engineer** | 8 | 20 | 25 | 20 | 30 | 20 | 30 | 35 | 188 |
| **TOTAL** | 51 | 103 | 123 | 95 | 105 | 80 | 96 | 90 | **743 hours** |

**Notes**:
- Same person-hour budget (~700 hours)
- Spread more evenly across weeks (no major spikes)
- W1 is critical (architecture decision is major time investment)
- W6 spike is UI/playtesting
- W2-3 physics complexity replaced with formula work (slightly longer)

**Average team size**: 5-6 people  
**Weekly commitment**: 50-130 hours (manageable, no extreme spikes)

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
