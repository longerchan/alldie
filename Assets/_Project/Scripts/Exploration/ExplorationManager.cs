using System;
using System.Collections.Generic;
using System.Linq;
using FrostShelter.Core;
using FrostShelter.Resource;
using FrostShelter.SaveSystem;

namespace FrostShelter.Exploration
{
    /// <summary>
    /// 探险主控制器。管理探险全生命周期：准备→生成地图→移动→节点交互→返回。
    /// </summary>
    public class ExplorationManager : IService
    {
        public bool IsExpeditionActive { get; private set; }
        public ExpeditionTeam CurrentTeam { get; private set; }
        public HexGrid CurrentMap { get; private set; }
        public HexCoord CurrentPosition { get; private set; }

        private EventDispatcher _events;
        private ResourceManager _resourceManager;
        private MapGenerator _mapGenerator;
        private MapConfigSO _config;

        public event Action<HexNode> OnNodeReached;
        public event Action<ExpeditionResult> OnExpeditionEnded;
        public event Action<float, float> OnSuppliesChanged;

        public void Initialize()
        {
            _mapGenerator = new MapGenerator();
            _config = GetDefaultConfig();
        }

        public void Shutdown()
        {
            ForceEndExpedition();
        }

        public void SetDependencies(EventDispatcher events, ResourceManager resourceManager)
        {
            _events = events;
            _resourceManager = resourceManager;
        }

        /// <summary>开始探险</summary>
        public bool StartExpedition(List<string> heroIds, List<Battle.TroopData> troops)
        {
            if (IsExpeditionActive) return false;
            if (heroIds.Count > Constants.MAX_EXPLORATION_HEROES) return false;

            CurrentTeam = new ExpeditionTeam
            {
                Food = _config.StartFood,
                Warmth = _config.StartWarmth,
                FoodConsumePerStep = _config.FoodConsumePerStep,
                WarmthConsumePerStep = _config.WarmthConsumePerStep,
            };

            foreach (var id in heroIds) CurrentTeam.AddHero(id);
            foreach (var troop in troops) CurrentTeam.AddTroop(troop);

            // 生成地图
            int gridSize = UnityEngine.Random.Range(_config.MinGridSize, _config.MaxGridSize + 1);
            int seed = UnityEngine.Random.Range(0, int.MaxValue);
            CurrentMap = _mapGenerator.GenerateMap(gridSize, seed, _config);
            CurrentPosition = CurrentMap.StartPosition;

            IsExpeditionActive = true;
            _events?.Dispatch(GameEventType.ExpeditionStarted);
            return true;
        }

        /// <summary>获取当前可移动到的邻居节点</summary>
        public List<HexCoord> GetReachableNeighbors()
        {
            if (!IsExpeditionActive || CurrentMap == null) return new List<HexCoord>();
            return CurrentMap.GetNeighbors(CurrentPosition)
                .Where(n => CurrentMap.Nodes.ContainsKey(n))
                .ToList();
        }

        /// <summary>移动到目标坐标</summary>
        public bool MoveTo(HexCoord target)
        {
            if (!IsExpeditionActive) return false;
            if (!GetReachableNeighbors().Contains(target)) return false;

            CurrentTeam.ConsumeStep();
            CurrentPosition = target;

            OnSuppliesChanged?.Invoke(CurrentTeam.Food, CurrentTeam.Warmth);
            _events?.Dispatch(GameEventType.ExpeditionStepTaken);

            // 检查供应品耗尽
            if (!CurrentTeam.CanContinue)
            {
                ForceEndExpedition();
                return false;
            }

            // 进入节点
            var node = CurrentMap.GetNode(target);
            if (node != null)
            {
                node.IsVisited = true;
                EnterNode(node);
            }

            return true;
        }

        /// <summary>进入一个节点</summary>
        public void EnterNode(HexNode node)
        {
            OnNodeReached?.Invoke(node);
            _events?.Dispatch(GameEventType.ExpeditionNodeReached);

            switch (node.Type)
            {
                case NodeType.Resource:
                    CollectResource(node);
                    break;
                case NodeType.Battle:
                    // Battle is triggered externally via BattleManager
                    _events?.Dispatch(GameEventType.ExpeditionBattleStarted);
                    break;
                case NodeType.Event:
                    // Event is triggered externally via RandomEventManager
                    _events?.Dispatch(GameEventType.RandomEventTriggered);
                    break;
                case NodeType.Merchant:
                    // Merchant interaction
                    break;
                case NodeType.Boss:
                    _events?.Dispatch(GameEventType.ExpeditionBattleStarted);
                    break;
            }
        }

        private void CollectResource(HexNode node)
        {
            if (node.ResourceData == null) return;
            _resourceManager?.AddResource(node.ResourceData.ResourceType,
                node.ResourceData.Amount, ResourceChangeReason.Expedition);
        }

        /// <summary>强制结束探险（食物/暖炉耗尽或全军覆没）</summary>
        public ExpeditionResult ForceEndExpedition()
        {
            if (!IsExpeditionActive) return default;

            IsExpeditionActive = false;
            var result = new ExpeditionResult
            {
                IsForceEnded = true,
                EndReason = !CurrentTeam.HasEnoughFood ? "食物耗尽"
                    : !CurrentTeam.HasEnoughWarmth ? "暖炉耗尽"
                    : "未知原因",
                NodesVisited = CurrentMap?.Nodes.Values.Count(n => n.IsVisited) ?? 0,
            };

            OnExpeditionEnded?.Invoke(result);
            _events?.Dispatch(GameEventType.ExpeditionEnded);
            return result;
        }

        /// <summary>正常结束探险（胜利返回）</summary>
        public ExpeditionResult CompleteExpedition(bool defeatedBoss = false)
        {
            var result = new ExpeditionResult
            {
                IsVictory = defeatedBoss,
                IsForceEnded = false,
                EndReason = defeatedBoss ? "击败首领" : "主动返回",
                NodesVisited = CurrentMap?.Nodes.Values.Count(n => n.IsVisited) ?? 0,
                HeroesDied = 0,
                TroopsDied = 0,
            };

            IsExpeditionActive = false;
            OnExpeditionEnded?.Invoke(result);
            _events?.Dispatch(GameEventType.ExpeditionEnded);
            return result;
        }

        public bool TryUseItem(string itemId)
        {
            return false; // TODO: implement item system
        }

        public ExplorationSaveData ToSaveData()
        {
            if (!IsExpeditionActive)
            {
                return new ExplorationSaveData { isActive = false };
            }

            return new ExplorationSaveData
            {
                isActive = true,
                teamHeroIds = CurrentTeam?.HeroIds ?? new List<string>(),
                teamTroops = CurrentTeam?.Troops?.Select(t => new TroopSaveData
                {
                    configId = t.ConfigId,
                    troopType = (TroopTypeSave)(int)t.TroopType,
                    count = t.Count,
                    aliveCount = t.AliveCount,
                }).ToList() ?? new List<TroopSaveData>(),
                foodSupply = CurrentTeam?.Food ?? 0f,
                warmthSupply = CurrentTeam?.Warmth ?? 0f,
                gridSize = CurrentMap?.Size ?? 8,
                discoveredNodes = CurrentMap?.Nodes.Values.Select(n =>
                    new HexNodeSaveData
                    {
                        q = n.Coordinate.q,
                        r = n.Coordinate.r,
                        nodeType = (int)n.Type,
                        isRevealed = n.IsRevealed,
                        isVisited = n.IsVisited,
                        difficultyLevel = n.DifficultyLevel,
                    }).ToList() ?? new List<HexNodeSaveData>(),
                currentPosition = CurrentPosition.ToSaveData(),
                seed = 0, // Would store in real implementation
            };
        }

        private static MapConfigSO GetDefaultConfig()
        {
            return new MapConfigSO
            {
                MinGridSize = 6,
                MaxGridSize = 8,
                DifficultyRampFactor = 1.15f,
                StartFood = 100,
                StartWarmth = 100,
                FoodConsumePerStep = 5f,
                WarmthConsumePerStep = 8f,
            };
        }
    }

    public struct ExpeditionResult
    {
        public bool IsVictory;
        public bool IsForceEnded;
        public string EndReason;
        public int HeroesDied;
        public int TroopsDied;
        public int NodesVisited;
    }
}
