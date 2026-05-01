using UnityEngine;

namespace F1Manager
{
    [CreateAssetMenu(fileName = "NewTeam", menuName = "F1Manager/Team Data")]
    public class TeamData : ScriptableObject
    {
        public int id;
        public string teamName;
        public string shortName;
        public string colorHex;
        public int carPerformance;
        public int pitCrewSkill;
        public int reliability;
        public int powerUnit;
        public int chassis;
    }
}
