using System;
using System.Collections.Generic;

namespace FrostShelter.Building
{
    /// <summary>
    /// 酒馆。每天刷新招募券，获取英雄的场所。
    /// </summary>
    public class Tavern : BuildingBase
    {
        public int DailyRecruitCharges
        {
            get
            {
                if (Level < 0) return 0;
                return CurrentLevelConfig?.DailyRecruitCharges ?? 1 + Level;
            }
        }

        public int RemainingCharges { get; private set; }
        public DateTime LastRefreshTime { get; private set; }

        private readonly List<string> _availableHeroConfigIds = new();

        public override void Initialize(BuildingId id, BuildingConfigSO config, int initialLevel = 0)
        {
            base.Initialize(id, config, initialLevel);
            RemainingCharges = DailyRecruitCharges;
            LastRefreshTime = DateTime.UtcNow;
        }

        public override void OnLevelUp(int newLevel)
        {
            base.OnLevelUp(newLevel);
            RemainingCharges = DailyRecruitCharges;
        }

        public bool CanRecruit()
        {
            return Level >= 0 && RemainingCharges > 0;
        }

        public string GetRandomHeroConfigId()
        {
            if (_availableHeroConfigIds.Count == 0) return null;
            int idx = UnityEngine.Random.Range(0, _availableHeroConfigIds.Count);
            return _availableHeroConfigIds[idx];
        }

        public void ConsumeCharge()
        {
            RemainingCharges = Math.Max(0, RemainingCharges - 1);
        }

        public void RefreshDaily()
        {
            RemainingCharges = DailyRecruitCharges;
            LastRefreshTime = DateTime.UtcNow;
        }

        /// <summary>权重招募：等级越高，高品质英雄出现概率越大</summary>
        public HeroQuality GetRecruitQuality()
        {
            float roll = UnityEngine.Random.value;
            if (Level >= 8 && roll < 0.05f) return HeroQuality.Gold;       // 5%
            if (Level >= 5 && roll < 0.20f) return HeroQuality.Purple;     // 15%
            if (Level >= 3 && roll < 0.55f) return HeroQuality.Purple;     // 35%
            return HeroQuality.Blue;                                        // 45%
        }
    }

    public enum HeroQuality { Blue, Purple, Gold }
}
