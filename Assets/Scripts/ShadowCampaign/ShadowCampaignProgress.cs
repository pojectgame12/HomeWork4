using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Local, offline campaign progression. Does not change the classic game.</summary>
public static class ShadowCampaignProgress
{
    public enum Difficulty { Apprentice = 0, Explorer = 1, Keeper = 2 }

    private const string Prefix = "ShadowCampaign.v1.";
    public const int MaxStage = ShadowGameTuning.FinalStage;
    public static Difficulty SelectedDifficulty { get; private set; } = Difficulty.Explorer;
    public static int CurrentStage { get; private set; } = 1;
    public static bool PlayingCampaign { get; private set; }

    public struct Result
    {
        public bool exists;
        public bool won;
        public int stage;
        public int medal;
        public float seconds;
        public float energy;
        public int crystals;
        public bool usedHint;
        public string reason;
    }

    public static Result LastResult { get; private set; }

    public static int UnlockedStage => Mathf.Clamp(PlayerPrefs.GetInt(Prefix + "unlocked", ShadowGameTuning.FirstStage), ShadowGameTuning.FirstStage, MaxStage);
    public static bool IsUnlocked(int stage) => stage >= ShadowGameTuning.FirstStage && stage <= UnlockedStage;
    public static int Failures(int stage) => PlayerPrefs.GetInt(Prefix + "failures." + stage, 0);
    public static bool BonusFound(int stage) => PlayerPrefs.GetInt(Prefix + "bonus." + stage, 0) == 1;
    public static void MarkBonus(int stage)
    {
        PlayerPrefs.SetInt(Prefix + "bonus." + stage, 1);
        Award("Explorer");
    }
    public static bool ClueFound(int stage) => PlayerPrefs.GetInt(Prefix + "clue." + stage, 0) == 1;
    public static int BestMedal(int stage) => PlayerPrefs.GetInt(Prefix + "medal." + stage, 0);
    public static float BestTime(int stage) => PlayerPrefs.GetFloat(Prefix + "besttime." + stage, -1f);
    public static float BestEnergy(int stage) => PlayerPrefs.GetFloat(Prefix + "bestenergy." + stage, -1f);
    public static bool Achievement(string key) => PlayerPrefs.GetInt(Prefix + "achievement." + key, 0) == 1;

    public static void SelectDifficulty(Difficulty difficulty) { SelectedDifficulty = difficulty; }
    public static void SetStageFromScene(int stage)
    {
        CurrentStage = Mathf.Clamp(stage, ShadowGameTuning.FirstStage, MaxStage);
        PlayingCampaign = true;
    }

    public static string StageScene(int stage) { return "ShadowStage" + Mathf.Clamp(stage, ShadowGameTuning.FirstStage, MaxStage); }
    public static string StageTitle(int stage)
    {
        switch (stage)
        {
            case ShadowGameTuning.FirstStage: return "THE FIRST BRIDGE";
            case ShadowGameTuning.ArchiveStage: return "THE LOST ARCHIVE";
            default: return "THE GREY WARDEN";
        }
    }

    public static void StartStage(int stage)
    {
        if (!IsUnlocked(stage)) return;
        string scene = StageScene(stage);
        if (!Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.LogError("Shadow Switch: missing " + scene + ". Use Shadow Switch > Install Campaign.");
            return;
        }
        CurrentStage = stage;
        PlayingCampaign = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }

    public static void ReturnToHub()
    {
        PlayingCampaign = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("ShadowCampaignHub");
    }

    public static void LeaveCampaign()
    {
        PlayingCampaign = false;
        Time.timeScale = 1f;
    }

    public static void AddClue(int stage)
    {
        PlayerPrefs.SetInt(Prefix + "clue." + stage, 1);
        if (ClueFound(ShadowGameTuning.FirstStage) && ClueFound(ShadowGameTuning.ArchiveStage) && ClueFound(ShadowGameTuning.FinalStage)) Award("Archivist");
        PlayerPrefs.Save();
    }

    public static void Award(string key)
    {
        PlayerPrefs.SetInt(Prefix + "achievement." + key, 1);
        PlayerPrefs.Save();
    }

    public static void Record(bool won, GameSession session, bool hintUsed, string reason)
    {
        if (!PlayingCampaign || session == null) return;
        int stage = CurrentStage;
        float elapsed = Mathf.Max(0f, session.elapsedSeconds);
        float energy = Mathf.Max(0f, session.remainingEnergy);
        int medal = won ? (hintUsed ? ShadowGameTuning.BronzeMedal : energy >= ShadowGameTuning.GoldMedalEnergyThreshold ? ShadowGameTuning.GoldMedal : ShadowGameTuning.SilverMedal) : 0;
        LastResult = new Result
        {
            exists = true,
            won = won,
            stage = stage,
            medal = medal,
            seconds = elapsed,
            energy = energy,
            crystals = session.crystalsCollected,
            usedHint = hintUsed,
            reason = reason
        };
        if (!won)
            PlayerPrefs.SetInt(Prefix + "failures." + stage, Failures(stage) + 1);
        else
        {
            Award("FirstLight");
            if (!hintUsed) Award("Independent");
            if (energy >= ShadowGameTuning.GoldMedalEnergyThreshold) Award("EnergySaver");
            if (stage == MaxStage) Award("BeaconKeeper");
            if (ShadowLiveOps.IsActive) Award("EclipseSurvivor");
            PlayerPrefs.SetInt(Prefix + "unlocked", Mathf.Max(UnlockedStage, Mathf.Min(MaxStage, stage + 1)));
            PlayerPrefs.SetInt(Prefix + "medal." + stage, Mathf.Max(BestMedal(stage), medal));
            float oldTime = BestTime(stage);
            if (oldTime < 0f || elapsed < oldTime) PlayerPrefs.SetFloat(Prefix + "besttime." + stage, elapsed);
            PlayerPrefs.SetFloat(Prefix + "bestenergy." + stage, Mathf.Max(BestEnergy(stage), energy));
        }
        PlayerPrefs.Save();
    }

    public static void SaveEnding(string ending)
    {
        Award("Ending." + ending);
    }
}
