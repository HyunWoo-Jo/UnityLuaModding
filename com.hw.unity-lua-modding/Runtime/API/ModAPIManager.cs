using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using Modding.Utils;
using System.Collections.ObjectModel;
namespace Modding.API {
    public class ModAPIManager {
        private static readonly Dictionary<string, MethodInfo> _apiMethods = new();
        private static readonly Dictionary<string, string> _apiCategories = new();
        private static bool _initialized = false;

        public static void Initialize() {
            if (_initialized) return;
            ScanAPIMethods();
            _initialized = true;
        }

        private static void ScanAPIMethods() {
            // Scan All Assemblies (모든 어셈블리에서 ModAPI 어트리뷰트가 있는 메서드 스캔)
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.GetTypes()) {
                    // Check Category (카테고리 확인)
                    var categoryAttr = type.GetCustomAttribute<ModAPICategoryAttribute>();
                    string category = categoryAttr?.Category ?? "Default";

                    // Scan Method (메서드 스캔)
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
                        var apiAttr = method.GetCustomAttribute<ModAPIAttribute>();
                        if (apiAttr != null) {
                            string fullApiName = $"{category}.{apiAttr.APIName}";
                            _apiMethods[fullApiName] = method;
                            _apiCategories[fullApiName] = category;

                            ModDebug.Log($"Registered API: {fullApiName} - {apiAttr.Description}");
                        }
                    }
                }
            }
        }

        // API calling method for Lua (Lua에서 호출할 수 있는 통합 API 호출 메서드)
        public static object CallAPI(string apiName, params object[] args) {
            if (!_initialized) Initialize();

            if (_apiMethods.TryGetValue(apiName, out var method)) {
                try {
                    return method.Invoke(null, args);
                } catch (Exception e) {
                    ModDebug.LogError($"API Call Error [{apiName}]: {e.Message}");
                    return null;
                }
            } else {
                ModDebug.LogError($"API not found: {apiName}");
                return null;
            }
        }

        public static Dictionary<string, string> GetAvailableAPIs() {
            if (!_initialized) Initialize();

            var result = new Dictionary<string, string>();
            foreach (var kvp in _apiMethods) {
                var method = kvp.Value;
                var apiAttr = method.GetCustomAttribute<ModAPIAttribute>();
                result[kvp.Key] = apiAttr?.Description ?? "";
            }
            return result;
        }

        public static Dictionary<string, List<string>> GetAPIsByCategory() {
            if (!_initialized) Initialize();

            var result = new Dictionary<string, List<string>>();
            foreach (var kvp in _apiCategories) {
                if (!result.ContainsKey(kvp.Value))
                    result[kvp.Value] = new List<string>();

                result[kvp.Value].Add(kvp.Key);
            }
            return result;
        }
    }
}