using UnityEngine;
using Modding.Engine;

namespace Modding.Loaders {
    public interface IModLoader {
        /// <summary>
        /// Check if the mod can be loaded (로드 가능한지 확인)
        /// </summary>
        bool CanLoad(string modFolderPath);
        /// <summary>
        /// Load the mod and return instance (로드하고 인스턴스 반환)
        /// </summary>
        IModInstance LoadMod(string modFolderPath);

        string LoaderName { get; }

        string[] SupportedExtensions { get; }
    }
}
