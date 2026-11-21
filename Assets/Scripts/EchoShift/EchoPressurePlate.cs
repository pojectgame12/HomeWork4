using System;
using UnityEngine;

/// <summary>Reads the active player and non-physical replay ghosts; emits only state changes.</summary>
public sealed class EchoPressurePlate : MonoBehaviour
{
    [SerializeField] private EchoLoopManager loop;
    [SerializeField] private string plateId = "A";
    [SerializeField, Min(.1f)] private float horizontalTolerance = .78f;
    private SpriteRenderer visual;

    public event Action<bool> PressedChanged;
    public bool IsPressed { get; private set; }
    public string PlateId => plateId;

    public void Configure(EchoLoopManager owner, string id)
    {
        loop = owner;
        plateId = id;
    }

    private void Awake() { visual = GetComponent<SpriteRenderer>(); }

    private void FixedUpdate()
    {
        if (loop == null || !loop.IsRunning) return;
        bool pressed = IsStanding(loop.ActivePlayer.Position);
        foreach (EchoGhost ghost in loop.Ghosts)
        {
            if (ghost != null && IsStanding(ghost.Position))
            {
                pressed = true;
                break;
            }
        }
        if (pressed == IsPressed) return;
        IsPressed = pressed;
        if (visual != null) visual.color = pressed ? new Color(.35f, 1f, .55f) : new Color(.13f, .53f, .29f);
        PressedChanged?.Invoke(pressed);
    }

    private bool IsStanding(Vector2 center)
    {
        return Mathf.Abs(center.x - transform.position.x) < horizontalTolerance
            && Mathf.Abs(center.y - (transform.position.y + .45f)) < .75f;
    }

    public void ResetState()
    {
        IsPressed = false;
        if (visual != null) visual.color = new Color(.13f, .53f, .29f);
        PressedChanged?.Invoke(false);
    }
}
