using Modding.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Modding.Events {


    /// <summary>
    /// Dedicated event bus for the modding system
    /// Operates independently from the game's event system
    /// 모딩 시스템 전용 이벤트 버스
    /// 게임의 이벤트 시스템과 독립적으로 동작
    /// </summary>
    public static class ModEventBus {
        private static Dictionary<Type, List<object>> _eventHandlers = new();
        private static Dictionary<string, List<Action<object>>> _stringEventHandlers = new();

        /// <summary>
        /// Type base
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : class {
            var eventType = typeof(T);
            if (!_eventHandlers.ContainsKey(eventType)) {
                _eventHandlers[eventType] = new List<object>();
            }
            _eventHandlers[eventType].Add(handler);
        }
        /// <summary>
        /// String base
        /// </summary>
        public static void Subscribe(string eventName, Action<object> handler) {
            if (!_stringEventHandlers.ContainsKey(eventName)) {
                _stringEventHandlers[eventName] = new List<Action<object>>();
            }
            _stringEventHandlers[eventName].Add(handler);
        }

        /// <summary>
        /// Type Base
        /// </summary>
        public static void Publish<T>(T eventData) where T : class {
            var eventType = typeof(T);
            if (_eventHandlers.ContainsKey(eventType)) {
                foreach (Action<T> handler in _eventHandlers[eventType]) {
                    try {
                        handler?.Invoke(eventData);
                    } catch (Exception e) {
                        ModDebug.LogError($"[ModEventBus] Error in event handler: {e.Message}");
                    }
                }
            }
        }
        /// <summary>
        /// String Base
        /// </summary>
        public static void Publish(string eventName, object eventData = null) {
            if (_stringEventHandlers.ContainsKey(eventName)) {
                foreach (var handler in _stringEventHandlers[eventName]) {
                    try {
                        handler?.Invoke(eventData);
                    } catch (Exception e) {
                        ModDebug.LogError($"[ModEventBus] Error in string event handler: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Type base
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : class {
            var eventType = typeof(T);
            if (_eventHandlers.ContainsKey(eventType)) {
                _eventHandlers[eventType].Remove(handler);
            }
        }
        /// <summary>
        /// String base
        /// </summary>
        public static void Unsubscribe(string eventName, Action<object> handler) {
            if (_stringEventHandlers.ContainsKey(eventName)) {
                _stringEventHandlers[eventName].Remove(handler);
            }
        }

        public static void Clear() {
            _eventHandlers.Clear();
            _stringEventHandlers.Clear();
        }

    }
}
