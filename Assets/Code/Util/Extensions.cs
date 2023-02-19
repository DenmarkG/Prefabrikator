using UnityEngine;
using UnityEditor;
using RNG = UnityEngine.Random;

namespace Prefabrikator
{
    public static class Extensions
    {
        public const float LabelWidth = 60f;
        public const int IndentSize = 20;

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

        public static GUIStyle LogStyle
        {
            get
            {
                if (_logStyle == null)
                {
                    _logStyle = new GUIStyle("LogStyle");
                }

                return _logStyle;
            }
        }
        private static GUIStyle _logStyle = null;

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


        public static Quaternion GetRandomRotation()
        {
            float max = 360f;
            float min = -360;
            float x = RNG.Range(min, max);
            float y = RNG.Range(min, max);
            float z = RNG.Range(min, max);

            return Quaternion.Euler(new Vector3(x, y, z));
        }

        public static Vector3 GetRandomPointInBounds(this Bounds bounds)
        {
            Vector3 min = bounds.center - bounds.extents;
            Vector3 max = bounds.center + bounds.extents;

            float x = RNG.Range(min.x, max.x);
            float y = RNG.Range(min.y, max.y);
            //float y = 0f;
            float z = RNG.Range(min.z, max.z);

            return new Vector3(x, y, z);
        }

        public static Vector3 RandomInsideSphere(float maxRadius)
        {
            return RNG.insideUnitSphere * Random.Range(0f, maxRadius);
        }

        public static Vector3 Clamp(Vector3 vect, Vector3 min, Vector3 max)
        {
            vect.x = Mathf.Clamp(vect.x, min.x, max.x);
            vect.y = Mathf.Clamp(vect.y, min.y, max.y);
            vect.z = Mathf.Clamp(vect.z, min.z, max.z);

            return vect;
        }

        /// <summary>
        /// Lerps from a to b based on t, where t : [-1, 1]
        /// </summary>
        public static float BiUnitLerp(float min, float max, float t)
        {
            float range = max - min;

            return (range * ((t + 1) / 2f)) + min;
        }

        /// <summary>
        /// Lerps from a to b based on t, where t : [-1, 1]
        /// </summary>
        public static Vector3 BiUnitLerp(Vector3 min, Vector3 max, Vector3 unitVect)
        {
            unitVect.x = BiUnitLerp(min.x, max.x, unitVect.x);
            unitVect.y = BiUnitLerp(min.y, max.y, unitVect.y);
            unitVect.z = BiUnitLerp(min.z, max.z, unitVect.z);

            return unitVect;
        }


        public static float Normalize(this float e, float min, float max)
        {
            float denominator = max - min;
            if (denominator != 0f)
            {
                return (e - min) / (denominator);
            }

            Debug.LogError("Attempt to divide by zero");
            return 0;
        }
    }
}
