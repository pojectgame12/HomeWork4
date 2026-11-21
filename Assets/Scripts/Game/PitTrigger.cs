using UnityEngine;

public sealed class PitTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
            GameEvents.RaisePlayerDied();
    }
}
