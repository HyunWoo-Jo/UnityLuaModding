using Modding.Engine;
using Modding.Events;
using Modding.Utils;
using NLua;
using System;
using System.Collections.Generic;

namespace Modding.API {
    public class LuaEventWrapper {
        /// <summary>
        /// 이벤트 구독 (Lua에서 Event:Subscribe() 형태로 호출)
        /// </summary>
        public void Subscribe(string eventName, LuaFunction handler) {
            LuaEventAPI.Subscribe(eventName, handler);
        }

        /// <summary>
        /// 이벤트 구독 해제 (Lua에서 Event:Unsubscribe() 형태로 호출)
        /// </summary>
        public void Unsubscribe(string eventName, LuaFunction handler) {
            LuaEventAPI.Unsubscribe(eventName, handler);
        }

        /// <summary>
        /// 이벤트 발생 (Lua에서 Event:Publish() 형태로 호출)
        /// </summary>
        public void Publish(string eventName, object data = null) {
            LuaEventAPI.Publish(eventName, data);
        }

        /// <summary>
        /// 특정 이벤트의 모든 핸들러 정리
        /// </summary>
        public void ClearEventHandlers(string eventName) {
            LuaEventAPI.UnsubscribeAll(eventName);
        }

    }
    public partial class LuaEventAPI {

        private static Dictionary<string, Dictionary<LuaFunction, Action<object>>> _handlerMappings = new();

        public static void Subscribe(string eventName, LuaFunction handler) {
            if (handler == null) {
                ModDebug.LogWarning("LuaEventAPI: Handler cannot be null");
                return;
            }
            Action<object> wrapperHandler = (data) => {
                try {
                    // Lua 함수 호출 (Lua function call)
                    handler.Call(data);
                } catch (Exception e) {
                    ModDebug.LogError($"LuaEventAPI: Error in Lua event handler: {e.Message}");
                }
            };

            // 매핑 저장
            if (!_handlerMappings.ContainsKey(eventName)) {
                _handlerMappings[eventName] = new Dictionary<LuaFunction, Action<object>>();
            }
            _handlerMappings[eventName][handler] = wrapperHandler;
            ModEventBus.Subscribe(eventName, wrapperHandler);
            ModDebug.Log($"LuaEventAPI: Subscribed to event '{eventName}'");
        }

        public static void Unsubscribe(string eventName, LuaFunction handler) {
            // 저장된 매핑에서 래퍼 핸들러 찾기
            if (_handlerMappings.ContainsKey(eventName) &&
                _handlerMappings[eventName].ContainsKey(handler)) {

                var wrapperHandler = _handlerMappings[eventName][handler];

                // 실제 이벤트 구독 해제
                ModEventBus.Unsubscribe(eventName, wrapperHandler);

                // 매핑에서 제거
                _handlerMappings[eventName].Remove(handler);

                // 해당 이벤트의 핸들러가 모두 제거되면 딕셔너리에서도 제거
                if (_handlerMappings[eventName].Count == 0) {
                    _handlerMappings.Remove(eventName);
                }

                ModDebug.Log($"LuaEventAPI: Unsubscribed from event '{eventName}'");
            } else {
                ModDebug.LogWarning($"LuaEventAPI: Handler not found for event '{eventName}'");
            }
        }

        public static void UnsubscribeAll(string eventName) {
            if (_handlerMappings.ContainsKey(eventName)) {
                foreach (var mapping in _handlerMappings[eventName]) {
                    ModEventBus.Unsubscribe(eventName, mapping.Value);
                }
                _handlerMappings.Remove(eventName);
                ModDebug.Log($"LuaEventAPI: Unsubscribed all handlers from event '{eventName}'");
            }
        }
        public static void ClearAllHandlers() {
            foreach (var eventName in _handlerMappings.Keys) {
                foreach (var mapping in _handlerMappings[eventName]) {
                    ModEventBus.Unsubscribe(eventName, mapping.Value);
                }
            }
            _handlerMappings.Clear();
            ModDebug.Log("LuaEventAPI: Cleared all Lua event handlers");
        }


        public static void Publish(string eventName, object data = null) {
            ModEventBus.Publish(eventName, data);
            ModDebug.Log($"LuaEventAPI: Published event '{eventName}'");
        }
    }
}
