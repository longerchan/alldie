using System.Linq;
using FrostShelter.Core;
using FrostShelter.Temperature;

namespace FrostShelter.Survivor
{
    /// <summary>
    /// 满意度衰减系统。管理所有幸存者的满意度/心情的持续衰减和加成。
    /// </summary>
    public class SatisfactionDecaySystem : IService
    {
        private SurvivorManager _survivorManager;
        private TemperatureManager _temperatureManager;
        private Building.BuildingManager _buildingManager;
        private EventDispatcher _events;

        public void Initialize() { }
        public void Shutdown() { }

        public void SetDependencies(SurvivorManager survivorManager,
            TemperatureManager temperatureManager, Building.BuildingManager buildingManager,
            EventDispatcher events)
        {
            _survivorManager = survivorManager;
            _temperatureManager = temperatureManager;
            _buildingManager = buildingManager;
            _events = events;
        }

        /// <summary>每小时tick，由TimeEngine.OnMinuteTick触发</summary>
        public void OnHourTick()
        {
            var survivors = _survivorManager.AllSurvivors;
            float tempEfficiency = _temperatureManager?.ProductionEfficiency ?? 1f;
            float tempDamage = _temperatureManager?.SurvivorDamagePerHour ?? 0f;
            float sickChance = _temperatureManager?.SickChancePerHour ?? 0f;

            foreach (var survivor in survivors)
            {
                survivor.TickHourly(tempDamage, sickChance);
            }

            // 检查是否有幸存者逃跑
            var escaped = survivors.Where(s => s.WantsToEscape).ToList();
            foreach (var s in escaped)
            {
                _events?.Dispatch(GameEventType.SurvivorSatisfactionChanged);
            }
        }

        /// <summary>有幸存者入住时提升全体满意度</summary>
        public void OnSurvivorArrived()
        {
            foreach (var s in _survivorManager.AllSurvivors)
            {
                s.ModifyMood(2f); // 新人到来提升士气
            }
        }

        /// <summary>有幸存者死亡时降低全体满意度</summary>
        public void OnSurvivorDied()
        {
            foreach (var s in _survivorManager.AllSurvivors)
            {
                s.ModifySatisfaction(-5f); // 死亡打击
                s.ModifyMood(-10f);
            }
        }
    }
}
