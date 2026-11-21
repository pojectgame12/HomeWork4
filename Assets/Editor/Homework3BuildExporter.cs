#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Convenience build menus; Unity Build Support modules are still required.</summary>
public static class Homework3BuildExporter
{
    private static string[] Scenes()
    {
        string[] required = {
            "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Level02.unity",
            "Assets/Scenes/Victory.unity", "Assets/Scenes/GameOver.unity"
        };
        foreach (string s in required)
            if (!File.Exists(s)) throw new System.Exception("Generate scenes before building: " + s);
        return required;
    }

    [MenuItem("Homework 3/Export WebGL (for itch.io)")]
    public static void ExportWebGL()
    {
        string output = Path.GetFullPath("Builds/WebGL");
        Directory.CreateDirectory(output);
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL,BuildTarget.WebGL))
        {
            Debug.LogError("Please install WebGL Build Support in Unity Hub.");
            return;
        }
        PlayerSettings.companyName = "Homework3";
        PlayerSettings.productName = "Shadow Switch";
        var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = Scenes(), locationPathName = output,
            target=BuildTarget.WebGL, options=BuildOptions.None
        });
        Debug.Log("WebGL build result: " + result.summary.result + ". Output: " + output);
    }

    [MenuItem("Homework 3/Export Android APK")]
    public static void ExportAndroid()
    {
        string path = Path.GetFullPath("Builds/Android/ShadowSwitch.apk");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android))
        {
            Debug.LogError("Please install Android Build Support, SDK & NDK via Unity Hub.");
            return;
        }
        PlayerSettings.companyName = "Homework3";
        PlayerSettings.productName = "Shadow Switch";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,"com.homework3.shadowswitch");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        EditorUserBuildSettings.buildAppBundle = false;
        var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes=Scenes(), locationPathName=path,
            target=BuildTarget.Android, options=BuildOptions.None
        });
        Debug.Log("Android build result: " + result.summary.result + ". Output: " + path);
    }
}
#endif
