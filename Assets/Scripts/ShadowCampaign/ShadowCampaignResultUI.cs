using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Campaign-aware result page with retry, unlock and adaptive hint assistance.</summary>
public sealed class ShadowCampaignResultUI : MonoBehaviour
{
    private void OnGUI()
    {
        float scale = Mathf.Min(Screen.width / ShadowGameTuning.ReferenceScreenWidth, Screen.height / ShadowGameTuning.ReferenceScreenHeight);
        Matrix4x4 prior = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - ShadowGameTuning.ReferenceScreenWidth * scale) / 2f,
            (Screen.height - ShadowGameTuning.ReferenceScreenHeight * scale) / 2f, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
        GUI.Box(new Rect(205, 75, 870, 585), "");
        ShadowCampaignProgress.Result r = ShadowCampaignProgress.LastResult;
        GUIStyle big = new GUIStyle(GUI.skin.label) { fontSize = 43, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        big.normal.textColor = r.won ? new Color(.45f, 1f, .8f) : new Color(1f, .63f, .56f);
        GUI.Label(new Rect(245, 95, 790, 75), r.won ? "THE BRIDGE HOLDS!" : "THE SHADOWS WON THIS ROUND", big);
        GUIStyle label = new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        label.normal.textColor = Color.white;
        if (!r.exists)
        {
            GUI.Label(new Rect(280, 190, 720, 90), "No campaign result yet. Start from the hub.", label);
            if (GUI.Button(new Rect(470, 460, 340, 60), "CAMPAIGN HUB")) ShadowCampaignProgress.ReturnToHub();
            GUI.matrix = prior; return;
        }
        string stats = "Stage " + r.stage + " | " + ShadowCampaignProgress.StageTitle(r.stage) +
            "\nTime: " + r.seconds.ToString("0.0") + "s | Energy: " + r.energy.ToString("0") +
            " | Crystals: " + r.crystals + "\nMedal: " + (r.medal == ShadowGameTuning.GoldMedal ? "GOLD" : r.medal == ShadowGameTuning.SilverMedal ? "SILVER" : r.medal == ShadowGameTuning.BronzeMedal ? "BRONZE" : "NONE");
        GUI.Label(new Rect(250, 180, 780, 135), stats, label);
        if (!r.won)
        {
            int failures = ShadowCampaignProgress.Failures(r.stage);
            GUI.Label(new Rect(250, 315, 780, 75), r.reason + "\nFailed attempts here: " + failures +
                (failures >= ShadowGameTuning.HintAfterFailures ? ". Press H during play for a hint." : ""), label);
            if (failures >= ShadowGameTuning.OfferApprenticeAfterFailures && ShadowCampaignProgress.SelectedDifficulty != ShadowCampaignProgress.Difficulty.Apprentice &&
                GUI.Button(new Rect(442, 403, 398, 48), "TRY APPRENTICE DIFFICULTY"))
                ShadowCampaignProgress.SelectDifficulty(ShadowCampaignProgress.Difficulty.Apprentice);
        }
        else GUI.Label(new Rect(250, 314, 780, 75), VictoryLine(r.stage), label);
        if (GUI.Button(new Rect(260, 515, 245, 66), "RETRY STAGE")) ShadowCampaignProgress.StartStage(r.stage);
        if (r.won)
        {
            string labelText = r.stage == ShadowCampaignProgress.MaxStage ? "CHOOSE THE ENDING" : "NEXT STAGE";
            if (GUI.Button(new Rect(517, 515, 245, 66), labelText))
            {
                if (r.stage == ShadowCampaignProgress.MaxStage) SceneManager.LoadScene("ShadowCampaignEnding");
                else ShadowCampaignProgress.StartStage(r.stage + 1);
            }
        }
        if (GUI.Button(new Rect(774, 515, 245, 66), "CAMPAIGN HUB")) ShadowCampaignProgress.ReturnToHub();
        GUI.matrix = prior;
    }

    private static string VictoryLine(int stage)
    {
        if (stage == ShadowGameTuning.FirstStage) return "MAYA: You lit the first passage. The archive is still ahead.";
        if (stage == ShadowGameTuning.ArchiveStage) return "NOOR: The safety records tell a different story. We must reach the core.";
        return "GREY WARDEN: You have reached the core. Now choose how to restore the light.";
    }
}
