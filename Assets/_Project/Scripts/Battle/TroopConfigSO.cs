using System;
using UnityEngine;

namespace FrostShelter.Battle
{
    /// <summary>
    /// 兵种配置。定义三种兵种的属性成长曲线。
    /// </summary>
    [Serializable]
    public class TroopConfigSO : ScriptableObject
    {
        public string ConfigId;
        public TroopType TroopType;
        public string DisplayName;
        public string Description;

        public float BaseAtk = 5f;
        public float BaseDef = 3f;
        public float BaseHp = 20f;
        public float BaseSpeed = 4f;

        // 克制关系描述
        public TroopType StrongAgainst;
        public TroopType WeakAgainst;

        // 每级成长
        public float AtkPerLevel = 2f;
        public float DefPerLevel = 1f;
        public float HpPerLevel = 8f;

        // 训练消耗
        public float TrainingCostWood;
        public float TrainingCostRawMeat;
        public float TrainingCostIronOre;
        public float TrainingTimePerUnit = 30f;

        public TroopData CreateTroopData(int count, int level)
        {
            float levelAtk = BaseAtk + AtkPerLevel * level;
            float levelDef = BaseDef + DefPerLevel * level;
            float levelHp = BaseHp + HpPerLevel * level;

            return new TroopData
            {
                ConfigId = ConfigId,
                TroopType = TroopType,
                Count = count,
                AliveCount = count,
                Atk = levelAtk,
                Def = levelDef,
                TotalHp = levelHp * count,
                MaxHp = levelHp * count,
                Speed = BaseSpeed,
            };
        }
    }
}
