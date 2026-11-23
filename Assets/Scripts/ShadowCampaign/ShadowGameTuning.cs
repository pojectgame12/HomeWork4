/// <summary>
/// Named gameplay settings shared by Shadow Switch. Keep values unchanged when refactoring
/// to avoid affecting saved progress, difficulty balance, or remote Live Ops deltas.
/// This is a set of fixed defaults; Unity Inspector values and live JSON still take priority.
/// </summary>
public static class ShadowGameTuning
{
    public const int FirstStage = 1;
    public const int ArchiveStage = 2;
    public const int FinalStage = 3;
    public const int InitialRequiredCrystals = 1;
    public const int LaterRequiredCrystals = 2;
    public const int HintAfterFailures = 2;
    public const int OfferApprenticeAfterFailures = 3;

    public const float BaseStartingEnergy = 60f;
    public const float BaseDrainPerSecond = 12f;
    public const float BaseTimeLimit = 90f;
    public const float BaseCrystalEnergyReward = 25f;
    public const float MaximumEnergy = 100f;

    public const float ApprenticeStartingEnergy = 80f;
    public const float ApprenticeDrainPerSecond = 8f;
    public const float ApprenticeTimeLimit = 120f;
    public const float KeeperStartingEnergy = 45f;
    public const float KeeperDrainPerSecond = 16f;
    public const float KeeperTimeLimit = 75f;

    public const float GoldMedalEnergyThreshold = 35f;
    public const float StableEndingEnergyThreshold = 25f;
    public const int BronzeMedal = 1;
    public const int SilverMedal = 2;
    public const int GoldMedal = 3;

    public const float ReferenceScreenWidth = 1280f;
    public const float ReferenceScreenHeight = 720f;
}
