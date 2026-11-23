using UnityEngine;

/// <summary>Bootstraps each COPIED level before LevelController.Awake; never alters the source asset.</summary>
[DefaultExecutionOrder(-10000)]
public sealed class ShadowCampaignRuntime : MonoBehaviour
{
    public int stage = 1;
    public static ShadowCampaignRuntime Current { get; private set; }
    public bool UsedHint { get; private set; }
    public string FailureReason { get; private set; } = "The light went out or the player fell.";

    private LevelSettings runtimeSettings;
    private bool showingIntro = true;
    private float loreUntil;
    private string loreText = "";
    private float bridgeFeedbackUntil;
    private string bridgeText = "";

    private void Awake()
    {
        Current = this;
        Time.timeScale = 1f;
        ShadowCampaignProgress.SetStageFromScene(stage);
        ShadowLiveOps.EnsureFallback();
        LevelController level = FindFirstObjectByType<LevelController>();
        if (level == null || level.Settings == null)
        {
            Debug.LogError("Shadow Campaign: LevelController or LevelSettings missing. Run Homework 4 upgrade first.");
            enabled = false;
            return;
        }
        runtimeSettings = Instantiate(level.Settings);
        runtimeSettings.name = "ShadowCampaign_RuntimeSettings";
        ApplyDifficulty(runtimeSettings, ShadowCampaignProgress.SelectedDifficulty);
        runtimeSettings.requiredCrystals = stage >= 2 ? 2 : 1;
        ShadowLiveOps.ApplyAtLevelStart(runtimeSettings);
        level.Configure(runtimeSettings);
        GameSession.Instance.Configure(runtimeSettings);
    }

    private static void ApplyDifficulty(LevelSettings s, ShadowCampaignProgress.Difficulty difficulty)
    {
        switch (difficulty)
        {
            case ShadowCampaignProgress.Difficulty.Apprentice:
                s.startingEnergy = 80f; s.drainPerSecond = 8f; s.timeLimitSeconds = 120f; break;
            case ShadowCampaignProgress.Difficulty.Keeper:
                s.startingEnergy = 45f; s.drainPerSecond = 16f; s.timeLimitSeconds = 75f; break;
            default:
                s.startingEnergy = 60f; s.drainPerSecond = 12f; s.timeLimitSeconds = 90f; break;
        }
    }

    private void Start()
    {
        if (!enabled) return;
        Time.timeScale = 0f;
        LampController lamp = FindFirstObjectByType<LampController>();
        if (lamp != null) lamp.BridgeConditionsChanged += BridgeChanged;
    }

    private void Update()
    {
        if (showingIntro && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            CloseIntro();
        if (!showingIntro && Input.GetKeyDown(KeyCode.H)) ShowHint();
    }

    private void CloseIntro()
    {
        showingIntro = false;
        Time.timeScale = 1f;
    }

    private void BridgeChanged(bool solid)
    {
        bridgeText = solid ? "SHADOW BRIDGE ACTIVE - CROSS NOW" : "Shadow bridge inactive";
        bridgeFeedbackUntil = Time.unscaledTime + 2.1f;
    }

    public void ShowLore(string speaker, string message)
    {
        loreText = speaker + ": " + message;
        loreUntil = Time.unscaledTime + 8f;
    }

    public void ShowHint()
    {
        UsedHint = true;
        string hint;
        if (stage == 1) hint = "MAYA: Stand near the lamp, press E, and aim at the obelisk with J/L or mouse.";
        else if (stage == 2) hint = "MAYA: Collect the first crystal BEFORE the bridge. The second is across it.";
        else hint = "MAYA: Observe the red patrol. Cross the bridge after it passes; conserve light.";
        ShowLore("HINT", hint);
    }

    public string DiagnoseFailure()
    {
        if (GameSession.Instance.TimeRemaining <= .2f) return "Time expired. Try moving sooner or choosing Apprentice.";
        LampController lamp = FindFirstObjectByType<LampController>();
        if (lamp != null && lamp.Energy <= .1f) return "Light energy exhausted. Turn the lamp off when not crossing.";
        return "You fell or touched a hazard. Watch the bridge and patrol timing.";
    }

    public void RecordBeforeExit(bool won)
    {
        FailureReason = won ? "Level completed" : DiagnoseFailure();
        ShadowCampaignProgress.Record(won, GameSession.Instance, UsedHint, FailureReason);
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        LampController lamp = FindFirstObjectByType<LampController>();
        if (lamp != null) lamp.BridgeConditionsChanged -= BridgeChanged;
        if (Current == this) Current = null;
        if (runtimeSettings != null) Destroy(runtimeSettings);
        Time.timeScale = 1f;
    }

    private void OnGUI()
    {
        if (!enabled) return;
        float scale = Mathf.Max(.55f, Mathf.Min(Screen.width / 1280f, Screen.height / 720f));
        Matrix4x4 prior = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280f * scale) / 2f,
            (Screen.height - 720f * scale) / 2f, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
        GUIStyle notice = new GUIStyle(GUI.skin.box) { fontSize = 18, wordWrap = true, alignment = TextAnchor.MiddleCenter };
        GUI.Box(new Rect(920, 15, 340, 88), "STAGE " + stage + ": " + ShadowCampaignProgress.StageTitle(stage) +
            "\n" + ShadowCampaignProgress.SelectedDifficulty + (ShadowLiveOps.IsActive ? " | ECLIPSE NIGHT" : ""), notice);
        if (ShadowCampaignProgress.Failures(stage) >= 2)
            GUI.Box(new Rect(920, 110, 340, 45), "Stuck? Press H for a hint", notice);
        if (Time.unscaledTime < bridgeFeedbackUntil)
            GUI.Box(new Rect(410, 130, 460, 40), bridgeText, notice);
        if (Time.unscaledTime < loreUntil)
            GUI.Box(new Rect(260, 570, 760, 80), loreText, notice);
        if (showingIntro)
        {
            GUI.Box(new Rect(170, 160, 940, 390), "");
            GUIStyle heading = new GUIStyle(GUI.skin.label) { fontSize = 36, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            heading.normal.textColor = Color.cyan;
            GUI.Label(new Rect(220, 180, 840, 65), "SHADOW SWITCH - " + ShadowCampaignProgress.StageTitle(stage), heading);
            GUI.Label(new Rect(240, 262, 800, 145), IntroText(), new GUIStyle(GUI.skin.label)
            { fontSize = 25, wordWrap = true, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } });
            if (GUI.Button(new Rect(480, 437, 320, 62), "START / ENTER")) CloseIntro();
        }
        GUI.matrix = prior;
    }

    private string IntroText()
    {
        switch (stage)
        {
            case 1: return "NOOR: The valley is dark. MAYA: Stand near the lamp and make the first bridge.\nCollect one crystal and reach the door. Press H if you need help.";
            case 2: return "MAYA: The safety archive survived beyond the broken bridge.\nCollect TWO crystals and find the archive terminal on the other side.";
            default: return "GREY WARDEN: Entry prohibited. A moving security sentinel now guards the bridge.\nTime your crossing, collect TWO crystals and reach the beacon core.";
        }
    }
}
