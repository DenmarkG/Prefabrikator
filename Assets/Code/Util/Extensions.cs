using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public static class Extensions
    {
        public const float LabelWidth = 50f;

        public static GUIStyle BoxedHeaderStyle
        {
            get
            {
                if (boxedHeaderStyle == null)
                {
                    boxedHeaderStyle = new GUIStyle("toolbar");
                    boxedHeaderStyle.fixedHeight = 0;
                    boxedHeaderStyle.fontSize = EditorStyles.label.fontSize;
                    int h = Mathf.CeilToInt(EditorGUIUtility.singleLineHeight);
                    int v = Mathf.CeilToInt(EditorGUIUtility.singleLineHeight * .3f);
                    boxedHeaderStyle.padding = new RectOffset(h, h, v, v);
                }

                return boxedHeaderStyle;
            }
        }
        private static GUIStyle boxedHeaderStyle = null;

        public static GUIStyle PopupStyle
        {
            get
            {
                if (popupStyle == null)
                {
                    popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                    popupStyle.imagePosition = ImagePosition.ImageOnly;
                }

                return popupStyle;
            }
        }
        private static GUIStyle popupStyle = null;

        public static bool DisplayCountField(ref int targetCount, string label = null)
        {
            bool needsRefresh = false;

            EditorGUILayout.BeginHorizontal(BoxedHeaderStyle);
            {
                EditorGUILayout.LabelField(label ?? "Count", GUILayout.Width(LabelWidth));

                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    if (targetCount > 0)
                    {
                        --targetCount;
                        needsRefresh = true;
                    }
                }

                float spacing = 30;
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(targetCount.ToString(), GUILayout.Width(spacing));
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("+", GUILayout.Width(30)))
                {
                    if (targetCount < int.MaxValue - 1)
                    {
                        ++targetCount;
                        needsRefresh = true;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            return needsRefresh;
        }

        public static bool DisplayRotationField(ref Quaternion rotation, string label = null)
        {
            bool needsRefresh = false;

            EditorGUILayout.BeginHorizontal(BoxedHeaderStyle);
            {
                Vector3 localEulerRotation = rotation.eulerAngles;
                localEulerRotation = EditorGUILayout.Vector3Field(label ?? "Rotation", localEulerRotation);
                if (localEulerRotation != rotation.eulerAngles)
                {
                    rotation = Quaternion.Euler(localEulerRotation);
                    needsRefresh = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            return needsRefresh;
        }

        public static bool DisplayScaleField(ref Vector3 scale, string label = null)
        {
            bool needsRefresh = false;

            EditorGUILayout.BeginHorizontal(BoxedHeaderStyle);
            {
                Vector3 localScale = scale;
                localScale = EditorGUILayout.Vector3Field(label ?? "Scale", localScale);
                if (localScale != scale)
                {
                    scale = localScale;
                    needsRefresh = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            return needsRefresh;
        }

        public static void Randomize(ref Vector3 vect, MinMax? xRange = null, MinMax? yRange = null, MinMax? zRange = null)
        {
            if (xRange != null)
            {
                vect.x = Random.Range(xRange.Value.Min, xRange.Value.Max);
            }

            if (yRange != null)
            {
                vect.y = Random.Range(yRange.Value.Min, yRange.Value.Max);
            }

            if (zRange != null)
            {
                vect.z = Random.Range(zRange.Value.Min, zRange.Value.Max);
            }
        }

        public static Vector3 Clamp(Vector3 vect, Vector3 min, Vector3 max)
        {
            vect.x = Mathf.Clamp(vect.x, min.x, max.x);
            vect.y = Mathf.Clamp(vect.y, min.y, max.y);
            vect.z = Mathf.Clamp(vect.z, min.z, max.z);

            return vect;
        }
    }
    /*EditorStyles.helpBox*/
}
