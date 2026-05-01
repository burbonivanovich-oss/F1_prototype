using UnityEngine;

namespace F1Manager
{
    [CreateAssetMenu(fileName = "NewDriver", menuName = "F1Manager/Driver Data")]
    public class DriverData : ScriptableObject
    {
        public int id;
        public string driverName;
        public string shortName;
        public int carNumber;
        public int teamID;
        public int pace;
        public int racecraft;
        public int defending;
        public int tireManagement;
        public int wetSkill;
        public int experience;
        public int morale;
        public bool isReserve;
    }
}
