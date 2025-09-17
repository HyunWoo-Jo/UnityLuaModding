# Unity Lua Modding
- [English](/README_eng.md)

Unity Lua Modding는 Unity용 Lua 기반 Modding 프레임 워크입니다.

---
## 목차
- [주요 기능](#feature)
- [설치 방법](#install)
- [시작](#start)
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
    private void DebugLog(){
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
