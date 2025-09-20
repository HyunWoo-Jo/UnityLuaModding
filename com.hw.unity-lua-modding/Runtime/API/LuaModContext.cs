using Modding.Utils;
using NLua;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Modding.API {
    /// <summary>
    /// Context API for Lua mods to interact with Unity environment
    /// Lua 모드에서 사용할 수 있는 컨텍스트 API
    /// </summary>
    public partial class LuaModContext {
        private string _modFolderPath;
        #region Cache
        private Dictionary<string, Texture2D> _textureCaches = new();
        private Dictionary<string, Sprite> _spriteCaches = new();
        #endregion

        private event Action OnDispose;

        #region LifeCycle
        public LuaModContext(string folderPath) {
            _modFolderPath = folderPath;
            OnDispose += DisposeAllTexture;
        }

        public void Dispose() {
            OnDispose?.Invoke();
        }
        #endregion
        public string LoadTextFile(string relativePath) {
            try {
                string fullPath = Path.Combine(_modFolderPath, relativePath);
                if (File.Exists(fullPath)) {
                    return File.ReadAllText(fullPath);
                }
                ModDebug.LogWarning($"[LuaContext] File not found: {relativePath}");
                return "";
            } catch (Exception e) {
                ModDebug.LogError($"[LuaContext] Failed to load file: {e.Message}");
                return "";
            }
        }

        public Sprite LoadSprite(string relativePath) {
            if(_spriteCaches.TryGetValue(relativePath, out var sprite)) {
                return sprite;
            }
            Texture2D texture = LoadTexture(relativePath);
            if (texture != null) {
                return _spriteCaches[relativePath] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            }
            return null;
        }

        private Texture2D LoadTexture(string relativePath) {
            try {
                if (_textureCaches.TryGetValue(relativePath, out Texture2D cacheTexture)) {
                    if(cacheTexture != null) {
                        return cacheTexture;
                    }
                }
                string fullPath = Path.Combine(_modFolderPath, relativePath);
                if (File.Exists(fullPath)) {
                    byte[] imageData = File.ReadAllBytes(fullPath);
                    Texture2D texture = new Texture2D(2, 2);
                    if (texture.LoadImage(imageData)) {
                        _textureCaches[relativePath] = texture;
                        return texture;
                    }
                    Texture2D.Destroy(texture);
                }
                ModDebug.LogWarning($"[LuaContext] Texture not found: {relativePath}");
                return null;
            } catch (Exception e) {
                ModDebug.LogError($"[LuaContext] Failed to load texture: {e.Message}");
                return null;
            }
        }

        private void DisposeAllTexture() {
            foreach (var texture in _textureCaches.Values) {
                Texture2D.Destroy(texture);
            }
            _textureCaches.Clear();
        }

        public ModUIInfos LoadUIInfo(string relativePath) {
            try {
                string fullPath = Path.Combine(_modFolderPath, relativePath);
                if (!File.Exists(fullPath)) {
                    ModDebug.LogError($"[LuaContext] UI file not found: {fullPath}");
                    return null;
                }

                string jsonContent = File.ReadAllText(fullPath);

                ModUIInfos uiInfos = JsonUtility.FromJson<ModUIInfos>(jsonContent);

                if (uiInfos == null) {
                    ModDebug.LogError($"[LuaContext] Failed to parse UI JSON: {relativePath}");
                    return null;
                }

                ModDebug.Log($"[LuaContext] Successfully loaded UI: {relativePath}");
                return uiInfos;
            } catch (Exception e) {
                ModDebug.LogError($"[LuaContext] Failed to load UIInfo: {e.Message}");
                return null;
            }

        }


        

        public GameObject FindGameObject(string name) {
            return GameObject.Find(name);
        }
    }
}