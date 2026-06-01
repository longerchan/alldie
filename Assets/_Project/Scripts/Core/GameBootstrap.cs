using UnityEngine;

namespace FrostShelter.Core
{
    /// <summary>
    /// Unity场景启动器。挂载在场景GameObject上，负责初始化GameManager和所有服务。
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _clearSaveOnStart;
        [SerializeField] private float _gameTimeScale = 1f;

        [Header("Configs")]
        [SerializeField] private GlobalConfigSO _globalConfig;

        private GameManager _gameManager;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            _gameManager = new GameManager();
            RegisterAllServices();
            _gameManager.InitializeGame();
        }

        private void RegisterAllServices()
        {
            // 创建并注册所有服务实例
            var timeEngine = new TimeEngine.TimeEngine();
            var resourceManager = new Resource.ResourceManager();
            var temperatureManager = new Temperature.TemperatureManager();
            var buildingManager = new Building.BuildingManager();
            var survivorManager = new Survivor.SurvivorManager();
            var satisfactionSystem = new Survivor.SatisfactionDecaySystem();
            var heroManager = new Hero.HeroManager();
            var battleManager = new Battle.BattleManager();
            var campaignManager = new Battle.CampaignManager();
            var troopTrainingSystem = new Battle.TroopTrainingSystem();
            var explorationManager = new Exploration.ExplorationManager();
            var techTreeManager = new TechTree.TechTreeManager();
            var eventManager = new Events.RandomEventManager();
            var merchantSystem = new Resource.MerchantSystem();
            var storyManager = new Story.StoryManager();
            var saveManager = new SaveSystem.SaveManager();
            var offlineEarningManager = new TimeEngine.OfflineEarningManager();
            var blizzardController = new Temperature.BlizzardEventController();
            var dataTableManager = new DataTable.DataTableManager();
            var uiManager = new UI.UIManager();

            // 按依赖顺序注册到 GameManager
            _gameManager.RegisterAndInitialize(saveManager);
            _gameManager.RegisterAndInitialize(timeEngine);
            _gameManager.RegisterAndInitialize(dataTableManager);

            // Foundation层
            _gameManager.RegisterAndInitialize(resourceManager);
            _gameManager.RegisterAndInitialize(temperatureManager);
            _gameManager.RegisterAndInitialize(blizzardController);

            // Feature层
            _gameManager.RegisterAndInitialize(buildingManager);
            _gameManager.RegisterAndInitialize(survivorManager);
            _gameManager.RegisterAndInitialize(satisfactionSystem);
            _gameManager.RegisterAndInitialize(heroManager);
            _gameManager.RegisterAndInitialize(battleManager);
            _gameManager.RegisterAndInitialize(campaignManager);
            _gameManager.RegisterAndInitialize(troopTrainingSystem);
            _gameManager.RegisterAndInitialize(techTreeManager);
            _gameManager.RegisterAndInitialize(eventManager);
            _gameManager.RegisterAndInitialize(merchantSystem);
            _gameManager.RegisterAndInitialize(storyManager);
            _gameManager.RegisterAndInitialize(explorationManager);
            _gameManager.RegisterAndInitialize(offlineEarningManager);

            // UI最后初始化
            _gameManager.RegisterAndInitialize(uiManager);

            // 建立服务间依赖关系
            SetupDependencies(timeEngine, resourceManager, temperatureManager,
                buildingManager, survivorManager, heroManager, battleManager,
                explorationManager, techTreeManager, eventManager, merchantSystem,
                storyManager, satisfactionSystem, offlineEarningManager,
                blizzardController, campaignManager, troopTrainingSystem, uiManager);

            // 设置时间缩放
            timeEngine.SetTimeScale(_gameTimeScale);

            // 生产系统注册到TimeEngine
            foreach (var producer in buildingManager.GetProductionBuildings())
            {
                timeEngine.RegisterProducer(producer);
                offlineEarningManager.RegisterProducer(producer);
            }

            // 自动存档回调
            timeEngine.SetAutoSaveCallback(() =>
            {
                saveManager.SaveToSlot(Constants.AUTO_SAVE_SLOT, BuildSaveData());
            });
        }

        private void SetupDependencies(
            TimeEngine.TimeEngine timeEngine,
            Resource.ResourceManager resourceManager,
            Temperature.TemperatureManager temperatureManager,
            Building.BuildingManager buildingManager,
            Survivor.SurvivorManager survivorManager,
            Hero.HeroManager heroManager,
            Battle.BattleManager battleManager,
            Exploration.ExplorationManager explorationManager,
            TechTree.TechTreeManager techTreeManager,
            Events.RandomEventManager eventManager,
            Resource.MerchantSystem merchantSystem,
            Story.StoryManager storyManager,
            Survivor.SatisfactionDecaySystem satisfactionSystem,
            TimeEngine.OfflineEarningManager offlineEarningManager,
            Temperature.BlizzardEventController blizzardController,
            Battle.CampaignManager campaignManager,
            Battle.TroopTrainingSystem troopTrainingSystem,
            UI.UIManager uiManager)
        {
            var events = _gameManager.Events;

            // ResourceManager
            resourceManager.SetEventDispatcher(events);

            // Temperature
            temperatureManager.SetDependencies(events, buildingManager.Furnace);

            // Building
            buildingManager.SetDependencies(events, resourceManager);

            // Survivor
            survivorManager.SetDependencies(events, temperatureManager);
            satisfactionSystem.SetDependencies(survivorManager, temperatureManager,
                buildingManager, events);

            // Hero
            heroManager.SetDependencies(events, resourceManager);

            // Battle
            battleManager.SetDependencies(events);
            campaignManager.SetDependencies(battleManager, resourceManager, events);
            troopTrainingSystem.SetDependencies(resourceManager);

            // Exploration
            explorationManager.SetDependencies(events, resourceManager);

            // TechTree
            techTreeManager.SetDependencies(events, resourceManager);

            // Events
            eventManager.SetDependencies(events, resourceManager);

            // Merchant
            merchantSystem.SetDependencies(events, resourceManager);

            // Story
            storyManager.SetDependencies(events);

            // Offline
            offlineEarningManager.SetDependencies(timeEngine, resourceManager,
                temperatureManager, survivorManager, events);

            // UI
            uiManager.SetDependencies(events);

            // TimeEngine每分钟tick连接
            timeEngine.OnMinuteTick += () =>
            {
                temperatureManager.OnMinuteTick();
                survivorManager.OnMinuteTick();
                satisfactionSystem.OnHourTick();
                buildingManager.OnHourTick();
                merchantSystem.OnMinuteTick();
                eventManager.OnMinuteTick();
            };

            // Temperature事件连接
            temperatureManager.OnBlizzardStarted += () =>
            {
                uiManager.RefreshAll();
            };

            temperatureManager.RecalculateTemperature();
        }

        private void Start()
        {
            timeEngine.StartTime();
        }

        private void Update()
        {
            if (_gameManager.CurrentState == GameState.Playing)
            {
                timeEngine.Tick(Time.deltaTime);
                blizzardController.Tick(Time.deltaTime);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                saveManager.SaveToSlot(Constants.AUTO_SAVE_SLOT, BuildSaveData());
                timeEngine.StopTime();
            }
            else
            {
                timeEngine.StartTime();
                offlineEarningManager.CalculateOfflineEarnings(
                    DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds());
            }
        }

        private void OnApplicationQuit()
        {
            saveManager.SaveToSlot(Constants.AUTO_SAVE_SLOT, BuildSaveData());
            _gameManager.Shutdown();
        }

        private SaveSystem.SaveData BuildSaveData()
        {
            return new SaveSystem.SaveData
            {
                player = new SaveSystem.PlayerProgressData
                {
                    furnaceLevel = buildingManager.Furnace?.Level ?? 1,
                    totalSurvivors = survivorManager.TotalSurvivorCount,
                },
                resources = resourceManager.Resources,
                buildings = buildingManager.ToSaveData(),
                survivors = survivorManager.ToSaveData(),
                heroes = heroManager.ToSaveData(),
                techTree = new SaveSystem.TechTreeProgressData(),
                exploration = explorationManager.ToSaveData(),
                story = storyManager.ToSaveData(),
                lastSaveUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };
        }

        // 引用保持（防GC）
        private TimeEngine.TimeEngine timeEngine;
        private Resource.ResourceManager resourceManager;
        private Temperature.TemperatureManager temperatureManager;
        private Building.BuildingManager buildingManager;
        private Survivor.SurvivorManager survivorManager;
        private Hero.HeroManager heroManager;
        private Battle.BattleManager battleManager;
        private Exploration.ExplorationManager explorationManager;
        private TechTree.TechTreeManager techTreeManager;
        private Events.RandomEventManager eventManager;
        private Story.StoryManager storyManager;
        private SaveSystem.SaveManager saveManager;
    }
}
