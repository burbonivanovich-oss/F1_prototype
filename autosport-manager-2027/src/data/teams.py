"""10 F1 constructor teams with car performance data."""

from dataclasses import dataclass, field


@dataclass
class Team:
    id: int
    name: str
    short_name: str          # 3-letter abbrev for display
    color: str               # Rich color string for terminal display
    car_performance: int     # 0–100: raw car pace ceiling this season
    reliability: float       # 0.0–1.0: modifier on DNF probability
    pit_crew_skill: int      # 0–100: affects pit stop time
    aero_philosophy: str     # "high_df" | "low_df" | "balanced"
    budget_m: float          # Season budget $M
    # These evolve over the season; prototype starts at initial values
    upgrade_rate: float      # R&D tokens per race window (higher = faster development)


TEAMS: list[Team] = [
    Team(
        id=0, name="Oracle Red Bull Racing", short_name="RBR",
        color="bold blue",
        car_performance=97, reliability=0.97,
        pit_crew_skill=96, aero_philosophy="low_df",
        budget_m=215.0, upgrade_rate=1.10,
    ),
    Team(
        id=1, name="Scuderia Ferrari", short_name="FER",
        color="bold red",
        car_performance=95, reliability=0.94,
        pit_crew_skill=93, aero_philosophy="balanced",
        budget_m=210.0, upgrade_rate=1.05,
    ),
    Team(
        id=2, name="Mercedes-AMG Petronas", short_name="MER",
        color="bold cyan",
        car_performance=93, reliability=0.96,
        pit_crew_skill=95, aero_philosophy="balanced",
        budget_m=210.0, upgrade_rate=1.05,
    ),
    Team(
        id=3, name="McLaren F1 Team", short_name="MCL",
        color="bold yellow",
        car_performance=94, reliability=0.95,
        pit_crew_skill=97, aero_philosophy="balanced",
        budget_m=200.0, upgrade_rate=1.08,
    ),
    Team(
        id=4, name="Aston Martin Aramco", short_name="AMR",
        color="bold green",
        car_performance=86, reliability=0.93,
        pit_crew_skill=88, aero_philosophy="high_df",
        budget_m=175.0, upgrade_rate=0.95,
    ),
    Team(
        id=5, name="Williams Racing", short_name="WIL",
        color="blue",
        car_performance=82, reliability=0.92,
        pit_crew_skill=84, aero_philosophy="low_df",
        budget_m=145.0, upgrade_rate=0.90,
    ),
    Team(
        id=6, name="BWT Alpine F1", short_name="ALP",
        color="bright_blue",
        car_performance=80, reliability=0.91,
        pit_crew_skill=83, aero_philosophy="balanced",
        budget_m=150.0, upgrade_rate=0.88,
    ),
    Team(
        id=7, name="MoneyGram Haas F1", short_name="HAS",
        color="white",
        car_performance=79, reliability=0.90,
        pit_crew_skill=81, aero_philosophy="balanced",
        budget_m=135.0, upgrade_rate=0.85,
    ),
    Team(
        id=8, name="Stake F1 / Audi", short_name="AUD",
        color="bright_magenta",
        car_performance=76, reliability=0.89,
        pit_crew_skill=78, aero_philosophy="balanced",
        budget_m=130.0, upgrade_rate=0.85,
    ),
    Team(
        id=9, name="Visa Cash App RB", short_name="VCR",
        color="bright_yellow",
        car_performance=81, reliability=0.93,
        pit_crew_skill=86, aero_philosophy="low_df",
        budget_m=160.0, upgrade_rate=0.92,
    ),
]

TEAM_MAP = {t.id: t for t in TEAMS}
