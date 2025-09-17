using UnityEngine;
using System;
namespace Modding.Engine {

    public interface IModInstance {

        ModInfo ModInfo { get; }
        string Name { get; }
        Version Version { get; }
        bool IsLoaded { get; }

        bool Initialize();
        void Update(float deltaTime);
        void OnGamePause();
        void OnGameResume();
        void Shutdown();

        void SceneChanged(string sceneName);
    }
}