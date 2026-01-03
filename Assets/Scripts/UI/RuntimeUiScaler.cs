using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applies consistent runtime UI scaling across aspect ratios, with tablet-friendly defaults.
/// </summary>
public static class RuntimeUiScaler
{
    private const float TabletAspectThreshold = 1.5f;
    private static readonly Vector2 TabletReferenceResolution = new Vector2(2048f, 1536f);
    private static readonly Vector2 WidescreenReferenceResolution = new Vector2(1920f, 1080f);

    public static void Apply(CanvasScaler scaler)
    {
        if (scaler == null)
        {
            return;
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 1.777f;
        if (aspect < TabletAspectThreshold)
        {
            scaler.referenceResolution = TabletReferenceResolution;
            scaler.matchWidthOrHeight = 0.7f;
        }
        else
        {
            scaler.referenceResolution = WidescreenReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }
}
