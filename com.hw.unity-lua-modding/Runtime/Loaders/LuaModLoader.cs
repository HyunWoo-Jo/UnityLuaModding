using UnityEngine;
using System.IO;
using Modding.Engine;
using Modding.Utils;
namespace Modding.Loaders {
    public class LuaModLoader : IModLoader {
        public string LoaderName => "Lua Mod Loader";

        public string[] SupportedExtensions => new string[] { ".lua" };

        public bool CanLoad(string modFolderPath) {
            // 1. Scan mod.json 
            if (!ModUtility.JsonExists(modFolderPath)) {
                return false;
            }

            // 2. Scan Lua Script (Lus script 파일 확인)
            string[] possibleLuaPaths = {
                Path.Combine(modFolderPath, "Scripts", "main.lua"),
                Path.Combine(modFolderPath, "Scripts", "mod.lua"),
                Path.Combine(modFolderPath, "main.lua")
            };

            foreach (string path in possibleLuaPaths) {
                if (File.Exists(path)) {
                    return true;
                }
            }

            return false;
        }

        public IModInstance LoadMod(string modFolderPath) {
            try {
                // Read mod.json (mod.json 읽어 info로 변환)
                ModInfo modInfo = ModUtility.LoadModInfo(modFolderPath);
                if (modInfo == null) {
                    return null;
                }

                // Create Lua mod instance (lua instance 생성)
                var luaMod = new LuaModInstance(modInfo);
                return luaMod;

            } catch (System.Exception e) {
                Debug.LogError($"[LuaLoader] failed mod loading: {e.Message}");
                return null;
            }
        }
    }
}
