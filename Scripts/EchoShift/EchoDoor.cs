using UnityEngine;

/// <summary>Unlocks only while both pressure plates are active.</summary>
[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public sealed class EchoDoor : MonoBehaviour
{
    [SerializeField] private EchoPressurePlate firstPlate;
    [SerializeField] private EchoPressurePlate secondPlate;
    private BoxCollider2D doorCollider;
    private SpriteRenderer visual;

    public bool IsOpen { get; private set; }

    public void Configure(EchoPressurePlate a, EchoPressurePlate b)
    {
        firstPlate = a;
        secondPlate = b;
    }

    private void Awake()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        visual = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (firstPlate != null) firstPlate.PressedChanged += PlateChanged;
        if (secondPlate != null) secondPlate.PressedChanged += PlateChanged;
    }

    private void Start() { Refresh(); }

    private void OnDisable()
    {
        if (firstPlate != null) firstPlate.PressedChanged -= PlateChanged;
        if (secondPlate != null) secondPlate.PressedChanged -= PlateChanged;
    }

    private void PlateChanged(bool _) { Refresh(); }

    public void Refresh()
    {
        IsOpen = firstPlate != null && secondPlate != null && firstPlate.IsPressed && secondPlate.IsPressed;
        doorCollider.enabled = !IsOpen;
        visual.color = IsOpen ? new Color(.35f, .95f, 1f, .16f) : new Color(.95f, .29f, .39f, .95f);
    }
}
