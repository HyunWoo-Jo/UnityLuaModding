using UnityEngine;
using System;
using System.Collections.Generic;
using Modding.Loaders;
using Modding.Utils;
namespace Modding.Engine {
    /// <summary>
    /// Manages the entire modding system
    /// 전체 Mod를 관리하는 핵심 시스템
    /// </summary>
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
        #endregion

        #region State (상태)
        private bool _isInitialized = false;
        private bool _isLoading = false;

        public bool IsInitialized => _isInitialized;
        public bool IsLoading => _isLoading;
        public string ModDirectoryPath { get; private set; }
        #endregion

        #region Manage Mod (모드관리)
        private Dictionary<string, IModInstance> _loadedMods = new();
        private List<string> _failedMods = new();
        private List<IModLoader> _modLoaders = new();

        public IReadOnlyDictionary<string, IModInstance> LoadedMods => _loadedMods;
        public int LoadedModCount => _loadedMods.Count;
        #endregion

        #region LifeCycle 
        private void Awake() {
            // Singleton
            if (Instance is null) {
                Instance = this;
            } else {
                GameObject.DestroyImmediate(this.gameObject);
            }
            DontDestroyOnLoad(this.gameObject);

            // Init (초기화)
            try {
                SetupDebug();
                RegisterModLoaders();
                SetupModDirectory();

                LoadAllMods(); // 추후 변경
                _isInitialized = true;
            } catch {

            }
        }

        private void OnDestroy() {
            if (!_isInitialized) return;

            // Dispose All Mod (모드 언로드)
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
            if (!_isEnableModding) return;
            UpdateAllMods(Time.deltaTime);
        }

        private void SetupDebug() {
            ModDebug.IsEnabledDebug = _isEnableDebug;
            if (_debugOption == DebugOption.UnityLog) {
                ModDebug.OnLog += Debug.Log;
                ModDebug.OnLogWarning += Debug.LogWarning;
                ModDebug.OnLogError += Debug.LogError;
            }
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

        #region Mod Loading (관리)
        public void LoadAllMods() {
            if (_isLoading) {
                ModDebug.Log("Already loading mods");
                return;
            }

            ModDebug.Log("Starting mod loading");
            _isLoading = true;

            try {
                var modFolders = ScanModsDicrectory();
                ModDebug.Log($"Found {modFolders.Length} mod folders");

                foreach (string modFolder in modFolders) {
                    LoadSingleMod(modFolder);
                }

                _isLoading = false;
                ModDebug.Log($"Loading complete Success: {_loadedMods.Count}, Failed: {_failedMods.Count}");
            } catch (System.Exception e) {
                ModDebug.LogError($"Error during mod loading: {e.Message}");
                _isLoading = false;
            }
        }
        private void LoadSingleMod(string modFolderPath) {
            string modName = System.IO.Path.GetFileName(modFolderPath);

            try {
                ModDebug.Log($"Loading mod: {modName}");

                // Find appropriate loader
                IModLoader selectedLoader = null;
                foreach (var loader in _modLoaders) {
                    if (loader.CanLoad(modFolderPath)) {
                        selectedLoader = loader;
                        break;
                    }
                }
                if (selectedLoader == null) {
                    _failedMods.Add(modName);
                    ModDebug.Log($"No suitable loader found: {modName}");
                    return;
                }

                // Load mod
                var modInstance = selectedLoader.LoadMod(modFolderPath);
                if (modInstance != null && modInstance.Initialize()) {
                    _loadedMods[modName] = modInstance;
                    ModDebug.Log($"Mod loaded successfully ({selectedLoader.LoaderName}): {modName}");

                    // Mod loaded event
                    //EventBus.Publish(new ModLoadedEvent { ModName = modName, Version = modInstance.Version });
                } else {
                    _failedMods.Add(modName);
                    ModDebug.Log($"Mod initialization failed: {modName}");
                }
            } catch (System.Exception e) {
                ModDebug.LogError($"Failed to load mod {modName}: {e.Message}");
                _failedMods.Add(modName);
            }
        }
        private bool IsValidModFolder(string folderPath) {
            // Check for mod.json file
            string modJsonPath = System.IO.Path.Combine(folderPath, "mod.json");
            return System.IO.File.Exists(modJsonPath);
        }
        #endregion 

        #region Mod Manage
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

                // Remove existing mod
                UnloadMod(modName);

                // Load again
                string modPath = System.IO.Path.Combine(ModDirectoryPath, modName);
                LoadSingleMod(modPath);
            }
        }
        public void UnloadMod(string modName) {
            if (_loadedMods.ContainsKey(modName)) {
                ModDebug.Log($"Unloading mod: {modName}");
                _loadedMods[modName].Shutdown();
                _loadedMods.Remove(modName);
            }
        }
        //private void OnGamePause(PauseGameEvent e) {
        //    ModDebug.Log("Game paused - Pausing mod system");
        //    // Notify mods about pause
        //    foreach (var mod in _loadedMods.Values) {
        //        mod.OnGamePause();
        //    }
        //}

        //private void OnGameResume(ResumeGameEvent e) {
        //    ModDebug.Log("Game resumed - Resuming mod system");
        //    // Notify mods about resume
        //    foreach (var mod in _loadedMods.Values) {
        //        mod.OnGameResume();
        //    }
        //}
        #endregion


        #region public
        public string[] ScanModsDicrectory() {
            return System.IO.Directory.GetDirectories(ModDirectoryPath);
        }

        public List<ModInfo> GetAllModInfo() {
            List<ModInfo> infos = new();
            string[] folderNames = ScanModsDicrectory();
            foreach (var folderName in folderNames) {
                if (ModJsonUtils.Exists(folderName)) {
                    infos.Add(ModJsonUtils.LoadModInfo(folderName));
                }
            }
            return infos;
        }

        #endregion
    }
}