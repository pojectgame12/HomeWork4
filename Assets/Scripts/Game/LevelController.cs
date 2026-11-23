using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Level lifetime, time limit, and lightweight HUD. Campaign-aware, classic-compatible.</summary>
public sealed class LevelController : MonoBehaviour
{
    private const float StatusMessageDisplaySeconds = 2.5f;

    [SerializeField] private LevelSettings settings;
    public LevelSettings Settings => settings;
    public void Configure(LevelSettings value) { settings = value; }

    private string status = "";
    private float statusUntil;

    private void Awake() { GameSession.Instance.Configure(settings); }
    private void OnEnable() { GameEvents.StatusMessage += ShowStatus; }
    private void OnDisable() { GameEvents.StatusMessage -= ShowStatus; }
    private void Start() { GameSession.Instance.BeginLevel(); }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ShadowCampaignRuntime.Current != null) ShadowCampaignProgress.ReturnToHub();
            else SceneManager.LoadScene("MainMenu");
        }
        if (!GameSession.Instance.IsTransitioning && GameSession.Instance.TimeRemaining <= 0f)
            GameEvents.RaisePlayerDied();
    }

    private void ShowStatus(string message)
    {
        status = message;
        statusUntil = Time.time + StatusMessageDisplaySeconds;
    }

    private void OnGUI()
    {
        GUIStyle label = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.Max(17, Mathf.RoundToInt(Screen.height * .028f)),
            fontStyle = FontStyle.Bold
        };
        label.normal.textColor = new Color(.8f, 1f, 1f);

        GameSession session = GameSession.Instance;
        float energy = LampController.Instance != null ? LampController.Instance.Energy : session.remainingEnergy;
        GUI.Box(new Rect(12, 12, Mathf.Min(800, Screen.width - 24), 113), "");
        bool campaign = ShadowCampaignRuntime.Current != null;
        string headline = campaign ?
            "SHADOW SWITCH  |  STAGE " + ShadowCampaignRuntime.Current.stage + ": " +
                ShadowCampaignProgress.StageTitle(ShadowCampaignRuntime.Current.stage) :
            "SHADOW SWITCH  |  LEVEL 2: FADING LIGHT";
        GUI.Label(new Rect(27, 14, 740, 38), headline, label);
        GUI.Label(new Rect(27, 53, 740, 38), "ENERGY: " + Mathf.RoundToInt(energy) + "%  |  CRYSTALS: " +
            session.crystalsCollected + "/" + session.RequiredCrystals +
            "  |  TIME: " + Mathf.CeilToInt(session.TimeRemaining), label);
        GUIStyle helper = new GUIStyle(GUI.skin.box)
        {
            fontSize = Mathf.Max(13, Mathf.RoundToInt(Screen.height * .020f)),
            alignment = TextAnchor.MiddleCenter
        };
        string help = "A/D: move  |  SPACE: jump  |  E: lamp  |  J/L or mouse: aim  |  R: retry" +
            (campaign ? "  |  H: hint" : "");
        GUI.Box(new Rect(Mathf.Max(5, (Screen.width - 800) / 2), Screen.height - 56,
            Mathf.Min(800, Screen.width - 10), 40), help, helper);
        if (Time.time < statusUntil) GUI.Label(new Rect(27, 90, 740, 34), status, label);
    }
}
