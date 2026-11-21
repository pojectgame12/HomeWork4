using UnityEngine;

/// <summary>Dynamic Rigidbody2D collectible hung from a DistanceJoint2D.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(DistanceJoint2D))]
public sealed class SwingingEnergyOrb : MonoBehaviour
{
    [Min(0f)] public float energyReward = 20f;
    private bool collected;

    private void Start()
    {
        LevelController level = FindFirstObjectByType<LevelController>();
        if (level != null && level.Settings != null)
            energyReward = level.Settings.swingingOrbReward;
        // A small impulse demonstrates physical swinging around the fixed joint anchor.
        GetComponent<Rigidbody2D>().AddForce(Vector2.right * .45f, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collected || collision.gameObject.GetComponent<PlayerController>() == null) return;
        collected = true;
        GameEvents.RaiseCrystalCollected(energyReward);
        Destroy(gameObject);
    }
}
