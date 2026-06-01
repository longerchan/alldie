using System.Collections.Generic;
using System.Linq;

namespace FrostShelter.Battle
{
    /// <summary>
    /// 战斗阵型。3前3后的6格站位系统。
    /// </summary>
    public class BattleFormation
    {
        public readonly BattleUnit?[] FrontRow = new BattleUnit[3];
        public readonly BattleUnit?[] BackRow = new BattleUnit[3];

        public int TotalAlive => GetAllAlive().Count;
        public bool IsAllDead => TotalAlive == 0;

        public BattleUnit? GetFrontUnit(int index) =>
            index >= 0 && index < 3 ? FrontRow[index] : null;

        public BattleUnit? GetBackUnit(int index) =>
            index >= 0 && index < 3 ? BackRow[index] : null;

        public void SetUnit(int row, int slot, BattleUnit unit)
        {
            unit.PositionRow = row;
            unit.PositionSlot = slot;
            if (row == 0) FrontRow[slot] = unit;
            else BackRow[slot] = unit;
        }

        public void SwapUnits(int fromRow, int fromSlot, int toRow, int toSlot)
        {
            var unitA = fromRow == 0 ? FrontRow[fromSlot] : BackRow[fromSlot];
            var unitB = toRow == 0 ? FrontRow[toSlot] : BackRow[toSlot];

            // Swap
            if (fromRow == 0) FrontRow[fromSlot] = unitB;
            else BackRow[fromSlot] = unitB;

            if (toRow == 0) FrontRow[toSlot] = unitA;
            else BackRow[toSlot] = unitA;

            if (unitA != null) { unitA.PositionRow = toRow; unitA.PositionSlot = toSlot; }
            if (unitB != null) { unitB.PositionRow = fromRow; unitB.PositionSlot = fromSlot; }
        }

        public List<BattleUnit> GetAllAlive()
        {
            var result = new List<BattleUnit>();
            foreach (var unit in FrontRow)
                if (unit != null && unit.IsAlive) result.Add(unit);
            foreach (var unit in BackRow)
                if (unit != null && unit.IsAlive) result.Add(unit);
            return result;
        }

        /// <summary>获取前排存活单位（优先打击目标）</summary>
        public List<BattleUnit> GetAliveFrontRow()
        {
            return FrontRow.Where(u => u != null && u.IsAlive).Select(u => u!).ToList();
        }

        /// <summary>获取后排存活单位</summary>
        public List<BattleUnit> GetAliveBackRow()
        {
            return BackRow.Where(u => u != null && u.IsAlive).Select(u => u!).ToList();
        }

        /// <summary>获取攻击目标：优先前排，前排全灭则攻击后排</summary>
        public BattleUnit? GetDefaultTarget()
        {
            var frontAlive = GetAliveFrontRow();
            if (frontAlive.Count > 0)
                return frontAlive[UnityEngine.Random.Range(0, frontAlive.Count)];

            var backAlive = GetAliveBackRow();
            if (backAlive.Count > 0)
                return backAlive[UnityEngine.Random.Range(0, backAlive.Count)];

            return null;
        }

        /// <summary>获取行动顺序（按速度降序）</summary>
        public List<BattleUnit> GetActionOrder()
        {
            return GetAllAlive()
                .OrderByDescending(u => u.Speed)
                .ToList();
        }
    }
}
