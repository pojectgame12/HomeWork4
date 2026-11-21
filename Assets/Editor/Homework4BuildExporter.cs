#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Five-scene WebGL exporter for Homework 4. Build Support required.</summary>
public static class Homework4BuildExporter
{
    [MenuItem("Homework 4/Export WebGL (all 5 scenes)")]
    public static void ExportWebGL()
    {
        string[] scenePaths = {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Level02.unity",
            "Assets/Scenes/Victory.unity",
            "Assets/Scenes/GameOver.unity",
            "Assets/Scenes/PhysicsBonus3D.unity"
        };
        foreach (string path in scenePaths)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null) continue;
            Debug.LogError("Run Homework 4 > Upgrade Existing Project first. Missing: " + path);
            return;
        }
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
        {
            EditorUtility.DisplayDialog("WebGL not installed", "Install WebGL Build Support using Unity Hub.", "OK");
            return;
        }
        PlayerSettings.companyName = "Homework4";
        PlayerSettings.productName = "Shadow Switch";
        string outputFolder = Path.GetFullPath("Builds/WebGL");
        Directory.CreateDirectory(outputFolder);
        var options = new BuildPlayerOptions
        {
            scenes = scenePaths,
            locationPathName = outputFolder,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log("Homework 4 WebGL build: " + report.summary.result + " at " + outputFolder);
    }
}
#endif
