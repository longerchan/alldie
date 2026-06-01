using FrostShelter.Core;

namespace FrostShelter.Events.EventHandlers
{
    /// <summary>
    /// 商队路过事件处理器。稀有物资交换。
    /// </summary>
    public class MerchantVisitHandler
    {
        private readonly Resource.MerchantSystem _merchantSystem;
        private readonly Resource.ResourceManager _resourceManager;

        public MerchantVisitHandler(Resource.MerchantSystem merchantSystem,
            Resource.ResourceManager resourceManager)
        {
            _merchantSystem = merchantSystem;
            _resourceManager = resourceManager;
        }

        public void Handle(RandomEventSO evt, int choiceIndex)
        {
            // 商队事件选项：
            // 0: 交易（打开游商界面）
            // 1: 讨价还价（消耗少量资源获得折扣）
            // 2: 赶走商队

            switch (choiceIndex)
            {
                case 0:
                    _merchantSystem.RefreshMerchant();
                    break;
                case 1:
                    // 有概率获得更好的货品
                    _merchantSystem.RefreshMerchant();
                    break;
                case 2:
                    // 可能失去未来交易机会（满意度变化）
                    break;
            }
        }
    }
}
