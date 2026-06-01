using System;
using System.Collections.Generic;
using FrostShelter.SaveSystem;

namespace FrostShelter.Building
{
    /// <summary>
    /// 兵营建筑。训练战斗单位（盾兵/矛兵/射手）。
    /// </summary>
    public class BarracksBuilding : BuildingBase
    {
        public TroopTypeSave TroopType { get; private set; }
        public int MaxTrainingQueue => CurrentLevelConfig?.MaxTrainingQueue ?? 3;
        public float TrainingSpeedMultiplier => CurrentLevelConfig?.TrainingSpeedMultiplier ?? 1f;

        public List<TrainingQueueItem> TrainingQueue { get; private set; } = new();

        public override void Initialize(BuildingId id, BuildingConfigSO config, int initialLevel = 0)
        {
            base.Initialize(id, config, initialLevel);
            TroopType = id switch
            {
                BuildingId.BarracksShield => TroopTypeSave.Shield,
                BuildingId.BarracksSpear => TroopTypeSave.Spear,
                BuildingId.BarracksArcher => TroopTypeSave.Archer,
                _ => TroopTypeSave.Shield,
            };
        }

        public bool CanTrain => Level >= 0 && TrainingQueue.Count < MaxTrainingQueue;

        public bool EnqueueTraining(string troopConfigId, int count)
        {
            if (!CanTrain) return false;
            float baseTime = 30f; // 基础训练时间（秒）
            float actualTime = baseTime / (TotalEfficiencyMultiplier() * TrainingSpeedMultiplier);

            TrainingQueue.Add(new TrainingQueueItem
            {
                TroopConfigId = troopConfigId,
                Count = count,
                TotalTimeSeconds = actualTime,
                RemainingTimeSeconds = actualTime,
            });
            return true;
        }

        public List<TroopSaveData> TickTraining(float deltaSeconds)
        {
            var completed = new List<TroopSaveData>();
            if (TrainingQueue.Count == 0) return completed;

            var first = TrainingQueue[0];
            first.RemainingTimeSeconds -= deltaSeconds;
            if (first.RemainingTimeSeconds <= 0f)
            {
                completed.Add(new TroopSaveData
                {
                    configId = first.TroopConfigId,
                    troopType = TroopType,
                    count = first.Count,
                    aliveCount = first.Count,
                });
                TrainingQueue.RemoveAt(0);
            }
            return completed;
        }
    }

    [Serializable]
    public class TrainingQueueItem
    {
        public string TroopConfigId;
        public int Count;
        public float TotalTimeSeconds;
        public float RemainingTimeSeconds;
    }
}
