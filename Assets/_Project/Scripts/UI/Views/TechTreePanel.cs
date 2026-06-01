using System;
using System.Collections.Generic;

namespace FrostShelter.UI.Views
{
    public class TechTreePanel : BasePanel
    {
        public List<TechNodeDisplay> DevelopmentNodes = new();
        public List<TechNodeDisplay> CombatNodes = new();
        public List<TechNodeDisplay> EconomyNodes = new();

        public string SelectedNodeId;

        public event Action<string> OnNodeClicked;
        public event Action<string> OnResearchClicked;

        public TechTreePanel() : base("TechTreePanel", PanelLayer.Normal) { }

        protected override void OnShow() { RefreshTechTree(); }
        protected override void OnHide() { }

        public override void Refresh() { RefreshTechTree(); }

        public override void OnBackPressed()
        {
            if (!string.IsNullOrEmpty(SelectedNodeId))
            {
                SelectedNodeId = null;
                Refresh();
            }
            else
            {
                Hide();
            }
        }

        private void RefreshTechTree()
        {
            DevelopmentNodes.Clear();
            CombatNodes.Clear();
            EconomyNodes.Clear();
        }
    }

    [Serializable]
    public class TechNodeDisplay
    {
        public string NodeId;
        public string DisplayName;
        public string Description;
        public bool IsUnlocked;
        public bool IsResearched;
        public bool IsResearching;
        public float ResearchProgress;
        public string CostDisplay;
        public List<string> PrerequisiteNames;
    }
}
