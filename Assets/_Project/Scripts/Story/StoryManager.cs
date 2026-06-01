using System;
using System.Collections.Generic;
using FrostShelter.Core;

namespace FrostShelter.Story
{
    /// <summary>
    /// 剧情管理器。管理3章主线剧情、多结局分支。
    /// </summary>
    public class StoryManager : IService
    {
        private readonly Dictionary<int, StoryChapterSO> _chapters = new();

        private EventDispatcher _events;

        public int CurrentChapter { get; private set; } = 1;
        public int CurrentNodeId { get; private set; }
        public List<int> CompletedNodeIds { get; private set; } = new();
        public Dictionary<int, int> BranchChoices { get; private set; } = new();
        public bool[] EndingsUnlocked { get; private set; } = new bool[3];

        public event Action<int> OnChapterStarted;
        public event Action<int> OnChapterCompleted;
        public event Action<int, int> OnBranchChoiceMade;
        public event Action<int> OnEndingReached;

        public void Initialize() { }

        public void Shutdown()
        {
            _chapters.Clear();
        }

        public void SetDependencies(EventDispatcher events)
        {
            _events = events;
        }

        public void RegisterChapter(StoryChapterSO chapter)
        {
            _chapters[chapter.ChapterNumber] = chapter;
        }

        public void StartChapter(int chapterNumber)
        {
            if (!_chapters.TryGetValue(chapterNumber, out var chapter)) return;

            CurrentChapter = chapterNumber;
            CurrentNodeId = chapter.StartNodeId;

            OnChapterStarted?.Invoke(chapterNumber);
            _events?.Dispatch(GameEventType.ChapterStarted);
        }

        public StoryNode GetCurrentNode()
        {
            if (_chapters.TryGetValue(CurrentChapter, out var chapter))
            {
                return chapter.GetNode(CurrentNodeId);
            }
            return null;
        }

        public void MakeChoice(int choiceIndex)
        {
            var node = GetCurrentNode();
            if (node == null) return;

            var choices = node.Choices;
            if (choiceIndex < 0 || choiceIndex >= choices.Length) return;

            var choice = choices[choiceIndex];
            BranchChoices[CurrentNodeId] = choiceIndex;

            OnBranchChoiceMade?.Invoke(CurrentNodeId, choiceIndex);
            _events?.Dispatch(GameEventType.BranchChoiceMade);

            if (choice.NextNodeId >= 0)
            {
                CompletedNodeIds.Add(CurrentNodeId);
                CurrentNodeId = choice.NextNodeId;

                // 检查是否到达结局
                var nextNode = GetCurrentNode();
                if (nextNode != null && nextNode.IsEnding)
                {
                    UnlockEnding(nextNode.EndingIndex);
                }
            }
        }

        public void CompleteChapter()
        {
            OnChapterCompleted?.Invoke(CurrentChapter);
            _events?.Dispatch(GameEventType.ChapterCompleted);

            if (CurrentChapter < _chapters.Count)
            {
                StartChapter(CurrentChapter + 1);
            }
        }

        private void UnlockEnding(int endingIndex)
        {
            if (endingIndex >= 0 && endingIndex < EndingsUnlocked.Length)
            {
                EndingsUnlocked[endingIndex] = true;
                OnEndingReached?.Invoke(endingIndex);
                _events?.Dispatch(GameEventType.EndingReached);
            }
        }

        public void LoadFromSaveData(SaveSystem.StoryProgressData data)
        {
            CurrentChapter = data.currentChapter;
            CurrentNodeId = data.currentNodeId;
            CompletedNodeIds = new List<int>(data.completedNodeIds);
            BranchChoices = new Dictionary<int, int>(data.branchChoices);
        }

        public SaveSystem.StoryProgressData ToSaveData()
        {
            return new SaveSystem.StoryProgressData
            {
                currentChapter = CurrentChapter,
                currentNodeId = CurrentNodeId,
                completedNodeIds = new List<int>(CompletedNodeIds),
                branchChoices = new Dictionary<int, int>(BranchChoices),
            };
        }
    }
}
