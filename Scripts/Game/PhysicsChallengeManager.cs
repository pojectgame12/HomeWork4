using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Bonus challenge: push the crate onto the switch, then reach the portal.</summary>
public sealed class PhysicsChallengeManager : MonoBehaviour
{
    [SerializeField] private PhysicsSwitch3D plate;
    private bool completed;
    private string hint = "";
    private float hintUntil;
    public void Configure(PhysicsSwitch3D newPlate) { plate = newPlate; }

    public void TryFinish()
    {
        if (plate != null && plate.IsPressed) completed = true;
        else
        {
            hint = "First push the crate onto the green pressure plate!";
            hintUntil = Time.time + 2.5f;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene("MainMenu");
        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene("PhysicsBonus3D");
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box)
        { fontSize = Mathf.Max(16, Screen.height / 35), wordWrap = true };
        string text = completed
            ? "BONUS COMPLETE! You used a 3D CharacterController and physical puzzle."
            : "PHYSICS BONUS (3D)  |  WASD: move   SPACE: jump   E: push (Raycast)   R: retry\n" +
              "Push the crate to the pressure plate, then enter the portal.\nPlate: " +
              (plate != null && plate.IsPressed ? "ACTIVE" : "INACTIVE");
        GUI.Box(new Rect(10, 10, Screen.width - 20, 115), text, style);
        if (Time.time < hintUntil)
            GUI.Box(new Rect(10, 130, Screen.width - 20, 50), hint, style);
        if (completed && GUI.Button(new Rect(Screen.width / 2f - 110f, 150f, 220f, 55f), "MAIN MENU"))
            SceneManager.LoadScene("MainMenu");
        if (GUI.Button(new Rect(10, Screen.height - 55, 160, 45), "MAIN MENU"))
            SceneManager.LoadScene("MainMenu");
    }
}
