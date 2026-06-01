using System;

namespace FrostShelter.Exploration
{
    /// <summary>
    /// 六边形地图上的一个节点。
    /// </summary>
    [Serializable]
    public class HexNode
    {
        public HexCoord Coordinate;
        public NodeType Type;
        public bool IsRevealed;
        public bool IsVisited;
        public int DifficultyLevel;
        public string NodeConfigId;

        // 节点特定数据
        public ResourceNodeData ResourceData;
        public BattleNodeData BattleData;
        public EventNodeData EventData;
        public MerchantNodeData MerchantData;

        public HexNode(HexCoord coord, NodeType type, int difficulty = 1)
        {
            Coordinate = coord;
            Type = type;
            DifficultyLevel = difficulty;
        }

        public bool CanEnter => IsRevealed && !IsVisited;
    }

    public enum NodeType
    {
        Start = 0,
        Resource = 1,
        Battle = 2,
        Event = 3,
        Merchant = 4,
        Boss = 5,
    }

    [Serializable]
    public class ResourceNodeData
    {
        public ResourceType ResourceType;
        public float Amount;
        public bool RequiresHero;
        public int GatherTimeSeconds;
    }

    [Serializable]
    public class BattleNodeData
    {
        public string EnemyConfigId;
        public int EnemyCount;
        public int EnemyLevel;
        public float LootMultiplier = 1f;
    }

    [Serializable]
    public class EventNodeData
    {
        public string EventConfigId;
    }

    [Serializable]
    public class MerchantNodeData
    {
        public string[] ItemIds;
        public float PriceMultiplier = 1f;
    }
}
