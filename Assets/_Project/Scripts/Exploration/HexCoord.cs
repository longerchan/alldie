using System;
using System.Collections.Generic;

namespace FrostShelter.Exploration
{
    /// <summary>
    /// 六边形网格的轴向坐标 (q, r)。
    /// 使用 "odd-r" 或 "axial" 坐标系统。
    /// </summary>
    [Serializable]
    public struct HexCoord : IEquatable<HexCoord>
    {
        public int q;
        public int r;

        public HexCoord(int q, int r)
        {
            this.q = q;
            this.r = r;
        }

        /// <summary>六个邻居方向（轴向坐标）</summary>
        public static readonly HexCoord[] Directions = new[]
        {
            new HexCoord(+1, 0), new HexCoord(+1, -1), new HexCoord(0, -1),
            new HexCoord(-1, 0), new HexCoord(-1, +1), new HexCoord(0, +1),
        };

        public IEnumerable<HexCoord> Neighbors
        {
            get
            {
                foreach (var dir in Directions)
                    yield return this + dir;
            }
        }

        public int DistanceTo(HexCoord other)
        {
            var dq = other.q - q;
            var dr = other.r - r;
            var ds = -dq - dr; // s = -q - r in cube coordinates
            return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(ds)) / 2;
        }

        public static HexCoord operator +(HexCoord a, HexCoord b) =>
            new HexCoord(a.q + b.q, a.r + b.r);

        public static HexCoord operator -(HexCoord a, HexCoord b) =>
            new HexCoord(a.q - b.q, a.r - b.r);

        public static bool operator ==(HexCoord a, HexCoord b) =>
            a.q == b.q && a.r == b.r;

        public static bool operator !=(HexCoord a, HexCoord b) =>
            !(a == b);

        public bool Equals(HexCoord other) =>
            q == other.q && r == other.r;

        public override bool Equals(object obj) =>
            obj is HexCoord other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(q, r);

        public override string ToString() =>
            $"({q}, {r})";

        public SaveSystem.HexCoordSaveData ToSaveData() =>
            new SaveSystem.HexCoordSaveData { q = q, r = r };

        public static HexCoord FromSaveData(SaveSystem.HexCoordSaveData data) =>
            new HexCoord(data.q, data.r);
    }
}
