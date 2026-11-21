using UnityEngine;

/// <summary>A sampled player pose stored for deterministic, fixed-step playback.</summary>
public struct EchoFrame
{
    public readonly Vector2 Position;
    public readonly bool FacingLeft;

    public EchoFrame(Vector2 position, bool facingLeft)
    {
        Position = position;
        FacingLeft = facingLeft;
    }
}
