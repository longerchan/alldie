using System;

namespace FrostShelter.Building
{
    /// <summary>
    /// BuildingManager 每小时tick扩展方法
    /// </summary>
    public partial class BuildingManager
    {
        public void OnHourTick()
        {
            // 处理建筑升级倒计时
            foreach (var kvp in AllBuildings)
            {
                var building = kvp.Value;
                if (building.IsUpgrading && building.UpgradeFinishTime <= DateTime.UtcNow)
                {
                    building.OnUpgradeComplete();
                }
            }
        }
    }
}
