using UnityEngine;

namespace Modding.Bridge
{
    public interface IGameEventBridge {
        void RegisterGameEvents();
        void UnregisterGameEvents();
    }
}
