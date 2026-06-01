using FrostShelter.Core;
using FrostShelter.Resource;

namespace FrostShelter.Events.EventHandlers
{
    /// <summary>
    /// 探险中特有事件的处理器集合
    /// </summary>
    public class ExplorationEventHandlers
    {
        private readonly ResourceManager _resourceManager;
        private readonly Survivor.SurvivorManager _survivorManager;
        private readonly EventDispatcher _events;

        public ExplorationEventHandlers(ResourceManager resourceManager,
            Survivor.SurvivorManager survivorManager, EventDispatcher events)
        {
            _resourceManager = resourceManager;
            _survivorManager = survivorManager;
            _events = events;
        }

        /// <summary>冰窟事件：进入冰窟探索可能获得火晶或受伤</summary>
        public void HandleIceCave(RandomEventSO evt, int choiceIndex)
        {
            switch (choiceIndex)
            {
                case 0: // 深入探索（高风险高回报）
                    if (UnityEngine.Random.value < 0.4f)
                    {
                        _resourceManager.AddResource(ResourceType.FireCrystal, 3f,
                            ResourceChangeReason.Event);
                    }
                    break;
                case 1: // 谨慎搜索（低风险低回报）
                    _resourceManager.AddResource(ResourceType.Coal, 10f,
                        ResourceChangeReason.Event);
                    break;
                case 2: // 放弃探索
                    break;
            }
        }

        /// <summary>废弃避难所事件：可能找到幸存者或物资</summary>
        public void HandleAbandonedShelter(RandomEventSO evt, int choiceIndex)
        {
            switch (choiceIndex)
            {
                case 0: // 搜索物资
                    _resourceManager.AddResource(ResourceType.Wood, 15f,
                        ResourceChangeReason.Event);
                    _resourceManager.AddResource(ResourceType.RawMeat, 8f,
                        ResourceChangeReason.Event);
                    break;
                case 1: // 发出信号等待救援
                    if (UnityEngine.Random.value < 0.5f)
                    {
                        _survivorManager.AddRandomSurvivor();
                    }
                    break;
            }
        }

        /// <summary>信号塔事件：修复信号塔获得科技加成或英雄线索</summary>
        public void HandleSignalTower(RandomEventSO evt, int choiceIndex)
        {
            var cost = new ResourceCost();
            cost.Add(ResourceType.IronOre, 8f);
            if (_resourceManager.Spend(cost, ResourceChangeReason.Event))
            {
                switch (choiceIndex)
                {
                    case 0: // 发送求救信号
                        _survivorManager.AddRandomSurvivor();
                        break;
                    case 1: // 搜索信号记录
                        _resourceManager.AddResource(ResourceType.HeroSoulStone, 2,
                            ResourceChangeReason.Event);
                        break;
                }
            }
        }
    }
}
