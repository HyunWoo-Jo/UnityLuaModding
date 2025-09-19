using UnityEngine;
using System.Collections.Generic;

namespace Modding.API {
    /// <summary>
    /// Use Game API in Lua (Lua 모드에서 사용할 수 있는 게임 API)
    /// </summary>
    public partial class LuaModAPI {
        #region GameObject

        public GameObject FindGameObject(string name) {
            return GameObject.Find(name);
        }

        public GameObject FindGameObjectWithTag(string tag) {
            return GameObject.FindGameObjectWithTag(tag);
        }


        public GameObject[] FindGameObjectsWithTag(string tag) {
            return GameObject.FindGameObjectsWithTag(tag);
        }


        public GameObject CreateGameObject(string name) {
            var obj = new GameObject(name);
            return obj;
        }

        public GameObject Instantiate(GameObject obj) {
            return GameObject.Instantiate(obj);
        }

        public void DestroyGameObject(GameObject obj) {
            if (obj != null) {
                Object.Destroy(obj);
            }
        }
        #endregion

        #region Player
        public GameObject GetPlayer() {
            return GameObject.FindGameObjectWithTag("Player");
        }

        public Vector3 GetPlayerPosition() {
            return GetPlayer().transform.position;
        }

        public void SetPlayerPosition(float x, float y, float z) {
            var player = GetPlayer();
            if (player != null) {
                player.transform.position = new Vector3(x, y, z);
            }
        }

        #endregion

    }
}