using UnityEngine;
using Modding.API;
public class TestEventPublish : MonoBehaviour
{
    private void Start() {
        LuaEventAPI.Publish("TestEvent", null);
    }
}
