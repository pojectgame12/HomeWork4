#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>One-click creation of a playable Echo Shift mini-game, without touching original scenes.</summary>
public static class Homework4EchoShiftBuilder
{
    private const string ScenePath = "Assets/Scenes/EchoShift.unity";
    private const string ArtPath = "Assets/EchoShift/Art/";

    [MenuItem("Homework 4/Create Echo Shift Level")]
    public static void Build()
    {
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "EchoShift");
        EnsureFolder("Assets/EchoShift", "Art");

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
        {
            AddToBuildSettings();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorUtility.DisplayDialog("Echo Shift", "Existing EchoShift scene opened; your edits were preserved.", "OK");
            return;
        }

        Sprite playerArt = ColoredSprite("echo_player.png", Color.white);
        Sprite floorArt = ColoredSprite("echo_floor.png", new Color(.23f, .34f, .46f));
        Sprite platformArt = ColoredSprite("echo_platform.png", new Color(.34f, .54f, .68f));
        Sprite switchArt = ColoredSprite("echo_switch.png", Color.white);
        Sprite doorArt = ColoredSprite("echo_door.png", Color.white);
        Sprite exitArt = ColoredSprite("echo_exit.png", new Color(.35f, .9f, 1f));

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 5.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.035f, .058f, .12f);
        cameraObject.transform.position = new Vector3(0f, -.35f, -10f);
        cameraObject.AddComponent<AudioListener>();

        Solid("Ground", floorArt, new Vector2(0f, -3.8f), new Vector2(20f, 1.2f));
        Solid("PracticePlatform_Left", platformArt, new Vector2(-5.2f, -.65f), new Vector2(2.5f, .32f));
        Solid("PracticePlatform_Right", platformArt, new Vector2(.9f, .15f), new Vector2(2.2f, .32f));

        GameObject playerObject = Visual("Active_Player", playerArt,
            new Vector2(-8f, -2.6f), new Vector2(.75f, 1.2f), 14);
        Rigidbody2D physics = playerObject.AddComponent<Rigidbody2D>();
        physics.gravityScale = 2.2f;
        physics.freezeRotation = true;
        physics.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        playerObject.AddComponent<BoxCollider2D>();
        EchoPlayer player = playerObject.AddComponent<EchoPlayer>();

        EchoLoopManager manager = new GameObject("EchoLoopManager").AddComponent<EchoLoopManager>();

        EchoPressurePlate a = CreatePlate("Plate_A", switchArt, new Vector2(-2.3f, -3.10f), manager, "A");
        EchoPressurePlate b = CreatePlate("Plate_B", switchArt, new Vector2(2.2f, -3.10f), manager, "B");

        GameObject doorObject = Visual("DualSwitch_Door", doorArt,
            new Vector2(5.8f, -1.96f), new Vector2(.50f, 2.49f), 9);
        doorObject.AddComponent<BoxCollider2D>();
        EchoDoor door = doorObject.AddComponent<EchoDoor>();
        door.Configure(a, b);

        GameObject exitObject = Visual("Exit_Portal", exitArt,
            new Vector2(8.25f, -2.38f), new Vector2(1.15f, 1.65f), 8);
        BoxCollider2D exitCollider = exitObject.AddComponent<BoxCollider2D>();
        exitCollider.isTrigger = true;
        EchoExit goal = exitObject.AddComponent<EchoExit>();
        goal.Configure(manager, door);

        manager.Configure(player, a, b, door);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        AddToBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("Echo Shift created. Open MainMenu, press Play, then select ECHO SHIFT.");
        EditorUtility.DisplayDialog("Echo Shift", "EchoShift.unity created and added to Build Settings. Open MainMenu and press Play.", "OK");
    }

    private static void AddToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (EditorBuildSettingsScene item in scenes)
            if (item.path == ScenePath) return;
        scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            AssetDatabase.CreateFolder(parent, name);
    }

    private static Sprite ColoredSprite(string filename, Color color)
    {
        string asset = ArtPath + filename;
        if (!File.Exists(asset))
        {
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(asset, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }
        AssetDatabase.ImportAsset(asset, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(asset) as TextureImporter;
        if (importer == null) throw new System.Exception("Cannot import sprite: " + asset);
        if (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != 16f)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(asset);
    }

    private static GameObject Visual(string name, Sprite sprite, Vector2 position, Vector2 size, int sortingOrder)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = new Vector3(position.x, position.y, 0f);
        obj.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        return obj;
    }

    private static void Solid(string name, Sprite sprite, Vector2 position, Vector2 size)
    {
        GameObject obj = Visual(name, sprite, position, size, 2);
        obj.AddComponent<BoxCollider2D>();
    }

    private static EchoPressurePlate CreatePlate(string name, Sprite sprite, Vector2 position,
        EchoLoopManager manager, string id)
    {
        GameObject plate = Visual(name, sprite, position, new Vector2(1.6f, .16f), 7);
        plate.GetComponent<SpriteRenderer>().color = new Color(.13f, .53f, .29f);
        BoxCollider2D trigger = plate.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        EchoPressurePlate plateCode = plate.AddComponent<EchoPressurePlate>();
        plateCode.Configure(manager, id);
        return plateCode;
    }
}
#endif
