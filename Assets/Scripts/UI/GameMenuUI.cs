using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Homework 4 menu retained, with an additional optional Shadow Switch story campaign.</summary>
public sealed class GameMenuUI : MonoBehaviour
{
    public enum MenuType { Main, Victory, GameOver }
    public MenuType menuType;

    private void OnGUI()
    {
        float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280f * scale) * .5f,
            (Screen.height - 720f * scale) * .5f, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
        GUI.Box(new Rect(270, 105, 740, 500), "");
        GUIStyle title = new GUIStyle(GUI.skin.label) { fontSize = 52, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        title.normal.textColor = new Color(.35f, .94f, .90f);
        GUIStyle body = new GUIStyle(GUI.skin.label) { fontSize = 23, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        body.normal.textColor = Color.white;
        GUIStyle button = new GUIStyle(GUI.skin.button) { fontSize = 25, fontStyle = FontStyle.Bold };
        GameSession session = GameSession.Instance;

        if (menuType == MenuType.Main)
        {
            GUI.Label(new Rect(320, 125, 640, 76), "HOMEWORK 4", title);
            GUI.Label(new Rect(330, 195, 620, 59), "Original games + Shadow Switch story campaign", body);
            if (GUI.Button(new Rect(455, 259, 370, 57), "SHADOW SWITCH: STORY", button))
                OpenIfAvailable("ShadowCampaignHub");
            if (GUI.Button(new Rect(455, 327, 370, 57), "SHADOW SWITCH: CLASSIC", button))
            {
                ShadowCampaignProgress.LeaveCampaign();
                SceneManager.LoadScene("Level02");
            }
            if (GUI.Button(new Rect(455, 395, 370, 57), "PHYSICS BONUS (3D)", button))
                OpenIfAvailable("PhysicsBonus3D");
            if (GUI.Button(new Rect(455, 463, 370, 57), "ECHO SHIFT", button))
                OpenIfAvailable("EchoShift");
            GUI.Label(new Rect(335, 530, 610, 55), "R: restart   |   ESC: return to menu", body);
        }
        else if (menuType == MenuType.Victory)
        {
            GUI.Label(new Rect(320, 130, 640, 85), "LEVEL COMPLETE!", title);
            GUI.Label(new Rect(320, 225, 640, 110),
                "Crystals: " + session.crystalsCollected + "   Energy left: " +
                session.remainingEnergy.ToString("0") + "%\nTime: " +
                session.elapsedSeconds.ToString("0.0") + "s   Attempts: " + session.totalAttempts, body);
            if (GUI.Button(new Rect(470, 350, 340, 65), "PLAY AGAIN", button))
                SceneManager.LoadScene("Level02");
            if (GUI.Button(new Rect(470, 442, 340, 65), "MAIN MENU", button))
                SceneManager.LoadScene("MainMenu");
        }
        else
        {
            GUI.Label(new Rect(320, 130, 640, 85), "GAME OVER", title);
            GUI.Label(new Rect(320, 225, 640, 110),
                "The player fell, or time ran out.\nAim the light before crossing the bridge!", body);
            if (GUI.Button(new Rect(470, 350, 340, 65), "RETRY", button))
                SceneManager.LoadScene("Level02");
            if (GUI.Button(new Rect(470, 442, 340, 65), "MAIN MENU", button))
                SceneManager.LoadScene("MainMenu");
        }
        GUI.matrix = Matrix4x4.identity;
    }

    private static void OpenIfAvailable(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else Debug.LogWarning("Missing scene " + sceneName + ". Run Shadow Switch > Install Campaign if needed.");
    }
}
