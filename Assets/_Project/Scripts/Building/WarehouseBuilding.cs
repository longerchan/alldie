namespace FrostShelter.Building
{
    /// <summary>
    /// 仓库 - 提升资源储存上限，减少随机事件资源损失。
    /// </summary>
    public class WarehouseBuilding : BuildingBase
    {
        public float StorageCapacity
        {
            get
            {
                if (Level < 0) return 500f;
                // 基础500，每级翻倍
                return 500f * (1 << Level);
            }
        }

        public float ProtectionRatio
        {
            get
            {
                if (Level < 0) return 0f;
                // 每级保护10%资源不被事件损失，最高80%
                return System.Math.Min(0.8f, Level * 0.1f);
            }
        }

        public override void Initialize(BuildingId id, BuildingConfigSO config, int initialLevel = 0)
        {
            base.Initialize(id, config, initialLevel);
        }
    }
}
