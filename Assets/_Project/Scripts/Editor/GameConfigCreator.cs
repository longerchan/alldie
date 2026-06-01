using UnityEditor;
using UnityEngine;
using FrostShelter.Core;
using FrostShelter.Temperature;
using FrostShelter.Exploration;

namespace FrostShelter.Editor
{
    /// <summary>
    /// 编辑器工具：一键创建所有 ScriptableObject 配置文件。
    /// 在Unity菜单栏: FrostShelter > Create All Configs
    /// </summary>
    public static class GameConfigCreator
    {
        private const string CONFIG_PATH = "Assets/_Project/Resources/Configs/";

        [MenuItem("FrostShelter/Create All Configs")]
        public static void CreateAllConfigs()
        {
            EnsureDirectoryExists();

            CreateGlobalConfig();
            CreateTemperatureConfig();
            CreateMapConfig();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[FrostShelter] All configs created at " + CONFIG_PATH);
        }

        private static void EnsureDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources/Configs"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Resources", "Configs");
            }
        }

        private static void CreateGlobalConfig()
        {
            var config = ScriptableObject.CreateInstance<GlobalConfigSO>();
            config.GameVersion = "0.1.0";
            config.Difficulty = DifficultyLevel.Normal;
            AssetDatabase.CreateAsset(config, CONFIG_PATH + "GlobalGameConfig.asset");
        }

        private static void CreateTemperatureConfig()
        {
            var config = ScriptableObject.CreateInstance<TemperatureConfigSO>();
            config.BaseTemperature = -30f;
            config.TemperaturePerFurnaceLevel = 3f;
            config.EfficiencyCurves = new[]
            {
                new TemperatureConfigSO.TemperatureThreshold
                {
                    MinTemp = -20f, MaxTemp = 100f,
                    ProductionEfficiency = 1.0f, SurvivorDamagePerHour = 0f, SickChancePerHour = 0f,
                },
                new TemperatureConfigSO.TemperatureThreshold
                {
                    MinTemp = -30f, MaxTemp = -20f,
                    ProductionEfficiency = 0.7f, SurvivorDamagePerHour = 0.02f, SickChancePerHour = 0.01f,
                },
                new TemperatureConfigSO.TemperatureThreshold
                {
                    MinTemp = -100f, MaxTemp = -30f,
                    ProductionEfficiency = 0.4f, SurvivorDamagePerHour = 0.05f, SickChancePerHour = 0.03f,
                },
            };
            AssetDatabase.CreateAsset(config, CONFIG_PATH + "TemperatureConfig.asset");
        }

        private static void CreateMapConfig()
        {
            var config = ScriptableObject.CreateInstance<MapConfigSO>();
            config.MinGridSize = 6;
            config.MaxGridSize = 8;
            config.DifficultyRampFactor = 1.15f;
            config.StartFood = 100;
            config.StartWarmth = 100;
            config.FoodConsumePerStep = 5f;
            config.WarmthConsumePerStep = 8f;
            AssetDatabase.CreateAsset(config, CONFIG_PATH + "MapGenerationConfig.asset");
        }

        [MenuItem("FrostShelter/Create Main Scene")]
        public static void CreateMainScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                UnityEditor.SceneManagement.NewSceneMode.Single);

            // Create GameBootstrap GameObject
            var bootstrapGO = new GameObject("GameBootstrap");
            bootstrapGO.AddComponent<GameBootstrap>();

            // Create UI Canvas
            var canvasGO = new GameObject("MainCanvas");
            canvasGO.tag = "MainCanvas";
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create EventSystem
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Create PanelRoot under Canvas
            var panelRoot = new GameObject("PanelRoot");
            panelRoot.transform.SetParent(canvasGO.transform);

            // Save scene
            string scenePath = "Assets/_Project/Scenes/Main.unity";
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log("[FrostShelter] Main scene created at " + scenePath);
        }
    }
}
