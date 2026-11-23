using UnityEngine;

/// <summary>Stage-three timed hazard; one visible, predictable horizontal patrol.</summary>
[RequireComponent(typeof(CircleCollider2D))]
public sealed class ShadowPatrolHazard : MonoBehaviour
{
    public float centerX = -.25f;
    public float patrolRadius = 1.65f;
    public float speed = 1.75f;
    private Vector3 origin;

    private void Start() { origin = transform.position; }
    private void Update()
    {
        transform.position = new Vector3(centerX + Mathf.Sin(Time.time * speed) * patrolRadius,
            origin.y, origin.z);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null && !GameSession.Instance.IsTransitioning)
            GameEvents.RaisePlayerDied();
    }
}
