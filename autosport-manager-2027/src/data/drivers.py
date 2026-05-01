"""20 F1 drivers + 4 reserve drivers for the 2027 season."""

from dataclasses import dataclass


@dataclass
class Driver:
    id: int
    name: str
    short_name: str          # 3-letter code for standings display
    car_number: int
    team_id: int
    nationality: str
    # Core ratings 0–100
    pace: int                # Raw one-lap speed potential
    racecraft: int           # Overtaking ability (used as skill_atk in sigmoid)
    defending: int           # Defensive ability (used as skill_def in sigmoid)
    wet_skill: int           # Performance modifier in rain
    consistency: int         # Variance control — lower = more random variance
    tire_management: int     # How well they manage tire degradation (affects wear rate)
    # Career state
    morale: float            # 0–100: affects pace_modifier ±0.5s/lap
    experience: int          # Seasons in F1; affects maturity and strategy following
    is_reserve: bool = False


DRIVERS: list[Driver] = [
    # ─── Red Bull ───────────────────────────────────────────────────────────────
    Driver(id=0,  name="Max Verstappen",        short_name="VER", car_number=1,   team_id=0,
           nationality="NLD", pace=98, racecraft=98, defending=93, wet_skill=96,
           consistency=97, tire_management=94, morale=90, experience=10),
    Driver(id=1,  name="Liam Lawson",           short_name="LAW", car_number=30,  team_id=0,
           nationality="NZL", pace=84, racecraft=83, defending=79, wet_skill=80,
           consistency=79, tire_management=78, morale=82, experience=2),

    # ─── Ferrari ────────────────────────────────────────────────────────────────
    Driver(id=2,  name="Charles Leclerc",       short_name="LEC", car_number=16,  team_id=1,
           nationality="MCO", pace=95, racecraft=91, defending=87, wet_skill=89,
           consistency=87, tire_management=88, morale=86, experience=7),
    Driver(id=3,  name="Lewis Hamilton",        short_name="HAM", car_number=44,  team_id=1,
           nationality="GBR", pace=92, racecraft=97, defending=92, wet_skill=98,
           consistency=91, tire_management=95, morale=84, experience=18),

    # ─── Mercedes ───────────────────────────────────────────────────────────────
    Driver(id=4,  name="George Russell",        short_name="RUS", car_number=63,  team_id=2,
           nationality="GBR", pace=91, racecraft=89, defending=85, wet_skill=87,
           consistency=89, tire_management=87, morale=85, experience=6),
    Driver(id=5,  name="Andrea Kimi Antonelli", short_name="ANT", car_number=12,  team_id=2,
           nationality="ITA", pace=84, racecraft=81, defending=76, wet_skill=78,
           consistency=75, tire_management=76, morale=88, experience=1),

    # ─── McLaren ────────────────────────────────────────────────────────────────
    Driver(id=6,  name="Lando Norris",          short_name="NOR", car_number=4,   team_id=3,
           nationality="GBR", pace=93, racecraft=92, defending=86, wet_skill=85,
           consistency=88, tire_management=86, morale=89, experience=6),
    Driver(id=7,  name="Oscar Piastri",         short_name="PIA", car_number=81,  team_id=3,
           nationality="AUS", pace=90, racecraft=88, defending=82, wet_skill=83,
           consistency=87, tire_management=85, morale=87, experience=3),

    # ─── Aston Martin ───────────────────────────────────────────────────────────
    Driver(id=8,  name="Fernando Alonso",       short_name="ALO", car_number=14,  team_id=4,
           nationality="ESP", pace=90, racecraft=97, defending=96, wet_skill=93,
           consistency=89, tire_management=92, morale=80, experience=22),
    Driver(id=9,  name="Lance Stroll",          short_name="STR", car_number=18,  team_id=4,
           nationality="CAN", pace=78, racecraft=74, defending=72, wet_skill=71,
           consistency=68, tire_management=72, morale=72, experience=8),

    # ─── Williams ───────────────────────────────────────────────────────────────
    Driver(id=10, name="Carlos Sainz",          short_name="SAI", car_number=55,  team_id=5,
           nationality="ESP", pace=89, racecraft=90, defending=87, wet_skill=84,
           consistency=87, tire_management=88, morale=83, experience=10),
    Driver(id=11, name="Alexander Albon",       short_name="ALB", car_number=23,  team_id=5,
           nationality="THA", pace=83, racecraft=82, defending=80, wet_skill=78,
           consistency=81, tire_management=80, morale=80, experience=6),

    # ─── Alpine ─────────────────────────────────────────────────────────────────
    Driver(id=12, name="Pierre Gasly",          short_name="GAS", car_number=10,  team_id=6,
           nationality="FRA", pace=82, racecraft=83, defending=81, wet_skill=80,
           consistency=79, tire_management=80, morale=78, experience=9),
    Driver(id=13, name="Jack Doohan",           short_name="DOO", car_number=7,   team_id=6,
           nationality="AUS", pace=76, racecraft=74, defending=70, wet_skill=71,
           consistency=70, tire_management=71, morale=80, experience=1),

    # ─── Haas ───────────────────────────────────────────────────────────────────
    Driver(id=14, name="Esteban Ocon",          short_name="OCO", car_number=31,  team_id=7,
           nationality="FRA", pace=80, racecraft=79, defending=78, wet_skill=76,
           consistency=77, tire_management=79, morale=74, experience=9),
    Driver(id=15, name="Oliver Bearman",        short_name="BEA", car_number=87,  team_id=7,
           nationality="GBR", pace=78, racecraft=76, defending=73, wet_skill=72,
           consistency=72, tire_management=73, morale=83, experience=1),

    # ─── Audi / Kick Sauber ─────────────────────────────────────────────────────
    Driver(id=16, name="Nico Hülkenberg",       short_name="HUL", car_number=27,  team_id=8,
           nationality="DEU", pace=82, racecraft=81, defending=83, wet_skill=79,
           consistency=83, tire_management=82, morale=76, experience=14),
    Driver(id=17, name="Gabriel Bortoleto",     short_name="BOR", car_number=5,   team_id=8,
           nationality="BRA", pace=80, racecraft=78, defending=74, wet_skill=75,
           consistency=74, tire_management=75, morale=86, experience=1),

    # ─── Racing Bulls (VCARB) ────────────────────────────────────────────────────
    Driver(id=18, name="Yuki Tsunoda",          short_name="TSU", car_number=22,  team_id=9,
           nationality="JPN", pace=84, racecraft=82, defending=78, wet_skill=77,
           consistency=77, tire_management=78, morale=79, experience=5),
    Driver(id=19, name="Isack Hadjar",          short_name="HAD", car_number=6,   team_id=9,
           nationality="FRA", pace=80, racecraft=77, defending=73, wet_skill=74,
           consistency=73, tire_management=74, morale=85, experience=1),

    # ─── Reserve Drivers ────────────────────────────────────────────────────────
    Driver(id=20, name="Nyck de Vries",         short_name="DVR", car_number=0,   team_id=-1,
           nationality="NLD", pace=72, racecraft=70, defending=68, wet_skill=69,
           consistency=72, tire_management=71, morale=70, experience=3, is_reserve=True),
    Driver(id=21, name="Callum Ilott",          short_name="ILO", car_number=0,   team_id=-1,
           nationality="GBR", pace=70, racecraft=68, defending=66, wet_skill=68,
           consistency=70, tire_management=69, morale=72, experience=2, is_reserve=True),
]

DRIVER_MAP = {d.id: d for d in DRIVERS}
