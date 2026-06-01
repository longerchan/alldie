using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostShelter.Exploration
{
    /// <summary>
    /// 六边形网格地图。管理所有节点及其连通性。
    /// </summary>
    [Serializable]
    public class HexGrid
    {
        public int Size { get; private set; }
        public Dictionary<HexCoord, HexNode> Nodes { get; private set; } = new();
        public HexCoord StartPosition { get; set; }
        public List<HexCoord> BossPositions { get; private set; } = new();

        public void Initialize(int size)
        {
            Size = size;
            Nodes.Clear();
        }

        public HexNode GetNode(HexCoord coord)
        {
            Nodes.TryGetValue(coord, out var node);
            return node;
        }

        public void AddNode(HexNode node)
        {
            Nodes[node.Coordinate] = node;
        }

        public void RemoveNode(HexCoord coord)
        {
            Nodes.Remove(coord);
        }

        public bool IsValidCoordinate(HexCoord coord)
        {
            // 菱形范围限制
            int s = -coord.q - coord.r;
            return Math.Abs(coord.q) < Size
                && Math.Abs(coord.r) < Size
                && Math.Abs(s) < Size;
        }

        public List<HexCoord> GetNeighbors(HexCoord coord)
        {
            var result = new List<HexCoord>();
            foreach (var dir in HexCoord.Directions)
            {
                var neighbor = coord + dir;
                if (IsValidCoordinate(neighbor))
                {
                    result.Add(neighbor);
                }
            }
            return result;
        }

        /// <summary>获取已存在的邻居节点</summary>
        public List<HexCoord> GetExistingNeighbors(HexCoord coord)
        {
            return GetNeighbors(coord).Where(n => Nodes.ContainsKey(n)).ToList();
        }

        public float Distance(HexCoord a, HexCoord b)
        {
            return a.DistanceTo(b);
        }

        /// <summary>A* 寻路</summary>
        public List<HexCoord> FindPath(HexCoord from, HexCoord to)
        {
            var openSet = new HashSet<HexCoord> { from };
            var cameFrom = new Dictionary<HexCoord, HexCoord>();
            var gScore = new Dictionary<HexCoord, float> { [from] = 0f };
            var fScore = new Dictionary<HexCoord, float> { [from] = Distance(from, to) };

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(c => fScore.GetValueOrDefault(c, float.MaxValue)).First();

                if (current == to)
                    return ReconstructPath(cameFrom, current);

                openSet.Remove(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    float tentativeG = gScore[current] + 1f;
                    if (tentativeG < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + Distance(neighbor, to);
                        openSet.Add(neighbor);
                    }
                }
            }

            return null; // 无路径
        }

        private List<HexCoord> ReconstructPath(Dictionary<HexCoord, HexCoord> cameFrom, HexCoord current)
        {
            var path = new List<HexCoord> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }

        /// <summary>BFS 连通性校验</summary>
        public bool IsFullyConnected()
        {
            if (Nodes.Count == 0) return true;

            var start = Nodes.Keys.First();
            var visited = new HashSet<HexCoord>();
            var queue = new Queue<HexCoord>();
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var neighbor in GetNeighbors(current))
                {
                    if (Nodes.ContainsKey(neighbor) && visited.Add(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return visited.Count == Nodes.Count;
        }

        /// <summary>获取所有可达的邻居节点（已存在的）</summary>
        public List<HexCoord> GetReachableFrom(HexCoord position)
        {
            return GetNeighbors(position)
                .Where(n => Nodes.ContainsKey(n))
                .ToList();
        }
    }
}
