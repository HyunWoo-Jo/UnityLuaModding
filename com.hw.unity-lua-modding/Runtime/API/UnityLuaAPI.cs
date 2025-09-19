using UnityEngine;
using System.Collections.Generic;

namespace Modding.API {
    /// <summary>
    /// Use Unity API in Lua (Lua에서 Unity API를 사용하기 위한 래퍼 클래스)
    /// </summary>
    public partial class UnityLuaAPI {
        #region Time

        public float GetDeltaTime() {
            return Time.deltaTime;
        }
        public float GetTime() {
            return Time.time;
        }

        public float GetTimeScale() {
            return Time.timeScale;
        }
        #endregion

        #region Input 

        public bool GetKey(string keyName) {
            try {
                return Input.GetKey(keyName);
            } catch (System.Exception e) {
                Debug.LogWarning($"[UnityAPI] Invalid key name: {keyName} - {e.Message}");
                return false;
            }
        }

        public bool GetKeyDown(string keyName) {
            try {
                return Input.GetKeyDown(keyName);
            } catch (System.Exception e) {
                Debug.LogWarning($"[UnityAPI] Invalid key name: {keyName} - {e.Message}");
                return false;
            }
        }

        public bool GetKeyUp(string keyName) {
            try {
                return Input.GetKeyUp(keyName);
            } catch (System.Exception e) {
                Debug.LogWarning($"[UnityAPI] Invalid key name: {keyName} - {e.Message}");
                return false;
            }
        }

        public bool GetMouseButton(int button) {
            return Input.GetMouseButton(button);
        }

        public Vector3 GetMousePosition() {
            return Input.mousePosition;
        }
        #endregion

        #region Vector3 관련

        public Vector3 CreateVector3(float x, float y, float z) {
            return new Vector3(x, y, z);
        }

        public float Distance(Vector3 pos1, Vector3 pos2) {
            return Vector3.Distance(pos1, pos2);
        }

        public string GetType(object obj) {
            return obj.GetType().Name;
        }

        public Vector3 Normalize(Vector3 vector) {
            return vector.normalized;
        }
        #endregion

        #region Physics (물리)

        public void AddForce(GameObject obj, Vector3 pos) {
            if (obj == null) return;

            var rigidbody = obj.GetComponent<Rigidbody>();
            if (rigidbody != null) {
                rigidbody.AddForce(pos);
            }
        }


        public bool Raycast(Vector3 origin, Vector3 direction, float maxDistance = 100f) {
            return Physics.Raycast(origin, direction.normalized, maxDistance);
        }
        #endregion

        #region Utils (유틸)

        public float Random(float min = 0f, float max = 1f) {
            return UnityEngine.Random.Range(min, max);
        }

        public int RandomInt(int min = 0, int max = 10) {
            return UnityEngine.Random.Range(min, max);
        }

        public float Sin(float value) => Mathf.Sin(value);
        public float Cos(float value) => Mathf.Cos(value);
        public float Sqrt(float value) => Mathf.Sqrt(value);
        public float Abs(float value) => Mathf.Abs(value);
        public float Clamp(float value, float min, float max) => Mathf.Clamp(value, min, max);

        #endregion

    }
}