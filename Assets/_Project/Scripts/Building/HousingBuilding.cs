using FrostShelter.SaveSystem;

namespace FrostShelter.Building
{
    /// <summary>
    /// 住宅/庇护所 - 提供幸存者空位。
    /// </summary>
    public class HousingBuilding : BuildingBase
    {
        public int MaxResidents
        {
            get
            {
                var cfg = CurrentLevelConfig;
                return cfg?.MaxResidents ?? 0;
            }
        }

        public override void Initialize(BuildingId id, BuildingConfigSO config, int initialLevel = 0)
        {
            base.Initialize(id, config, initialLevel);
        }
    }
}
