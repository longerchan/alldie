namespace FrostShelter.Building
{
    /// <summary>
    /// 科研所。解锁科技节点，提升研究效率。
    /// </summary>
    public class ResearchLab : BuildingBase
    {
        public float ResearchSpeedMultiplier
        {
            get
            {
                if (Level < 0) return 0f;
                return 1f + Level * 0.2f; // 每级+20%研究速度
            }
        }

        public int MaxConcurrentResearch
        {
            get
            {
                if (Level < 0) return 0;
                return Level >= 5 ? 2 : 1; // 5级后可同时研究2个
            }
        }

        public override void Initialize(BuildingId id, BuildingConfigSO config, int initialLevel = 0)
        {
            base.Initialize(id, config, initialLevel);
        }
    }
}
