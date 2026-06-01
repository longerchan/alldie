using System;
using System.Collections.Generic;

namespace FrostShelter.SaveSystem
{
    [Serializable]
    public class SaveData
    {
        public int dataVersion = Constants.SAVE_DATA_VERSION;
        public string saveTime;
        public float playTimeHours;

        public PlayerProgressData player = new();
        public ResourceStorageData resources = new();
        public TemperatureData temperature = new();
        public Dictionary<BuildingId, BuildingData> buildings = new();
        public List<SurvivorData> survivors = new();
        public List<HeroData> heroes = new();
        public TechTreeProgressData techTree = new();
        public ExplorationSaveData exploration = new();
        public StoryProgressData story = new();
        public List<RandomEventRecord> eventHistory = new();
        public MerchantData merchant = new();

        public long lastSaveUnixSeconds;
    }

    [Serializable]
    public class PlayerProgressData
    {
        public int furnaceLevel = 1;
        public int totalSurvivors;
        public int maxSurvivors = 4;
        public int completedChapters;
        public bool[] endingsUnlocked = new bool[3];
    }

    [Serializable]
    public class ResourceStorageData
    {
        public float rawMeat;
        public float wood;
        public float coal;
        public float ironOre;
        public float steel;
        public float fireCrystal;
        public int heroSoulStone;
        public int gems;

        public float GetAmount(ResourceType type)
        {
            return type switch
            {
                ResourceType.RawMeat => rawMeat,
                ResourceType.Wood => wood,
                ResourceType.Coal => coal,
                ResourceType.IronOre => ironOre,
                ResourceType.Steel => steel,
                ResourceType.FireCrystal => fireCrystal,
                ResourceType.HeroSoulStone => heroSoulStone,
                ResourceType.Gems => gems,
                _ => 0f,
            };
        }

        public void SetAmount(ResourceType type, float value)
        {
            switch (type)
            {
                case ResourceType.RawMeat: rawMeat = value; break;
                case ResourceType.Wood: wood = value; break;
                case ResourceType.Coal: coal = value; break;
                case ResourceType.IronOre: ironOre = value; break;
                case ResourceType.Steel: steel = value; break;
                case ResourceType.FireCrystal: fireCrystal = value; break;
                case ResourceType.HeroSoulStone: heroSoulStone = (int)value; break;
                case ResourceType.Gems: gems = (int)value; break;
            }
        }

        public void AddAmount(ResourceType type, float amount)
        {
            SetAmount(type, GetAmount(type) + amount);
        }
    }

    [Serializable]
    public class TemperatureData
    {
        public float currentTemperature = Constants.BASE_TEMPERATURE;
        public bool isBlizzardActive;
        public float blizzardRemainingSeconds;
    }

    [Serializable]
    public class BuildingData
    {
        public BuildingId id;
        public int level = 0;
        public float accumulatedProduction;
        public float productionMultiplier = 1f;
        public long lastCollectTimestamp;
        public List<string> assignedSurvivorIds = new();
        public string assignedHeroId;
        public bool isUpgrading;
        public long upgradeFinishTimestamp;
        public bool isUnlocked;
    }

    [Serializable]
    public class SurvivorData
    {
        public string id;
        public string name;
        public string occupationTag;
        public float satisfaction = 100f;
        public float health = 100f;
        public float mood = 100f;
        public float efficiencyBonus;
        public bool isSick;
        public bool isInExpedition;
        public string assignedBuildingId;
        public int daysSurvived;
    }

    [Serializable]
    public class HeroData
    {
        public string id;
        public string configId;
        public HeroQuality quality = HeroQuality.Blue;
        public int level = 1;
        public int currentExp;
        public int starLevel;
        public int soulStoneInvested;
        public List<HeroSkillSaveData> skills = new();
        public HeroCategory category = HeroCategory.Development;
        public string assignedBuildingId;
        public bool isInExpedition;
        public int formationSlotIndex = -1;
    }

    [Serializable]
    public class HeroSkillSaveData
    {
        public string skillId;
        public int currentLevel = 1;
        public bool isUnlocked;
    }

    [Serializable]
    public class TechTreeProgressData
    {
        public List<string> unlockedNodeIds = new();
        public List<string> researchingNodeIds = new();
        public Dictionary<string, float> researchProgress = new();
    }

    [Serializable]
    public class ExplorationSaveData
    {
        public bool isActive;
        public List<string> teamHeroIds = new();
        public List<TroopSaveData> teamTroops = new();
        public float foodSupply;
        public float warmthSupply;
        public int gridSize = 8;
        public List<HexNodeSaveData> discoveredNodes = new();
        public List<HexCoordSaveData> pathHistory = new();
        public HexCoordSaveData currentPosition;
        public int seed;
    }

    [Serializable]
    public class HexNodeSaveData
    {
        public int q;
        public int r;
        public int nodeType;
        public bool isRevealed;
        public bool isVisited;
        public int difficultyLevel;
    }

    [Serializable]
    public struct HexCoordSaveData
    {
        public int q;
        public int r;
    }

    [Serializable]
    public class TroopSaveData
    {
        public string configId;
        public TroopTypeSave troopType;
        public int count;
        public int aliveCount;
    }

    public enum TroopTypeSave { Shield, Spear, Archer }

    [Serializable]
    public class StoryProgressData
    {
        public int currentChapter;
        public int currentNodeId;
        public List<int> completedNodeIds = new();
        public Dictionary<int, int> branchChoices = new();
    }

    [Serializable]
    public class RandomEventRecord
    {
        public string eventId;
        public long triggeredAt;
        public int choiceIndex = -1;
        public bool isResolved;
    }

    [Serializable]
    public class MerchantData
    {
        public long lastRefreshTimestamp;
        public List<string> currentItemIds = new();
        public int refreshCount;
    }
}
