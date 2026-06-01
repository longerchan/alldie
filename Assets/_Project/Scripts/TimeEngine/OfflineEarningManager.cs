using System;
using System.Collections.Generic;
using System.Linq;
using FrostShelter.Core;
using FrostShelter.Resource;

namespace FrostShelter.TimeEngine
{
    /// <summary>
    /// 离线收益管理器。玩家回到游戏时结算离线期间的生产和消耗。
    /// </summary>
    public class OfflineEarningManager : IService
    {
        private TimeEngine _timeEngine;
        private ResourceManager _resourceManager;
        private Temperature.TemperatureManager _temperatureManager;
        private Survivor.SurvivorManager _survivorManager;
        private EventDispatcher _events;

        private readonly List<ITimeProducer> _producers = new();

        public void Initialize() { }

        public void Shutdown()
        {
            _producers.Clear();
        }

        public void SetDependencies(TimeEngine timeEngine, ResourceManager resourceManager,
            Temperature.TemperatureManager temperatureManager, Survivor.SurvivorManager survivorManager,
            EventDispatcher events)
        {
            _timeEngine = timeEngine;
            _resourceManager = resourceManager;
            _temperatureManager = temperatureManager;
            _survivorManager = survivorManager;
            _events = events;
        }

        public void RegisterProducer(ITimeProducer producer)
        {
            _producers.Add(producer);
        }

        /// <summary>计算离线收益并返回摘要</summary>
        public OfflineEarningsSummary CalculateOfflineEarnings(long lastSaveUnixSeconds)
        {
            var summary = new OfflineEarningsSummary();
            var offlineDuration = _timeEngine.GetOfflineDuration(lastSaveUnixSeconds);
            float totalOfflineSeconds = (float)offlineDuration.TotalSeconds;

            if (totalOfflineSeconds <= 0f) return summary;

            float tempEfficiency = _temperatureManager?.ProductionEfficiency ?? 1f;

            // 各生产者的离线产出
            foreach (var producer in _producers)
            {
                var record = producer.CalculateOfflineProduction(totalOfflineSeconds, tempEfficiency);
                if (record.TotalProduced > 0f)
                {
                    summary.Records.Add(record);
                    _resourceManager?.AddResource(record.OutputType, record.TotalProduced,
                        ResourceChangeReason.Offline);
                }
            }

            // 幸存者离线衰减
            float hoursOffline = totalOfflineSeconds / 3600f;
            float tempDamagePerHour = _temperatureManager?.SurvivorDamagePerHour ?? 0f;
            float sickChance = _temperatureManager?.SickChancePerHour ?? 0f;

            if (_survivorManager != null)
            {
                // 简化的离线幸存者处理
                for (int i = 0; i < (int)hoursOffline; i++)
                {
                    _survivorManager.OnMinuteTick();
                }

                // 统计离线期间死伤
                // (实际项目中从SaveData对比计算)
            }

            _events?.Dispatch(GameEventType.OfflineEarningsCalculated);
            return summary;
        }

        /// <summary>获取格式化的离线时间字符串</summary>
        public string GetOfflineTimeString(long lastSaveUnixSeconds)
        {
            var duration = _timeEngine.GetOfflineDuration(lastSaveUnixSeconds);
            if (duration.TotalHours < 1)
                return $"{duration.Minutes}分钟";
            if (duration.TotalHours < 24)
                return $"{duration.Hours}小时{duration.Minutes}分钟";
            return $"{duration.Days}天{duration.Hours}小时";
        }
    }
}
