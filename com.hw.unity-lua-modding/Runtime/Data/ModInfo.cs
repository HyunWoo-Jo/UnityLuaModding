using System;

namespace Modding {
    [Serializable]
    public class ModInfo {
        public string name;
        public string version = "1.0";
        public string description;
        public string author;
        public ModDependency[] requiredMods = new ModDependency[0];
        public string[] targetScenes = new string[0];

       
        public bool enabled = true;
        public string path;

        public bool HasRequiredMods => requiredMods != null && requiredMods.Length > 0;
        public System.Version GetVersion() {
            if (System.Version.TryParse(version, out System.Version ver))
                return ver;
            return new System.Version(1, 0, 0);
        }
    }
}