using FrostShelter.Core;
using FrostShelter.Resource;

namespace FrostShelter.Exploration.NodeTypes
{
    /// <summary>
    /// 资源节点控制器。处理采集资源交互。
    /// </summary>
    public class ResourceNodeController
    {
        public HexNode Node { get; private set; }
        private ResourceManager _resourceManager;
        private EventDispatcher _events;

        public ResourceNodeController(HexNode node, ResourceManager resourceManager, EventDispatcher events)
        {
            Node = node;
            _resourceManager = resourceManager;
            _events = events;
        }

        public void OnEnter()
        {
            if (Node.ResourceData == null) return;

            var data = Node.ResourceData;
            float baseAmount = data.Amount;
            // 难度加成的资源量
            float amount = baseAmount * (1f + Node.DifficultyLevel * 0.2f);

            // 如果是需要英雄采集的资源点，计算额外加成
            float bonus = 0f;
            if (data.RequiresHero)
            {
                bonus = amount * 0.3f; // 英雄加成30%
            }

            _resourceManager.AddResource(data.ResourceType, amount + bonus,
                ResourceChangeReason.Expedition);
        }
    }
}
