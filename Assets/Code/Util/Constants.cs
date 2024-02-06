using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    [System.Flags]
    public enum EditMode : int
    {
        None = 0,
        Center = 0x1,
        Size = 0x2,
        Angle = 0x4,
        Position = 0x8,

        OffsetX = 0x10,
        OffsetY = 0x20,
        OffsetZ = 0x40,

        RadiusII = 0x80,
    }

    public static class Constants
    {
        public static GUIContent EditButton => EditorGUIUtility.IconContent("d_Preset.Context", "|Edit");
        public static GUIContent PlusButton => EditorGUIUtility.IconContent("d_Toolbar Plus", "|Add");
        public static GUIContent MinusButton => EditorGUIUtility.IconContent("d_Toolbar Minus", "|Remove");
        public static GUIContent CheckMark => EditorGUIUtility.IconContent("d_FilterSelectedOnly", "|Apply");
        public static GUIContent XButton => EditorGUIUtility.IconContent("d_winbtn_win_close", "|Cancel");
        public static GUIContent SaveButton => EditorGUIUtility.IconContent("SaveActive", "|Save and Close");
        public static GUIContent UndoButton => EditorGUIUtility.IconContent("oi--action-undo", "|Undo");
        public static GUIContent UndoDisabledButton => EditorGUIUtility.IconContent("oi--action-undo-disabled", "|Undo Unavailable");
        public static GUIContent RedoButton => EditorGUIUtility.IconContent("oi--action-redo", "|Redo");
        public static GUIContent RedoDisabledButton => EditorGUIUtility.IconContent("oi--action-redo-disabled", "|Redo Unvailable");

        public const float MaxWidth = 500;
        public const float MaxHeght = 750;
        public const int ToolbarButtonWidth = 75;
        public const int DefaultButtonWidth = 25;
        public const float LabelWidth = 60f;
        public const int IndentSize = 20;
        public const int SmallButtonWidth = 30;
    }
}