#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Run once via Homework 4 > Upgrade Existing Project.
/// Keeps the existing Homework 3 scenes/prefabs, adds a jointed pickup and 3D bonus.
/// Safe to rerun; does not regenerate or replace the original level.
/// </summary>
public static class Homework4ProjectSetup
{
    private const string Scenes = "Assets/Scenes/";
    private const string Data = "Assets/Data/";
    private const string Materials = "Assets/Materials/";

    [MenuItem("Homework 4/Upgrade Existing Project")]
    public static void Upgrade()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Scenes + "Level02.unity") == null)
        {
            EditorUtility.DisplayDialog("Homework 4", "Missing Assets/Scenes/Level02.unity. Open the Homework 3 Unity project first.", "OK");
            return;
        }
        EnsureFolder("Assets", "Data");
        EnsureFolder("Assets", "Materials");
        LevelSettings settings = GetOrCreateSettings();
        AddOrbAndSettings(settings);
        CreateBonus3D();
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(Scenes + "MainMenu.unity", true),
            new EditorBuildSettingsScene(Scenes + "Level02.unity", true),
            new EditorBuildSettingsScene(Scenes + "Victory.unity", true),
            new EditorBuildSettingsScene(Scenes + "GameOver.unity", true),
            new EditorBuildSettingsScene(Scenes + "PhysicsBonus3D.unity", true)
        };
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(Scenes + "MainMenu.unity", OpenSceneMode.Single);
        Debug.Log("Homework 4 upgrade complete: LevelSettings, swinging 2D joint orb, 3D bonus scene, build scenes.");
        EditorUtility.DisplayDialog("Homework 4", "Upgrade complete. Press Play in MainMenu and test both modes.", "OK");
    }

    private static void EnsureFolder(string parent, string folder)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + folder))
            AssetDatabase.CreateFolder(parent, folder);
    }

    private static LevelSettings GetOrCreateSettings()
    {
        const string path = Data + "LevelSettings.asset";
        LevelSettings settings = AssetDatabase.LoadAssetAtPath<LevelSettings>(path);
        if (settings != null) return settings;
        settings = ScriptableObject.CreateInstance<LevelSettings>();
        AssetDatabase.CreateAsset(settings, path);
        return settings;
    }

    private static void AddOrbAndSettings(LevelSettings settings)
    {
        EditorSceneManager.OpenScene(Scenes + "Level02.unity", OpenSceneMode.Single);
        LevelController controller = UnityEngine.Object.FindFirstObjectByType<LevelController>();
        if (controller == null) throw new Exception("Level02 has no LevelController");
        controller.Configure(settings);
        EditorUtility.SetDirty(controller);

        GameObject existing = GameObject.Find("SwingingEnergyOrb_Joint2D");
        if (existing == null)
        {
            GameObject orb = new GameObject("SwingingEnergyOrb_Joint2D");
            orb.transform.position = new Vector3(4.65f, -1.75f, 0f);
            orb.transform.localScale = Vector3.one * .40f;
            SpriteRenderer image = orb.AddComponent<SpriteRenderer>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Crystal.png");
            image.sortingOrder = 16;
            Rigidbody2D body = orb.AddComponent<Rigidbody2D>();
            body.gravityScale = 1.1f;
            body.mass = .5f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            CircleCollider2D collider = orb.AddComponent<CircleCollider2D>();
            collider.radius = .75f;
            DistanceJoint2D joint = orb.AddComponent<DistanceJoint2D>();
            joint.autoConfigureDistance = false;
            joint.connectedAnchor = new Vector2(4.65f, -.15f);
            joint.distance = 1.60f;
            orb.AddComponent<SwingingEnergyOrb>();
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }

    private static Material MakeMaterial(string name, Color color)
    {
        string path = Materials + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        material = new Material(shader);
        material.color = color;
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static GameObject Cube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.position = position;
        obj.transform.localScale = scale;
        if (material != null) obj.GetComponent<Renderer>().sharedMaterial = material;
        return obj;
    }

    private static void CreateBonus3D()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Material ground = MakeMaterial("BonusGround", new Color(.15f, .23f, .34f));
        Material wall = MakeMaterial("BonusWall", new Color(.14f, .51f, .58f));
        Material crateMaterial = MakeMaterial("BonusCrate", new Color(.93f, .66f, .18f));
        Material plateMaterial = MakeMaterial("BonusPlate", new Color(.20f, .85f, .45f));
        Material portalMaterial = MakeMaterial("BonusPortal", new Color(.66f, .39f, 1f));

        Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.gameObject.tag = "MainCamera";
        camera.transform.position = new Vector3(0f, 10.5f, -14.5f);
        camera.transform.LookAt(new Vector3(0f, .5f, 1.5f));
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.04f, .07f, .12f);
        camera.gameObject.AddComponent<AudioListener>();
        Light light = new GameObject("Directional Light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.3f;
        light.transform.rotation = Quaternion.Euler(45f, -25f, 0f);

        Cube("ArenaFloor", new Vector3(0f, -.5f, 0f), new Vector3(14f, 1f, 20f), ground);
        Cube("LeftWall", new Vector3(-7.1f, .9f, 0f), new Vector3(.3f, 1.8f, 20f), wall);
        Cube("RightWall", new Vector3(7.1f, .9f, 0f), new Vector3(.3f, 1.8f, 20f), wall);
        Cube("BackWall", new Vector3(0f, .9f, -9.8f), new Vector3(14f, 1.8f, .3f), wall);
        Cube("FrontWall", new Vector3(0f, .9f, 9.8f), new Vector3(14f, 1.8f, .3f), wall);

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Bonus3D_CharacterController_Player";
        player.transform.position = new Vector3(0f, .05f, -6f);
        UnityEngine.Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController character = player.AddComponent<CharacterController>();
        character.center = Vector3.up;
        character.height = 2f;
        character.radius = .45f;
        player.AddComponent<CharacterController3D>();
        player.GetComponent<Renderer>().sharedMaterial = wall;

        GameObject crate = Cube("PushablePhysicsCrate", new Vector3(0f, .62f, -1.3f),
                                new Vector3(1.2f, 1.2f, 1.2f), crateMaterial);
        Rigidbody crateBody = crate.AddComponent<Rigidbody>();
        crateBody.mass = 1f;
        crateBody.linearDamping = 1.6f;
        crateBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        GameObject plate = Cube("GreenPressurePlate", new Vector3(0f, .07f, 3.6f),
                               new Vector3(2.2f, .14f, 2.2f), plateMaterial);
        plate.GetComponent<BoxCollider>().isTrigger = true;
        PhysicsSwitch3D switch3D = plate.AddComponent<PhysicsSwitch3D>();
        switch3D.Configure(crateBody);

        PhysicsChallengeManager manager = new GameObject("Bonus3D_GameManager")
            .AddComponent<PhysicsChallengeManager>();
        manager.Configure(switch3D);

        GameObject portal = Cube("PurpleExitPortal", new Vector3(3.5f, 1.4f, 7.1f),
                                 new Vector3(1.6f, 2.8f, .35f), portalMaterial);
        portal.GetComponent<BoxCollider>().isTrigger = true;
        portal.AddComponent<ExitPortal3D>().Configure(manager);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), Scenes + "PhysicsBonus3D.unity");
    }
}
#endif
