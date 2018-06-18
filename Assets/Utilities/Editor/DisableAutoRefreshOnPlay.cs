using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class DisableAutoRefreshOnPlay
{
    static DisableAutoRefreshOnPlay()
    {
#if UNITY_2017_2_OR_NEWER
        EditorApplication.playModeStateChanged += ChangePlaymodeCallback;
#else
        EditorApplication.playmodeStateChanged += ChangePlaymodeCallback;
#endif
    }

#if UNITY_2017_2_OR_NEWER
    private static void ChangePlaymodeCallback(PlayModeStateChange state)
#else
    private static void ChangePlaymodeCallback()
#endif
    {
        //This is only kicks off when you have exited play mode.
        if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
        {
            EditorPrefs.SetBool("kAutoRefresh", true);
            AssetDatabase.Refresh();
        }

        //Called just after play mode is entered
        if (EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPlaying)
        {
            EditorPrefs.SetBool("kAutoRefresh", false);
        }
    }
}
