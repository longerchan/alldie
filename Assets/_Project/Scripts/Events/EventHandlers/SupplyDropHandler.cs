using FrostShelter.Core;
using FrostShelter.Resource;

namespace FrostShelter.Events.EventHandlers
{
    /// <summary>
    /// 空投补给事件。发现补给箱，随机获得资源。
    /// </summary>
    public class SupplyDropHandler
    {
        private readonly ResourceManager _resourceManager;
        private readonly EventDispatcher _events;

        public SupplyDropHandler(ResourceManager resourceManager, EventDispatcher events)
        {
            _resourceManager = resourceManager;
            _events = events;
        }

        public void Handle(RandomEventSO evt, int choiceIndex)
        {
            var types = new[] { ResourceType.RawMeat, ResourceType.Wood,
                ResourceType.Coal, ResourceType.IronOre };
            var amounts = new[] { 15f, 20f, 8f, 5f };

            int idx = UnityEngine.Random.Range(0, types.Length);
            _resourceManager.AddResource(types[idx], amounts[idx], ResourceChangeReason.Event);
        }
    }
}
