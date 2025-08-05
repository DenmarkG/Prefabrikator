using Prefabrikator.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator.Editor
{
    public abstract class ModifierEditor : UnityEditor.Editor
    {
        public void UpdateInspector(Modifier modifier, IShape target, Transform[] proxies)
        {
            EditorGUILayout.BeginVertical(new GUIStyle("Tooltip"), GUILayout.MaxWidth(Constants.MaxWidth - Constants.IndentSize), GUILayout.ExpandWidth(false));
            {
                OnInspectorUpdate(modifier, target, proxies);
            }
            EditorGUILayout.EndVertical();
        }

        protected abstract void OnInspectorUpdate(Modifier modifier, IShape target, Transform[] proxies);

        // #DG: TODO: Move to editor code
        public OnValueSetDelegate<T> CreateCommand<T>(Shared<T> field, IShape target) where T : struct
        {
            return (T current, T previous) => { target.CommandQueue.Enqueue(new GenericCommand<T>(field, previous, current)); };
        }
    }
}