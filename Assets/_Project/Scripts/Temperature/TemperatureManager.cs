using System;
using FrostShelter.Core;
using CoreCtx = FrostShelter.Core.AppContext;
using FrostShelter.Building;

namespace FrostShelter.Temperature
{
    /// <summary>
    /// 温度管理器。基于熔炉等级计算营地温度，控制生产效率梯度，
    /// 管理暴风雪事件。
    /// </summary>
    public class TemperatureManager : IService
    {
        private TemperatureConfigSO _config;
        private EventDispatcher _events;
        private FurnaceBuilding _furnace;

        public float CurrentTemperature { get; private set; }
        public float BaseTemperature { get; private set; }
        public bool IsBlizzardActive { get; private set; }
        public float BlizzardRemainingSeconds { get; private set; }
        public float ProductionEfficiency { get; private set; } = 1f;
        public float SurvivorDamagePerHour { get; private set; }
        public float SickChancePerHour { get; private set; }

        public event Action OnBlizzardStarted;
        public event Action OnBlizzardEnded;
        public event Action<float, float> OnTemperatureChanged; // old, new

        public void Initialize()
        {
            _config = GetDefaultConfig();
            CurrentTemperature = Core.Constants.BASE_TEMPERATURE;
            UpdateThresholdEffects();
        }

        public void Shutdown() { }

        public void SetDependencies(EventDispatcher events, FurnaceBuilding furnace)
        {
            _events = events;
            _furnace = furnace;
        }

        /// <summary>熔炉升级后重新计算基准温度</summary>
        public void RecalculateTemperature()
        {
            if (_furnace == null) return;

            float furnaceBonus = _furnace.TemperatureBonus;
            float newBaseTemp = _config.BaseTemperature + furnaceBonus;

            // 暴风雪叠加
            float previousTemp = CurrentTemperature;
            float targetTemp = IsBlizzardActive
                ? newBaseTemp - _config.BlizzardTempDrop
                : newBaseTemp;

            BaseTemperature = newBaseTemp;
            CurrentTemperature = Math.Max(_config.MaxMinTemperature,
                Math.Min(_config.MaxTemperature, targetTemp));

            UpdateThresholdEffects();

            if (Math.Abs(previousTemp - CurrentTemperature) > 0.1f)
            {
                OnTemperatureChanged?.Invoke(previousTemp, CurrentTemperature);
                _events?.Dispatch(GameEventType.TemperatureChanged,
                    new CoreCtx.TemperatureChangedArgs
                    {
                        CurrentTemp = CurrentTemperature,
                        PreviousTemp = previousTemp,
                        Reason = TemperatureChangeReason.FurnaceUpgrade,
                        IsBlizzardActive = IsBlizzardActive,
                    });
            }
        }

        public void OnMinuteTick()
        {
            // 处理暴风雪倒计时
            if (IsBlizzardActive)
            {
                BlizzardRemainingSeconds -= 60f; // 1 in-game hour
                if (BlizzardRemainingSeconds <= 0f)
                {
                    EndBlizzard();
                }
            }
        }

        /// <summary>尝试触发暴风雪（由 RandomEventManager 或定时器触发）</summary>
        public void TriggerBlizzard(float? durationSeconds = null)
        {
            if (IsBlizzardActive) return;

            float duration = durationSeconds
                ?? UnityEngine.Random.Range(
                    _config.BlizzardMinDuration,
                    _config.BlizzardMaxDuration);

            IsBlizzardActive = true;
            BlizzardRemainingSeconds = duration;

            float previousTemp = CurrentTemperature;
            CurrentTemperature -= _config.BlizzardTempDrop;
            UpdateThresholdEffects();

            OnBlizzardStarted?.Invoke();
            _events?.Dispatch(GameEventType.BlizzardStarted);
            _events?.Dispatch(GameEventType.TemperatureChanged,
                new CoreCtx.TemperatureChangedArgs
                {
                    CurrentTemp = CurrentTemperature,
                    PreviousTemp = previousTemp,
                    Reason = TemperatureChangeReason.BlizzardStart,
                    IsBlizzardActive = true,
                });
        }

        public void EndBlizzard()
        {
            if (!IsBlizzardActive) return;

            IsBlizzardActive = false;
            BlizzardRemainingSeconds = 0f;

            float previousTemp = CurrentTemperature;
            CurrentTemperature = BaseTemperature;
            UpdateThresholdEffects();

            OnBlizzardEnded?.Invoke();
            _events?.Dispatch(GameEventType.BlizzardEnded);
            _events?.Dispatch(GameEventType.TemperatureChanged,
                new CoreCtx.TemperatureChangedArgs
                {
                    CurrentTemp = CurrentTemperature,
                    PreviousTemp = previousTemp,
                    Reason = TemperatureChangeReason.BlizzardEnd,
                    IsBlizzardActive = false,
                });
        }

        public void ApplyEventTemperatureChange(float delta)
        {
            float previousTemp = CurrentTemperature;
            CurrentTemperature = Math.Max(_config.MaxMinTemperature,
                Math.Min(_config.MaxTemperature, CurrentTemperature + delta));
            UpdateThresholdEffects();

            _events?.Dispatch(GameEventType.TemperatureChanged,
                new CoreCtx.TemperatureChangedArgs
                {
                    CurrentTemp = CurrentTemperature,
                    PreviousTemp = previousTemp,
                    Reason = TemperatureChangeReason.Event,
                    IsBlizzardActive = IsBlizzardActive,
                });
        }

        private void UpdateThresholdEffects()
        {
            var thresholds = _config?.EfficiencyCurves;
            if (thresholds == null) return;

            foreach (var t in thresholds)
            {
                if (CurrentTemperature >= t.MinTemp && CurrentTemperature < t.MaxTemp)
                {
                    ProductionEfficiency = t.ProductionEfficiency;
                    SurvivorDamagePerHour = t.SurvivorDamagePerHour;
                    SickChancePerHour = t.SickChancePerHour;
                    return;
                }
            }

            // 低于最低阈值
            ProductionEfficiency = 0.4f;
            SurvivorDamagePerHour = 0.1f;
            SickChancePerHour = 0.05f;
        }

        private static TemperatureConfigSO GetDefaultConfig()
        {
            return new TemperatureConfigSO
            {
                BaseTemperature = Constants.BASE_TEMPERATURE,
                EfficiencyCurves = new[]
                {
                    // > -20°C: 舒适区间
                    new TemperatureConfigSO.TemperatureThreshold
                    {
                        MinTemp = -20f, MaxTemp = 100f,
                        ProductionEfficiency = 1.0f, SurvivorDamagePerHour = 0f, SickChancePerHour = 0f,
                    },
                    // -20°C ~ -30°C: 寒冷区间
                    new TemperatureConfigSO.TemperatureThreshold
                    {
                        MinTemp = -30f, MaxTemp = -20f,
                        ProductionEfficiency = 0.7f, SurvivorDamagePerHour = 0.02f, SickChancePerHour = 0.01f,
                    },
                    // < -30°C: 极寒区间
                    new TemperatureConfigSO.TemperatureThreshold
                    {
                        MinTemp = -100f, MaxTemp = -30f,
                        ProductionEfficiency = 0.4f, SurvivorDamagePerHour = 0.05f, SickChancePerHour = 0.03f,
                    },
                },
            };
        }
    }
}
