using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace LogKill.Core
{
    public interface IService
    {
        void Initialize();
    }

    public class ServiceLocator
    {
        private static Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static bool _initialized;

        private static void RegisterServiceWithInterface(Type interfaceType, object serviceInstance)
        {
            if (_services.ContainsKey(interfaceType))
            {
                Debug.LogWarning($"Service {interfaceType.Name} already registered. Overwriting...");
                _services[interfaceType] = serviceInstance;
            }
            else
            {
                _services.Add(interfaceType, serviceInstance);
            }
        }

        public static void Register<TInterface, TImplementation>(TImplementation service)
            where TImplementation : class, TInterface
        {
            Type interfaceType = typeof(TInterface);

            if (_services.ContainsKey(interfaceType))
            {
                Debug.LogWarning($"Service {interfaceType.Name} already registered. Overwriting...");
                _services[interfaceType] = service;
            }
            else
            {
                _services.Add(interfaceType, service);
                Debug.Log($"Service {interfaceType.Name} registered.");
            }
        }

        /// <summary>
        /// 서비스 가져오기
        /// </summary>
        /// <typeparam name="TInterface">서비스 인터페이스 타입</typeparam>
        /// <returns>등록된 서비스 인스턴스 또는 기본값</returns>
        public static TInterface Get<TInterface>()
        {
            Type interfaceType = typeof(TInterface);

            if (_services.TryGetValue(interfaceType, out object service))
            {
                return (TInterface)service;
            }

            Debug.LogError($"Service {interfaceType.Name} not found!");
            return default;
        }

        /// <summary>
        /// 서비스 존재 여부 확인
        /// </summary>
        /// <typeparam name="TInterface">서비스 인터페이스 타입</typeparam>
        /// <returns>서비스 존재 여부</returns>
        public static bool HasService<TInterface>()
        {
            return _services.ContainsKey(typeof(TInterface));
        }

        /// <summary>
        /// 서비스 제거
        /// </summary>
        /// <typeparam name="TInterface">서비스 인터페이스 타입</typeparam>
        /// <returns>제거 성공 여부</returns>
        public static bool Unregister<TInterface>()
        {
            Type interfaceType = typeof(TInterface);

            if (_services.ContainsKey(interfaceType))
            {
                _services.Remove(interfaceType);
                Debug.Log($"Service {interfaceType.Name} unregistered.");
                return true;
            }

            return false;
        }

        public static void ClearAll()
        {
            _services.Clear();
        }

        public static void AutoRegisterServices()
        {
            if (_initialized)
            {
                Debug.LogWarning("ServiceLocator is already initialized.");
                return;
            }

            var serviceTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IService).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            foreach (var serviceType in serviceTypes)
            {
                try
                {
                    var serviceInstance = Activator.CreateInstance(serviceType);

                    var interfaces = serviceType.GetInterfaces();

                    foreach (var interfaceType in interfaces)
                    {
                        if (interfaceType == typeof(IService) || interfaceType.GetInterfaces().Contains(typeof(IService)))
                        {
                            RegisterServiceWithInterface(serviceType, serviceInstance);
                        }
                    }

                    ((IService)serviceInstance).Initialize();

                    Debug.Log($"Auto-registered service: {serviceType.Name}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to auto-register service {serviceType.Name}: {ex.Message}");
                }
            }

            _initialized = true;
            Debug.Log("ServiceLocator initialization completed.");
        }
    }

}
