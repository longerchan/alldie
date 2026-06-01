using System;
using System.Collections.Generic;
using System.Linq;
using FrostShelter.Core;

namespace FrostShelter.UI
{
    /// <summary>
    /// UI管理器。管理所有面板的生命周期和层级。
    /// </summary>
    public class UIManager : IService
    {
        private readonly Dictionary<string, BasePanel> _panelRegistry = new();
        private readonly Dictionary<PanelLayer, Stack<BasePanel>> _panelStacks = new();
        private readonly List<BasePanel> _activePanels = new();

        private EventDispatcher _events;

        public event Action<BasePanel> OnPanelShown;
        public event Action<BasePanel> OnPanelHidden;

        public void Initialize()
        {
            foreach (PanelLayer layer in Enum.GetValues(typeof(PanelLayer)))
            {
                _panelStacks[layer] = new Stack<BasePanel>();
            }
        }

        public void Shutdown()
        {
            HideAll();
            _panelRegistry.Clear();
            _panelStacks.Clear();
            _activePanels.Clear();
        }

        public void SetDependencies(EventDispatcher events)
        {
            _events = events;
        }

        public void RegisterPanel(BasePanel panel)
        {
            _panelRegistry[panel.PanelId] = panel;
        }

        public T GetPanel<T>(string panelId) where T : BasePanel
        {
            _panelRegistry.TryGetValue(panelId, out var panel);
            return panel as T;
        }

        public void ShowPanel(string panelId)
        {
            if (!_panelRegistry.TryGetValue(panelId, out var panel)) return;

            // 隐藏同层或更高层的面板
            var layersToHide = _activePanels
                .Where(p => p.Layer >= panel.Layer && p.PanelId != panelId)
                .ToList();

            foreach (var p in layersToHide)
            {
                p.Hide();
                _activePanels.Remove(p);
                OnPanelHidden?.Invoke(p);
            }

            panel.Show();
            _activePanels.Add(panel);
            _panelStacks[panel.Layer].Push(panel);

            // 排序确保渲染顺序
            _activePanels.Sort((a, b) => a.Layer.CompareTo(b.Layer));

            OnPanelShown?.Invoke(panel);
        }

        public void HidePanel(string panelId)
        {
            if (!_panelRegistry.TryGetValue(panelId, out var panel)) return;
            if (!panel.IsVisible) return;

            panel.Hide();
            _activePanels.Remove(panel);
            OnPanelHidden?.Invoke(panel);

            // 恢复下一层面板
            var nextPanel = _activePanels.LastOrDefault();
            nextPanel?.Refresh();
        }

        public void HideAll()
        {
            foreach (var panel in _activePanels.ToList())
            {
                panel.Hide();
            }
            _activePanels.Clear();
        }

        public void GoBack()
        {
            if (_activePanels.Count == 0) return;
            var topPanel = _activePanels.Last();
            topPanel.OnBackPressed();
        }

        public bool IsPanelVisible(string panelId)
        {
            return _panelRegistry.TryGetValue(panelId, out var panel) && panel.IsVisible;
        }

        public void RefreshAll()
        {
            foreach (var panel in _activePanels)
            {
                panel.Refresh();
            }
        }
    }
}
