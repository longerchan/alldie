using System;
using System.Collections.Generic;
using FrostShelter.Core;

namespace FrostShelter.TimeEngine
{
    /// <summary>
    /// 时间驱动引擎。管理在线 tick 循环和离线时间计算。
    /// 是所有与时间相关的生产、消耗、事件判定的唯一时间源。
    /// </summary>
    public class TimeEngine : IService
    {
        public float GameTimeScale { get; set; } = 1f;
        public DateTime LastOnlineTime { get; private set; }
        public TimeSpan TotalPlayTime { get; private set; } = TimeSpan.Zero;

        public event Action<float> OnTick;              // 每秒
        public event Action OnProductionTick;           // 每10秒
        public event Action OnMinuteTick;               // 每60秒（游戏内1小时）
        public event Action<TimeSpan> OnOfflineCalculated;

        private readonly List<ITimeProducer> _producers = new();
        private float _prodTickAccumulator;
        private float _minuteTickAccumulator;
        private float _autoSaveAccumulator;
        private bool _isRunning;
        private Action _autoSaveCallback;

        public void Initialize()
        {
            LastOnlineTime = DateTime.UtcNow;
        }

        public void Shutdown()
        {
            _isRunning = false;
            _producers.Clear();
        }

        public void RegisterProducer(ITimeProducer producer)
        {
            if (!_producers.Contains(producer))
            {
                _producers.Add(producer);
            }
        }

        public void UnregisterProducer(ITimeProducer producer)
        {
            _producers.Remove(producer);
        }

        public void SetAutoSaveCallback(Action callback)
        {
            _autoSaveCallback = callback;
        }

        /// <summary>应在 Unity 的 Update 中每帧调用</summary>
        public void Tick(float deltaTime)
        {
            if (!_isRunning) return;

            float scaledDelta = deltaTime * GameTimeScale;

            // 每秒 tick
            OnTick?.Invoke(scaledDelta);

            // 每10秒生产结算
            _prodTickAccumulator += scaledDelta;
            while (_prodTickAccumulator >= Constants.PRODUCTION_TICK)
            {
                _prodTickAccumulator -= Constants.PRODUCTION_TICK;
                var tempEfficiency = GetTemperatureEfficiency();
                foreach (var producer in _producers)
                {
                    producer.Produce(Constants.PRODUCTION_TICK, tempEfficiency);
                }
                OnProductionTick?.Invoke();
            }

            // 每60秒游戏内逻辑tick
            _minuteTickAccumulator += scaledDelta;
            while (_minuteTickAccumulator >= Constants.SECONDS_PER_IN_GAME_HOUR)
            {
                _minuteTickAccumulator -= Constants.SECONDS_PER_IN_GAME_HOUR;
                OnMinuteTick?.Invoke();
            }

            // 自动存档
            _autoSaveAccumulator += scaledDelta;
            if (_autoSaveAccumulator >= Constants.AUTO_SAVE_INTERVAL_SECONDS)
            {
                _autoSaveAccumulator = 0f;
                _autoSaveCallback?.Invoke();
            }
        }

        public void StartTime()
        {
            // 先计算离线收益
            CalculateOfflineEarnings();
            LastOnlineTime = DateTime.UtcNow;
            _isRunning = true;
        }

        public void StopTime()
        {
            _isRunning = false;
            LastOnlineTime = DateTime.UtcNow;
        }

        public void SetTimeScale(float scale)
        {
            GameTimeScale = Math.Max(0f, scale);
        }

        public TimeSpan GetOfflineDuration(long lastSaveUnixSeconds)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var offlineSeconds = now - lastSaveUnixSeconds;

            if (offlineSeconds <= 0) return TimeSpan.Zero;

            // 最大离线计算时长
            var maxSeconds = Constants.OFFLINE_MAX_HOURS * 3600;
            return TimeSpan.FromSeconds(Math.Min(offlineSeconds, (long)maxSeconds));
        }

        public OfflineEarningsSummary CalculateOfflineEarnings()
        {
            var summary = new OfflineEarningsSummary();
            float tempEfficiency = GetTemperatureEfficiency();
            float totalOfflineSeconds = (float)GetOfflineDuration(
                DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds()).TotalSeconds;

            foreach (var producer in _producers)
            {
                var record = producer.CalculateOfflineProduction(totalOfflineSeconds, tempEfficiency);
                summary.Records.Add(record);
            }

            OnOfflineCalculated?.Invoke(TimeSpan.FromSeconds(totalOfflineSeconds));
            return summary;
        }

        /// <summary>
        /// 温度效率因子。实际项目中应从 TemperatureManager 获取。
        /// </summary>
        private float GetTemperatureEfficiency()
        {
            return 1.0f; // Default, will be overridden by TemperatureManager integration
        }
    }

    public class OfflineEarningsSummary
    {
        public List<OfflineProductionRecord> Records = new();
        public int SurvivorsDied;
        public int SurvivorsEscaped;
        public bool BlizzardOccurred;
    }
}
