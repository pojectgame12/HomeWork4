using System;

/// <summary>Gameplay messages: publishers do not depend on listeners.</summary>
public static class GameEvents
{
    public static event Action<float> CrystalCollected;
    public static event Action GoalReached;
    public static event Action PlayerDied;
    public static event Action<float> LampEnergyChanged;
    public static event Action<string> StatusMessage;

    public static void RaiseCrystalCollected(float energyReward) => CrystalCollected?.Invoke(energyReward);
    public static void RaiseGoalReached() => GoalReached?.Invoke();
    public static void RaisePlayerDied() => PlayerDied?.Invoke();
    public static void RaiseLampEnergyChanged(float energy) => LampEnergyChanged?.Invoke(energy);
    public static void RaiseStatusMessage(string message) => StatusMessage?.Invoke(message);

    // Useful if Enter Play Mode Options disables domain reload.
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetListeners()
    {
        CrystalCollected = null;
        GoalReached = null;
        PlayerDied = null;
        LampEnergyChanged = null;
        StatusMessage = null;
    }
}
