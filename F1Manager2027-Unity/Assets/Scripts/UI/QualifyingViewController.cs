// QualifyingViewController.cs — Interactive Q1/Q2/Q3 qualifying results viewer.
// Binds QualifyingResult data to the qualifying-screen in MainMenu.uxml.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace F1Manager
{
    public class QualifyingViewController
    {
        private readonly VisualElement _root;
        private readonly GameManager   _gm;

        private VisualElement _screen;
        private Label         _sessionTitle;
        private Label         _sessionDesc;
        private Button        _tabQ1, _tabQ2, _tabQ3;
        private ScrollView    _tower;
        private Label         _poleNotice;
        private Button        _nextBtn;

        private QualifyingResult _result;
        private int _currentPart = 1;

        public QualifyingViewController(VisualElement root, GameManager gm)
        {
            _root = root;
            _gm   = gm;

            _screen       = root.Q<VisualElement>("qualifying-screen");
            _sessionTitle = root.Q<Label>("qual-session-title");
            _sessionDesc  = root.Q<Label>("qual-session-desc");
            _tabQ1        = root.Q<Button>("qual-tab-q1");
            _tabQ2        = root.Q<Button>("qual-tab-q2");
            _tabQ3        = root.Q<Button>("qual-tab-q3");
            _tower        = root.Q<ScrollView>("qual-timing-tower");
            _poleNotice   = root.Q<Label>("qual-pole-notice");
            _nextBtn      = root.Q<Button>("qual-next-btn");

            _tabQ1.clicked += () => ShowSession(1);
            _tabQ2.clicked += () => ShowSession(2);
            _tabQ3.clicked += () => ShowSession(3);
            _nextBtn.clicked += OnNextClicked;
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void Show(QualifyingResult result)
        {
            _result      = result;
            _currentPart = 1;

            _screen.RemoveFromClassList("hidden-screen");
            _screen.AddToClassList("active-screen");

            ShowSession(1);
        }

        public void Hide()
        {
            _screen.RemoveFromClassList("active-screen");
            _screen.AddToClassList("hidden-screen");
        }

        // ─────────────────────────────────────────────────────────────────────

        private void ShowSession(int part)
        {
            _currentPart = part;
            UpdateTabs();

            switch (part)
            {
                case 1:
                    _sessionTitle.text = "Q1  —  QUALIFYING";
                    _sessionDesc.text  = "20 drivers · bottom 5 eliminated";
                    break;
                case 2:
                    _sessionTitle.text = "Q2  —  QUALIFYING";
                    _sessionDesc.text  = "15 drivers · bottom 5 eliminated  ·  Q2 tyre rule applies";
                    break;
                case 3:
                    _sessionTitle.text = "Q3  —  QUALIFYING";
                    _sessionDesc.text  = "Top-10 shootout  ·  sets grid P1–P10";
                    break;
            }

            _nextBtn.text = part < 3
                ? (part == 1 ? "Q2  →" : "Q3  →")
                : "STRATEGY  ▶";

            BuildTimingTower(part);
        }

        private void UpdateTabs()
        {
            SetTab(_tabQ1, 1);
            SetTab(_tabQ2, 2);
            SetTab(_tabQ3, 3);
        }

        private void SetTab(Button btn, int part)
        {
            btn.RemoveFromClassList("qual-tab-active");
            btn.RemoveFromClassList("qual-tab-inactive");
            btn.AddToClassList(_currentPart == part ? "qual-tab-active" : "qual-tab-inactive");
        }

        private void BuildTimingTower(int part)
        {
            _tower.Clear();

            var session = _result.Sessions[part - 1];
            if (session == null) return;

            var laps = session.BestLaps();
            var eliminatedSet = new HashSet<int>(session.EliminatedDriverIDs);

            // Elimination cut line position (0 = no cut, e.g. Q3)
            int cutAfter = part == 1 ? 15 : (part == 2 ? 10 : 0);

            float poleTime = laps.Count > 0 ? laps[0].LapTimeS : 0f;

            for (int i = 0; i < laps.Count; i++)
            {
                var lap    = laps[i];
                int pos    = i + 1;
                bool isElim   = eliminatedSet.Contains(lap.DriverID);
                bool isPole   = part == 3 && i == 0;

                var driver = GameDataFactory.GetDriver(lap.DriverID);
                var team   = driver != null ? GameDataFactory.GetTeam(driver.teamID) : null;
                bool isPlayer = team != null && team.id == _gm.SelectedTeam.id;

                // Row element
                var row = new VisualElement();
                row.AddToClassList("qual-row");
                if (isPlayer) row.AddToClassList("qual-row-player");
                if (isElim)   row.AddToClassList("qual-row-elim");
                if (isPole)   row.AddToClassList("qual-row-pole");

                // Position number
                var posLbl = new Label(pos.ToString("D2"));
                posLbl.AddToClassList("qual-col-pos");
                row.Add(posLbl);

                // Team colour bar
                var bar = new VisualElement();
                bar.AddToClassList("qual-col-bar");
                if (team != null) bar.style.backgroundColor = HexColor(team.colorHex);
                row.Add(bar);

                // Driver name
                string name = driver?.driverName ?? $"DRV {lap.DriverID}";
                var nameLbl = new Label(Trunc(name, 16).ToUpper());
                nameLbl.AddToClassList("qual-col-name");
                row.Add(nameLbl);

                // Team abbreviation
                string teamAbbr = team != null ? Trunc(team.shortName ?? team.teamName, 6).ToUpper() : "---";
                var teamLbl = new Label(teamAbbr);
                teamLbl.AddToClassList("qual-col-team");
                if (team != null) teamLbl.style.color = HexColor(team.colorHex);
                row.Add(teamLbl);

                // Lap time
                var timeLbl = new Label(FormatTime(lap.LapTimeS));
                timeLbl.AddToClassList("qual-col-time");
                if (isPole) timeLbl.AddToClassList("qual-time-pole");
                row.Add(timeLbl);

                // Gap to P1
                string gapStr = i == 0 ? "POLE" : $"+{(lap.LapTimeS - poleTime):F3}";
                var gapLbl = new Label(gapStr);
                gapLbl.AddToClassList("qual-col-gap");
                row.Add(gapLbl);

                // Compound
                var cmpdLbl = new Label(lap.Compound.DisplayName());
                cmpdLbl.AddToClassList("qual-col-cmpd");
                cmpdLbl.style.color = lap.Compound.Color();
                row.Add(cmpdLbl);

                // Status badge
                string badgeText;
                string badgeClass;
                if (isPole)       { badgeText = "POLE"; badgeClass = "qual-badge-pole"; }
                else if (isElim)  { badgeText = "ELIM"; badgeClass = "qual-badge-elim"; }
                else if (cutAfter == 0) { badgeText = "P" + pos; badgeClass = "qual-badge-q3"; }
                else              { badgeText = "ADV";  badgeClass = "qual-badge-adv";  }

                var badge = new Label(badgeText);
                badge.AddToClassList("qual-badge");
                badge.AddToClassList(badgeClass);
                row.Add(badge);

                _tower.Add(row);

                // Dashed separator after cut line
                if (cutAfter > 0 && i == cutAfter - 1)
                {
                    var sep = new VisualElement();
                    sep.AddToClassList("qual-elim-line");
                    _tower.Add(sep);
                }
            }

            // Q2 compound rule footer
            if (part == 2 && _result.Q2CompoundMap.Count > 0)
            {
                _poleNotice.text = "★  Top-10 must start the race on their Q2 compound.";
                _poleNotice.style.display = DisplayStyle.Flex;
            }
            else if (part == 3 && _result.PoleSitterID >= 0)
            {
                var pd = GameDataFactory.GetDriver(_result.PoleSitterID);
                string pnm = pd?.driverName ?? $"#{_result.PoleSitterID}";
                _poleNotice.text = $"POLE  ·  {pnm.ToUpper()}  ·  {FormatTime(_result.PoleTimeS)}";
                _poleNotice.style.display = DisplayStyle.Flex;
            }
            else
            {
                _poleNotice.style.display = DisplayStyle.None;
            }
        }

        private void OnNextClicked()
        {
            if (_currentPart < 3)
                ShowSession(_currentPart + 1);
            else
            {
                Hide();
                _gm.OnQualifyingComplete(_result);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string FormatTime(float s)
        {
            int m = (int)(s / 60f);
            float rem = s - m * 60f;
            return $"{m}:{rem:00.000}";
        }

        private static string Trunc(string s, int max)
            => s.Length > max ? s.Substring(0, max) : s;

        private static Color HexColor(string hex)
        {
            hex = hex.TrimStart('#');
            if (hex.Length >= 6)
            {
                float r = Convert.ToInt32(hex.Substring(0, 2), 16) / 255f;
                float g = Convert.ToInt32(hex.Substring(2, 2), 16) / 255f;
                float b = Convert.ToInt32(hex.Substring(4, 2), 16) / 255f;
                return new Color(r, g, b);
            }
            return Color.white;
        }
    }
}
