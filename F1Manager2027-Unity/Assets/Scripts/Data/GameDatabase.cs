using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace F1Manager
{
    [CreateAssetMenu(fileName = "GameDatabase", menuName = "F1Manager/Game Database")]
    public class GameDatabase : ScriptableObject
    {
        public CircuitData[] circuits;
        public TeamData[] teams;
        public DriverData[] drivers;

        public CircuitData GetCircuit(int id)
        {
            return circuits.FirstOrDefault(c => c.id == id);
        }

        public TeamData GetTeam(int id)
        {
            return teams.FirstOrDefault(t => t.id == id);
        }

        public DriverData GetDriver(int id)
        {
            return drivers.FirstOrDefault(d => d.id == id);
        }

        public List<DriverData> GetTeamDrivers(int teamId)
        {
            return drivers.Where(d => d.teamID == teamId).ToList();
        }

        public List<DriverData> GetActiveDrivers()
        {
            return drivers.Where(d => !d.isReserve).ToList();
        }
    }
}
