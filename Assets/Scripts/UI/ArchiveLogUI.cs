using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays lightweight system boot logs based on archive state changes.
/// </summary>
public class ArchiveLogUI : MonoBehaviour
{
    [SerializeField] private ArchiveManager archiveManager;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text logText;
    [SerializeField] private int maxLines = 6;
    [SerializeField] private float messageStepDelay = 0.25f;

    [Header("Messages")]
    [SerializeField]
    private string[] offlineMessages =
    {
        "Archive offline.",
        "Awaiting activation signal."
    };

    [SerializeField]
    private string[] flickerMessages =
    {
        "Power surge detected...",
        "Stabilizing archive conduits...",
        "Boot sequence initiated."
    };

    [SerializeField]
    private string[] partialOnlineMessages =
    {
        "Core systems online.",
        "Signal lattice stabilized."
    };

    private readonly List<string> messages = new List<string>();
    private Coroutine messageRoutine;
    private Coroutine glitchRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (FindObjectOfType<ArchiveLogUI>() != null)
        {
            return;
        }

        GameObject logObject = new GameObject("Archive Log UI");
        logObject.AddComponent<ArchiveLogUI>();
    }

    public static void AppendEntry(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        ArchiveLogUI logUi = FindObjectOfType<ArchiveLogUI>();
        if (logUi == null)
        {
            GameObject logObject = new GameObject("Archive Log UI");
            logUi = logObject.AddComponent<ArchiveLogUI>();
        }

        logUi.AppendMessage(message);
    }

    public static void TriggerGlitch(float duration, float maxOffset, float minInterval, float maxInterval)
    {
        ArchiveLogUI logUi = FindObjectOfType<ArchiveLogUI>();
        if (logUi == null)
        {
            GameObject logObject = new GameObject("Archive Log UI");
            logUi = logObject.AddComponent<ArchiveLogUI>();
        }

        logUi.PlayGlitch(duration, maxOffset, minInterval, maxInterval);
    }

    private void Awake()
    {
        if (canvasGroup == null || logText == null)
        {
            BuildRuntimeUi();
        }

        ConfigureCanvasGroup();
    }

    private void OnEnable()
    {
        if (archiveManager == null)
        {
            archiveManager = FindObjectOfType<ArchiveManager>();
        }

        if (archiveManager != null)
        {
            archiveManager.OnStateChanged += HandleStateChanged;
            AppendMessagesForState(archiveManager.CurrentState, true);
        }
    }

    private void OnDisable()
    {
        if (archiveManager != null)
        {
            archiveManager.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(ArchiveManager.ArchiveState state)
    {
        AppendMessagesForState(state, false);
    }

    private void AppendMessagesForState(ArchiveManager.ArchiveState state, bool immediate)
    {
        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
        }

        string[] lines = GetMessagesForState(state);
        if (lines == null || lines.Length == 0)
        {
            return;
        }

        if (immediate || messageStepDelay <= 0f)
        {
            foreach (string line in lines)
            {
                AddMessage(line);
            }

            return;
        }

        messageRoutine = StartCoroutine(QueueMessages(lines));
    }

    private IEnumerator QueueMessages(string[] lines)
    {
        foreach (string line in lines)
        {
            AddMessage(line);
            yield return new WaitForSeconds(messageStepDelay);
        }

        messageRoutine = null;
    }

    private string[] GetMessagesForState(ArchiveManager.ArchiveState state)
    {
        switch (state)
        {
            case ArchiveManager.ArchiveState.Offline:
                return offlineMessages;
            case ArchiveManager.ArchiveState.Flicker:
                return flickerMessages;
            case ArchiveManager.ArchiveState.PartialOnline:
                return partialOnlineMessages;
            default:
                return null;
        }
    }

    private void AddMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        messages.Add(message);
        while (messages.Count > maxLines)
        {
            messages.RemoveAt(0);
        }

        if (logText != null)
        {
            logText.text = string.Join("\n", messages);
        }
    }

    private void AppendMessage(string message)
    {
        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
            messageRoutine = null;
        }

        AddMessage(message);
    }

    private void PlayGlitch(float duration, float maxOffset, float minInterval, float maxInterval)
    {
        if (canvasGroup == null || logText == null)
        {
            return;
        }

        if (duration <= 0f || maxOffset <= 0f)
        {
            return;
        }

        if (glitchRoutine != null)
        {
            StopCoroutine(glitchRoutine);
        }

        glitchRoutine = StartCoroutine(GlitchRoutine(duration, maxOffset, minInterval, maxInterval));
    }

    private IEnumerator GlitchRoutine(float duration, float maxOffset, float minInterval, float maxInterval)
    {
        float elapsed = 0f;
        float originalAlpha = canvasGroup.alpha;
        RectTransform textTransform = logText.rectTransform;
        Vector2 originalPosition = textTransform.anchoredPosition;
        Color originalColor = logText.color;
        float intervalMin = Mathf.Max(0.01f, minInterval);
        float intervalMax = Mathf.Max(intervalMin, maxInterval);

        while (elapsed < duration)
        {
            canvasGroup.alpha = Random.Range(0.3f, 1f);
            textTransform.anchoredPosition = originalPosition + Random.insideUnitCircle * maxOffset;

            Color flicker = originalColor;
            flicker.r = Mathf.Clamp01(flicker.r + Random.Range(-0.05f, 0.05f));
            flicker.g = Mathf.Clamp01(flicker.g + Random.Range(-0.05f, 0.05f));
            flicker.b = Mathf.Clamp01(flicker.b + Random.Range(-0.05f, 0.05f));
            logText.color = flicker;

            float waitTime = Random.Range(intervalMin, intervalMax);
            elapsed += waitTime;
            yield return new WaitForSeconds(waitTime);
        }

        canvasGroup.alpha = originalAlpha;
        textTransform.anchoredPosition = originalPosition;
        logText.color = originalColor;
        glitchRoutine = null;
    }

    private void ConfigureCanvasGroup()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void BuildRuntimeUi()
    {
        GameObject canvasObject = new GameObject("Archive Log Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 95;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        RuntimeUiScaler.Apply(scaler);

        GameObject panelObject = new GameObject("Archive Log Panel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(32f, -32f);
        panelRect.sizeDelta = new Vector2(420f, 160f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.55f);
        panelImage.raycastTarget = false;

        GameObject textObject = new GameObject("Log Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(panelObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 12f);
        textRect.offsetMax = new Vector2(-16f, -12f);

        logText = textObject.GetComponent<Text>();
        logText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        logText.fontSize = 18;
        logText.alignment = TextAnchor.UpperLeft;
        logText.color = new Color(0.68f, 0.9f, 1f, 0.95f);
        logText.horizontalOverflow = HorizontalWrapMode.Wrap;
        logText.verticalOverflow = VerticalWrapMode.Truncate;
        logText.raycastTarget = false;
        logText.text = string.Empty;

        canvasGroup = panelObject.GetComponent<CanvasGroup>();
    }
}
