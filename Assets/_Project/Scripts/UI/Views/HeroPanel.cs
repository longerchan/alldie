using System;
using System.Collections.Generic;

namespace FrostShelter.UI.Views
{
    /// <summary>
    /// 英雄面板。展示英雄列表、详情、升星、编队。
    /// </summary>
    public class HeroPanel : BasePanel
    {
        public List<HeroSlot> HeroSlots = new();
        public string SelectedHeroId;

        public event Action<string> OnHeroSelected;
        public event Action<string> OnLevelUpClicked;
        public event Action<string> OnStarUpClicked;
        public event Action<string> OnAssignClicked;
        public event Action<string> OnAddToExpeditionClicked;

        public HeroPanel() : base("HeroPanel", PanelLayer.Normal) { }

        protected override void OnShow()
        {
            RefreshHeroList();
        }

        protected override void OnHide() { }

        public override void Refresh()
        {
            RefreshHeroList();
        }

        public override void OnBackPressed()
        {
            if (!string.IsNullOrEmpty(SelectedHeroId))
            {
                SelectedHeroId = null; // 返回列表视图
                Refresh();
            }
            else
            {
                Hide();
            }
        }

        private void RefreshHeroList()
        {
            HeroSlots.Clear();
            // 从 HeroManager 获取所有英雄数据填充列表
        }
    }

    [Serializable]
    public class HeroSlot
    {
        public string HeroId;
        public string HeroName;
        public HeroQuality Quality;
        public int Level;
        public int StarLevel;
        public float Power;
        public bool IsInExpedition;
        public string AssignedBuildingName;
    }

    public enum HeroQuality { Blue, Purple, Gold }
}
