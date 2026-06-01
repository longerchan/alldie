namespace FrostShelter.UI
{
    /// <summary>
    /// UI面板层级。高值覆盖低值。
    /// </summary>
    public enum PanelLayer
    {
        Background = 0,   // 背景层（主场景HUD）
        Normal = 10,      // 普通面板（建筑/英雄/仓库等）
        Popup = 20,       // 弹窗（事件对话框/离线收益）
        Top = 30,         // 顶层（Loading/Settings/SaveLoad）
        System = 40,      // 系统级（Tips/确认框/Toast）
    }
}
