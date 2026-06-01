using System;
using FrostShelter.Resource;

namespace FrostShelter.Events
{
    [Serializable]
    public class RandomEventSO
    {
        public string EventId;
        public string EventTitle;
        public string EventDescription;

        public EventTriggerType TriggerType = EventTriggerType.Timer;
        public float TriggerIntervalMinMinutes = 2f;
        public float TriggerIntervalMaxMinutes = 10f;
        public EventCondition[] Conditions;
        public float Weight = 1f;

        public EventChoice[] Choices;
        public EventOutcome ConsequenceIfTimeout;

        public float DurationSeconds;  // 0 = 立即处理
        public bool IsRecurring = true;
        public int MaxTriggerCount = -1; // -1 = 无限制
    }

    public enum EventTriggerType
    {
        Timer,
        OnBuildingUpgrade,
        OnSurvivorCount,
        OnExpeditionReturn,
        OnChapterProgress,
    }

    [Serializable]
    public class EventCondition
    {
        public ConditionType ConditionType;
        public string TargetId;
        public int ThresholdValue;
        public ComparisonOp Op;

        public bool CheckInt(int actualValue)
        {
            return Op switch
            {
                ComparisonOp.GreaterThan => actualValue > ThresholdValue,
                ComparisonOp.GreaterOrEqual => actualValue >= ThresholdValue,
                ComparisonOp.LessThan => actualValue < ThresholdValue,
                ComparisonOp.LessOrEqual => actualValue <= ThresholdValue,
                ComparisonOp.Equal => actualValue == ThresholdValue,
                _ => true,
            };
        }
    }

    public enum ConditionType { Always, FurnaceLevel, SurvivorCount, HeroCount, ChapterProgress }
    public enum ComparisonOp { GreaterThan, GreaterOrEqual, LessThan, LessOrEqual, Equal }

    [Serializable]
    public class EventChoice
    {
        public string ChoiceText;
        public EventOutcome[] Outcomes;
    }

    [Serializable]
    public class EventOutcome
    {
        public string OutcomeText;
        public float Weight = 1f;
        public ResourceDelta[] ResourceChanges;
        public float SurvivorSatisfactionDelta;
        public float TemperatureDelta;
        public string SpawnSurvivorOccupationTag;
        public string SpawnHeroConfigId;
        public int StoryNodeUnlock;
    }

    [Serializable]
    public struct ResourceDelta
    {
        public ResourceType ResourceType;
        public float Amount;
    }
}
