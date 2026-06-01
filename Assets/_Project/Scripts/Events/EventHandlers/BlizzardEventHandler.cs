using FrostShelter.Core;

namespace FrostShelter.Events.EventHandlers
{
    /// <summary>
    /// 暴风雪事件处理器。触发暴风雪，影响营地温度和资源。
    /// </summary>
    public class BlizzardEventHandler
    {
        private readonly Temperature.TemperatureManager _temperatureManager;
        private readonly EventDispatcher _events;

        public BlizzardEventHandler(Temperature.TemperatureManager temperatureManager,
            EventDispatcher events)
        {
            _temperatureManager = temperatureManager;
            _events = events;
        }

        public void Handle(RandomEventSO evt, int choiceIndex)
        {
            var choice = evt.Choices[choiceIndex];
            // 暴风雪事件的选择：
            // 0: 储备柴薪（消耗木材，减少影响）
            // 1: 提升火炉功率（消耗火晶，完全抵御）
            // 2: 硬扛（不做任何事）

            switch (choiceIndex)
            {
                case 0:
                    _temperatureManager.TriggerBlizzard(240f); // 4分钟（缩短版）
                    break;
                case 1:
                    // 消耗火晶完全抵御，不触发暴风雪
                    break;
                case 2:
                    _temperatureManager.TriggerBlizzard(600f); // 10分钟完整版
                    break;
            }
        }
    }
}
