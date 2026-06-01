using System.Collections.Generic;

namespace FrostShelter.Battle
{
    /// <summary>
    /// 战斗单位。可以是英雄或兵种小队。
    /// </summary>
    public class BattleUnit
    {
        public string InstanceId;
        public string DisplayName;
        public bool IsHero;
        public string SourceHeroId;

        public TroopType TroopType;
        public float Atk;
        public float Def;
        public float Hp;
        public float MaxHp;
        public float Speed;
        public float CritRate = 0.1f;
        public float CritDamage = 1.5f;

        // 仅英雄单位有技能
        public List<BattleSkill> Skills = new();

        // 战斗状态
        public bool IsAlive = true;
        public int PositionRow;   // 0 = front, 1 = back
        public int PositionSlot;  // 0, 1, 2

        // Buff/Debuff
        public List<BattleBuff> ActiveBuffs = new();
        public float HpRatio => MaxHp > 0f ? Hp / MaxHp : 0f;

        public void TakeDamage(float damage, out bool isDead)
        {
            Hp -= damage;
            if (Hp <= 0f)
            {
                Hp = 0f;
                IsAlive = false;
                isDead = true;
            }
            else
            {
                isDead = false;
            }
        }

        public void Heal(float amount)
        {
            Hp = System.Math.Min(Hp + amount, MaxHp);
        }

        public void ApplyBuff(BattleBuff buff)
        {
            ActiveBuffs.Add(buff);
        }

        public void TickBuffs()
        {
            for (int i = ActiveBuffs.Count - 1; i >= 0; i--)
            {
                ActiveBuffs[i].RemainingTurns--;
                if (ActiveBuffs[i].RemainingTurns <= 0)
                {
                    ActiveBuffs.RemoveAt(i);
                }
            }
        }

        public float GetEffectiveAtk()
        {
            float bonus = 0f;
            foreach (var buff in ActiveBuffs)
            {
                if (buff.Type == BuffType.AtkUp) bonus += buff.Value;
                else if (buff.Type == BuffType.AtkDown) bonus -= buff.Value;
            }
            return Atk * (1f + bonus);
        }

        public float GetEffectiveDef()
        {
            float bonus = 0f;
            foreach (var buff in ActiveBuffs)
            {
                if (buff.Type == BuffType.DefUp) bonus += buff.Value;
                else if (buff.Type == BuffType.DefDown) bonus -= buff.Value;
            }
            return Def * (1f + bonus);
        }
    }

    public class BattleSkill
    {
        public string SkillId;
        public string SkillName;
        public SkillTriggerType TriggerType;
        public float[] ValuesPerLevel;
        public int CurrentLevel;
        public int CooldownTurns;
        public int CurrentCooldown;
        public bool IsReady => CurrentCooldown <= 0;

        public float GetValue()
        {
            if (ValuesPerLevel == null || ValuesPerLevel.Length == 0) return 0f;
            int index = System.Math.Min(CurrentLevel - 1, ValuesPerLevel.Length - 1);
            return ValuesPerLevel[index];
        }

        public void Use() { CurrentCooldown = CooldownTurns; }
        public void TickCooldown() { if (CurrentCooldown > 0) CurrentCooldown--; }
    }

    public enum SkillTriggerType { Passive, OnAttack, OnHurt, OnBattleStart, Manual }

    public class BattleBuff
    {
        public string Id;
        public BuffType Type;
        public float Value;
        public int RemainingTurns;
    }

    public enum BuffType { AtkUp, AtkDown, DefUp, DefDown, SpeedUp, SpeedDown, Dot, Hot }
}
