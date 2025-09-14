using UnityEngine;

namespace Modding.Engine {

    public interface IModInstance {
        string Name { get; }
        string Version { get; }
        bool IsLoaded { get; }

        bool Initialize();
        void Update(float deltaTime);
        void OnGamePause();
        void OnGameResume();
        void Shutdown();
    }
}