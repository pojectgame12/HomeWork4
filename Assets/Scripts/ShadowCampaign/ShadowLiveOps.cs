using System;
using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public sealed class ShadowEventParameters
{
    private const float DefaultEventDrainPerSecond = 15f;
    private const float DefaultEventTimeLimit = 80f;
    private const float DefaultEventCrystalReward = 30f;
    public float startingEnergy = ShadowGameTuning.BaseStartingEnergy;
    public float drainPerSecond = DefaultEventDrainPerSecond;
    public float timeLimitSeconds = DefaultEventTimeLimit;
    public float crystalEnergyReward = DefaultEventCrystalReward;
}

[Serializable]
public sealed class ShadowEventConfig
{
    public int schemaVersion;
    public string eventId;
    public string title;
    public bool enabled;
    public string startsAtUtc;
    public string endsAtUtc;
    public ShadowEventParameters parameters;
    public bool applyOnNextLevelStart;
}

/// <summary>
/// Remote JSON fetched in the hub, with validated local fallback; applied at NEXT stage start only.
/// Host liveops/eclipse-night.json as a raw HTTPS file to change the event without a new build.
/// </summary>
public static class ShadowLiveOps
{
    private const int JsonSchemaVersion = 1;
    private const float MinimumStartingEnergy = 1f;
    private const float MinimumDrainRate = 0f;
    private const float MaximumDrainRate = 35f;
    private const float MinimumTimeLimitSeconds = 20f;
    private const float MaximumTimeLimitSeconds = 300f;
    private const float MinimumCrystalReward = 0f;
    private const int RemoteRequestTimeoutSeconds = 5;

    // The repo path becomes valid after the file is uploaded. This URL can be changed in source if needed.
    public const string RemoteUrl =
        "https://raw.githubusercontent.com/pojectgame12/HomeWork4/main/liveops/eclipse-night.json";

    private static ShadowEventConfig current;
    private static bool initialized;
    public static string Source { get; private set; } = "Built-in default";
    public static string Status { get; private set; } = "Not loaded";

    public static bool IsActive => ActiveNow(current);
    public static string EventTitle => IsActive && current != null ? current.title : "No active event";

    public static void EnsureFallback()
    {
        if (initialized) return;
        initialized = true;
        TextAsset bundled = Resources.Load<TextAsset>("ShadowCampaign/eclipse-night");
        if (bundled == null || !TryParse(bundled.text, out current))
        {
            current = null;
            Source = "Default gameplay";
            Status = "No valid bundled event; base settings active";
        }
        else
        {
            Source = "Bundled fallback";
            Status = IsActive ? "Eclipse Night active" : "No event in current UTC window";
        }
    }

    private static bool TryParse(string json, out ShadowEventConfig parsed)
    {
        parsed = null;
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            ShadowEventConfig candidate = JsonUtility.FromJson<ShadowEventConfig>(json);
            if (candidate == null || candidate.schemaVersion != JsonSchemaVersion || candidate.eventId != "eclipse-night" ||
                candidate.parameters == null || !candidate.applyOnNextLevelStart) return false;
            DateTimeOffset start, end;
            if (!ParseUtc(candidate.startsAtUtc, out start) || !ParseUtc(candidate.endsAtUtc, out end) || end <= start) return false;
            if (candidate.parameters.startingEnergy < MinimumStartingEnergy || candidate.parameters.startingEnergy > ShadowGameTuning.MaximumEnergy ||
                candidate.parameters.drainPerSecond < MinimumDrainRate || candidate.parameters.drainPerSecond > MaximumDrainRate ||
                candidate.parameters.timeLimitSeconds < MinimumTimeLimitSeconds || candidate.parameters.timeLimitSeconds > MaximumTimeLimitSeconds ||
                candidate.parameters.crystalEnergyReward < MinimumCrystalReward || candidate.parameters.crystalEnergyReward > ShadowGameTuning.MaximumEnergy) return false;
            parsed = candidate;
            return true;
        }
        catch (Exception) { return false; }
    }

    private static bool ParseUtc(string s, out DateTimeOffset value)
    {
        return DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out value);
    }

    private static bool ActiveNow(ShadowEventConfig config)
    {
        if (config == null || !config.enabled) return false;
        DateTimeOffset a, b;
        return ParseUtc(config.startsAtUtc, out a) && ParseUtc(config.endsAtUtc, out b) &&
               DateTimeOffset.UtcNow >= a && DateTimeOffset.UtcNow <= b;
    }

    public static IEnumerator Refresh(Action onFinished)
    {
        EnsureFallback();
        Status = "Checking local and remote configuration...";

#if UNITY_EDITOR || (!UNITY_WEBGL && !UNITY_ANDROID && !UNITY_IOS)
        string localPath = Path.Combine(Application.streamingAssetsPath, "ShadowSwitch/eclipse-night.json");
        if (File.Exists(localPath))
        {
            ShadowEventConfig local;
            try
            {
                string json = File.ReadAllText(localPath);
                if (TryParse(json, out local))
                {
                    current = local;
                    Source = "Local StreamingAssets (reloadable)";
                }
            }
            catch (Exception e) { Debug.LogWarning("Shadow Live Ops local: " + e.Message); }
        }
#else
        string localUrl = Application.streamingAssetsPath.TrimEnd('/') + "/ShadowSwitch/eclipse-night.json";
        using (UnityWebRequest localReq = UnityWebRequest.Get(localUrl))
        {
            localReq.timeout = RemoteRequestTimeoutSeconds;
            yield return localReq.SendWebRequest();
            if (localReq.result == UnityWebRequest.Result.Success)
            {
                ShadowEventConfig local;
                if (TryParse(localReq.downloadHandler.text, out local))
                {
                    current = local;
                    Source = "Local StreamingAssets";
                }
            }
        }
#endif
        using (UnityWebRequest request = UnityWebRequest.Get(RemoteUrl + "?check=" + DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
        {
            request.timeout = RemoteRequestTimeoutSeconds;
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                ShadowEventConfig remote;
                if (TryParse(request.downloadHandler.text, out remote))
                {
                    current = remote;
                    Source = "Remote GitHub JSON";
                }
                else Debug.LogWarning("Shadow Live Ops: invalid remote JSON; retaining local fallback.");
            }
            else Debug.Log("Shadow Live Ops: remote unavailable (" + request.responseCode + "); retaining fallback.");
        }
        Status = IsActive ? "ACTIVE: " + EventTitle : "No active event (UTC/date/enable check)";
        if (onFinished != null) onFinished();
    }

    public static void ApplyAtLevelStart(LevelSettings settings)
    {
        EnsureFallback();
        if (!IsActive || settings == null) return;
        // Delta-based modifiers preserve the differences between difficulty profiles.
        settings.startingEnergy = Mathf.Clamp(settings.startingEnergy + (current.parameters.startingEnergy - ShadowGameTuning.BaseStartingEnergy), MinimumStartingEnergy, ShadowGameTuning.MaximumEnergy);
        settings.drainPerSecond = Mathf.Max(0f, settings.drainPerSecond + (current.parameters.drainPerSecond - ShadowGameTuning.BaseDrainPerSecond));
        settings.timeLimitSeconds = Mathf.Max(MinimumTimeLimitSeconds, settings.timeLimitSeconds + (current.parameters.timeLimitSeconds - ShadowGameTuning.BaseTimeLimit));
        settings.crystalEnergyReward = Mathf.Clamp(settings.crystalEnergyReward + (current.parameters.crystalEnergyReward - ShadowGameTuning.BaseCrystalEnergyReward), MinimumCrystalReward, ShadowGameTuning.MaximumEnergy);
    }
}
