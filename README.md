# Unity Lua Modding
Unity Lua Modding는 Unity용 Lua 기반 Modding 프레임 워크입니다.

---
## 목차
- [주요 기능](#feature)
- [설치 방법](#install)
- [시작](#start)
- [UI 생성](#createUI)
- [API](#api)
- [보안](#security)
- [샘플](#sample)
- [License](#license)
<a id = install></a>
---
<a id = feature></a>
## 주요 기능
- 의존성 관리: 모드 의존성 해결 및 로딩 순서 관리
- 핫 리로딩: 게임 재시작 없이 런타임에서 모드 로딩/언로딩
- 이벤트 시스템: 게임 상태 이벤트 처리

---
<a id = install></a>
## 설치 방법
설치를 위해서는 [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity/tree/master)에서 NLua 설치가 필요합니다.

<img width="376" height="106" alt="image" src="https://github.com/user-attachments/assets/cf150651-7b6b-453c-add4-f6733633f0bc" />

### 방법 1: Package Manager UI 사용
1. Unity를 열고 `Window` → `Package Manager`로 이동
2. 좌측 상단의 `+` 버튼 클릭
3. `Add package from git URL...` 선택
4. 다음 URL을 붙여넣기:
```
   https://github.com/HyunWoo-Jo/UnityLuaModding.git?path=/com.hw.unity-lua-modding
```
5. `Add` 클릭
   
### 방법 2: manifest.json 직접 편집
1. 프로젝트의 Packages/manifest.json 파일 열기
2. dependencies 섹션에 다음 라인 추가:
```json
  {
    "dependencies": {
      "com.hw.unity-lua-modding": "https://github.com/HyunWoo-Jo/UnityLuaModding.git?path=/com.hw.unity-lua-modding"
    }
  }
```
3. 파일 저장 후 Unity에서 자동으로 패키지 설치

---
<a id = start></a>
## 시작
### 1. ModEngine 설정
씬의 **GameObject**에 **`ModEngine`** 컴포넌트를 추가합니다. `ModEngine`은 `Singleton`으로 관리 됩니다.

### 2. 모드 만들기
#### 모드 폴더 구조를 생성합니다
```
Application.persistentDataPath (C:\Users\{UserName}\AppData\LocalLow\{CompanyName}\{ProductName}\
└─ Mods\
 └── {ModName}\
    ├── mod.json
    └── main.lua
```

#### mod.json
```json
{
  "name": "ModName",
  "version": "1.0.0",
  "description": "설명",
  "author": "제작자",
  "requiredMods": []
  "targetScenes": []
}
```

#### 모드 의존성이 존재할 경우
```json
"requiredMods": [
  {
    "name": "CoreLibrary",
    "version": "1.0.0",
    "operator": ">="
  }
]
```
**연산자**
- `>=`: 이상 (기본값)
- `>`: 초과
- `<=`: 이하
- `<`: 미만
- `==` 또는 `=`: 정확한 버전
- `!=`: 같지 않음
- `~>`: 호환 버전 (같은 major.minor)

**시스템이 자동으로 처리하는 작업**
- 의존성 순서 해결
- 의존성을 먼저 로드
- 의존성이 누락된 경우 로딩 방지
- 버전 호환성 처리

#### 목표로하는 Scene이 존재할경우
없을 경우 모든 씬에 적용
```json
"targetScenes": ["TestScene"]

```

#### main.lua
```lua
local ModName = {}

function ModName.Initialize(context)
  -- 컨텍스트 저장
  ModName.context = context
  print("MyFirstMod이 성공적으로 초기화되었습니다!")
  return true
end

function ModName.Update(deltaTime)
  -- 매 프레임마다 호출됩니다
end

function ModName.Shutdown()
   -- 모드가 언로드될 때 정리 작업
end
function ModName.OnPause()
   -- 게임이 정지될때 호출
end

function ModName.OnResume()
   -- 게임이 다시 작동할때 호출
end

function ModName.SceneChanged(sceneName)
   -- 씬이 변경될때 호출출
end
--테이블 반환 중요
return ModName
```
---
<a id = createUI></a>
## UI 생성
### json Style
json을 이용해서 UI 생성 Style을 정의 할 수 있습니다.
```json
// 기본 정의
{
"ui_infos": [ 
{
  "id": 0, // 기본 id는 0으로 정의 (0 == 최상위)
  "name": "SimpleButton",
  "position": [0, 0],
  "size": [150, 150],
  "rotation": [0, 0, 0],
  "scale": [1, 1, 1],
  "anchor": {
    "preset": "MiddleCenter",
    "pivot": [0.5, 0.5],
    "offsetMin": [0, 0],
    "offsetMax": [0, 0]
  },
  "imageOption": {
    "enabled": true,
    "color": [0.2, 0.7, 1.0, 1.0],
    "imageType": "Simple",
    "preserveAspect": false,
    "raycastTarget": true,
    "imagePath": "UI/slot.png",
    "materialPath": ""
  },
  "buttonOption": {
    "enabled": true,
    "interactable": true,
    "events": [
    {
      "triggerType": "PointerClick",
      "luaFunctionName": "OnButtonClick",
      "parameters": ["button1", "clicked"]
    }
    ]
  },
  "children": []
}
]

// 계층 구조로 생성해야 될 경우
{
"ui_infos": [ 
{
  "id": 0,
   ...
  "children": [1] 
},
{
  "id": 1,
  ...
}
]
}
```
```lua
// lua 코드로도 계층 구조를 정의 가능
ModAPI:CreateUI("UI/inventory_parent_ui.json", mainCanvas.transform)

```
---
<a id = api></a>
## API
### LuaModContext
폴더 리소스의 접근을 제공합니다.
```lua
function MyMod.Initialize(context)
    -- 파일 작업 (모드 폴더로 제한됨)
    local config = context:LoadTextFile("config.txt")
    local icon = context:LoadTexture("icon.png")
    return true
end
```
### 기본 API
- **ModAPI: 게임별 모딩 기능**
- **Unity: Unity 코드 접근**
- **Event: 모드 이벤트 시스템**
```
-- API 사용 예시

ModAPI:GetPlayer()

Unity:RandomInt(0,10)

Event:Subscribe("TestEvent", func(data)
  print("이벤트 발생")
end)
```

### API 확장
- API 코드들은 `partial`로 작성이 되어있으며 다음과 같이 확장이 가능합니다.
  - **`partial` 확장**
  - **`DI` + `Interface` 확장**
  - **`Attribute` 확장**
```c#
// Ex  Extension.cs
// Assembly Definition Reference을 활용 같은 Assembly영역에 정의
namespace Modding.API {
  public partial class LuaModAPI {
    public void DebugLog(){
    ...
    }
  }
}
```
```c#
// 외부 함수를 이용할 경우 DI + Interface 방식을 활용
// Assembly Definition Reference을 활용 같은 Assembly영역에 정의
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
// 다른 어셈블리 영역
namespace Other
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
```
```c#
// Attribute 확장
[ModAPICategory("Unity.Core")]
public static class UnityGameObjectAPI {
   [ModAPI("GameObject.Create", "새 게임오브젝트 생성")]
   public static GameObject CreateGameObject(string name) {
       return new GameObject(name);
   }
}
// lua
CallAPI("Unity.Core.GameObject.Create", "test attribute")

```
## Lua에서 사용 가능한 API 목록

### **ModAPI** - 게임 객체 및 UI 제어 API

#### GameObject 관련
- `SetActive(GameObject gameObject, bool isActive)` - GameObject의 활성 상태 조절
- `FindGameObject(string name)` - 이름으로 GameObject 찾기
- `FindGameObjectWithTag(string tag)` - 태그로 GameObject 찾기
- `FindGameObjectsWithTag(string tag)` - 태그로 GameObject 배열 찾기
- `CreateGameObject(string name)` - 새로운 GameObject 생성
- `Instantiate(GameObject obj)` - GameObject 복제
- `GetInstanceID(GameObject obj)` - GameObject의 인스턴스 ID 반환
- `DestroyGameObject(GameObject obj)` - GameObject 삭제
- `SetParent(Transform tr, Transform parentTr)` - 부모 Transform 설정

#### Player 관련
- `GetPlayer()` - Player GameObject 반환
- `GetPlayerPosition()` - Player 위치(Vector3) 반환
- `SetPlayerPosition(float x, float y, float z)` - Player 위치 설정

#### UI 관련
- `GetMainCanvas()` - MainCanvas GameObject 반환
- `CreateUI(string relativePath, Transform parent = null, string name = null)` - UI 생성
- `GetUIGameObject(int instanceId)` - 인스턴스 ID로 UI GameObject 반환
- `GetAllUI()` - 모든 UI GameObject 배열 반환
- `GetAllUICount()` - UI 개수 반환
- `SetSprite(GameObject uiObject, string relativePath)` - UI 이미지 스프라이트 설정
- `SetRectPosition(GameObject uiObject, float x, float y)` - UI RectTransform 위치 설정
- `SetText(GameObject uiObject, string text)` - UI 텍스트 설정
- `SetButtonEvents(GameObject uiObject, string triggerType, string luaFunctionName, LuaTable parameters = null)` - 버튼 이벤트 설정

---

### **Unity** - Unity 엔진 API 래퍼

#### Time 관련
- `GetDeltaTime()` - 프레임 간 시간 차이(deltaTime) 반환
- `GetTime()` - 게임 시작 이후 경과 시간 반환
- `GetTimeScale()` - 시간 스케일 값 반환

#### Input 관련
- `GetKey(string keyName)` - 키가 눌려있는지 확인
- `GetKeyDown(string keyName)` - 키가 눌렸는지 확인
- `GetKeyUp(string keyName)` - 키가 떼어졌는지 확인
- `GetMouseButton(int button)` - 마우스 버튼 상태 확인
- `GetMousePosition()` - 마우스 위치(Vector3) 반환

#### Vector3 관련
- `CreateVector3(float x, float y, float z)` - 새로운 Vector3 생성
- `Distance(Vector3 pos1, Vector3 pos2)` - 두 점 사이의 거리 계산
- `GetType(object obj)` - 객체 타입 이름 반환
- `Normalize(Vector3 vector)` - 벡터 정규화

#### Physics 관련
- `AddForce(GameObject obj, Vector3 pos)` - GameObject에 힘 적용
- `Raycast(Vector3 origin, Vector3 direction, float maxDistance = 100f)` - 레이캐스트 실행

#### Utils 관련
- `Random(float min = 0f, float max = 1f)` - 랜덤 실수 반환
- `RandomInt(int min = 0, int max = 10)` - 랜덤 정수 반환
- `Sin(float value)` - 사인 값 계산
- `Cos(float value)` - 코사인 값 계산
- `Sqrt(float value)` - 제곱근 계산
- `Abs(float value)` - 절대값 반환
- `Clamp(float value, float min, float max)` - 값을 범위 내로 제한

---

### **Event** - 이벤트 시스템 API (LuaEventWrapper)

- `Subscribe(string eventName, LuaFunction handler)` - 이벤트 구독
- `Unsubscribe(string eventName, LuaFunction handler)` - 이벤트 구독 해제
- `Publish(string eventName, object data = null)` - 이벤트 발생
- `ClearEventHandlers(string eventName)` - 특정 이벤트의 모든 핸들러 제거

---

### **LuaModContext** - 모드 컨텍스트 API

- `LoadTextFile(string relativePath)` - 텍스트 파일 로드
- `LoadSprite(string relativePath)` - 스프라이트 이미지 로드
- `LoadUIInfo(string relativePath)` - UI 정보 JSON 파일 로드
- `FindGameObject(string name)` - GameObject 찾기

---

### **전역 함수** (LuaModInstance에서 등록)

- `CallAPI(string apiName, params object[] args)` - 확장 API 호출
- `print(string message)` - 콘솔에 메시지 출력
- `log(string level, string message)` - 레벨별 로그 출력 (info/warning/error)
- `LoadLua(string relativePath)` - 다른 Lua 스크립트 파일 로드

---

## 사용 예시

```lua
-- Lua 모드 예시
local MyMod = {}

function MyMod:Initialize(context)
    -- GameObject 찾기 및 제어
    local player = ModAPI:GetPlayer()
    ModAPI:SetPlayerPosition(0, 10, 0)
    
    -- UI 생성
    local ui = ModAPI:CreateUI("ui/myui.json", ModAPI:GetMainCanvas())
    ModAPI:SetText(ui, "Hello World!")
    
    -- 이벤트 구독
    Event:Subscribe("PlayerDamaged", function(data)
        print("Player took damage: " .. tostring(data))
    end)
    
    -- Unity API 사용
    local deltaTime = Unity:GetDeltaTime()
    local mousePos = Unity:GetMousePosition()
    
    return true
end

function MyMod:Update(deltaTime)
    -- 키 입력 체크
    if Unity:GetKeyDown("space") then
        Event:Publish("PlayerJumped", {height = 5.0})
    end
end

return MyMod
```

이 API들을 통해 Lua 스크립트에서 Unity 게임 엔진의 다양한 기능을 제어할 수 있습니다.
---
<a id = security></a>
## 보안
현재 버전에서는 기본 적인 수준의 보안만 제공합니다.
- `io`: 파일 시스템 접근 차단
- `os`: 운영체제 상호 작용 차단

---
<a id = sample></a>
## 샘플
샘플의 경우 `Application.persistentDataPath`로 각 sample에 정의된 폴더 `Mods`를 이동하여 사용이 가능합니다.

---
<a id = license></a>
## License
This library is MIT License.
