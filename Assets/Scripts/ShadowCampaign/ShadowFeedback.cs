using UnityEngine;

/// <summary>Original generated sound effects and immediate audio cues (no imported audio or copyright).</summary>
[RequireComponent(typeof(AudioSource))]
public sealed class ShadowFeedback : MonoBehaviour
{
    private const float EffectsVolume = 0.28f;
    private const float BridgeSoundFrequencyHz = 550f;
    private const float BridgeSoundDurationSeconds = 0.16f;
    private const float CrystalSoundFrequencyHz = 820f;
    private const float CrystalSoundDurationSeconds = 0.14f;
    private const float ToneAmplitude = 0.34f;
    private const int ToneSampleRateHz = 22050;

    private AudioSource source;
    private AudioClip bridgeOn;
    private AudioClip pickup;
    private LampController lamp;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.volume = EffectsVolume;
        bridgeOn = Tone("ShadowBridgeActivation", BridgeSoundFrequencyHz, BridgeSoundDurationSeconds);
        pickup = Tone("ShadowCrystalPickup", CrystalSoundFrequencyHz, CrystalSoundDurationSeconds);
    }

    private void Start()
    {
        lamp = FindFirstObjectByType<LampController>();
        if (lamp != null) lamp.BridgeConditionsChanged += BridgeChanged;
        GameEvents.CrystalCollected += CrystalCollected;
    }

    private void BridgeChanged(bool active)
    {
        if (active && source != null) source.PlayOneShot(bridgeOn);
    }
    private void CrystalCollected(float _) { if (source != null) source.PlayOneShot(pickup); }

    private void OnDestroy()
    {
        if (lamp != null) lamp.BridgeConditionsChanged -= BridgeChanged;
        GameEvents.CrystalCollected -= CrystalCollected;
        if (bridgeOn != null) Destroy(bridgeOn);
        if (pickup != null) Destroy(pickup);
    }

    private static AudioClip Tone(string name, float hz, float seconds)
    {
        int rate = ToneSampleRateHz;
        int count = Mathf.RoundToInt(rate * seconds);
        float[] samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            float fade = 1f - (float)i / count;
            samples[i] = Mathf.Sin(2f * Mathf.PI * hz * i / rate) * fade * ToneAmplitude;
        }
        AudioClip clip = AudioClip.Create(name, count, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
