using FrostShelter.SaveSystem;
using FrostShelter.Core;
using FrostShelter.Resource;

namespace FrostShelter.Events.EventHandlers
{
    /// <summary>
    /// 救援求助事件处理器。消耗资源获得幸存者或英雄。
    /// </summary>
    public class RescueEventHandler
    {
        private readonly Survivor.SurvivorManager _survivorManager;
        private readonly Hero.HeroManager _heroManager;
        private readonly ResourceManager _resourceManager;
        private readonly EventDispatcher _events;

        public RescueEventHandler(Survivor.SurvivorManager survivorManager,
            Hero.HeroManager heroManager, ResourceManager resourceManager, EventDispatcher events)
        {
            _survivorManager = survivorManager;
            _heroManager = heroManager;
            _resourceManager = resourceManager;
            _events = events;
        }

        public void Handle(RandomEventSO evt, int choiceIndex)
        {
            var choice = evt.Choices[choiceIndex];
            // 救援事件选项：
            // 0: 派出救援队（消耗食物+木材，获得幸存者+可能英雄线索）
            // 1: 发送信号（消耗少量资源，获得幸存者）
            // 2: 无视（什么都不做）

            switch (choiceIndex)
            {
                case 0:
                    var cost = new ResourceCost();
                    cost.Add(ResourceType.RawMeat, 10f);
                    cost.Add(ResourceType.Wood, 15f);
                    if (_resourceManager.Spend(cost, ResourceChangeReason.Event))
                    {
                        _survivorManager.AddRandomSurvivor();
                        // 20%概率获得英雄线索
                        if (UnityEngine.Random.value < 0.2f)
                        {
                            _survivorManager.AddRandomSurvivor();
                        }
                    }
                    break;
                case 1:
                    var signalCost = new ResourceCost();
                    signalCost.Add(ResourceType.Wood, 5f);
                    if (_resourceManager.Spend(signalCost, ResourceChangeReason.Event))
                    {
                        _survivorManager.AddRandomSurvivor();
                    }
                    break;
                case 2:
                    // 什么都不做，可能降低幸存者满意度
                    break;
            }
        }
    }
}
