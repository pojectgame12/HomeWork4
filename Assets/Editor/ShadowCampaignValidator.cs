#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Editor-side structural validation; NOT a substitute for Play/WebGL tests.</summary>
public static class ShadowCampaignValidator
{
    [MenuItem("Shadow Switch/4 - Validate Campaign Scenes")]
    public static void Validate()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Exit Play mode before running campaign validation.");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        string original = SceneManager.GetActiveScene().path;
        int errors = 0;
        for (int i = 1; i <= 3; i++)
        {
            string path = "Assets/Scenes/ShadowStage" + i + ".unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            {
                Debug.LogError("Missing stage: " + path); errors++; continue;
            }
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            LevelController controller = Object.FindFirstObjectByType<LevelController>();
            ShadowCampaignRuntime runtime = Object.FindFirstObjectByType<ShadowCampaignRuntime>();
            if (controller == null || controller.Settings == null)
            {
                Debug.LogError(path + ": missing LevelController or LevelSettings"); errors++;
            }
            if (runtime == null || runtime.stage != i)
            {
                Debug.LogError(path + ": missing/wrong campaign runtime"); errors++;
            }
            if (Object.FindFirstObjectByType<LampController>() == null ||
                Object.FindFirstObjectByType<ShadowBridge>() == null ||
                Object.FindFirstObjectByType<PlayerController>() == null)
            {
                Debug.LogError(path + ": base mechanics missing"); errors++;
            }
            if (Object.FindFirstObjectByType<ShadowLoreTerminal>() == null)
            {
                Debug.LogError(path + ": lore terminal missing"); errors++;
            }
            if (i >= 2 && GameObject.Find("Campaign_SecondCrystal") == null)
            {
                Debug.LogError(path + ": second required crystal missing"); errors++;
            }
            if (i == 3 && Object.FindFirstObjectByType<ShadowPatrolHazard>() == null)
            {
                Debug.LogError(path + ": moving hazard missing"); errors++;
            }
        }
        string[] ui = { "ShadowCampaignHub", "ShadowCampaignResult", "ShadowCampaignJournal", "ShadowCampaignEnding" };
        foreach (string name in ui)
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/" + name + ".unity") == null)
            { Debug.LogError("Missing campaign UI scene: " + name); errors++; }
        if (!string.IsNullOrEmpty(original) && AssetDatabase.LoadAssetAtPath<SceneAsset>(original) != null)
            EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
        Debug.Log(errors == 0 ?
            "Shadow Switch: Structural checks PASSED. Next run all manual gameplay/LiveOps/WebGL tests." :
            "Shadow Switch: Structural checks found " + errors + " issue(s). Inspect red Console entries.");
    }
}
#endif
