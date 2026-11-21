using UnityEngine;

[CreateAssetMenu(fileName = "LevelSettings", menuName = "Shadow Switch/Level Settings")]
public sealed class LevelSettings : ScriptableObject
{
    [Header("Player physics")]
    [Min(0.1f)] public float moveSpeed = 5.3f;
    [Min(0.1f)] public float jumpSpeed = 9.8f;

    [Header("Lamp")]
    [Range(1f, 100f)] public float startingEnergy = 60f;
    [Min(0f)] public float drainPerSecond = 12f;

    [Header("Game rules")]
    [Min(10f)] public float timeLimitSeconds = 90f;
    [Min(1)] public int requiredCrystals = 1;
    [Min(0f)] public float crystalEnergyReward = 25f;
    [Min(0f)] public float swingingOrbReward = 20f;
}
