using UnityEngine;

/// <summary>Responds to lamp change notifications instead of polling every frame.</summary>
[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public sealed class ShadowBridge : MonoBehaviour
{
    public LampController lamp;
    public bool IsSolid { get; private set; }

    private BoxCollider2D bridgeCollision;
    private SpriteRenderer visual;

    private void Awake()
    {
        bridgeCollision = GetComponent<BoxCollider2D>();
        visual = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (lamp == null) lamp = FindFirstObjectByType<LampController>();
        if (lamp != null) lamp.BridgeConditionsChanged += ApplyState;
        ApplyState(lamp != null && lamp.BridgeConditionsMet);
    }

    private void OnDisable()
    {
        if (lamp != null) lamp.BridgeConditionsChanged -= ApplyState;
    }

    private void ApplyState(bool solid)
    {
        IsSolid = solid;
        bridgeCollision.enabled = solid;
        visual.color = solid ? Color.white : new Color(.65f, .55f, .95f, .17f);
    }
}
