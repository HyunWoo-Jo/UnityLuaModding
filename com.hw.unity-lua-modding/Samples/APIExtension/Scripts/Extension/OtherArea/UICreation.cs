using UnityEngine;
using Modding.API;
namespace Modding.Sample
{
    public class UICreation : MonoBehaviour, IModUIManager
    {
        private void Awake() {
           ModLuaUIExtension.Instance.InjectUIManager(this);
        }
        public void CreateUI(GameObject obj) {
            Debug.Log("Creation UI: " + obj.name);
        }

 
    }
}
