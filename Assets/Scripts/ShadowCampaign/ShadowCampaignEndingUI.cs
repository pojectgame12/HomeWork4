using UnityEngine;

/// <summary>In-game branching ending. Twine remains a separate richer playable story artifact.</summary>
public sealed class ShadowCampaignEndingUI : MonoBehaviour
{
    private string ending;
    private string endingText;

    private void Choose(string key, string message)
    {
        ending = key;
        endingText = message;
        ShadowCampaignProgress.SaveEnding(key);
    }

    private void OnGUI()
    {
        float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        Matrix4x4 prior = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280f * scale) / 2f,
            (Screen.height - 720f * scale) / 2f, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
        GUI.Box(new Rect(120, 55, 1040, 610), "");
        GUIStyle heading = new GUIStyle(GUI.skin.label) { fontSize = 36, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        heading.normal.textColor = Color.cyan;
        GUI.Label(new Rect(180, 77, 920, 66), "THE FINAL CHOICE - BEACON CORE", heading);
        GUIStyle body = new GUIStyle(GUI.skin.label) { fontSize = 21, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        body.normal.textColor = Color.white;
        if (string.IsNullOrEmpty(ending))
        {
            GUI.Label(new Rect(200, 157, 880, 110), "GREY WARDEN: The last experiment almost destroyed the valley.\nMAYA: We can bring the light back, but choose carefully.", body);
            if (GUI.Button(new Rect(270, 292, 740, 53), "QUICK RESTART - Restore light, accept the risk"))
                Choose("Quick", "Light returns at once, but the valley's beacons flicker. The repair is incomplete.");
            bool stable = ShadowCampaignProgress.ClueFound(2) &&
                ShadowCampaignProgress.LastResult.energy >= 25f;
            GUI.enabled = stable;
            if (GUI.Button(new Rect(270, 357, 740, 53), stable ?
                "SAFE CALIBRATION - Use archive records and reserve energy" :
                "SAFE CALIBRATION - Requires stage 2 archive + 25 energy"))
                Choose("Stable", "Noor uses the calibration log. The core stabilizes and the valley's paths shine safely.");
            GUI.enabled = true;
            if (GUI.Button(new Rect(270, 422, 740, 53), "CONTROLLED LOCKDOWN - Protect the valley for now"))
                Choose("Lockdown", "The observatory remains dark, but no families are exposed to a second disaster.");
            if (ShadowCampaignProgress.ClueFound(3) &&
                GUI.Button(new Rect(270, 487, 740, 53), "ALLY WITH GREY WARDEN - Joint rehabilitation plan"))
                Choose("Guardian", "The Warden agrees to a safer restoration plan. An unlikely partnership is born.");
        }
        else
        {
            GUI.Label(new Rect(225, 210, 830, 210), ending.ToUpperInvariant() + " ENDING\n\n" + endingText, body);
            GUI.Label(new Rect(225, 435, 830, 50), "Ending saved as a local achievement. Try another path in Twine too.", body);
        }
        if (GUI.Button(new Rect(460, 580, 360, 60), "RETURN TO CAMPAIGN")) ShadowCampaignProgress.ReturnToHub();
        GUI.matrix = prior;
    }
}
