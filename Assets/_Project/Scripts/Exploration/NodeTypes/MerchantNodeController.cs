using FrostShelter.Core;
using FrostShelter.SaveSystem;
using System.Collections.Generic;
using FrostShelter.Resource;

namespace FrostShelter.Exploration.NodeTypes
{
    /// <summary>
    /// 商贩节点控制器。冰上游商，稀有物资交换。
    /// </summary>
    public class MerchantNodeController
    {
        public HexNode Node { get; private set; }
        private ResourceManager _resourceManager;

        public List<MerchantItem> Items { get; private set; } = new();

        public MerchantNodeController(HexNode node, ResourceManager resourceManager)
        {
            Node = node;
            _resourceManager = resourceManager;
            GenerateItems();
        }

        private void GenerateItems()
        {
            Items.Clear();
            int itemCount = 3 + Node.DifficultyLevel;
            var rng = new System.Random();

            for (int i = 0; i < itemCount; i++)
            {
                var costType = (ResourceType)(rng.Next(0, 4));
                var rewardType = (ResourceType)(rng.Next(2, 6));
                Items.Add(new MerchantItem
                {
                    ItemId = $"merchant_node_{i}",
                    DisplayName = $"{costType} → {rewardType}",
                    Cost = new ResourceCost(costType, 5f + rng.Next(1, 10) * Node.DifficultyLevel),
                    RewardType = rewardType,
                    RewardAmount = 2f + rng.Next(1, 5) * Node.PriceMultiplier,
                });
            }
        }

        public bool PurchaseItem(int index)
        {
            if (index < 0 || index >= Items.Count) return false;
            var item = Items[index];
            if (item.IsPurchased) return false;

            if (!_resourceManager.CanAfford(item.Cost)) return false;
            _resourceManager.Spend(item.Cost, ResourceChangeReason.Merchant);
            _resourceManager.AddResource(item.RewardType, item.RewardAmount,
                ResourceChangeReason.Merchant);

            item.IsPurchased = true;
            return true;
        }
    }
}
