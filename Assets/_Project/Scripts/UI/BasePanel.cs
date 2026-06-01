using System;
using System.Collections;

namespace FrostShelter.UI
{
    /// <summary>
    /// UI面板基类。所有具体Panel继承此类。
    /// </summary>
    public abstract class BasePanel
    {
        public string PanelId { get; protected set; }
        public PanelLayer Layer { get; protected set; }
        public bool IsVisible { get; protected set; }
        public bool IsTransitioning { get; protected set; }

        public event Action<BasePanel> OnShowCompleted;
        public event Action<BasePanel> OnHideCompleted;

        protected BasePanel(string panelId, PanelLayer layer)
        {
            PanelId = panelId;
            Layer = layer;
        }

        public virtual void Show()
        {
            IsVisible = true;
            OnShow();
            OnShowCompleted?.Invoke(this);
        }

        public virtual void Hide()
        {
            IsVisible = false;
            OnHide();
            OnHideCompleted?.Invoke(this);
        }

        public virtual void Toggle()
        {
            if (IsVisible) Hide();
            else Show();
        }

        protected abstract void OnShow();
        protected abstract void OnHide();
        public abstract void Refresh();
        public abstract void OnBackPressed();
    }
}
