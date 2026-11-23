using UnityEngine;

/// <summary>World history and achievements; collected records are persisted on the device.</summary>
public sealed class ShadowCampaignJournalUI : MonoBehaviour
{
    private void OnGUI()
    {
        float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        Matrix4x4 prior = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280f * scale) / 2f,
            (Screen.height - 720f * scale) / 2f, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
        GUI.Box(new Rect(110, 40, 1060, 635), "");
        GUIStyle head = new GUIStyle(GUI.skin.label) { fontSize = 38, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        head.normal.textColor = Color.cyan;
        GUI.Label(new Rect(160, 55, 960, 70), "OBSERVATORY ARCHIVE", head);
        GUIStyle body = new GUIStyle(GUI.skin.label) { fontSize = 21, wordWrap = true };
        body.normal.textColor = Color.white;
        GUI.Label(new Rect(165, 137, 950, 380),
            "WORLD: The Great Eclipse disabled the valley's beacons. Light crystals power the old transit network. " +
            "Engineers argued about unsafe experiments, leaving the Grey Warden to protect the sealed core.\n\n" +
            Entry(1, "FIRST LAW", "An obelisk can solidify a shadow bridge while correctly illuminated.") + "\n\n" +
            Entry(2, "SAFETY ARCHIVE", "Without calibration, an immediate restart can overload the valley.") + "\n\n" +
            Entry(3, "WARDEN RECORD", "The Warden is a safety system, not a malicious ruler.") + "\n\n" +
            "ACHIEVEMENTS: " +
            Badge("FirstLight", "First Light") + " | " + Badge("Independent", "No Hints") +
            " | " + Badge("EnergySaver", "Energy Saver") + "\n" +
            Badge("Archivist", "Archivist") + " | " + Badge("Explorer", "Secret Crystal") + " | " + Badge("BeaconKeeper", "Beacon Keeper") +
            " | " + Badge("EclipseSurvivor", "Eclipse Survivor") + "\n" +
            "Endings: " + Badge("Ending.Quick", "Quick") + ", " + Badge("Ending.Stable", "Stable") +
            ", " + Badge("Ending.Lockdown", "Lockdown") + ", " + Badge("Ending.Guardian", "Guardian"), body);
        if (GUI.Button(new Rect(445, 594, 390, 55), "BACK TO CAMPAIGN")) ShadowCampaignProgress.ReturnToHub();
        GUI.matrix = prior;
    }

    private static string Entry(int stage, string title, string info)
    {
        return "[" + (ShadowCampaignProgress.ClueFound(stage) ? "FOUND" : "LOCKED") + "] " + title + ": " +
            (ShadowCampaignProgress.ClueFound(stage) ? info : "Collect this terminal in stage " + stage + ".");
    }
    private static string Badge(string id, string display) { return (ShadowCampaignProgress.Achievement(id) ? "[X] " : "[ ] ") + display; }
}
