using Modding.Engine;
using Modding.Events;
using Modding.Utils;
using NLua;
using System;
using System.Collections.Generic;

namespace Modding.API {
    public class LuaEventWrapper {
        /// <summary>
        /// �̺�Ʈ ���� (Lua���� Event:Subscribe() ���·� ȣ��)
        /// </summary>
        public void Subscribe(string eventName, LuaFunction handler) {
            LuaEventAPI.Subscribe(eventName, handler);
        }

        /// <summary>
        /// �̺�Ʈ ���� ���� (Lua���� Event:Unsubscribe() ���·� ȣ��)
        /// </summary>
        public void Unsubscribe(string eventName, LuaFunction handler) {
            LuaEventAPI.Unsubscribe(eventName, handler);
        }

        /// <summary>
        /// �̺�Ʈ �߻� (Lua���� Event:Publish() ���·� ȣ��)
        /// </summary>
        public void Publish(string eventName, object data = null) {
            LuaEventAPI.Publish(eventName, data);
        }

        /// <summary>
        /// Ư�� �̺�Ʈ�� ��� �ڵ鷯 ����
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
                    // Lua �Լ� ȣ�� (Lua function call)
                    handler.Call(data);
                } catch (Exception e) {
                    ModDebug.LogError($"LuaEventAPI: Error in Lua event handler: {e.Message}");
                }
            };

            // ���� ����
            if (!_handlerMappings.ContainsKey(eventName)) {
                _handlerMappings[eventName] = new Dictionary<LuaFunction, Action<object>>();
            }
            _handlerMappings[eventName][handler] = wrapperHandler;
            ModEventBus.Subscribe(eventName, wrapperHandler);
            ModDebug.Log($"LuaEventAPI: Subscribed to event '{eventName}'");
        }

        public static void Unsubscribe(string eventName, LuaFunction handler) {
            // ����� ���ο��� ���� �ڵ鷯 ã��
            if (_handlerMappings.ContainsKey(eventName) &&
                _handlerMappings[eventName].ContainsKey(handler)) {

                var wrapperHandler = _handlerMappings[eventName][handler];

                // ���� �̺�Ʈ ���� ����
                ModEventBus.Unsubscribe(eventName, wrapperHandler);

                // ���ο��� ����
                _handlerMappings[eventName].Remove(handler);

                // �ش� �̺�Ʈ�� �ڵ鷯�� ��� ���ŵǸ� ��ųʸ������� ����
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
