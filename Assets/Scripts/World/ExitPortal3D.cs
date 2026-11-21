using UnityEngine;

/// <summary>Independent 3D trigger that notifies the local challenge manager.</summary>
[RequireComponent(typeof(Collider))]
public sealed class ExitPortal3D : MonoBehaviour
{
    [SerializeField] private PhysicsChallengeManager manager;
    public void Configure(PhysicsChallengeManager value) { manager = value; }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController3D>() != null && manager != null)
            manager.TryFinish();
    }
}
