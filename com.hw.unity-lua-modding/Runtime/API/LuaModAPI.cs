using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using NLua;
using System;
namespace Modding.API {
    /// <summary>
    /// Use Game API in Lua (Lua 모드에서 사용할 수 있는 게임 API)
    /// </summary>
    public partial class LuaModAPI {
        #region Cache
        private static GameObject _player;
        private static GameObject _mainCanvas;

        private Lua _luaState;
        private LuaTable _luaTable;
        private LuaModContext _modContext;

        private readonly Dictionary<string, LuaFunction> _cacheLuaFunctions = new();
        private readonly Dictionary<string, GameObject> _cacheUIObjects = new();
        #endregion
        public event Action OnDispose;

        #region LifeCycle


        public void Initialize(Lua lua, LuaTable luaTable, LuaModContext context) {
            _modContext = context;
            _luaState = lua;
            _luaTable = luaTable;
            OnDispose += OnDisposeFunc;
        }
        public void Dispose() {
            OnDispose?.Invoke();
        }
        private void OnDisposeFunc() {
            foreach (var func in _cacheLuaFunctions.Values) {
                func?.Dispose();
            }
        }

        #endregion
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
                GameObject.Destroy(obj);
            }
        }
        #endregion

        #region Player
        public GameObject GetPlayer() {
            return _player == null ? _player = GameObject.FindGameObjectWithTag("Player") : _player;
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

        #region UI
        public GameObject GetMainCanvas() {
            return _mainCanvas == null ? _mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas") : _mainCanvas;
        }

        public GameObject CreateUI(string relativePath, Transform parent) {
            ModUIInfo uiInfo = _modContext.LoadUIInfo(relativePath);
            return CreateUIObject(uiInfo, parent);
        }

        public GameObject CreateUIObject(ModUIInfo uiInfo, Transform parent) {
            GameObject uiObject = new GameObject(uiInfo.name);
            _cacheUIObjects[uiInfo.name] = uiObject;
            RectTransform rectTransform = uiObject.AddComponent<RectTransform>();
            SetupRectTransform(rectTransform, uiInfo, parent);

            // Image
            if (uiInfo.imageOption != null && uiInfo.imageOption.enabled) {
                SetupImage(uiObject, uiInfo);
            }

            // Text
            if (uiInfo.textOption != null && uiInfo.textOption.enabled) {
                SetupText(uiObject, uiInfo.textOption);
            }

            // Button
            if (uiInfo.buttonOption != null && uiInfo.buttonOption.enabled) {
                SetupButton(uiObject, uiInfo.buttonOption);
            }

            foreach (var info in uiInfo.children) {
                CreateUIObject(info, uiObject.transform);
            }

            return uiObject;
        }
        public GameObject GetUIGameObject(string name) {
            return _cacheUIObjects[name];
        }
        #endregion
        #region UI Private

        private void SetupRectTransform(RectTransform rectTransform, ModUIInfo uiInfo, Transform parent) {
            if (parent != null) {
                rectTransform.SetParent(parent, false);
            }

            // Anchor
            SetAnchor(rectTransform, uiInfo.anchor);

            // Transform
            rectTransform.anchoredPosition = uiInfo.Position;
            rectTransform.sizeDelta = uiInfo.Size;
            rectTransform.localRotation = Quaternion.Euler(uiInfo.Rotation);
            rectTransform.localScale = uiInfo.Scale;
        }

        private void SetAnchor(RectTransform rectTransform, ModUIAnchor anchor) {
            if (anchor.preset != "Custom") {
                SetAnchorPreset(rectTransform, anchor.Preset);
            } else {
                rectTransform.anchorMin = anchor.AnchorMin;
                rectTransform.anchorMax = anchor.AnchorMax;
            }

            rectTransform.pivot = anchor.Pivot;
            rectTransform.offsetMin = anchor.OffsetMin;
            rectTransform.offsetMax = anchor.OffsetMax;
        }
        private void SetAnchorPreset(RectTransform rectTransform, AnchorPresets preset) {
            switch (preset) {
                case AnchorPresets.TopLeft:
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                break;
                case AnchorPresets.TopCenter:
                rectTransform.anchorMin = new Vector2(0.5f, 1);
                rectTransform.anchorMax = new Vector2(0.5f, 1);
                break;
                case AnchorPresets.TopRight:
                rectTransform.anchorMin = new Vector2(1, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                break;
                case AnchorPresets.MiddleLeft:
                rectTransform.anchorMin = new Vector2(0, 0.5f);
                rectTransform.anchorMax = new Vector2(0, 0.5f);
                break;
                case AnchorPresets.MiddleCenter:
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                break;
                case AnchorPresets.MiddleRight:
                rectTransform.anchorMin = new Vector2(1, 0.5f);
                rectTransform.anchorMax = new Vector2(1, 0.5f);
                break;
                case AnchorPresets.BottomLeft:
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                break;
                case AnchorPresets.BottomCenter:
                rectTransform.anchorMin = new Vector2(0.5f, 0);
                rectTransform.anchorMax = new Vector2(0.5f, 0);
                break;
                case AnchorPresets.BottomRight:
                rectTransform.anchorMin = new Vector2(1, 0);
                rectTransform.anchorMax = new Vector2(1, 0);
                break;
                case AnchorPresets.StretchAll:
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                break;
            }
        }

        private void SetupImage(GameObject uiObject, ModUIInfo uiInfo) {
            Image image = uiObject.AddComponent<Image>();

            // 텍스처 로드
            if (!string.IsNullOrEmpty(uiInfo.imageOption.imagePath)) {
                Texture2D texture = _modContext.LoadTexture(uiInfo.imageOption.imagePath);
                if (texture != null) {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                    image.sprite = sprite;
                }

            }

            // Image 옵션 적용
            if (uiInfo.imageOption != null) {
                image.color = uiInfo.imageOption.color;
                image.type = uiInfo.imageOption.imageType;
                image.preserveAspect = uiInfo.imageOption.preserveAspect;
                image.raycastTarget = uiInfo.imageOption.raycastTarget;

                if (!string.IsNullOrEmpty(uiInfo.imageOption.materialPath)) {
                    // 머티리얼 로드 로직
                }
            }
        }

        private void SetupText(GameObject uiObject, ModUITextOption option) {
            TextMeshProUGUI text = uiObject.AddComponent<TextMeshProUGUI>();
            text.text = option.text;
            text.fontSize = option.fontSize;
            text.color = option.TextColor;
            text.alignment = option.Alignment;
            text.richText = option.richText;
            text.raycastTarget = option.raycastTarget;
            text.fontStyle = option.FontStyle;
            text.lineSpacing = option.lineSpacing;
            text.enableAutoSizing = option.resizeTextForBestFit;
            text.fontSizeMax = option.resizeTextMinSize;
            text.fontSizeMax = option.resizeTextMaxSize;
        }

        private void SetupButton(GameObject uiObject, ModUIButtonOption option) {
            // 이벤트 설정
            if (option.events != null) {
                SetupButtonEvents(uiObject, option.events);
            }
        }
        private void SetupButtonEvents(GameObject uiObject, ModUIButtonEvent[] events) {
            EventTrigger trigger = uiObject.GetComponent<EventTrigger>();
            if (trigger == null) {
                trigger = uiObject.AddComponent<EventTrigger>();
            }

            foreach (var eventInfo in events) {
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = eventInfo.triggerType;

                // Lua 함수 호출 설정
                entry.callback.AddListener((data) => {
                    // ModEngine을 통해 Lua 함수 호출
                    CallLuaFunction(eventInfo.luaFunctionName, eventInfo.parameters);
                });
                trigger.triggers.Add(entry);
            }
        }
        private void CallLuaFunction(string functionName, string[] parameters) {
            if (!_cacheLuaFunctions.TryGetValue(functionName, out LuaFunction func)) {
                func =_luaTable[functionName] as LuaFunction;
                _cacheLuaFunctions[functionName] = func;
            }
            func?.Call(parameters);
            Debug.Log($"[ModUI] Calling Lua function: {functionName}");
        }
        #endregion

    }

}