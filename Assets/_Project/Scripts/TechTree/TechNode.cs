using System;
using System.Collections.Generic;
using FrostShelter.Resource;

namespace FrostShelter.TechTree
{
    /// <summary>
    /// 单个科技节点。
    /// </summary>
    [Serializable]
    public class TechNode
    {
        public string NodeId;
        public string DisplayName;
        public string Description;
        public TechColumn Column;
        public int Row;
        public int ColumnIndex;

        public List<string> PrerequisiteNodeIds = new();
        public ResourceCost ResearchCost = new();
        public float ResearchTimeSeconds = 60f;

        public List<TechEffect> Effects = new();

        public bool IsUnlocked;
        public bool IsResearched;
        public bool IsResearching;
        public float ResearchProgress;

        public bool PrerequisitesMet(List<TechNode> allNodes)
        {
            foreach (var prereqId in PrerequisiteNodeIds)
            {
                var prereq = allNodes.Find(n => n.NodeId == prereqId);
                if (prereq == null || !prereq.IsResearched)
                    return false;
            }
            return true;
        }
    }

    public enum TechColumn
    {
        Development = 0,
        Combat = 1,
        Economy = 2,
    }

    [Serializable]
    public struct TechEffect
    {
        public TechEffectType EffectType;
        public float Value;
        public string TargetId;
    }

    public enum TechEffectType
    {
        ProductionRate_Building,
        ProductionRate_All,
        StorageCapacity_Bonus,
        SurvivorEfficiency_Bonus,
        SurvivorMaxCapacity_Bonus,
        HeroExpGain_Bonus,
        TrainingSpeed_Bonus,
        ExplorationFoodEfficiency,
        ExplorationWarmthEfficiency,
        CombatDamage_Bonus_TroopType,
        CombatDefense_Bonus_TroopType,
        UnlockBuilding,
        UnlockRecipe,
    }
}
