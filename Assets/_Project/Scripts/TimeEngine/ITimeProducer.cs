using FrostShelter.SaveSystem;

namespace FrostShelter.TimeEngine
{
    public interface ITimeProducer
    {
        string ProducerId { get; }
        void Produce(float deltaSeconds, float efficiencyMultiplier);
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
}
