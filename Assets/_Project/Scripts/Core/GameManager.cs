using System;
using System.Collections;
using System.Collections.Generic;

namespace FrostShelter.Core
{
    /// <summary>
    /// 全局游戏生命周期管理器。
    /// 负责按序初始化所有系统服务，驱动主循环。
    /// </summary>
    public class GameManager
    {
        public static GameManager Instance { get; private set; }

        public AppContext Context { get; private set; }
        public EventDispatcher Events { get; private set; }
        public GameState CurrentState => Context.CurrentState;

        public event Action<GameState, GameState> OnGameStateChanged;

        private readonly List<IService> _initOrder = new();

        public GameManager()
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("GameManager already exists.");
            }
            Instance = this;
        }

        public void InitializeGame()
        {
            Context = new AppContext();
            Events = new EventDispatcher();

            Context.CurrentState = GameState.Loading;

            RegisterCoreServices();
            InitializeAllServices();
        }

        private void RegisterCoreServices()
        {
            ServiceLocator.Register(Events);
        }

        /// <summary>
        /// 注册所有业务系统服务并定义初始化顺序。
        /// 方法为 virtual 以便各模块在具体项目中重写扩展。
        /// </summary>
        protected virtual void RegisterFeatureServices()
        {
            // 子类或外部初始化流程中按顺序注册各 Feature 系统
        }

        private void InitializeAllServices()
        {
            // Core services first
            Events.Initialize();

            // Feature services will be initialized in registration order
            foreach (var service in _initOrder)
            {
                service.Initialize();
            }

            Context.CurrentState = GameState.Playing;
            Context.IsFirstLaunch = false;

            Events.Dispatch(GameEventType.GameInitialized);
            OnGameStateChanged?.Invoke(GameState.Loading, GameState.Playing);
        }

        public void RegisterAndInitialize(IService service)
        {
            _initOrder.Add(service);
            var type = service.GetType();
            // Register to ServiceLocator via reflection-like registration
            RegisterServiceByType(type, service);
            service.Initialize();
        }

        private void RegisterServiceByType(Type type, IService service)
        {
            var registerMethod = typeof(ServiceLocator).GetMethod("Register");
            var genericRegister = registerMethod?.MakeGenericMethod(type);
            genericRegister?.Invoke(null, new object[] { service });
        }

        public void SetState(GameState newState)
        {
            if (Context.CurrentState == newState) return;
            var oldState = Context.CurrentState;
            Context.CurrentState = newState;
            OnGameStateChanged?.Invoke(oldState, newState);

            switch (newState)
            {
                case GameState.Paused:
                    Events.Dispatch(GameEventType.GamePaused);
                    break;
                case GameState.Playing:
                    Events.Dispatch(GameEventType.GameResumed);
                    break;
            }
        }

        public void SaveGame(int slot)
        {
            Context.CurrentState = GameState.Saving;
            Events.Dispatch(GameEventType.GameSaved);
            Context.CurrentState = GameState.Playing;
            Context.CurrentSaveSlot = slot;
        }

        public void LoadGame(int slot)
        {
            Context.CurrentState = GameState.Loading;
            Context.CurrentSaveSlot = slot;
            Events.Dispatch(GameEventType.GameLoaded);
            Context.CurrentState = GameState.Playing;
        }

        public void Shutdown()
        {
            // Shutdown in reverse order
            for (int i = _initOrder.Count - 1; i >= 0; i--)
            {
                _initOrder[i].Shutdown();
            }
            Events.Shutdown();
            ServiceLocator.Clear();
        }
    }
}
