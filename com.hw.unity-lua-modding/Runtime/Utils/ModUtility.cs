using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Modding.Utils
{
    public static class ModUtility {

        public static bool IsValidModFolder(string folderPath) {
            // Check for mod.json file (mod.json 파일 확인)
            string modJsonPath = Path.Combine(folderPath, "mod.json");
            return File.Exists(modJsonPath);
        }
        public static string[] ScanModsDicrectory(string path) {
            return Directory.GetDirectories(path);
        }

        public static List<ModInfo> GetAllModInfo(string path) {
            List<ModInfo> infos = new();
            string[] folderNames = ScanModsDicrectory(path);
            foreach (var folderName in folderNames) {
                if (JsonExists(folderName)) {
                    infos.Add(LoadModInfo(folderName));
                }
            }
            return infos;
        }
        public static bool JsonExists(string modFolderPath) {
            // mod.json
            string modJsonPath = Path.Combine(modFolderPath, "mod.json");
            if (!File.Exists(modJsonPath)) {
                return false;
            }
            return true;
        }
        public static ModInfo LoadModInfo(string modFolderPath) {
            try {
                string modJsonPath = Path.Combine(modFolderPath, "mod.json");
                string jsonContent = File.ReadAllText(modJsonPath);
                ModInfo info = JsonUtility.FromJson<ModInfo>(jsonContent);

                if (PlayerPrefs.HasKey(info.name)) {
                    info.enabled = PlayerPrefs.GetInt(info.name) == 1 ? true : false;
                } else {
                    PlayerPrefs.SetInt(info.name, 1);
                }

                return info;
            } catch (System.Exception e) {
                ModDebug.LogError($"fail read mod.json : {e.Message}");
                return null;
            }
        }
    }
}
