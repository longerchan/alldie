using FrostShelter.SaveSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostShelter.Hero
{
    /// <summary>
    /// 英雄实体。管理等级、星级、技能、属性计算。
    /// 数据源自 HeroConfigSO，运行时状态存储在 HeroData。
    /// </summary>
    public class Hero
    {
        public string Id { get; private set; }
        public string ConfigId { get; private set; }
        public HeroConfigSO Config { get; private set; }
        public HeroQuality Quality => Config?.Quality ?? HeroQuality.Blue;
        public HeroCategory Category => Config?.Category ?? HeroCategory.Development;

        public int Level { get; private set; } = 1;
        public int CurrentExp { get; private set; }
        public int StarLevel { get; private set; }
        public int SoulStoneInvested { get; set; }

        public List<HeroSkill> Skills { get; private set; } = new();
        public bool IsMaxLevel => Level >= Core.Constants.MAX_HERO_LEVEL;
        public bool IsMaxStar => Config != null && StarLevel >= Config.MaxStarLevel;

        // 计算属性
        public float Atk => CalculateAttrib(Config?.BaseAtk ?? 0, Config?.AtkGrowth);
        public float Def => CalculateAttrib(Config?.BaseDef ?? 0, Config?.DefGrowth);
        public float Hp => CalculateAttrib(Config?.BaseHp ?? 0, Config?.HpGrowth);
        public float Speed => Config?.BaseSpeed ?? 0;

        public float Power => (Atk * 1.5f + Def + Hp * 0.3f) * (1f + StarLevel * 0.15f);

        public string AssignedBuildingId { get; set; }
        public bool IsInExpedition { get; set; }
        public int FormationSlotIndex { get; set; } = -1;

        public Hero(string id, string configId, HeroConfigSO config)
        {
            Id = id;
            ConfigId = configId;
            Config = config;
            Level = 1;
            StarLevel = 0;
            InitializeSkills();
        }

        private void InitializeSkills()
        {
            Skills.Clear();
            if (Config?.Skills == null) return;

            foreach (var skillConfig in Config.Skills)
            {
                Skills.Add(new HeroSkill
                {
                    SkillId = skillConfig.SkillId,
                    Config = skillConfig,
                    CurrentLevel = 1,
                    IsUnlocked = skillConfig.UnlockStarLevel <= StarLevel,
                });
            }
        }

        private float CalculateAttrib(float baseValue, float[] growth)
        {
            if (growth == null || growth.Length == 0) return baseValue;
            int index = Math.Min(Level - 1, growth.Length - 1);
            return baseValue + growth[index];
        }

        public void AddExp(int amount)
        {
            if (IsMaxLevel) return;
            CurrentExp += amount;

            // 自动升级
            while (!IsMaxLevel && CurrentExp >= ExpToNextLevel)
            {
                CurrentExp -= ExpToNextLevel;
                LevelUp();
            }
        }

        public int ExpToNextLevel
        {
            get
            {
                if (Config?.ExpToLevel == null || Level > Config.ExpToLevel.Length) return int.MaxValue;
                return Config.ExpToLevel[Level - 1];
            }
        }

        public int StoredExp => CurrentExp;
        public float LevelProgress => ExpToNextLevel > 0
            ? (float)CurrentExp / ExpToNextLevel
            : 1f;

        private void LevelUp()
        {
            int oldLevel = Level;
            Level = Math.Min(Level + 1, Core.Constants.MAX_HERO_LEVEL);
        }

        public bool TryStarUp()
        {
            if (IsMaxStar) return false;
            int cost = Config?.SoulStoneCostPerStar != null
                && StarLevel < Config.SoulStoneCostPerStar.Length
                ? Config.SoulStoneCostPerStar[StarLevel]
                : 10;

            StarLevel++;
            SoulStoneInvested += cost;

            // 检查技能解锁
            foreach (var skill in Skills)
            {
                if (!skill.IsUnlocked && skill.Config.UnlockStarLevel <= StarLevel)
                {
                    skill.IsUnlocked = true;
                }
            }

            return true;
        }

        public HeroSkill GetSkill(string skillId)
        {
            return Skills.Find(s => s.SkillId == skillId);
        }

        public HeroSkill GetReadySkill()
        {
            return Skills.Find(s => s.IsUnlocked && s.IsReady);
        }

        public void OnTurnEnd()
        {
            foreach (var skill in Skills)
            {
                skill.TickCooldown();
            }
        }

        /// <summary>发展型英雄的建筑物生产效率加成</summary>
        public float GetBuildingProductionBonus(BuildingId buildingId)
        {
            if (Category != HeroCategory.Development) return 0f;
            if (Config?.BonusBuildingId != buildingId) return 0f;

            int index = Math.Min(Level - 1,
                (Config.BuildingProductionBonusPerLevel?.Length ?? 1) - 1);
            if (index < 0) return 0f;

            return Config.BuildingProductionBonusPerLevel?[index] ?? 0f;
        }

        public HeroData ToSaveData()
        {
            return new HeroData
            {
                id = Id,
                configId = ConfigId,
                quality = Quality,
                level = Level,
                currentExp = CurrentExp,
                starLevel = StarLevel,
                soulStoneInvested = SoulStoneInvested,
                skills = Skills.Select(s => new HeroSkillSaveData
                {
                    skillId = s.SkillId,
                    currentLevel = s.CurrentLevel,
                    isUnlocked = s.IsUnlocked,
                }).ToList(),
                category = Category,
                assignedBuildingId = AssignedBuildingId,
                isInExpedition = IsInExpedition,
                formationSlotIndex = FormationSlotIndex,
            };
        }

        public static Hero FromSaveData(HeroData data, HeroConfigSO config)
        {
            var hero = new Hero(data.id, data.configId, config)
            {
                Level = data.level,
                CurrentExp = data.currentExp,
                StarLevel = data.starLevel,
                SoulStoneInvested = data.soulStoneInvested,
                AssignedBuildingId = data.assignedBuildingId,
                IsInExpedition = data.isInExpedition,
                FormationSlotIndex = data.formationSlotIndex,
            };

            // 恢复技能状态
            foreach (var skillData in data.skills)
            {
                var skill = hero.GetSkill(skillData.skillId);
                if (skill != null)
                {
                    skill.CurrentLevel = skillData.currentLevel;
                    skill.IsUnlocked = skillData.isUnlocked;
                }
            }

            return hero;
        }
    }

    public class HeroSkill
    {
        public string SkillId;
        public HeroSkillConfig Config;
        public int CurrentLevel = 1;
        public bool IsUnlocked;
        public int CurrentCooldown;

        public bool IsReady => IsUnlocked && CurrentCooldown <= 0;
        public int MaxLevel => Config?.ValuesPerLevel?.Length ?? 1;

        public float GetCurrentValue()
        {
            if (Config?.ValuesPerLevel == null || Config.ValuesPerLevel.Length == 0) return 0f;
            int index = Math.Min(CurrentLevel - 1, Config.ValuesPerLevel.Length - 1);
            return Config.ValuesPerLevel[index];
        }

        public void Use()
        {
            CurrentCooldown = Config?.CooldownTurns ?? 0;
        }

        public void TickCooldown()
        {
            if (CurrentCooldown > 0) CurrentCooldown--;
        }

        public void ResetCooldown()
        {
            CurrentCooldown = 0;
        }
    }
}
