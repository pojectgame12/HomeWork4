using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Entry point for the three-stage story campaign, progression and event refresh.</summary>
public sealed class ShadowCampaignHubUI : MonoBehaviour
{
    private const int FirstStageRowY = 290;
    private const int StageRowHeight = 86;
    private const int StageStatsOffsetY = 33;

    private bool checkingEvent;
    private Vector2 scroll;

    private void Start()
    {
        ShadowCampaignProgress.LeaveCampaign();
        ShadowLiveOps.EnsureFallback();
        RefreshEvent();
    }

    private void RefreshEvent()
    {
        if (checkingEvent) return;
        checkingEvent = true;
        StartCoroutine(ShadowLiveOps.Refresh(() => checkingEvent = false));
    }

    private void OnGUI()
    {
        float scale = Mathf.Min(Screen.width / ShadowGameTuning.ReferenceScreenWidth, Screen.height / ShadowGameTuning.ReferenceScreenHeight);
        Matrix4x4 old = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - ShadowGameTuning.ReferenceScreenWidth * scale) / 2f,
            (Screen.height - ShadowGameTuning.ReferenceScreenHeight * scale) / 2f, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
        GUI.Box(new Rect(85, 40, 1110, 640), "");
        GUIStyle title = new GUIStyle(GUI.skin.label) { fontSize = 39, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        title.normal.textColor = new Color(.40f, 1f, .88f);
        GUI.Label(new Rect(135, 52, 1010, 60), "SHADOW SWITCH - THE LAST BEACON", title);
        GUIStyle caption = new GUIStyle(GUI.skin.label) { fontSize = 21, wordWrap = true, alignment = TextAnchor.MiddleCenter };
        caption.normal.textColor = Color.white;
        GUI.Label(new Rect(180, 113, 920, 53),
            "Noor returns to the fallen observatory. Maya guides the way; the Grey Warden guards the core.", caption);
        GUI.Label(new Rect(150, 173, 980, 35), "CHOOSE DIFFICULTY (applies to all three stages)", caption);

        DrawDifficulty(new Rect(188, 210, 280, 48), ShadowCampaignProgress.Difficulty.Apprentice);
        DrawDifficulty(new Rect(500, 210, 280, 48), ShadowCampaignProgress.Difficulty.Explorer);
        DrawDifficulty(new Rect(812, 210, 280, 48), ShadowCampaignProgress.Difficulty.Keeper);

        for (int stage = ShadowGameTuning.FirstStage; stage <= ShadowGameTuning.FinalStage; stage++)
        {
            int top = FirstStageRowY + (stage - ShadowGameTuning.FirstStage) * StageRowHeight;
            bool unlocked = ShadowCampaignProgress.IsUnlocked(stage);
            int n = stage;
            string details = "Best: " + (ShadowCampaignProgress.BestTime(stage) >= 0f ?
                ShadowCampaignProgress.BestTime(stage).ToString("0.0") + "s" : "--") +
                "   Medal: " + Medal(ShadowCampaignProgress.BestMedal(stage));
            GUI.Label(new Rect(155, top, 720, 35), "STAGE " + stage + " - " + ShadowCampaignProgress.StageTitle(stage), caption);
            GUI.Label(new Rect(160, top + StageStatsOffsetY, 600, 32), details, caption);
            GUI.enabled = unlocked;
            if (GUI.Button(new Rect(885, top + 3, 225, 55), unlocked ? "PLAY STAGE " + stage : "LOCKED"))
                ShadowCampaignProgress.StartStage(n);
            GUI.enabled = true;
        }
        GUI.Label(new Rect(150, 557, 900, 29),
            "LIVE OPS: " + ShadowLiveOps.Status + " | " + ShadowLiveOps.Source, caption);
        if (GUI.Button(new Rect(135, 611, 225, 45), checkingEvent ? "CHECKING..." : "REFRESH EVENT") && !checkingEvent)
            RefreshEvent();
        if (GUI.Button(new Rect(385, 611, 225, 45), "LORE / ACHIEVEMENTS"))
            SceneManager.LoadScene("ShadowCampaignJournal");
        if (GUI.Button(new Rect(890, 611, 225, 45), "MAIN MENU"))
            SceneManager.LoadScene("MainMenu");
        GUI.matrix = old;
    }

    private void DrawDifficulty(Rect rect, ShadowCampaignProgress.Difficulty difficulty)
    {
        bool selected = ShadowCampaignProgress.SelectedDifficulty == difficulty;
        Color old = GUI.backgroundColor;
        GUI.backgroundColor = selected ? new Color(.3f, .95f, .7f) : Color.white;
        if (GUI.Button(rect, difficulty.ToString().ToUpperInvariant()))
            ShadowCampaignProgress.SelectDifficulty(difficulty);
        GUI.backgroundColor = old;
    }

    private static string Medal(int n) { return n == ShadowGameTuning.GoldMedal ? "GOLD" : n == ShadowGameTuning.SilverMedal ? "SILVER" : n == ShadowGameTuning.BronzeMedal ? "BRONZE" : "NONE"; }
}
