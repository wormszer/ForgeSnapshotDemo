using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Linq;
#endif

public struct TransformData
{
    public Vector3 Position;
    public Vector3 Scale;
    public Quaternion Rotation;
}


static public class TransformExtensions
{
    public static TransformData GetTransformData(this Transform transform)
    {
        var tdata = new TransformData();
        tdata.Position = transform.position;
        tdata.Rotation = transform.rotation;
        tdata.Scale = transform.localScale;

        return tdata;
    }

    public static void SetTransformData(this Transform transform, TransformData tdata)
    {
        transform.position = tdata.Position;
        transform.rotation = tdata.Rotation;
        transform.localScale = tdata.Scale;
    }

    public static TransformData GetLocalTransformData(this Transform transform)
    {
        var tdata = new TransformData();
        tdata.Position = transform.localPosition;
        tdata.Rotation = transform.localRotation;
        tdata.Scale = transform.localScale;

        return tdata;
    }

    public static void SetLocalTransformData(this Transform transform, TransformData tdata)
    {
        transform.localPosition = tdata.Position;
        transform.localRotation = tdata.Rotation;
        transform.localScale = tdata.Scale;
    }

    public static void Reset(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public static void Reset(this RectTransform rectTransform)
    {
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.anchorMax = Vector2.one * 0.5f;
        rectTransform.anchorMin = Vector2.one * 0.5f;
        rectTransform.pivot = Vector2.one * 0.5f;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
    }

    public static void DestroyChildren(this Transform t)
    {
        bool isPlaying = Application.isPlaying;

        while (t.childCount != 0)
        {
            Transform child = t.GetChild(0);

            if (isPlaying)
            {
                child.SetParent(null, false);
                UnityEngine.Object.Destroy(child.gameObject);
            }
            else UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }
    
    public static bool IsParentOf(this Transform t, Transform child)
    {
        while (child != null)
        {
            if (child.parent == t)
            {
                return true;
            }
            else
            {
                child = child.parent;
            }
        }
        return false;
    }

    /// <summary>
    /// Find frist child in hierarchy by name. Not supported find path xxx/xxx/xxx
    /// </summary>
    /// <param name="t"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Transform FindChildInHierarchy(this Transform t, string name)
    {
        if (t == null)
        {
            return null;
        }
        foreach (var child in t.gameObject.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Equals(name))
            {
                return child;
            }
        }
        return null;
    }

    public static Transform FindChildInHierarchy(this Transform t, string name, System.Func<Transform, bool> check)
    {
        if (t == null)
        {
            return null;
        }
        foreach (var child in t.gameObject.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Equals(name))
            {
                if (check(child))
                {
                    return child;
                }
            }
        }
        return null;
    }

    public static void SortChildInHierarchy(this Transform t, Comparison<Transform> comparison)
    {
        if (t == null)
        {
            return;
        }
        List<Transform> childrens = new List<Transform>();
        for(int i = 0; i < t.childCount; i++)
        {
            var child = t.GetChild(i);
            childrens.Add(child);
        }
        childrens.Sort(comparison);
        for (int i = 0; i < childrens.Count; i++)
        {
            childrens[i].SetSiblingIndex(i);
        }
    }
}
