# RISKS ASSESSMENT & MITIGATION
## Autosport Manager 2027

**Date**: 2026-04-06  
**Status**: Pre-Production Review  

---

## RISK REGISTER

### HIGH-PRIORITY RISKS (Critical Path Impact)

#### RISK #1: Physics Engine Performance Bottleneck
**Severity**: HIGH  
**Probability**: MEDIUM (60%)  
**Impact**: Game unplayable (FPS < 30 at target hardware)

**Problem**:
- 24 simultaneous cars × physics calculations per frame
- Collision detection: 276 potential car-pairs (24×23÷2)
- Tire temperature updates (4 tires × 24 cars per frame)
- Expected at ~100+ ms per physics frame (unacceptable)

**Mitigation Strategy**:
1. **Prototype with 8 cars first** (not 24)
   - Achieves proof-of-concept faster
   - Profiles real bottlenecks before heavy optimization
   - If 8 cars runs at 60 FPS, scaling to 24 becomes feasible
   
2. **Physics optimization techniques** (in priority order):
   - Spatial partitioning (quadtree): Reduces collision checks from 276 to ~40 pairs
   - SIMD vectorization (Burst compiler): 3-4× speedup on numerical math
   - Fixed timestep: Decouple simulation from rendering (sim at 50 Hz, render at 60 FPS)
   - Reduce physics precision on distant cars (LOD system)
   
3. **Fallback plan** (if 24 cars unachievable):
   - **Option A**: Reduce simultaneous AI cars to 16-18 (rest are simplified/static)
   - **Option B**: Extend development timeline (use 2-3 months for optimization)
   - **Option C**: Target lower FPS (stable 30 FPS acceptable for management game)

**Success Criteria**:
- 8-car prototype: 60+ FPS on reference hardware (GTX 1060, i5-8400)
- 24-car race: 50+ FPS minimum (acceptable for turn-based management)
- Profile data showing bottleneck location (CPU? collision? AI?)

**Timeline**: Prototype phase (Weeks 2-4)

---

#### RISK #2: AI Decision Lag & Unresponsiveness
**Severity**: HIGH  
**Probability**: MEDIUM (55%)  
**Impact**: Game feels unresponsive, AI makes poor pit decisions

**Problem**:
- 24 AI drivers making decisions simultaneously
- Each decision involves complex evaluation: pit strategy, fuel, degradation, morale
- If AI thread falls behind physics thread, decisions are stale/incorrect
- Player sees AI pitting at bad times (wasted pit stop)

**Mitigation Strategy**:
1. **Parallel AI thread** (separate from physics thread)
   - Main thread: Rendering (60 FPS)
   - Physics thread: Simulation (50 Hz fixed step)
   - AI thread: Decisions (~10 Hz, 100 ms cycles)
   - No blocking between threads (eventual consistency)

2. **Simplified decision tree** (vs. complex optimization):
   - Don't try to find "optimal" pit window (too slow)
   - Use heuristics: pit when degradation > 0.8 OR fuel < 10 laps
   - Morale affects risk tolerance (not slow computation)

3. **Caching & memoization**:
   - Pit window calculation cached (update every 5 laps, not every frame)
   - Competitor analysis cached (update every 100 ms, not continuously)

**Success Criteria**:
- AI decisions made within 100 ms (no visible lag)
- AI pit stops occur at reasonable times (within 2-lap window of optimal)
- AI responds to weather changes within 1 lap

**Timeline**: Module prototype (Weeks 5-6)

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

#### RISK #4: Memory Leaks Over Long Race
**Severity**: MEDIUM  
**Probability**: MEDIUM-HIGH (70%)  
**Impact**: Frame rate degradation after 1+ hour play session

**Problem**:
- Long-running races (30+ min at 1x speed)
- Event listeners not unsubscribed (common in game code)
- Garbage collection stutters at 60+ FPS (noticeable pause every 2-3 seconds)
- Player frustration: "Frame rate got worse mid-race"

**Mitigation Strategy**:
1. **Profiling from day 1**:
   - Unity Memory Profiler (built-in)
   - Monitor memory growth over 1-hour test run
   - Flag leaks immediately (before shipping)

2. **Best practices**:
   - Event unsubscription on object destruction
   - Object pooling for frequently-created objects (telemetry logs)
   - Nullable types for explicit memory management

3. **GC tuning**:
   - Mono GC settings: `Time.capturFrameRate` during races to control GC
   - Use `GC.Collect()` during load screens (not during race)

4. **Testing**:
   - Automated test: run 1-hour race at 1x speed, log memory every minute
   - Alert if memory growth > 100 MB/hour

**Success Criteria**:
- Memory growth < 50 MB over 1-hour race
- No frame drops below 58 FPS in final 30 minutes
- Zero detected memory leaks in profiler

**Timeline**: QA phase (Weeks 9-10)

---

#### RISK #5: Floating-Point Determinism Loss
**Severity**: MEDIUM  
**Probability**: MEDIUM (50%)  
**Impact**: Save/load produces different race outcomes (unfair to player)

**Problem**:
- Physics simulation depends on floating-point math (velocity, position)
- Floating-point is non-associative: (a+b)+c ≠ a+(b+c) in many cases
- Different compiler optimizations can change precision slightly
- Save/load might produce different results than continuous simulation

**Mitigation Strategy**:
1. **Fixed-point arithmetic** (where possible):
   - Use integer math for tire degradation (0-1000 scale instead of 0.0-1.0 float)
   - Use fixed-step physics integration (semi-implicit Euler, not adaptive)

2. **Deterministic math functions**:
   - Avoid `Math.Sqrt()` (precision varies by CPU)
   - Use lookup tables for expensive functions (sin, cos)
   - Explicit casting to float (prevent double precision conversion)

3. **Physics replay validation**:
   - Record first 5 laps without save/load
   - Save at lap 5, load, continue race
   - Compare lap 6+ data: should match within ±0.01 seconds
   - If diverges, investigate and fix

**Success Criteria**:
- Load/save test: lap time difference < 0.5%
- 10 save/load cycles show consistent results
- Physics replay deterministic within floating-point epsilon

**Timeline**: Prototype testing (Week 6)

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
