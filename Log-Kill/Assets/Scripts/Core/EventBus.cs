using System;
using System.Collections.Generic;

namespace LogKill.Core
{
    public static class EventBus
    {
        private static Dictionary<Type, List<Delegate>> _eventDictionary = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> callback) where T : struct
        {
            Type eventType = typeof(T);

            if (!_eventDictionary.ContainsKey(eventType))
            {
                _eventDictionary[eventType] = new List<Delegate>();
            }

            _eventDictionary[eventType].Add(callback);
        }

        public static void Unsubscribe<T>(Action<T> callback) where T : struct
        {
            Type eventType = typeof(T);

            if (_eventDictionary.ContainsKey(eventType))
            {
                _eventDictionary[eventType].Remove(callback);
            }
        }

        public static void Publish<T>(T eventData) where T : struct
        {
            Type eventType = typeof(T);

            if (_eventDictionary.ContainsKey(eventType))
            {
                foreach (var callback in _eventDictionary[eventType])
                {
                    ((Action<T>)callback).Invoke(eventData);
                }
            }
        }
    }
}
