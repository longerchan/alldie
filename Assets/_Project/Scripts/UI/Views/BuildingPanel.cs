using System;
using System.Collections.Generic;

namespace FrostShelter.UI.Views
{
    /// <summary>
    /// 建筑面板。展示单个建筑详情：当前等级、生产状态、升级消耗、驻守分配。
    /// </summary>
    public class BuildingPanel : BasePanel
    {
        public Building.BuildingId CurrentBuildingId { get; private set; }

        public BuildingInfo CurrentInfo;

        public event Action OnUpgradeClicked;
        public event Action OnCollectClicked;
        public event Action OnAssignSurvivorClicked;
        public event Action OnAssignHeroClicked;

        public BuildingPanel() : base("BuildingPanel", PanelLayer.Normal) { }

        public void ShowForBuilding(Building.BuildingId buildingId)
        {
            CurrentBuildingId = buildingId;
            Show();
        }

        protected override void OnShow()
        {
            RefreshBuildingInfo();
        }

        protected override void OnHide() { }

        public override void Refresh()
        {
            RefreshBuildingInfo();
        }

        public override void OnBackPressed()
        {
            Hide();
        }

        private void RefreshBuildingInfo()
        {
            // 从 BuildingManager 获取实时数据
        }
    }

    [Serializable]
    public struct BuildingInfo
    {
        public Building.BuildingId Id;
        public string DisplayName;
        public string Description;
        public int Level;
        public int MaxLevel;
        public bool IsUpgrading;
        public float UpgradeProgress;
        public string UpgradeCostDisplay;
        public bool IsProduction;
        public float AccumulatedProduction;
        public bool CanCollect;
        public List<SurvivorSlot> AssignedSurvivors;
        public string AssignedHeroName;
    }

    [Serializable]
    public struct SurvivorSlot
    {
        public string SurvivorId;
        public string SurvivorName;
        public string OccupationTag;
        public float EfficiencyBonus;
        public bool IsAssigned;
    }
}
