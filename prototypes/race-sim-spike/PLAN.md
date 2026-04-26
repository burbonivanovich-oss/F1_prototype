# Race Sim Spike — Plan

**Status**: Awaiting producer kickoff approval
**Timebox**: 5 working days, single developer
**Source**: prototyper agent proposal, 2026-04-25
**Cross-references**: ADR `ARCHITECTURE_DECISION_PHYSICS_VS_SIMPLIFIED.md` (status ACCEPTED), TDD `technical/01_ARCHITECTURE_OVERVIEW_REVISED.md` §1.6.1 / §1.6.2 / §1.7 / §1.9 / §1.14

This is a **throwaway** spike. Standards are intentionally relaxed: no error handling, hardcoded values, crash loudly. Nothing in this directory survives into production. The goal is to answer the riskiest unknowns in the just-accepted Hybrid + Level 2 architecture before Week 2 starts on real production code.

---

## 1. Question Being Answered

> Can the Hybrid sim + `SnapshotBuffer` + polyline projection actually produce smooth, interesting-looking 24-car races at acceptable frame cost?

If yes → green light Week 2 production work on real systems.
If no → re-architect before sinking weeks into the wrong foundation.

## 2. Risk-Ranked Hypotheses

1. **H1 — UI interpolation smoothness.** Invisible until you see it move; bad result invalidates the entire visual tier.
2. **H3 — GPS arc-length projection.** Unknown unknowns in OSM data quality; could eat days.
3. **H2 — 24-car frame budget.** Measurable, but depends on projection cost.
4. **H5 — `SnapshotBuffer` thread-swap readiness.** Low risk, mostly a code-reading / interface exercise.
5. **H4 — Overtake pattern quality.** Hardest to quantify; gut-check only this week.

The day-by-day plan attacks H1 / H3 first, defers H4 / H5 to later in the week.

## 3. Day-by-Day Plan

### Day 1 — Polyline Asset + Basic Projection

**Deliverable**: one circuit rendered as a white polyline in an empty Unity scene.

- Download Monza OSM data; extract the circuit loop as an ordered lat/lon list.
- Export to `Assets/Prototypes/RaceSimSpike/Data/monza_polyline.json` (flat array of `[lat, lon]` pairs).
- `PolylineLoader.cs` — converts lat/lon to local Unity XZ coordinates (simple equirectangular, good enough at one circuit's spatial scale).
- `ArcLengthTable.cs` — precomputes cumulative distances; exposes `WorldPositionAt(float t)` where `t ∈ [0, 1]` is normalized track distance.
- Render the polyline using `LineRenderer`.

**Day-1 fallback**: If OSM data for Monza is too noisy to produce a recognizable circuit shape without significant manual cleanup, switch to a hand-authored 20-point bezier and **note the data risk in the report**. This signals that production needs a dedicated polyline-import tool pass before the 24-circuit content lock-in.

### Day 2 — Fake Sim + SnapshotBuffer

**Deliverable**: 24 colored dots moving around the polyline.

- `FakeLapSim.cs` — 24 cars, each with a `track_distance` float advancing by `speed * delta` each 50 ms tick. No overtake logic yet. Speeds slightly randomized at start, then held constant.
- `SnapshotBuffer.cs` — double-buffered `CarSnapshot[24]` of structs `(position_t, car_id, color)`. Sim writes to back buffer; UI reads front. Single-threaded for now — the swap is a field reassign.
- `DotRenderer.cs` — reads front buffer, lerps each dot between previous and current snapshot using `Time.time` as interpolation parameter, calls `ArcLengthTable.WorldPositionAt()` for world position, moves a `GameObject` dot.

**End of Day 2** = first real visual answer to **H1**. Watch for jitter, snapping, drift.

### Day 3 — Frame Budget Measurement + Overtake Roll

**Deliverable**: profiler screenshot in the report; first-pass gut-check on race pattern quality.

- Add Unity Profiler markers around: sim tick, snapshot swap, per-dot lerp.
- Add a primitive overtake: each tick, if car N is within `0.005` of car N−1's `track_distance`, roll `Random.value < overtake_prob` and swap positions. Hardcode `overtake_prob = 0.15` (do **not** import the real sigmoid from `08_TUNING_OVERTAKE_TIRE_v0.md` — out of scope for spike).
- Watch ~5 minutes. Does it look like a race? Note subjectively in report.

**Bail-out check**: if sim tick > 2 ms or per-frame lerp loop > 1 ms at 24 cars, investigate **before Day 4** — do not push through to Day 4 with a budget violation.

### Day 4 — Thread-Swap Adapter + Stress Test

**Deliverable**: `ThreadBridgeAdapter.cs` proving the `SnapshotBuffer` interface is sufficient for a future thread boundary.

- `ThreadBridgeAdapter.cs` runs sim update inside a `Task.Run` coroutine (not true multithreading — but forces all data crossing to go through the buffer struct, proving the interface is sufficient for Option B migration).
- Increase car count to 50 to find the actual breaking point; record the number in the report.
- Capture final Profiler screenshot at 24 cars.

### Day 5 — Slip Buffer + Report

- First half: slip buffer for any of Days 1–4 that ran long.
- Second half: write `prototypes/race-sim-spike/REPORT.md` covering:
  - What was validated (per H1–H5).
  - What surprised.
  - What changed about the architecture (if anything).
  - 24-car Profiler screenshot.
  - 5-second screen recording of dots moving (Unity Recorder).

## 4. Concrete Deliverables

- `Assets/Prototypes/RaceSimSpike/` — all spike scripts.
- `Assets/Prototypes/RaceSimSpike/Data/monza_polyline.json` (or hand-authored bezier fallback).
- `Assets/Prototypes/RaceSimSpike/RaceSimSpike.unity` — runs standalone in Play mode.
- `prototypes/race-sim-spike/REPORT.md` — produced on Day 5.
- 24-car Profiler screenshot (attached to report).
- 5-second screen recording (attached to report).

## 5. Out of Scope (Do NOT Build)

- Real overtake formula tuning — being produced separately by systems-designer (`08_TUNING_OVERTAKE_TIRE_v0.md`).
- Real driver / team data.
- Real UI styling.
- Save / load.
- Pit stops.
- Multiple circuits.
- Any production-quality code that should survive into v1.

## 6. Kill Criteria

Stop the spike and **re-architect** if any of the following are true at end of Day 4:

- Sim tick + lerp loop combined exceeds **4 ms at 24 cars** after one obvious optimization attempt (projection table cache, struct vs class, etc.).
- Arc-length lerp produces **visible jitter or stutter** at 30 FPS that is not fixable by adjusting interpolation parameters within one hour.
- OSM polyline data requires more than **4 hours of manual cleanup** to produce one usable circuit — indicates the GPS data pipeline needs a dedicated tool pass before production.
- `ThreadBridgeAdapter` reveals that `SnapshotBuffer` requires **mutable shared state** to function, invalidating the immutability guarantee in TDD §1.9.

## 7. Status

**Awaiting producer approval before any code is written.**
