using System;
using System.Collections.Generic;

namespace FrostShelter.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IService> _services = new();

        public static void Register<T>(T service) where T : class, IService
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                throw new InvalidOperationException($"Service of type {type.Name} is already registered.");
            }
            _services[type] = service;
        }

        public static T Get<T>() where T : class, IService
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }
            throw new InvalidOperationException($"Service of type {type.Name} is not registered.");
        }

        public static bool TryGet<T>(out T service) where T : class, IService
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var s))
            {
                service = s as T;
                return true;
            }
            service = null;
            return false;
        }

        public static bool HasService<T>() where T : class, IService
        {
            return _services.ContainsKey(typeof(T));
        }

        public static void Unregister<T>() where T : class, IService
        {
            _services.Remove(typeof(T));
        }

        public static void Clear()
        {
            foreach (var service in _services.Values)
            {
                service.Shutdown();
            }
            _services.Clear();
        }
    }
}
