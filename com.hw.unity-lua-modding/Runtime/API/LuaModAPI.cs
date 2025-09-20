using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using NLua;
using System;
using System.Linq;
using Modding.Utils;
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
        private readonly Dictionary<int, GameObject> _cacheUIObjects = new();
      
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
        public void SetActive(GameObject gameObject, bool isActive) {
            gameObject.SetActive(isActive);
        }

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

        public int GetInstanceID(GameObject obj) {
            return obj.GetInstanceID();
        }

        public void DestroyGameObject(GameObject obj) {
            if (obj != null) {
                GameObject.Destroy(obj);
            }
        }
            public void SetParent(Transform tr, Transform parentTr) {
                tr.SetParent(parentTr);
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

        public GameObject CreateUI(string relativePath, Transform parent = null, string name = null) {
            ModUIInfos uiInfos = _modContext.LoadUIInfo(relativePath);
            GameObject uiObject = CreateUIObject(uiInfos, 0, parent, name);
#if UNITY_EDITOR
            // Debug Code
            uiObject.AddComponent<MonoUIInfoDebuger>().uiInfos = uiInfos;
#endif
            return uiObject;
        }
        public GameObject GetUIGameObject(int instanceId) {
            return _cacheUIObjects[instanceId];
        }
        public GameObject[] GetAllUI() {
            return _cacheUIObjects.Select(kv => kv.Value).ToArray();
        }
        public int GetAllUICount() {
            return _cacheUIObjects.Count;
        }
        public void SetSprite(GameObject uiObject, string relativePath) {
            Image image = uiObject.GetComponent<Image>();
            if (image != null) {
                image.sprite = _modContext.LoadSprite(relativePath);
            }
        }
        public void SetRectPosition(GameObject uiObject, float x, float y) {
            RectTransform rectTransform = uiObject.GetComponent<RectTransform>();
            if (rectTransform != null) {
                rectTransform.anchoredPosition = new Vector2(x, y);
            }
        }
        public void SetText(GameObject uiObject, string text) {
            TextMeshProUGUI tm = uiObject.GetComponent<TextMeshProUGUI>();
            if (tm != null) {
                tm.text = text;
            }
        }
        public void SetButtonEvents(GameObject uiObject, string triggerType, string luaFunctionName, LuaTable parameters = null) {
            ModUIButtonEvent[] events = new ModUIButtonEvent[1];
            var bevent = new ModUIButtonEvent();
            bevent.triggerType = triggerType;
            bevent.luaFunctionName = luaFunctionName;
            bevent.parameters = null;
            if (parameters != null) {
                var paramList = new List<string>();
                // luatable to string[]
                foreach (var key in parameters.Keys) {
                    var value = parameters[key];
                    paramList.Add(value?.ToString() ?? "");    
                }
                bevent.parameters = paramList.ToArray();
            }
            events[0] = bevent;
            SetupButtonEvents(uiObject, events);
        }
        #endregion
        #region UI Private
        private GameObject CreateUIObject(ModUIInfos uiInfos, int id, Transform parent, string name) {
            ModUIInfo uiInfo = uiInfos.ui_infos.Where(uiInfo => uiInfo.id == id).FirstOrDefault();

            if (uiInfo == null) {
                ModDebug.Log($"not exists: {id}");
                return null;
            }
            string uiName = string.IsNullOrEmpty(name) ? uiInfo.name : name;
            GameObject uiObject = new GameObject(uiName);

            _cacheUIObjects[uiObject.GetInstanceID()] = uiObject;

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

            foreach (var childId in uiInfo.children) {
                CreateUIObject(uiInfos, childId, uiObject.transform, null);
            }

            return uiObject;
        }
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

            // Load Texture
            if (!string.IsNullOrEmpty(uiInfo.imageOption.imagePath)) {
                Sprite sprite = _modContext.LoadSprite(uiInfo.imageOption.imagePath);
                if (sprite != null) {
                    image.sprite = sprite;
                }
            }

            // Apply Image Option
            if (uiInfo.imageOption != null) {
                image.color = uiInfo.imageOption.ImageColor;
                image.type = uiInfo.imageOption.ImageType;
                image.preserveAspect = uiInfo.imageOption.preserveAspect;
                image.raycastTarget = uiInfo.imageOption.raycastTarget;
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
            // Set Event
            if (option.events != null && option.enabled) {
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
                entry.eventID = eventInfo.TriggerType;

                // Lua method call
                entry.callback.AddListener((data) => {
                    CallLuaFunction(eventInfo.luaFunctionName, eventInfo.parameters);
                });
                trigger.triggers.Add(entry);
            }
        }
        private void CallLuaFunction(string functionName, string[] parameters) {
            if (!_cacheLuaFunctions.TryGetValue(functionName, out LuaFunction func)) {
                func = _luaTable[functionName] as LuaFunction;
                _cacheLuaFunctions[functionName] = func;
            }
            // Call Lua function with appropriate number of parameters
            // 매개변수 개수에 따라 적절한 방식으로 Lua 함수 호출
            switch (parameters.Length) {
                case 0:
                func.Call();
                break;
                case 1:
                func.Call(parameters[0]);
                break;
                case 2:
                func.Call(parameters[0], parameters[1]);
                break;
                case 3:
                func.Call(parameters[0], parameters[1], parameters[2]);
                break;
                case 4:
                func.Call(parameters[0], parameters[1], parameters[2], parameters[3]);
                break;
                default:
                func.Call(parameters);
                break;
            }
            Debug.Log($"[ModUI] Calling Lua function: {functionName}");
        }
        #endregion

    }

}