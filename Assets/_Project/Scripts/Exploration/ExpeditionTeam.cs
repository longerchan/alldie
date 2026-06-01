using System;
using System.Collections.Generic;
using System.Linq;
using FrostShelter.Battle;
using FrostShelter.Core;

namespace FrostShelter.Exploration
{
    /// <summary>
    /// 探险队。管理探险中的英雄编队、兵种、供应品。
    /// </summary>
    [Serializable]
    public class ExpeditionTeam
    {
        public List<string> HeroIds = new(Constants.MAX_EXPLORATION_HEROES);
        public List<Battle.TroopData> Troops = new(6);

        public float Food;
        public float Warmth;
        public float FoodConsumePerStep;
        public float WarmthConsumePerStep;

        public bool HasEnoughFood => Food > 0f;
        public bool HasEnoughWarmth => Warmth > 0f;
        public bool CanContinue => HasEnoughFood && HasEnoughWarmth;

        public int TotalPower
        {
            get
            {
                int power = 0;
                foreach (var troop in Troops)
                {
                    if (troop != null)
                        power += (int)(troop.Atk * troop.AliveCount * 1.5f
                            + troop.Def * troop.AliveCount);
                }
                return power;
            }
        }

        public void AddHero(string heroId)
        {
            if (HeroIds.Count >= Constants.MAX_EXPLORATION_HEROES) return;
            if (!HeroIds.Contains(heroId))
                HeroIds.Add(heroId);
        }

        public void RemoveHero(string heroId)
        {
            HeroIds.Remove(heroId);
        }

        public void AddTroop(Battle.TroopData troop)
        {
            if (Troops.Count >= 6) return;
            Troops.Add(troop);
        }

        public void ConsumeStep()
        {
            Food = Math.Max(0f, Food - FoodConsumePerStep);
            Warmth = Math.Max(0f, Warmth - WarmthConsumePerStep);
        }

        public int GetAliveUnitCount()
        {
            return Troops.Sum(t => t?.AliveCount ?? 0) + HeroIds.Count;
        }

        public BattleFormation ToBattleFormation()
        {
            // 委托 BattleManager.BuildFormationFromHeroes
            var formation = new BattleFormation();
            int frontIdx = 0, backIdx = 0;

            foreach (var troop in Troops)
            {
                if (troop == null) continue;
                var unit = CreateTroopUnit(troop);
                if (troop.PositionRow == 0 && frontIdx < 3)
                    formation.SetUnit(0, frontIdx++, unit);
                else if (backIdx < 3)
                    formation.SetUnit(1, backIdx++, unit);
            }

            return formation;
        }

        private BattleUnit CreateTroopUnit(Battle.TroopData troop)
        {
            return new BattleUnit
            {
                InstanceId = Guid.NewGuid().ToString(),
                DisplayName = troop.ConfigId,
                IsHero = false,
                TroopType = troop.TroopType,
                Atk = troop.Atk * troop.AliveCount,
                Def = troop.Def,
                Hp = troop.TotalHp,
                MaxHp = troop.MaxHp,
                Speed = troop.Speed,
            };
        }
    }
}
