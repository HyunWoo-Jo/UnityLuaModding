using Modding.Utils;
using System.IO;
using UnityEngine;
namespace Modding.API {
    /// <summary>
    /// Context API for Lua mods to interact with Unity environment
    /// Lua 모드에서 사용할 수 있는 컨텍스트 API
    /// </summary>
    public partial class LuaModContext {
        private string _modFolderPath;

        public LuaModContext(string folderPath) {
            _modFolderPath = folderPath;
        }

        public string LoadTextFile(string relativePath) {
            try {
                string fullPath = Path.Combine(_modFolderPath, relativePath);
                if (File.Exists(fullPath)) {
                    return File.ReadAllText(fullPath);
                }
                ModDebug.LogWarning($"[LuaContext] File not found: {relativePath}");
                return "";
            } catch (System.Exception e) {
                ModDebug.LogError($"[LuaContext] Failed to load file: {e.Message}");
                return "";
            }
        }


        public Texture2D LoadTexture(string relativePath) {
            try {
                string fullPath = Path.Combine(_modFolderPath, relativePath);
                if (File.Exists(fullPath)) {
                    byte[] imageData = File.ReadAllBytes(fullPath);
                    Texture2D texture = new Texture2D(2, 2);
                    if (texture.LoadImage(imageData)) {
                        return texture;
                    }
                    Object.Destroy(texture);
                }
                ModDebug.LogWarning($"[LuaContext] Texture not found: {relativePath}");
                return null;
            } catch (System.Exception e) {
                ModDebug.LogError($"[LuaContext] Failed to load texture: {e.Message}");
                return null;
            }
        }


        public GameObject FindGameObject(string name) {
            return GameObject.Find(name);
        }
    }
}