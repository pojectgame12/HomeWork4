using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Game state and rule transitions; receives gameplay events.</summary>
public sealed class GameSession : MonoBehaviour
{
    private static GameSession instance;
    public static GameSession Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameSession_Persistent");
                instance = go.AddComponent<GameSession>();
            }
            return instance;
        }
    }

    public int crystalsCollected { get; private set; }
    public int totalAttempts { get; private set; }
    public float remainingEnergy { get; private set; }
    public float elapsedSeconds { get; private set; }
    public bool completed { get; private set; }
    public int RequiredCrystals => settings != null ? settings.requiredCrystals : 1;
    public float TimeLimit => settings != null ? settings.timeLimitSeconds : 90f;
    public float TimeRemaining => Mathf.Max(0f, TimeLimit - (Time.time - levelStartTime));
    public bool IsTransitioning => transitioning;

    private LevelSettings settings;
    private float levelStartTime;
    private bool transitioning;

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        GameEvents.CrystalCollected += OnCrystalCollected;
        GameEvents.LampEnergyChanged += OnLampEnergyChanged;
        GameEvents.GoalReached += OnGoalReached;
        GameEvents.PlayerDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        GameEvents.CrystalCollected -= OnCrystalCollected;
        GameEvents.LampEnergyChanged -= OnLampEnergyChanged;
        GameEvents.GoalReached -= OnGoalReached;
        GameEvents.PlayerDied -= OnPlayerDied;
    }

    public void Configure(LevelSettings levelSettings) { settings = levelSettings; }

    public void BeginLevel()
    {
        totalAttempts++;
        crystalsCollected = 0;
        remainingEnergy = settings != null ? settings.startingEnergy : 60f;
        elapsedSeconds = 0f;
        completed = false;
        transitioning = false;
        levelStartTime = Time.time;
        MobileInputState.Clear();
    }

    private void OnCrystalCollected(float energyReward)
    {
        if (transitioning) return;
        crystalsCollected++;
        GameEvents.RaiseStatusMessage("Crystal collected! " + crystalsCollected + "/" + RequiredCrystals);
    }

    private void OnLampEnergyChanged(float energy) { remainingEnergy = Mathf.Max(0f, energy); }

    private void OnGoalReached()
    {
        if (transitioning) return;
        if (crystalsCollected < RequiredCrystals)
        {
            GameEvents.RaiseStatusMessage("Collect at least " + RequiredCrystals + " crystal(s) first!");
            return;
        }
        FinishLevel(remainingEnergy);
    }

    private void OnPlayerDied() { FailLevel(remainingEnergy); }

    public void FinishLevel(float energy)
    {
        if (transitioning) return;
        transitioning = true;
        completed = true;
        RecordResult(energy);
        SceneManager.LoadScene("Victory");
    }

    public void FailLevel(float energy)
    {
        if (transitioning) return;
        transitioning = true;
        completed = false;
        RecordResult(energy);
        SceneManager.LoadScene("GameOver");
    }

    private void RecordResult(float energy)
    {
        remainingEnergy = Mathf.Max(0f, energy);
        elapsedSeconds = Mathf.Max(0f, Time.time - levelStartTime);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
