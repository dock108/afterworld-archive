using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a temporary toast when a scan is completed.
/// </summary>
public class ScanToastUI : MonoBehaviour
{
    [SerializeField] private HoldToScanSystem scanSystem;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text toastText;
    [SerializeField] private string defaultMessage = "Scan Added";
    [SerializeField, Range(0.5f, 5f)] private float visibleDuration = 1.5f;
    [SerializeField, Range(0f, 1f)] private float fadeDuration = 0.25f;

    private Coroutine toastRoutine;

    private void Awake()
    {
        if (canvasGroup == null || toastText == null)
        {
            BuildRuntimeUi();
        }

        SetVisible(false);
    }

    private void OnEnable()
    {
        if (scanSystem == null)
        {
            scanSystem = FindObjectOfType<HoldToScanSystem>();
        }

        if (scanSystem != null)
        {
            scanSystem.OnScanCompleted.AddListener(ShowScanAdded);
        }
    }

    private void OnDisable()
    {
        if (scanSystem != null)
        {
            scanSystem.OnScanCompleted.RemoveListener(ShowScanAdded);
        }
    }

    public void ShowScanAdded()
    {
        ShowToast(defaultMessage);
    }

    public void ShowToast(string message)
    {
        if (toastText != null)
        {
            toastText.text = message;
        }

        if (toastRoutine != null)
        {
            StopCoroutine(toastRoutine);
        }

        toastRoutine = StartCoroutine(ToastRoutine());
    }

    private IEnumerator ToastRoutine()
    {
        SetVisible(true);
        yield return new WaitForSeconds(visibleDuration);
        yield return FadeOut();
        SetVisible(false);
        toastRoutine = null;
    }

    private IEnumerator FadeOut()
    {
        if (canvasGroup == null || fadeDuration <= 0f)
        {
            yield break;
        }

        float startAlpha = canvasGroup.alpha;
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void BuildRuntimeUi()
    {
        GameObject canvasObject = new GameObject("Scan Toast Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject toastObject = new GameObject("Scan Toast", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        toastObject.transform.SetParent(canvasObject.transform, false);

        RectTransform toastRect = toastObject.GetComponent<RectTransform>();
        toastRect.anchorMin = new Vector2(0.5f, 1f);
        toastRect.anchorMax = new Vector2(0.5f, 1f);
        toastRect.pivot = new Vector2(0.5f, 1f);
        toastRect.anchoredPosition = new Vector2(0f, -40f);
        toastRect.sizeDelta = new Vector2(240f, 48f);

        Image toastBackground = toastObject.GetComponent<Image>();
        toastBackground.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject textObject = new GameObject("Toast Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(toastObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 6f);
        textRect.offsetMax = new Vector2(-12f, -6f);

        toastText = textObject.GetComponent<Text>();
        toastText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        toastText.fontSize = 18;
        toastText.alignment = TextAnchor.MiddleCenter;
        toastText.color = Color.white;
        toastText.text = defaultMessage;

        canvasGroup = toastObject.GetComponent<CanvasGroup>();
    }
}
