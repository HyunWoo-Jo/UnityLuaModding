# Changelog

## [Unreleased]

### Added

## [0.0.3] - 2025-09-18
### Added
- Added Attribute API Extension (Attribute를 이용한 확장 기능)
- Added individual scene loading capability (씬별 개별 로드 기능 추가)


## [0.0.2] - 2025-09-16
### Added
- EventBus System (이벤트 버스 시스템): Event system support for Lua mods
    - LuaEventAPI: API for Lua scripts to access C# event bus (Lua 스크립트에서 C# 이벤트 버스에 접근할 수 있는 API)
    - LuaEventWrapper: Wrapper class to use static methods as instance methods in Lua (Static 메서드를 Lua 인스턴스 방식으로 사용할 수 있게 해주는 래퍼 클래스)
    - Event subscription/unsubscription/publishing functionality (이벤트 구독/구독해제/발생 기능) (Subscribe, Unsubscribe, Publish)
- Added dependency-aware loading system (의존성을 고려한 로드 방식 추가)
- Added Samples (샘플 코드 추가) 

## [0.0.1] - 2025-09-15
### Added
- Initial Unity Lua Modding System (Unity Lua 모딩 시스템 초기 버전)
- Lua mod loading with NLua integration (NLua 통합을 통한 Lua 모드 로딩)
- ModEngine with hot reloading support (핫 리로딩 지원하는 ModEngine)
- Basic mod management and lifecycle (기본 모드 관리 및 생명주기)
- Debug logging system (디버그 로깅 시스템)
- simple API (간단한 API 기능)
