using FrostShelter.SaveSystem;
using FrostShelter.Core;
using FrostShelter.Resource;

namespace FrostShelter.Events.EventHandlers
{
    /// <summary>
    /// 疫病事件处理器。营地爆发疾病，消耗医疗资源。
    /// </summary>
    public class PlagueEventHandler
    {
        private readonly Survivor.SurvivorManager _survivorManager;
        private readonly ResourceManager _resourceManager;
        private readonly EventDispatcher _events;

        public PlagueEventHandler(Survivor.SurvivorManager survivorManager,
            ResourceManager resourceManager, EventDispatcher events)
        {
            _survivorManager = survivorManager;
            _resourceManager = resourceManager;
            _events = events;
        }

        public void Handle(RandomEventSO evt, int choiceIndex)
        {
            switch (choiceIndex)
            {
                case 0: // 隔离病人（消耗木+煤，减少传播）
                    if (_resourceManager.Spend(
                        new ResourceCost(ResourceType.Wood, 10f),
                        ResourceChangeReason.Event))
                    {
                        // 随机感染30%幸存者
                        InfectSurvivors(0.3f);
                    }
                    break;
                case 1: // 使用医疗资源（消耗铁矿+煤，完全治愈）
                    var cost = new ResourceCost();
                    cost.Add(ResourceType.IronOre, 5f);
                    cost.Add(ResourceType.Coal, 8f);
                    if (_resourceManager.Spend(cost, ResourceChangeReason.Event))
                    {
                        // 治愈所有幸存者
                        foreach (var survivor in _survivorManager.AllSurvivors)
                        {
                            if (survivor.IsSick)
                            {
                                survivor.IsSick = false;
                                survivor.ModifyHealth(10f);
                            }
                        }
                    }
                    break;
                case 2: // 放任不管（50%幸存者感染，满意度下降）
                    InfectSurvivors(0.5f);
                    _survivorManager.BoostMorale(-10f);
                    break;
            }
        }

        private void InfectSurvivors(float ratio)
        {
            var survivors = _survivorManager.AllSurvivors;
            int count = (int)(survivors.Count * ratio);
            for (int i = 0; i < count && i < survivors.Count; i++)
            {
                survivors[i].IsSick = true;
                survivors[i].ModifyHealth(-5f);
                survivors[i].ModifySatisfaction(-10f);
            }
            _events?.Dispatch(GameEventType.SurvivorSatisfactionChanged);
        }
    }
}
