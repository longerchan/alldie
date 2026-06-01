using System;
using FrostShelter.Core;
using FrostShelter.SaveSystem;

namespace FrostShelter.Resource
{
    /// <summary>
    /// 资源管理器。负责所有资源的增删查改、容量管理。
    /// 是资源数据的唯一写入口，所有资源变更都必须通过此Manager。
    /// </summary>
    public class ResourceManager : IService
    {
        public ResourceStorageData Resources { get; private set; } = new();
        public float StorageCapacity { get; private set; } = 500f; // 基础仓库容量

        public event Action<ResourceType, float, float, ResourceChangeReason> OnResourceChanged;

        private EventDispatcher _events;

        public void Initialize() { }

        public void Shutdown() { }

        public void SetEventDispatcher(EventDispatcher events)
        {
            _events = events;
        }

        public void LoadFromSaveData(ResourceStorageData data)
        {
            Resources = data;
        }

        public float GetAmount(ResourceType type)
        {
            return Resources.GetAmount(type);
        }

        public bool CanAfford(ResourceCost cost)
        {
            if (cost == null || cost.IsEmpty) return true;
            foreach (var kvp in cost.Costs)
            {
                if (GetAmount(kvp.Key) < kvp.Value) return false;
            }
            return true;
        }

        public bool Spend(ResourceCost cost, ResourceChangeReason reason = ResourceChangeReason.Upgrade)
        {
            if (!CanAfford(cost)) return false;
            foreach (var kvp in cost.Costs)
            {
                RemoveResource(kvp.Key, kvp.Value, reason);
            }
            return true;
        }

        public void AddResource(ResourceType type, float amount, ResourceChangeReason reason)
        {
            float oldValue = GetAmount(type);
            float newValue = oldValue + amount;

            // 应用存储上限
            if (IsStorableResource(type) && newValue > StorageCapacity)
            {
                newValue = StorageCapacity;
            }

            Resources.SetAmount(type, newValue);

            OnResourceChanged?.Invoke(type, oldValue, newValue, reason);
            _events?.Dispatch(GameEventType.ResourceChanged,
                new AppContext.ResourceChangedArgs
                {
                    Type = type,
                    OldValue = oldValue,
                    NewValue = newValue,
                    Delta = newValue - oldValue,
                    Reason = reason,
                });

            if (IsStorableResource(type) && newValue >= StorageCapacity * 0.99f)
            {
                _events?.Dispatch(GameEventType.ResourceReachedMax);
            }
        }

        public void RemoveResource(ResourceType type, float amount, ResourceChangeReason reason)
        {
            float oldValue = GetAmount(type);
            float newValue = Math.Max(0f, oldValue - amount);
            Resources.SetAmount(type, newValue);

            OnResourceChanged?.Invoke(type, oldValue, newValue, reason);
            _events?.Dispatch(GameEventType.ResourceChanged,
                new AppContext.ResourceChangedArgs
                {
                    Type = type,
                    OldValue = oldValue,
                    NewValue = newValue,
                    Delta = newValue - oldValue,
                    Reason = reason,
                });
        }

        public float GetFillRatio(ResourceType type)
        {
            if (!IsStorableResource(type)) return 0f;
            return GetAmount(type) / StorageCapacity;
        }

        public bool IsAtCapacity(ResourceType type)
        {
            return GetFillRatio(type) >= 0.99f;
        }

        public void SetStorageCapacity(float capacity)
        {
            StorageCapacity = capacity;
            _events?.Dispatch(GameEventType.ResourceCapacityChanged);
        }

        private static bool IsStorableResource(ResourceType type)
        {
            return type switch
            {
                ResourceType.HeroSoulStone => false,
                ResourceType.Gems => false,
                _ => true,
            };
        }
    }
}
