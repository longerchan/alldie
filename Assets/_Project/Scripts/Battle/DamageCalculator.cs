using UnityEngine;

namespace FrostShelter.Battle
{
    /// <summary>
    /// 伤害计算器。兵种克制三角 + 防御减免 + 暴击判定。
    /// 克制关系: 盾→矛→射→盾
    /// </summary>
    public static class DamageCalculator
    {
        private const float ADVANTAGE_MULTIPLIER = 1.25f;
        private const float DISADVANTAGE_MULTIPLIER = 0.75f;
        private const float DEFENSE_FACTOR = 100f;
        private const float RANDOM_MIN = 0.9f;
        private const float RANDOM_MAX = 1.1f;

        public static TroopAdvantage GetTroopAdvantage(TroopType attacker, TroopType defender)
        {
            return (attacker, defender) switch
            {
                (TroopType.Shield, TroopType.Spear) => TroopAdvantage.Advantage,
                (TroopType.Spear, TroopType.Archer) => TroopAdvantage.Advantage,
                (TroopType.Archer, TroopType.Shield) => TroopAdvantage.Advantage,
                (TroopType.Spear, TroopType.Shield) => TroopAdvantage.Disadvantage,
                (TroopType.Archer, TroopType.Spear) => TroopAdvantage.Disadvantage,
                (TroopType.Shield, TroopType.Archer) => TroopAdvantage.Disadvantage,
                _ => TroopAdvantage.Neutral,
            };
        }

        public static float CalculateDamage(
            BattleUnit attacker,
            BattleUnit defender,
            TroopAdvantage advantage,
            out DamageReport report)
        {
            // 基础伤害
            float baseDamage = attacker.GetEffectiveAtk();

            // 兵种克制加成
            float advantageMod = advantage switch
            {
                TroopAdvantage.Advantage => ADVANTAGE_MULTIPLIER,
                TroopAdvantage.Disadvantage => DISADVANTAGE_MULTIPLIER,
                _ => 1.0f,
            };

            // 随机浮动
            float randomFactor = Random.Range(RANDOM_MIN, RANDOM_MAX);

            // 暴击判定
            bool isCrit = Random.value < attacker.CritRate;
            float critMod = isCrit ? attacker.CritDamage : 1.0f;

            // 原始伤害（通过防御前）
            float rawDamage = baseDamage * advantageMod * randomFactor * critMod;

            // 防御减免
            float effectiveDef = defender.GetEffectiveDef();
            float mitigatedDamage = rawDamage * (DEFENSE_FACTOR / (DEFENSE_FACTOR + effectiveDef));
            float finalDamage = Mathf.Max(1f, mitigatedDamage);

            report = new DamageReport
            {
                RawDamage = rawDamage,
                MitigatedDamage = finalDamage,
                ActualDamage = finalDamage,
                IsCrit = isCrit,
                Advantage = advantage,
            };

            return finalDamage;
        }

        /// <summary>快速计算（无报告）</summary>
        public static float CalculateDamageQuick(BattleUnit attacker, BattleUnit defender)
        {
            var advantage = GetTroopAdvantage(attacker.TroopType, defender.TroopType);
            return CalculateDamage(attacker, defender, advantage, out _);
        }
    }

    public struct DamageReport
    {
        public float RawDamage;
        public float MitigatedDamage;
        public float ActualDamage;
        public bool IsCrit;
        public TroopAdvantage Advantage;
    }
}
