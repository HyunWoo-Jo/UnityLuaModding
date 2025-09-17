using UnityEngine;

namespace Modding.API {
    public interface IModUIManager {
        void CreateUI(GameObject obj);
    }
    
    public class ModLuaUIExtension {
        public static ModLuaUIExtension Instance { get; }  = new ModLuaUIExtension();

        public IModUIManager UIManager { get; private set; }

        public void InjectUIManager(IModUIManager uiManager) {
            UIManager = uiManager;
        }

    }

    public partial class LuaModAPI {
        public void CreateUI(GameObject obj) {
            ModLuaUIExtension.Instance.UIManager.CreateUI(obj);
        }
    }
}