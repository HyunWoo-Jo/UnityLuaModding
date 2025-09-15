using UnityEngine;
using NLua;
using Modding.API;
using Modding.Utils;
using System.Text;
using Modding.Engine;
using System;
namespace Modding.Loaders {
    public class LuaModInstance : IModInstance {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public bool IsLoaded { get; private set; }

        private Lua _luaState;
        private string _modFolderPath;

        // Cache Lua functions (Lua 함수들 캐싱)
        private LuaFunction _initializeFunc;
        private LuaFunction _updateFunc;
        private LuaFunction _pauseFunc;
        private LuaFunction _resumeFunc;
        private LuaFunction _shutdownFunc;

        public LuaModInstance(string name, string version, string folderPath) {
            Name = name;
            Version = version;
            _modFolderPath = folderPath;
        }

        public bool Initialize() {
            try {
                ModDebug.Log($"[LuaMod] {Name} initializing...");

                // Create Lua state (Lua 상태 생성)
                _luaState = new Lua();
                _luaState.State.Encoding = UTF8Encoding.UTF8;

                // Register C# API to Lua (C# API를 Lua에 등록)
                RegisterCSharpAPI();

                // Find and execute Lua script (Lua 스크립트 파일 찾기 및 실행)
                string luaScript = FindLuaScript();
                if (string.IsNullOrEmpty(luaScript)) {
                    ModDebug.LogError($"[LuaMod] {Name}: Lua script not found");
                    return false;
                }

                // Execute Lua script (Lua 스크립트 실행)
                var results = _luaState.DoString(luaScript);

                // Get mod table (모드 테이블 가져오기)
                var modTable = GetModTable(results);
                if (modTable == null) {
                    ModDebug.LogError($"[LuaMod] {Name}: Mod table not found");
                    return false;
                }

                // Cache Lua functions (Lua 함수들 캐싱)
                CacheLuaFunctions(modTable);

                // Call Lua mod initialization function (Lua 모드 초기화 함수 호출)
                if (_initializeFunc != null) {
                    var context = new LuaModContext(_modFolderPath);
                    var initResults = _initializeFunc.Call(context);

                    if (initResults.Length > 0 && initResults[0] is bool success && success) {
                        IsLoaded = true;
                        ModDebug.Log($"[LuaMod] {Name} initialization successful");
                        return true;
                    }
                }

                ModDebug.LogError($"[LuaMod] {Name} initialization function failed");
                return false;
            } catch (System.Exception e) {
                ModDebug.LogError($"[LuaMod] {Name} initialization error: {e.Message}");
                CleanupLuaState();
                return false;
            }
        }

        private void RegisterCSharpAPI() {
            try {
                // Register C# API for use in Lua (Lua에서 사용할 C# API 등록)
                _luaState["ModAPI"] = new LuaModAPI();
                _luaState["Unity"] = new UnityLuaAPI();
                _luaState["Event"] = new LuaEventWrapper();

                // Register C# functions as Lua functions (C# 함수들을 Lua 함수로 등록)
                _luaState.RegisterFunction("print", this, typeof(LuaModInstance).GetMethod("LuaPrint"));
                _luaState.RegisterFunction("log", this, typeof(LuaModInstance).GetMethod("LuaLog"));

                ModDebug.Log("LuaModInstance: Successfully registered C# APIs for Lua");
            } catch (Exception e) {
                ModDebug.LogError($"LuaModInstance: Failed to register C# APIs: {e.Message}");
                throw;
            }
        }

        private string FindLuaScript() {
            // Possible Lua script paths (가능한 Lua 스크립트 경로들)
            string[] possiblePaths = {
                System.IO.Path.Combine(_modFolderPath, "Scripts", "main.lua"),
                System.IO.Path.Combine(_modFolderPath, "Scripts", "mod.lua"),
                System.IO.Path.Combine(_modFolderPath, "main.lua"),
                System.IO.Path.Combine(_modFolderPath, $"{Name}.lua")
            };

            foreach (string path in possiblePaths) {
                if (System.IO.File.Exists(path)) {
                    return System.IO.File.ReadAllText(path);
                }
            }

            return null;
        }

        private LuaTable GetModTable(object[] results) {
            // 1. Find in return values (반환값에서 찾기)
            if (results != null && results.Length > 0 && results[0] is LuaTable) {
                return results[0] as LuaTable;
            }

            // 2. Find in global variables (전역 변수에서 찾기)
            var candidates = new string[] { "MyMod", Name, "Mod", "mod" };
            foreach (string candidate in candidates) {
                var table = _luaState[candidate] as LuaTable;
                if (table != null) return table;
            }

            return null;
        }

        private void CacheLuaFunctions(LuaTable modTable) {
            _initializeFunc = modTable["Initialize"] as LuaFunction;
            _updateFunc = modTable["Update"] as LuaFunction;
            _pauseFunc = modTable["OnPause"] as LuaFunction;
            _resumeFunc = modTable["OnResume"] as LuaFunction;
            _shutdownFunc = modTable["Shutdown"] as LuaFunction;
        }

        public void Update(float deltaTime) {
            if (!IsLoaded || _updateFunc == null) return;

            try {
                _updateFunc.Call(deltaTime);
            } catch (System.Exception e) {
                ModDebug.LogError($"[LuaMod] {Name} update error: {e.Message}");

            }
        }

        public void OnGamePause() {
            try {
                _pauseFunc?.Call();
            } catch (System.Exception e) {
                ModDebug.LogError($"[LuaMod] {Name} pause error: {e.Message}");
            }
        }

        public void OnGameResume() {
            try {
                _resumeFunc?.Call();
            } catch (System.Exception e) {
                ModDebug.LogError($"[LuaMod] {Name} resume error: {e.Message}");
            }
        }

        public void Shutdown() {
            try {
                _shutdownFunc?.Call();
            } catch (System.Exception e) {
                ModDebug.LogError($"[LuaMod] {Name} shutdown error: {e.Message}");
            } finally {
                CleanupLuaState();
                IsLoaded = false;
            }
        }

        private void CleanupLuaState() {
            // Clean up Lua function references (Lua 함수 참조 정리)
            _initializeFunc?.Dispose();
            _updateFunc?.Dispose();
            _pauseFunc?.Dispose();
            _resumeFunc?.Dispose();
            _shutdownFunc?.Dispose();

            // Clean up Lua state (Lua 상태 정리)
            _luaState?.Dispose();
            _luaState = null;
        }

        // C# functions that can be called from Lua (Lua에서 호출할 수 있는 C# 함수들)
        public static void LuaPrint(string message) {
            ModDebug.Log($"[Lua] {message}");

        }

        public static void LuaLog(string level, string message) {
            switch (level.ToLower()) {
                case "info":
                ModDebug.Log($"[Lua Info] {message}");
                break;
                case "warning":
                ModDebug.LogWarning($"[Lua Warning] {message}");
                break;
                case "error":
                ModDebug.LogError($"[Lua Error] {message}");
                break;
                default:
                ModDebug.Log($"[Lua] {message}");
                break;
            }
        }
    }
}