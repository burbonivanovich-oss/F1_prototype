# RISKS ASSESSMENT & MITIGATION
## Autosport Manager 2027

**Date**: 2026-04-06  
**Status**: Pre-Production Review  

---

## RISK REGISTER

### HIGH-PRIORITY RISKS (Critical Path Impact)

#### RISK #1: Over-Engineered Architecture (Lap-Tick Approach Risk)
**Severity**: HIGH  
**Probability**: MEDIUM (50%)  
**Impact**: Wrong architectural direction wastes weeks, delays prototype

**Problem**:
- If team chooses "full physics" approach (Approach A) instead of "distance-based" (Approach B)
- Full physics adds 30+ weeks of development (vs 8 weeks for distance-based)
- Player never steers/drives car → physics unnecessary for gameplay
- Risk: Team implements full physics, realizes too complex mid-Week 4, must restart

**Mitigation Strategy**:
1. **CRITICAL: Week 1 Kickoff Decision** (MUST HAPPEN)
   - Review ARCHITECTURE_DECISION_PHYSICS_VS_SIMPLIFIED.md
   - Choose Approach: A (full physics) / B (distance-based) / C (hybrid)
   - **Recommendation**: Approach B or C (management game, not racing sim)
   - Document decision in writing (signature from tech lead, producer)

2. **Prototype validation** (Week 2):
   - Implement lap-tick simulator with 3 cars
   - Verify: lap time formula works, overtake logic works, deterministic
   - If formula-based approach feels good → continue Approach B/C
   - If feels too "gamey" → pivot to hybrid with more detail

3. **Fallback plan**:
   - If team chose wrong approach mid-project (Week 3), can't easily pivot
   - Better to decide correctly NOW than restart later

**Success Criteria**:
- Approach decision made and documented by Apr 13 (end of Week 1)
- 3-car prototype running deterministically by end of Week 2
- Team agrees: "This formula-based approach captures the strategy"

**Timeline**: Week 1 (CRITICAL), validate Week 2

---

#### RISK #2: AI Makes Illogical Pit Strategy Decisions
**Severity**: HIGH  
**Probability**: MEDIUM (55%)  
**Impact**: Player sees AI pitting at wrong times, undermines strategy game

**Problem**:
- AI pit decision logic must seem realistic to player
- If AI pits too early (fresh tires) or too late (cliff phase crash), game feels broken
- Decision tree must account for fuel, tire degradation, position, morale
- Testing hard: need to watch 10+ races to validate pit timing is reasonable

**Mitigation Strategy**:
1. **Simple decision tree** (not complex optimization):
   - Use heuristic thresholds: pit when degradation > 0.75 AND fuel < 15 laps
   - Don't try to find globally optimal pit window (too complex)
   - Player shouldn't see AI pitting within 1-2 laps of optimal (acceptable)

2. **Playtesting in Week 4** (pit strategy validation):
   - Watch 3 full races
   - Log all AI pit stops (lap number, tire choice, result)
   - Check: AI pit timing reasonable? Fuel decisions sensible?
   - If AI pitting illogically → simplify decision tree further

3. **Fallback plan**:
   - If AI logic too complex for Week 4: remove morale/risk factors
   - Use deterministic pit rules: "pit at lap 25 if soft tires, lap 35 if medium"
   - Less realistic, but predictable and understandable

**Success Criteria**:
- AI pit timing within 2-lap window of "reasonable" (subjective, playtested)
- No AI crashes into pit wall or dangerous driving
- Player says: "AI strategy makes sense" after watching 2 races

**Timeline**: Week 4 implementation, Week 4 playtesting validation

---

#### RISK #3: Save/Load System Corruption
**Severity**: HIGH  
**Probability**: LOW (20%)  
**Impact**: Player loses race progress (catastrophic for user experience)

**Problem**:
- Large binary save files (50 MB uncompressed)
- Complex floating-point state (position, velocity, tire temp)
- Deserialization bugs could corrupt race state
- Power failure during save could create corrupted file

**Mitigation Strategy**:
1. **Atomic writes**:
   - Write to temporary file first
   - Checksum validation
   - Atomic file swap (replace old with new)
   - Never overwrite old save until new is verified

2. **Auto-backups**:
   - Keep 3 rolling backups of save files
   - Auto-save every 5 laps (before critical decision points)
   - User can rollback to previous save

3. **Version control**:
   - Save format versioning in header
   - Migration code for format changes
   - Validation on load: checksum check

4. **Testing**:
   - Unit tests for serialization/deserialization
   - Fuzz testing (corrupt random bytes, verify rejection)
   - Stress test: save/load 100 times, verify consistency

**Success Criteria**:
- 1,000 save/load cycles without corruption
- Corrupted saves detected and rejected (not silent failure)
- Auto-backup restore tested and working

**Timeline**: Testing phase (Week 10)

---

#### RISK #4: Memory Leaks Over Long Play Session
**Severity**: MEDIUM  
**Probability**: MEDIUM (60%)  
**Impact**: Frame rate degradation after 2+ hours (multiple races)

**Problem**:
- Telemetry logs accumulate (every lap = new data point)
- Event listeners not unsubscribed (common Unity bug)
- GC pauses (noticeable at 30 FPS target, even 50ms pause is 1 frame)
- Player plays 5 races straight (2+ hours) → "game slowed down"

**Mitigation Strategy**:
1. **Profiling from Week 1**:
   - Unity Profiler window (built-in, free)
   - Monitor heap growth during 2-hour play session
   - Mark leaks immediately (fix before Week 10)

2. **Best practices**:
   - Event unsubscription: `OnDestroy()` cleans up listeners
   - Telemetry log cap: only keep last 1000 entries (not infinite)
   - Object pooling: reuse pit-stop UI elements, not recreate

3. **GC tuning**:
   - GC.Collect() only during race load screens
   - Avoid large allocations during race (heap fragmentation)

4. **Testing** (Week 10):
   - Automated: play 5 races back-to-back (~2 hours)
   - Log memory every 5 minutes
   - Alert if growth > 100 MB total

**Success Criteria**:
- Total memory growth < 200 MB over 5 races
- Frame rate stable (30 FPS ±1) throughout
- Zero memory leaks detected in profiler

**Timeline**: Profiling Week 1, validation Week 10

---

#### RISK #5: Save/Load Determinism Loss
**Severity**: MEDIUM  
**Probability**: MEDIUM (45%)  
**Impact**: Save at Lap 10, load, Lap 11 produces different results (unfair)

**Problem**:
- Lap time formula depends on floating-point math
- Floating-point is non-associative: (a+b)+c ≠ a+(b+c)
- Random number generation must be seeded identically for reproducibility
- If RNG seed lost on load → entire race diverges

**Mitigation Strategy**:
1. **Deterministic RNG**:
   - Store RNG seed in race state (not just save position)
   - When loading, restore exact RNG seed
   - Each "lap tick" advances RNG deterministically (same seed = same random numbers)

2. **Testing protocol** (Week 9):
   - Save at lap 5
   - Load and continue to lap 30
   - Separately, run lap 6-30 continuously (no load)
   - Compare: lap times should be identical ±0.0001 seconds
   - If diverges: debug RNG seed or floating-point accumulation

3. **Fallback**:
   - If determinism hard to achieve: document as "known issue"
   - Race divergence < 1% acceptable for player experience
   - Better to ship with minor divergence than delay by weeks

**Success Criteria**:
- 10 save/load cycles: < 0.01% lap time variation
- RNG seed properly restored on load
- Player unaware of any divergence (no visible effect)

**Timeline**: Week 9 (save/load module), full validation Week 10

---

### MEDIUM-PRIORITY RISKS (Schedule Impact)

#### RISK #6: Licensing Issues
**Severity**: MEDIUM  
**Probability**: HIGH (80%)  
**Impact**: Cannot use F1/real team names, delayed launch

**Problem**:
- F1 is heavily licensed (FIA, team-specific)
- Real driver likenesses require permissions
- Using real names without license = legal cease-and-desist

**Current Status**: Generic fictional teams + drivers (safe from licensing)

**Mitigation Strategy**:
1. **Design for licensing flexibility**:
   - All team names/driver names in data files (not hardcoded)
   - License-free art assets (fictional team logos)
   - Can easily swap data files for licensed version later

2. **License investigation** (Q3 2026):
   - Contact F1 rights holder (Liberty Media)
   - Understand licensing cost/timeline
   - Plan accordingly: licensed or remain fictional

3. **Pivot plan**:
   - If F1 licensing impossible: Create fictional "International Racing Championship" series
   - Real physics, fictional teams (still compelling)
   - Focus on gameplay depth, not branding

**Success Criteria**:
- Licensing decision made by Q3 2026 (before beta)
- Game design doesn't depend on F1 license

**Timeline**: Ongoing (decision needed Week 20)

---

#### RISK #7: UI Complexity Overwhelming Players
**Severity**: MEDIUM  
**Probability**: MEDIUM (45%)  
**Impact**: High learning curve, player frustration, lower engagement

**Problem**:
- Management games are complex (many screens, many decisions)
- Poor UX leads to: "I don't understand how pit strategy works"
- Player quits after 1 race

**Mitigation Strategy**:
1. **Tutorial/onboarding**:
   - Interactive tutorial (1st race walkthrough)
   - Overlay tooltips on first encounter with UI elements
   - Glossary of terms (DRS, ATR, PARC FERMÉ, etc.)

2. **Progressive disclosure**:
   - Hide advanced options initially (tire pressure tuning, fuel mapping)
   - Unlock features as player masters basics
   - Settings to disable advanced features

3. **Playtesting early** (Week 6):
   - Test UI with 5-10 non-gamers
   - Track: confusion points, time to complete first race
   - Iterate based on feedback

**Success Criteria**:
- Average player completes 1st race in 45 minutes
- 80% of beta testers understand pit strategy within 2 races
- Support ticket volume < 5% of user base

**Timeline**: Prototype playtesting (Week 6-7)

---

#### RISK #8: Network/Modding Community Needs
**Severity**: MEDIUM  
**Probability**: MEDIUM (40%)  
**Impact**: Missed opportunity for player retention via mods

**Problem**:
- Management games like FM have strong modding communities
- Lack of mod support = less player-generated content
- No guarantee players will add mods, but support enables it

**Mitigation Strategy**:
1. **Plan for mods** (post-launch):
   - Design data formats that support easy modification (JSON for team data)
   - Documentation for modding community
   - Steam Workshop integration (deferred to post-launch)

2. **MVP approach**:
   - Launch without mod support
   - Add modding tools in post-launch patch (if community demands)
   - Don't sacrifice core gameplay for premature mod infrastructure

**Success Criteria**:
- Game ships with documented JSON formats
- First mod appears within 1 month of launch
- Modding guide published within 2 months post-launch

**Timeline**: Post-launch (Q2-Q3 2025 planning)

---

### LOW-PRIORITY RISKS (Manageable)

#### RISK #9: Platform-Specific Optimization Needed
**Severity**: LOW  
**Probability**: LOW (30%)  
**Impact**: PC version requires driver-specific tweaks (AMD vs NVIDIA)

**Mitigation**: Standard game development problem (well-known solutions)
- Profile on both GPU vendors
- Use graphics abstraction layers (built into Unity)
- Release with day-1 driver compatibility guide

---

#### RISK #10: Third-Party Asset Conflicts
**Severity**: LOW  
**Probability**: MEDIUM (35%)  
**Impact**: Purchased asset has bugs/incompatibility with our code

**Mitigation**: Use only well-reviewed, widely-adopted assets (Cinemachine, DOTween)

---

## RISK SUMMARY TABLE

| # | Risk | Severity | Probability | Mitigation Status | Owner |
|---|------|----------|-------------|-------------------|-------|
| 1 | Physics bottleneck | HIGH | 60% | In progress | Tech Lead |
| 2 | AI lag | HIGH | 55% | Planning | Gameplay Lead |
| 3 | Save corruption | HIGH | 20% | Planning | Tools Programmer |
| 4 | Memory leaks | MEDIUM | 70% | Planning | Programmer |
| 5 | Physics determinism | MEDIUM | 50% | Planning | Programmer |
| 6 | Licensing | MEDIUM | 80% | Deferred (Q3) | Producer |
| 7 | UI UX complexity | MEDIUM | 45% | Mitigation → Week 6 | Designer |
| 8 | Modding community | MEDIUM | 40% | Post-launch plan | Producer |
| 9 | Platform optimization | LOW | 30% | Standard practice | Tech Lead |
| 10 | Asset conflicts | LOW | 35% | Dependency mgmt | Programmer |

---

## CONTINGENCY BUDGET

**Time Reserve**: 3 weeks (buffer for unexpected issues)
- Prototyping: 10 weeks → 13 weeks with contingency
- Can be consumed by: physics optimization, AI refinement, playtesting feedback

**Personnel Flexibility**:
- If physics team needs help: Designer can contribute to UI
- If AI complexity spikes: Reduce UI polish (defer post-launch)

**Scope Reduction** (if timeline threatened):
1. Remove weather rain simulation (dry races only)
2. Reduce AI drivers from 24 to 12 (rest are ghost cars)
3. Simplify tire degradation model (linear instead of 3-phase)

---

**Prepared by**: Claude Assistant  
**Status**: Ready for team review  
**Next Steps**: 
1. Assign risk owners (producer/tech lead)
2. Schedule prototyping phase risk validation (Weeks 2-4)
3. Monthly risk review meetings
