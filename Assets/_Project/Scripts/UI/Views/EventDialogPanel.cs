using System;

namespace FrostShelter.UI.Views
{
    /// <summary>
    /// 随机事件对话框。展示事件描述和选项。
    /// </summary>
    public class EventDialogPanel : BasePanel
    {
        public string EventTitle;
        public string EventDescription;
        public float RemainingSeconds;
        public EventChoiceDisplay[] Choices;

        public event Action<int> OnChoiceSelected;
        public event Action OnDismissed;
        public event Action OnTimeout;

        public EventDialogPanel() : base("EventDialog", PanelLayer.Popup) { }

        public void PresentEvent(Events.RandomEventSO evt)
        {
            EventTitle = evt.EventTitle;
            EventDescription = evt.EventDescription;
            RemainingSeconds = evt.DurationSeconds;

            Choices = new EventChoiceDisplay[evt.Choices.Length];
            for (int i = 0; i < evt.Choices.Length; i++)
            {
                Choices[i] = new EventChoiceDisplay
                {
                    Index = i,
                    Text = evt.Choices[i].ChoiceText,
                };
            }

            Show();
        }

        protected override void OnShow() { }
        protected override void OnHide() { }

        public override void Refresh()
        {
            // 更新倒计时
        }

        public override void OnBackPressed()
        {
            // 随机事件不可跳过（除非有超时自动选择）
        }

        public void TickTimer(float deltaSeconds)
        {
            RemainingSeconds -= deltaSeconds;
            if (RemainingSeconds <= 0f)
            {
                OnTimeout?.Invoke();
                Hide();
            }
        }
    }

    [Serializable]
    public struct EventChoiceDisplay
    {
        public int Index;
        public string Text;
    }
}
