using UnityEngine;

/// <summary>Original generated sound effects and immediate audio cues (no imported audio or copyright).</summary>
[RequireComponent(typeof(AudioSource))]
public sealed class ShadowFeedback : MonoBehaviour
{
    private AudioSource source;
    private AudioClip bridgeOn;
    private AudioClip pickup;
    private LampController lamp;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.volume = .28f;
        bridgeOn = Tone("ShadowBridgeActivation", 550f, .16f);
        pickup = Tone("ShadowCrystalPickup", 820f, .14f);
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
        const int rate = 22050;
        int count = Mathf.RoundToInt(rate * seconds);
        float[] samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            float fade = 1f - (float)i / count;
            samples[i] = Mathf.Sin(2f * Mathf.PI * hz * i / rate) * fade * .34f;
        }
        AudioClip clip = AudioClip.Create(name, count, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
