using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public static class Helpers
    {
        public static void Add(this GenericMenu menu, string itemName, UnityEditor.GenericMenu.MenuFunction func, bool disabled = false)
        {
            if (disabled)
            {
                menu.AddDisabledItem(new GUIContent(itemName), false);
            }
            else
            {
                menu.AddItem(new GUIContent(itemName), false, func);
            }
        }
    }
}