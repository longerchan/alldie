using System;
using System.Collections.Generic;

namespace FrostShelter.Core
{
    public class EventDispatcher : IService
    {
        private readonly Dictionary<GameEventType, Delegate> _eventTable = new();

        public void Initialize() { }

        public void Shutdown()
        {
            _eventTable.Clear();
        }

        public void AddListener(GameEventType type, Action handler)
        {
            _eventTable.TryGetValue(type, out var existing);
            _eventTable[type] = (Action)existing + handler;
        }

        public void AddListener<T>(GameEventType type, Action<T> handler)
        {
            _eventTable.TryGetValue(type, out var existing);
            _eventTable[type] = (Action<T>)existing + handler;
        }

        public void RemoveListener(GameEventType type, Action handler)
        {
            if (_eventTable.TryGetValue(type, out var existing))
            {
                _eventTable[type] = (Action)existing - handler;
                CleanupEmpty(type);
            }
        }

        public void RemoveListener<T>(GameEventType type, Action<T> handler)
        {
            if (_eventTable.TryGetValue(type, out var existing))
            {
                _eventTable[type] = (Action<T>)existing - handler;
                CleanupEmpty(type);
            }
        }

        public void Dispatch(GameEventType type)
        {
            if (_eventTable.TryGetValue(type, out var d) && d is Action handler)
            {
                handler.Invoke();
            }
        }

        public void Dispatch<T>(GameEventType type, T args)
        {
            if (_eventTable.TryGetValue(type, out var d) && d is Action<T> handler)
            {
                handler.Invoke(args);
            }
        }

        public void RemoveAllListeners()
        {
            _eventTable.Clear();
        }

        private void CleanupEmpty(GameEventType type)
        {
            if (_eventTable.TryGetValue(type, out var d) && d == null)
            {
                _eventTable.Remove(type);
            }
        }
    }
}
