using System.Collections.Generic;
using FrostShelter.Core;

namespace FrostShelter.TimeEngine
{
    /// <summary>
    /// 生产力计算工具。汇聚温度效率、科技加成、幸存者加成等因子，
    /// 统一计算任意建筑的实际产出倍率。
    /// </summary>
    public static class ProductionCalculator
    {
        /// <summary>
        /// 计算综合产出倍率
        /// </summary>
        public static float CalculateTotalEfficiency(
            float temperatureEfficiency,
            float heroBonus,
            float survivorBonus,
            float techBonus)
        {
            return temperatureEfficiency * (1f + heroBonus + survivorBonus + techBonus);
        }

        /// <summary>
        /// 根据基础产出速率计算实际产量
        /// </summary>
        public static float CalculateProduction(
            float baseProductionPerHour,
            float totalEfficiency,
            float deltaSeconds)
        {
            float hoursPassed = deltaSeconds / 3600f;
            return baseProductionPerHour * totalEfficiency * hoursPassed;
        }

        /// <summary>
        /// 应用仓库容量上限
        /// </summary>
        public static float ApplyStorageCap(float amount, float currentStorage, float maxCapacity)
        {
            float available = maxCapacity - currentStorage;
            if (available <= 0f) return 0f;
            return amount > available ? available : amount;
        }
    }
}
