using System;
using System.Collections.Generic;
using FrostShelter.Core;
using FrostShelter.Resource;

namespace FrostShelter.Exploration.NodeTypes
{
    /// <summary>
    /// 战斗节点控制器。触发探险中的遭遇战斗。
    /// </summary>
    public class BattleNodeController
    {
        public HexNode Node { get; private set; }
        private Battle.BattleManager _battleManager;
        private ResourceManager _resourceManager;
        private EventDispatcher _events;

        public event Action<Battle.BattleResult> OnBattleFinished;

        public BattleNodeController(HexNode node, Battle.BattleManager battleManager,
            ResourceManager resourceManager, EventDispatcher events)
        {
            Node = node;
            _battleManager = battleManager;
            _resourceManager = resourceManager;
            _events = events;
        }

        public void OnEnter(Battle.BattleFormation playerFormation)
        {
            var enemyFormation = GenerateEnemyFormation();
            _battleManager.StartRogueBattle(playerFormation, enemyFormation);
            _battleManager.OnBattleCompleted += HandleBattleResult;
        }

        private Battle.BattleFormation GenerateEnemyFormation()
        {
            var formation = new Battle.BattleFormation();
            var data = Node.BattleData;
            int difficulty = Node.DifficultyLevel;

            // 根据难度和敌人配置生成敌方阵型
            for (int i = 0; i < Math.Min(3, data.EnemyCount); i++)
            {
                var unit = new Battle.BattleUnit
                {
                    InstanceId = Guid.NewGuid().ToString(),
                    DisplayName = $"冰原狼 Lv.{data.EnemyLevel}",
                    IsHero = false,
                    TroopType = (Battle.TroopType)(i % 3),
                    Atk = 10f * difficulty,
                    Def = 5f * difficulty,
                    Hp = 50f * difficulty,
                    MaxHp = 50f * difficulty,
                    Speed = 5f + difficulty,
                    PositionRow = 0,
                    PositionSlot = i,
                };
                formation.SetUnit(0, i, unit);
            }

            return formation;
        }

        private void HandleBattleResult(Battle.BattleResult result)
        {
            _battleManager.OnBattleCompleted -= HandleBattleResult;

            if (result.IsPlayerVictory)
            {
                // 掉落奖励
                float lootAmount = 10f * Node.DifficultyLevel * Node.BattleData.LootMultiplier;
                _resourceManager.AddResource(ResourceType.Wood, lootAmount,
                    ResourceChangeReason.Expedition);
            }

            OnBattleFinished?.Invoke(result);
        }
    }
}
