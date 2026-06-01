using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FrostShelter.Battle
{
    /// <summary>
    /// 回合制战斗模拟器。统一驱动自动战斗和半手动战斗。
    /// </summary>
    public class BattleSimulator
    {
        public BattleFormation PlayerFormation { get; private set; }
        public BattleFormation EnemyFormation { get; private set; }
        public float BattleSpeed { get; set; } = 1f;
        public BattlePhase CurrentPhase { get; private set; } = BattlePhase.Preparation;
        public int CurrentTurn { get; private set; }
        public int MaxTurns { get; set; } = 100;

        private System.Random _rng;
        private int _seed;

        public event Action<int, BattleAction> OnActionExecuted;
        public event Action<BattlePhase> OnPhaseChanged;

        public void Setup(BattleFormation player, BattleFormation enemy, int seed = 0)
        {
            PlayerFormation = player;
            EnemyFormation = enemy;
            _seed = seed;
            _rng = new System.Random(seed != 0 ? seed : Environment.TickCount);
            CurrentTurn = 0;
            CurrentPhase = BattlePhase.Preparation;
        }

        public void SetPhase(BattlePhase phase)
        {
            CurrentPhase = phase;
            OnPhaseChanged?.Invoke(phase);
        }

        /// <summary>执行一个完整回合</summary>
        public BattleTurnResult ExecuteTurn()
        {
            if (CurrentPhase == BattlePhase.Completed) return null;

            CurrentTurn++;
            SetPhase(BattlePhase.Action);

            var result = new BattleTurnResult { Turn = CurrentTurn };
            var allUnits = GetAllUnitsBySpeed();

            foreach (var unit in allUnits)
            {
                if (!unit.IsAlive) continue;
                if (IsFormationDead(PlayerFormation) || IsFormationDead(EnemyFormation))
                    break;

                var target = SelectTarget(unit);
                if (target == null) continue;

                var action = ExecuteAction(unit, target);
                result.Actions.Add(action);

                OnActionExecuted?.Invoke(CurrentTurn, action);

                if (!target.IsAlive)
                {
                    result.Casualties.Add(target);
                }
            }

            // 回合结束处理
            foreach (var unit in allUnits)
            {
                unit.TickBuffs();
            }

            SetPhase(BattlePhase.Resolution);

            // 判定结束条件
            if (IsFormationDead(PlayerFormation) || IsFormationDead(EnemyFormation) || CurrentTurn >= MaxTurns)
            {
                SetPhase(BattlePhase.Completed);
                result.IsBattleEnded = true;
                result.IsPlayerVictory = !IsFormationDead(PlayerFormation) && IsFormationDead(EnemyFormation);
            }

            return result;
        }

        /// <summary>快速模拟到结束（自动战斗用）</summary>
        public BattleResult SimulateToEnd()
        {
            BattleTurnResult lastTurn = null;
            while (CurrentPhase != BattlePhase.Completed)
            {
                lastTurn = ExecuteTurn();
            }

            return BuildResult();
        }

        /// <summary>半手动战斗：执行玩家方技能释放</summary>
        public bool ExecuteManualSkill(string unitId, string skillId)
        {
            var unit = PlayerFormation.GetAllAlive().Find(u => u.InstanceId == unitId);
            if (unit == null) return false;

            var skill = unit.Skills.Find(s => s.SkillId == skillId && s.IsReady);
            if (skill == null) return false;

            var target = SelectTarget(unit);
            if (target == null) return false;

            float damage = DamageCalculator.CalculateDamageQuick(unit, target);
            damage *= skill.GetValue();
            target.TakeDamage(damage, out bool isDead);

            skill.Use();
            OnActionExecuted?.Invoke(CurrentTurn, new BattleAction
            {
                Attacker = unit,
                Defender = target,
                DamageDealt = damage,
                SkillUsed = skill,
                IsCrit = false,
            });

            return true;
        }

        private BattleAction ExecuteAction(BattleUnit attacker, BattleUnit defender)
        {
            var advantage = DamageCalculator.GetTroopAdvantage(attacker.TroopType, defender.TroopType);
            float damage = DamageCalculator.CalculateDamage(attacker, defender, advantage, out var report);

            // 被动技能加成
            foreach (var skill in attacker.Skills)
            {
                if (skill.TriggerType == SkillTriggerType.OnAttack && skill.IsReady)
                {
                    damage *= skill.GetValue();
                    skill.Use();
                }
            }

            defender.TakeDamage(damage, out _);

            return new BattleAction
            {
                Attacker = attacker,
                Defender = defender,
                DamageDealt = damage,
                Advantage = advantage,
                IsCrit = report.IsCrit,
            };
        }

        private BattleUnit SelectTarget(BattleUnit attacker)
        {
            bool isPlayerUnit = PlayerFormation.GetAllAlive().Contains(attacker);
            var enemyFormation = isPlayerUnit ? EnemyFormation : PlayerFormation;

            // 优先攻击前排
            var frontTargets = enemyFormation.GetAliveFrontRow();
            if (frontTargets.Count > 0)
                return frontTargets[_rng.Next(frontTargets.Count)];

            var backTargets = enemyFormation.GetAliveBackRow();
            if (backTargets.Count > 0)
                return backTargets[_rng.Next(backTargets.Count)];

            return null;
        }

        private List<BattleUnit> GetAllUnitsBySpeed()
        {
            var units = new List<BattleUnit>();
            units.AddRange(PlayerFormation.GetAllAlive());
            units.AddRange(EnemyFormation.GetAllAlive());
            units.Sort((a, b) => b.Speed.CompareTo(a.Speed));
            return units;
        }

        private bool IsFormationDead(BattleFormation formation) => formation.IsAllDead;

        private BattleResult BuildResult()
        {
            return new BattleResult
            {
                IsPlayerVictory = !IsFormationDead(PlayerFormation) && IsFormationDead(EnemyFormation),
                TotalTurns = CurrentTurn,
                PlayerCasualties = PlayerFormation.GetAllAlive().Count,
                EnemyCasualties = EnemyFormation.GetAllAlive().Count,
            };
        }
    }

    public enum BattlePhase { Preparation, Action, Resolution, Completed }

    public class BattleTurnResult
    {
        public int Turn;
        public List<BattleAction> Actions = new();
        public List<BattleUnit> Casualties = new();
        public bool IsBattleEnded;
        public bool IsPlayerVictory;
    }

    public class BattleAction
    {
        public BattleUnit Attacker;
        public BattleUnit Defender;
        public float DamageDealt;
        public TroopAdvantage Advantage;
        public bool IsCrit;
        public BattleSkill SkillUsed;
    }

    public class BattleResult
    {
        public bool IsPlayerVictory;
        public int TotalTurns;
        public int PlayerCasualties;
        public int EnemyCasualties;
    }
}
