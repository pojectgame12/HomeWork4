using System.Collections.Generic;
using UnityEngine;

/// <summary>Visual replay. Ghosts do not block the active player.</summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class EchoGhost : MonoBehaviour
{
    private List<EchoFrame> recording;
    private SpriteRenderer sprite;

    public Vector2 Position => transform.position;

    public void Initialize(IReadOnlyList<EchoFrame> samples, Sprite sourceSprite, Color tint)
    {
        sprite = GetComponent<SpriteRenderer>();
        sprite.sprite = sourceSprite;
        sprite.color = tint;
        sprite.sortingOrder = 12;
        recording = new List<EchoFrame>(samples);
        SetFrame(0);
    }

    public void SetFrame(int frameIndex)
    {
        if (recording == null || recording.Count == 0) return;
        EchoFrame frame = recording[Mathf.Clamp(frameIndex, 0, recording.Count - 1)];
        transform.position = new Vector3(frame.Position.x, frame.Position.y, 0f);
        sprite.flipX = frame.FacingLeft;
    }
}
