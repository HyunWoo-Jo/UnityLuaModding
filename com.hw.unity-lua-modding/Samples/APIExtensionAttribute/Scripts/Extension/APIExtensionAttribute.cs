using UnityEngine;
using Modding.API;
namespace Modding.Sample
{
    // 3. Unity API 클래스들
    [ModAPICategory("Unity.Core")]
    public static class UnityGameObjectAPI {
        [ModAPI("GameObject.Find", "게임오브젝트 찾기")]
        public static GameObject Find(string name) {
            return GameObject.Find(name);
        }

        [ModAPI("GameObject.Create", "새 게임오브젝트 생성")]
        public static GameObject CreateGameObject(string name) {
            return new GameObject(name);
        }

        [ModAPI("GameObject.Destroy", "게임오브젝트 삭제")]
        public static void DestroyGameObject(GameObject obj) {
            Object.Destroy(obj);
        }
    }

    [ModAPICategory("Unity.Transform")]
    public static class UnityTransformAPI {
        [ModAPI("Transform.SetPosition", "위치 설정")]
        public static void SetPosition(Transform transform, float x, float y, float z) {
            transform.position = new Vector3(x, y, z);
        }

        [ModAPI("Transform.GetPosition", "위치 가져오기")]
        public static Vector3 GetPosition(Transform transform) {
            return transform.position;
        }
    }

    [ModAPICategory("Unity.Physics")]
    public static class UnityPhysicsAPI {
        [ModAPI("Physics.Raycast", "레이캐스트")]
        public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance) {
            return Physics.Raycast(origin, direction, maxDistance);
        }
    }

    // 4. 게임 전용 API
    [ModAPICategory("Game.Player")]
    public static class GamePlayerAPI {
        [ModAPI("Player.GetHealth", "플레이어 체력 가져오기")]
        public static float GetPlayerHealth() {
            // 실제 플레이어 시스템과 연결
            return 100f; // 예시
        }

        [ModAPI("Player.SetHealth", "플레이어 체력 설정")]
        public static void SetPlayerHealth(float health) {
            // 실제 플레이어 시스템과 연결
        }
    }

    [ModAPICategory("Game.UI")]
    public static class GameUIAPI {
        [ModAPI("UI.ShowMessage", "메시지 표시")]
        public static void ShowMessage(string message) {
            // UI 시스템과 연결
            Debug.Log($"UI Message: {message}");
        }
    }

}
