using System;
using System.Collections.Generic;
using FrostShelter.SaveSystem;

namespace FrostShelter.Hero
{
    [Serializable]
    public class HeroConfigSO
    {
        public string HeroId;
        public string HeroName;
        public HeroQuality Quality;
        public HeroCategory Category;
        public string Description;

        // 基础属性
        public float BaseAtk;
        public float BaseDef;
        public float BaseHp;
        public float BaseSpeed;

        // 成长曲线（50级）
        public float[] AtkGrowth = new float[50];
        public float[] DefGrowth = new float[50];
        public float[] HpGrowth = new float[50];

        // 升星
        public int MaxStarLevel = 5;
        public int[] SoulStoneCostPerStar;

        // 技能
        public List<HeroSkillConfig> Skills = new();

        // 建筑加成（发展型英雄专用）
        public BuildingId BonusBuildingId;
        public float[] BuildingProductionBonusPerLevel;

        // 经验曲线
        public int[] ExpToLevel = new int[50];
    }

    [Serializable]
    public class HeroSkillConfig
    {
        public string SkillId;
        public string SkillName;
        public string Description;
        public SkillTriggerType TriggerType;
        public float[] ValuesPerLevel;
        public int CooldownTurns;
        public int UnlockStarLevel;
    }

    public enum SkillTriggerType
    {
        Passive,
        OnAttack,
        OnHurt,
        OnBattleStart,
        Manual,
    }
}
