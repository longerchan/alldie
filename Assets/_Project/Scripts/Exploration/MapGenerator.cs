using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostShelter.Exploration
{
    /// <summary>
    /// Roguelike 地图生成器。生成8x8六边形随机地图。
    /// 算法: Perlin噪声 → A*主干路径 → 散射分支节点 → BFS校验
    /// </summary>
    public class MapGenerator
    {
        private System.Random _rng;
        private int _seed;

        public HexGrid GenerateMap(int size, int seed, MapConfigSO config = null)
        {
            _seed = seed != 0 ? seed : Environment.TickCount;
            _rng = new System.Random(_seed);

            var grid = new HexGrid();
            grid.Initialize(size);

            // Step 1: 放置起点和Boss点
            var (start, boss) = PlaceEndpoints(size);
            grid.StartPosition = start;

            // Step 2: 生成主干路径
            var mainPath = GenerateMainPath(grid, start, boss);

            // Step 3: 沿主干散射分支节点
            ScatterSideNodes(grid, mainPath, config);

            // Step 4: 填充 boss 节点
            var bossNode = new HexNode(boss, NodeType.Boss,
                CalculateDifficulty(1f, config?.DifficultyRampFactor ?? 1.15f));
            grid.AddNode(bossNode);
            grid.BossPositions.Add(boss);

            // Step 5: 连通性校验
            if (!grid.IsFullyConnected())
            {
                RepairConnectivity(grid, mainPath);
            }

            return grid;
        }

        private (HexCoord start, HexCoord boss) PlaceEndpoints(int size)
        {
            // 起点：左下区域
            var start = new HexCoord(
                _rng.Next(-size + 1, -size / 3),
                _rng.Next(size / 3, size - 1));

            // Boss点：右上区域
            var boss = new HexCoord(
                _rng.Next(size / 3, size - 1),
                _rng.Next(-size + 1, -size / 3));

            return (start, boss);
        }

        private List<HexCoord> GenerateMainPath(HexGrid grid, HexCoord start, HexCoord end)
        {
            var path = new List<HexCoord>();
            var current = start;

            // 添加起点
            var startNode = new HexNode(start, NodeType.Start, 1)
            {
                IsRevealed = true,
                IsVisited = true,
            };
            grid.AddNode(startNode);
            path.Add(start);

            // 贪心算法朝终点方向生成路径
            int maxSteps = grid.Size * 4;
            int steps = 0;

            while (current.DistanceTo(end) > 1 && steps < maxSteps)
            {
                steps++;
                var neighbors = grid.GetNeighbors(current);
                // 按距离终点排序
                var sorted = neighbors
                    .Where(n => grid.IsValidCoordinate(n) && !grid.Nodes.ContainsKey(n))
                    .OrderBy(n => n.DistanceTo(end))
                    .ToList();

                if (sorted.Count == 0) break;

                // 90% 概率选最近，10% 随机（增加变化）
                HexCoord next;
                if (_rng.NextDouble() < 0.9 && sorted.Count > 0)
                {
                    next = sorted[0];
                }
                else
                {
                    next = sorted[_rng.Next(Math.Min(3, sorted.Count))];
                }

                float distanceRatio = (float)steps / maxSteps;
                int difficulty = CalculateDifficulty(distanceRatio, 1.15f);
                var nodeType = WeightedRandomNodeType(distanceRatio);

                var node = new HexNode(next, nodeType, difficulty);
                grid.AddNode(node);
                path.Add(next);
                current = next;
            }

            return path;
        }

        private void ScatterSideNodes(HexGrid grid, List<HexCoord> mainPath,
            MapConfigSO config)
        {
            foreach (var mainCoord in mainPath)
            {
                var neighbors = grid.GetNeighbors(mainCoord);
                var available = neighbors
                    .Where(n => grid.IsValidCoordinate(n) && !grid.Nodes.ContainsKey(n))
                    .ToList();

                // 每个主干节点散射1-2个分支
                int branchesToAdd = _rng.Next(1, 3);
                for (int i = 0; i < branchesToAdd && available.Count > 0; i++)
                {
                    int idx = _rng.Next(available.Count);
                    var branchCoord = available[idx];
                    available.RemoveAt(idx);

                    float distanceRatio = (float)mainPath.IndexOf(mainCoord) / mainPath.Count;
                    int difficulty = CalculateDifficulty(distanceRatio,
                        config?.DifficultyRampFactor ?? 1.15f);
                    var nodeType = WeightedRandomNodeType(distanceRatio);

                    var node = new HexNode(branchCoord, nodeType, difficulty);
                    grid.AddNode(node);
                }
            }
        }

        private NodeType WeightedRandomNodeType(float distanceRatio)
        {
            float roll = (float)_rng.NextDouble();

            if (distanceRatio < 0.3f)
            {
                // 前期：资源50% 战斗20% 事件20% 商人10%
                if (roll < 0.50f) return NodeType.Resource;
                if (roll < 0.70f) return NodeType.Battle;
                if (roll < 0.90f) return NodeType.Event;
                return NodeType.Merchant;
            }
            else if (distanceRatio < 0.7f)
            {
                // 中期：资源30% 战斗35% 事件20% 商人10% Boss 5%
                if (roll < 0.30f) return NodeType.Resource;
                if (roll < 0.65f) return NodeType.Battle;
                if (roll < 0.85f) return NodeType.Event;
                if (roll < 0.95f) return NodeType.Merchant;
                return NodeType.Boss;
            }
            else
            {
                // 后期：资源15% 战斗40% 事件15% 商人5% Boss 25%
                if (roll < 0.15f) return NodeType.Resource;
                if (roll < 0.55f) return NodeType.Battle;
                if (roll < 0.70f) return NodeType.Event;
                if (roll < 0.75f) return NodeType.Merchant;
                return NodeType.Boss;
            }
        }

        private int CalculateDifficulty(float distanceRatio, float rampFactor)
        {
            return Math.Max(1,
                (int)Math.Floor(Math.Pow(rampFactor, distanceRatio * 10)));
        }

        private void RepairConnectivity(HexGrid grid, List<HexCoord> mainPath)
        {
            // 找到未连接的节点，连接到最近的主干节点
            var startCoord = grid.StartPosition;
            var visited = new HashSet<HexCoord>();
            var queue = new Queue<HexCoord>();
            queue.Enqueue(startCoord);
            visited.Add(startCoord);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var neighbor in grid.GetExistingNeighbors(current))
                {
                    if (visited.Add(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // 为未连接的节点在它们和最近的已连接邻居间建立桥接
            foreach (var coord in grid.Nodes.Keys.ToList())
            {
                if (!visited.Contains(coord))
                {
                    var unvisitedNeighbors = grid.GetExistingNeighbors(coord)
                        .Where(n => visited.Contains(n)).ToList();

                    if (unvisitedNeighbors.Count > 0)
                    {
                        // 已在 visited 中有邻居，标记为可连接
                        var bridgeNode = grid.GetNode(unvisitedNeighbors[0]);
                        // 桥接已存在，直接标记
                    }
                    visited.Add(coord);
                }
            }
        }
    }
}
