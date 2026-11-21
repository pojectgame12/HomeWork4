using UnityEngine;

/// <summary>Collectible publishes one message; it does not know the lamp or the session.</summary>
[RequireComponent(typeof(Collider2D))]
public sealed class CrystalPickup : MonoBehaviour
{
    public float energyReward = 25f;
    private Vector3 startPosition;
    private bool collected;

    private void Start()
    {
        startPosition = transform.position;
        LevelController level = FindFirstObjectByType<LevelController>();
        if (level != null && level.Settings != null)
            energyReward = level.Settings.crystalEnergyReward;
    }

    private void Update()
    {
        float bob = Mathf.Sin(Time.time * 2.8f) * .12f;
        transform.position = startPosition + new Vector3(0f, bob, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || other.GetComponent<PlayerController>() == null) return;
        collected = true;
        GameEvents.RaiseCrystalCollected(energyReward);
        Destroy(gameObject);
    }
}
