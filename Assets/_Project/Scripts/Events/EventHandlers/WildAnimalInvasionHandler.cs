using FrostShelter.Core;
using FrostShelter.Resource;

namespace FrostShelter.Events.EventHandlers
{
    /// <summary>
    /// 野兽入侵事件。冰原野兽攻击营地。
    /// </summary>
    public class WildAnimalInvasionHandler
    {
        private readonly Battle.BattleManager _battleManager;
        private readonly ResourceManager _resourceManager;
        private readonly Survivor.SurvivorManager _survivorManager;
        private readonly EventDispatcher _events;

        public WildAnimalInvasionHandler(Battle.BattleManager battleManager,
            ResourceManager resourceManager, Survivor.SurvivorManager survivorManager,
            EventDispatcher events)
        {
            _battleManager = battleManager;
            _resourceManager = resourceManager;
            _survivorManager = survivorManager;
            _events = events;
        }

        public void Handle(RandomEventSO evt, int choiceIndex)
        {
            switch (choiceIndex)
            {
                case 0: // 派兵迎战（触发战斗）
                    FightAnimals();
                    break;
                case 1: // 用食物引开（消耗食物，避免战斗）
                    _resourceManager.Spend(
                        new ResourceCost(ResourceType.RawMeat, 30f),
                        ResourceChangeReason.Event);
                    break;
                case 2: // 加固防御（消耗木材，减少损失）
                    _resourceManager.Spend(
                        new ResourceCost(ResourceType.Wood, 20f),
                        ResourceChangeReason.Event);
                    // 减少幸存者损伤
                    break;
            }
        }

        private void FightAnimals()
        {
            var playerFormation = CreateDefenseFormation();
            var enemyFormation = CreateAnimalFormation();
            _battleManager.StartAutoBattle(playerFormation, enemyFormation);

            _battleManager.OnBattleCompleted += (result) =>
            {
                if (result.IsPlayerVictory)
                {
                    _resourceManager.AddResource(ResourceType.RawMeat, 25f,
                        ResourceChangeReason.Event);
                }
                else
                {
                    // 防御失败，损失资源和幸存者
                    _resourceManager.RemoveResource(ResourceType.RawMeat, 10f,
                        ResourceChangeReason.Event);
                }
            };
        }

        private Battle.BattleFormation CreateDefenseFormation()
        {
            var formation = new Battle.BattleFormation();
            // 用现有英雄和兵种构建防御阵型
            return formation;
        }

        private Battle.BattleFormation CreateAnimalFormation()
        {
            var formation = new Battle.BattleFormation();
            for (int i = 0; i < 3; i++)
            {
                var unit = new Battle.BattleUnit
                {
                    InstanceId = System.Guid.NewGuid().ToString(),
                    DisplayName = $"冰原狼{i + 1}",
                    IsHero = false,
                    TroopType = Battle.TroopType.Spear,
                    Atk = 8f,
                    Def = 4f,
                    Hp = 40f,
                    MaxHp = 40f,
                    Speed = 7f,
                    PositionRow = 0,
                    PositionSlot = i,
                };
                formation.SetUnit(0, i, unit);
            }
            return formation;
        }
    }
}
