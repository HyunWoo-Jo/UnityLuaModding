using Modding.Engine;
using System;
using UnityEngine;

namespace Modding {
    public partial class ModEvents {
        public class ModLoaded {
            public string ModName { get; set; }
            public System.Version Version { get; set; }
            public IModInstance Instance { get; set; }
        }

        public class ModUnloaded {
            public string ModName { get; set; }
        }

        public class GameStateChanged {
            public string State { get; set; } // "Paused", "Resumed", "Started", "Stopped"
        }

        public class ModsReloaded { 
            public int LoadedCount { get; set; }
            public int FailedCount { get; set; }
        }
    }
}
