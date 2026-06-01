using FrostShelter.SaveSystem;
using FrostShelter.TimeEngine;

namespace FrostShelter.Building
{
    /// <summary>
    /// 生产型建筑。自动生产资源，累积待收集。
    /// 实现 ITimeProducer 以被 TimeEngine 驱动。
    /// </summary>
    public class ProductionBuilding : BuildingBase, ITimeProducer
    {
        public ResourceType OutputResource => Config?.OutputResource ?? ResourceType.Wood;
        public float AccumulatedProduction { get; set; }
        public float MaxAccumulation { get; set; } = 500f;

        public string ProducerId => $"Building_{Id}";

        public float BaseProductionPerHour
        {
            get
            {
                var cfg = CurrentLevelConfig;
                return cfg?.BaseProductionPerHour ?? 0f;
            }
        }

        public override void Initialize(BuildingId id, BuildingConfigSO config, int initialLevel = 0)
        {
            base.Initialize(id, config, initialLevel);
        }

        /// <summary>在线生产 tick（由 TimeEngine 调用）</summary>
        public void Produce(float deltaSeconds, float efficiencyMultiplier)
        {
            if (Level < 0) return; // 未解锁

            float totalEfficiency = TotalEfficiencyMultiplier() * efficiencyMultiplier;
            float produced = ProductionCalculator.CalculateProduction(
                BaseProductionPerHour, totalEfficiency, deltaSeconds);

            float cap = Math.Min(MaxAccumulation, AccumulatedProduction + produced);
            AccumulatedProduction = cap;
        }

        /// <summary>离线生产结算（由 TimeEngine 调用）</summary>
        public OfflineProductionRecord CalculateOfflineProduction(
            float totalOfflineSeconds, float efficiencyMultiplier)
        {
            if (Level < 0) return default;

            float totalEfficiency = TotalEfficiencyMultiplier() * efficiencyMultiplier;
            float produced = ProductionCalculator.CalculateProduction(
                BaseProductionPerHour, totalEfficiency, totalOfflineSeconds);

            float cappedAmount = 0f;
            bool wasCapped = false;
            float newTotal = AccumulatedProduction + produced;
            if (newTotal > MaxAccumulation)
            {
                cappedAmount = newTotal - MaxAccumulation;
                wasCapped = true;
                newTotal = MaxAccumulation;
            }
            AccumulatedProduction = newTotal;

            return new OfflineProductionRecord
            {
                ProducerId = ProducerId,
                OutputType = OutputResource,
                TotalProduced = produced - cappedAmount,
                EfficiencyUsed = totalEfficiency,
                WasCapped = wasCapped,
                CappedAmount = cappedAmount,
            };
        }

        /// <summary>收集所有积累的资源</summary>
        public float Collect()
        {
            float amount = AccumulatedProduction;
            AccumulatedProduction = 0f;
            return amount;
        }

        public bool HasAccumulated => AccumulatedProduction > 0f;
        public float FillRatio => MaxAccumulation > 0f
            ? AccumulatedProduction / MaxAccumulation
            : 0f;
    }
}
