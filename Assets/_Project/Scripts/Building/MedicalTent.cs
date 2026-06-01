namespace FrostShelter.Building
{
    /// <summary>
    /// 医疗帐篷。治疗伤员，降低非战斗减员概率。
    /// </summary>
    public class MedicalTent : BuildingBase
    {
        public float HealRatePerHour
        {
            get
            {
                if (Level < 0) return 0f;
                var cfg = CurrentLevelConfig;
                return cfg != null ? cfg.HealRatePerHour : 1f + Level * 0.5f;
            }
        }

        public float DeathPreventionRate
        {
            get
            {
                if (Level < 0) return 0f;
                return Level * 0.08f; // 每级降低8%死亡概率
            }
        }

        public bool CanHeal()
        {
            return Level >= 0;
        }

        public float CalculateHealAmount(float hoursPassed)
        {
            return HealRatePerHour * hoursPassed * TotalEfficiencyMultiplier();
        }

        public override void Initialize(BuildingId id, BuildingConfigSO config, int initialLevel = 0)
        {
            base.Initialize(id, config, initialLevel);
        }
    }
}
