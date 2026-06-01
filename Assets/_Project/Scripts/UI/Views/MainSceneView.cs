using System;
using FrostShelter.SaveSystem;

namespace FrostShelter.UI.Views
{
    /// <summary>
    /// 主场景HUD视图。显示营地俯视图，包含顶部资源栏、温度指示器、
    /// 幸存者气泡、建筑交互入口、探险按钮、事件提示等。
    /// </summary>
    public class MainSceneView : BasePanel
    {
        public event Action OnBuildingClicked;
        public event Action OnHeroPanelClicked;
        public event Action OnExplorationClicked;
        public event Action OnSurvivorClicked;
        public event Action OnTechTreeClicked;
        public event Action OnSettingsClicked;

        // 实时数据绑定
        public float CurrentTemperature;
        public string TemperatureDisplay;
        public ResourceDisplayEntry[] ResourceDisplays;

        public MainSceneView() : base("MainScene", PanelLayer.Background) { }

        protected override void OnShow()
        {
            RefreshResourceDisplay();
            RefreshTemperature();
        }

        protected override void OnHide() { }

        public override void Refresh()
        {
            RefreshResourceDisplay();
            RefreshTemperature();
        }

        public override void OnBackPressed()
        {
            // 主场景不支持回退，弹出退出确认
            OnSettingsClicked?.Invoke();
        }

        private void RefreshResourceDisplay()
        {
            if (ResourceDisplays == null) return;
            // 实际项目从 ResourceManager 获取实时数据
        }

        private void RefreshTemperature()
        {
            TemperatureDisplay = CurrentTemperature > -20f
                ? $"温度: {CurrentTemperature:F1}°C (舒适)"
                : CurrentTemperature > -30f
                    ? $"温度: {CurrentTemperature:F1}°C (寒冷)"
                    : $"温度: {CurrentTemperature:F1}°C (极寒!)";
        }

        /// <summary>显示幸存者快捷气泡</summary>
        public void ShowSurvivorBubble(string survivorId, string message)
        {
            // 在主场景中弹出气泡提示
        }

        /// <summary>显示事件提示图标</summary>
        public void ShowEventIndicator(bool show)
        {
            // 显示/隐藏事件提示红点
        }
    }

    [Serializable]
    public struct ResourceDisplayEntry
    {
        public ResourceType Type;
        public string IconName;
        public float CurrentAmount;
        public float MaxCapacity;
        public string DisplayText;
    }
}
