using UnityEngine;
using System;
using System.Collections.Generic;
using Modding.Loaders;
using Modding.Utils;
using Modding.Events;
using static Modding.ModEvents;
namespace Modding.Engine {
    /// <summary>
    /// Manages the entire modding system
    /// 전체 모드를 관리하는 핵심 시스템
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class ModEngine : MonoBehaviour {
        #region Singleton
        public static ModEngine Instance { get; private set; }
        #endregion

        #region Settings (설정)
        [SerializeField] private bool _isEnableModding = true;
        [SerializeField] private bool _isEnableDebug = true;
        private enum DebugOption {
            UnityLog,
            Custom
        }
        [SerializeField] private DebugOption _debugOption = DebugOption.UnityLog;
        [SerializeField] private string _modDirectory = "Mods";

        private enum TimeOption {
            UnityDeltaTime,
            Custom
        }
        [SerializeField] private TimeOption _timeOption = TimeOption.UnityDeltaTime;
        public Func<float> GameDeltaTime;
        #endregion

        #region State (상태)
        private bool _isInitialized = false;
        private bool _isLoading = false;

        public bool IsInitialized => _isInitialized;
        public bool IsLoading => _isLoading;
        public string ModDirectoryPath { get; private set; }
        #endregion

        #region Manage Mod (모드 관리)
        private Dictionary<string, IModInstance> _loadedMods = new();
        private List<string> _failedMods = new();
        private List<IModLoader> _modLoaders = new();

        public IReadOnlyDictionary<string, IModInstance> LoadedMods => _loadedMods;
        public int LoadedModCount => _loadedMods.Count;
        #endregion

        /////// Method ///////

        #region LifeCycle (라이프사이클)
        private void Awake() {
            // Singleton
            if (Instance is null) {
                Instance = this;
            } else {
                GameObject.DestroyImmediate(this.gameObject);
            }
            DontDestroyOnLoad(this.gameObject);

            // Initialize (초기화)
            try {
                SetupDebug();
                SetupTime();
                SetupEventSystem();
                RegisterModLoaders();
                SetupModDirectory();
                LoadAllMods(); // 추후 변경 필요
                _isInitialized = true;
            } catch {

            }
        }

        private void OnDestroy() {
            if (!_isInitialized) return;

            // Dispose all mods (모든 모드 해제)
            foreach (var mod in _loadedMods.Values) {
                try {
                    mod.Shutdown();
                } catch (System.Exception e) {
                    ModDebug.LogError($"Mod shutdown error: {e.Message}");
                }
            }

            _loadedMods.Clear();
            _failedMods.Clear();
            _isInitialized = false;

            ModDebug.Log("ModEngine shutdown complete");
        }

        private void Update() {
            if (!_isEnableModding || GameDeltaTime == null) return;
            UpdateAllMods(GameDeltaTime());
        }

        private void SetupDebug() {
            ModDebug.IsEnabledDebug = _isEnableDebug;
            if (_debugOption == DebugOption.UnityLog) {
                ModDebug.OnLog += Debug.Log;
                ModDebug.OnLogWarning += Debug.LogWarning;
                ModDebug.OnLogError += Debug.LogError;
            }
        }

        private void SetupTime() {
            if (_timeOption == TimeOption.UnityDeltaTime) {
                GameDeltaTime += () => { return Time.deltaTime; };
            }
        }

        private void SetupEventSystem() {
            ModEventBus.Subscribe<ModEvents.ModLoaded>(OnModLoaded);
            ModEventBus.Subscribe<ModEvents.ModUnloaded>(OnModUnloaded);
            ModEventBus.Subscribe<ModEvents.GameStateChanged>(OnGameStateChanged);

            ModDebug.Log("Event system initialized");
        }

        private void RegisterModLoaders() {
            _modLoaders.Add(new LuaModLoader());
            ModDebug.Log($"Lua loader registered");
        }

        private void SetupModDirectory() {
            ModDirectoryPath = System.IO.Path.Combine(Application.persistentDataPath, _modDirectory);
            ModDebug.Log($"Mod directory: {ModDirectoryPath}");

            if (!System.IO.Directory.Exists(ModDirectoryPath)) {
                System.IO.Directory.CreateDirectory(ModDirectoryPath);
                ModDebug.Log($"Created mod directory: {ModDirectoryPath}");
            }
        }
        #endregion

        #region Mod Loading (모드 로딩)
        public void LoadAllMods() {
            if (_isLoading) {
                ModDebug.Log("Already loading mods");
                return;
            }

            ModDebug.Log("Starting mod loading with dependency resolution");
            _isLoading = true;

            try {
                var modFolders = ModUtility.ScanModsDicrectory(ModDirectoryPath);
                ModDebug.Log($"Found {modFolders.Length} mod folders");

                // 1. Scan all mod (모든 모드 정보를 먼저 스캔)
                var allModInfoDict = new Dictionary<string, (ModInfo info, string path)>();
                foreach (string modFolder in modFolders) {
                    try {
                        if (ModUtility.IsValidModFolder(modFolder)) {
                            var modInfo = ModUtility.LoadModInfo(modFolder);
                            if (modInfo != null && modInfo.enabled) {
                                allModInfoDict[modInfo.name] = (modInfo, modFolder);
                            }
                        }
                    } catch (System.Exception e) {
                        string modName = System.IO.Path.GetFileName(modFolder);
                        ModDebug.LogError($"Failed to read mod info for {modName}: {e.Message}");
                        _failedMods.Add(modName);
                    }
                }

                // 2. Resolve dependencies and determine loading order (의존성 해결 및 로딩 순서 결정)
                var loadingOrder = ResolveDependencies(allModInfoDict);
                if (loadingOrder == null) {
                    ModDebug.LogError("Failed to resolve mod dependencies");
                    _isLoading = false;
                    return;
                }

                // 3. Load mods in dependency order (의존성 순서대로 모드 로딩)
                foreach (string modName in loadingOrder) {
                    if (allModInfoDict.ContainsKey(modName)) {
                        LoadSingleMod(allModInfoDict[modName].path, allModInfoDict[modName].info);
                    }
                }

                _isLoading = false;
                ModDebug.Log($"Loading complete - Success: {_loadedMods.Count}, Failed: {_failedMods.Count}");

            } catch (System.Exception e) {
                ModDebug.LogError($"Error during mod loading: {e.Message}");
                _isLoading = false;
            }
        }

        private void LoadSingleMod(string modFolderPath, ModInfo modInfo = null) {
            string modName = System.IO.Path.GetFileName(modFolderPath);

            try {
                ModDebug.Log($"Loading mod: {modName}");

                // Load mod info if not provided (제공되지 않은 경우 모드 정보 로드)
                if (modInfo == null) {
                    modInfo = ModUtility.LoadModInfo(modFolderPath);
                    if (modInfo == null) {
                        _failedMods.Add(modName);
                        ModDebug.LogError($"Failed to load mod info: {modName}");
                        return;
                    }
                }

                // Check if mod is enabled (모드 활성화 확인)
                if (!modInfo.enabled) {
                    ModDebug.Log($"Mod is disabled: {modName}");
                    return;
                }

                // Check dependencies before loading (로딩 전 의존성 검사)
                if (!CheckDependencies(modInfo)) {
                    _failedMods.Add(modName);
                    ModDebug.LogError($"Dependency check failed for mod: {modName}");
                    return;
                }

                // Find appropriate loader (적절한 로더 찾기)
                IModLoader selectedLoader = null;
                foreach (var loader in _modLoaders) {
                    if (loader.CanLoad(modFolderPath)) {
                        selectedLoader = loader;
                        break;
                    }
                }
                if (selectedLoader == null) {
                    _failedMods.Add(modName);
                    ModDebug.LogError($"No suitable loader found: {modName}");
                    return;
                }

                // Load mod (모드 로딩)
                var modInstance = selectedLoader.LoadMod(modFolderPath);
                if (modInstance != null && modInstance.Initialize()) {
                    _loadedMods[modName] = modInstance;
                    ModDebug.Log($"Mod loaded successfully ({selectedLoader.LoaderName}): {modName}");

                    // Publish mod loaded event (모드 로드 이벤트 발행)
                    ModEventBus.Publish(new ModEvents.ModLoaded { ModName = modName, Version = modInstance.Version });
                } else {
                    _failedMods.Add(modName);
                    ModDebug.LogError($"Mod initialization failed: {modName}");
                }
            } catch (System.Exception e) {
                ModDebug.LogError($"Failed to load mod {modName}: {e.Message}");
                _failedMods.Add(modName);
            }
        }
        #endregion 

        #region Mod Management (모드 관리)
        private void UpdateAllMods(float gameDeltaTime) {
            foreach (var mod in _loadedMods.Values) {
                try {
                    mod.Update(gameDeltaTime);
                } catch (System.Exception e) {
                    ModDebug.LogError($"Mod {mod.Name} update error: {e.Message}");
                }
            }
        }

        public void ReloadMod(string modName) {
            if (_loadedMods.ContainsKey(modName)) {
                ModDebug.Log($"Reloading mod: {modName}");

                // Remove existing mod (기존 모드 제거)
                UnloadMod(modName);

                // Load again (다시 로드)
                string modPath = System.IO.Path.Combine(ModDirectoryPath, modName);
                LoadSingleMod(modPath);
            }
        }

        public void UnloadMod(string modName) {
            if (!_loadedMods.ContainsKey(modName)) {
                ModDebug.LogWarning($"Mod {modName} is not loaded");
                return;
            }

            // Check for dependent mods (종속 모드 확인)
            var dependents = GetDependentMods(modName);
            if (dependents.Count > 0) {
                ModDebug.LogWarning($"Cannot unload {modName}: Other mods depend on it: {string.Join(", ", dependents)}");
                ModDebug.Log($"Consider unloading dependent mods first: {string.Join(", ", dependents)}");
                return;
            }

            ModDebug.Log($"Unloading mod: {modName}");

            try {
                _loadedMods[modName].Shutdown();
                _loadedMods.Remove(modName);

                // Publish unload event (언로드 이벤트 발행)
                ModEventBus.Publish(new ModEvents.ModUnloaded { ModName = modName });

                ModDebug.Log($"Mod {modName} unloaded successfully");
            } catch (System.Exception e) {
                ModDebug.LogError($"Error unloading mod {modName}: {e.Message}");
            }
        }

        /// <summary>
        /// Force unload a mod and all its dependents
        /// 모드와 그에 의존하는 모든 모드를 강제로 언로드
        /// </summary>
        public void ForceUnloadMod(string modName) {
            if (!_loadedMods.ContainsKey(modName)) {
                ModDebug.LogWarning($"Mod {modName} is not loaded");
                return;
            }

            // Recursively collect all dependent mods (모든 종속 모드를 재귀적으로 수집)
            var toUnload = new List<string>();
            var visited = new HashSet<string>();

            void CollectDependents(string targetMod) {
                if (visited.Contains(targetMod)) return;
                visited.Add(targetMod);

                var dependents = GetDependentMods(targetMod);
                foreach (var dependent in dependents) {
                    CollectDependents(dependent);
                    toUnload.Add(dependent);
                }
            }

            CollectDependents(modName);
            toUnload.Add(modName);

            // Remove duplicates and reverse order (중복 제거 및 순서 역순 - 종속 모드를 먼저 언로드)
            toUnload = new List<string>(new HashSet<string>(toUnload));
            toUnload.Reverse();

            ModDebug.Log($"Force unloading mods: {string.Join(" -> ", toUnload)}");

            foreach (string mod in toUnload) {
                if (_loadedMods.ContainsKey(mod)) {
                    try {
                        _loadedMods[mod].Shutdown();
                        _loadedMods.Remove(mod);
                        ModEventBus.Publish(new ModEvents.ModUnloaded { ModName = mod });
                        ModDebug.Log($"Force unloaded: {mod}");
                    } catch (System.Exception e) {
                        ModDebug.LogError($"Error force unloading mod {mod}: {e.Message}");
                    }
                }
            }
        }

        #region Dependency Management (의존성 관리)
        /// <summary>
        /// Resolve mod dependencies and return loading order
        /// 모드 의존성을 해결하고 로딩 순서를 반환
        /// </summary>
        private List<string> ResolveDependencies(Dictionary<string, (ModInfo info, string path)> allMods) {
            var loadingOrder = new List<string>();
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();

            // Topological sort with cycle detection (사이클 검사와 함께 위상 정렬)
            bool DFS(string modName) {
                if (visiting.Contains(modName)) {
                    ModDebug.LogError($"Circular dependency detected involving mod: {modName}");
                    return false; // Circular dependency (순환 의존성)
                }

                if (visited.Contains(modName)) {
                    return true; // Already processed (이미 처리됨)
                }

                if (!allMods.ContainsKey(modName)) {
                    // Dependency not found - handled in CheckDependencies() (의존성을 찾을 수 없음 - CheckDependencies()에서 처리됨)
                    return true;
                }

                visiting.Add(modName);
                var modInfo = allMods[modName].info;

                // Process dependencies first (의존성을 먼저 처리)
                if (modInfo.HasRequiredMods) {
                    foreach (var dependency in modInfo.requiredMods) {
                        if (!DFS(dependency.name)) {
                            return false;
                        }
                    }
                }

                visiting.Remove(modName);
                visited.Add(modName);
                loadingOrder.Add(modName);
                return true;
            }

            // Process all mods (모든 모드 처리)
            foreach (string modName in allMods.Keys) {
                if (!DFS(modName)) {
                    loadingOrder.Remove(modName);
                    continue; // Circular dependency found (순환 의존성 발견)
                }
            }

            ModDebug.Log($"Dependency resolution complete. Loading order: {string.Join(" -> ", loadingOrder)}");
            return loadingOrder;
        }

        /// <summary>
        /// Check if all dependencies are satisfied
        /// 모든 의존성이 만족되는지 확인
        /// </summary>
        private bool CheckDependencies(ModInfo modInfo) {
            if (!modInfo.HasRequiredMods) {
                return true; // No dependencies (의존성 없음)
            }

            foreach (var dependency in modInfo.requiredMods) {
                // Check if dependency is loaded (의존성이 로드되었는지 확인)
                if (!_loadedMods.ContainsKey(dependency.name)) {
                    ModDebug.LogError($"Missing dependency: {modInfo.name} requires {dependency}");
                    return false;
                }

                // Check version compatibility (버전 호환성 확인)
                var loadedMod = _loadedMods[dependency.name];
                if (!dependency.CheckVersion(loadedMod.Version)) {
                    ModDebug.LogError($"Version mismatch: {modInfo.name} requires {dependency}, but loaded version is {loadedMod.Version}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get all mods that depend on the specified mod
        /// 지정된 모드에 의존하는 모든 모드를 가져옴
        /// </summary>
        public List<string> GetDependentMods(string modName) {
            var dependents = new List<string>();

            foreach (var mod in _loadedMods.Values) {
                if (mod.ModInfo.HasRequiredMods) {
                    foreach (var dependency in mod.ModInfo.requiredMods) {
                        if (dependency.name == modName) {
                            dependents.Add(mod.Name);
                            break;
                        }
                    }
                }
            }

            return dependents;
        }
        #endregion

        #endregion

        #region Events (이벤트)
        private void OnModLoaded(ModEvents.ModLoaded eventData) {
            ModDebug.Log($"Event: Mod {eventData.ModName} loaded successfully");
        }

        private void OnModUnloaded(ModEvents.ModUnloaded eventData) {
            ModDebug.Log($"Event: Mod {eventData.ModName} unloaded");
        }

        private void OnGameStateChanged(ModEvents.GameStateChanged eventData) {
            ModDebug.Log($"Event: Game state changed to {eventData.State}");

            // Notify all mods according to game state (게임 상태에 따라 모든 모드에 알림)
            switch (eventData.State) {
                case "Paused":
                foreach (var mod in _loadedMods.Values) {
                    try {
                        mod.OnGamePause();
                    } catch (Exception e) {
                        ModDebug.LogError($"Mod {mod.Name} pause error: {e.Message}");
                    }
                }
                break;
                case "Resumed":
                foreach (var mod in _loadedMods.Values) {
                    try {
                        mod.OnGameResume();
                    } catch (Exception e) {
                        ModDebug.LogError($"Mod {mod.Name} resume error: {e.Message}");
                    }
                }
                break;
            }
        }
        #endregion
    }
}