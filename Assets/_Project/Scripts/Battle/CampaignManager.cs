using System;
using System.Collections.Generic;
using FrostShelter.Core;
using FrostShelter.Resource;
using FrostShelter.SaveSystem;

namespace FrostShelter.Battle
{
    /// <summary>
    /// 主线战役管理器。管理关卡推图、自动战斗流程。
    /// </summary>
    public class CampaignManager : IService
    {
        private readonly Dictionary<int, CampaignChapter> _chapters = new();

        private BattleManager _battleManager;
        private ResourceManager _resourceManager;
        private EventDispatcher _events;

        public int CurrentChapter { get; private set; } = 1;
        public int CurrentStage { get; private set; } = 1;
        public int HighestUnlockedStage { get; private set; } = 1;

        public event Action<int, int> OnStageCleared;
        public event Action<int> OnChapterCleared;

        public void Initialize() { }
        public void Shutdown() { _chapters.Clear(); }

        public void SetDependencies(BattleManager battleManager, ResourceManager resourceManager,
            EventDispatcher events)
        {
            _battleManager = battleManager;
            _resourceManager = resourceManager;
            _events = events;
        }

        public void RegisterChapter(CampaignChapter chapter)
        {
            _chapters[chapter.ChapterNumber] = chapter;
        }

        public CampaignChapter GetChapter(int chapterNumber)
        {
            _chapters.TryGetValue(chapterNumber, out var chapter);
            return chapter;
        }

        public CampaignStage GetCurrentStage()
        {
            var chapter = GetChapter(CurrentChapter);
            return chapter?.GetStage(CurrentStage);
        }

        public bool CanChallengeStage(int chapter, int stage)
        {
            if (chapter > HighestUnlockedStage / 100) return false;
            // 需要前一关通关才能挑战
            if (stage > 1 && !IsStageCleared(chapter, stage - 1)) return false;
            return true;
        }

        public bool IsStageCleared(int chapter, int stage)
        {
            return chapter < CurrentChapter ||
                   (chapter == CurrentChapter && stage < CurrentStage);
        }

        /// <summary>挑战关卡</summary>
        public BattleResult ChallengeStage(BattleFormation playerFormation)
        {
            var stage = GetCurrentStage();
            if (stage == null) return null;

            var enemyFormation = stage.GenerateEnemyFormation();
            var result = _battleManager.StartAutoBattle(playerFormation, enemyFormation);

            if (result.IsPlayerVictory)
            {
                // 发放通关奖励
                foreach (var reward in stage.Rewards)
                {
                    _resourceManager.AddResource(reward.Type, reward.Amount,
                        ResourceChangeReason.Story);
                }

                OnStageCleared?.Invoke(CurrentChapter, CurrentStage);

                // 推进进度
                var chapter = GetChapter(CurrentChapter);
                if (CurrentStage >= chapter.TotalStages)
                {
                    OnChapterCleared?.Invoke(CurrentChapter);
                    _events?.Dispatch(GameEventType.ChapterCompleted);
                    if (_chapters.ContainsKey(CurrentChapter + 1))
                    {
                        CurrentChapter++;
                        CurrentStage = 1;
                    }
                }
                else
                {
                    CurrentStage++;
                }

                if (CurrentStage > HighestUnlockedStage % 100)
                {
                    HighestUnlockedStage = CurrentChapter * 100 + CurrentStage;
                }
            }

            return result;
        }
    }

    [Serializable]
    public class CampaignChapter
    {
        public int ChapterNumber;
        public string ChapterTitle;
        public string ChapterDescription;
        public int TotalStages;
        public List<CampaignStage> Stages = new();

        public CampaignStage GetStage(int stageNumber)
        {
            return Stages.Find(s => s.StageNumber == stageNumber);
        }
    }

    [Serializable]
    public class CampaignStage
    {
        public int StageNumber;
        public string StageName;
        public string StageDescription;
        public int RecommendedPower;
        public int EnemyCount;
        public float EnemyPowerMultiplier;

        public List<CampaignReward> Rewards = new();

        public BattleFormation GenerateEnemyFormation()
        {
            var formation = new BattleFormation();
            int frontSlots = Math.Min(3, EnemyCount);
            int backSlots = Math.Min(3, Math.Max(0, EnemyCount - 3));

            float basePower = 20f * EnemyPowerMultiplier;
            for (int i = 0; i < frontSlots; i++)
            {
                var unit = new BattleUnit
                {
                    InstanceId = Guid.NewGuid().ToString(),
                    DisplayName = $"拾荒者{i + 1}",
                    IsHero = false,
                    TroopType = TroopType.Shield,
                    Atk = basePower * 0.8f,
                    Def = basePower * 1.2f,
                    Hp = basePower * 2f,
                    MaxHp = basePower * 2f,
                    Speed = 4f,
                    PositionRow = 0,
                    PositionSlot = i,
                };
                formation.SetUnit(0, i, unit);
            }
            for (int i = 0; i < backSlots; i++)
            {
                var unit = new BattleUnit
                {
                    InstanceId = Guid.NewGuid().ToString(),
                    DisplayName = $"拾荒者弓手{i + 1}",
                    IsHero = false,
                    TroopType = TroopType.Archer,
                    Atk = basePower * 1.2f,
                    Def = basePower * 0.6f,
                    Hp = basePower * 1.2f,
                    MaxHp = basePower * 1.2f,
                    Speed = 6f,
                    PositionRow = 1,
                    PositionSlot = i,
                };
                formation.SetUnit(1, i, unit);
            }
            return formation;
        }
    }

    [Serializable]
    public struct CampaignReward
    {
        public ResourceType Type;
        public float Amount;
    }
}
