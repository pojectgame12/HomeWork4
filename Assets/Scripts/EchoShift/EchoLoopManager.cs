using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// Echo Shift loop manager + Live Ops demo.
/// Preserves the original ten-second echo game when no live event is active.
/// Replace only the .cs contents of the existing file, retaining its .meta GUID.
/// </summary>
public sealed class EchoLoopManager : MonoBehaviour
{
    [SerializeField] private EchoPlayer activePlayer;
    [SerializeField] private EchoPressurePlate plateA;
    [SerializeField] private EchoPressurePlate plateB;
    [SerializeField] private EchoDoor door;
    [SerializeField, Min(1f)] private float loopSeconds = 10f;
    [SerializeField, Range(1, 3)] private int maxEchoes = 3;
    [SerializeField, Min(2)] private int maxLoops = 8;

    [Header("Live Ops (optional)")]
    [Tooltip("Optional HTTPS JSON endpoint, with CORS configured for WebGL. Empty = local StreamingAssets JSON.")]
    [SerializeField] private string liveOpsRemoteUrl = "";

    [Serializable]
    private sealed class LiveOpsConfig
    {
        public string eventId;
        public bool enabled;
        public string startUtc;
        public string endUtc;
        public float loopSeconds;
        public int maxLoops;
        public int maxEchoes;
        public string banner;
    }

    private readonly List<EchoGhost> ghosts = new List<EchoGhost>();
    private readonly List<EchoFrame> recording = new List<EchoFrame>();
    private Vector2 startingPosition;
    private float elapsed;
    private int loopNumber = 1;
    private bool running;
    private bool won;
    private bool isLoading = true;
    private bool liveEventActive;
    private string liveEventBanner = "";
    private string message = "";
    private float messageUntil;
    private bool loadedFromRemote;

    public EchoPlayer ActivePlayer => activePlayer;
    public IReadOnlyList<EchoGhost> Ghosts => ghosts;
    public bool IsRunning => running;

    public void Configure(EchoPlayer player, EchoPressurePlate a, EchoPressurePlate b, EchoDoor gate)
    {
        activePlayer = player;
        plateA = a;
        plateB = b;
        door = gate;
    }

    // Unity permits Start to be an IEnumerator for asynchronous initialization.
    private IEnumerator Start()
    {
        if (activePlayer == null || plateA == null || plateB == null || door == null)
        {
            Debug.LogError("Echo Shift: unassigned components. Use Homework 4 > Create Echo Shift Level.");
            enabled = false;
            yield break;
        }

        // Load optional configuration before the first recorded cycle.
        yield return StartCoroutine(LoadLiveOpsConfig());
        startingPosition = activePlayer.Position;
        activePlayer.ResetAt(startingPosition);
        isLoading = false;
        running = true;
        message = liveEventActive
            ? "Resonance Rush is active! Plan shorter loops."
            : "Record a loop on plate A. Next loop, stand on B. Then run through the door!";
        messageUntil = Time.time + 7f;
    }

    private IEnumerator LoadLiveOpsConfig()
    {
        if (!string.IsNullOrWhiteSpace(liveOpsRemoteUrl))
        {
            yield return StartCoroutine(ReadConfig(liveOpsRemoteUrl.Trim()));
            if (loadedFromRemote) yield break;
            Debug.LogWarning("Echo Shift Live Ops: remote configuration unavailable; trying StreamingAssets.");
        }

        string localUrl = Application.streamingAssetsPath.TrimEnd('/', '\\') + "/echo-liveops.json";
        if (!localUrl.Contains("://"))
            localUrl = new Uri(localUrl).AbsoluteUri;
        yield return StartCoroutine(ReadConfig(localUrl));
    }

    private IEnumerator ReadConfig(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 6;
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Echo Shift Live Ops: cannot read config from " + url + " (" + request.error + "). Base rules preserved.");
                yield break;
            }

            try
            {
                LiveOpsConfig config = JsonUtility.FromJson<LiveOpsConfig>(request.downloadHandler.text);
                // A readable JSON document counts as a valid config source even if no event is active.
                loadedFromRemote = !string.IsNullOrWhiteSpace(liveOpsRemoteUrl)
                    && string.Equals(url, liveOpsRemoteUrl.Trim(), StringComparison.Ordinal);
                ApplyConfig(config);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Echo Shift Live Ops: invalid JSON; base rules preserved. " + ex.Message);
            }
        }
    }

    private void ApplyConfig(LiveOpsConfig config)
    {
        if (config == null || !config.enabled) return;

        DateTimeOffset start, end;
        bool hasStart = DateTimeOffset.TryParse(config.startUtc, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal, out start);
        bool hasEnd = DateTimeOffset.TryParse(config.endUtc, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal, out end);
        if (!hasStart || !hasEnd || end <= start)
        {
            Debug.LogWarning("Echo Shift Live Ops: invalid UTC date window. Ignoring event.");
            return;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (now < start || now > end) return;

        if (config.loopSeconds < 3f || config.loopSeconds > 30f || config.maxLoops < 2 || config.maxLoops > 20
            || config.maxEchoes < 1 || config.maxEchoes > 3)
        {
            Debug.LogWarning("Echo Shift Live Ops: event settings outside safe limits. Ignoring event.");
            return;
        }

        loopSeconds = config.loopSeconds;
        maxLoops = config.maxLoops;
        maxEchoes = config.maxEchoes;
        liveEventActive = true;
        liveEventBanner = string.IsNullOrWhiteSpace(config.banner) ? "LIVE EVENT" : config.banner;
        Debug.Log("Echo Shift Live Ops: active event " + config.eventId +
                  ", loopSeconds=" + loopSeconds + ", maxLoops=" + maxLoops);
    }

    private void FixedUpdate()
    {
        if (!running) return;
        if (activePlayer.Position.y < -6f)
        {
            Lose();
            return;
        }
        int frame = recording.Count;
        foreach (EchoGhost ghost in ghosts)
            if (ghost != null) ghost.SetFrame(frame);

        recording.Add(new EchoFrame(activePlayer.Position, activePlayer.FacingLeft));
        elapsed += Time.fixedDeltaTime;
        if (elapsed + .0001f >= loopSeconds) EndLoop();
    }

    private void EndLoop()
    {
        if (loopNumber >= maxLoops)
        {
            Lose();
            return;
        }
        if (ghosts.Count >= maxEchoes)
        {
            EchoGhost oldest = ghosts[0];
            ghosts.RemoveAt(0);
            if (oldest != null) Destroy(oldest.gameObject);
        }
        GameObject objectForEcho = new GameObject("RecordedEcho_" + loopNumber);
        objectForEcho.transform.localScale = activePlayer.transform.localScale;
        EchoGhost newGhost = objectForEcho.AddComponent<EchoGhost>();
        Color tint = loopNumber % 3 == 1 ? new Color(.3f, .95f, 1f, .65f) :
            loopNumber % 3 == 2 ? new Color(.7f, .48f, 1f, .65f) : new Color(1f, .63f, .37f, .65f);
        newGhost.Initialize(recording, activePlayer.PlayerSprite, tint);
        ghosts.Add(newGhost);

        loopNumber++;
        elapsed = 0f;
        recording.Clear();
        activePlayer.ResetAt(startingPosition);
        foreach (EchoGhost ghost in ghosts) ghost.SetFrame(0);
        plateA.ResetState();
        plateB.ResetState();
        door.Refresh();
        ShowMessage("Loop " + loopNumber + ": your previous movement is now an echo!");
    }

    private void ShowMessage(string msg)
    {
        message = msg;
        messageUntil = Time.time + 3f;
    }

    public void Win()
    {
        if (!running) return;
        won = true;
        EndGame();
    }

    private void Lose()
    {
        if (!running) return;
        won = false;
        EndGame();
    }

    private void EndGame()
    {
        running = false;
        activePlayer.FreezeForResult();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene("MainMenu");
        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene("EchoShift");
    }

    private void OnGUI()
    {
        int size = Mathf.Max(15, Mathf.RoundToInt(Screen.height * .028f));
        GUIStyle label = new GUIStyle(GUI.skin.box) { fontSize = size, wordWrap = true };
        if (isLoading)
        {
            GUI.Box(new Rect(12, 12, Screen.width - 24, 46), "ECHO SHIFT | Loading rules...", label);
            return;
        }
        string heading = "ECHO SHIFT  |  LOOP " + loopNumber + "/" + maxLoops +
                         "  |  TIME " + Mathf.CeilToInt(Mathf.Max(0f, loopSeconds - elapsed)) +
                         "s  |  ECHOES " + ghosts.Count + "/" + maxEchoes;
        GUI.Box(new Rect(12, 12, Screen.width - 24, 46), heading, label);
        float controlsY = 62f;
        if (liveEventActive)
        {
            GUI.Box(new Rect(12, 62, Screen.width - 24, 44), liveEventBanner, label);
            controlsY = 112f;
        }
        GUI.Box(new Rect(12, controlsY, Screen.width - 24, 75),
            "A/D: Move  |  SPACE: Jump  |  R: Restart  |  ESC: Main Menu\n" +
            "PLATE A: " + (plateA != null && plateA.IsPressed ? "ON" : "OFF") +
            "     PLATE B: " + (plateB != null && plateB.IsPressed ? "ON" : "OFF") +
            "     DOOR: " + (door != null && door.IsOpen ? "OPEN" : "CLOSED"), label);
        if (running && Time.time < messageUntil)
            GUI.Box(new Rect(12, Screen.height - 80, Screen.width - 24, 58), message, label);
        if (running) return;

        float x = Screen.width / 2f - 250f;
        float y = Screen.height / 2f - 125f;
        GUI.Box(new Rect(x, y, 500f, 250f), won
            ? "YOU WIN!  Echoes cooperated to open the exit."
            : "GAME OVER!  Retry and use the loops carefully.", label);
        if (GUI.Button(new Rect(x + 45f, y + 120f, 185f, 65f), "RETRY"))
            SceneManager.LoadScene("EchoShift");
        if (GUI.Button(new Rect(x + 275f, y + 120f, 185f, 65f), "MAIN MENU"))
            SceneManager.LoadScene("MainMenu");
    }
}
