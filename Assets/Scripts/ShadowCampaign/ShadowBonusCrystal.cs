using UnityEngine;

/// <summary>Optional extra treasure. Never required to open the exit.</summary>
[RequireComponent(typeof(Collider2D))]
public sealed class ShadowBonusCrystal : MonoBehaviour
{
    private const float SpinDegreesPerSecond = 65f;
    private const float BobFrequency = 2.8f;
    private const float BobAmplitudePerFrame = 0.0025f;

    public int stage = 1;
    private bool taken;
    private void Update()
    {
        transform.Rotate(0f, 0f, SpinDegreesPerSecond * Time.deltaTime);
        transform.position += Vector3.up * Mathf.Cos(Time.time * BobFrequency) * BobAmplitudePerFrame;
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
