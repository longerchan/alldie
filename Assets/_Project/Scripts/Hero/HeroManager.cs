using System;
using System.Collections.Generic;
using System.Linq;
using FrostShelter.Core;
using FrostShelter.Resource;

namespace FrostShelter.Hero
{
    /// <summary>
    /// 英雄管理器。负责英雄招募、升级、升星、驻守分配。
    /// </summary>
    public class HeroManager : IService
    {
        private readonly List<Hero> _heroes = new();
        private readonly Dictionary<string, HeroConfigSO> _configTable = new();

        private EventDispatcher _events;
        private ResourceManager _resourceManager;

        public IReadOnlyList<Hero> AllHeroes => _heroes;

        public event Action<Hero> OnHeroRecruited;
        public event Action<Hero, int> OnHeroLevelUp;
        public event Action<Hero, int> OnHeroStarUp;
        public event Action<Hero, string> OnHeroSkillUnlocked;

        public void Initialize() { }

        public void Shutdown()
        {
            _heroes.Clear();
            _configTable.Clear();
        }

        public void SetDependencies(EventDispatcher events, ResourceManager resourceManager)
        {
            _events = events;
            _resourceManager = resourceManager;
        }

        public void RegisterConfig(HeroConfigSO config)
        {
            _configTable[config.HeroId] = config;
        }

        public Hero RecruitHero(string configId)
        {
            if (!_configTable.TryGetValue(configId, out var config))
                return null;

            var hero = new Hero(Guid.NewGuid().ToString(), configId, config);
            _heroes.Add(hero);

            OnHeroRecruited?.Invoke(hero);
            _events?.Dispatch(GameEventType.HeroRecruited);
            return hero;
        }

        public Hero GetHero(string heroId)
        {
            return _heroes.Find(h => h.Id == heroId);
        }

        public List<Hero> GetHeroesByCategory(HeroCategory category)
        {
            return _heroes.Where(h => h.Category == category).ToList();
        }

        public List<Hero> GetAvailableDevelopmentHeroes()
        {
            return _heroes.Where(h =>
                h.Category == HeroCategory.Development &&
                string.IsNullOrEmpty(h.AssignedBuildingId) &&
                !h.IsInExpedition).ToList();
        }

        public List<Hero> GetAvailableCombatHeroes()
        {
            return _heroes.Where(h =>
                h.Category == HeroCategory.Combat &&
                !h.IsInExpedition).ToList();
        }

        public void AddExperience(string heroId, int expAmount)
        {
            var hero = GetHero(heroId);
            if (hero == null || hero.IsMaxLevel) return;

            int oldLevel = hero.Level;
            hero.AddExp(expAmount);

            if (hero.Level > oldLevel)
            {
                OnHeroLevelUp?.Invoke(hero, hero.Level);
                _events?.Dispatch(GameEventType.HeroLevelUp);
            }
        }

        public bool TryStarUp(string heroId)
        {
            var hero = GetHero(heroId);
            if (hero == null || hero.IsMaxStar) return false;

            int cost = hero.Config?.SoulStoneCostPerStar != null
                && hero.StarLevel < hero.Config.SoulStoneCostPerStar.Length
                ? hero.Config.SoulStoneCostPerStar[hero.StarLevel]
                : 10;

            if (_resourceManager.GetAmount(ResourceType.HeroSoulStone) < cost)
                return false;

            _resourceManager.RemoveResource(ResourceType.HeroSoulStone, cost,
                ResourceChangeReason.Upgrade);

            int oldStar = hero.StarLevel;
            // Check for skill unlocks
            var unlockedSkills = new List<string>();
            foreach (var skill in hero.Skills)
            {
                if (!skill.IsUnlocked && skill.Config.UnlockStarLevel <= hero.StarLevel + 1)
                {
                    unlockedSkills.Add(skill.SkillId);
                }
            }

            hero.TryStarUp();

            foreach (var skillId in unlockedSkills)
            {
                OnHeroSkillUnlocked?.Invoke(hero, skillId);
                _events?.Dispatch(GameEventType.HeroSkillUnlocked);
            }

            OnHeroStarUp?.Invoke(hero, hero.StarLevel);
            _events?.Dispatch(GameEventType.HeroStarUp);
            return true;
        }

        public void AssignToBuilding(string heroId, BuildingId buildingId)
        {
            var hero = GetHero(heroId);
            if (hero == null || hero.Category != HeroCategory.Development) return;
            hero.AssignedBuildingId = buildingId.ToString();
        }

        public void UnassignFromBuilding(string heroId)
        {
            var hero = GetHero(heroId);
            if (hero != null) hero.AssignedBuildingId = null;
        }

        public float GetBuildingBonus(BuildingId buildingId)
        {
            float totalBonus = 0f;
            foreach (var hero in _heroes)
            {
                if (hero.Category == HeroCategory.Development &&
                    hero.AssignedBuildingId == buildingId.ToString())
                {
                    totalBonus += hero.GetBuildingProductionBonus(buildingId);
                }
            }
            return totalBonus;
        }

        public void LoadFromSaveData(List<HeroData> dataList)
        {
            _heroes.Clear();
            foreach (var data in dataList)
            {
                if (_configTable.TryGetValue(data.configId, out var config))
                {
                    var hero = Hero.FromSaveData(data, config);
                    _heroes.Add(hero);
                }
            }
        }

        public List<HeroData> ToSaveData()
        {
            return _heroes.Select(h => h.ToSaveData()).ToList();
        }
    }
}
