using FrostShelter.SaveSystem;
using System;
using System.Collections.Generic;
using FrostShelter.Core;
using FrostShelter.Resource;

namespace FrostShelter.Exploration.NodeTypes
{
    /// <summary>
    /// Boss节点控制器。本张地图的最终敌人，击败后大量奖励+推进主线。
    /// </summary>
    public class BossNodeController
    {
        public HexNode Node { get; private set; }
        private Battle.BattleManager _battleManager;
        private ResourceManager _resourceManager;
        private EventDispatcher _events;

        public event Action<bool> OnBossDefeated;

        public BossNodeController(HexNode node, Battle.BattleManager battleManager,
            ResourceManager resourceManager, EventDispatcher events)
        {
            Node = node;
            _battleManager = battleManager;
            _resourceManager = resourceManager;
            _events = events;
        }

        public void OnEnter(Battle.BattleFormation playerFormation)
        {
            var bossFormation = GenerateBossFormation();
            _battleManager.StartRogueBattle(playerFormation, bossFormation);
            _battleManager.OnBattleCompleted += HandleBossResult;
        }

        private Battle.BattleFormation GenerateBossFormation()
        {
            var formation = new Battle.BattleFormation();
            int difficulty = Node.DifficultyLevel;
            string bossName = GetBossName();
            var bossTypes = GetBossTroopTypes();

            for (int i = 0; i < bossTypes.Length; i++)
            {
                var unit = new Battle.BattleUnit
                {
                    InstanceId = Guid.NewGuid().ToString(),
                    DisplayName = i == 0 ? bossName : $"精英 {bossTypes[i]}兵",
                    IsHero = true,
                    TroopType = bossTypes[i],
                    Atk = 15f * difficulty,
                    Def = 10f * difficulty,
                    Hp = 100f * difficulty,
                    MaxHp = 100f * difficulty,
                    Speed = 8f,
                    PositionRow = i < 2 ? 0 : 1,
                    PositionSlot = i < 2 ? i : i - 2,
                    CritRate = 0.15f + difficulty * 0.02f,
                    CritDamage = 2f,
                };
                formation.SetUnit(unit.PositionRow, unit.PositionSlot, unit);
            }

            return formation;
        }

        private string GetBossName()
        {
            return Node.BattleData?.EnemyConfigId switch
            {
                _ => "变异冰原巨熊",
            };
        }

        private Battle.TroopType[] GetBossTroopTypes()
        {
            return Node.DifficultyLevel switch
            {
                >= 5 => new[] { Battle.TroopType.Shield, Battle.TroopType.Shield,
                    Battle.TroopType.Archer, Battle.TroopType.Spear },
                _ => new[] { Battle.TroopType.Shield, Battle.TroopType.Spear,
                    Battle.TroopType.Archer },
            };
        }

        private void HandleBossResult(Battle.BattleResult result)
        {
            _battleManager.OnBattleCompleted -= HandleBossResult;

            if (result.IsPlayerVictory)
            {
                // Boss掉落大量奖励
                float multiplier = Node.DifficultyLevel;
                _resourceManager.AddResource(ResourceType.Steel, 5f * multiplier,
                    ResourceChangeReason.Expedition);
                _resourceManager.AddResource(ResourceType.FireCrystal, 2f * multiplier,
                    ResourceChangeReason.Expedition);
                _resourceManager.AddResource(ResourceType.HeroSoulStone, 3 * Node.DifficultyLevel,
                    ResourceChangeReason.Expedition);
            }

            OnBossDefeated?.Invoke(result.IsPlayerVictory);
        }
    }
}
