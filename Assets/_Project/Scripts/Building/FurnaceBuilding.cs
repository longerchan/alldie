using FrostShelter.SaveSystem;

namespace FrostShelter.Building
{
    /// <summary>
    /// 大熔炉 - 游戏核心建筑。决定营地温度、人口上限、建筑等级上限。
    /// </summary>
    public class FurnaceBuilding : BuildingBase
    {
        public float TemperatureBonus
        {
            get
            {
                if (Level < 0) return 0f;
                return Level * Core.Constants.TEMP_PER_FURNACE_LEVEL;
            }
        }

        public int MaxSurvivorCapacity
        {
            get
            {
                if (Level < 0) return 0;
                // 基础4人，每级+2
                return 4 + Level * 2;
            }
        }

        public int MaxBuildingLevel
        {
            get
            {
                // 其他建筑等级上限受熔炉等级限制
                return Level + 3;
            }
        }

        public override void Initialize(BuildingId id, BuildingConfigSO config, int initialLevel = 0)
        {
            base.Initialize(id, config, initialLevel);
        }

        public override void OnLevelUp(int newLevel)
        {
            base.OnLevelUp(newLevel);
        }

        public int GetFurnaceLevel() => Math.Max(0, Level);
    }
}
