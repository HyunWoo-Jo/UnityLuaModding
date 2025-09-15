using System;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace Modding.Utils
{
    public static class ModDebug {


        public static bool IsEnabledDebug { get; set; }
        public static event Action<object> OnLog;
        public static event Action<object> OnLogWarning;
        public static event Action<object> OnLogError;

        public static void Log(object message) {
            if(IsEnabledDebug) OnLog?.Invoke(message);
        }
        public static void LogWarning(object message) {
            if (IsEnabledDebug) OnLogWarning?.Invoke(message);
        }
        public static void LogError(object message) {
            if (IsEnabledDebug) OnLogError?.Invoke(message);
        }
    }
}
