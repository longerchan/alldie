using System;

namespace FrostShelter.Core
{
    /// <summary>
    /// 全局游戏配置。所有系统共用参数的新手/标准/硬核档位。
    /// </summary>
    [Serializable]
    public class GlobalConfigSO
    {
        public string GameVersion = "0.1.0";
        public DifficultyLevel Difficulty = DifficultyLevel.Normal;

        // 难度调节系数
        public DifficultyModifiers Modifiers;

        [Serializable]
        public struct DifficultyModifiers
        {
            public float ResourceProductionRate;   // 资源产出倍率
            public float TemperatureDecayRate;     // 温度衰减速度
            public float SurvivorDamageRate;       // 幸存者受伤频率
            public float ExplorationEnemyPower;    // 探险敌人强度
            public float EventFrequencyRate;       // 随机事件频率
            public float UpgradeCostMultiplier;    // 升级消耗倍率
        }

        public static DifficultyModifiers GetModifiersForDifficulty(DifficultyLevel level)
        {
            return level switch
            {
                DifficultyLevel.Easy => new DifficultyModifiers
                {
                    ResourceProductionRate = 1.5f,
                    TemperatureDecayRate = 0.5f,
                    SurvivorDamageRate = 0.5f,
                    ExplorationEnemyPower = 0.7f,
                    EventFrequencyRate = 0.8f,
                    UpgradeCostMultiplier = 0.7f,
                },
                DifficultyLevel.Normal => new DifficultyModifiers
                {
                    ResourceProductionRate = 1.0f,
                    TemperatureDecayRate = 1.0f,
                    SurvivorDamageRate = 1.0f,
                    ExplorationEnemyPower = 1.0f,
                    EventFrequencyRate = 1.0f,
                    UpgradeCostMultiplier = 1.0f,
                },
                DifficultyLevel.Hard => new DifficultyModifiers
                {
                    ResourceProductionRate = 0.7f,
                    TemperatureDecayRate = 1.5f,
                    SurvivorDamageRate = 2.0f,
                    ExplorationEnemyPower = 1.5f,
                    EventFrequencyRate = 1.3f,
                    UpgradeCostMultiplier = 1.5f,
                },
                _ => new DifficultyModifiers(),
            };
        }
    }

    public enum DifficultyLevel
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
    }
}
