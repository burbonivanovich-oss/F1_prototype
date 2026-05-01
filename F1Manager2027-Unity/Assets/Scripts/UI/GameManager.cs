using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace F1Manager
{
    /// <summary>
    /// Central game flow controller. Manages transitions between screens and
    /// holds shared session state (selected team, circuit, qualifying result).
    /// Attach to a GameObject with a UIDocument component (MainMenu.uxml + RaceScreen.uxml).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ── Inspector refs ────────────────────────────────────────────────────
        [Header("UI Documents")]
        [SerializeField] private UIDocument menuDocument;
        [SerializeField] private UIDocument raceDocument;

        [Header("Optional asset database (set in Editor)")]
        [SerializeField] private GameDatabase gameDatabase;

        // ── Sub-controllers ───────────────────────────────────────────────────
        private MenuViewController  _menuVC;
        private RaceViewController  _raceVC;

        // ── Session state ─────────────────────────────────────────────────────
        public TeamInfo    SelectedTeam    { get; private set; }
        public CircuitInfo SelectedCircuit { get; private set; }
        public QualifyingResult QualifyingResult { get; private set; }
        public RaceEngine  ActiveRaceEngine { get; private set; }

        private System.Random _rng;

        // ── Singleton ─────────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ─────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            GameDataFactory.Initialize();
            _rng = new System.Random();
        }

        private void Start()
        {
            _menuVC = new MenuViewController(menuDocument.rootVisualElement, this);
            _raceVC = new RaceViewController(raceDocument.rootVisualElement, this);

            raceDocument.rootVisualElement.style.display = DisplayStyle.None;
            menuDocument.rootVisualElement.style.display = DisplayStyle.Flex;

            _menuVC.ShowTeamSelect();
        }

        // ── Navigation ────────────────────────────────────────────────────────

        public void OnTeamSelected(TeamInfo team)
        {
            SelectedTeam = team;
            _menuVC.ShowCircuitSelect(team);
        }

        public void OnCircuitSelected(CircuitInfo circuit)
        {
            SelectedCircuit = circuit;
            RunQualifying();
        }

        private void RunQualifying()
        {
            var allDrivers = new Dictionary<int, DriverInfo>();
            foreach (var d in GameDataFactory.GetAllDrivers())
                allDrivers[d.id] = d;

            var allTeams = new Dictionary<int, TeamInfo>();
            foreach (var t in GameDataFactory.Teams)
                allTeams[t.id] = t;

            var availableCompounds = GetAvailableCompounds(SelectedCircuit);
            var qEngine = new QualifyingEngine(SelectedCircuit, allDrivers, allTeams, _rng);
            QualifyingResult = qEngine.RunQualifying(availableCompounds, SelectedTeam.id);

            _menuVC.ShowStrategyScreen(SelectedTeam, SelectedCircuit, QualifyingResult);
        }

        public void OnStrategyConfirmed(Dictionary<int, StrategyOverride> overrides)
        {
            var allDrivers = new Dictionary<int, DriverInfo>();
            foreach (var d in GameDataFactory.GetAllDrivers())
                allDrivers[d.id] = d;

            var allTeams = new Dictionary<int, TeamInfo>();
            foreach (var t in GameDataFactory.Teams)
                allTeams[t.id] = t;

            ActiveRaceEngine = new RaceEngine(
                SelectedCircuit, allTeams, allDrivers,
                SelectedTeam.id, _rng, QualifyingResult
            );

            // Apply fuel overrides
            foreach (var car in ActiveRaceEngine.RaceState.Cars)
            {
                if (overrides.TryGetValue(car.DriverID, out var ov))
                {
                    car.FuelKg = ov.FuelKg;
                    if (ov.StartingCompound.HasValue)
                        car.TireCompound = ov.StartingCompound.Value;
                }
            }

            // Switch to race screen
            menuDocument.rootVisualElement.style.display = DisplayStyle.None;
            raceDocument.rootVisualElement.style.display = DisplayStyle.Flex;

            _raceVC.StartRace(ActiveRaceEngine);
        }

        public void OnRaceComplete()
        {
            raceDocument.rootVisualElement.style.display = DisplayStyle.None;
            menuDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            _menuVC.ShowResults(ActiveRaceEngine.RaceState);
        }

        public void OnNewRace()
        {
            ActiveRaceEngine = null;
            QualifyingResult = null;
            _menuVC.ShowTeamSelect();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        public List<TireCompound> GetAvailableCompounds(CircuitInfo circuit)
        {
            // Circuit-specific compound allocation (authentic F1 rules)
            string name = circuit.circuitName;
            if (name.Contains("Italian") || name.Contains("Monza") || name.Contains("Belgian"))
                return new List<TireCompound> { TireCompound.C2, TireCompound.C3, TireCompound.C4 };
            if (name.Contains("Monaco") || name.Contains("Singapore") || name.Contains("Hungarian"))
                return new List<TireCompound> { TireCompound.C4, TireCompound.C5, TireCompound.C6 };
            if (name.Contains("Azerbaijan") || name.Contains("Las Vegas") || name.Contains("Saudi"))
                return new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 };
            // Default (most circuits)
            return new List<TireCompound> { TireCompound.C3, TireCompound.C4, TireCompound.C5 };
        }

        public List<DriverInfo> GetTeamDrivers(int teamId)
            => GameDataFactory.GetTeamDrivers(teamId);
    }

    /// <summary>Player strategy override for pre-race screen.</summary>
    public class StrategyOverride
    {
        public float FuelKg = 105f;
        public TireCompound? StartingCompound;
    }
}
