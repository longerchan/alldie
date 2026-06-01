using FrostShelter.SaveSystem;

namespace FrostShelter.Building
{
    /// <summary>
    /// 指挥部。提升探险队出征人数上限。
    /// </summary>
    public class CommandCenter : BuildingBase
    {
        public int MaxExpeditionHeroes
        {
            get
            {
                if (Level < 0) return 0;
                // 基础3人，每级+1，最高8人
                return System.Math.Min(8, 3 + Level);
            }
        }

        public int MaxExpeditionTroops
        {
            get
            {
                if (Level < 0) return 0;
                return System.Math.Min(9, 3 + Level * 2);
            }
        }

        public float ExpeditionFoodEfficiency
        {
            get
            {
                if (Level < 0) return 0f;
                return 1f - Level * 0.05f; // 每级减少5%食物消耗
            }
        }

        public float ExpeditionWarmthEfficiency
        {
            get
            {
                if (Level < 0) return 0f;
                return 1f - Level * 0.05f; // 每级减少5%暖炉消耗
            }
        }

        public override void Initialize(BuildingId id, BuildingConfigSO config, int initialLevel = 0)
        {
            base.Initialize(id, config, initialLevel);
        }
    }
}
