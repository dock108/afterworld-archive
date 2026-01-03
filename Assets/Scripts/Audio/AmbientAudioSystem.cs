using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Generates lightweight ambient audio clips and routes them through an audio mixer.
/// </summary>
public class AmbientAudioSystem : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup ambienceGroup;
    [SerializeField, Range(0f, 1f)] private float masterVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float ambienceVolume = 0.6f;
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [SerializeField] private string ambienceVolumeParam = "AmbienceVolume";

    [Header("Source Levels")]
    [SerializeField, Range(0f, 1f)] private float windSourceVolume = 0.18f;
    [SerializeField, Range(0f, 1f)] private float humSourceVolume = 0.1f;

    [Header("Loop Settings")]
    [SerializeField, Min(1f)] private float windLoopSeconds = 6f;
    [SerializeField, Min(1f)] private float humLoopSeconds = 4f;
    [SerializeField, Min(8000)] private int sampleRate = 48000;

    private AudioSource windSource;
    private AudioSource humSource;

    private void Awake()
    {
        CreateSources();
        ApplyMixerVolumes();
    }

    private void Start()
    {
        PlayAmbience();
    }

    private void OnValidate()
    {
        // Keep inspector adjustments reflected in the live mixer when possible.
        ApplyMixerVolumes();
        UpdateSourceVolumes();
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyMixerVolumes();
    }

    public void SetAmbienceVolume(float volume)
    {
        ambienceVolume = Mathf.Clamp01(volume);
        ApplyMixerVolumes();
    }

    private void CreateSources()
    {
        windSource = CreateLoopSource("Ambient Wind", windSourceVolume, CreateWindClip());
        humSource = CreateLoopSource("Ambient Hum", humSourceVolume, CreateHumClip());
    }

    private AudioSource CreateLoopSource(string name, float volume, AudioClip clip)
    {
        var sourceObject = new GameObject(name);
        sourceObject.transform.SetParent(transform, false);

        var source = sourceObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;
        source.outputAudioMixerGroup = ambienceGroup;

        return source;
    }

    private void PlayAmbience()
    {
        if (windSource != null && !windSource.isPlaying)
        {
            windSource.Play();
        }

        if (humSource != null && !humSource.isPlaying)
        {
            humSource.Play();
        }
    }

    private void UpdateSourceVolumes()
    {
        if (windSource != null)
        {
            windSource.volume = windSourceVolume;
        }

        if (humSource != null)
        {
            humSource.volume = humSourceVolume;
        }
    }

    private void ApplyMixerVolumes()
    {
        if (mixer == null)
        {
            return;
        }

        // Unity mixers expect decibel values, so convert linear sliders.
        mixer.SetFloat(masterVolumeParam, ToDecibels(masterVolume));
        mixer.SetFloat(ambienceVolumeParam, ToDecibels(ambienceVolume));
    }

    private static float ToDecibels(float value)
    {
        return Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
    }

    private AudioClip CreateWindClip()
    {
        int lengthSamples = Mathf.CeilToInt(windLoopSeconds * sampleRate);
        float[] data = new float[lengthSamples];

        // Layer multiple sines and a slow LFO to approximate turbulent wind.
        var random = new System.Random(48291);
        int layers = 6;
        float[] frequencies = new float[layers];
        float[] phases = new float[layers];
        float[] amplitudes = new float[layers];

        for (int i = 0; i < layers; i++)
        {
            frequencies[i] = Mathf.Lerp(60f, 420f, (float)random.NextDouble());
            phases[i] = (float)random.NextDouble() * Mathf.PI * 2f;
            amplitudes[i] = Mathf.Lerp(0.02f, 0.08f, (float)random.NextDouble());
        }

        float lfoFrequency = 0.12f;
        float lfoPhase = (float)random.NextDouble() * Mathf.PI * 2f;

        for (int i = 0; i < lengthSamples; i++)
        {
            float t = i / (float)sampleRate;
            float sample = 0f;

            for (int j = 0; j < layers; j++)
            {
                sample += Mathf.Sin((t * frequencies[j] + phases[j]) * Mathf.PI * 2f) * amplitudes[j];
            }

            float lfo = 0.7f + 0.3f * Mathf.Sin((t * lfoFrequency + lfoPhase) * Mathf.PI * 2f);
            data[i] = sample * lfo;
        }

        var clip = AudioClip.Create("AmbientWind", lengthSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateHumClip()
    {
        int lengthSamples = Mathf.CeilToInt(humLoopSeconds * sampleRate);
        float[] data = new float[lengthSamples];

        // Build a subtle tonal bed with gentle detune and harmonics.
        float baseFrequency = 110f;
        float detuneFrequency = 0.6f;
        float detuneDepth = 1.8f;

        for (int i = 0; i < lengthSamples; i++)
        {
            float t = i / (float)sampleRate;
            float detune = Mathf.Sin(t * detuneFrequency * Mathf.PI * 2f) * detuneDepth;
            float carrier = Mathf.Sin((baseFrequency + detune) * t * Mathf.PI * 2f) * 0.12f;
            float harmonic = Mathf.Sin((baseFrequency * 2f + detune * 0.5f) * t * Mathf.PI * 2f) * 0.05f;
            float shimmer = Mathf.Sin((baseFrequency * 3.5f) * t * Mathf.PI * 2f) * 0.015f;
            data[i] = carrier + harmonic + shimmer;
        }

        var clip = AudioClip.Create("AmbientHum", lengthSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
