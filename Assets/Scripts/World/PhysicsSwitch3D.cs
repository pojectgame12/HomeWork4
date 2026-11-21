using System;
using UnityEngine;

/// <summary>Pressure plate only reacts to its specifically assigned crate rigidbody.</summary>
[RequireComponent(typeof(BoxCollider))]
public sealed class PhysicsSwitch3D : MonoBehaviour
{
    [SerializeField] private Rigidbody targetCrate;
    public bool IsPressed { get; private set; }
    public event Action<bool> PressedChanged;

    public void Configure(Rigidbody crate) { targetCrate = crate; }
    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == targetCrate) SetPressed(true);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody == targetCrate) SetPressed(true);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody == targetCrate) SetPressed(false);
    }
    private void SetPressed(bool value)
    {
        if (IsPressed == value) return;
        IsPressed = value;
        PressedChanged?.Invoke(value);
    }
}
