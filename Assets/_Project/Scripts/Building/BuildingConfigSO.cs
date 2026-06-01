using System;
using UnityEngine;
using FrostShelter.SaveSystem;

namespace FrostShelter.Building
{
    /// <summary>
    /// 建筑配置 ScriptableObject。在 Unity Editor 中创建和编辑。
    /// </summary>
    [Serializable]
    public class BuildingConfigSO : ScriptableObject
    {
        public BuildingId BuildingId;
        public string DisplayName;
        public string Description;
        public bool IsCore;               // 核心建筑不可拆除
        public bool IsProduction;         // 是否为生产型建筑
        public int UnlockFurnaceLevel;    // 解锁所需熔炉等级
        public ResourceType OutputResource;
        public BuildingLevelConfig[] Levels;
    }

    [Serializable]
    public class BuildingLevelConfig
    {
        public int Level;
        public int UpgradeCostWood;
        public int UpgradeCostRawMeat;
        public int UpgradeCostCoal;
        public int UpgradeCostIron;
        public int UpgradeCostSteel;
        public int UpgradeCostFireCrystal;
        public float UpgradeTimeSeconds;
        public int RequiredFurnaceLevel;

        // 生产型建筑专用
        public float BaseProductionPerHour;
        public ResourceType OutputResource;

        // 住宅专用
        public int MaxResidents;

        // 仓库专用
        public float StorageCapacity;

        // 兵营专用
        public int MaxTrainingQueue;
        public float TrainingSpeedMultiplier;

        // 医疗帐篷专用
        public float HealRatePerHour;

        // 酒馆专用
        public int DailyRecruitCharges;

        // 温暖工坊专用
        public int CraftQueueSize;
    }
}
