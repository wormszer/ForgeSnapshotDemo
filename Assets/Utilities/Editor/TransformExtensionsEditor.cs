using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;

static public class TransformExtensionsEditor
{
#if UNITY_EDITOR
    public static void UndoDestroyChildrenImmediate(this Transform t)
    {
        bool isPlaying = Application.isPlaying;

        while (t.childCount != 0)
        {
            Transform child = t.GetChild(0);
            Undo.DestroyObjectImmediate(child.gameObject);
        }
    }

    /// <summary>
    /// Find childs in hierarchy by name. Not supported find path xxx/xxx/xxx
    /// </summary>
    /// <param name="t"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Transform[] FindChildsInHierarchyEditor(this Transform t, string name, bool matchAll = true)
    {
        if (t == null)
        {
            return null;
        }
        var childs = t.gameObject.GetComponentsInChildren<Transform>(true);

        var childList = from child in childs
                        where matchAll ? child.name.Equals(name) : child.name.Contains(name)
                        select child;

        if (childList != null && childList.Any())
        {
            return childList.ToArray();
        }
        return null;

    }
#endif
}
