using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Presents the instinct prediction mini-game prompt and results.
/// </summary>
public class InstinctPredictionUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text titleText;
    [SerializeField] private Text bodyText;
    [SerializeField] private RectTransform buttonRow;

    private Button[] buttons;
    private Text[] buttonLabels;
    private CreatureInstinct[] currentInstincts;
    private Action<CreatureInstinct> selectionHandler;
    private bool acceptInput;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (FindObjectOfType<InstinctPredictionUI>() != null)
        {
            return;
        }

        GameObject uiObject = new GameObject("Instinct Prediction UI");
        uiObject.AddComponent<InstinctPredictionUI>();
    }

    public static InstinctPredictionUI ShowPrompt(string title, string body, CreatureInstinct[] instincts, Action<CreatureInstinct> onSelect)
    {
        InstinctPredictionUI ui = FindObjectOfType<InstinctPredictionUI>();
        if (ui == null)
        {
            return null;
        }

        ui.ShowPromptInternal(title, body, instincts, onSelect);
        return ui;
    }

    public static void HidePrompt()
    {
        InstinctPredictionUI ui = FindObjectOfType<InstinctPredictionUI>();
        if (ui == null)
        {
            return;
        }

        ui.SetButtonsVisible(false);
        ui.SetVisible(false);
    }

    public static void ShowResult(string title, string body, float duration)
    {
        InstinctPredictionUI ui = FindObjectOfType<InstinctPredictionUI>();
        if (ui == null)
        {
            return;
        }

        ui.ShowResultInternal(title, body, duration);
    }

    private void Awake()
    {
        if (canvasGroup == null || titleText == null || bodyText == null || buttonRow == null)
        {
            BuildRuntimeUi();
        }

        SetVisible(false);
    }

    private void Update()
    {
        if (!acceptInput || currentInstincts == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectInstinct(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectInstinct(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectInstinct(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectInstinct(3);
        }
    }

    private void ShowPromptInternal(string title, string body, CreatureInstinct[] instincts, Action<CreatureInstinct> onSelect)
    {
        if (instincts == null || instincts.Length == 0)
        {
            return;
        }

        EnsureButtons(instincts.Length);
        currentInstincts = instincts;
        selectionHandler = onSelect;
        acceptInput = true;

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (bodyText != null)
        {
            bodyText.text = body;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            bool active = i < instincts.Length;
            buttons[i].gameObject.SetActive(active);
            if (active && buttonLabels != null && buttonLabels.Length > i)
            {
                buttonLabels[i].text = GetInstinctLabel(instincts[i], i + 1);
            }
        }

        SetButtonsVisible(true);
        SetVisible(true);
    }

    private void ShowResultInternal(string title, string body, float duration)
    {
        acceptInput = false;
        currentInstincts = null;
        selectionHandler = null;

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (bodyText != null)
        {
            bodyText.text = body;
        }

        SetButtonsVisible(false);
        SetVisible(true);

        if (duration > 0f)
        {
            CancelInvoke(nameof(HideAfterResult));
            Invoke(nameof(HideAfterResult), duration);
        }
    }

    private void HideAfterResult()
    {
        SetVisible(false);
    }

    private void SelectInstinct(int index)
    {
        if (currentInstincts == null || index < 0 || index >= currentInstincts.Length)
        {
            return;
        }

        acceptInput = false;
        selectionHandler?.Invoke(currentInstincts[index]);
    }

    private void EnsureButtons(int count)
    {
        if (buttons != null && buttons.Length >= count)
        {
            return;
        }

        buttons = new Button[count];
        buttonLabels = new Text[count];

        for (int i = 0; i < count; i++)
        {
            GameObject buttonObject = new GameObject($"Instinct Button {i + 1}", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(buttonRow, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(130f, 40f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.1f, 0.14f, 0.2f, 0.85f);

            Button button = buttonObject.GetComponent<Button>();
            int capturedIndex = i;
            button.onClick.AddListener(() => SelectInstinct(capturedIndex));

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text label = labelObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 14;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.9f, 0.94f, 1f, 0.95f);
            label.raycastTarget = false;

            buttons[i] = button;
            buttonLabels[i] = label;
        }
    }

    private string GetInstinctLabel(CreatureInstinct instinct, int index)
    {
        return $"{index}. {instinct}";
    }

    private void SetButtonsVisible(bool visible)
    {
        if (buttonRow != null)
        {
            buttonRow.gameObject.SetActive(visible);
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
        EnsureEventSystem();

        GameObject canvasObject = new GameObject("Instinct Prediction Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = new GameObject("Instinct Panel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 208f);
        panelRect.sizeDelta = new Vector2(640f, 220f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.7f);
        panelImage.raycastTarget = false;

        canvasGroup = panelObject.GetComponent<CanvasGroup>();

        GameObject titleObject = new GameObject("Title", typeof(RectTransform), typeof(Text));
        titleObject.transform.SetParent(panelObject.transform, false);

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -16f);
        titleRect.sizeDelta = new Vector2(600f, 32f);

        titleText = titleObject.GetComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 20;
        titleText.alignment = TextAnchor.UpperCenter;
        titleText.color = new Color(0.8f, 0.92f, 1f, 0.95f);
        titleText.raycastTarget = false;

        GameObject bodyObject = new GameObject("Body", typeof(RectTransform), typeof(Text));
        bodyObject.transform.SetParent(panelObject.transform, false);

        RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRect.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRect.pivot = new Vector2(0.5f, 0.5f);
        bodyRect.anchoredPosition = new Vector2(0f, 12f);
        bodyRect.sizeDelta = new Vector2(600f, 72f);

        bodyText = bodyObject.GetComponent<Text>();
        bodyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        bodyText.fontSize = 16;
        bodyText.alignment = TextAnchor.MiddleCenter;
        bodyText.color = new Color(0.95f, 0.95f, 0.95f, 0.95f);
        bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
        bodyText.verticalOverflow = VerticalWrapMode.Overflow;
        bodyText.raycastTarget = false;

        GameObject buttonRowObject = new GameObject("Button Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        buttonRowObject.transform.SetParent(panelObject.transform, false);

        buttonRow = buttonRowObject.GetComponent<RectTransform>();
        buttonRow.anchorMin = new Vector2(0.5f, 0f);
        buttonRow.anchorMax = new Vector2(0.5f, 0f);
        buttonRow.pivot = new Vector2(0.5f, 0f);
        buttonRow.anchoredPosition = new Vector2(0f, 20f);
        buttonRow.sizeDelta = new Vector2(600f, 44f);

        HorizontalLayoutGroup layoutGroup = buttonRowObject.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = 12f;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.padding = new RectOffset(8, 8, 4, 4);
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystemObject.transform.SetParent(transform, false);
    }
}
