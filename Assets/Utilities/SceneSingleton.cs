using UnityEngine;
using System.Collections;

public class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _Instance = null;
    private static bool _Valid = false;
    private static bool _WasValid = false;
    public static T Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<T>();

#if UNITY_EDITOR
                if (_Instance)
                {
                    SingletonUtility.SceneSingletonCount++;
#if VERBOSE
                    Debug.Log(typeof(T) + " SingletonUtility.SceneSingletonCount = " + SingletonUtility.SceneSingletonCount);
#endif
                }
#endif
            }

            if (_Instance == null)
            {
                _Instance = null;
                if (_WasValid) //only show this if it was created before
                {
                    // the object already destoryed
                    Debug.LogWarning("You should not use the " + typeof(T).Name + " after it already destory!");
                }
            }
            else
            {
                //is the instance valid
                _Valid = true;
                //has the instance ever been valid
                _WasValid = true;
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
#if UNITY_EDITOR
        if (_Instance)
        {
            SingletonUtility.SceneSingletonCount--;
#if VERBOSE
            Debug.Log(typeof(T) + " SingletonUtility.SceneSingletonCount = " + SingletonUtility.SceneSingletonCount);
#endif
        }
#endif

        _Instance = null;
        //instance is no longer valid
        _Valid = false;
    }
}
