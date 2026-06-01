using System;
using System.Collections.Generic;
using System.Linq;
using FrostShelter.Core;
using FrostShelter.Resource;

namespace FrostShelter.Events
{
    /// <summary>
    /// 随机事件管理器。管理20+种随机事件的触发、选择和结算。
    /// </summary>
    public class RandomEventManager : IService
    {
        private readonly List<RandomEventSO> _eventPool = new();
        private readonly Dictionary<string, int> _triggerCounts = new();

        private EventDispatcher _events;
        private ResourceManager _resourceManager;
        private System.Random _rng = new();

        public RandomEventSO CurrentEvent { get; private set; }
        public bool IsEventActive => CurrentEvent != null;
        public float EventTimer { get; private set; }

        public event Action<RandomEventSO> OnEventTriggered;
        public event Action OnEventResolved;

        public void Initialize()
        {
            EventTimer = UnityEngine.Random.Range(60f, 300f); // 初始随机间隔
        }

        public void Shutdown()
        {
            _eventPool.Clear();
            CurrentEvent = null;
        }

        public void SetDependencies(EventDispatcher events, ResourceManager resourceManager)
        {
            _events = events;
            _resourceManager = resourceManager;
        }

        public void RegisterEvent(RandomEventSO evt)
        {
            _eventPool.Add(evt);
        }

        /// <summary>每60秒tick检查是否触发随机事件</summary>
        public void OnMinuteTick()
        {
            EventTimer -= 60f;

            if (EventTimer <= 0f && !IsEventActive)
            {
                // 重置计时器
                EventTimer = UnityEngine.Random.Range(120f, 600f); // 2-10分钟间隔

                // 按权重选择事件
                var availableEvents = _eventPool
                    .Where(e => CanTriggerEvent(e))
                    .ToList();

                if (availableEvents.Count > 0)
                {
                    float totalWeight = availableEvents.Sum(e => e.Weight);
                    float roll = (float)_rng.NextDouble() * totalWeight;
                    float cumulative = 0f;

                    foreach (var evt in availableEvents)
                    {
                        cumulative += evt.Weight;
                        if (roll <= cumulative)
                        {
                            TriggerEvent(evt);
                            break;
                        }
                    }
                }
            }
        }

        private bool CanTriggerEvent(RandomEventSO evt)
        {
            if (evt == null) return false;
            if (evt.MaxTriggerCount > 0 &&
                _triggerCounts.GetValueOrDefault(evt.EventId, 0) >= evt.MaxTriggerCount)
                return false;

            return EvaluateConditions(evt.Conditions);
        }

        private bool EvaluateConditions(EventCondition[] conditions)
        {
            if (conditions == null || conditions.Length == 0) return true;

            foreach (var condition in conditions)
            {
                if (!EvaluateSingleCondition(condition)) return false;
            }
            return true;
        }

        private bool EvaluateSingleCondition(EventCondition condition)
        {
            return condition.ConditionType switch
            {
                ConditionType.FurnaceLevel => condition.CheckInt(
                    ServiceLocator.TryGet<Building.BuildingManager>(out var bm)
                        ? bm.Furnace?.Level ?? 0 : 0),
                ConditionType.SurvivorCount => condition.CheckInt(
                    ServiceLocator.TryGet<Survivor.SurvivorManager>(out var sm)
                        ? sm.TotalSurvivorCount : 0),
                ConditionType.Always => true,
                _ => true,
            };
        }

        public void TriggerEvent(RandomEventSO evt)
        {
            CurrentEvent = evt;
            _triggerCounts[evt.EventId] = _triggerCounts.GetValueOrDefault(evt.EventId, 0) + 1;

            OnEventTriggered?.Invoke(evt);
            _events?.Dispatch(GameEventType.RandomEventTriggered);
        }

        /// <summary>玩家做出选择</summary>
        public void ResolveEvent(int choiceIndex)
        {
            if (CurrentEvent == null) return;

            var choices = CurrentEvent.Choices;
            if (choiceIndex < 0 || choiceIndex >= choices.Length) return;

            var choice = choices[choiceIndex];
            float totalWeight = choice.Outcomes.Sum(o => o.Weight);
            float roll = (float)_rng.NextDouble() * totalWeight;
            float cumulative = 0f;

            foreach (var outcome in choice.Outcomes)
            {
                cumulative += outcome.Weight;
                if (roll <= cumulative)
                {
                    ApplyOutcome(outcome);
                    break;
                }
            }

            CurrentEvent = null;
            OnEventResolved?.Invoke();
            _events?.Dispatch(GameEventType.RandomEventResolved);
        }

        private void ApplyOutcome(EventOutcome outcome)
        {
            // 资源变化
            if (outcome.ResourceChanges != null)
            {
                foreach (var change in outcome.ResourceChanges)
                {
                    if (change.Amount > 0)
                        _resourceManager.AddResource(change.ResourceType, change.Amount,
                            ResourceChangeReason.Event);
                    else
                        _resourceManager.RemoveResource(change.ResourceType, -change.Amount,
                            ResourceChangeReason.Event);
                }
            }

            // 幸存者满意度变化
            if (outcome.SurvivorSatisfactionDelta != 0)
            {
                if (ServiceLocator.TryGet<Survivor.SurvivorManager>(out var sm))
                {
                    sm.BoostMorale(outcome.SurvivorSatisfactionDelta);
                }
            }

            // 温度变化
            if (outcome.TemperatureDelta != 0)
            {
                if (ServiceLocator.TryGet<Temperature.TemperatureManager>(out var tm))
                {
                    tm.ApplyEventTemperatureChange(outcome.TemperatureDelta);
                }
            }
        }
    }
}
