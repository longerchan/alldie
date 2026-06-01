using FrostShelter.SaveSystem;
using System.Collections.Generic;

namespace FrostShelter.Resource
{
    /// <summary>
    /// 资源消耗定义，用于建筑升级、科技研究、训练等操作。
    /// </summary>
    public class ResourceCost
    {
        public Dictionary<ResourceType, float> Costs { get; } = new();
        public bool IsEmpty => Costs.Count == 0;

        public ResourceCost() { }

        public ResourceCost(ResourceType type, float amount)
        {
            Costs[type] = amount;
        }

        public void Add(ResourceType type, float amount)
        {
            if (Costs.ContainsKey(type))
                Costs[type] += amount;
            else
                Costs[type] = amount;
        }

        public float GetCost(ResourceType type)
        {
            return Costs.TryGetValue(type, out var v) ? v : 0f;
        }

        public static ResourceCost operator +(ResourceCost a, ResourceCost b)
        {
            var result = new ResourceCost();
            foreach (var kvp in a.Costs)
                result.Add(kvp.Key, kvp.Value);
            foreach (var kvp in b.Costs)
                result.Add(kvp.Key, kvp.Value);
            return result;
        }
    }
}
