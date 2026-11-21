#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Exports Shadow Switch, Physics Bonus and Echo Shift as one WebGL site.</summary>
public static class Homework4AllGamesBuildExporter
{
    [MenuItem("Homework 4/Export WebGL (6 scenes incl. Echo Shift)")]
    public static void ExportAll()
    {
        string[] paths = {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Level02.unity",
            "Assets/Scenes/Victory.unity",
            "Assets/Scenes/GameOver.unity",
            "Assets/Scenes/PhysicsBonus3D.unity",
            "Assets/Scenes/EchoShift.unity"
        };
        foreach (string path in paths)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null) continue;
            Debug.LogError("Missing scene: " + path + ". Use Homework 4 > Create Echo Shift Level.");
            return;
        }
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
        {
            Debug.LogError("Install WebGL Build Support in Unity Hub first.");
            return;
        }
        PlayerSettings.productName = "Homework 4 - Shadow Switch and Echo Shift";
        string folder = Path.GetFullPath("Builds/WebGL_Homework4_AllGames");
        Directory.CreateDirectory(folder);
        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = paths,
            locationPathName = folder,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });
        Debug.Log("Homework 4 six-scene WebGL build: " + result.summary.result + " at " + folder);
    }
}
#endif
