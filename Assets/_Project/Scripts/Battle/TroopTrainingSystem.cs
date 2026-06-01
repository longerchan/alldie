using System.Collections.Generic;
using FrostShelter.Core;
using FrostShelter.Resource;

namespace FrostShelter.Battle
{
    /// <summary>
    /// 兵种训练系统。管理三种兵营的训练产出。
    /// </summary>
    public class TroopTrainingSystem : IService
    {
        private readonly Dictionary<SaveSystem.TroopTypeSave, List<TroopData>> _trainedTroops = new();
        private readonly List<TroopConfigSO> _configs = new();
        private ResourceManager _resourceManager;

        public void Initialize()
        {
            foreach (SaveSystem.TroopTypeSave t in System.Enum.GetValues(typeof(SaveSystem.TroopTypeSave)))
            {
                _trainedTroops[t] = new List<TroopData>();
            }
        }

        public void Shutdown()
        {
            _trainedTroops.Clear();
            _configs.Clear();
        }

        public void SetDependencies(ResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public void RegisterConfig(TroopConfigSO config)
        {
            _configs.Add(config);
        }

        public TroopConfigSO GetConfig(SaveSystem.TroopTypeSave type)
        {
            return _configs.Find(c => c.TroopType == (TroopType)(int)type);
        }

        /// <summary>训练完成的兵种入库</summary>
        public void AddTrainedTroop(TroopData troop)
        {
            var type = (SaveSystem.TroopTypeSave)(int)troop.TroopType;
            _trainedTroops[type].Add(troop);
        }

        /// <summary>获取指定类型的可用兵种列表</summary>
        public List<TroopData> GetAvailableTroops(SaveSystem.TroopTypeSave type)
        {
            return _trainedTroops.TryGetValue(type, out var list)
                ? list.FindAll(t => t.AliveCount > 0)
                : new List<TroopData>();
        }

        /// <summary>获取所有可用兵种</summary>
        public List<TroopData> GetAllAvailableTroops()
        {
            var result = new List<TroopData>();
            foreach (var kvp in _trainedTroops)
            {
                result.AddRange(kvp.Value.FindAll(t => t.AliveCount > 0));
            }
            return result;
        }
    }
}
