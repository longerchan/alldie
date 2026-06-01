namespace FrostShelter.TimeEngine
{
    /// <summary>
    /// 所有基于时间进行生产的系统必须实现此接口。
    /// TimeEngine 通过此接口统一驱动在线生产和离线结算。
    /// </summary>
    public interface ITimeProducer
    {
        string ProducerId { get; }

        /// <summary>在线期间每次 production tick 调用</summary>
        void Produce(float deltaSeconds, float efficiencyMultiplier);

        /// <summary>离线结算时调用，返回离线期间的总产量</summary>
        OfflineProductionRecord CalculateOfflineProduction(float totalOfflineSeconds, float efficiency);
    }

    public struct OfflineProductionRecord
    {
        public string ProducerId;
        public ResourceType OutputType;
        public float TotalProduced;
        public float EfficiencyUsed;
        public bool WasCapped;
        public float CappedAmount;
    }

    // Forward reference to ResourceType for this namespace
    public enum ResourceType
    {
        RawMeat = 0, Wood = 1, Coal = 2, IronOre = 3,
        Steel = 4, FireCrystal = 5, HeroSoulStone = 6, Gems = 7,
    }
}
