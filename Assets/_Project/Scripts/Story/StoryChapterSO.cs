using System;
using UnityEngine;
using System.Collections.Generic;

namespace FrostShelter.Story
{
    [Serializable]
    public class StoryChapterSO : ScriptableObject
    {
        public int ChapterNumber;
        public string ChapterTitle;
        public string ChapterDescription;
        public int StartNodeId;

        public List<StoryNode> Nodes = new();

        public StoryNode GetNode(int nodeId)
        {
            return Nodes.Find(n => n.NodeId == nodeId);
        }
    }

    [Serializable]
    public class StoryNode
    {
        public int NodeId;
        public string Title;
        public string DialogueText;
        public string SpeakerName;
        public string BackgroundImageId;
        public bool IsEnding;
        public int EndingIndex = -1;

        public StoryChoice[] Choices;

        public StoryCondition[] EntryConditions;
        public StoryEffect[] OnEnterEffects;
    }

    [Serializable]
    public class StoryChoice
    {
        public string ChoiceText;
        public int NextNodeId;

        public StoryCondition[] UnlockConditions;
        public StoryEffect[] OnSelectEffects;
    }

    [Serializable]
    public class StoryCondition
    {
        public ConditionType Type;
        public string TargetId;
        public int RequiredValue;
    }

    public enum ConditionType
    {
        FurnaceLevel,
        SurvivorCount,
        HeroOwned,
        ItemOwned,
        PreviousChoice,
        ChapterCompleted,
    }

    [Serializable]
    public class StoryEffect
    {
        public EffectType Type;
        public string TargetId;
        public float Value;
    }

    public enum EffectType
    {
        AddResource,
        AddHero,
        AddSurvivor,
        ChangeSatisfaction,
        UnlockBuilding,
        TriggerEvent,
    }
}
