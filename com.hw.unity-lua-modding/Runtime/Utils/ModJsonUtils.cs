using UnityEngine;
using System.IO;

namespace Modding.Utils
{
    public static class ModJsonUtils {
        public static bool Exists(string modFolderPath) {
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
                return JsonUtility.FromJson<ModInfo>(jsonContent);
            } catch (System.Exception e) {
                ModDebug.LogError($"fail read mod.json : {e.Message}");
                return null;
            }
        }
    }
}
