using System;

namespace FrostShelter.Exploration
{
    [Serializable]
    public class MapConfigSO
    {
        public int MinGridSize = 6;
        public int MaxGridSize = 8;
        public float NodeDensity = 0.6f;

        public MapNodeWeight NodeWeights;

        public int MinBossNodes = 1;
        public int MaxBossNodes = 2;
        public float DifficultyRampFactor = 1.15f;

        // 初始供应品
        public int StartFood = 100;
        public int StartWarmth = 100;
        public float FoodConsumePerStep = 5f;
        public float WarmthConsumePerStep = 8f;

        [Serializable]
        public struct MapNodeWeight
        {
            public float ResourceNodeWeight;
            public float BattleNodeWeight;
            public float EventNodeWeight;
            public float MerchantNodeWeight;
            public float BossNodeWeight;
        }
    }
}
