using UnityEngine;
using System.Collections;

public static class GameObjectExtensions
{
    public static string Path(this GameObject gameobject)
    {
        string path = gameobject.name;
        var parent = gameobject.transform.parent;
        while(parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.transform.parent;
        }

        return path;
    }

    public static void SetLayersRecursivly(this GameObject gameobject, LayerMask layer)
    {
        gameobject.layer = layer;
        for (int i = 0; i < gameobject.transform.childCount; i++ )
        {
            SetLayersRecursivly(gameobject.transform.GetChild(i).gameObject, layer);
        }
    }

}
