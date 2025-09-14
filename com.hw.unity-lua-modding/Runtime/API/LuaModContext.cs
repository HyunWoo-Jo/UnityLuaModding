using Modding.Utils;
using System.IO;
using UnityEngine;

namespace Modding.API {
    /// <summary>
    /// Lua 모드에서 사용할 수 있는 컨텍스트 API
    /// </summary>
    public class LuaModContext {
        private string modFolderPath;

        public LuaModContext(string folderPath) {
            modFolderPath = folderPath;
        }

        /// <summary>
        /// 텍스트 파일 로드
        /// </summary>
        public string LoadTextFile(string relativePath) {
            try {
                string fullPath = Path.Combine(modFolderPath, relativePath);
                if (File.Exists(fullPath)) {
                    return File.ReadAllText(fullPath);
                }

                ModDebug.LogWarning($"[LuaContext] 파일을 찾을 수 없음: {relativePath}");
                return "";
            } catch (System.Exception e) {
                ModDebug.LogError($"[LuaContext] 파일 로드 실패: {e.Message}");
                return "";
            }
        }

        /// <summary>
        /// 이미지 파일 로드
        /// </summary>
        public Texture2D LoadTexture(string relativePath) {
            try {
                string fullPath = Path.Combine(modFolderPath, relativePath);
                if (File.Exists(fullPath)) {
                    byte[] imageData = File.ReadAllBytes(fullPath);
                    Texture2D texture = new Texture2D(2, 2);

                    if (texture.LoadImage(imageData)) {
                        return texture;
                    }

                    Object.Destroy(texture);
                }
                ModDebug.LogWarning($"[LuaContext] 텍스처를 찾을 수 없음: {relativePath}");
                return null;
            } catch (System.Exception e) {
                ModDebug.LogError($"[LuaContext] 텍스처 로드 실패: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 게임 오브젝트 찾기
        /// </summary>
        public GameObject FindGameObject(string name) {
            return GameObject.Find(name);
        }

        /// <summary>
        /// 로그 출력
        /// </summary>
        public void Log(string message) {
            ModDebug.Log($"[LuaContext] {message}");
        }
    }
}