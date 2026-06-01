using System;
using UnityEngine;

namespace FrostShelter.Temperature
{
    [Serializable]
    public class TemperatureConfigSO : ScriptableObject
    {
        public float BaseTemperature = -30f;
        public float TemperaturePerFurnaceLevel = 3f;
        public float MaxTemperature = 20f;
        public float MaxMinTemperature = -60f;

        public TemperatureThreshold[] EfficiencyCurves;

        // Blizzard params
        public float BlizzardTempDrop = 10f;
        public float BlizzardMinDuration = 180f;
        public float BlizzardMaxDuration = 600f;
        public float BlizzardIntervalMinMinutes = 20f;
        public float BlizzardIntervalMaxMinutes = 60f;

        [Serializable]
        public struct TemperatureThreshold
        {
            public float MinTemp;
            public float MaxTemp;
            public float ProductionEfficiency;
            public float SurvivorDamagePerHour;
            public float SickChancePerHour;
        }
    }
}
