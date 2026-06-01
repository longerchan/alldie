using System;
using System.Collections.Generic;
using System.Linq;
using FrostShelter.Core;
using FrostShelter.Resource;

namespace FrostShelter.TechTree
{
    /// <summary>
    /// 科技树管理器。管理三列科技节点（发展/战斗/经济）的解锁和研究。
    /// </summary>
    public class TechTreeManager : IService
    {
        private readonly List<TechNode> _allNodes = new();

        private EventDispatcher _events;
        private ResourceManager _resourceManager;

        public IReadOnlyList<TechNode> AllNodes => _allNodes;
        public IReadOnlyList<TechNode> UnlockedNodes =>
            _allNodes.Where(n => n.IsResearched).ToList();
        public IReadOnlyList<TechNode> ResearchableNodes =>
            _allNodes.Where(n => n.IsUnlocked && !n.IsResearched).ToList();

        public event Action<TechNode> OnNodeUnlocked;
        public event Action<TechNode> OnResearchStarted;
        public event Action<TechNode> OnResearchCompleted;

        public void Initialize() { }

        public void Shutdown()
        {
            _allNodes.Clear();
        }

        public void SetDependencies(EventDispatcher events, ResourceManager resourceManager)
        {
            _events = events;
            _resourceManager = resourceManager;
        }

        // 注册科技节点配置
        public void RegisterNode(TechNode node)
        {
            _allNodes.Add(node);
        }

        public bool CanResearch(string nodeId)
        {
            var node = FindNode(nodeId);
            if (node == null || !node.IsUnlocked || node.IsResearched)
                return false;

            // 检查前置节点
            foreach (var prereqId in node.PrerequisiteNodeIds)
            {
                var prereq = FindNode(prereqId);
                if (prereq == null || !prereq.IsResearched)
                    return false;
            }

            return _resourceManager.CanAfford(node.ResearchCost);
        }

        public bool StartResearch(string nodeId)
        {
            if (!CanResearch(nodeId)) return false;

            var node = FindNode(nodeId);
            _resourceManager.Spend(node.ResearchCost, ResourceChangeReason.Upgrade);

            node.IsResearching = true;
            node.ResearchProgress = 0f;

            OnResearchStarted?.Invoke(node);
            _events?.Dispatch(GameEventType.TechResearchStarted);
            return true;
        }

        /// <summary>添加研究进度（由TimeEngine驱动）</summary>
        public void AddResearchProgress(float deltaSeconds, float efficiency)
        {
            foreach (var node in _allNodes)
            {
                if (node.IsResearching)
                {
                    node.ResearchProgress += deltaSeconds * efficiency;
                    if (node.ResearchProgress >= node.ResearchTimeSeconds)
                    {
                        CompleteResearch(node);
                    }
                }
            }
        }

        private void CompleteResearch(TechNode node)
        {
            node.IsResearching = false;
            node.IsResearched = true;
            node.ResearchProgress = node.ResearchTimeSeconds;

            // 解锁后续节点
            foreach (var other in _allNodes)
            {
                if (!other.IsUnlocked &&
                    other.PrerequisiteNodeIds.All(pid =>
                        FindNode(pid)?.IsResearched == true))
                {
                    other.IsUnlocked = true;
                    OnNodeUnlocked?.Invoke(other);
                    _events?.Dispatch(GameEventType.TechNodeUnlocked);
                }
            }

            OnResearchCompleted?.Invoke(node);
            _events?.Dispatch(GameEventType.TechResearchCompleted);
        }

        /// <summary>获取科技效果总值</summary>
        public float GetTechEffect(TechEffectType effectType, string targetId = null)
        {
            float total = 0f;
            foreach (var node in UnlockedNodes)
            {
                foreach (var effect in node.Effects)
                {
                    if (effect.EffectType == effectType &&
                        (string.IsNullOrEmpty(targetId) || effect.TargetId == targetId))
                    {
                        total += effect.Value;
                    }
                }
            }
            return total;
        }

        private TechNode FindNode(string nodeId) => _allNodes.Find(n => n.NodeId == nodeId);
    }
}
