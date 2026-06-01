using System;
using System.Collections.Generic;
using System.Linq;
using FrostShelter.Core;
using CoreCtx = FrostShelter.Core.AppContext;
using FrostShelter.SaveSystem;
using FrostShelter.Resource;

namespace FrostShelter.Building
{
    /// <summary>
    /// 建筑系统总管理器。统一管理13种建筑的创建、升级、驻守、生产收集。
    /// </summary>
    public partial class BuildingManager : IService
    {
        private readonly Dictionary<BuildingId, BuildingBase> _buildings = new();

        private EventDispatcher _events;
        private ResourceManager _resourceManager;

        public IReadOnlyDictionary<BuildingId, BuildingBase> AllBuildings => _buildings;
        public FurnaceBuilding Furnace => GetBuilding<FurnaceBuilding>(BuildingId.Furnace);
        public WarehouseBuilding Warehouse => GetBuilding<WarehouseBuilding>(BuildingId.Warehouse);
        public HousingBuilding Housing => GetBuilding<HousingBuilding>(BuildingId.Shelter);

        public event Action<BuildingId, int, int> OnBuildingLevelUp;

        public void Initialize()
        {
            // 创建初始建筑
            CreateBuilding(BuildingId.Furnace, 1);    // 熔炉初始1级
            CreateBuilding(BuildingId.Lumberyard, 0);  // 伐木场初始0级（已解锁）
        }

        public void Shutdown()
        {
            _buildings.Clear();
        }

        public void SetDependencies(EventDispatcher events, ResourceManager resourceManager)
        {
            _events = events;
            _resourceManager = resourceManager;
        }

        public BuildingBase GetBuilding(BuildingId id)
        {
            _buildings.TryGetValue(id, out var building);
            return building;
        }

        public T GetBuilding<T>(BuildingId id) where T : BuildingBase
        {
            return GetBuilding(id) as T;
        }

        public void CreateBuilding(BuildingId id, int initialLevel)
        {
            if (_buildings.ContainsKey(id)) return;

            BuildingBase building = CreateBuildingInstance(id);
            building.Initialize(id, GetConfigForBuilding(id), initialLevel);
            _buildings[id] = building;
        }

        private BuildingBase CreateBuildingInstance(BuildingId id)
        {
            return id switch
            {
                BuildingId.Furnace => new FurnaceBuilding(),
                BuildingId.Shelter => new HousingBuilding(),
                BuildingId.Warehouse => new WarehouseBuilding(),
                _ => new ProductionBuilding(),
            };
        }

        private BuildingConfigSO GetConfigForBuilding(BuildingId id)
        {
            // 在真实项目中从 DataTable 或 ScriptableObject 加载
            return null;
        }

        public bool CanUpgrade(BuildingId id)
        {
            var building = GetBuilding(id);
            if (building == null || building.IsMaxLevel || building.IsUpgrading)
                return false;

            // 检查熔炉等级限制
            if (id != BuildingId.Furnace && Furnace != null)
            {
                if (building.NextLevelConfig?.RequiredFurnaceLevel > Furnace.Level)
                    return false;
            }

            var cost = building.GetNextUpgradeCost();
            return _resourceManager?.CanAfford(cost) ?? true;
        }

        public bool UpgradeBuilding(BuildingId id)
        {
            if (!CanUpgrade(id)) return false;

            var building = GetBuilding(id);
            var cost = building.GetNextUpgradeCost();

            if (!_resourceManager.Spend(cost, ResourceChangeReason.Upgrade))
                return false;

            int oldLevel = building.Level;
            building.IsUpgrading = true;
            building.UpgradeProgress = 0f;
            building.UpgradeFinishTime = DateTime.UtcNow.AddSeconds(
                building.NextLevelConfig?.UpgradeTimeSeconds ?? 0);

            building.OnLevelUp(oldLevel + 1);

            OnBuildingLevelUp?.Invoke(id, oldLevel, building.Level);
            _events?.Dispatch(GameEventType.BuildingLevelUp,
                new CoreCtx.BuildingUpgradeArgs
                {
                    BuildingId = id,
                    OldLevel = oldLevel,
                    NewLevel = building.Level,
                });

            // 熔炉升级时更新仓库容量
            if (id == BuildingId.Furnace && Warehouse != null)
            {
                _resourceManager?.SetStorageCapacity(Warehouse.StorageCapacity);
            }

            return true;
        }

        public void AssignSurvivor(BuildingId buildingId, string survivorId, float efficiencyBonus)
        {
            var building = GetBuilding(buildingId);
            building?.AssignSurvivor(survivorId, efficiencyBonus);
        }

        public void UnassignSurvivor(BuildingId buildingId, string survivorId, float efficiencyBonus)
        {
            var building = GetBuilding(buildingId);
            building?.UnassignSurvivor(survivorId, efficiencyBonus);
        }

        public void AssignHero(BuildingId buildingId, string heroId, float efficiencyBonus)
        {
            var building = GetBuilding(buildingId);
            building?.AssignHero(heroId, efficiencyBonus);
        }

        public float CollectProduction(BuildingId id)
        {
            var building = GetBuilding(id);
            if (building is ProductionBuilding prod)
            {
                float amount = prod.Collect();
                if (amount > 0f)
                {
                    _resourceManager.AddResource(prod.OutputResource, amount,
                        ResourceChangeReason.Collection);
                    _events?.Dispatch(GameEventType.BuildingProductionCollected);
                }
                return amount;
            }
            return 0f;
        }

        /// <summary>获取所有生产型建筑（供 TimeEngine 注册）</summary>
        public IEnumerable<TimeEngine.ITimeProducer> GetProductionBuildings()
        {
            return _buildings.Values
                .OfType<TimeEngine.ITimeProducer>();
        }

        /// <summary>从存档恢复所有建筑</summary>
        public void LoadFromSaveData(Dictionary<BuildingId, BuildingData> data)
        {
            foreach (var kvp in data)
            {
                if (!_buildings.TryGetValue(kvp.Key, out var building))
                {
                    CreateBuilding(kvp.Key, kvp.Value.level);
                    building = _buildings[kvp.Key];
                }
                building?.LoadFromSaveData(kvp.Value);
            }
        }

        public Dictionary<BuildingId, BuildingData> ToSaveData()
        {
            var result = new Dictionary<BuildingId, BuildingData>();
            foreach (var kvp in _buildings)
            {
                result[kvp.Key] = kvp.Value.ToSaveData();
            }
            return result;
        }
    }
}
