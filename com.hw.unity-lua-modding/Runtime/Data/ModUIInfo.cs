using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Presets;

namespace Modding {


    [Serializable]
    public partial class ModUIInfo {
        public string name = "ModUIElement";
        public float[] position = new float[2] { 0, 0 };
        public float[] size = new float[2] { 100, 100 };
        public float[] rotation = new float[3] { 0, 0, 0 };
        public float[] scale = new float[3] { 1, 1, 1 };

        // Anchor
        public ModUIAnchor anchor = new ModUIAnchor();

        // ComponentOptions
        public ModUIImageOption imageOption;
        public ModUIButtonOption buttonOption;
        public ModUITextOption textOption;

        // ui children 
        public ModUIInfo[] children;

        public Vector2 Position => new Vector2(position[0], position[1]);
        public Vector2 Size => new Vector2(size[0], size[1]);
        public Vector3 Rotation => new Vector3(rotation[0], rotation[1], rotation[2]);
        public Vector3 Scale => new Vector3(scale[0], scale[1], scale[2]);
    }

    [Serializable]
    public partial class ModUIAnchor {
        public string preset = AnchorPresets.MiddleCenter.ToString();
        public float[] anchorMin = new float[2] { 0.5f, 0.5f};
        public float[] anchorMax = new float[2] { 0.5f, 0.5f };
        public float[] pivot = new float[2] { 0.5f, 0.5f };
        public float[] offsetMin = new float[2] { 0, 0 }; // left, bottom
        public float[] offsetMax = new float[2] { 0, 0 }; // right, top
        public AnchorPresets Preset => System.Enum.Parse<AnchorPresets>(preset);
        public Vector2 AnchorMin => new Vector2(anchorMin[0], anchorMin[1]);
        public Vector2 AnchorMax => new Vector2(anchorMax[0], anchorMax[1]);
        public Vector2 Pivot => new Vector2(pivot[0], pivot[1]);
        public Vector2 OffsetMin => new Vector2(offsetMin[0], offsetMin[1]);
        public Vector2 OffsetMax => new Vector2(offsetMax[0], offsetMax[1]);
    }

    public enum AnchorPresets {
        TopLeft, TopCenter, TopRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        BottomLeft, BottomCenter, BottomRight,
        StretchTop, StretchMiddle, StretchBottom,
        StretchLeft, StretchCenter, StretchRight,
        StretchAll,
        Custom
    }

    [Serializable]
    public partial class ModUIImageOption {
        public bool enabled = false;
        public Color color = Color.white;
        public Image.Type imageType = Image.Type.Simple;
        public bool preserveAspect = false;
        public bool raycastTarget = true;
        public string imagePath;
        public Material material;
        public string materialPath;
    }

    [Serializable]
    public class ModUIButtonOption {
        public bool enabled = false;
        public bool interactable = true;

        public ModUIButtonEvent[] events;
    }

    [Serializable]
    public class ModUIButtonEvent {
        public EventTriggerType triggerType = EventTriggerType.PointerClick;
        public string luaFunctionName;
        public string[] parameters;
    }

    [Serializable]
    public class ModUITextOption {
        public bool enabled = false;
        public string text = "New Text";
        public int fontSize = 14;
        public float[] color = new float[3] { 0, 0, 0 };
        public string alignment = TextAlignmentOptions.Midline.ToString();
        public bool richText = true;
        public bool raycastTarget = true;
        public string fontStyle = FontStyles.Normal.ToString();
        public float lineSpacing = 0;

        // Auto size
        public bool resizeTextForBestFit = false;
        public int resizeTextMinSize = 10;
        public int resizeTextMaxSize = 40;


        public Color TextColor => new Color(color[0], color[1], color[2]);
        public TextAlignmentOptions Alignment => System.Enum.Parse<TextAlignmentOptions>(alignment);
        public FontStyles FontStyle => System.Enum.Parse<FontStyles>(fontStyle);
    }
}