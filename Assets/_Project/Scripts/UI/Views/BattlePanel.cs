using System;
using System.Collections.Generic;

namespace FrostShelter.UI.Views
{
    /// <summary>
    /// 战斗面板。显示双方阵型、回合进度、技能按钮。
    /// </summary>
    public class BattlePanel : BasePanel
    {
        public BattleUnitSlot[] PlayerFrontRow = new BattleUnitSlot[3];
        public BattleUnitSlot[] PlayerBackRow = new BattleUnitSlot[3];
        public BattleUnitSlot[] EnemyFrontRow = new BattleUnitSlot[3];
        public BattleUnitSlot[] EnemyBackRow = new BattleUnitSlot[3];

        public string BattleLog;
        public int CurrentTurn;
        public bool IsPlayerTurn;
        public bool IsRogueMode;  // 半手动模式

        public event Action<string, string> OnSkillCast;       // heroId, skillId
        public event Action<int, int, int, int> OnSwapUnit;    // fromRow, fromSlot, toRow, toSlot
        public event Action OnSkipTurn;
        public event Action OnAutoResolve;

        public BattlePanel() : base("BattlePanel", PanelLayer.Popup) { }

        protected override void OnShow()
        {
            CurrentTurn = 0;
            BattleLog = "";
        }

        protected override void OnHide() { }

        public override void Refresh()
        {
            RefreshUnitDisplays();
        }

        public override void OnBackPressed()
        {
            // 战斗中不可退出
        }

        private void RefreshUnitDisplays()
        {
            // 刷新血条、状态、技能冷却
        }

        /// <summary>显示伤害飘字</summary>
        public void ShowDamageNumber(BattleUnitSlot target, float damage, bool isCrit)
        {
            // 在目标单位上弹出伤害数字
        }

        /// <summary>添加战斗日志</summary>
        public void AppendBattleLog(string log)
        {
            BattleLog += $"[回合{CurrentTurn}] {log}\n";
        }
    }

    [Serializable]
    public class BattleUnitSlot
    {
        public string UnitId;
        public string DisplayName;
        public float HpRatio;
        public bool IsAlive;
        public bool IsHero;
        public List<SkillSlot> ReadySkills;
    }

    [Serializable]
    public class SkillSlot
    {
        public string SkillId;
        public string SkillName;
        public bool IsReady;
        public int CooldownRemaining;
    }
}
