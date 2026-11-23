using UnityEngine;

/// <summary>Optional extra treasure. Never required to open the exit.</summary>
[RequireComponent(typeof(Collider2D))]
public sealed class ShadowBonusCrystal : MonoBehaviour
{
    public int stage = 1;
    private bool taken;
    private void Update()
    {
        transform.Rotate(0f, 0f, 65f * Time.deltaTime);
        transform.position += Vector3.up * Mathf.Cos(Time.time * 2.8f) * .0025f;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (taken || other.GetComponent<PlayerController>() == null) return;
        taken = true;
        ShadowCampaignProgress.MarkBonus(stage);
        GameEvents.RaiseStatusMessage("SECRET CRYSTAL! Bonus explorer badge unlocked.");
        Destroy(gameObject);
    }
}
