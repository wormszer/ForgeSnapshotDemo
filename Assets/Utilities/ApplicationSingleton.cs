using UnityEngine;
using System.Collections;

public class ApplicationSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _Instance;
    private static bool _IsDestroyed = false;
    private static bool _Valid = false;
#if UNITY_EDITOR
    private static bool _Created = false;  //used to stop the double print and ++ of the count, do to re-entrant Instance during initialization
#endif

    public static T Instance
    {
        get
        {
            if ((System.Object)_Instance == null && !_IsDestroyed)
            {
                _Instance = GameObject.FindObjectOfType<T>();
                if (_Instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    _Instance = go.AddComponent<T>(); //causes Re-entrant initialization
#if UNITY_EDITOR
                    _Created = true;
#endif
                    go.name = _Instance.GetType().Name;
                }
                if (Application.isPlaying)
                {
                    //only for use during the Runtime
                    //https://forum.unity3d.com/threads/hidden-gameobjects-in-unity-5-3-scene-hierarchy.376613/
                    UnityEngine.Object.DontDestroyOnLoad(_Instance.gameObject);
                }

#if UNITY_EDITOR
                if (_Created)
                {
                    SingletonUtility.ApplicationSingletonCount++;
                    Debug.Log(_Instance.gameObject.name + " SingletonUtility.ApplicationSingletonCount = " + SingletonUtility.ApplicationSingletonCount);
                }
#endif
            }

            if (_Instance == null)
            {
                // the object already destoryed
                Debug.LogWarning("You should not use the " + typeof(T).Name + " after it already destory!");
            }
            else
            {
                _Valid = true;
            }

            return _Instance;
        }
    }

    public static bool Valid
    {
        get { return _Valid; }
    }

    protected virtual void OnDestroy()
    {
        if (_Instance == this)
        {
            _IsDestroyed = true;
            _Instance = null;
            _Valid = false;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _IsDestroyed = false;
            }
#endif
        }
    }
}