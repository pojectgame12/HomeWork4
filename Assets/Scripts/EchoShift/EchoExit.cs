using UnityEngine;

/// <summary>The active player, not a replay, may finish the level.</summary>
[RequireComponent(typeof(Collider2D))]
public sealed class EchoExit : MonoBehaviour
{
    [SerializeField] private EchoLoopManager loop;
    [SerializeField] private EchoDoor door;

    public void Configure(EchoLoopManager owner, EchoDoor gate)
    {
        loop = owner;
        door = gate;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (loop == null || !loop.IsRunning || door == null || !door.IsOpen) return;
        if (other.GetComponent<EchoPlayer>() != null)
            loop.Win();
    }
}
