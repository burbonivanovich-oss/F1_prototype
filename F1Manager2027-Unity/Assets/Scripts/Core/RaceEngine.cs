// RaceEngine.cs — Main race simulation engine for F1Manager.
// Advances the race by one lap per SimulateLap() call, producing a full RaceState snapshot.
// Pure C#, no Unity dependencies.  Mirrors Python core/engine.py.
using System;
using System.Collections.Generic;
using System.Linq;

namespace F1Manager
{
    // ─────────────────────────────────────────────────────────────────────────
    // RaceEngine
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Central race simulation object. Create once per race then call
    /// <see cref="SimulateLap"/> repeatedly until <c>RaceState.IsRaceComplete</c>.
    ///
    /// Player commands are queued via <see cref="CommandPit"/>,
    /// <see cref="CommandInstruction"/>, and <see cref="CommandTeamOrder"/>;
    /// they are consumed during the next <see cref="SimulateLap"/> call.
    /// </summary>
    public class RaceEngine
    {
        // ── Configuration ─────────────────────────────────────────────────────
        private readonly CircuitInfo                  _circuit;
        private readonly Dictionary<int, TeamInfo>    _teams;
        private readonly Dictionary<int, DriverInfo>  _drivers;
        private readonly int                          _playerTeamID;
        private readonly System.Random                _rng;

        // ── Sub-systems ───────────────────────────────────────────────────────
        private readonly WeatherSystem      _weatherSys;
        private readonly AIStrategyEngine   _ai;

        // ── Race-weekend compounds ────────────────────────────────────────────
        private readonly List<TireCompound>  _availableCompounds;
        private readonly List<TireCompound>  _availableDry;

        // ── Player command queues ─────────────────────────────────────────────
        /// <summary>Pending pit commands: driverID → desired compound (null = auto).</summary>
        private readonly Dictionary<int, TireCompound?> _playerPitCommands
            = new Dictionary<int, TireCompound?>();

        /// <summary>Pending driver instructions set by the player this lap.</summary>
        private readonly Dictionary<int, DriverInstruction> _playerInstructions
            = new Dictionary<int, DriverInstruction>();

        /// <summary>Current team-wide tactical order.</summary>
        private TeamOrder _teamOrder = TeamOrder.FREE_RACE;

        /// <summary>Engineer radio cooldown: (driverID, category) → last lap sent.</summary>
        private readonly Dictionary<(int, string), int> _radioLast
            = new Dictionary<(int, string), int>();

        // ── Public race state ─────────────────────────────────────────────────
        public RaceState RaceState { get; private set; }

        // ─────────────────────────────────────────────────────────────────────
        // Construction
        // ─────────────────────────────────────────────────────────────────────

        /// <param name="circuit">Immutable circuit data for this race weekend.</param>
        /// <param name="teams">All teams indexed by teamID.</param>
        /// <param name="drivers">All drivers indexed by driverID.</param>
        /// <param name="playerTeamID">The human player's constructor ID.</param>
        /// <param name="rng">Seeded RNG; if null a random seed is used.</param>
        /// <param name="qualifyingResult">Pre-computed qualifying result for the starting grid.</param>
        public RaceEngine(
            CircuitInfo                  circuit,
            Dictionary<int, TeamInfo>    teams,
            Dictionary<int, DriverInfo>  drivers,
            int                          playerTeamID,
            System.Random                rng              = null,
            QualifyingResult             qualifyingResult = null)
        {
            _circuit      = circuit;
            _teams        = teams;
            _drivers      = drivers;
            _playerTeamID = playerTeamID;
            _rng          = rng ?? new System.Random();

            // Resolve available compounds from circuit name/id
            _availableCompounds = _ResolveCircuitCompounds(circuit);
            _availableDry       = _availableCompounds.Where(c => c.IsDry()).ToList();

            // Sub-systems
            _weatherSys = new WeatherSystem(
                circuit,
                new System.Random(_rng.Next()),
                circuit.totalLaps);

            _ai = new AIStrategyEngine(
                circuit,
                _availableCompounds,
                new System.Random(_rng.Next()));

            // Build initial race state
            RaceState = _BuildInitialState(qualifyingResult);
        }

        // ── Compound resolution ───────────────────────────────────────────────

        private static List<TireCompound> _ResolveCircuitCompounds(CircuitInfo circuit)
        {
            // Select three dry compounds by circuit identity; rain compounds always added.
            TireCompound hard, medium, soft;
            string name = circuit.circuitName ?? string.Empty;

            if (name.Contains("Monza") || name.Contains("Italian"))
            {
                hard = TireCompound.C2; medium = TireCompound.C3; soft = TireCompound.C4;
            }
            else if (name.Contains("Monaco") || name.Contains("Hungary") || name.Contains("Singapore"))
            {
                hard = TireCompound.C4; medium = TireCompound.C5; soft = TireCompound.C6;
            }
            else
            {
                // Default: C3/C4/C5
                hard = TireCompound.C3; medium = TireCompound.C4; soft = TireCompound.C5;
            }

            return new List<TireCompound> { hard, medium, soft, TireCompound.INTER, TireCompound.WET };
        }

        // ── Initial state construction ────────────────────────────────────────

        private RaceState _BuildInitialState(QualifyingResult qualResult)
        {
            var state = new RaceState(_circuit.circuitName, _circuit.totalLaps)
            {
                Weather           = _weatherSys.Condition,
                TrackTempC        = _weatherSys.TrackTempC,
                AirTempC          = _weatherSys.AirTempC,
                PlayerTeamID      = _playerTeamID,
                AvailableCompounds = new List<TireCompound>(_availableCompounds),
            };
            state.WeatherForecast = new List<WeatherCondition>(_weatherSys.Forecast);

            // Determine grid order
            List<int> gridOrder;
            if (qualResult != null)
            {
                gridOrder = qualResult.GridOrder;
            }
            else
            {
                // Performance-based order (fastest qualifier first)
                var gridTimes = new List<(float time, int driverID)>();
                foreach (var kvp in _drivers)
                {
                    DriverInfo driver = kvp.Value;
                    if (driver.isReserve) continue;
                    if (!_teams.TryGetValue(driver.teamID, out TeamInfo team)) continue;

                    float carGap  = (100 - team.carPerformance) * 0.050f;
                    float ps      = _circuit.powerSensitivity;
                    carGap -= (team.powerUnit - 90) * ps * 0.015f
                            + (team.chassis   - 90) * (1f - ps) * 0.015f;
                    float drvGain = (driver.pace - 75) * 0.012f;
                    float noise   = (float)((_rng.NextDouble() - 0.5) * 0.16);
                    float qTime   = _circuit.baseLapTimeS + carGap - drvGain + noise;
                    gridTimes.Add((qTime, driver.id));
                }
                gridTimes.Sort((a, b) => a.time.CompareTo(b.time));
                gridOrder = gridTimes.Select(x => x.driverID).ToList();
            }

            // Build CarState for each car
            TireCompound defaultSoft = _availableDry.Count > 0
                ? _availableDry.OrderBy(c => TireProfiles.All[c].GripBonusS).First()
                : TireCompound.C5;

            for (int gridPos = 1; gridPos <= gridOrder.Count; gridPos++)
            {
                int driverID = gridOrder[gridPos - 1];
                if (!_drivers.TryGetValue(driverID, out DriverInfo drv)) continue;
                if (drv.isReserve) continue;
                if (!_teams.TryGetValue(drv.teamID, out TeamInfo _)) continue;

                // Starting compound: Q2 rule for P1-10
                TireCompound startCompound = defaultSoft;
                if (qualResult != null && gridPos <= 10)
                {
                    if (qualResult.Q2CompoundMap.TryGetValue(driverID, out TireCompound q2cmpd)
                            && _availableCompounds.Contains(q2cmpd))
                        startCompound = q2cmpd;
                }

                var car = new CarState(driverID, drv.teamID, drv.carNumber)
                {
                    Position    = gridPos,
                    Compound    = startCompound,
                    FuelKg      = 105f,
                    Instruction = DriverInstruction.MANAGE,
                };
                car.CompoundsUsed.Add(startCompound);
                // Morale modifier: driver morale 80 = neutral; ±0.2s range
                car.MoraleModifierS = -(drv.morale - 80) * 0.01f;
                state.Cars.Add(car);
            }

            return state;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Player commands
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Queue a pit-stop order for <paramref name="driverID"/>.</summary>
        /// <param name="compound">Compound to fit; null = AI auto-select.</param>
        public void CommandPit(int driverID, TireCompound? compound)
            => _playerPitCommands[driverID] = compound;

        /// <summary>Set a driver instruction for the next lap.</summary>
        public void CommandInstruction(int driverID, DriverInstruction instr)
            => _playerInstructions[driverID] = instr;

        /// <summary>Issue a team-wide tactical order.</summary>
        public void CommandTeamOrder(TeamOrder order)
        {
            _teamOrder = order;
            RaceState.Events.Add(new RaceEvent(
                RaceState.CurrentLap, "Team Principal", "INFO",
                $"Team orders: {order.DisplayName()}", isPlayerEvent: true));
        }

        // ─────────────────────────────────────────────────────────────────────
        // Main lap simulation
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Advances the race by exactly one lap for all cars.
        /// Returns the updated <see cref="F1Manager.RaceState"/> snapshot.
        /// </summary>
        public RaceState SimulateLap()
        {
            RaceState state = RaceState;
            if (state.IsRaceComplete)
                return state;

            state.CurrentLap++;
            int lap = state.CurrentLap;

            // ── 1. Lap 1 race start ───────────────────────────────────────────
            if (lap == 1)
                _SimulateRaceStart(state);

            // ── 2. Weather update ─────────────────────────────────────────────
            List<string> weatherMsgs = _weatherSys.Advance(lap);
            state.Weather         = _weatherSys.Condition;
            state.TrackTempC      = _weatherSys.TrackTempC;
            state.AirTempC        = _weatherSys.AirTempC;
            state.WeatherForecast = new List<WeatherCondition>(_weatherSys.Forecast);
            foreach (string msg in weatherMsgs)
                state.Events.Add(new RaceEvent(lap, "Weather", "WEATHER", msg));

            // ── 3. Safety car management ──────────────────────────────────────
            _UpdateSafetyCar(state, lap);

            // ── 4. Rotate pitting flags ───────────────────────────────────────
            foreach (CarState car in state.Cars)
            {
                car.PittedLastLap      = car.IsPittingThisLap;
                car.IsPittingThisLap   = false;
                car.PitStopDurationS   = 0f;
            }

            // ── 5. Apply player instructions ──────────────────────────────────
            foreach (var kvp in _playerInstructions)
            {
                foreach (CarState car in state.Cars)
                {
                    if (car.DriverID == kvp.Key && !car.DNF)
                        car.Instruction = kvp.Value;
                }
            }

            // ── 6. Apply team orders ──────────────────────────────────────────
            _ApplyTeamOrders(state);

            // ── 7. Execute pit stops (player + AI) ───────────────────────────
            var pitResults = _ExecutePitStops(state, lap);

            // ── 8. Simulate lap times ─────────────────────────────────────────
            _SimulateLapTimes(state, lap);

            // ── 9. Resolve overtakes ──────────────────────────────────────────
            _UpdateGaps(state);
            _ResolveOvertakes(state, lap);

            // ── 10. Check DNFs ────────────────────────────────────────────────
            _CheckDNF(state, lap);

            // ── 11. Update positions and gaps ─────────────────────────────────
            _UpdatePositions(state);
            _UpdateGaps(state);

            // Record gap history for trend arrows
            foreach (CarState car in state.Cars)
            {
                if (!car.DNF)
                    car.GapHistory.Add(car.GapToLeaderS);
            }

            // ── 12. Update tyre phases ────────────────────────────────────────
            _UpdateTirePhases(state);

            // ── 13. Engineer radio messages for player cars ───────────────────
            _EngineerRadio(state, lap);

            // ── 14. Emit pit-stop events ──────────────────────────────────────
            foreach (var pit in pitResults)
            {
                if (!_drivers.TryGetValue(pit.DriverID, out DriverInfo drv)) continue;
                bool isPlayer = _drivers[pit.DriverID].teamID == _playerTeamID;
                string msg = $"{drv.shortName} pits -> {pit.NewCompound.DisplayName()} "
                           + $"({pit.DurationS:F2}s stop).";
                state.Events.Add(new RaceEvent(lap, drv.driverName, "PIT", msg, isPlayerEvent: isPlayer));
            }

            // ── 15. Update AI instructions ────────────────────────────────────
            _UpdateAIInstructions(state);

            // ── 16. Probabilistic safety car trigger ──────────────────────────
            _MaybeTriggerSafetyCar(state, lap);

            // ── 17. Race completion check ─────────────────────────────────────
            var activeCars = state.Cars.Where(c => !c.DNF).ToList();
            if (activeCars.Count > 0 && activeCars.All(c => c.LapsCompleted >= state.TotalLaps))
            {
                state.IsRaceComplete = true;
                _FinalizeRace(state);
            }

            // ── 18. Clear consumed commands ───────────────────────────────────
            _playerPitCommands.Clear();

            return state;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Race start
        // ─────────────────────────────────────────────────────────────────────

        private void _SimulateRaceStart(RaceState state)
        {
            var gridOrder = state.SortedCars();

            // Store qualifying positions
            var gridPositions = gridOrder.ToDictionary(c => c.DriverID, c => c.Position);

            // Small staggered starting offsets (200 m grid spread ≈ 0.15 s per slot)
            foreach (CarState car in gridOrder)
                car.TotalRaceTimeS = (car.Position - 1) * 0.15f;

            // Compute start score: racecraft + experience + gaussian noise
            var startScores = new List<(float score, CarState car, int originalPos)>();
            foreach (CarState car in gridOrder)
            {
                if (!_drivers.TryGetValue(car.DriverID, out DriverInfo drv)) continue;
                float score = drv.racecraft * 0.45f
                            + Math.Min(drv.experience, 15) * 1.8f
                            + _NextGaussian(0f, 6f);
                startScores.Add((score, car, car.Position));
            }

            // Sort by score descending (higher score = better start)
            startScores.Sort((a, b) => b.score.CompareTo(a.score));

            // Assign new time offsets, clamped to ±3 positions from grid
            for (int newRank = 1; newRank <= startScores.Count; newRank++)
            {
                var (_, car, originalPos) = startScores[newRank - 1];
                int clampedRank = Math.Max(originalPos - 3, Math.Min(originalPos + 3, newRank));
                car.TotalRaceTimeS = (clampedRank - 1) * 0.15f + (float)(_rng.NextDouble() * 0.03);
            }

            _UpdatePositions(state);
            _UpdateGaps(state);

            // Hard-clamp cascading effects: re-check position deltas
            bool needsRecheck = false;
            foreach (CarState car in state.Cars)
            {
                int oldQ = gridPositions.GetValueOrDefault(car.DriverID, car.Position);
                if (Math.Abs(car.Position - oldQ) > 3)
                {
                    car.TotalRaceTimeS = (oldQ - 1 + (float)(_rng.NextDouble() * 4 - 2)) * 0.15f;
                    needsRecheck = true;
                }
            }
            if (needsRecheck)
            {
                _UpdatePositions(state);
                _UpdateGaps(state);
            }

            // Log notable starts (≥3 position change)
            foreach (CarState car in state.SortedCars())
            {
                if (!_drivers.TryGetValue(car.DriverID, out DriverInfo drv)) continue;
                int oldQ  = gridPositions.GetValueOrDefault(car.DriverID, car.Position);
                int delta = oldQ - car.Position;
                bool isPlayer = car.TeamID == _playerTeamID;

                if (delta >= 3)
                    state.Events.Add(new RaceEvent(1, drv.driverName, "INFO",
                        $"{drv.shortName} rocket start! +{delta} (P{oldQ}->P{car.Position})",
                        isPlayerEvent: isPlayer));
                else if (delta <= -3)
                    state.Events.Add(new RaceEvent(1, drv.driverName, "INFO",
                        $"{drv.shortName} poor start: P{oldQ}->P{car.Position}",
                        isPlayerEvent: isPlayer));
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Team orders
        // ─────────────────────────────────────────────────────────────────────

        private void _ApplyTeamOrders(RaceState state)
        {
            if (_teamOrder == TeamOrder.FREE_RACE)
                return;

            var playerCars = state.Cars
                .Where(c => c.TeamID == _playerTeamID && !c.DNF)
                .OrderBy(c => c.Position)
                .ToList();

            if (playerCars.Count < 2)
                return;

            CarState leadCar   = playerCars[0];
            CarState followCar = playerCars[1];

            switch (_teamOrder)
            {
                case TeamOrder.HOLD_GAP:
                    if (followCar.GapToAheadS < 1.5f)
                        followCar.Instruction = DriverInstruction.MANAGE;
                    break;

                case TeamOrder.SWAP_DRIVERS:
                    if (followCar.GapToAheadS < 0.5f
                            && leadCar.TireAgeLaps > followCar.TireAgeLaps + 3)
                    {
                        // Execute the swap
                        float tmp = leadCar.TotalRaceTimeS;
                        leadCar.TotalRaceTimeS   = followCar.TotalRaceTimeS - 0.1f;
                        followCar.TotalRaceTimeS = tmp + 0.1f;

                        if (_drivers.TryGetValue(leadCar.DriverID,   out DriverInfo d1) &&
                            _drivers.TryGetValue(followCar.DriverID, out DriverInfo d2))
                        {
                            state.Events.Add(new RaceEvent(
                                state.CurrentLap, "Team Principal", "INFO",
                                $"{d2.shortName} passes {d1.shortName} — team order executed.",
                                isPlayerEvent: true));
                        }
                        _teamOrder = TeamOrder.FREE_RACE;
                    }
                    break;

                case TeamOrder.PUSH_BOTH:
                    foreach (var car in playerCars)
                        car.Instruction = DriverInstruction.ATTACK;
                    break;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Pit stops
        // ─────────────────────────────────────────────────────────────────────

        private List<PitStopResult> _ExecutePitStops(RaceState state, int lap)
        {
            var results = new List<PitStopResult>();

            foreach (CarState car in state.Cars)
            {
                if (car.DNF || car.LapsCompleted >= state.TotalLaps)
                    continue;

                if (!_drivers.TryGetValue(car.DriverID, out DriverInfo drv)) continue;
                if (!_teams.TryGetValue(car.TeamID,     out TeamInfo   team)) continue;

                bool          isPlayer     = team.id == _playerTeamID;
                bool          shouldPit    = false;
                TireCompound? chosenCmpd   = null;

                if (isPlayer && _playerPitCommands.TryGetValue(car.DriverID, out TireCompound? playerCmpd))
                {
                    shouldPit  = true;
                    chosenCmpd = playerCmpd;
                }
                else if (!isPlayer)
                {
                    shouldPit = _ai.ShouldPit(car, state, drv);
                }

                if (!shouldPit)
                    continue;

                // Double-stacking delay
                float extraDelay = 0f;
                bool  teammatePitting = state.Cars.Any(c =>
                    c.TeamID == car.TeamID
                    && c.DriverID != car.DriverID
                    && c.IsPittingThisLap);
                if (teammatePitting)
                    extraDelay = (float)(2.8 + _rng.NextDouble() * 1.2);

                // Choose compound
                if (chosenCmpd == null)
                    chosenCmpd = _ai.ChooseCompound(car, state);

                // Stop duration
                float pitDur = _ai.PitStopDurationS(team.pitCrewSkill, _rng) + extraDelay;

                // Fuel
                float fuelAdd = _ai.FuelToAddKg(car, state);

                TireCompound oldCmpd = car.Compound;

                // Apply pit stop
                car.IsPittingThisLap  = true;
                car.PitStopDurationS  = pitDur;
                car.Compound          = chosenCmpd.Value;
                car.TireAgeLaps       = 0;
                car.PitStopCount++;
                car.FuelKg            = Math.Min(110f, car.FuelKg + fuelAdd);
                car.CompoundsUsed.Add(chosenCmpd.Value);

                results.Add(new PitStopResult(
                    driverID:    car.DriverID,
                    lap:         lap,
                    oldCompound: oldCmpd,
                    newCompound: chosenCmpd.Value,
                    durationS:   pitDur));
            }

            return results;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Lap time simulation
        // ─────────────────────────────────────────────────────────────────────

        private void _SimulateLapTimes(RaceState state, int lap)
        {
            var sortedCars = state.SortedCars();

            for (int i = 0; i < sortedCars.Count; i++)
            {
                CarState car = sortedCars[i];
                if (car.DNF)
                    continue;

                car.TireAgeLaps++;

                if (!_drivers.TryGetValue(car.DriverID, out DriverInfo drv)) continue;
                if (!_teams.TryGetValue(car.TeamID,     out TeamInfo   team)) continue;

                // ── Safety Car / VSC: all cars at controlled pace ─────────────
                if (state.SafetyCar == SafetyCarState.DEPLOYED ||
                    state.SafetyCar == SafetyCarState.VSC)
                {
                    float scMult = state.SafetyCar == SafetyCarState.DEPLOYED ? 1.25f : 1.30f;
                    float scTime = _circuit.baseLapTimeS * scMult;

                    // Field compression under full SC
                    if (state.SafetyCar == SafetyCarState.DEPLOYED && car.GapToLeaderS > 1.0f)
                        car.TotalRaceTimeS -= car.GapToLeaderS * 0.18f;

                    float noise = (float)((_rng.NextDouble() - 0.5) * 0.2);
                    car.LastLapTimeS    = scTime + noise;
                    car.TotalRaceTimeS += car.LastLapTimeS;
                    car.LapsCompleted++;

                    // Fuel still consumed
                    car.FuelKg = Math.Max(0.1f, car.FuelKg - _circuit.fuelConsumptionKg);
                    continue;
                }

                // ── Normal lap time calculation ───────────────────────────────
                float lapTime = _ComputeLapTime(car, drv, team, state, i, sortedCars);
                car.LastLapTimeS    = lapTime;
                car.TotalRaceTimeS += lapTime;
                car.LapsCompleted++;

                // Fuel consumption (instruction-adjusted)
                float fuelMult = car.Instruction == DriverInstruction.FUEL_SAVE ? 0.88f
                               : car.Instruction == DriverInstruction.ATTACK    ? 1.05f
                               : 1.0f;
                car.FuelKg = Math.Max(0.1f, car.FuelKg - _circuit.fuelConsumptionKg * fuelMult);

                // Record lap history
                car.LapTimes.Add(lapTime);
                if (car.BestLapTimeS <= 0f || lapTime < car.BestLapTimeS)
                    car.BestLapTimeS = lapTime;

                // Sector times: proportional split with independent noise
                float[] s = _circuit.sectorSplits;
                if (s == null || s.Length < 3)
                    s = new float[] { 0.33f, 0.34f, 0.33f };
                float n1 = _NextGaussian(0f, 0.06f);
                float n2 = _NextGaussian(0f, 0.06f);
                float s1 = lapTime * s[0] + n1;
                float s2 = lapTime * s[1] + n2;
                float s3 = Math.Max(0.1f, lapTime - s1 - s2);
                car.LastSectorTimes[0] = s1;
                car.LastSectorTimes[1] = s2;
                car.LastSectorTimes[2] = s3;
                if (s1 < car.BestSectorTimes[0]) car.BestSectorTimes[0] = s1;
                if (s2 < car.BestSectorTimes[1]) car.BestSectorTimes[1] = s2;
                if (s3 < car.BestSectorTimes[2]) car.BestSectorTimes[2] = s3;

                // Track fastest lap
                if (lapTime < state.FastestLapTimeS)
                {
                    state.FastestLapTimeS    = lapTime;
                    state.FastestLapDriverID = car.DriverID;
                }
            }
        }

        private float _ComputeLapTime(
            CarState   car,
            DriverInfo drv,
            TeamInfo   team,
            RaceState  state,
            int        positionIndex,
            List<CarState> sortedCars)
        {
            float base_t = _circuit.baseLapTimeS;

            // ── Car performance gap ───────────────────────────────────────────
            float carGap = (100 - team.carPerformance) * 0.050f;
            float ps     = _circuit.powerSensitivity;
            carGap -= (team.powerUnit - 90) * ps * 0.015f
                    + (team.chassis   - 90) * (1f - ps) * 0.015f;
            base_t += carGap;

            // ── Fuel weight penalty ───────────────────────────────────────────
            float fuelPenalty = car.FuelKg * 0.003f;
            base_t += fuelPenalty;

            // ── Tyre degradation + warm-up ────────────────────────────────────
            if (TireProfiles.All.TryGetValue(car.Compound, out TireProfile profile))
            {
                float degMult = _circuit.tireDegMultiplier;

                float tireDegPen = car.IsPittingThisLap
                    ? TireSystem.DegPenaltyS(profile, Math.Max(1, car.TireAgeLaps - 1),
                                             degMult, drv.tireManagement) * 0.5f
                    : TireSystem.DegPenaltyS(profile, car.TireAgeLaps,
                                             degMult, drv.tireManagement);

                float warmUpPen = TireSystem.WarmUpPenaltyS(profile, car.TireAgeLaps, state.TrackTempC);

                base_t += tireDegPen + warmUpPen;

                // Compound grip vs C4 baseline (negative = faster for softs)
                if (!car.IsPittingThisLap)
                    base_t += profile.GripBonusS;
            }

            // ── Driver instruction modifier ───────────────────────────────────
            switch (car.Instruction)
            {
                case DriverInstruction.ATTACK:    base_t -= 0.25f; break;
                case DriverInstruction.MANAGE:    base_t += 0.00f; break;
                case DriverInstruction.DEFEND:    base_t += 0.05f; break;
                case DriverInstruction.FUEL_SAVE: base_t -= 0.10f; break;
            }

            // ── Weather penalty + wet-skill modifier ──────────────────────────
            float weatherPen = _weatherSys.LapTimeWeatherPenaltyS(car.Compound);
            if (weatherPen > 0f)
            {
                // wet_skill 85 = neutral; each point above reduces penalty 0.75%
                float wetMod = weatherPen * (drv.wetSkill - 85) * -0.0075f;
                weatherPen += wetMod;
                weatherPen  = Math.Max(0f, weatherPen);
            }
            base_t += weatherPen;

            // ── Dirty air from following closely ─────────────────────────────
            if (positionIndex > 0)
                base_t += _DirtyAirPenaltyS(car.GapToAheadS);

            // ── Morale momentum ───────────────────────────────────────────────
            base_t += car.MoraleModifierS;

            // ── Gaussian noise: consistency reduces variance ──────────────────
            // consistency 60-100 → sigma 0.12 → ~0.02
            float consistency = drv.pace; // no consistency field — use pace as proxy
            // Use driver.tireManagement as a stand-in for consistency if no field exists
            // GDD sigma formula: 0.12 * (1 - (consistency - 60) / 80)
            float sigma = 0.12f * (1f - (drv.tireManagement - 60f) / 80f);
            sigma = Math.Max(0.02f, sigma);
            base_t += _NextGaussian(0f, sigma);

            // ── Pit stop time addition ────────────────────────────────────────
            if (car.IsPittingThisLap)
                base_t += car.PitStopDurationS + 20.0f; // 20 s pit-lane speed-limiter loss

            // Physical floor: cannot be faster than 90% of base lap time
            return Math.Max(base_t, _circuit.baseLapTimeS * 0.90f);
        }

        // ── Dirty air helper (pure C# equivalent of OvertakeSystem) ───────────

        private float _DirtyAirPenaltyS(float gapAhead)
        {
            if (gapAhead <= 0f || gapAhead > 2f)
                return 0f;

            float dfMult;
            if      (_circuit.overtakeDifficulty > 0.7f) dfMult = 1.3f;
            else if (_circuit.overtakeDifficulty < 0.2f) dfMult = 0.6f;
            else                                          dfMult = 1.0f;

            float basePen;
            if      (gapAhead <= 0.5f) basePen = 0.30f;
            else if (gapAhead <= 1.0f) basePen = 0.20f;
            else                       basePen = 0.10f;

            return basePen * dfMult;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Overtake resolution
        // ─────────────────────────────────────────────────────────────────────

        private void _ResolveOvertakes(RaceState state, int lap)
        {
            var sortedCars = state.SortedCars();

            for (int i = 1; i < sortedCars.Count; i++)
            {
                CarState attacker = sortedCars[i];
                CarState defender = sortedCars[i - 1];

                if (attacker.DNF || defender.DNF)                           continue;
                if (attacker.IsPittingThisLap || defender.IsPittingThisLap) continue;
                if (attacker.GapToAheadS <= 0f || attacker.GapToAheadS > 1.2f) continue;
                if (attacker.LapsCompleted != defender.LapsCompleted)       continue;

                if (!_drivers.TryGetValue(attacker.DriverID, out DriverInfo atkDrv)) continue;
                if (!_drivers.TryGetValue(defender.DriverID, out DriverInfo defDrv)) continue;

                // Pace delta (positive = attacker faster)
                float paceD = defender.LastLapTimeS - attacker.LastLapTimeS;
                paceD = Math.Max(-3f, Math.Min(3f, paceD));

                bool drsOn = _circuit.drsZones > 0 && lap >= 2
                          && attacker.GapToAheadS > 0f && attacker.GapToAheadS <= 1.0f;
                float minPace = drsOn ? 0.05f : 0.15f;
                if (paceD < minPace) continue;

                // Sigmoid probability
                float skillDelta  = atkDrv.racecraft - defDrv.defending;
                float tireDelta   = attacker.TireAgeLaps - defender.TireAgeLaps;
                float defAgg      = _DefenderAggression(defender, defDrv);
                float trackDiff   = _circuit.overtakeDifficulty;

                double logit =
                      0.04f * skillDelta
                    + 1.80f * paceD
                    + 0.05f * (-tireDelta)
                    + 1.50f * (drsOn ? 1f : 0f)
                    - 3.00f * trackDiff
                    - 2.00f * defAgg;

                float pRaw    = (float)(1.0 / (1.0 + Math.Exp(-logit)));
                float pSuccess = Math.Min(pRaw, 0.85f);
                bool  success  = _rng.NextDouble() < pSuccess;

                if (success)
                {
                    // Swap total race times
                    float attTime = attacker.TotalRaceTimeS;
                    float defTime = defender.TotalRaceTimeS;
                    attacker.TotalRaceTimeS = defTime - 0.05f;
                    defender.TotalRaceTimeS = Math.Max(defTime, attTime + 0.01f);

                    // Momentum: overtake win → attacker boost, defender setback
                    attacker.MoraleModifierS = Math.Max(-0.30f, attacker.MoraleModifierS - 0.10f);
                    defender.MoraleModifierS = Math.Min( 0.25f, defender.MoraleModifierS + 0.08f);

                    bool isPlayerOvertake = attacker.TeamID == _playerTeamID
                                        || defender.TeamID == _playerTeamID;
                    string drsTag = drsOn ? " [DRS]" : string.Empty;
                    state.Events.Add(new RaceEvent(
                        lap, atkDrv.driverName, "OVERTAKE",
                        $"{atkDrv.shortName} overtakes {defDrv.shortName} "
                        + $"for P{attacker.Position - 1}{drsTag}",
                        isPlayerEvent: isPlayerOvertake));

                    state.OverallOvertakeCount++;
                }

                // Momentum decay for all cars each lap (applied once per overtake resolution pass)
            }

            // ── Decay momentum for all cars ───────────────────────────────────
            foreach (CarState car in state.Cars)
            {
                if (car.DNF) continue;
                // Fastest-lap holder sustains a small pace benefit
                if (car.DriverID == state.FastestLapDriverID)
                    car.MoraleModifierS = Math.Max(-0.12f, car.MoraleModifierS - 0.02f);

                car.MoraleModifierS *= 0.82f;
                car.MoraleModifierS  = Math.Max(-0.30f, Math.Min(0.25f, car.MoraleModifierS));
            }
        }

        private static float _DefenderAggression(CarState car, DriverInfo driver)
        {
            float baseAgg;
            switch (car.Instruction)
            {
                case DriverInstruction.DEFEND: baseAgg = 0.85f; break;
                case DriverInstruction.ATTACK: baseAgg = 0.15f; break;
                default:                       baseAgg = 0.30f; break;
            }
            float moraleFactor = driver.morale / 100f;
            return Math.Min(1f, baseAgg * moraleFactor * 1.1f);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Position & gap management
        // ─────────────────────────────────────────────────────────────────────

        private static void _UpdatePositions(RaceState state)
        {
            var sorted = state.SortedCars();
            for (int pos = 1; pos <= sorted.Count; pos++)
                sorted[pos - 1].Position = pos;
        }

        private static void _UpdateGaps(RaceState state)
        {
            var sorted = state.SortedCars();
            if (sorted.Count == 0) return;

            CarState leader = sorted[0];
            for (int i = 0; i < sorted.Count; i++)
            {
                CarState car = sorted[i];
                if (i == 0)
                {
                    car.GapToLeaderS = 0f;
                    car.GapToAheadS  = 0f;
                }
                else
                {
                    CarState ahead = sorted[i - 1];
                    if (car.LapsCompleted < ahead.LapsCompleted)
                    {
                        car.GapToAheadS  = 9999f;
                        car.GapToLeaderS = 9999f;
                    }
                    else
                    {
                        car.GapToAheadS  = Math.Max(0f, car.TotalRaceTimeS - ahead.TotalRaceTimeS);
                        car.GapToLeaderS = Math.Max(0f, car.TotalRaceTimeS - leader.TotalRaceTimeS);
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Safety car management
        // ─────────────────────────────────────────────────────────────────────

        private void _UpdateSafetyCar(RaceState state, int lap)
        {
            switch (state.SafetyCar)
            {
                case SafetyCarState.DEPLOYED:
                    state.SafetyCarLapsRemaining--;
                    if (state.SafetyCarLapsRemaining <= 0)
                    {
                        state.SafetyCar = SafetyCarState.ENDING;
                        state.Events.Add(new RaceEvent(lap, "Race Control", "SC",
                            "Safety Car is coming in. Green flag next lap!"));
                    }
                    break;

                case SafetyCarState.ENDING:
                    state.SafetyCar = SafetyCarState.NONE;
                    state.Events.Add(new RaceEvent(lap, "Race Control", "SC",
                        "GREEN FLAG — Racing resumes!"));
                    break;

                case SafetyCarState.VSC:
                    state.SafetyCarLapsRemaining--;
                    if (state.SafetyCarLapsRemaining <= 0)
                    {
                        state.SafetyCar = SafetyCarState.NONE;
                        state.Events.Add(new RaceEvent(lap, "Race Control", "SC",
                            "Virtual Safety Car period has ended."));
                    }
                    break;
            }
        }

        private void _MaybeTriggerSafetyCar(RaceState state, int lap)
        {
            if (state.SafetyCar != SafetyCarState.NONE) return;
            if (lap <= 2 || lap >= state.TotalLaps - 3)  return;

            float triggerProb = _circuit.safetyCarlProbability / Math.Max(1, state.TotalLaps);
            if (_rng.NextDouble() < triggerProb)
            {
                bool isVSC = _rng.NextDouble() < 0.35;
                if (isVSC)
                {
                    state.SafetyCar              = SafetyCarState.VSC;
                    state.SafetyCarLapsRemaining = 2 + _rng.Next(0, 3); // 2-4 laps
                    state.Events.Add(new RaceEvent(lap, "Race Control", "SC",
                        "VIRTUAL SAFETY CAR deployed — debris on track."));
                }
                else
                {
                    state.SafetyCar              = SafetyCarState.DEPLOYED;
                    state.SafetyCarLapsRemaining = 3 + _rng.Next(0, 3); // 3-5 laps
                    state.Events.Add(new RaceEvent(lap, "Race Control", "SC",
                        $"SAFETY CAR deployed! (~{state.SafetyCarLapsRemaining} laps)"));
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DNF / incident checks
        // ─────────────────────────────────────────────────────────────────────

        private void _CheckDNF(RaceState state, int lap)
        {
            const float BASE_CRASH_PROB = 0.0015f;

            foreach (CarState car in state.Cars)
            {
                if (car.DNF) continue;
                if (!_drivers.TryGetValue(car.DriverID, out DriverInfo drv)) continue;
                if (!_teams.TryGetValue(car.TeamID,     out TeamInfo   team)) continue;

                // ── Aquaplaning ───────────────────────────────────────────────
                float aquaChance = _weatherSys.AquaplaningChance(car.Compound);
                if (aquaChance > 0f && _rng.NextDouble() < aquaChance)
                {
                    float penalty = (float)(0.5 + _rng.NextDouble() * 0.7);
                    car.LastLapTimeS    += penalty;
                    car.TotalRaceTimeS  += penalty;
                    bool ip = team.id == _playerTeamID;
                    state.Events.Add(new RaceEvent(lap, drv.driverName, "INFO",
                        $"{drv.shortName} aquaplanes! Loses {penalty:F1}s",
                        isPlayerEvent: ip));
                }

                // ── Mechanical reliability ────────────────────────────────────
                // base rate: (1 - reliability/100) * 2, distributed over all laps
                float mechRate = (1f - team.reliability / 100f) * 2f;
                float dnfProb  = mechRate / Math.Max(1, _circuit.totalLaps);
                if (car.Instruction == DriverInstruction.ATTACK) dnfProb *= 1.5f;
                if (_rng.NextDouble() < dnfProb)
                {
                    _RetireCar(car, drv, team, state, lap, "Mechanical failure");
                    continue;
                }

                // ── Crash check ───────────────────────────────────────────────
                float crashProb = BASE_CRASH_PROB;
                crashProb *= 1f + (1f - team.reliability / 100f) * 2f;
                if (state.Weather == WeatherCondition.LIGHT_RAIN
                 || state.Weather == WeatherCondition.HEAVY_RAIN)
                    crashProb *= 2.2f;
                if (car.Instruction == DriverInstruction.ATTACK) crashProb *= 1.6f;

                if (_rng.NextDouble() < crashProb)
                {
                    float severity = (float)_rng.NextDouble();
                    bool  ip       = team.id == _playerTeamID;

                    if (severity > 0.90f)
                    {
                        _RetireCar(car, drv, team, state, lap, "Crash");
                        if (state.SafetyCar == SafetyCarState.NONE)
                        {
                            state.SafetyCar              = SafetyCarState.DEPLOYED;
                            state.SafetyCarLapsRemaining = 3 + _rng.Next(0, 3);
                            state.Events.Add(new RaceEvent(lap, "Race Control", "SC",
                                $"SAFETY CAR — {drv.shortName} has crashed out!"));
                        }
                    }
                    else if (severity > 0.65f)
                    {
                        float dmg = (float)(0.15 + _rng.NextDouble() * 0.35);
                        car.DamageS += dmg;
                        state.Events.Add(new RaceEvent(lap, drv.driverName, "INFO",
                            $"{drv.shortName} has car damage: +{dmg:F1}s/lap",
                            isPlayerEvent: ip));
                    }
                }
            }
        }

        private void _RetireCar(CarState car, DriverInfo drv, TeamInfo team,
                                 RaceState state, int lap, string reason)
        {
            car.DNF       = true;
            car.DNFReason = reason;
            bool ip = team.id == _playerTeamID;
            state.Events.Add(new RaceEvent(lap, drv.driverName, "DNF",
                $"{drv.shortName} ({team.shortName}) RETIRES — {reason}",
                isPlayerEvent: ip));
        }

        // ─────────────────────────────────────────────────────────────────────
        // Tyre phase updates
        // ─────────────────────────────────────────────────────────────────────

        private void _UpdateTirePhases(RaceState state)
        {
            foreach (CarState car in state.Cars)
            {
                if (car.DNF) continue;
                if (!TireProfiles.All.TryGetValue(car.Compound, out TireProfile profile)) continue;
                float degMult = _circuit.tireDegMultiplier;
                car.TirePhase  = TireSystem.GetPhase(profile, car.TireAgeLaps, degMult);
                car.TireDegPct = TireSystem.DegPct(profile, car.TireAgeLaps, degMult);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // AI instruction updates
        // ─────────────────────────────────────────────────────────────────────

        private void _UpdateAIInstructions(RaceState state)
        {
            foreach (CarState car in state.Cars)
            {
                if (car.DNF || car.TeamID == _playerTeamID) continue;
                if (!_drivers.TryGetValue(car.DriverID, out DriverInfo drv)) continue;
                car.Instruction = _ai.ChooseInstruction(car, state, drv);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Engineer radio
        // ─────────────────────────────────────────────────────────────────────

        private void _EngineerRadio(RaceState state, int lap)
        {
            var playerCars = state.GetPlayerCars();

            foreach (CarState car in playerCars)
            {
                if (car.DNF || car.IsPittingThisLap) continue;
                if (!_drivers.TryGetValue(car.DriverID, out DriverInfo drv)) continue;

                int lapsLeft = state.TotalLaps - lap;

                // Helper: emit rate-limited radio message
                void Radio(string category, int cooldown, string msg)
                {
                    var key = (car.DriverID, category);
                    int lastLap = _radioLast.TryGetValue(key, out int v) ? v : -999;
                    if (lap - lastLap >= cooldown)
                    {
                        _radioLast[key] = lap;
                        state.Events.Add(new RaceEvent(lap, drv.driverName, "INFO",
                            msg, isPlayerEvent: true));
                    }
                }

                // ── Tyre warnings ─────────────────────────────────────────────
                if (TireProfiles.All.TryGetValue(car.Compound, out TireProfile profile))
                {
                    float degMult = _circuit.tireDegMultiplier;
                    int   window  = TireSystem.WindowRemaining(profile, car.TireAgeLaps, degMult);
                    TirePhase phase = TireSystem.GetPhase(profile, car.TireAgeLaps, degMult);

                    if (phase == TirePhase.CLIFF)
                        Radio("tire_cliff", 2,
                            $"Box box box! {drv.shortName}: tyre is gone — pit NOW!");
                    else if (window == 4)
                        Radio("tire_4", 5,
                            $"Engineer: {drv.shortName}, you have four laps of tyre left.");
                    else if (window == 2)
                        Radio("tire_2", 3,
                            $"Engineer: Two laps, {drv.shortName}. Start thinking about the box.");
                    else if (window == 1)
                        Radio("tire_1", 2,
                            $"Engineer: Last lap of the tyre, {drv.shortName}. Box next lap.");
                }

                // ── Safety car pit window ─────────────────────────────────────
                if ((state.SafetyCar == SafetyCarState.DEPLOYED ||
                     state.SafetyCar == SafetyCarState.VSC)
                        && car.TireAgeLaps >= 6 && lapsLeft > 8)
                {
                    Radio("sc_window", 4,
                        $"Engineer: Safety car — free stop available! Box this lap, {drv.shortName}!");
                }

                // ── DRS / gap battle ahead ────────────────────────────────────
                if (car.GapToAheadS > 0f && car.GapToAheadS <= 1.0f && car.Position > 1)
                {
                    var sorted   = state.SortedCars();
                    int posIdx   = sorted.FindIndex(c => c.DriverID == car.DriverID);
                    if (posIdx > 0)
                    {
                        CarState    target     = sorted[posIdx - 1];
                        string targetName = _drivers.TryGetValue(target.DriverID, out DriverInfo td)
                            ? td.shortName : "car ahead";

                        if (car.GapToAheadS <= 0.5f)
                            Radio("drs_attack", 3,
                                $"Engineer: DRS engaged — go for it on {targetName}, {drv.shortName}!");
                        else
                            Radio("drs_hunt", 4,
                                $"Engineer: Gap to {targetName}: {car.GapToAheadS:F1}s — keep pushing.");
                    }
                }

                // ── Defend warning: car closing from behind ───────────────────
                {
                    var sorted = state.SortedCars();
                    int posIdx = sorted.FindIndex(c => c.DriverID == car.DriverID);
                    if (posIdx >= 0 && posIdx < sorted.Count - 1)
                    {
                        CarState behind = sorted[posIdx + 1];
                        if (!behind.DNF && behind.GapToAheadS > 0f && behind.GapToAheadS <= 1.2f)
                        {
                            string behindName = _drivers.TryGetValue(behind.DriverID, out DriverInfo bd)
                                ? bd.shortName : "car behind";
                            Radio("defend_warning", 4,
                                $"Engineer: {behindName} is {behind.GapToAheadS:F1}s behind "
                                + "— watch your mirrors.");
                        }
                    }
                }

                // ── Undercut threat: rival behind pitted last lap ──────────────
                if (lapsLeft > 8)
                {
                    var sorted = state.SortedCars();
                    int posIdx = sorted.FindIndex(c => c.DriverID == car.DriverID);
                    if (posIdx >= 0)
                    {
                        int rangeEnd = Math.Min(sorted.Count, posIdx + 4);
                        for (int j = posIdx + 1; j < rangeEnd; j++)
                        {
                            CarState other = sorted[j];
                            if (other.DNF || !other.PittedLastLap) continue;
                            string otherName = _drivers.TryGetValue(other.DriverID, out DriverInfo od)
                                ? od.shortName : "rival";
                            float gap = other.GapToLeaderS - car.GapToLeaderS;
                            Radio("undercut_threat", 6,
                                $"Engineer: {otherName} has pitted — undercut threat! "
                                + $"Gap: {gap:F1}s. Consider boxing.");
                            break;
                        }
                    }
                }

                // ── Overcut opportunity: rival ahead pitted ───────────────────
                if (lapsLeft > 5)
                {
                    var sorted = state.SortedCars();
                    int posIdx = sorted.FindIndex(c => c.DriverID == car.DriverID);
                    if (posIdx > 0)
                    {
                        CarState ahead = sorted[posIdx - 1];
                        if (ahead.IsPittingThisLap || ahead.PittedLastLap)
                        {
                            string aheadName = _drivers.TryGetValue(ahead.DriverID, out DriverInfo ad)
                                ? ad.shortName : "car ahead";
                            Radio("overcut_opp", 6,
                                $"Engineer: {aheadName} is in the pits — push hard for the overcut!");
                        }
                    }
                }

                // ── Fuel warning ──────────────────────────────────────────────
                float fuelLaps = car.FuelKg / Math.Max(0.1f, _circuit.fuelConsumptionKg);
                if (fuelLaps < lapsLeft - 1 && lapsLeft > 3)
                    Radio("fuel_warn", 5,
                        $"Engineer: Fuel is short, {drv.shortName} — switch to fuel save mode.");
                else if (lapsLeft > 1 && lapsLeft <= 5 && fuelLaps >= lapsLeft)
                    Radio("fuel_ok", 99,
                        "Engineer: Fuel is fine to the end. Push if you need to.");

                // ── Final laps countdown ──────────────────────────────────────
                if (lapsLeft == 5)
                    Radio("5_to_go", 99,
                        $"Engineer: Five laps to go, {drv.shortName}. Give it everything.");
                else if (lapsLeft == 3)
                    Radio("3_to_go", 99,
                        $"Engineer: Three laps remaining. P{car.Position} — keep it clean.");
                else if (lapsLeft == 1)
                    Radio("last_lap", 99,
                        $"Engineer: Final lap, {drv.shortName}! You're P{car.Position}.");

                // ── Podium / points encouragement ─────────────────────────────
                if (car.Position <= 3)
                    Radio("podium", 10,
                        $"Engineer: P{car.Position} — you're on the podium! Keep it together.");
                else if (car.Position <= 10 && lapsLeft <= 10)
                    Radio("points", 8,
                        $"Engineer: P{car.Position} — you're in the points. Bring it home.");

                // ── Fastest lap within 0.8 s ──────────────────────────────────
                float fl = state.FastestLapTimeS;
                float ll = car.LastLapTimeS;
                if (lapsLeft >= 2 && lapsLeft <= 10
                        && ll > 0f && fl > 0f && fl < 9999f
                        && ll - fl < 0.8f
                        && car.TirePhase != TirePhase.CLIFF)
                {
                    Radio("fl_attempt", 5,
                        $"Engineer: You're {ll - fl:F2}s off fastest lap — push for the bonus point!");
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Race finalisation
        // ─────────────────────────────────────────────────────────────────────

        private void _FinalizeRace(RaceState state)
        {
            int[] pointsMap = { 25, 18, 15, 12, 10, 8, 6, 4, 2, 1 };

            var active = state.Cars.Where(c => !c.DNF)
                .OrderByDescending(c => c.LapsCompleted)
                .ThenBy(c => c.TotalRaceTimeS)
                .ToList();
            var dnfd = state.Cars.Where(c => c.DNF).ToList();

            for (int pos = 1; pos <= active.Count; pos++)
                active[pos - 1].Position = pos;
            for (int pos = active.Count + 1; pos <= active.Count + dnfd.Count; pos++)
                dnfd[pos - active.Count - 1].Position = pos;

            // Fastest lap bonus: +1 point if in P1-10
            int flDriverID = state.FastestLapDriverID;

            if (_drivers.TryGetValue(active[0].DriverID, out DriverInfo winner))
            {
                state.Events.Add(new RaceEvent(
                    state.TotalLaps, "Race Control", "INFO",
                    $"CHEQUERED FLAG — {winner.shortName} wins!"));
            }

            // Points are tracked on CarState implicitly via position; UI reads pointsMap[position-1]
            // Fastest lap bonus handled by UI when reading state.FastestLapDriverID
        }

        // ─────────────────────────────────────────────────────────────────────
        // Gaussian helper (Box-Muller)
        // ─────────────────────────────────────────────────────────────────────

        private float _NextGaussian(float mean, float sigma)
        {
            double u1 = 1.0 - _rng.NextDouble();
            double u2 = 1.0 - _rng.NextDouble();
            double z  = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            return (float)(mean + sigma * z);
        }
    }
}
