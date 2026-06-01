using System;
using System.Collections.Generic;
using FrostShelter.Core;

namespace FrostShelter.Battle
{
    /// <summary>
    /// 战斗总管理器。提供自动战斗和半手动战斗两个入口。
    /// </summary>
    public class BattleManager : IService
    {
        public bool IsBattleActive { get; private set; }
        public BattleType CurrentBattleType { get; private set; }
        public BattleSimulator Simulator { get; private set; }

        private EventDispatcher _events;

        public event Action<BattleResult> OnBattleCompleted;
        public event Action<BattleUnit> OnUnitDied;

        public void Initialize() { }

        public void Shutdown()
        {
            StopBattle();
        }

        public void SetDependencies(EventDispatcher events)
        {
            _events = events;
        }

        /// <summary>启动自动战斗（推图关卡）</summary>
        public BattleResult StartAutoBattle(BattleFormation playerFormation,
            BattleFormation enemyFormation)
        {
            IsBattleActive = true;
            CurrentBattleType = BattleType.AutoCampaign;

            Simulator = new BattleSimulator();
            Simulator.Setup(playerFormation, enemyFormation);
            Simulator.OnActionExecuted += OnAutoAction;

            var result = Simulator.SimulateToEnd();

            IsBattleActive = false;
            OnBattleCompleted?.Invoke(result);
            _events?.Dispatch(GameEventType.BattleCompleted);
            return result;
        }

        /// <summary>启动半手动战斗（探险遭遇）</summary>
        public void StartRogueBattle(BattleFormation playerFormation,
            BattleFormation enemyFormation)
        {
            IsBattleActive = true;
            CurrentBattleType = BattleType.RogueExploration;

            Simulator = new BattleSimulator();
            Simulator.Setup(playerFormation, enemyFormation);
            Simulator.OnActionExecuted += OnRogueAction;

            Simulator.SetPhase(BattlePhase.Preparation);
            Simulator.SetPhase(BattlePhase.Action);
        }

        /// <summary>半手动战斗：执行一个回合</summary>
        public BattleTurnResult ExecuteRogueTurn()
        {
            if (!IsBattleActive || CurrentBattleType != BattleType.RogueExploration)
                return null;

            var result = Simulator.ExecuteTurn();

            if (result?.IsBattleEnded == true)
            {
                IsBattleActive = false;
                var battleResult = new BattleResult
                {
                    IsPlayerVictory = result.IsPlayerVictory,
                    TotalTurns = result.Turn,
                };
                OnBattleCompleted?.Invoke(battleResult);
                _events?.Dispatch(GameEventType.BattleCompleted);
            }

            return result;
        }

        /// <summary>半手动战斗：释放英雄技能</summary>
        public bool CastSkill(string heroId, string skillId)
        {
            if (!IsBattleActive || CurrentBattleType != BattleType.RogueExploration)
                return false;
            return Simulator.ExecuteManualSkill(heroId, skillId);
        }

        /// <summary>半手动战斗：跳过剩余回合快速结算</summary>
        public BattleResult SkipToEnd()
        {
            if (Simulator == null) return null;
            var result = Simulator.SimulateToEnd();
            IsBattleActive = false;
            OnBattleCompleted?.Invoke(result);
            _events?.Dispatch(GameEventType.BattleCompleted);
            return result;
        }

        public void StopBattle()
        {
            IsBattleActive = false;
            Simulator = null;
        }

        private void OnAutoAction(int turn, BattleAction action)
        {
            if (!action.Defender.IsAlive)
            {
                OnUnitDied?.Invoke(action.Defender);
                _events?.Dispatch(GameEventType.BattleUnitDied);
            }
        }

        private void OnRogueAction(int turn, BattleAction action)
        {
            if (!action.Defender.IsAlive)
            {
                OnUnitDied?.Invoke(action.Defender);
                _events?.Dispatch(GameEventType.BattleUnitDied);
            }
        }

        /// <summary>将英雄列表转换为战斗阵型</summary>
        public static BattleFormation BuildFormationFromHeroes(
            List<Hero.Hero> heroes,
            List<TroopData> troops)
        {
            var formation = new BattleFormation();
            int frontIdx = 0, backIdx = 0;

            // 英雄放置在阵型中
            foreach (var hero in heroes)
            {
                if (hero == null) continue;
                var unit = CreateBattleUnitFromHero(hero);
                if (unit == null) continue;

                if (hero.FormationSlotIndex < 3)
                {
                    if (frontIdx < 3) formation.SetUnit(0, frontIdx++, unit);
                }
                else
                {
                    if (backIdx < 3) formation.SetUnit(1, backIdx++, unit);
                }
            }

            // 兵种单位填充剩余位置
            if (troops != null)
            {
                foreach (var troop in troops)
                {
                    var unit = CreateBattleUnitFromTroop(troop);
                    if (unit == null) continue;

                    if (troop.PositionRow == 0 && frontIdx < 3)
                        formation.SetUnit(0, frontIdx++, unit);
                    else if (troop.PositionRow == 1 && backIdx < 3)
                        formation.SetUnit(1, backIdx++, unit);
                }
            }

            return formation;
        }

        private static BattleUnit CreateBattleUnitFromHero(Hero.Hero hero)
        {
            if (hero == null) return null;

            var unit = new BattleUnit
            {
                InstanceId = hero.Id,
                DisplayName = hero.Config?.HeroName ?? "Unknown",
                IsHero = true,
                SourceHeroId = hero.Id,
                TroopType = TroopType.Shield, // 可由英雄配置决定
                Atk = hero.Atk,
                Def = hero.Def,
                Hp = hero.Hp,
                MaxHp = hero.Hp,
                Speed = hero.Speed,
            };

            foreach (var skill in hero.Skills)
            {
                if (skill.IsUnlocked)
                {
                    unit.Skills.Add(new BattleSkill
                    {
                        SkillId = skill.SkillId,
                        SkillName = skill.Config.SkillName,
                        TriggerType = skill.Config.TriggerType,
                        ValuesPerLevel = skill.Config.ValuesPerLevel,
                        CurrentLevel = skill.CurrentLevel,
                        CooldownTurns = skill.Config.CooldownTurns,
                    });
                }
            }

            return unit;
        }

        private static BattleUnit CreateBattleUnitFromTroop(TroopData troop)
        {
            if (troop == null) return null;

            return new BattleUnit
            {
                InstanceId = Guid.NewGuid().ToString(),
                DisplayName = troop.ConfigId,
                IsHero = false,
                TroopType = troop.TroopType,
                Atk = troop.Atk,
                Def = troop.Def,
                Hp = troop.TotalHp,
                MaxHp = troop.MaxHp,
                Speed = troop.Speed,
            };
        }
    }

    public enum BattleType { AutoCampaign, RogueExploration }

    public class TroopData
    {
        public string ConfigId;
        public TroopType TroopType;
        public int Count;
        public int AliveCount;
        public float Atk;
        public float Def;
        public float TotalHp;
        public float MaxHp;
        public float Speed;
        public int PositionRow;
        public int PositionSlot;
    }
}
