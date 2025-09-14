using UnityEngine;
using System.Collections.Generic;

namespace Modding.API {
    /// <summary>
    /// Lua에서 Unity API를 사용하기 위한 래퍼 클래스
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
                Debug.LogWarning($"[UnityAPI] 잘못된 키 이름: {keyName} - {e.Message}");
                return false;
            }
        }

        public bool GetKeyDown(string keyName) {
            try {
                return Input.GetKeyDown(keyName);
            } catch (System.Exception e) {
                Debug.LogWarning($"[UnityAPI] 잘못된 키 이름: {keyName} - {e.Message}");
                return false;
            }
        }

        public bool GetKeyUp(string keyName) {
            try {
                return Input.GetKeyUp(keyName);
            } catch (System.Exception e) {
                Debug.LogWarning($"[UnityAPI] 잘못된 키 이름: {keyName} - {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 마우스 버튼 확인
        /// </summary>
        public bool GetMouseButton(int button) {
            return Input.GetMouseButton(button);
        }

        /// <summary>
        /// 마우스 위치 가져오기
        /// </summary>
        public Dictionary<string, float> GetMousePosition() {
            var mousePos = Input.mousePosition;
            return new Dictionary<string, float> {
                ["x"] = mousePos.x,
                ["y"] = mousePos.y
            };
        }
        #endregion

        #region Vector3 관련
        /// <summary>
        /// Vector3 생성 (Lua에서 사용하기 쉽도록 Dictionary 반환)
        /// </summary>
        public Dictionary<string, float> CreateVector3(float x, float y, float z) {
            return new Dictionary<string, float> {
                ["x"] = x,
                ["y"] = y,
                ["z"] = z
            };
        }

        /// <summary>
        /// 두 Vector3 사이의 거리 계산
        /// </summary>
        public float Distance(Dictionary<string, float> pos1, Dictionary<string, float> pos2) {
            Vector3 v1 = new Vector3(pos1["x"], pos1["y"], pos1["z"]);
            Vector3 v2 = new Vector3(pos2["x"], pos2["y"], pos2["z"]);
            return Vector3.Distance(v1, v2);
        }

        /// <summary>
        /// Vector3 정규화
        /// </summary>
        public Dictionary<string, float> Normalize(Dictionary<string, float> vector) {
            Vector3 v = new Vector3(vector["x"], vector["y"], vector["z"]);
            Vector3 normalized = v.normalized;

            return new Dictionary<string, float> {
                ["x"] = normalized.x,
                ["y"] = normalized.y,
                ["z"] = normalized.z
            };
        }
        #endregion

        #region 물리 관련
        /// <summary>
        /// 오브젝트에 힘 가하기
        /// </summary>
        public void AddForce(GameObject obj, float x, float y, float z) {
            if (obj == null) return;

            var rigidbody = obj.GetComponent<Rigidbody>();
            if (rigidbody != null) {
                rigidbody.AddForce(new Vector3(x, y, z));
                Debug.Log($"[UnityAPI] {obj.name}에 힘 적용: ({x}, {y}, {z})");
            } else {
                Debug.LogWarning($"[UnityAPI] {obj.name}에 Rigidbody가 없습니다.");
            }
        }

        /// <summary>
        /// 레이캐스트 (간단한 버전)
        /// </summary>
        public bool Raycast(Dictionary<string, float> origin, Dictionary<string, float> direction, float maxDistance = 100f) {
            Vector3 originVec = new Vector3(origin["x"], origin["y"], origin["z"]);
            Vector3 directionVec = new Vector3(direction["x"], direction["y"], direction["z"]);

            return Physics.Raycast(originVec, directionVec.normalized, maxDistance);
        }
        #endregion

        #region 유틸리티
        /// <summary>
        /// 랜덤 값 생성
        /// </summary>
        public float Random(float min = 0f, float max = 1f) {
            return UnityEngine.Random.Range(min, max);
        }

        /// <summary>
        /// 정수 랜덤 값 생성
        /// </summary>
        public int RandomInt(int min = 0, int max = 10) {
            return UnityEngine.Random.Range(min, max);
        }

        /// <summary>
        /// 수학 함수들
        /// </summary>
        public float Sin(float value) => Mathf.Sin(value);
        public float Cos(float value) => Mathf.Cos(value);
        public float Sqrt(float value) => Mathf.Sqrt(value);
        public float Abs(float value) => Mathf.Abs(value);
        public float Clamp(float value, float min, float max) => Mathf.Clamp(value, min, max);

        /// <summary>
        /// 로그 출력
        /// </summary>
        public void Log(string message) {
            Debug.Log($"[Unity Lua] {message}");
        }

        public void LogWarning(string message) {
            Debug.LogWarning($"[Unity Lua] {message}");
        }

        public void LogError(string message) {
            Debug.LogError($"[Unity Lua] {message}");
        }
        #endregion

        #region 오디오 관련 (기본 구현)
        /// <summary>
        /// 사운드 재생 (AudioSource가 있는 경우)
        /// </summary>
        public void PlaySound(GameObject audioObject) {
            if (audioObject != null) {
                var audioSource = audioObject.GetComponent<AudioSource>();
                if (audioSource != null) {
                    audioSource.Play();
                    Debug.Log($"[UnityAPI] 사운드 재생: {audioObject.name}");
                }
            }
        }
        #endregion
    }
}