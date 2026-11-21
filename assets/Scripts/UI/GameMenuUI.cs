using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameMenuUI : MonoBehaviour
{
    public enum MenuType { Main, Victory, GameOver }
    public MenuType menuType;

    private void OnGUI()
    {
        float s = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * s) * .5f,
                                                (Screen.height - 720 * s) * .5f, 0f),
                                    Quaternion.identity, new Vector3(s, s, 1f));
        GUI.Box(new Rect(270, 105, 740, 500), "");
        GUIStyle title = new GUIStyle(GUI.skin.label)
        { fontSize = 52, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        title.normal.textColor = new Color(.35f, .94f, .90f);
        GUIStyle body = new GUIStyle(GUI.skin.label)
        { fontSize = 23, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        body.normal.textColor = Color.white;
        GUIStyle button = new GUIStyle(GUI.skin.button)
        { fontSize = 25, fontStyle = FontStyle.Bold };

        GameSession session = GameSession.Instance;
        if (menuType == MenuType.Main)
        {
            GUI.Label(new Rect(320, 130, 640, 85), "SHADOW SWITCH", title);
            GUI.Label(new Rect(340, 218, 600, 90),
                      "LEVEL 2  -  FADING LIGHT\nCreate the bridge, collect crystals, reach the exit.", body);
            if (GUI.Button(new Rect(460, 310, 360, 65), "PLAY LEVEL 2", button))
                SceneManager.LoadScene("Level02");
            if (GUI.Button(new Rect(460, 392, 360, 65), "PHYSICS BONUS (3D)", button))
            {
                if (Application.CanStreamedLevelBeLoaded("PhysicsBonus3D"))
                    SceneManager.LoadScene("PhysicsBonus3D");
                else Debug.LogWarning("Run Homework 4 > Upgrade Existing Project first.");
            }
            GUI.Label(new Rect(350, 482, 580, 85),
                      "Desktop: A/D, SPACE, E, J/L, R\nMobile: touch the on-screen buttons", body);
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
}
