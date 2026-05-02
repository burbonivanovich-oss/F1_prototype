# F1 Manager 2027 — Project Root

## Structure

```
F1_prototype/
  autosport-manager-2027/    Python prototype — canonical design reference
  F1Manager2027-Unity/       Unity 2022.3 LTS — production target
  prototypes/                Early spike experiments
```

## How the two codebases relate

`autosport-manager-2027/` is the **source of truth** for all game mechanics:
formulas, tuning constants, tire curves, overtake weights, AI thresholds, and
design decisions. Its `CLAUDE.md` is the primary reference document.

`F1Manager2027-Unity/` is a faithful C# port. All simulation logic lives in
`Assets/Scripts/Core/` with zero `UnityEngine` dependencies. When a mechanic
changes, update Python first (faster to iterate), then mirror it to C#.

## Where to look

| Question | Go to |
|----------|-------|
| How does tire degradation work? | `autosport-manager-2027/CLAUDE.md` |
| What are the overtake formula weights? | `autosport-manager-2027/CLAUDE.md` |
| How does the AI decide to pit? | `autosport-manager-2027/CLAUDE.md` |
| Which C# class implements the pit AI? | `F1Manager2027-Unity/CLAUDE.md` |
| How do I run the Unity tests? | `F1Manager2027-Unity/CLAUDE.md` |
| What's the EDUO API signature? | `F1Manager2027-Unity/CLAUDE.md` |
| What design docs exist? | `autosport-manager-2027/documentation/` |

## Running things

```bash
# Python prototype
cd autosport-manager-2027
pip install rich
python -m src.main

# Unity — open F1Manager2027-Unity/ in Unity 2022.3.20f1, then hit Play
# Tests — Unity → Window → Test Runner → EditMode → Run All
```
