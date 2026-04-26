# ARCHITECTURE DECISION: Full Physics vs Distance-Based Simulation

**Date**: 2026-04-06  
**Status**: ACCEPTED 2026-04-25  
**Impact**: Determines entire technical direction for TDD  

---

## PROBLEM STATEMENT

The project is a **management game** (like Football Manager, Motorsport Manager), **NOT** a driving simulator. Yet TDD Part 1-2 describes a full physics engine with:
- Semi-implicit Euler integration
- Collision detection (quadtree)
- Tire temperature/degradation physics
- 24-car multithreading for real-time simulation

**Question**: Is this necessary for a management game where **player never controls a car**?

**Answer**: Probably not. This architecture is over-engineered for the genre.

---

## TWO APPROACHES COMPARED

### APPROACH A: Full Physics Engine (Current TDD)

**What it includes**:
- Real-time physics simulation (50-100 Hz)
- Longitudinal dynamics (power, drag, acceleration)
- Lateral dynamics (cornering, tire grip, oversteer)
- Collision detection & damage model
- Tire temperature + degradation calculations
- 24 simultaneous cars in physics thread
- Player can "watch" realistic car behavior during race

**Pros**:
✅ Immersive — feels like watching real F1 (cars actually drive around track)
✅ Emergent gameplay — unexpected crashes, overtakes, incidents feel organic
✅ Extensible — can add features (ERS deployment, DRS, fuel maps) without redesign
✅ If it works, it's technically impressive

**Cons**:
❌ ~800+ hours development time (just for physics engine)
❌ Complex multithreading (main/physics/AI/render threads)
❌ Performance risk: 24 cars × 100 Hz = 2,400+ physics calculations/sec
❌ Debugging nightmare (physics bugs are subtle, hard to reproduce)
❌ Overkill for a management game (player never steers/accelerates)
❌ Determinism issues (save/load must be pixel-perfect)

**Example gameplay**: 
Player pits driver for mediums on Lap 15. During Lap 16, the game simulates the car exiting pit lane, tires warming up gradually, grip increasing each lap. By Lap 18, grip reaches optimal. Lap 19, driver can push hard. This feels great, but...

Does the player **see** this? Or just reads telemetry numbers?
If reads numbers → Full physics is wasted effort.

---

### APPROACH B: Distance-Based Simulation (Simplified)

**What it includes**:
- Lap-by-lap race progression (no frame-by-frame physics)
- Each car has: position (0-100%), fuel, tire degradation, morale
- Every "lap tick" (every 1-2 seconds real-time):
  - Update positions based on car speed (derived from setup + driver skill)
  - Apply tire degradation (formula, not physics)
  - Apply weather effects (rain → slower pace)
  - Check for overtakes (if car A speed > car B speed and close distance)
  - Check for pit stop triggers
- Race finishes in 5-10 minutes real-time (with acceleration options)
- No multithreading needed

**Pros**:
✅ Simple implementation (~100-200 hours)
✅ Fast development cycle (can test design quickly)
✅ Deterministic (same setup = same race every time, great for save/load)
✅ No threading complexity
✅ Performance: trivial (24 cars × 20 Hz = 480 calculations/sec, negligible)
✅ Debugging: easy (race progression is visible, deterministic)
✅ Appropriate for genre (management game, not sim)

**Cons**:
❌ Less immersive (race feels "ticky" not smooth)
❌ Less realistic (no actual physics, just formulas)
❌ Overtaking feels arbitrary (luck-based, not skill-based)
❌ Crashes = instant DNF (no gradual damage model)
❌ Player might feel: "I'm watching a spreadsheet, not a race"

**Example gameplay**:
Player pits for mediums on Lap 15. System instantly shows:
- Lap 16: Pit stop executed, tires fresh (full grip penalty -0.8 sec)
- Lap 17: Tires warming (-0.5 sec penalty)
- Lap 18: Tires optimal (0 sec penalty)
- Lap 19: Driver can push (+0.2 sec gain)

This is clear, deterministic, but less "cinematic."

---

## DECISION MATRIX

| Factor | Full Physics | Distance-Based | Weight |
|--------|--------------|-----------------|--------|
| **Dev Time** | 40 weeks | 8 weeks | HIGH |
| **Performance Risk** | HIGH | LOW | HIGH |
| **Immersion** | HIGH | MEDIUM | MEDIUM |
| **Gameplay Impact** | MEDIUM (player watches, doesn't drive) | MEDIUM | MEDIUM |
| **Debugging Ease** | LOW | HIGH | MEDIUM |
| **Genre Fit** | ❌ Overkill | ✅ Perfect | HIGH |
| **Extensibility** | ✅ Good | MEDIUM | LOW |

**Weighted Score**:
- Full Physics: 55 points (impressive, but risky)
- Distance-Based: 78 points (practical, safe, fast)

---

## HYBRID APPROACH (RECOMMENDED)

**Combine the best of both**:

```
RACE SIMULATION LOOP (Every 1-2 seconds real-time):
  1. Update each car's lap time (based on setup, driver skill, tire degradation)
  2. Update positions using simplified physics:
     - Current speed = base speed - tire_degradation - weather_penalty
     - If trailing car speed > leading car speed:
       Check overtake chance (skill gap, DRS available, distance to corner)
  3. Apply damage model:
     - If overtake fails (high risk): probability of collision
     - Collision → reduce speed by 0.5-2.0 sec/lap (damage penalty)
  4. Check pit stop conditions
  5. Display race progress (car positions, lap times, pit stops)

TIER SYSTEM (Visual Fidelity):
  Level 0: Text-based race log only ("Hamilton pits for mediums")
  Level 1: Position numbers + telemetry graphs
  Level 2: Simple 2D track map (dots = cars, line = overtake)
  Level 3: 3D visualization (optional post-launch)
```

**This approach**:
- Implements core gameplay in ~12 weeks (hybrid of A & B)
- Keeps simulation realistic (pit strategy still matters)
- Avoids full physics complexity
- Allows visual upgrade later (same simulation, better graphics)

---

## RECOMMENDATION FOR WEEK 1 KICKOFF

**Decision Point**: At Week 1 team meeting, discuss:

1. **Which approach aligns with vision?**
   - If "I want to FEEL like team principal watching real race" → Approach A (full physics)
   - If "I want STRATEGIC depth (pit decisions matter)" → Approach B (distance-based)
   - If "Mix of both, optimize for time" → Hybrid

2. **Risk tolerance?**
   - Can we afford physics scaling to fail (Approach A)?
   - Or do we need safe path to prototype (Approach B)?

3. **Timeline reality check**:
   - Full physics: 10 weeks pre-prod → maybe OK
   - Distance-based: 10 weeks pre-prod → definitely OK
   - Hybrid: 10 weeks pre-prod → tight but doable

**If choosing Approach B or Hybrid**, TDD Parts 1-2 need heavy revision:
- Remove threading model (single-threaded OK)
- Remove collision detection (just formula-based damage)
- Simplify tire system (degradation = linear formula, no physics)
- Simplify AI (no complex decision trees, simple pit window calculation)
- New TDD size: ~1000 lines instead of 4000

---

## NEXT STEPS

**Before Week 1 starts**:
1. Read this document
2. Ask yourself: **What's the core experience?** (watching simulation or making decisions?)
3. At kickoff, discuss as team
4. Make decision: **Approach A/B/Hybrid?**
5. If Approach B/Hybrid chosen: **TDD needs revision** (I can do it)

**Risk if decision is wrong**:
- Approach A chosen, but too complex → hit bottleneck Week 5-6, scramble to simplify
- Approach B chosen, but feels too gamey → players dislike it, pivot mid-production (expensive)

Better to decide now than fix later.

---

## APPENDIX: Why Current TDD Assumed Approach A

When you said "24 machines in real-time," I interpreted as "full driving simulation." That's where the physics engine came from.

But re-reading your GDD:
- Focus: **Team management** & race tactics
- Player actions: Pit stop decisions, driver instructions (push/defend/save)
- Player does NOT: Steer, accelerate, brake

This is clearly **Approach B or Hybrid**, not full physics.

**My mistake**: I should have asked for clarification instead of assuming.

---

**Decision Owner**: Producer (You)  
**Timeline to Decide**: By end of Week 1 (Apr 13)  
**Impact if Deferred**: TDD becomes incorrect, team builds wrong thing

This is the most important decision for the entire project. Get it right now.

---

## DECISION RECORDED — 2026-04-25

**Adopted approach**: **Hybrid** (lap-tick base from Approach B + probabilistic overtake resolution + lightweight collision/damage model).

**Visual fidelity**: **Level 2** — 2D track map with cars rendered as moving dots. No 3D for v1.

**Threading**: Single-threaded simulation (Option A from revised TDD §1.9), with an immutable `SnapshotBuffer` abstraction in place from day 1 so a future migration to two-threaded (Option B) is a boundary swap rather than a rewrite.

**Track geometry pipeline**: Per-circuit polylines sourced from open GPS tracks (e.g. OpenStreetMap traces). Licensing per circuit must be validated before content lock-in (see new medium-priority risk in `project-config.json`).

**Simulation determinism**: **Non-deterministic by design.** Same player inputs after a save/load may produce different race outcomes. Rationale: race replayability, anti-save-scum, and "living race" feel where reloading does not lock in past outcomes. This explicitly reverses the determinism requirement in `01_ARCHITECTURE_OVERVIEW_REVISED.md` §1.1 and §1.12 — those sections are revised to match.

**QA mitigation for non-determinism**: A dev-only seed override (env var `AUTOSPORT_RNG_SEED` or `--seed=<int>` CLI flag) freezes RNG in development and QA builds for bug reproduction. The override is compiled out / unreachable in release builds, so end users always get non-deterministic races.

**Rationale for Hybrid + Level 2**:
- Producer's core experience: "watch the race and make race-engineer decisions during it." Hybrid gives organic-feeling overtakes/incidents that justify watching; Level 2 dots are sufficient visual fidelity for that experience without 3D-track production cost.
- Hybrid resolves a pre-existing inconsistency in revised TDD §1.6 (loop calls `CheckCollision` despite §1.2 promising "no physics"). Hybrid legitimizes that call via the lightweight incident model.

**Consequences accepted**:
- Per-track polyline content pipeline (24 circuits) not previously scoped — added as risk.
- Overtake probability tuning is now a balance risk — added as risk.
- Loss of QA "load save, reproduce bug" workflow without dev-seed override — mitigated by override flag.
- TDD §1.1 / §1.12 determinism requirements removed.

**Alternatives considered and rejected**:
- Pure Approach B: rejected — overtakes would feel arbitrary, undermining "watch the race" goal.
- Full Approach A (physics): rejected per original ADR analysis — overkill for management genre.
- Strict input-driven determinism: rejected — producer wanted reloads to materially change race outcomes, not just QA reproducibility.

**Owner**: Producer
**Implementation kickoff**: Week 1 of pre-production (per existing roadmap).

