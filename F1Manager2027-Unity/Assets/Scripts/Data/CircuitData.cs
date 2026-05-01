using UnityEngine;

namespace F1Manager
{
    [CreateAssetMenu(fileName = "NewCircuit", menuName = "F1Manager/Circuit Data")]
    public class CircuitData : ScriptableObject
    {
        public int id;
        public string circuitName;
        public string country;
        public string city;
        public int totalLaps;
        public float baseLapTimeS;
        public float circuitLengthKm;
        public float tireDegMultiplier;
        public float overtakeDifficulty;
        public int drsZones;
        public float fuelConsumptionKg;
        public float rainProbability;
        public float trackTempRangeMin;
        public float trackTempRangeMax;
        public float safetyCarlProbability;
        // length 3: [s1, s2, s3]  (set in Inspector — always 3 elements)
        public float[] sectorSplits = new float[3];
        public float powerSensitivity;
    }
}
