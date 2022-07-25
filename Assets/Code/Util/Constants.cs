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
    }

    public static class Constants
    {
        public static GUIContent EditButton => EditorGUIUtility.IconContent("d_Preset.Context");
        public static GUIContent PlusButton => EditorGUIUtility.IconContent("d_Toolbar Plus");
        public static GUIContent MinusButton => EditorGUIUtility.IconContent("d_Toolbar Minus");
        public static GUIContent CheckMark => EditorGUIUtility.IconContent("d_FilterSelectedOnly");
        public static GUIContent XButton => EditorGUIUtility.IconContent("d_winbtn_win_close");
    }
}