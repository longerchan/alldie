using System;
using UnityEngine;

namespace FrostShelter.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _clearSaveOnStart;
        [SerializeField] private float _gameTimeScale = 1f;

        private GameManager _gameManager;

        // All service instances stored as fields for cross-method access
        private TimeEngine.TimeEngine timeEngine;
        private Resource.ResourceManager resourceManager;
        private Temperature.TemperatureManager temperatureManager;
        private Temperature.BlizzardEventController blizzardController;
        private Building.BuildingManager buildingManager;
        private Survivor.SurvivorManager survivorManager;
        private Survivor.SatisfactionDecaySystem satisfactionSystem;
        private Hero.HeroManager heroManager;
        private Battle.BattleManager battleManager;
        private Battle.CampaignManager campaignManager;
        private Battle.TroopTrainingSystem troopTrainingSystem;
        private Exploration.ExplorationManager explorationManager;
        private TechTree.TechTreeManager techTreeManager;
        private Events.RandomEventManager eventManager;
        private Resource.MerchantSystem merchantSystem;
        private Story.StoryManager storyManager;
        private SaveSystem.SaveManager saveManager;
        private TimeEngine.OfflineEarningManager offlineEarningManager;
        private DataTable.DataTableManager dataTableManager;
        private UI.UIManager uiManager;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            _gameManager = new GameManager();
            CreateAllServices();
            RegisterAllServices();
            SetupAllDependencies();
            _gameManager.InitializeGame();
        }

        private void CreateAllServices()
        {
            timeEngine = new TimeEngine.TimeEngine();
            resourceManager = new Resource.ResourceManager();
            temperatureManager = new Temperature.TemperatureManager();
            blizzardController = new Temperature.BlizzardEventController();
            buildingManager = new Building.BuildingManager();
            survivorManager = new Survivor.SurvivorManager();
            satisfactionSystem = new Survivor.SatisfactionDecaySystem();
            heroManager = new Hero.HeroManager();
            battleManager = new Battle.BattleManager();
            campaignManager = new Battle.CampaignManager();
            troopTrainingSystem = new Battle.TroopTrainingSystem();
            explorationManager = new Exploration.ExplorationManager();
            techTreeManager = new TechTree.TechTreeManager();
            eventManager = new Events.RandomEventManager();
            merchantSystem = new Resource.MerchantSystem();
            storyManager = new Story.StoryManager();
            saveManager = new SaveSystem.SaveManager();
            offlineEarningManager = new TimeEngine.OfflineEarningManager();
            dataTableManager = new DataTable.DataTableManager();
            uiManager = new UI.UIManager();
        }

        private void RegisterAllServices()
        {
            _gameManager.RegisterAndInitialize(saveManager);
            _gameManager.RegisterAndInitialize(timeEngine);
            _gameManager.RegisterAndInitialize(dataTableManager);
            _gameManager.RegisterAndInitialize(resourceManager);
            _gameManager.RegisterAndInitialize(temperatureManager);
            _gameManager.RegisterAndInitialize(blizzardController);
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
            _gameManager.RegisterAndInitialize(uiManager);
        }

        private void SetupAllDependencies()
        {
            var events = _gameManager.Events;

            resourceManager.SetEventDispatcher(events);
            temperatureManager.SetDependencies(events, buildingManager.Furnace);
            buildingManager.SetDependencies(events, resourceManager);
            survivorManager.SetDependencies(events, temperatureManager);
            satisfactionSystem.SetDependencies(survivorManager, temperatureManager, buildingManager, events);
            heroManager.SetDependencies(events, resourceManager);
            battleManager.SetDependencies(events);
            campaignManager.SetDependencies(battleManager, resourceManager, events);
            troopTrainingSystem.SetDependencies(resourceManager);
            explorationManager.SetDependencies(events, resourceManager);
            techTreeManager.SetDependencies(events, resourceManager);
            eventManager.SetDependencies(events, resourceManager);
            merchantSystem.SetDependencies(events, resourceManager);
            storyManager.SetDependencies(events);
            offlineEarningManager.SetDependencies(timeEngine, resourceManager, temperatureManager, survivorManager, events);
            uiManager.SetDependencies(events);

            timeEngine.OnMinuteTick += () =>
            {
                temperatureManager.OnMinuteTick();
                survivorManager.OnMinuteTick();
                satisfactionSystem.OnHourTick();
                buildingManager.OnHourTick();
                merchantSystem.OnMinuteTick();
                eventManager.OnMinuteTick();
            };

            temperatureManager.OnBlizzardStarted += () => uiManager.RefreshAll();
            temperatureManager.RecalculateTemperature();

            timeEngine.SetTimeScale(_gameTimeScale);

            foreach (var producer in buildingManager.GetProductionBuildings())
            {
                timeEngine.RegisterProducer(producer);
                offlineEarningManager.RegisterProducer(producer);
            }

            timeEngine.SetAutoSaveCallback(() =>
            {
                saveManager.SaveToSlot(Constants.AUTO_SAVE_SLOT, BuildSaveData());
            });
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
    }
}
