"""Dynamic weather system with forecast accuracy degrading over distance."""

from __future__ import annotations
import random
from dataclasses import dataclass, field
from typing import TYPE_CHECKING

from .models import WeatherCondition, TireCompound

if TYPE_CHECKING:
    from ..data.circuits import Circuit


@dataclass
class WeatherSystem:
    circuit: "Circuit"
    rng: random.Random = field(default_factory=random.Random)
    total_laps: int = 57

    # Current state
    condition: WeatherCondition = WeatherCondition.DRY
    track_temp_c: float = 28.0
    air_temp_c: float = 22.0
    water_depth_mm: float = 0.0

    # Forecast (list of WeatherCondition per next 5 laps)
    forecast: list[WeatherCondition] = field(default_factory=list)
    rain_start_lap: int = -1       # -1 = no rain planned this race
    rain_end_lap: int = -1
    rain_intensity_peak: float = 0.0  # 0–1

    # Internal random seed for reproducibility in QA
    _seed: int = 0

    def __post_init__(self) -> None:
        self.track_temp_c = self.rng.uniform(*self.circuit.track_temp_range)
        self.air_temp_c = self.track_temp_c - self.rng.uniform(4.0, 10.0)
        self._plan_weather_events()
        self._update_forecast(current_lap=0)

    def _plan_weather_events(self) -> None:
        """Decide upfront if/when rain arrives this race (hidden from player)."""
        if self.rng.random() > self.circuit.rain_probability:
            return

        # Rain start: spread across laps 5 to total-10
        self.rain_start_lap = self.rng.randint(5, max(6, self.total_laps - 10))
        duration = self.rng.randint(5, 20)
        self.rain_end_lap = min(self.total_laps, self.rain_start_lap + duration)
        # Peak intensity: heavier on Street circuits & high-rain tracks
        base_intensity = self.circuit.rain_probability * 1.2
        self.rain_intensity_peak = min(1.0, self.rng.uniform(base_intensity * 0.4,
                                                              base_intensity * 1.6))

    def advance(self, current_lap: int) -> list[str]:
        """Advance weather state to current_lap. Returns list of weather event messages."""
        messages: list[str] = []

        if self.rain_start_lap != -1:
            if current_lap == self.rain_start_lap:
                if self.rain_intensity_peak > 0.6:
                    self.condition = WeatherCondition.HEAVY_RAIN
                    self.water_depth_mm = self.rain_intensity_peak * 12
                    messages.append("⛈ Heavy rain has arrived! Wet tyres strongly advised.")
                else:
                    self.condition = WeatherCondition.LIGHT_RAIN
                    self.water_depth_mm = self.rain_intensity_peak * 5
                    messages.append("🌧 Light rain falling. Intermediates coming into play.")
                self.track_temp_c = max(10.0, self.track_temp_c - self.rng.uniform(5, 10))

            elif current_lap == self.rain_end_lap:
                self.condition = WeatherCondition.DRYING
                self.water_depth_mm *= 0.2
                messages.append("☀ Rain has stopped. Track is drying — slick tyres soon viable.")

            elif current_lap > self.rain_end_lap and self.condition == WeatherCondition.DRYING:
                drying_laps = current_lap - self.rain_end_lap
                if drying_laps >= 3:
                    self.condition = WeatherCondition.DRY
                    self.water_depth_mm = 0.0
                    self.track_temp_c = min(
                        self.circuit.track_temp_range[1],
                        self.track_temp_c + drying_laps * 1.5
                    )
                    messages.append("✅ Track fully dry. Slick tyres are now optimal.")
                else:
                    self.water_depth_mm *= 0.5

        # Natural temperature oscillation ±0.5°C per lap
        self.track_temp_c += self.rng.uniform(-0.3, 0.3)
        self.track_temp_c = max(5.0, min(50.0, self.track_temp_c))
        self.air_temp_c = self.track_temp_c - self.rng.uniform(4.0, 8.0)

        self._update_forecast(current_lap)
        return messages

    def _update_forecast(self, current_lap: int) -> None:
        """Build 5-lap forecast (condition only; accuracy degrades with distance)."""
        self.forecast = []
        for offset in range(1, 6):
            lap = current_lap + offset
            self.forecast.append(self._forecast_condition(lap, offset))

    def _forecast_condition(self, lap: int, horizon: int) -> WeatherCondition:
        """
        Predict condition at lap. Accuracy degrades beyond 3 laps.
        Horizon 1 = 95%, 3 = 88%, 5 = 82%.
        """
        accuracy = max(0.80, 0.97 - horizon * 0.03)
        if self.rng.random() > accuracy:
            # Mis-predict: flip to opposite
            return WeatherCondition.DRY if self._true_condition_at(lap) != WeatherCondition.DRY \
                   else WeatherCondition.LIGHT_RAIN
        return self._true_condition_at(lap)

    def _true_condition_at(self, lap: int) -> WeatherCondition:
        if self.rain_start_lap == -1:
            return WeatherCondition.DRY
        if lap < self.rain_start_lap:
            return WeatherCondition.DRY
        if lap < self.rain_end_lap:
            if self.rain_intensity_peak > 0.6:
                return WeatherCondition.HEAVY_RAIN
            return WeatherCondition.LIGHT_RAIN
        if lap < self.rain_end_lap + 3:
            return WeatherCondition.DRYING
        return WeatherCondition.DRY

    def lap_time_weather_penalty_s(self, compound: TireCompound) -> float:
        """
        Returns lap-time penalty (seconds) for using the given tyre in current conditions.
        Correct tyre choice = 0 penalty. Wrong tyre = significant penalty.
        """
        cond = self.condition
        is_dry_tyre = compound.is_dry
        is_inter = compound == TireCompound.INTER
        is_wet  = compound == TireCompound.WET

        if cond == WeatherCondition.DRY:
            if is_dry_tyre:
                return 0.0
            if is_inter:
                return 1.5   # Inters on dry = graining, too slow
            return 2.5       # Wets on dry = extremely slow

        if cond == WeatherCondition.LIGHT_RAIN:
            if is_inter:
                return 0.0   # Optimal
            if is_dry_tyre:
                water = self.water_depth_mm
                if water < 2:
                    return 0.6    # Slight disadvantage
                elif water < 4:
                    return 1.2
                return 2.0        # Aquaplaning risk
            return 0.8           # Wets in light rain: too much rubber

        if cond == WeatherCondition.HEAVY_RAIN:
            if is_wet:
                return 0.0
            if is_inter:
                depth = self.water_depth_mm
                if depth < 5:
                    return 0.3
                elif depth < 8:
                    return 0.8
                return 1.5
            return 3.5           # Slicks in heavy rain = dangerous

        if cond == WeatherCondition.DRYING:
            if is_inter:
                return 0.2       # Still reasonable
            if is_dry_tyre:
                return 0.5       # Damp track; losing a bit
            return 1.0           # Wets on drying track

        return 0.0

    def aquaplaning_chance(self, compound: TireCompound) -> float:
        """Probability of aquaplaning event this lap (0–1)."""
        if compound.is_dry and self.condition in (WeatherCondition.LIGHT_RAIN,
                                                   WeatherCondition.HEAVY_RAIN):
            depth = self.water_depth_mm
            if depth < 3:
                return 0.012
            if depth < 6:
                return 0.025
            return 0.04
        if compound == TireCompound.INTER and self.condition == WeatherCondition.HEAVY_RAIN:
            depth = self.water_depth_mm
            if depth > 10:
                return 0.004
            if depth > 7:
                return 0.002
        return 0.0

    def recommended_compound(self) -> TireCompound:
        """Current optimal tyre for conditions."""
        if self.condition == WeatherCondition.HEAVY_RAIN:
            return TireCompound.WET
        if self.condition in (WeatherCondition.LIGHT_RAIN, WeatherCondition.DRYING):
            return TireCompound.INTER
        return TireCompound.C5  # Dry default; caller should override with best available dry

    @property
    def forecast_string(self) -> str:
        """5-lap forecast as short icons."""
        icons = {
            WeatherCondition.DRY: "☀",
            WeatherCondition.LIGHT_RAIN: "🌦",
            WeatherCondition.HEAVY_RAIN: "⛈",
            WeatherCondition.DRYING: "🌤",
        }
        return " ".join(icons.get(c, "?") for c in self.forecast)
