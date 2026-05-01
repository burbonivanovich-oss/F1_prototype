using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace F1Manager
{
    /// <summary>
    /// Binds the RaceEngine simulation state to the RaceScreen.uxml UI.
    /// Call StartRace() to begin; handles command input and per-lap display updates.
    /// </summary>
    public class RaceViewController
    {
        private readonly VisualElement _root;
        private readonly GameManager   _gm;
        private RaceEngine             _engine;

        // ── Cached element refs ───────────────────────────────────────────────
        private Label       _lapCounter;
        private Label       _weatherIcon, _weatherLabel, _tempLabel, _forecastIcons;
        private VisualElement _scBadge, _vscBadge;
        private ScrollView  _standingsList;
        private VisualElement _playerPanels;
        private Label       _trackMapArt;
        private ScrollView  _eventLog;
        private TextField   _commandInput;
        private Button      _confirmBtn, _ff5Btn, _ff10Btn;

        // ── State ─────────────────────────────────────────────────────────────
        private List<DriverInfo> _playerDrivers;
        private int _pendingAdvanceLaps = 0;  // 0=waiting for command, N=auto-advance
        private bool _raceRunning = false;

        // ─────────────────────────────────────────────────────────────────────

        public RaceViewController(VisualElement root, GameManager gm)
        {
            _root = root;
            _gm   = gm;
            CacheElements();
            BindButtons();
        }

        private void CacheElements()
        {
            _lapCounter     = _root.Q<Label>("lap-counter");
            _weatherIcon    = _root.Q<Label>("weather-icon");
            _weatherLabel   = _root.Q<Label>("weather-label");
            _tempLabel      = _root.Q<Label>("temp-label");
            _forecastIcons  = _root.Q<Label>("forecast-icons");
            _scBadge        = _root.Q("sc-badge");
            _vscBadge       = _root.Q("vsc-badge");
            _standingsList  = _root.Q<ScrollView>("standings-list");
            _playerPanels   = _root.Q("player-panels");
            _trackMapArt    = _root.Q<Label>("track-map-art");
            _eventLog       = _root.Q<ScrollView>("event-log");
            _commandInput   = _root.Q<TextField>("command-input");
            _confirmBtn     = _root.Q<Button>("confirm-btn");
            _ff5Btn         = _root.Q<Button>("ff5-btn");
            _ff10Btn        = _root.Q<Button>("ff10-btn");
        }

        private void BindButtons()
        {
            _confirmBtn.clicked += () => HandleCommand(_commandInput.value);
            _ff5Btn.clicked     += () => AdvanceLaps(5);
            _ff10Btn.clicked    += () => AdvanceLaps(10);
            _commandInput.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode == KeyCode.Return) HandleCommand(_commandInput.value);
            });
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void StartRace(RaceEngine engine)
        {
            _engine = engine;
            _raceRunning = true;
            _playerDrivers = _gm.GetTeamDrivers(_gm.SelectedTeam.id);

            AddEventLogEntry("🏁 Lights out and away we go!", "event-info");
            Refresh();
        }

        // ── Command handling ──────────────────────────────────────────────────

        private void HandleCommand(string raw)
        {
            if (!_raceRunning) return;
            _commandInput.value = "";
            raw = raw.Trim().ToLower();

            if (raw == "q" || raw == "quit") { EndRace(); return; }
            if (raw == "ff" || raw == "5")   { AdvanceLaps(5);  return; }
            if (raw == "fff" || raw == "10") { AdvanceLaps(10); return; }

            // Team orders
            var toMap = new Dictionary<string, TeamOrder>
            {
                ["tofree"] = TeamOrder.FREE_RACE,   ["to0"] = TeamOrder.FREE_RACE,
                ["tohold"] = TeamOrder.HOLD_GAP,    ["toswap"] = TeamOrder.SWAP_DRIVERS,
                ["topush"] = TeamOrder.PUSH_BOTH,
            };
            if (toMap.TryGetValue(raw, out var order))
            {
                _engine.CommandTeamOrder(order);
                AddEventLogEntry($"▶ Team order: {order}", "event-info");
                AdvanceLaps(1);
                return;
            }

            // Per-car commands: b44, b44s/m/h, a44, d44, mn44, fs44
            var compoundMap = new Dictionary<char, TireCompound>
            {
                ['s'] = TireCompound.C5, ['m'] = TireCompound.C4, ['h'] = TireCompound.C3,
                ['i'] = TireCompound.INTER, ['w'] = TireCompound.WET,
            };
            var instrMap = new Dictionary<string, DriverInstruction>
            {
                ["a"] = DriverInstruction.ATTACK,  ["d"] = DriverInstruction.DEFEND,
                ["mn"] = DriverInstruction.MANAGE, ["fs"] = DriverInstruction.FUEL_SAVE,
            };

            bool matched = false;
            foreach (var driver in _playerDrivers)
            {
                string n = driver.carNumber.ToString();
                if (raw.StartsWith("b" + n))
                {
                    string suffix = raw.Substring(1 + n.Length);
                    TireCompound? cmpd = null;
                    if (suffix.Length == 1 && compoundMap.TryGetValue(suffix[0], out var c)) cmpd = c;
                    _engine.CommandPit(driver.id, cmpd);
                    AddEventLogEntry($"▶ Pit command: #{n} → {(cmpd.HasValue ? cmpd.Value.DisplayName() : "auto")}", "event-player");
                    matched = true;
                }
                else
                {
                    foreach (var kv in instrMap)
                    {
                        if (raw == kv.Key + n)
                        {
                            _engine.CommandInstruction(driver.id, kv.Value);
                            AddEventLogEntry($"▶ #{n}: {kv.Value.DisplayName()}", "event-player");
                            matched = true;
                        }
                    }
                }
            }

            // Advance 1 lap on any command (or blank enter)
            AdvanceLaps(1);
        }

        private void AdvanceLaps(int count)
        {
            if (!_raceRunning || _engine == null) return;
            var state = _engine.RaceState;
            int actual = Math.Min(count, state.TotalLaps - state.CurrentLap);
            for (int i = 0; i < actual; i++)
            {
                if (state.IsRaceComplete) break;
                _engine.SimulateLap();
            }
            Refresh();
            if (state.IsRaceComplete) EndRace();
        }

        private void EndRace()
        {
            _raceRunning = false;
            _gm.OnRaceComplete();
        }

        // ── Display refresh ───────────────────────────────────────────────────

        private void Refresh()
        {
            var state = _engine.RaceState;
            RefreshTopBar(state);
            RefreshStandings(state);
            RefreshPlayerPanels(state);
            RefreshEventLog(state);
        }

        private void RefreshTopBar(RaceState state)
        {
            _lapCounter.text = $"LAP {state.CurrentLap} / {state.TotalLaps}";

            // Weather
            var weatherIcons = new Dictionary<WeatherCondition, string>
            {
                [WeatherCondition.DRY]        = "☀",
                [WeatherCondition.LIGHT_RAIN] = "🌦",
                [WeatherCondition.HEAVY_RAIN] = "⛈",
                [WeatherCondition.DRYING]     = "🌤",
            };
            _weatherIcon.text  = weatherIcons.TryGetValue(state.Weather, out var ic) ? ic : "?";
            _weatherLabel.text = state.Weather.ToString().Replace("_", " ");
            _tempLabel.text    = $"{state.TrackTempC:F0}°C";

            _forecastIcons.text = string.Join(" ", state.WeatherForecast.Select(w =>
                weatherIcons.TryGetValue(w, out var fi) ? fi : "?"));

            // SC / VSC badges
            bool scActive  = state.SafetyCar == SafetyCarState.DEPLOYED;
            bool vscActive = state.SafetyCar == SafetyCarState.VSC;
            _scBadge.style.display  = scActive  ? DisplayStyle.Flex : DisplayStyle.None;
            _vscBadge.style.display = vscActive ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void RefreshStandings(RaceState state)
        {
            _standingsList.Clear();
            var sorted = state.SortedCars();
            var drivers = GameDataFactory.GetAllDriversDict();
            var teams   = GameDataFactory.GetTeamsDict();
            var available = _gm.GetAvailableCompounds(_gm.SelectedCircuit);

            foreach (var car in sorted)
            {
                if (!drivers.TryGetValue(car.DriverID, out var drv)) continue;
                if (!teams.TryGetValue(car.TeamID, out var team)) continue;

                var row = new VisualElement();
                row.AddToClassList("standings-row");
                if (car.TeamID == state.PlayerTeamID) row.AddToClassList("player-row");
                if (car.DNF) row.AddToClassList("dnf-row");
                if (!car.DNF && car.GapToAheadS > 0f && car.GapToAheadS <= 0.5f)
                    row.AddToClassList("battle-row");

                // Position
                var pos = new Label(car.DNF ? "DNF" : car.Position.ToString());
                pos.AddToClassList("label-pos");
                row.Add(pos);

                // Car number
                var num = new Label($"#{car.CarNumber}");
                num.AddToClassList("label-num");
                row.Add(num);

                // Driver name (colored by team)
                var name = new Label(drv.shortName);
                name.AddToClassList("label-name");
                name.style.color = ParseHexColor(team.colorHex);
                if (car.TeamID == state.PlayerTeamID) name.style.unityFontStyleAndWeight = FontStyle.Bold;
                row.Add(name);

                // Gap
                var gapText = FormatGap(car);
                var gap = new Label(gapText);
                gap.AddToClassList("label-gap");
                if (!car.DNF && car.GapToAheadS > 0f && car.GapToAheadS <= 0.5f)
                    gap.style.color = new Color(1f, 0.27f, 0.27f);
                row.Add(gap);

                // Gap trend
                var trend = new Label(GetGapTrend(car));
                trend.AddToClassList("label-trend");
                row.Add(trend);

                // Tire
                string raceLabel = GetRaceLabel(car.Compound, available);
                var tire = new Label($"{raceLabel}·{car.TireAgeLaps:D2}");
                tire.AddToClassList("label-tire");
                tire.AddToClassList(GetTireClass(car.TirePhase));
                row.Add(tire);

                // Pits
                var pit = new Label(car.PitStopCount.ToString());
                pit.AddToClassList("label-pit");
                row.Add(pit);

                // Instruction
                var instr = new Label(car.Instruction.ToString().Substring(0, 3));
                instr.AddToClassList("label-instr");
                instr.AddToClassList(GetInstrClass(car.Instruction));
                row.Add(instr);

                // Last lap
                string lastStr = FormatLapTime(car.LastLapTimeS);
                var last = new Label(lastStr);
                last.AddToClassList("label-last");
                // Fastest lap: magenta
                if (car.DriverID == state.FastestLapDriverID && state.FastestLapTimeS > 0)
                    last.style.color = new Color(0.8f, 0.2f, 1f);
                row.Add(last);

                _standingsList.Add(row);
            }
        }

        private void RefreshPlayerPanels(RaceState state)
        {
            _playerPanels.Clear();
            var playerCars = state.GetPlayerCars();
            var drivers    = GameDataFactory.GetAllDriversDict();
            var teams      = GameDataFactory.GetTeamsDict();
            var available  = _gm.GetAvailableCompounds(_gm.SelectedCircuit);
            var sorted     = state.SortedCars();
            var circuit    = _gm.SelectedCircuit;
            int lapsLeft   = state.TotalLaps - state.CurrentLap;

            foreach (var car in playerCars)
            {
                if (!drivers.TryGetValue(car.DriverID, out var drv)) continue;
                if (!teams.TryGetValue(car.TeamID, out var team)) continue;

                var panel = new VisualElement();
                panel.AddToClassList("player-car-panel");

                // Header
                var header = new VisualElement(); header.AddToClassList("car-header");
                var nameL = new Label($"{drv.driverName} #{car.CarNumber}");
                nameL.AddToClassList("car-name");
                nameL.style.color = ParseHexColor(team.colorHex);
                var posL = new Label($"P{car.Position}");
                posL.AddToClassList("car-position");
                header.Add(nameL); header.Add(posL);
                panel.Add(header);

                // Gap
                var gapL = new Label(car.Position == 1 ? "RACE LEADER" :
                    car.GapToLeaderS >= 9000 ? "LAP DOWN" : $"Gap: +{car.GapToLeaderS:F3}s");
                gapL.AddToClassList("car-gap-line");
                panel.Add(gapL);

                // Tire status
                string raceLabel = GetRaceLabel(car.Compound, available);
                int window = 0;
                if (TireProfiles.All.TryGetValue(car.Compound, out var prof))
                    window = TireSystem.WindowRemaining(prof, car.TireAgeLaps, circuit.tireDegMultiplier);

                var tireRow = new VisualElement(); tireRow.AddToClassList("tire-status-line");
                var tireLbl = new Label("Tyre: "); tireLbl.AddToClassList("tire-label"); tireRow.Add(tireLbl);
                var tireInfo = new Label($"{raceLabel} ({car.Compound}) [{car.TireAgeLaps} laps] — {car.TirePhase}");
                tireInfo.AddToClassList("tire-info");
                tireInfo.AddToClassList(GetTireClass(car.TirePhase));
                tireRow.Add(tireInfo);
                if (car.TirePhase == TirePhase.CLIFF)
                {
                    var warn = new Label(" ‼ PIT NOW!"); warn.AddToClassList("tire-warning"); tireRow.Add(warn);
                }
                else if (window <= 3)
                {
                    var warn = new Label($" ⚠ {window} laps!"); warn.AddToClassList("tire-warning"); tireRow.Add(warn);
                }
                panel.Add(tireRow);

                // Sparkline
                if (car.LapTimes.Count >= 3)
                {
                    var sparkRow = new VisualElement(); sparkRow.AddToClassList("sparkline-row");
                    var sl = new Label("Pace: "); sl.AddToClassList("sparkline-label"); sparkRow.Add(sl);
                    var spark = new Label(BuildSparkline(car.LapTimes, 18));
                    spark.AddToClassList("sparkline-chars");
                    spark.AddToClassList(GetTireClass(car.TirePhase));
                    sparkRow.Add(spark);
                    int stintStart = Math.Max(0, car.LapTimes.Count - car.TireAgeLaps);
                    var stintTimes = car.LapTimes.Skip(stintStart).ToList();
                    if (stintTimes.Count >= 2)
                    {
                        float best = stintTimes.Min();
                        float last = stintTimes.Last();
                        float delta = last - best;
                        var dl = new Label($" +{delta:F2}s vs best");
                        dl.AddToClassList(delta < 0.3f ? "tire-plateau" : delta < 1.0f ? "tire-linear" : "tire-cliff");
                        sparkRow.Add(dl);
                    }
                    panel.Add(sparkRow);
                }

                // Sector times
                if (car.LastSectorTimes[0] > 0.1f)
                {
                    var secRow = new VisualElement(); secRow.AddToClassList("sector-row");
                    var sl = new Label("Sectors: "); sl.AddToClassList("sector-label"); secRow.Add(sl);
                    for (int s = 0; s < 3; s++)
                    {
                        if (s > 0) { var sep = new Label("│"); sep.AddToClassList("sector-sep"); secRow.Add(sep); }
                        float t = car.LastSectorTimes[s];
                        float b = car.BestSectorTimes[s];
                        var sl2 = new Label($"S{s + 1} {t:F3}");
                        sl2.AddToClassList(s == 0 ? "sector-s1" : s == 1 ? "sector-s2" : "sector-s3");
                        sl2.style.color = GetSectorColor(t, b);
                        secRow.Add(sl2);
                    }
                    panel.Add(secRow);
                }

                // Fuel
                float fuelLaps = car.FuelKg / Math.Max(0.01f, circuit.fuelConsumptionKg);
                var fuelRow = new VisualElement(); fuelRow.AddToClassList("fuel-row");
                var fl = new Label("Fuel: "); fl.AddToClassList("fuel-label"); fuelRow.Add(fl);
                var fv = new Label($"{car.FuelKg:F1} kg (~{fuelLaps:F0} laps)");
                fv.AddToClassList(fuelLaps > lapsLeft + 3 ? "fuel-ok" : fuelLaps > lapsLeft ? "fuel-warn" : "fuel-critical");
                fuelRow.Add(fv); panel.Add(fuelRow);

                // Mode
                var modeRow = new VisualElement(); modeRow.AddToClassList("fuel-row");
                var ml = new Label("Mode: "); ml.AddToClassList("fuel-label"); modeRow.Add(ml);
                var mv = new Label(car.Instruction.ToString());
                mv.AddToClassList(GetInstrClass(car.Instruction));
                modeRow.Add(mv); panel.Add(modeRow);

                // Momentum
                if (Math.Abs(car.MoraleModifierS) > 0.02f)
                {
                    var moRow = new VisualElement(); moRow.AddToClassList("momentum-row");
                    var mol = new Label("Momentum: "); mol.AddToClassList("momentum-label"); moRow.Add(mol);
                    string moText; string moCls;
                    if (car.MoraleModifierS < -0.12f)       { moText = $"IN THE ZONE ({car.MoraleModifierS:+0.00;-0.00}s)"; moCls = "momentum-zone"; }
                    else if (car.MoraleModifierS < -0.02f)  { moText = $"on form ({car.MoraleModifierS:+0.00;-0.00}s)"; moCls = "momentum-good"; }
                    else if (car.MoraleModifierS > 0.10f)   { moText = $"struggling ({car.MoraleModifierS:+0.00;-0.00}s)"; moCls = "momentum-bad"; }
                    else                                     { moText = $"settling ({car.MoraleModifierS:+0.00;-0.00}s)"; moCls = ""; }
                    var mov = new Label(moText); if (!string.IsNullOrEmpty(moCls)) mov.AddToClassList(moCls);
                    moRow.Add(mov); panel.Add(moRow);
                }

                // Tire delta vs rivals
                int myIdx = sorted.FindIndex(c => c.DriverID == car.DriverID);
                if (myIdx >= 0)
                {
                    var tdRow = new VisualElement(); tdRow.AddToClassList("tire-delta-row");
                    var tdl = new Label("Tyre Δ: "); tdl.AddToClassList("tire-delta-label"); tdRow.Add(tdl);
                    if (myIdx > 0)
                    {
                        var ahead = sorted[myIdx - 1];
                        int d = car.TireAgeLaps - ahead.TireAgeLaps;
                        var v = new Label($"P{ahead.Position}:{(d >= 0 ? "+" : "")}{d}  ");
                        v.AddToClassList(d < -3 ? "tire-delta-fresh" : d < 3 ? "tire-delta-equal" : "tire-delta-old");
                        tdRow.Add(v);
                    }
                    if (myIdx < sorted.Count - 1)
                    {
                        var behind = sorted[myIdx + 1];
                        if (!behind.DNF)
                        {
                            int d = car.TireAgeLaps - behind.TireAgeLaps;
                            var v = new Label($"P{behind.Position}:{(d >= 0 ? "+" : "")}{d}");
                            v.AddToClassList(d < -3 ? "tire-delta-fresh" : d < 3 ? "tire-delta-equal" : "tire-delta-old");
                            tdRow.Add(v);
                        }
                    }
                    panel.Add(tdRow);
                }

                // Engineer advice
                if (prof != null)
                {
                    string advice;
                    if (car.TirePhase == TirePhase.CLIFF) advice = "⚑ ENGINEER: Box box box!! Tyre is destroyed!";
                    else if (window <= 4) advice = $"⚑ ENGINEER: Consider pitting — {window} laps of grip left";
                    else advice = "⚑ ENGINEER: All looks good. Stay out.";
                    var advL = new Label(advice); advL.AddToClassList("engineer-advice"); panel.Add(advL);
                }

                // Pit projection
                var availDry = available.Where(c => c.IsDry()).ToList();
                var proj = AIStrategyEngine.PitStopProjection(car, sorted, circuit, availDry, lapsLeft);
                if (proj != null)
                {
                    string projStr;
                    string projCls;
                    int delta = proj.PositionAfter - proj.PositionBefore;
                    if (proj.CarsLost == 0)     { projStr = $"⚑ Pit now: P{proj.PositionBefore}→P{proj.PositionAfter} {proj.Compound.DisplayName()} (clean stop!)"; projCls = "proj-good"; }
                    else if (proj.CanRecover)   { projStr = $"⚑ Pit now: P{proj.PositionBefore}→P{proj.PositionAfter} (+{delta}p, recover ~L{state.CurrentLap + proj.LapsToRecover})"; projCls = "proj-ok"; }
                    else                        { projStr = $"⚑ Pit now: P{proj.PositionBefore}→P{proj.PositionAfter} (+{delta}p, hard to recover)"; projCls = "proj-hard"; }
                    var pjL = new Label(projStr); pjL.AddToClassList("pit-projection"); pjL.AddToClassList(projCls);
                    panel.Add(pjL);
                }

                // Quick pit buttons
                var pitBtns = new VisualElement(); pitBtns.AddToClassList("pit-buttons");
                foreach (var cmpd in available.Where(c => c.IsDry()))
                {
                    string lbl = GetRaceLabel(cmpd, available);
                    var btn = new Button(() =>
                    {
                        _engine.CommandPit(car.DriverID, cmpd);
                        AdvanceLaps(1);
                    });
                    btn.text = $"PIT {lbl}";
                    btn.AddToClassList("pit-button");
                    btn.AddToClassList(lbl == "S" ? "pit-soft" : lbl == "M" ? "pit-medium" : "pit-hard");
                    pitBtns.Add(btn);
                }
                panel.Add(pitBtns);

                _playerPanels.Add(panel);
            }
        }

        private void RefreshEventLog(RaceState state)
        {
            // Only append new events (don't rebuild from scratch every lap)
            int existingCount = _eventLog.childCount;
            var allEvents = state.Events;
            for (int i = existingCount; i < allEvents.Count; i++)
            {
                var ev = allEvents[i];
                var label = new Label($"[L{ev.Lap}] {ev.DriverName}: {ev.Message}");
                label.AddToClassList(ev.IsPlayerEvent ? "event-player" :
                    ev.Category == "WARNING" ? "event-warning" : "event-info");
                _eventLog.Add(label);
            }
            // Scroll to bottom
            _eventLog.ScrollTo(_eventLog.Children().LastOrDefault());
        }

        private void AddEventLogEntry(string text, string cssClass)
        {
            var label = new Label(text);
            label.AddToClassList(cssClass);
            _eventLog.Add(label);
        }

        // ── Formatting helpers ────────────────────────────────────────────────

        private static string FormatGap(CarState car)
        {
            if (car.DNF) return "DNF";
            if (car.Position == 1) return "LEADER";
            if (car.GapToLeaderS >= 9000) return "LAP";
            return $"+{car.GapToLeaderS:F3}";
        }

        private static string FormatLapTime(float t)
        {
            if (t <= 0) return "—";
            int m = (int)(t / 60f);
            float s = t % 60f;
            return $"{m}:{s:06.3f}";
        }

        private static string GetGapTrend(CarState car)
        {
            if (car.GapHistory.Count < 4) return "=";
            float recent = car.GapHistory.Last();
            float earlier = car.GapHistory[car.GapHistory.Count - 4];
            float diff = recent - earlier;
            return diff < -0.3f ? "▲" : diff > 0.3f ? "▼" : "=";
        }

        private static string GetRaceLabel(TireCompound compound, List<TireCompound> available)
        {
            var dry = available.Where(c => c.IsDry()).OrderBy(c => (int)c).ToList();
            if (!dry.Contains(compound)) return compound.DisplayName();
            int idx = dry.IndexOf(compound);
            return idx == 0 ? "H" : idx == 1 ? "M" : "S";
        }

        private static string GetTireClass(TirePhase phase) => phase switch
        {
            TirePhase.PLATEAU => "tire-plateau",
            TirePhase.LINEAR  => "tire-linear",
            TirePhase.CLIFF   => "tire-cliff",
            TirePhase.WARM_UP => "tire-warmup",
            _ => ""
        };

        private static string GetInstrClass(DriverInstruction instr) => instr switch
        {
            DriverInstruction.ATTACK    => "instr-attack",
            DriverInstruction.DEFEND    => "instr-defend",
            DriverInstruction.FUEL_SAVE => "instr-fuelsave",
            _ => "instr-manage"
        };

        private static string BuildSparkline(List<float> times, int width)
        {
            const string bars = "▁▂▃▄▅▆▇█";
            var recent = times.Skip(Math.Max(0, times.Count - width)).ToList();
            if (recent.Count < 2) return "";
            float min = recent.Min();
            float max = recent.Max();
            float range = Math.Max(0.01f, max - min);
            return string.Concat(recent.Select(t =>
            {
                int idx = (int)((t - min) / range * (bars.Length - 1));
                return bars[Math.Clamp(idx, 0, bars.Length - 1)].ToString();
            }));
        }

        private static Color GetSectorColor(float t, float best)
        {
            if (t <= best + 0.001f) return new Color(0.8f, 0.2f, 1f);    // magenta = PB
            if (t <= best + 0.15f)  return Color.green;
            if (t <= best + 0.5f)   return Color.yellow;
            return Color.red;
        }

        private static Color ParseHexColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            return Color.white;
        }
    }
}
