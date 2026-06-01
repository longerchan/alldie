using System;
using System.Collections.Generic;
using FrostShelter.SaveSystem;
using FrostShelter.Resource;

namespace FrostShelter.Building
{
    /// <summary>
    /// 建筑抽象基类。所有13种具体建筑类型继承此类。
    /// </summary>
    public abstract class BuildingBase
    {
        public BuildingId Id { get; protected set; }
        public int Level { get; protected set; }
        public BuildingConfigSO Config { get; protected set; }
        public bool IsUpgrading { get; set; }
        public float UpgradeProgress { get; protected set; }
        public DateTime UpgradeFinishTime { get; protected set; }

        public List<string> AssignedSurvivorIds { get; protected set; } = new();
        public string AssignedHeroId { get; protected set; }

        // 效率加成
        public float SurvivorEfficiencyBonus { get; protected set; }
        public float HeroEfficiencyBonus { get; protected set; }
        public float TechEfficiencyBonus { get; set; }

        protected BuildingBase() { }

        public virtual void Initialize(BuildingId id, BuildingConfigSO config, int initialLevel = 0)
        {
            Id = id;
            Config = config;
            Level = initialLevel;
        }

        public BuildingLevelConfig CurrentLevelConfig
        {
            get
            {
                if (Config?.Levels == null || Level < 0 || Level >= Config.Levels.Length)
                    return null;
                return Config.Levels[Level];
            }
        }

        public BuildingLevelConfig NextLevelConfig
        {
            get
            {
                if (Config?.Levels == null || Level + 1 >= Config.Levels.Length)
                    return null;
                return Config.Levels[Level + 1];
            }
        }

        public bool IsMaxLevel => Config?.Levels != null && Level >= Config.Levels.Length - 1;

        public float TotalEfficiencyMultiplier()
        {
            return 1f + SurvivorEfficiencyBonus + HeroEfficiencyBonus + TechEfficiencyBonus;
        }

        public ResourceCost GetNextUpgradeCost()
        {
            var next = NextLevelConfig;
            if (next == null) return null;

            var cost = new ResourceCost();
            if (next.UpgradeCostWood > 0) cost.Add(ResourceType.Wood, next.UpgradeCostWood);
            if (next.UpgradeCostRawMeat > 0) cost.Add(ResourceType.RawMeat, next.UpgradeCostRawMeat);
            if (next.UpgradeCostCoal > 0) cost.Add(ResourceType.Coal, next.UpgradeCostCoal);
            if (next.UpgradeCostIron > 0) cost.Add(ResourceType.IronOre, next.UpgradeCostIron);
            if (next.UpgradeCostSteel > 0) cost.Add(ResourceType.Steel, next.UpgradeCostSteel);
            if (next.UpgradeCostFireCrystal > 0) cost.Add(ResourceType.FireCrystal, next.UpgradeCostFireCrystal);
            return cost;
        }

        public virtual void OnLevelUp(int newLevel)
        {
            Level = newLevel;
            OnUpgradeComplete();
        }

        public virtual void OnUpgradeComplete()
        {
            IsUpgrading = false;
            UpgradeProgress = 0f;
        }

        public virtual void AssignSurvivor(string survivorId, float efficiencyBonus)
        {
            if (!AssignedSurvivorIds.Contains(survivorId))
            {
                AssignedSurvivorIds.Add(survivorId);
                SurvivorEfficiencyBonus += efficiencyBonus;
            }
        }

        public virtual void UnassignSurvivor(string survivorId, float efficiencyBonus)
        {
            if (AssignedSurvivorIds.Remove(survivorId))
            {
                SurvivorEfficiencyBonus -= efficiencyBonus;
            }
        }

        public virtual void AssignHero(string heroId, float efficiencyBonus)
        {
            AssignedHeroId = heroId;
            HeroEfficiencyBonus = efficiencyBonus;
        }

        public virtual void UnassignHero()
        {
            AssignedHeroId = null;
            HeroEfficiencyBonus = 0f;
        }

        public virtual string GetStatusDescription() => $"Lv.{Level}";

        public BuildingData ToSaveData()
        {
            return new BuildingData
            {
                id = Id,
                level = Level,
                accumulatedProduction = (this as ProductionBuilding)?.AccumulatedProduction ?? 0f,
                productionMultiplier = TotalEfficiencyMultiplier(),
                assignedSurvivorIds = new List<string>(AssignedSurvivorIds),
                assignedHeroId = AssignedHeroId,
                isUpgrading = IsUpgrading,
                upgradeFinishTimestamp = IsUpgrading
                    ? new DateTimeOffset(UpgradeFinishTime).ToUnixTimeSeconds()
                    : 0,
                isUnlocked = Level >= 0,
            };
        }

        public void LoadFromSaveData(BuildingData data)
        {
            Level = data.level;
            AssignedSurvivorIds = new List<string>(data.assignedSurvivorIds ?? new List<string>());
            AssignedHeroId = data.assignedHeroId;
            IsUpgrading = data.isUpgrading;
            if (data.isUpgrading && data.upgradeFinishTimestamp > 0)
            {
                UpgradeFinishTime = DateTimeOffset.FromUnixTimeSeconds(data.upgradeFinishTimestamp).DateTime;
            }
        }
    }
}
