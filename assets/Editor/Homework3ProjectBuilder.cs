#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Open Unity, then Homework 3 -> Generate Complete Game.
/// Builds saved .prefab assets and four .unity scenes; no hand-wiring is required.
/// </summary>
public static class Homework3ProjectBuilder
{
    const string ART = "Assets/Art/";
    const string PF = "Assets/Prefabs/";
    const string SC = "Assets/Scenes/";

    [MenuItem("Homework 3/Generate Complete Game (Scenes + Prefabs)")]
    public static void BuildAll()
    {
        // ZIP/Git does not preserve empty directories. Recreate them in Unity.
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        PrepareSprite("Player.png", 256);
        PrepareSprite("Lamp.png", 256);
        PrepareSprite("Crystal.png", 256);
        PrepareSprite("Obelisk.png", 256);
        PrepareSprite("Background.png", 256);
        PrepareSprite("StoneTile.png", 256);
        PrepareSprite("ShadowBridge.png", 256);
        PrepareSprite("ExitDoor.png", 256);
        PrepareSprite("LightMote.png", 256);
        AssetDatabase.Refresh();
        CreatePrefabs();
        CreateMenu("MainMenu", GameMenuUI.MenuType.Main);
        CreateLevel();
        CreateMenu("Victory", GameMenuUI.MenuType.Victory);
        CreateMenu("GameOver", GameMenuUI.MenuType.GameOver);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(SC+"MainMenu.unity",true),
            new EditorBuildSettingsScene(SC+"Level02.unity",true),
            new EditorBuildSettingsScene(SC+"Victory.unity",true),
            new EditorBuildSettingsScene(SC+"GameOver.unity",true)
        };
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(SC+"MainMenu.unity", OpenSceneMode.Single);
        Debug.Log("Homework 3 game created: 4 scenes + 4 prefabs. Press Play in MainMenu.");
        EditorUtility.DisplayDialog("Homework 3", "All 4 scenes, prefabs and touch controls were generated. Press Play to test.", "OK");
    }

    private static void PrepareSprite(string file, float ppu)
    {
        string path = ART+file;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new System.Exception("Missing texture: " + path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = ppu;   
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);

        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
    }

    private static Sprite Sprite(string file)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(ART+file);
        if (s == null) throw new System.Exception("Cannot load sprite: " + file);
        return s;
    }

    private static GameObject MakePrefab(string name, string file, float scale, int order)
    {
        GameObject obj = new GameObject(name);
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite(file);
        sr.sortingOrder = order;
        obj.transform.localScale = new Vector3(scale,scale,1);
        return obj;
    }

    private static void CreatePrefabs()
    {
        // Created from image-generated sprite art, not merely a regular scene object.
        GameObject player = MakePrefab("Player", "Player.png", 0.76f, 15);
        Rigidbody2D physics = player.AddComponent<Rigidbody2D>();
        physics.gravityScale = 2.7f;
        physics.freezeRotation = true;
        physics.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        BoxCollider2D box = player.AddComponent<BoxCollider2D>();
        box.size = new Vector2(.85f, 1.53f);
        player.AddComponent<PlayerController>();
        PrefabUtility.SaveAsPrefabAsset(player,PF+"Player.prefab");
        Object.DestroyImmediate(player);

        GameObject lamp = MakePrefab("Lamp", "Lamp.png", 0.90f, 12);
        lamp.AddComponent<LampController>();
        PrefabUtility.SaveAsPrefabAsset(lamp,PF+"Lamp.prefab");
        Object.DestroyImmediate(lamp);

        GameObject crystal = MakePrefab("Crystal_AISprite", "Crystal.png", .46f, 16);
        var trigger = crystal.AddComponent<CircleCollider2D>();
        trigger.isTrigger=true;
        trigger.radius = .55f;
        crystal.AddComponent<CrystalPickup>();
        PrefabUtility.SaveAsPrefabAsset(crystal,PF+"Crystal.prefab");
        Object.DestroyImmediate(crystal);

        GameObject mote = MakePrefab("LightMote", "LightMote.png", .33f, 17);
        mote.AddComponent<MoteLife>();
        PrefabUtility.SaveAsPrefabAsset(mote,PF+"LightMote.prefab");
        Object.DestroyImmediate(mote);
    }

    private static void SetupCameraAndBackground()
    {
        GameObject camera = new GameObject("Main Camera");
        Camera c = camera.AddComponent<Camera>();
        c.orthographic=true;
        c.orthographicSize=6.0f;
        c.clearFlags=CameraClearFlags.SolidColor;
        c.backgroundColor=new Color(.02f,.06f,.13f);
        camera.tag="MainCamera";
        camera.AddComponent<AudioListener>();
        camera.transform.position = new Vector3(0,-.25f,-10);
        GameObject background = new GameObject("Lab_Background");
        var sr = background.AddComponent<SpriteRenderer>();
        sr.sprite=Sprite("Background.png");
        sr.sortingOrder=-100;
        background.transform.position=new Vector3(0,0,2);
        background.transform.localScale=new Vector3(3.2f,3.1f,1);
    }

    private static GameObject CreateFloor(string name,float x,float width)
    {
        GameObject floor = new GameObject(name);
        floor.transform.position = new Vector3(x,-3.75f,0);
        SpriteRenderer sr = floor.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite("StoneTile.png");
        sr.drawMode=SpriteDrawMode.Tiled;
        sr.size = new Vector2(width,1.5f);
        sr.sortingOrder = 5;
        BoxCollider2D collider = floor.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(width,1.5f);
        return floor;
    }

    private static void CreateLevel()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SetupCameraAndBackground();
        new GameObject("LevelController").AddComponent<LevelController>();
        CreateFloor("Left_Stone_Floor",-6.05f,5.9f); // spans -9.0 to -3.1
        CreateFloor("Right_Stone_Floor",6.15f,7.7f); // spans +2.3 to +10.0

        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PF+"Player.prefab"));
        player.name="Player";
        player.transform.position=new Vector3(-8.25f,-2.42f,0);

        GameObject statue = new GameObject("Designated_Shadow_Caster_O1");
        SpriteRenderer stone = statue.AddComponent<SpriteRenderer>();
        stone.sprite=Sprite("Obelisk.png");
        stone.sortingOrder=9;
        statue.transform.localScale = new Vector3(.68f,.68f,1);
        statue.transform.position = new Vector3(-4.05f,-2.32f,0);
        // Intentionally no solid collider on stone: it is a shadow caster, not a blocker.

        GameObject lamp = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PF+"Lamp.prefab"));
        lamp.name="Rotatable_Lamp_L1";
        lamp.transform.position=new Vector3(-5.7f,-2.18f,0);
        LampController lampLogic = lamp.GetComponent<LampController>();
        lampLogic.designatedObelisk=statue.transform;
        lampLogic.energy=60f;
        lampLogic.aimDegrees=-38f;

        GameObject bridge = new GameObject("ShadowBridge_B1");
        bridge.transform.position=new Vector3(-.4f,-3.135f,0);
        SpriteRenderer bridgeSprite=bridge.AddComponent<SpriteRenderer>();
        bridgeSprite.sprite=Sprite("ShadowBridge.png");
        bridgeSprite.drawMode=SpriteDrawMode.Tiled;
        bridgeSprite.size=new Vector2(5.55f,.27f);
        bridgeSprite.sortingOrder=6;
        BoxCollider2D bridgeCollider=bridge.AddComponent<BoxCollider2D>();
        bridgeCollider.size=new Vector2(5.55f,.27f);
        bridgeCollider.enabled=false;
        ShadowBridge bridgeLogic=bridge.AddComponent<ShadowBridge>();
        bridgeLogic.lamp=lampLogic;

        GameObject crystalSpawner = new GameObject("Crystal_Prefab_Spawner");
        PrefabSpawner spawner=crystalSpawner.AddComponent<PrefabSpawner>();
        spawner.prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PF+"Crystal.prefab");
        spawner.spawnPosition=new Vector3(-7.1f,-2.15f,0);

        GameObject moteSpawner=new GameObject("Automatic_Particle_Spawner");
        MoteSpawner motes=moteSpawner.AddComponent<MoteSpawner>();
        motes.bridge=bridgeLogic;
        motes.motePrefab=AssetDatabase.LoadAssetAtPath<GameObject>(PF+"LightMote.prefab");

        GameObject death=new GameObject("DeathZone_UnderPit");
        death.transform.position=new Vector3(-.4f,-5.15f,0);
        BoxCollider2D pit=death.AddComponent<BoxCollider2D>();
        pit.isTrigger=true;
        pit.size=new Vector2(5.7f,1.3f);
        death.AddComponent<PitTrigger>();

        GameObject exit=new GameObject("Door_Goal_Exit");
        exit.transform.position=new Vector3(8.30f,-2.12f,0);
        var door=exit.AddComponent<SpriteRenderer>();
        door.sprite=Sprite("ExitDoor.png");
        door.sortingOrder=10;
        exit.transform.localScale=new Vector3(1.05f,1.05f,1);
        BoxCollider2D exitCollider=exit.AddComponent<BoxCollider2D>();
        exitCollider.isTrigger=true;
        exitCollider.size=new Vector2(1.6f,1.6f);
        exit.AddComponent<GoalTrigger>();

        CreateTouchCanvas();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),SC+"Level02.unity");
    }

    private static void CreateMenu(string name, GameMenuUI.MenuType type)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SetupCameraAndBackground();
        new GameObject("Screen_"+name).AddComponent<GameMenuUI>().menuType=type;
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),SC+name+".unity");
    }

    private static void CreateTouchCanvas()
    {
        var canvasGO = new GameObject("TouchControlsCanvas");
        Canvas canvas=canvasGO.AddComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder=100;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        CanvasScaler scaler=canvasGO.GetComponent<CanvasScaler>();
        scaler.referenceResolution=new Vector2(1280,720);
        scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight=.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Every arrow is a real touch target: IPointerDown/Up + Unity EventSystem.
        TouchButton(canvasGO.transform,"LEFT", "<",new Vector2(0,0),new Vector2(75,120),TouchControlButton.Action.Left);
        TouchButton(canvasGO.transform,"RIGHT",">",new Vector2(0,0),new Vector2(180,120),TouchControlButton.Action.Right);
        TouchButton(canvasGO.transform,"ROTATE LEFT", "-",new Vector2(1,0),new Vector2(-285,245),TouchControlButton.Action.RotateLeft);
        TouchButton(canvasGO.transform,"ROTATE RIGHT","+",new Vector2(1,0),new Vector2(-180,245),TouchControlButton.Action.RotateRight);
        TouchButton(canvasGO.transform,"LAMP", "LIGHT",new Vector2(1,0),new Vector2(-255,125),TouchControlButton.Action.Lamp);
        TouchButton(canvasGO.transform,"JUMP", "JUMP",new Vector2(1,0),new Vector2(-100,125),TouchControlButton.Action.Jump);

        GameObject events=new GameObject("EventSystem");
        events.AddComponent<EventSystem>();
        events.AddComponent<StandaloneInputModule>();
    }

    private static void TouchButton(Transform parent,string objName,string caption,Vector2 anchor,Vector2 offset,TouchControlButton.Action action)
    {
        var go=new GameObject("Touch_"+objName,typeof(RectTransform),typeof(Image),typeof(TouchControlButton));
        go.transform.SetParent(parent,false);
        RectTransform rect=go.GetComponent<RectTransform>();
        rect.anchorMin=rect.anchorMax=anchor;
        rect.pivot=new Vector2(.5f,.5f);
        rect.sizeDelta = new Vector2(92,92);
        rect.anchoredPosition=offset;
        Image img=go.GetComponent<Image>();
        img.color=new Color(.07f,.40f,.47f,.75f);
        img.raycastTarget=true;
        go.GetComponent<TouchControlButton>().action=action;
        var textGO=new GameObject("Text",typeof(RectTransform),typeof(Text));
        textGO.transform.SetParent(go.transform,false);
        RectTransform tRect=textGO.GetComponent<RectTransform>();
        tRect.anchorMin=Vector2.zero;
        tRect.anchorMax=Vector2.one;
        tRect.offsetMin=Vector2.zero;
        tRect.offsetMax=Vector2.zero;
        Text t=textGO.GetComponent<Text>();
        t.text=caption;
        t.alignment=TextAnchor.MiddleCenter;
        t.fontSize=(caption.Length>2?23:45);
        t.fontStyle=FontStyle.Bold;
        t.color=Color.white;
        t.raycastTarget=false;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    }
}
#endif
