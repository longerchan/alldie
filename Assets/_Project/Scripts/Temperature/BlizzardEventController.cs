using System;
using FrostShelter.Core;

namespace FrostShelter.Temperature
{
    /// <summary>
    /// 暴风雪事件控制器。管理暴风雪的触发时机和强度。
    /// </summary>
    public class BlizzardEventController : IService
    {
        private TemperatureManager _temperatureManager;
        private EventDispatcher _events;

        private float _nextBlizzardCheckSeconds;
        private float _checkInterval;
        private readonly System.Random _rng = new();

        public bool IsBlizzardPending { get; private set; }
        public bool HasWarningBeenIssued { get; private set; }

        public event Action OnBlizzardWarning;
        public event Action OnBlizzardImminent;

        public void Initialize()
        {
            _checkInterval = UnityEngine.Random.Range(600f, 1800f); // 10-30分钟
            _nextBlizzardCheckSeconds = _checkInterval;
        }

        public void Shutdown() { }

        public void SetDependencies(TemperatureManager temperatureManager, EventDispatcher events)
        {
            _temperatureManager = temperatureManager;
            _events = events;
        }

        /// <summary>每秒tick检查暴风雪触发</summary>
        public void Tick(float deltaSeconds)
        {
            if (_temperatureManager.IsBlizzardActive) return;

            _nextBlizzardCheckSeconds -= deltaSeconds;
            if (_nextBlizzardCheckSeconds <= 0f)
            {
                // 重置检查间隔
                _checkInterval = UnityEngine.Random.Range(600f, 1800f);
                _nextBlizzardCheckSeconds = _checkInterval;

                // 概率触发暴风雪（根据地温概率递增）
                float currentTemp = _temperatureManager.CurrentTemperature;
                float triggerChance = currentTemp > -15f ? 0.05f
                    : currentTemp > -25f ? 0.15f
                    : 0.30f;

                if (_rng.NextDouble() < triggerChance)
                {
                    // 30秒前发出预警
                    IsBlizzardPending = true;
                    HasWarningBeenIssued = false;
                    OnBlizzardWarning?.Invoke();

                    // 30秒后触发暴风雪
                    // (实际项目中用协程或定时器)
                    TriggerDelayedBlizzard(30f);
                }
            }
        }

        private void TriggerDelayedBlizzard(float delaySeconds)
        {
            IsBlizzardPending = true;
            // 简化：直接随机时长触发
            float duration = UnityEngine.Random.Range(180f, 600f);
            _temperatureManager.TriggerBlizzard(duration);
            IsBlizzardPending = false;
        }
    }
}
