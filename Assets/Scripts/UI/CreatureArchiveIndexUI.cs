using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a creature archive index with scan states and lore panel.
/// </summary>
public class CreatureArchiveIndexUI : MonoBehaviour
{
    [SerializeField] private CreatureDatabase creatureDatabase;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private Text headerText;

    [Header("Lore Panel")]
    [SerializeField] private CanvasGroup loreCanvasGroup;
    [SerializeField] private Text loreTitleText;
    [SerializeField] private Text loreStatusText;
    [SerializeField] private Text loreBodyText;
    [SerializeField] private Button loreCloseButton;

    [Header("Visuals")]
    [SerializeField] private Color unknownColor = new Color(0.05f, 0.05f, 0.08f, 0.85f);
    [SerializeField] private Color partialColor = new Color(0.16f, 0.32f, 0.38f, 0.9f);
    [SerializeField] private Color understoodColor = new Color(0.2f, 0.6f, 0.72f, 0.95f);
    [SerializeField] private Color unknownTextColor = new Color(0.7f, 0.75f, 0.8f, 0.85f);
    [SerializeField] private Color knownTextColor = new Color(0.92f, 0.97f, 1f, 0.95f);

    private readonly List<CreatureEntry> entries = new List<CreatureEntry>();
    private CreatureData selectedCreature;
    private bool entriesBuilt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (FindObjectOfType<CreatureArchiveIndexUI>() != null)
        {
            return;
        }

        GameObject uiObject = new GameObject("Creature Archive Index UI");
        uiObject.AddComponent<CreatureArchiveIndexUI>();
    }

    private void Awake()
    {
        if (contentRoot == null || loreCanvasGroup == null)
        {
            BuildRuntimeUi();
        }

        SetLoreVisible(false);
    }

    private void OnEnable()
    {
        EnsureDatabase();
        BuildEntries();
        RefreshEntries();
        CreatureArchiveProgress.OnKnowledgeChanged += HandleKnowledgeChanged;
    }

    private void OnDisable()
    {
        CreatureArchiveProgress.OnKnowledgeChanged -= HandleKnowledgeChanged;
    }

    private void HandleKnowledgeChanged(int total)
    {
        RefreshEntries();
        RefreshLorePanel();
    }

    private void EnsureDatabase()
    {
        if (creatureDatabase == null)
        {
            creatureDatabase = Resources.Load<CreatureDatabase>("CreatureDatabase");
        }
    }

    private void BuildEntries()
    {
        if (entriesBuilt || creatureDatabase == null)
        {
            return;
        }

        if (contentRoot == null)
        {
            return;
        }

        foreach (CreatureData creature in creatureDatabase.Creatures)
        {
            if (creature == null)
            {
                continue;
            }

            CreatureEntry entry = CreateEntry(creature);
            entries.Add(entry);
        }

        entriesBuilt = true;
    }

    private CreatureEntry CreateEntry(CreatureData creature)
    {
        GameObject entryObject = new GameObject($"Creature Entry {creature.DisplayName}", typeof(RectTransform), typeof(Image), typeof(Button));
        entryObject.transform.SetParent(contentRoot, false);

        Image entryImage = entryObject.GetComponent<Image>();
        entryImage.raycastTarget = true;

        Button entryButton = entryObject.GetComponent<Button>();
        entryButton.transition = Selectable.Transition.ColorTint;
        entryButton.onClick.AddListener(() => ShowLore(creature));

        GameObject nameObject = new GameObject("Name", typeof(RectTransform), typeof(Text));
        nameObject.transform.SetParent(entryObject.transform, false);

        RectTransform nameRect = nameObject.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(1f, 0f);
        nameRect.pivot = new Vector2(0.5f, 0f);
        nameRect.anchoredPosition = new Vector2(0f, 10f);
        nameRect.sizeDelta = new Vector2(-12f, 40f);

        Text nameText = nameObject.GetComponent<Text>();
        nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        nameText.fontSize = 16;
        nameText.alignment = TextAnchor.LowerCenter;
        nameText.color = knownTextColor;
        nameText.horizontalOverflow = HorizontalWrapMode.Wrap;
        nameText.verticalOverflow = VerticalWrapMode.Overflow;

        GameObject statusObject = new GameObject("Status", typeof(RectTransform), typeof(Text));
        statusObject.transform.SetParent(entryObject.transform, false);

        RectTransform statusRect = statusObject.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0f, 1f);
        statusRect.anchorMax = new Vector2(1f, 1f);
        statusRect.pivot = new Vector2(0.5f, 1f);
        statusRect.anchoredPosition = new Vector2(0f, -8f);
        statusRect.sizeDelta = new Vector2(-12f, 24f);

        Text statusText = statusObject.GetComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        statusText.fontSize = 12;
        statusText.alignment = TextAnchor.UpperCenter;
        statusText.color = knownTextColor;
        statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
        statusText.verticalOverflow = VerticalWrapMode.Overflow;

        return new CreatureEntry(creature, entryImage, nameText, statusText, entryButton);
    }

    private void RefreshEntries()
    {
        foreach (CreatureEntry entry in entries)
        {
            CreatureUnderstandingState state = CreatureArchiveProgress.GetUnderstanding(entry.Creature.Id);
            ApplyEntryState(entry, state);
        }
    }

    private void ApplyEntryState(CreatureEntry entry, CreatureUnderstandingState state)
    {
        switch (state)
        {
            case CreatureUnderstandingState.Partial:
                entry.Image.color = partialColor;
                entry.NameText.text = entry.Creature.DisplayName;
                entry.NameText.color = knownTextColor;
                entry.StatusText.text = "SCANNED";
                entry.StatusText.color = knownTextColor;
                break;
            case CreatureUnderstandingState.Understood:
                entry.Image.color = understoodColor;
                entry.NameText.text = entry.Creature.DisplayName;
                entry.NameText.color = knownTextColor;
                entry.StatusText.text = "COMPLETE";
                entry.StatusText.color = knownTextColor;
                break;
            default:
                entry.Image.color = unknownColor;
                entry.NameText.text = "UNKNOWN";
                entry.NameText.color = unknownTextColor;
                entry.StatusText.text = "SILHOUETTE";
                entry.StatusText.color = unknownTextColor;
                break;
        }
    }

    private void ShowLore(CreatureData creature)
    {
        selectedCreature = creature;
        UpdateLorePanel();
        SetLoreVisible(true);
    }

    private void RefreshLorePanel()
    {
        if (loreCanvasGroup != null && loreCanvasGroup.alpha > 0.01f)
        {
            UpdateLorePanel();
        }
    }

    private void UpdateLorePanel()
    {
        if (loreTitleText == null || loreStatusText == null || loreBodyText == null)
        {
            return;
        }

        if (selectedCreature == null)
        {
            loreTitleText.text = "Archive Lore";
            loreStatusText.text = "Select an entry";
            loreBodyText.text = "Choose a creature from the archive index to view its notes.";
            return;
        }

        CreatureUnderstandingState state = CreatureArchiveProgress.GetUnderstanding(selectedCreature.Id);
        string title = state == CreatureUnderstandingState.Unknown ? "Unknown Entity" : selectedCreature.DisplayName;
        loreTitleText.text = title;
        loreStatusText.text = BuildStatusLine(selectedCreature, state);
        loreBodyText.text = BuildLoreBody(selectedCreature, state);
    }

    private string BuildStatusLine(CreatureData creature, CreatureUnderstandingState state)
    {
        string stateLabel = state switch
        {
            CreatureUnderstandingState.Partial => "Partial Scan",
            CreatureUnderstandingState.Understood => "Complete Archive",
            _ => "Silhouette Only"
        };

        string rarity = creature != null ? creature.Rarity.ToString().ToUpperInvariant() : "UNKNOWN";
        return $"{stateLabel} • {rarity}";
    }

    private string BuildLoreBody(CreatureData creature, CreatureUnderstandingState state)
    {
        if (creature == null)
        {
            return "No archive data available.";
        }

        string encounterNotes = string.IsNullOrWhiteSpace(creature.EncounterNotes)
            ? "Sensor impressions are faint."
            : creature.EncounterNotes;
        string deepDiveNotes = string.IsNullOrWhiteSpace(creature.DeepDiveNotes)
            ? "Archive analysts have not compiled a full report yet."
            : creature.DeepDiveNotes;
        string behaviors = BuildBehaviorList(creature);

        switch (state)
        {
            case CreatureUnderstandingState.Partial:
                return CombineLoreSections(encounterNotes, "Partial scan data logged.", behaviors);
            case CreatureUnderstandingState.Understood:
                return CombineLoreSections(encounterNotes, deepDiveNotes, behaviors);
            default:
                return "Silhouette recorded. Deploy scanners to unlock archive data.";
        }
    }

    private string BuildBehaviorList(CreatureData creature)
    {
        if (creature == null || creature.Behaviors == null || creature.Behaviors.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();
        builder.Append("Known behaviors:\n");
        foreach (string behavior in creature.Behaviors)
        {
            if (string.IsNullOrWhiteSpace(behavior))
            {
                continue;
            }

            builder.Append("• ");
            builder.Append(behavior.Trim());
            builder.Append('\n');
        }

        return builder.ToString().TrimEnd();
    }

    private string CombineLoreSections(string primary, string secondary, string behaviors)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(primary);

        if (!string.IsNullOrWhiteSpace(secondary))
        {
            builder.Append("\n\n");
            builder.Append(secondary);
        }

        if (!string.IsNullOrWhiteSpace(behaviors))
        {
            builder.Append("\n\n");
            builder.Append(behaviors);
        }

        return builder.ToString();
    }

    private void SetLoreVisible(bool visible)
    {
        if (loreCanvasGroup == null)
        {
            return;
        }

        loreCanvasGroup.alpha = visible ? 1f : 0f;
        loreCanvasGroup.interactable = visible;
        loreCanvasGroup.blocksRaycasts = visible;
    }

    private void BuildRuntimeUi()
    {
        GameObject canvasObject = new GameObject("Creature Archive Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 92;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = new GameObject("Archive Index Panel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-36f, 0f);
        panelRect.sizeDelta = new Vector2(520f, 760f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.65f);
        panelImage.raycastTarget = false;

        GameObject headerObject = new GameObject("Archive Header", typeof(RectTransform), typeof(Text));
        headerObject.transform.SetParent(panelObject.transform, false);

        RectTransform headerRect = headerObject.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = new Vector2(0f, -16f);
        headerRect.sizeDelta = new Vector2(-24f, 36f);

        headerText = headerObject.GetComponent<Text>();
        headerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        headerText.fontSize = 22;
        headerText.alignment = TextAnchor.UpperCenter;
        headerText.color = new Color(0.72f, 0.9f, 1f, 0.95f);
        headerText.text = "Archive Index";

        GameObject scrollObject = new GameObject("Archive Scroll", typeof(RectTransform), typeof(ScrollRect));
        scrollObject.transform.SetParent(panelObject.transform, false);

        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0f, 0f);
        scrollRectTransform.anchorMax = new Vector2(1f, 1f);
        scrollRectTransform.offsetMin = new Vector2(16f, 20f);
        scrollRectTransform.offsetMax = new Vector2(-16f, -64f);

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportObject.transform.SetParent(scrollObject.transform, false);

        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.2f);
        viewportImage.raycastTarget = true;

        Mask viewportMask = viewportObject.GetComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        contentObject.transform.SetParent(viewportObject.transform, false);

        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        GridLayoutGroup grid = contentObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(140f, 140f);
        grid.spacing = new Vector2(12f, 12f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        contentRoot = contentRect;

        BuildLorePanel(canvasObject.transform);
    }

    private void BuildLorePanel(Transform parent)
    {
        GameObject lorePanelObject = new GameObject("Archive Lore Panel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        lorePanelObject.transform.SetParent(parent, false);

        RectTransform loreRect = lorePanelObject.GetComponent<RectTransform>();
        loreRect.anchorMin = new Vector2(0.5f, 0.5f);
        loreRect.anchorMax = new Vector2(0.5f, 0.5f);
        loreRect.pivot = new Vector2(0.5f, 0.5f);
        loreRect.anchoredPosition = new Vector2(-120f, 0f);
        loreRect.sizeDelta = new Vector2(620f, 420f);

        Image loreImage = lorePanelObject.GetComponent<Image>();
        loreImage.color = new Color(0f, 0f, 0f, 0.78f);

        loreCanvasGroup = lorePanelObject.GetComponent<CanvasGroup>();

        GameObject titleObject = new GameObject("Lore Title", typeof(RectTransform), typeof(Text));
        titleObject.transform.SetParent(lorePanelObject.transform, false);

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -20f);
        titleRect.sizeDelta = new Vector2(-64f, 32f);

        loreTitleText = titleObject.GetComponent<Text>();
        loreTitleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        loreTitleText.fontSize = 24;
        loreTitleText.alignment = TextAnchor.UpperLeft;
        loreTitleText.color = new Color(0.8f, 0.95f, 1f, 0.95f);
        loreTitleText.text = "Archive Lore";

        GameObject statusObject = new GameObject("Lore Status", typeof(RectTransform), typeof(Text));
        statusObject.transform.SetParent(lorePanelObject.transform, false);

        RectTransform statusRect = statusObject.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0f, 1f);
        statusRect.anchorMax = new Vector2(1f, 1f);
        statusRect.pivot = new Vector2(0.5f, 1f);
        statusRect.anchoredPosition = new Vector2(0f, -56f);
        statusRect.sizeDelta = new Vector2(-64f, 24f);

        loreStatusText = statusObject.GetComponent<Text>();
        loreStatusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        loreStatusText.fontSize = 14;
        loreStatusText.alignment = TextAnchor.UpperLeft;
        loreStatusText.color = new Color(0.68f, 0.85f, 0.95f, 0.95f);
        loreStatusText.text = "Select an entry";

        GameObject bodyObject = new GameObject("Lore Body", typeof(RectTransform), typeof(Text));
        bodyObject.transform.SetParent(lorePanelObject.transform, false);

        RectTransform bodyRect = bodyObject.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.offsetMin = new Vector2(24f, 24f);
        bodyRect.offsetMax = new Vector2(-24f, -96f);

        loreBodyText = bodyObject.GetComponent<Text>();
        loreBodyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        loreBodyText.fontSize = 16;
        loreBodyText.alignment = TextAnchor.UpperLeft;
        loreBodyText.color = new Color(0.92f, 0.95f, 0.98f, 0.95f);
        loreBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
        loreBodyText.verticalOverflow = VerticalWrapMode.Truncate;
        loreBodyText.text = "Choose a creature from the archive index to view its notes.";

        GameObject closeObject = new GameObject("Lore Close", typeof(RectTransform), typeof(Image), typeof(Button));
        closeObject.transform.SetParent(lorePanelObject.transform, false);

        RectTransform closeRect = closeObject.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-16f, -16f);
        closeRect.sizeDelta = new Vector2(80f, 28f);

        Image closeImage = closeObject.GetComponent<Image>();
        closeImage.color = new Color(0.2f, 0.3f, 0.35f, 0.9f);

        loreCloseButton = closeObject.GetComponent<Button>();
        loreCloseButton.onClick.AddListener(() => SetLoreVisible(false));

        GameObject closeTextObject = new GameObject("Lore Close Text", typeof(RectTransform), typeof(Text));
        closeTextObject.transform.SetParent(closeObject.transform, false);

        RectTransform closeTextRect = closeTextObject.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;

        Text closeText = closeTextObject.GetComponent<Text>();
        closeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        closeText.fontSize = 14;
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.color = new Color(0.9f, 0.95f, 1f, 0.95f);
        closeText.text = "CLOSE";
    }

    private readonly struct CreatureEntry
    {
        public CreatureData Creature { get; }
        public Image Image { get; }
        public Text NameText { get; }
        public Text StatusText { get; }
        public Button Button { get; }

        public CreatureEntry(CreatureData creature, Image image, Text nameText, Text statusText, Button button)
        {
            Creature = creature;
            Image = image;
            NameText = nameText;
            StatusText = statusText;
            Button = button;
        }
    }
}
