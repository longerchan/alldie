using FrostShelter.Core;

namespace FrostShelter.Exploration.NodeTypes
{
    /// <summary>
    /// 事件节点控制器。触发探险中的随机事件。
    /// </summary>
    public class EventNodeController
    {
        public HexNode Node { get; private set; }
        private Events.RandomEventManager _eventManager;
        private EventDispatcher _events;

        public EventNodeController(HexNode node, Events.RandomEventManager eventManager)
        {
            Node = node;
            _eventManager = eventManager;
        }

        public void OnEnter()
        {
            if (Node.EventData == null || string.IsNullOrEmpty(Node.EventData.EventConfigId))
            {
                // 随机选择一个探险事件
                TriggerRandomExplorationEvent();
                return;
            }

            // 触发指定事件（由 RandomEventManager 处理）
        }

        private void TriggerRandomExplorationEvent()
        {
            // 探险中特有事件：冰窟、废弃避难所、信号塔等
            var eventTypes = new[]
            {
                "exploration_ice_cave",
                "exploration_abandoned_shelter",
                "exploration_signal_tower",
                "exploration_wrecked_convoy",
            };
            var selectedId = eventTypes[UnityEngine.Random.Range(0, eventTypes.Length)];
            // 触发对应的 RandomEventSO
        }
    }
}
