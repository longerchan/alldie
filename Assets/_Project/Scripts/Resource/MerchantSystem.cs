using FrostShelter.SaveSystem;
using System;
using System.Collections.Generic;
using FrostShelter.Core;

namespace FrostShelter.Resource
{
    /// <summary>
    /// 游商系统。每3天刷新一次商品，支持以物易物。
    /// </summary>
    public class MerchantSystem : IService
    {
        public bool IsMerchantAvailable { get; private set; }
        public long LastRefreshTimestamp { get; private set; }
        public List<MerchantItem> CurrentItems { get; private set; } = new();

        private const float REFRESH_INTERVAL_SECONDS = 259200f; // 3 days
        private const int MAX_ITEMS = 6;

        private EventDispatcher _events;
        private ResourceManager _resourceManager;
        private System.Random _rng = new();

        public event Action OnMerchantRefreshed;
        public event Action<MerchantItem> OnItemPurchased;

        public void Initialize()
        {
            LastRefreshTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public void Shutdown()
        {
            CurrentItems.Clear();
        }

        public void SetDependencies(EventDispatcher events, ResourceManager resourceManager)
        {
            _events = events;
            _resourceManager = resourceManager;
        }

        public void OnMinuteTick()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now - LastRefreshTimestamp >= REFRESH_INTERVAL_SECONDS)
            {
                RefreshMerchant();
            }
        }

        public void RefreshMerchant()
        {
            IsMerchantAvailable = true;
            LastRefreshTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            CurrentItems.Clear();

            // 随机生成 3-6 个物品
            int itemCount = _rng.Next(3, MAX_ITEMS + 1);
            for (int i = 0; i < itemCount; i++)
            {
                CurrentItems.Add(GenerateRandomItem());
            }

            OnMerchantRefreshed?.Invoke();
            _events?.Dispatch(GameEventType.RandomEventTriggered); // 复用事件
        }

        public bool PurchaseItem(int itemIndex)
        {
            if (!IsMerchantAvailable || itemIndex < 0 || itemIndex >= CurrentItems.Count)
                return false;

            var item = CurrentItems[itemIndex];
            if (item.IsPurchased) return false;

            // 检查支付资源
            if (!_resourceManager.CanAfford(item.Cost)) return false;

            _resourceManager.Spend(item.Cost, ResourceChangeReason.Merchant);
            _resourceManager.AddResource(item.RewardType, item.RewardAmount,
                ResourceChangeReason.Merchant);

            item.IsPurchased = true;
            OnItemPurchased?.Invoke(item);
            return true;
        }

        private MerchantItem GenerateRandomItem()
        {
            // 随机生成交易项：用常见资源换稀有资源
            var allTypes = Enum.GetValues(typeof(ResourceType));
            var costType = (ResourceType)allTypes.GetValue(
                _rng.Next(0, Math.Min(allTypes.Length, 4))); // 前4种为常见资源
            var rewardType = (ResourceType)allTypes.GetValue(
                _rng.Next(2, allTypes.Length)); // 较稀有资源

            return new MerchantItem
            {
                ItemId = Guid.NewGuid().ToString(),
                DisplayName = $"{costType} → {rewardType}",
                Cost = new ResourceCost(costType,
                    UnityEngine.Random.Range(10f, 50f)),
                RewardType = rewardType,
                RewardAmount = UnityEngine.Random.Range(1f, 10f),
            };
        }
    }

    [Serializable]
    public class MerchantItem
    {
        public string ItemId;
        public string DisplayName;
        public ResourceCost Cost;
        public ResourceType RewardType;
        public float RewardAmount;
        public bool IsPurchased;
    }
}
