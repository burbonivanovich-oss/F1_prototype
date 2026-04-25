# Tuning v0: Overtake Sigmoid + Tire Degradation

**Status**: v0 — needs playtest tuning
**Date**: 2026-04-25
**Source**: systems-designer agent proposal, producer-approved with one binding decision (overtake cap = 0.85).
**Cross-references**: ADR `ARCHITECTURE_DECISION_PHYSICS_VS_SIMPLIFIED.md` (status ACCEPTED), TDD `technical/01_ARCHITECTURE_OVERVIEW_REVISED.md` §1.6.1 / §1.6.2, design `06_RACE_SIMULATION_MECHANICS.md` §6.2.

This document holds the first-pass numeric values for the Hybrid simulation's two probability-driven systems: overtake resolution and tire degradation. All numbers are starting points for playtest — none are locked.

---

## 1. Overtake Sigmoid (TDD §1.6.1)

### 1.1 Formula

```
logit = w1 * (skill_atk - skill_def)
      + w2 * (pace_atk - pace_def)
      + w3 * tire_age_delta
      + w4 * DRS_flag
      - w5 * track_overtake_difficulty
      - w6 * defender_aggression

P_raw    = 1 / (1 + exp(-logit))
P(success) = min(P_raw, 0.85)             // cap: see §1.4
```

### 1.2 Input Variables

| Symbol | Type | Range | Source |
|---|---|---|---|
| `skill_atk`, `skill_def` | float | 0–100 | Driver `overtaking` / `defending` sub-stats |
| `pace_delta = pace_atk - pace_def` | float | −3.0 … +3.0 s/lap | Current lap-time potential delta |
| `tire_age_delta` | int | −50 … +50 laps | `laps_since_pit_atk - laps_since_pit_def` |
| `DRS_flag` | bool | 0 / 1 | DRS available + active per track + race rules |
| `track_overtake_difficulty` | float | 0.0–1.0 | Per-circuit constant. 0 = Monza, 1 = Monaco |
| `defender_aggression` | float | 0.0–1.0 | Derived from driver instruction (push/defend/save) and morale |
| `P(success)` | float | (0, 0.85] | Sigmoid output, hard-capped |

### 1.3 v0 Weights

| Weight | v0 Value | Rationale | Status |
|---|---|---|---|
| `w1` (skill) | **0.04** | 25-pt skill advantage = +1.0 logit; meaningful but not dominant | locked v0 |
| `w2` (pace) | **1.80** | 0.5 s/lap pace advantage = +0.9 logit; rivals big skill gap | locked v0 |
| `w3` (tire age) | **0.05** | 10 laps fresher = +0.5 logit | locked v0 |
| `w4` (DRS) | **1.50** | Fixed +1.5 logit (~+24 pp at neutral baseline) | **flag for playtest** |
| `w5` (track difficulty) | **3.00** | Monaco (1.0) = −3.0 logit, Monza (0.0) = 0 | **flag for playtest** |
| `w6` (defender aggression) | **2.00** | Max-aggressive defender = −2.0 logit | **flag for playtest, gated on morale calibration** |

### 1.4 Hard Cap on P(success) — DECISION

`P(success) = min(P_raw, 0.85)`. **Cap = 0.85 (locked v0 by producer 2026-04-25.)**

Rationale: without the cap, a stacked-advantage scenario (top driver + DRS + fresh tires + low-difficulty track) produced P_raw ≈ 0.95 on Monza — overtakes would feel automatic. The cap preserves a 15% upset chance even in maximally favourable conditions. Drama is preserved; the wider sigmoid still differentiates the rest of the distribution.

Side effects to watch in playtest:
- Probability mass piles up at the cap when several inputs are favourable. If the "feels too easy" complaint persists, the next move is to lower `w4` (DRS) toward 1.20 rather than reduce the cap further.

### 1.5 Reference Scenarios

| # | Setup | logit | P_raw | P(success) after cap |
|---|---|---|---|---|
| 1 | Equal cars; DRS off; mid-difficulty track (0.5); passive defender (0.3) | −2.10 | 0.109 | 0.109 (≈ 11%) |
| 2 | +15 skill, +0.6 s/lap pace, 10-lap fresher, DRS on, Monza (0.1), passive defender (0.2) | +2.98 | 0.952 | **0.85 (capped)** |
| 3 | Equal cars; Monaco (1.0); aggressive defender (0.9); DRS off | −4.80 | 0.0082 | 0.0082 (≈ 0.8%) |

Targets per ADR brief: scenario 1 ≈ 5–10% (slightly above; see §1.6 open question), scenario 2 ≈ 50–70% (cap brings it down from saturation but still above target — **monitor in playtest**), scenario 3 < 2% ✓.

### 1.6 Open Tuning Questions

1. **Canonical mid-field track difficulty: 0.5 or 0.6?** At 0.5, scenario 1 yields 11% (above target). At 0.6 it yields ~8.3% (in target). v0 leaves at 0.5; flagged for design call before content lock-in on the 24 circuits.
2. **Scenario 2 still above target** (0.85 vs 50–70% target). The cap mitigates the "feels automatic" symptom but does not fully reach target. Acceptable for v0 — revisit after first playtest. Levers: lower `w4` to 1.20, raise track difficulty floor for "nominal" circuits.
3. **`w6` (defender aggression) coupling to morale.** Calibration of `defender_aggression` input depends on the morale + driver-instruction system; `w6` may shift once that range is settled.

### 1.7 Target Race-Level Distribution

Assumptions: 60-lap race, 20-car field, ~40 gap-close events per race (mixed circuit types), blended average P(success) ≈ 0.18 across attempts (mix of equal and advantaged cars).

- Attempts: ~40 per race
- Successful passes: ~7 (low-overtake circuits) … ~12 (Monza-like)

Below real F1's 30–50 passes/race — intentional. Management sim presents aggregate outcomes; players watch standings, not per-corner drama. References: F1 official 2021–2024 season averages; Motorsport Manager 2016 default tuning (~20–40 passes/race).

If first playtest reports "races feel static," primary lever is reducing `w5` (track difficulty floor) or raising attempt frequency by widening the gap-detection threshold in §1.6.1, not retuning `w1`–`w4`.

---

## 2. Tire Degradation (extends `06_RACE_SIMULATION_MECHANICS.md` §6.2)

All compounds use the same three-phase shape: plateau (no loss) → linear loss → cliff. Only boundaries and rates differ. Numbers below assume an optimal-temperature, average-degradation track ("canonical mid-field"). v0 — needs playtest tuning.

### 2.1 Per-Compound v0 Numbers

| Compound | Δ vs fresh medium | Plateau (laps) | Linear loss | Cliff onset | Cliff penalty | Useful life¹ | Operational temp | Optimal temp |
|---|---|---|---|---|---|---|---|---|
| **C5 Soft** | −0.35 s | 1–10 (±0.05 s) | +0.08 s/lap (laps 11–22) | lap 23 | +0.50 s | ~22 laps | 15–30 °C | 18–28 °C |
| **C4 Medium** | 0.00 s (baseline) | 1–15 (±0.05 s) | +0.06 s/lap (laps 16–35) | lap 36 | +0.40 s | ~34 laps | 10–38 °C | 20–32 °C |
| **C3 Hard** | +0.25 s | 1–20 (±0.05 s) | +0.04 s/lap (laps 21–45) | lap 46 | +0.35 s | ~46 laps | 18–40 °C | 25–35 °C |

¹ "Useful life" = lap at which cumulative pace loss exceeds 1.5 s; tires considered "done" beyond this.

### 2.2 Curve Formula

```
PaceLoss(lap) =
    0,                                                 if lap ≤ plateau_end
    rate_linear * (lap - plateau_end),                 if plateau_end < lap ≤ cliff_start
    rate_linear * (cliff_start - plateau_end)
        + cliff_penalty,                               if lap > cliff_start
```

### 2.3 Worked Example — Soft (C5), "done" Threshold = 1.5 s

- Phase 2 accrual: 0.08 × (22 − 10) = 0.96 s by end of lap 22
- Cliff at lap 23: + 0.50 s → cumulative 1.46 s
- Threshold (1.5 s) breached at lap 23–24
- Pit window: laps 20–22 (before cliff)

Matches the design intent in `06_RACE_SIMULATION_MECHANICS.md` §6.2.3.

### 2.4 Temperature Window Effects

Outside operational temp range: grip penalty applies as additional pace loss on top of the degradation curve. Suggested v0:

- Within optimal range: no penalty.
- Within operational but outside optimal: +0.10 s/lap.
- Outside operational: +0.40 s/lap and accelerated wear (multiply linear-phase rate by 1.5× while out-of-window).

Track + ambient temperature inputs and the formula linking them belong in the weather system spec — to be added when that system is detailed.

### 2.5 Weather Interaction (mandatory rules, not probabilities)

- **Rain onset → slick compounds illegal.** While `rain_flag = true` AND compound ∈ {C3, C4, C5}:
  - +5.0 s/lap aquaplane penalty applied immediately.
  - `incidentRiskAccumulator` increments by **+0.15 per lap** (see TDD §1.7 `CarState`).
  - Player must pit within 2 laps of rain onset or face near-certain incident escalation.
- Intermediate / wet compound numbers — out of scope for v0; to be specified when the weather system is detailed.

---

## 3. Status Summary

**Locked for v0:**
- All six overtake weights `w1`–`w6`
- Overtake cap = 0.85
- All tire compound v0 numbers
- Rain rule (slicks illegal, +5.0 s/lap, +0.15 incident risk/lap)

**Needs playtest before lock:**
- `w4`, `w5`, `w6` (flagged in §1.3)
- Canonical mid-field track difficulty value (§1.6 q1)
- Scenario-2 cap effectiveness (§1.6 q2)
- Tire numbers across all compounds (§2.1) once a playable race is running

**Blocked on other systems:**
- `w6` calibration depends on morale + driver-instruction system
- Temperature window formula depends on weather system spec
- Intermediate/wet tire numbers depend on weather system spec
