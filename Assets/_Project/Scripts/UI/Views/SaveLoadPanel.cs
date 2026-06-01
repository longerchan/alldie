using System;

namespace FrostShelter.UI.Views
{
    /// <summary>
    /// 存档/读档面板。展示4个槽位（3手动+1自动）。
    /// </summary>
    public class SaveLoadPanel : BasePanel
    {
        public bool IsSaveMode;  // true=保存, false=加载
        public SaveSlotDisplay[] Slots = new SaveSlotDisplay[4];

        public event Action<int> OnSlotSelected;
        public event Action<int> OnSlotDeleted;
        public event Action OnCloseClicked;

        public SaveLoadPanel() : base("SaveLoad", PanelLayer.Top) { }

        public void SetMode(bool isSave)
        {
            IsSaveMode = isSave;
            Show();
        }

        protected override void OnShow()
        {
            RefreshSlots();
        }

        protected override void OnHide() { }

        public override void Refresh()
        {
            RefreshSlots();
        }

        public override void OnBackPressed()
        {
            Hide();
        }

        private void RefreshSlots()
        {
            // 从 SaveManager 读取所有槽位信息
        }
    }

    [Serializable]
    public struct SaveSlotDisplay
    {
        public int SlotIndex;
        public bool IsOccupied;
        public string SaveTime;
        public int FurnaceLevel;
        public float PlayTimeHours;
        public string DisplayText;
    }
}
