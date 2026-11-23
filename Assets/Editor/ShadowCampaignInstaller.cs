#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Safe one-click installer: COPIES the original Level02 scene three times, adds story
/// components to the copies only, and appends build settings without deleting other games.
/// </summary>
public static class ShadowCampaignInstaller
{
    private const string Scenes = "Assets/Scenes/";
    private const string Source = Scenes + "Level02.unity";

    [MenuItem("Shadow Switch/1 - Install Campaign (safe)")]
    public static void Install()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Exit Play mode before installing the campaign.");
            return;
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Source) == null ||
            AssetDatabase.LoadAssetAtPath<LevelSettings>("Assets/Data/LevelSettings.asset") == null)
        {
            EditorUtility.DisplayDialog("Shadow Switch", "Missing Level02 scene or LevelSettings.asset. Check the Homework 4 project.", "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        int newScenes = 0;
        for (int i = 1; i <= 3; i++)
        {
            string target = Scenes + "ShadowStage" + i + ".unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(target) != null)
            {
                Debug.Log("Preserved existing campaign stage: " + target);
                continue;
            }
            if (!AssetDatabase.CopyAsset(Source, target))
                throw new Exception("Failed to copy " + Source + " -> " + target);
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(target, OpenSceneMode.Single);
            BuildStage(i);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            newScenes++;
        }
        newScenes += MenuScene("ShadowCampaignHub", typeof(ShadowCampaignHubUI)) ? 1 : 0;
        newScenes += MenuScene("ShadowCampaignResult", typeof(ShadowCampaignResultUI)) ? 1 : 0;
        newScenes += MenuScene("ShadowCampaignJournal", typeof(ShadowCampaignJournalUI)) ? 1 : 0;
        newScenes += MenuScene("ShadowCampaignEnding", typeof(ShadowCampaignEndingUI)) ? 1 : 0;

        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string[] names = { "MainMenu", "Level02", "Victory", "GameOver", "PhysicsBonus3D", "EchoShift",
            "ShadowCampaignHub", "ShadowStage1", "ShadowStage2", "ShadowStage3",
            "ShadowCampaignResult", "ShadowCampaignJournal", "ShadowCampaignEnding" };
        foreach (string name in names)
        {
            string path = Scenes + name + ".unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;
            bool exists = false;
            foreach (EditorBuildSettingsScene entry in scenes)
                if (entry.path == path) { exists = true; break; }
            if (!exists) scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(Scenes + "ShadowCampaignHub.unity", OpenSceneMode.Single);
        Debug.Log("Shadow Switch campaign installed. New scenes: " + newScenes + ". Existing scenes preserved. Press Play.");
        EditorUtility.DisplayDialog("Shadow Switch Campaign",
            "Campaign installation complete (" + newScenes + " new scenes). Original Level02, Echo Shift and physics bonus have NOT been overwritten.\n\nThe Campaign Hub is open. Press Play and test.", "OK");
    }

    private static bool MenuScene(string name, Type component)
    {
        string path = Scenes + name + ".unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
        {
            Debug.Log("Preserved existing scene: " + path);
            return false;
        }
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.035f, .065f, .13f);
        cameraObject.transform.position = new Vector3(0, 0, -10f);
        cameraObject.AddComponent<AudioListener>();
        GameObject ui = new GameObject(name + "_UI");
        ui.AddComponent(component);
        EditorSceneManager.SaveScene(scene, path);
        return true;
    }

    private static void BuildStage(int stage)
    {
        GameObject stageSettings = new GameObject("ShadowCampaignRuntime");
        ShadowCampaignRuntime runtime = stageSettings.AddComponent<ShadowCampaignRuntime>();
        runtime.stage = stage;
        new GameObject("CampaignSoundFeedback").AddComponent<ShadowFeedback>();

        // Accessible optional lore in each stage; stage two's record enables the safe ending.
        Vector2 lorePosition = stage == 1 ? new Vector2(-7.60f, -2.0f) :
                               stage == 2 ? new Vector2(4.12f, -2.0f) : new Vector2(6.2f, -2.0f);
        GameObject lore = new GameObject("Lore_Terminal_" + stage);
        lore.transform.position = lorePosition;
        lore.transform.localScale = Vector3.one * .65f;
        SpriteRenderer visual = lore.AddComponent<SpriteRenderer>();
        visual.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Obelisk.png");
        visual.sortingOrder = 18;
        CircleCollider2D trigger = lore.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 1.25f;
        ShadowLoreTerminal terminal = lore.AddComponent<ShadowLoreTerminal>();
        terminal.stage = stage;

        // Stage two/three require two crystals: the original starter pickup plus one
        // accessible on the far shore. These copies do NOT modify the original Level02.
        if (stage >= 2)
        {
            GameObject crystalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Crystal.prefab");
            GameObject crystal = crystalPrefab != null ?
                (GameObject)PrefabUtility.InstantiatePrefab(crystalPrefab) : new GameObject("Crystal_Extra");
            crystal.name = "Campaign_SecondCrystal";
            crystal.transform.position = new Vector3(3.30f, -2.15f, 0f);
            if (crystalPrefab == null)
            {
                SpriteRenderer art = crystal.AddComponent<SpriteRenderer>();
                art.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Crystal.png");
                art.sortingOrder = 16;
                crystal.transform.localScale = Vector3.one * .5f;
                CircleCollider2D collide = crystal.AddComponent<CircleCollider2D>();
                collide.radius = 1f;
                collide.isTrigger = true;
                crystal.AddComponent<CrystalPickup>();
            }
        }

        if (stage == 1 || stage == 3)
        {
            GameObject bonus = new GameObject("Optional_SecretCrystal");
            bonus.transform.position = new Vector3(stage == 1 ? -4.4f : 7.05f, -1.95f, 0f);
            bonus.transform.localScale = Vector3.one * .33f;
            SpriteRenderer bonusArt = bonus.AddComponent<SpriteRenderer>();
            bonusArt.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Crystal.png");
            bonusArt.color = new Color(.86f, .49f, 1f);
            bonusArt.sortingOrder = 19;
            CircleCollider2D bonusTrigger = bonus.AddComponent<CircleCollider2D>();
            bonusTrigger.radius = 1.6f;
            bonusTrigger.isTrigger = true;
            ShadowBonusCrystal collector = bonus.AddComponent<ShadowBonusCrystal>();
            collector.stage = stage;
        }

        if (stage == 3)
        {
            // The third stage adds a timed obstacle at the center of the original bridge.
            GameObject hazard = new GameObject("GreyWarden_MovingSentinel");
            hazard.transform.position = new Vector3(-.25f, -2.57f, 0f);
            hazard.transform.localScale = Vector3.one * .52f;
            SpriteRenderer art = hazard.AddComponent<SpriteRenderer>();
            art.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Crystal.png");
            art.color = new Color(1f, .3f, .33f, 1f);
            art.sortingOrder = 20;
            CircleCollider2D collider = hazard.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = .95f;
            hazard.AddComponent<ShadowPatrolHazard>();
        }
    }

    [MenuItem("Shadow Switch/2 - Open Campaign Hub")]
    public static void OpenHub()
    {
        string path = Scenes + "ShadowCampaignHub.unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) { Install(); return; }
        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }

    [MenuItem("Shadow Switch/3 - Export Campaign WebGL")]
    public static void ExportWebGL()
    {
        string[] names = { "MainMenu", "Level02", "Victory", "GameOver", "PhysicsBonus3D", "EchoShift",
            "ShadowCampaignHub", "ShadowStage1", "ShadowStage2", "ShadowStage3",
            "ShadowCampaignResult", "ShadowCampaignJournal", "ShadowCampaignEnding" };
        var existing = new List<string>();
        foreach (string n in names)
        {
            string path = Scenes + n + ".unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null) existing.Add(path);
        }
        if (existing.Count < 11)
        {
            EditorUtility.DisplayDialog("Shadow Switch", "Missing campaign scenes. Run Shadow Switch > 1 - Install Campaign (safe) first.", "OK");
            return;
        }
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
        {
            EditorUtility.DisplayDialog("WebGL", "Install WebGL Build Support through Unity Hub first.", "OK");
            return;
        }
        PlayerSettings.productName = "Shadow Switch - The Last Beacon";
        string folder = Path.GetFullPath("Builds/WebGL_ShadowSwitch_Campaign");
        Directory.CreateDirectory(folder);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = existing.ToArray(),
            locationPathName = folder,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });
        Debug.Log("Shadow Switch campaign WebGL build: " + report.summary.result + " at " + folder);
    }
}
#endif
