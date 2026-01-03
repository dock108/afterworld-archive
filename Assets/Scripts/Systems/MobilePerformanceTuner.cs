using UnityEngine;

/// <summary>
/// Applies mobile-focused performance settings for consistent frame pacing and memory pressure.
/// </summary>
public class MobilePerformanceTuner : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField] private bool applyOnMobileOnly = true;

    [Header("Frame Consistency")]
    [SerializeField, Range(30, 120)] private int targetFrameRate = 60;
    [SerializeField] private bool disableVSync = true;
    [SerializeField] private bool alignFixedDeltaTime = true;

    [Header("LOD")]
    [SerializeField, Range(0.2f, 2f)] private float lodBias = 0.6f;
    [SerializeField, Range(0, 2)] private int maximumLodLevel = 1;

    [Header("Lighting")]
    [SerializeField] private bool disableRealtimeShadows = true;
    [SerializeField, Range(0f, 80f)] private float shadowDistance = 25f;
    [SerializeField, Range(0, 4)] private int pixelLightCount = 1;

    [Header("Texture Streaming")]
    [SerializeField] private bool enableTextureStreaming = true;
    [SerializeField, Range(64, 1024)] private int streamingMipmapsMemoryBudget = 256;
    [SerializeField, Range(16, 512)] private int streamingMipmapsRenderersPerFrame = 128;
    [SerializeField, Range(1, 4)] private int streamingMipmapsMaxLevelReduction = 2;
    [SerializeField, Range(32, 512)] private int streamingMipmapsMaxFileIORequests = 128;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (FindObjectOfType<MobilePerformanceTuner>() != null)
        {
            return;
        }

        GameObject tunerObject = new GameObject("Mobile Performance Tuner");
        tunerObject.AddComponent<MobilePerformanceTuner>();
        DontDestroyOnLoad(tunerObject);
    }

    private void Awake()
    {
        if (!ShouldApply())
        {
            return;
        }

        ApplyFrameSettings();
        ApplyLodSettings();
        ApplyLightingSettings();
        ApplyStreamingSettings();
    }

    private bool ShouldApply()
    {
        return !applyOnMobileOnly || Application.isMobilePlatform;
    }

    private void ApplyFrameSettings()
    {
        if (disableVSync)
        {
            QualitySettings.vSyncCount = 0;
        }

        if (targetFrameRate > 0)
        {
            Application.targetFrameRate = targetFrameRate;
            if (alignFixedDeltaTime)
            {
                Time.fixedDeltaTime = 1f / targetFrameRate;
            }
        }
    }

    private void ApplyLodSettings()
    {
        QualitySettings.lodBias = lodBias;
        QualitySettings.maximumLODLevel = maximumLodLevel;
    }

    private void ApplyLightingSettings()
    {
        QualitySettings.shadowDistance = shadowDistance;
        QualitySettings.pixelLightCount = pixelLightCount;

        if (!disableRealtimeShadows)
        {
            return;
        }

        Light[] sceneLights = FindObjectsOfType<Light>(true);
        foreach (Light lightSource in sceneLights)
        {
            if (lightSource == null)
            {
                continue;
            }

            lightSource.shadows = LightShadows.None;
            lightSource.renderMode = LightRenderMode.ForceVertex;
        }
    }

    private void ApplyStreamingSettings()
    {
        if (!enableTextureStreaming)
        {
            return;
        }

        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsAddAllCameras = true;
        QualitySettings.streamingMipmapsMemoryBudget = streamingMipmapsMemoryBudget;
        QualitySettings.streamingMipmapsRenderersPerFrame = streamingMipmapsRenderersPerFrame;
        QualitySettings.streamingMipmapsMaxLevelReduction = streamingMipmapsMaxLevelReduction;
        QualitySettings.streamingMipmapsMaxFileIORequests = streamingMipmapsMaxFileIORequests;
    }
}
