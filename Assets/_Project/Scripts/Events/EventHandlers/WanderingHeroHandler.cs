using FrostShelter.Core;
using FrostShelter.Resource;

namespace FrostShelter.Events.EventHandlers
{
    /// <summary>
    /// 流浪英雄事件。遇到路过的英雄，可招募。
    /// </summary>
    public class WanderingHeroHandler
    {
        private readonly Hero.HeroManager _heroManager;
        private readonly ResourceManager _resourceManager;
        private readonly EventDispatcher _events;

        public WanderingHeroHandler(Hero.HeroManager heroManager,
            ResourceManager resourceManager, EventDispatcher events)
        {
            _heroManager = heroManager;
            _resourceManager = resourceManager;
            _events = events;
        }

        public void Handle(RandomEventSO evt, int choiceIndex)
        {
            switch (choiceIndex)
            {
                case 0: // 邀请加入（消耗大量资源，确定获得英雄）
                    var cost = new ResourceCost();
                    cost.Add(ResourceType.RawMeat, 20f);
                    cost.Add(ResourceType.Wood, 30f);
                    if (_resourceManager.Spend(cost, ResourceChangeReason.Event))
                    {
                        _heroManager.RecruitHero("wandering_hero_01");
                        _events?.Dispatch(GameEventType.HeroRecruited);
                    }
                    break;
                case 1: // 提供帮助（消耗少量资源，可能获得英雄线索或魂石）
                    var helpCost = new ResourceCost();
                    helpCost.Add(ResourceType.RawMeat, 5f);
                    if (_resourceManager.Spend(helpCost, ResourceChangeReason.Event))
                    {
                        _resourceManager.AddResource(ResourceType.HeroSoulStone,
                            UnityEngine.Random.Range(1, 4), ResourceChangeReason.Event);
                    }
                    break;
                case 2: // 赶走（什么都不做）
                    // 可能降低满意度
                    break;
            }
        }
    }
}
