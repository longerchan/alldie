using System;
using System.Collections.Generic;

namespace FrostShelter.UI.Views
{
    /// <summary>
    /// 探险面板。管理探险准备、地图展示、节点交互。
    /// </summary>
    public class ExplorationPanel : BasePanel
    {
        public bool IsInExpedition;
        public List<string> SelectedHeroIds = new();
        public List<Battle.TroopData> SelectedTroops = new();

        // 地图显示数据
        public Exploration.ExplorationManager ExplorationMgr;

        public event Action OnStartExpeditionClicked;
        public event Action OnReturnToCampClicked;
        public event Action<Exploration.HexCoord> OnNodeClicked;
        public event Action OnForceEndClicked;

        public ExplorationPanel() : base("ExplorationPanel", PanelLayer.Normal) { }

        protected override void OnShow()
        {
            RefreshTeamSelection();
        }

        protected override void OnHide() { }

        public override void Refresh()
        {
            if (IsInExpedition)
                RefreshMapView();
            else
                RefreshTeamSelection();
        }

        public override void OnBackPressed()
        {
            if (IsInExpedition)
            {
                // 探险中不能直接退出，弹出确认
            }
            else
            {
                Hide();
            }
        }

        private void RefreshTeamSelection()
        {
            SelectedHeroIds.Clear();
            SelectedTroops.Clear();
        }

        private void RefreshMapView()
        {
            // 刷新六边形地图显示
        }

        /// <summary>获取当前可进入的节点列表</summary>
        public List<Exploration.HexCoord> GetReachableNodes()
        {
            return ExplorationMgr?.GetReachableNeighbors() ?? new List<Exploration.HexCoord>();
        }

        /// <summary>更新供应品显示</summary>
        public void UpdateSupplies(float food, float warmth)
        {
            // 更新食物/暖炉进度条
        }
    }
}
