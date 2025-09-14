using UnityEngine;
using System.Collections.Generic;

namespace Modding.API {
    /// <summary>
    /// Lua 모드에서 사용할 수 있는 게임 API
    /// </summary>
    public partial class LuaModAPI {
        #region 게임 오브젝트 관련
        /// <summary>
        /// 이름으로 게임 오브젝트 찾기
        /// </summary>
        public GameObject FindGameObject(string name) {
            return GameObject.Find(name);
        }

        /// <summary>
        /// 태그로 게임 오브젝트 찾기
        /// </summary>
        public GameObject FindGameObjectWithTag(string tag) {
            return GameObject.FindGameObjectWithTag(tag);
        }

        /// <summary>
        /// 태그로 모든 게임 오브젝트 찾기
        /// </summary>
        public GameObject[] FindGameObjectsWithTag(string tag) {
            return GameObject.FindGameObjectsWithTag(tag);
        }

        /// <summary>
        /// 새 게임 오브젝트 생성
        /// </summary>
        public GameObject CreateGameObject(string name) {
            var obj = new GameObject(name);
            Debug.Log($"[ModAPI] 오브젝트 생성: {name}");
            return obj;
        }

        /// <summary>
        /// 게임 오브젝트 삭제
        /// </summary>
        public void DestroyGameObject(GameObject obj) {
            if (obj != null) {
                Object.Destroy(obj);
                Debug.Log($"[ModAPI] 오브젝트 삭제: {obj.name}");
            }
        }
        #endregion

        #region 플레이어 관련
        /// <summary>
        /// 플레이어 오브젝트 가져오기
        /// </summary>
        public GameObject GetPlayer() {
            return GameObject.FindGameObjectWithTag("Player");
        }

        /// <summary>
        /// 플레이어 위치 가져오기 (Lua 테이블로 반환)
        /// </summary>
        public Dictionary<string, float> GetPlayerPosition() {
            var player = GetPlayer();
            if (player != null) {
                var pos = player.transform.position;
                return new Dictionary<string, float> {
                    ["x"] = pos.x,
                    ["y"] = pos.y,
                    ["z"] = pos.z
                };
            }
            return new Dictionary<string, float> { ["x"] = 0, ["y"] = 0, ["z"] = 0 };
        }

        /// <summary>
        /// 플레이어 위치 설정
        /// </summary>
        public void SetPlayerPosition(float x, float y, float z) {
            var player = GetPlayer();
            if (player != null) {
                player.transform.position = new Vector3(x, y, z);
                Debug.Log($"[ModAPI] 플레이어 위치 변경: ({x}, {y}, {z})");
            }
        }

        /// <summary>
        /// 플레이어 텔레포트
        /// </summary>
        public void TeleportPlayer(float x, float y, float z) {
            SetPlayerPosition(x, y, z);
            ShowNotification($"플레이어 텔레포트: ({x:F1}, {y:F1}, {z:F1})", 2.0f);
        }
        #endregion

        #region UI 관련
        /// <summary>
        /// 화면에 알림 표시
        /// </summary>
        public void ShowNotification(string message, float duration = 3.0f) {
            Debug.Log($"[ModAPI 알림] {message} ({duration}초)");
            // 실제로는 UI 시스템과 연동
            // EventBus.Publish(new ShowNotificationEvent { Message = message, Duration = duration });
        }

        /// <summary>
        /// 콘솔 메시지 출력
        /// </summary>
        public void ShowConsoleMessage(string message) {
            Debug.Log($"[ModAPI Console] {message}");
        }
        #endregion

        //#region 이벤트 관련
        ///// <summary>
        ///// 게임 이벤트 발행
        ///// </summary>
        //public void TriggerGameEvent(string eventName, object data = null) {
        //    Debug.Log($"[ModAPI] 이벤트 발행: {eventName}");

        //    // 실제로는 EventBus와 연동
        //    switch (eventName.ToLower()) {
        //        case "pause":
        //        EventBus.Publish(new PauseGameEvent());
        //        break;
        //        case "resume":
        //        EventBus.Publish(new ResumeGameEvent());
        //        break;
        //        default:
        //        Debug.LogWarning($"[ModAPI] 알 수 없는 이벤트: {eventName}");
        //        break;
        //    }
        //}
        //#endregion



        #region 게임 상태 관련
        /// <summary>
        /// 게임 일시정지
        /// </summary>
        public void PauseGame() {
            //TriggerGameEvent("pause");
        }

        /// <summary>
        /// 게임 재시작
        /// </summary>
        public void ResumeGame() {
            //TriggerGameEvent("resume");
        }

        /// <summary>
        /// 게임 시간 배속 설정
        /// </summary>
        public void SetGameSpeed(float speed) {
            Time.timeScale = Mathf.Clamp(speed, 0f, 10f);
            Debug.Log($"[ModAPI] 게임 속도 변경: {speed}x");
        }
        #endregion
    }
}