using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace F1Manager
{
    /// <summary>
    /// Controls all menu screens: team select, circuit select, strategy, results.
    /// Works with MainMenu.uxml.
    /// </summary>
    public class MenuViewController
    {
        private readonly VisualElement _root;
        private readonly GameManager   _gm;

        // ── Screen elements ───────────────────────────────────────────────────
        private VisualElement _teamScreen, _circuitScreen, _strategyScreen, _resultsScreen;
        private ScrollView    _teamList, _circuitList, _resultsList;
        private VisualElement _strategyContent, _resultsPodium, _constructorPoints;
        private Button        _teamConfirmBtn, _circuitConfirmBtn, _raceStartBtn, _newRaceBtn;

        // ── State ─────────────────────────────────────────────────────────────
        private TeamInfo    _selectedTeam;
        private CircuitInfo _selectedCircuit;
        private readonly Dictionary<int, StrategyOverride> _overrides = new();

        // ── Fuel slider refs ──────────────────────────────────────────────────
        private Slider _car1FuelSlider, _car2FuelSlider;
        private Label  _car1FuelVal, _car2FuelVal;
        private DropdownField _car1Compound, _car2Compound;
        private List<DriverInfo> _playerDrivers;

        // ─────────────────────────────────────────────────────────────────────

        public MenuViewController(VisualElement root, GameManager gm)
        {
            _root = root;
            _gm   = gm;
            CacheElements();
            BindButtons();
        }

        private void CacheElements()
        {
            _teamScreen     = _root.Q("team-select-screen");
            _circuitScreen  = _root.Q("circuit-select-screen");
            _strategyScreen = _root.Q("strategy-screen");
            _resultsScreen  = _root.Q("results-screen");

            _teamList          = _root.Q<ScrollView>("team-list");
            _circuitList       = _root.Q<ScrollView>("circuit-list");
            _strategyContent   = _root.Q("strategy-content");
            _resultsPodium     = _root.Q("results-podium");
            _resultsList       = _root.Q<ScrollView>("results-list");
            _constructorPoints = _root.Q("constructor-points");

            _teamConfirmBtn    = _root.Q<Button>("team-confirm-btn");
            _circuitConfirmBtn = _root.Q<Button>("circuit-confirm-btn");
            _raceStartBtn      = _root.Q<Button>("race-start-btn");
            _newRaceBtn        = _root.Q<Button>("new-race-btn");

            _car1FuelSlider = _root.Q<Slider>("car1-fuel");
            _car2FuelSlider = _root.Q<Slider>("car2-fuel");
            _car1FuelVal    = _root.Q<Label>("car1-fuel-val");
            _car2FuelVal    = _root.Q<Label>("car2-fuel-val");
            _car1Compound   = _root.Q<DropdownField>("car1-compound");
            _car2Compound   = _root.Q<DropdownField>("car2-compound");
        }

        private void BindButtons()
        {
            _teamConfirmBtn.clicked    += OnTeamConfirm;
            _circuitConfirmBtn.clicked += OnCircuitConfirm;
            _raceStartBtn.clicked      += OnRaceStart;
            _newRaceBtn.clicked        += () => _gm.OnNewRace();

            _car1FuelSlider?.RegisterValueChangedCallback(e =>
                { if (_car1FuelVal != null) _car1FuelVal.text = $"{e.newValue:F0} kg"; });
            _car2FuelSlider?.RegisterValueChangedCallback(e =>
                { if (_car2FuelVal != null) _car2FuelVal.text = $"{e.newValue:F0} kg"; });

            // Circuit type filter buttons
            BindFilterButton("filter-all",    null);
            BindFilterButton("filter-pwr",    "PWR");
            BindFilterButton("filter-df",     "DF");
            BindFilterButton("filter-street", "STREET");
        }

        private void BindFilterButton(string btnName, string filterType)
        {
            var btn = _root.Q<Button>(btnName);
            if (btn == null) return;
            btn.clicked += () =>
            {
                // Update active state
                foreach (var b in _root.Query<Button>(className: "filter-btn").ToList())
                    b.RemoveFromClassList("active-filter");
                btn.AddToClassList("active-filter");
                PopulateCircuitList(filterType);
            };
        }

        // ── Show screens ──────────────────────────────────────────────────────

        public void ShowTeamSelect()
        {
            ShowOnly(_teamScreen);
            PopulateTeamList();
        }

        public void ShowCircuitSelect(TeamInfo team)
        {
            _selectedTeam = team;
            ShowOnly(_circuitScreen);
            PopulateCircuitList(null);
        }

        public void ShowStrategyScreen(TeamInfo team, CircuitInfo circuit, QualifyingResult qr)
        {
            _selectedTeam    = team;
            _selectedCircuit = circuit;
            _playerDrivers   = _gm.GetTeamDrivers(team.id);
            ShowOnly(_strategyScreen);
            PopulateStrategyScreen(qr);
        }

        public void ShowResults(RaceState state)
        {
            ShowOnly(_resultsScreen);
            PopulateResults(state);
        }

        /// <summary>Hides all managed screens (so QualifyingViewController can take over).</summary>
        public void HideAll()
        {
            foreach (var s in new[] { _teamScreen, _circuitScreen, _strategyScreen, _resultsScreen })
            {
                if (s == null) continue;
                s.RemoveFromClassList("active-screen");
                s.AddToClassList("hidden-screen");
            }
        }

        private void ShowOnly(VisualElement screen)
        {
            HideAll();
            screen?.RemoveFromClassList("hidden-screen");
            screen?.AddToClassList("active-screen");
        }

        // ── Team list ─────────────────────────────────────────────────────────

        private int _selectedTeamID = -1;

        private void PopulateTeamList()
        {
            _teamList.Clear();
            var teams = GameDataFactory.Teams.OrderByDescending(t => t.carPerformance);
            foreach (var team in teams)
            {
                var row = BuildTeamRow(team);
                row.RegisterCallback<ClickEvent>(_ =>
                {
                    _selectedTeamID = team.id;
                    foreach (var r in _teamList.Query<VisualElement>(className: "team-row").ToList())
                        r.RemoveFromClassList("selected");
                    row.AddToClassList("selected");
                });
                _teamList.Add(row);
            }
        }

        private VisualElement BuildTeamRow(TeamInfo team)
        {
            var row = new VisualElement(); row.AddToClassList("team-row");

            var bar = new VisualElement(); bar.AddToClassList("team-color-bar");
            if (ColorUtility.TryParseHtmlString(team.colorHex, out var col))
                bar.style.backgroundColor = col;
            row.Add(bar);

            var name = new Label(team.teamName); name.AddToClassList("team-row-name"); row.Add(name);
            var abbr = new Label(team.shortName); abbr.AddToClassList("team-row-short"); row.Add(abbr);
            var perf = new Label($"★ {team.carPerformance}"); perf.AddToClassList("team-row-perf"); row.Add(perf);

            // Badges: PWR / DF
            var badges = new VisualElement(); badges.AddToClassList("team-row-badges");
            if (team.powerUnit >= 95) { var b = new Label("PWR+"); b.AddToClassList("team-badge badge-pwr"); badges.Add(b); }
            if (team.chassis >= 95)   { var b = new Label("DF+");  b.AddToClassList("team-badge badge-df");  badges.Add(b); }
            row.Add(badges);

            return row;
        }

        private void OnTeamConfirm()
        {
            if (_selectedTeamID < 0) return;
            var team = GameDataFactory.GetTeam(_selectedTeamID);
            if (team != null) _gm.OnTeamSelected(team);
        }

        // ── Circuit list ──────────────────────────────────────────────────────

        private int _selectedCircuitID = -1;

        private void PopulateCircuitList(string filter)
        {
            _circuitList.Clear();
            var circuits = GameDataFactory.Circuits.AsEnumerable();
            if (filter == "PWR")    circuits = circuits.Where(c => c.powerSensitivity >= 0.70f);
            if (filter == "DF")     circuits = circuits.Where(c => c.powerSensitivity <= 0.25f);
            if (filter == "STREET") circuits = circuits.Where(c => c.overtakeDifficulty >= 0.80f);

            foreach (var circuit in circuits)
            {
                var row = BuildCircuitRow(circuit);
                row.RegisterCallback<ClickEvent>(_ =>
                {
                    _selectedCircuitID = circuit.id;
                    foreach (var r in _circuitList.Query<VisualElement>(className: "circuit-row").ToList())
                        r.RemoveFromClassList("selected");
                    row.AddToClassList("selected");
                });
                _circuitList.Add(row);
            }
        }

        private VisualElement BuildCircuitRow(CircuitInfo circuit)
        {
            var row = new VisualElement(); row.AddToClassList("circuit-row");

            var name = new Label(circuit.circuitName); name.AddToClassList("circuit-row-name"); row.Add(name);
            var country = new Label(circuit.country); country.AddToClassList("circuit-row-country"); row.Add(country);
            var laps = new Label($"{circuit.totalLaps} laps"); laps.AddToClassList("circuit-row-laps"); row.Add(laps);
            var len = new Label($"{circuit.circuitLengthKm:F3} km"); len.AddToClassList("circuit-row-len"); row.Add(len);

            string typeLabel = circuit.powerSensitivity >= 0.70f ? "PWR" :
                               circuit.powerSensitivity <= 0.25f ? "DF"  : "BAL";
            string typeCls   = typeLabel == "PWR" ? "type-pwr" : typeLabel == "DF" ? "type-df" : "type-bal";
            var type = new Label(typeLabel); type.AddToClassList($"circuit-row-type {typeCls}"); row.Add(type);

            return row;
        }

        private void OnCircuitConfirm()
        {
            if (_selectedCircuitID < 0) return;
            var circuit = GameDataFactory.GetCircuit(_selectedCircuitID);
            if (circuit != null) _gm.OnCircuitSelected(circuit);
        }

        // ── Strategy screen ───────────────────────────────────────────────────

        private void PopulateStrategyScreen(QualifyingResult qr)
        {
            if (_playerDrivers == null || _playerDrivers.Count == 0) return;

            var available = _gm.GetAvailableCompounds(_selectedCircuit);
            var dryLabels = available.Where(c => c.IsDry()).OrderBy(c => (int)c).ToList();
            var compoundNames = dryLabels.Select(c =>
            {
                int idx = dryLabels.IndexOf(c);
                string label = idx == 0 ? "H" : idx == 1 ? "M" : "S";
                return $"{label} ({c})";
            }).ToList();

            // Car 1
            var d1 = _playerDrivers.Count > 0 ? _playerDrivers[0] : null;
            if (d1 != null)
            {
                var nameL = _root.Q<Label>("car1-name");
                if (nameL != null) nameL.text = $"{d1.driverName} #{d1.carNumber}";
                if (_car1Compound != null)
                {
                    _car1Compound.choices = compoundNames;
                    // Q2 compound rule
                    TireCompound startCmpd = qr.Q2CompoundMap.TryGetValue(d1.id, out var q2c) ? q2c : dryLabels[1];
                    int defIdx = dryLabels.IndexOf(startCmpd);
                    _car1Compound.index = Math.Max(0, defIdx);
                }
            }

            // Car 2
            var d2 = _playerDrivers.Count > 1 ? _playerDrivers[1] : null;
            if (d2 != null)
            {
                var nameL = _root.Q<Label>("car2-name");
                if (nameL != null) nameL.text = $"{d2.driverName} #{d2.carNumber}";
                if (_car2Compound != null)
                {
                    _car2Compound.choices = compoundNames;
                    TireCompound startCmpd = qr.Q2CompoundMap.TryGetValue(d2.id, out var q2c) ? q2c : dryLabels[1];
                    int defIdx = dryLabels.IndexOf(startCmpd);
                    _car2Compound.index = Math.Max(0, defIdx);
                }
            }

            // Recommended strategies (compute from tire physics)
            var stratList = _root.Q("strat-list");
            if (stratList != null)
            {
                stratList.Clear();
                var strats = ComputeStrategyHints(available, _selectedCircuit);
                foreach (var strat in strats.Take(4))
                {
                    var row = new VisualElement(); row.AddToClassList("strat-item");
                    var lbl = new Label(strat.Label); lbl.AddToClassList("strat-item-label"); row.Add(lbl);
                    var badge = new Label(strat.Stops == 1 ? "1 STOP" : "2 STOP");
                    badge.AddToClassList($"strat-item-badge {(strat.Stops == 1 ? "strat-1stop" : "strat-2stop")}");
                    row.Add(badge);
                    stratList.Add(row);
                }
            }

            // Grid positions from qualifying
            var gridHints = _root.Q("car1-strategy-hints");
            if (gridHints != null && d1 != null)
            {
                gridHints.Clear();
                int gridPos = qr.GridOrder.IndexOf(d1.id) + 1;
                var h = new Label($"Qualifying: P{gridPos}"); h.AddToClassList("strategy-hint"); gridHints.Add(h);
                if (qr.Q2CompoundMap.TryGetValue(d1.id, out var q2c2))
                {
                    var h2 = new Label($"Must start on: {GetRaceLabel(q2c2, available)} ({q2c2})");
                    h2.AddToClassList("strategy-hint"); gridHints.Add(h2);
                }
            }
        }

        private void OnRaceStart()
        {
            _overrides.Clear();
            var available = _gm.GetAvailableCompounds(_selectedCircuit);
            var dryLabels = available.Where(c => c.IsDry()).OrderBy(c => (int)c).ToList();

            if (_playerDrivers != null && _playerDrivers.Count > 0)
            {
                var d1 = _playerDrivers[0];
                var ov1 = new StrategyOverride();
                if (_car1FuelSlider != null) ov1.FuelKg = _car1FuelSlider.value;
                if (_car1Compound != null && _car1Compound.index >= 0 && _car1Compound.index < dryLabels.Count)
                    ov1.StartingCompound = dryLabels[_car1Compound.index];
                _overrides[d1.id] = ov1;
            }

            if (_playerDrivers != null && _playerDrivers.Count > 1)
            {
                var d2 = _playerDrivers[1];
                var ov2 = new StrategyOverride();
                if (_car2FuelSlider != null) ov2.FuelKg = _car2FuelSlider.value;
                if (_car2Compound != null && _car2Compound.index >= 0 && _car2Compound.index < dryLabels.Count)
                    ov2.StartingCompound = dryLabels[_car2Compound.index];
                _overrides[d2.id] = ov2;
            }

            _gm.OnStrategyConfirmed(_overrides);
        }

        // ── Results screen ────────────────────────────────────────────────────

        private void PopulateResults(RaceState state)
        {
            var sorted  = state.SortedCars();
            var drivers = GameDataFactory.GetAllDriversDict();
            var teams   = GameDataFactory.GetTeamsDict();
            int ptsMap(int pos) => pos switch { 1=>25,2=>18,3=>15,4=>12,5=>10,6=>8,7=>6,8=>4,9=>2,10=>1,_=>0 };
            bool flEligible = state.FastestLapTimeS > 0 &&
                              sorted.Any(c => c.DriverID == state.FastestLapDriverID && !c.DNF && c.Position <= 10);

            // Podium
            _resultsPodium.Clear();
            foreach (int p in new[] { 2, 1, 3 })
            {
                var podCar = sorted.FirstOrDefault(c => c.Position == p);
                if (podCar == null) continue;
                drivers.TryGetValue(podCar.DriverID, out var podDrv);
                teams.TryGetValue(podCar.TeamID, out var podTeam);

                var place = new VisualElement(); place.AddToClassList($"podium-place podium-p{p}");
                var posL = new Label(p == 1 ? "🏆" : p == 2 ? "🥈" : "🥉"); posL.AddToClassList("podium-pos"); place.Add(posL);
                var nameL = new Label(podDrv?.shortName ?? "?"); nameL.AddToClassList("podium-name"); place.Add(nameL);
                var teamL = new Label(podTeam?.shortName ?? ""); teamL.AddToClassList("podium-team"); place.Add(teamL);
                _resultsPodium.Add(place);
            }

            // Full result list
            _resultsList.Clear();
            foreach (var car in sorted)
            {
                drivers.TryGetValue(car.DriverID, out var drv);
                teams.TryGetValue(car.TeamID, out var team);

                int pts = ptsMap(car.Position);
                if (flEligible && car.DriverID == state.FastestLapDriverID) pts++;

                var row = new VisualElement(); row.AddToClassList("result-row");
                if (car.TeamID == state.PlayerTeamID) row.style.backgroundColor = new Color(0.05f, 0.1f, 0.15f);

                var posL = new Label(car.DNF ? "DNF" : car.Position.ToString());
                posL.AddToClassList("result-pos");
                if (car.Position == 1) posL.style.color = new Color(1f, 0.8f, 0f);
                row.Add(posL);

                var numL = new Label($"#{car.CarNumber}"); numL.AddToClassList("result-num"); row.Add(numL);

                var nameL = new Label(drv?.driverName ?? "?"); nameL.AddToClassList("result-name");
                if (team != null && ColorUtility.TryParseHtmlString(team.colorHex, out var col))
                    nameL.style.color = col;
                row.Add(nameL);

                var teamL = new Label(team?.shortName ?? ""); teamL.AddToClassList("result-team"); row.Add(teamL);

                string gap = car.DNF ? "DNF" : car.Position == 1 ? "—" :
                    car.GapToLeaderS >= 9000 ? "+1 LAP" : $"+{car.GapToLeaderS:F3}";
                var gapL = new Label(gap); gapL.AddToClassList("result-gap"); row.Add(gapL);

                var ptsL = new Label(pts > 0 ? pts.ToString() : ""); ptsL.AddToClassList("result-pts"); row.Add(ptsL);

                // FL badge
                var flL = new Label(car.DriverID == state.FastestLapDriverID ? "FL" : "");
                flL.AddToClassList("result-fl"); row.Add(flL);

                _resultsList.Add(row);
            }

            // Constructor points summary
            _constructorPoints.Clear();
            var teamPts = new Dictionary<int, int>();
            foreach (var car in sorted)
            {
                int pts = ptsMap(car.Position);
                if (flEligible && car.DriverID == state.FastestLapDriverID) pts++;
                if (!teamPts.ContainsKey(car.TeamID)) teamPts[car.TeamID] = 0;
                teamPts[car.TeamID] += pts;
            }
            foreach (var kv in teamPts.OrderByDescending(x => x.Value))
            {
                teams.TryGetValue(kv.Key, out var t);
                var entry = new Label($"{t?.shortName ?? "?"}: {kv.Value} pts");
                entry.style.fontSize = 12;
                entry.style.color = Color.gray;
                entry.style.marginRight = 16;
                _constructorPoints.Add(entry);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string GetRaceLabel(TireCompound compound, List<TireCompound> available)
        {
            var dry = available.Where(c => c.IsDry()).OrderBy(c => (int)c).ToList();
            if (!dry.Contains(compound)) return compound.DisplayName();
            int idx = dry.IndexOf(compound);
            return idx == 0 ? "H" : idx == 1 ? "M" : "S";
        }

        private struct StratHint { public string Label; public int Stops; }

        private static List<StratHint> ComputeStrategyHints(List<TireCompound> available, CircuitInfo circuit)
        {
            var hints = new List<StratHint>();
            var dry = available.Where(c => c.IsDry()).OrderBy(c => (int)c).ToList();
            if (dry.Count < 2) return hints;

            float deg = circuit.tireDegMultiplier;
            int laps  = circuit.totalLaps;

            // 1-stop hints
            for (int i = 0; i < dry.Count; i++)
            for (int j = 0; j < dry.Count; j++)
            {
                if (i == j) continue;
                if (!TireProfiles.All.TryGetValue(dry[i], out var p1)) continue;
                if (!TireProfiles.All.TryGetValue(dry[j], out var p2)) continue;
                int cliff1 = Math.Max(3,(int)(p1.PlateauLaps/deg)) + Math.Max(3,(int)(p1.LinearPhaseLaps/deg));
                int pit = Math.Min(cliff1 - 2, (int)(laps * 0.45f));
                int end2 = laps - pit;
                int cliff2 = Math.Max(3,(int)(p2.PlateauLaps/deg)) + Math.Max(3,(int)(p2.LinearPhaseLaps/deg));
                if (cliff2 < end2) continue;
                string s1l = GetDryLabel(dry[i], dry); string s2l = GetDryLabel(dry[j], dry);
                hints.Add(new StratHint { Label = $"{s1l}→{s2l}  box ~L{pit}", Stops = 1 });
            }

            // 2-stop: H→M→S or S→M→H style
            if (dry.Count >= 2)
            {
                string labels = string.Join("→", dry.Select(c => GetDryLabel(c, dry)));
                hints.Add(new StratHint { Label = $"{labels}  2-stop aggressive", Stops = 2 });
            }

            return hints.Take(4).ToList();
        }

        private static string GetDryLabel(TireCompound c, List<TireCompound> dry)
        {
            int idx = dry.IndexOf(c);
            return idx == 0 ? "H" : idx == 1 ? "M" : "S";
        }
    }
}
