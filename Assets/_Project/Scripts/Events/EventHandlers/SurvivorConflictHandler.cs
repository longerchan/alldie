using FrostShelter.Core;

namespace FrostShelter.Events.EventHandlers
{
    /// <summary>
    /// 幸存者冲突事件处理器。两派争执，选择站队影响满意度和阵营好感。
    /// </summary>
    public class SurvivorConflictHandler
    {
        private readonly Survivor.SurvivorManager _survivorManager;
        private readonly EventDispatcher _events;

        public SurvivorConflictHandler(Survivor.SurvivorManager survivorManager,
            EventDispatcher events)
        {
            _survivorManager = survivorManager;
            _events = events;
        }

        public void Handle(RandomEventSO evt, int choiceIndex)
        {
            // 冲突事件选项：
            // 0: 支持A派（资源优先派，获得资源产出加成，B派满意度下降）
            // 1: 支持B派（生存优先派，获得防御加成，A派满意度下降）
            // 2: 调解（消耗资源，双方满意度小幅提升）

            switch (choiceIndex)
            {
                case 0:
                    // 50%幸存者满意度+5, 50%满意度-10
                    ApplyFactionEffect(0.5f, 5f, -10f);
                    break;
                case 1:
                    ApplyFactionEffect(0.5f, -10f, 5f);
                    break;
                case 2:
                    _survivorManager.BoostMorale(3f);
                    _events?.Dispatch(GameEventType.SurvivorSatisfactionChanged);
                    break;
            }
        }

        private void ApplyFactionEffect(float ratio, float factionADelta, float factionBDelta)
        {
            var survivors = _survivorManager.AllSurvivors;
            int splitPoint = (int)(survivors.Count * ratio);
            for (int i = 0; i < survivors.Count; i++)
            {
                float delta = i < splitPoint ? factionADelta : factionBDelta;
                survivors[i].ModifySatisfaction(delta);
            }
            _events?.Dispatch(GameEventType.SurvivorSatisfactionChanged);
        }
    }
}
