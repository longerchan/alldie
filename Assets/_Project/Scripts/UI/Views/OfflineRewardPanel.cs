using System;
using System.Collections.Generic;
using FrostShelter.SaveSystem;

namespace FrostShelter.UI.Views
{
    /// <summary>
    /// 离线收益面板。玩家回到游戏时展示离线期间各建筑的产出。
    /// </summary>
    public class OfflineRewardPanel : BasePanel
    {
        public TimeSpan OfflineDuration;
        public List<OfflineRewardEntry> Rewards = new();
        public int SurvivorsDied;
        public int SurvivorsEscaped;

        public event Action OnCollectClicked;

        public OfflineRewardPanel() : base("OfflineReward", PanelLayer.Popup) { }

        protected override void OnShow() { }
        protected override void OnHide() { }

        public override void Refresh() { }

        public override void OnBackPressed()
        {
            // 离线收益面板不可关闭，必须点击收集
        }

        public void Present(TimeEngine.OfflineEarningsSummary summary)
        {
            OfflineDuration = TimeSpan.Zero;
            Rewards.Clear();
            foreach (var record in summary.Records)
            {
                Rewards.Add(new OfflineRewardEntry
                {
                    OutputType = record.OutputType,
                    Amount = record.TotalProduced,
                    WasCapped = record.WasCapped,
                });
            }
            SurvivorsDied = summary.SurvivorsDied;
            SurvivorsEscaped = summary.SurvivorsEscaped;
            Show();
        }
    }

    [Serializable]
    public struct OfflineRewardEntry
    {
        public ResourceType OutputType;
        public float Amount;
        public bool WasCapped;
    }
}
