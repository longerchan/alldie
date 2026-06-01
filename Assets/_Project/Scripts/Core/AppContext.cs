using System;
using System.Collections.Generic;
using FrostShelter.SaveSystem;

namespace FrostShelter.Core
{
    /// <summary>
    /// 全局应用上下文，持有当前游戏运行时的所有核心状态引用
    /// 不作为Service注册，而是由GameManager直接管理
    /// </summary>
    public class AppContext
    {
        public GameState CurrentState { get; set; } = GameState.Title;
        public DateTime LastOnlineTime { get; set; }
        public float TotalPlayTimeSeconds { get; set; }
        public bool IsFirstLaunch { get; set; } = true;
        public int CurrentSaveSlot { get; set; } = -1;

        // 事件参数类型定义
        public class ResourceChangedArgs
        {
            public ResourceType Type;
            public float OldValue;
            public float NewValue;
            public float Delta;
            public ResourceChangeReason Reason;
        }

        public class TemperatureChangedArgs
        {
            public float CurrentTemp;
            public float PreviousTemp;
            public TemperatureChangeReason Reason;
            public bool IsBlizzardActive;
        }

        public class BuildingUpgradeArgs
        {
            public BuildingId BuildingId;
            public int OldLevel;
            public int NewLevel;
        }
    }

    public enum ResourceChangeReason
    {
        Production,
        Collection,
        Upgrade,
        Event,
        Merchant,
        Expedition,
        Story,
        Offline,
    }

    public enum TemperatureChangeReason
    {
        FurnaceUpgrade,
        BlizzardStart,
        BlizzardEnd,
        Event,
    }
}
