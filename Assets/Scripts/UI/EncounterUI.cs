using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a lightweight encounter panel when a creature engages.
/// </summary>
public class EncounterUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text titleText;
    [SerializeField] private Text notesText;
    [SerializeField] private string fallbackTitle = "Encounter";
    [SerializeField] private string fallbackNotes = "A creature draws near.";
    [SerializeField, Range(1f, 6f)] private float visibleDuration = 3.5f;
    [SerializeField, Range(0f, 1f)] private float fadeDuration = 0.35f;

    private Coroutine displayRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (FindObjectOfType<EncounterUI>() != null)
        {
            return;
        }

        GameObject uiObject = new GameObject("Encounter UI");
        uiObject.AddComponent<EncounterUI>();
    }

    private void Awake()
    {
        if (canvasGroup == null || titleText == null || notesText == null)
        {
            BuildRuntimeUi();
        }

        SetVisible(false);
    }

    public static void ShowEncounter(CreatureData creature)
    {
        EncounterUI ui = FindObjectOfType<EncounterUI>();
        if (ui == null)
        {
            return;
        }

        string title = creature != null && !string.IsNullOrWhiteSpace(creature.DisplayName)
            ? creature.DisplayName
            : ui.fallbackTitle;
        string notes = creature != null && !string.IsNullOrWhiteSpace(creature.EncounterNotes)
            ? creature.EncounterNotes
            : ui.fallbackNotes;

        ui.ShowPanel(title, notes);
    }

    private void ShowPanel(string title, string notes)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }

        if (notesText != null)
        {
            notesText.text = notes;
        }

        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
        }

        displayRoutine = StartCoroutine(DisplayRoutine());
    }

    private IEnumerator DisplayRoutine()
    {
        SetVisible(true);
        if (visibleDuration > 0f)
        {
            yield return new WaitForSeconds(visibleDuration);
        }

        if (fadeDuration > 0f)
        {
            yield return FadeOut();
        }

        SetVisible(false);
        displayRoutine = null;
    }

    private IEnumerator FadeOut()
    {
        if (canvasGroup == null)
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
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void BuildRuntimeUi()
    {
        GameObject canvasObject = new GameObject("Encounter Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 98;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        RuntimeUiScaler.Apply(scaler);

        GameObject panelObject = new GameObject("Encounter Panel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 48f);
        panelRect.sizeDelta = new Vector2(520f, 140f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.6f);
        panelImage.raycastTarget = false;

        GameObject titleObject = new GameObject("Encounter Title", typeof(RectTransform), typeof(Text));
        titleObject.transform.SetParent(panelObject.transform, false);

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -12f);
        titleRect.sizeDelta = new Vector2(-24f, 32f);

        titleText = titleObject.GetComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 22;
        titleText.alignment = TextAnchor.UpperCenter;
        titleText.color = new Color(0.85f, 0.95f, 1f, 0.95f);
        titleText.raycastTarget = false;
        titleText.text = fallbackTitle;

        GameObject notesObject = new GameObject("Encounter Notes", typeof(RectTransform), typeof(Text));
        notesObject.transform.SetParent(panelObject.transform, false);

        RectTransform notesRect = notesObject.GetComponent<RectTransform>();
        notesRect.anchorMin = new Vector2(0f, 0f);
        notesRect.anchorMax = new Vector2(1f, 1f);
        notesRect.offsetMin = new Vector2(16f, 12f);
        notesRect.offsetMax = new Vector2(-16f, -48f);

        notesText = notesObject.GetComponent<Text>();
        notesText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        notesText.fontSize = 16;
        notesText.alignment = TextAnchor.UpperCenter;
        notesText.color = new Color(0.95f, 0.95f, 0.95f, 0.95f);
        notesText.horizontalOverflow = HorizontalWrapMode.Wrap;
        notesText.verticalOverflow = VerticalWrapMode.Truncate;
        notesText.raycastTarget = false;
        notesText.text = fallbackNotes;

        canvasGroup = panelObject.GetComponent<CanvasGroup>();
    }
}
