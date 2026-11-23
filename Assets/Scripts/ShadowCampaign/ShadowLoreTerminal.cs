using UnityEngine;

/// <summary>Optional environmental storytelling collectible; persists lore in PlayerPrefs.</summary>
[RequireComponent(typeof(Collider2D))]
public sealed class ShadowLoreTerminal : MonoBehaviour
{
    private const float TerminalPulseSpeed = 2f;

    public int stage = 1;
    private float phase;
    private SpriteRenderer visual;
    private bool triggered;

    private void Awake() { visual = GetComponent<SpriteRenderer>(); }

    private void Update()
    {
        phase += Time.deltaTime * TerminalPulseSpeed;
        if (visual != null) visual.color = Color.Lerp(new Color(.15f, .70f, .9f),
            Color.white, .5f + .5f * Mathf.Sin(phase));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || other.GetComponent<PlayerController>() == null) return;
        triggered = true;
        ShadowCampaignProgress.AddClue(stage);
        if (ShadowCampaignRuntime.Current != null)
            ShadowCampaignRuntime.Current.ShowLore("ARCHIVE", Lore());
        GameEvents.RaiseStatusMessage("Lore entry " + stage + "/" + ShadowCampaignProgress.MaxStage + " recovered!");
        if (visual != null) visual.enabled = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    public string Lore()
    {
        switch (stage)
        {
            case 1: return "Shadows solidify only when the beam reaches a designated obelisk.";
            case 2: return "Safety calibration prevents the core from overloading the valley.";
            default: return "The Grey Warden was built to protect the valley, not to imprison it.";
        }
    }
}
