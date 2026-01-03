using UnityEngine;

/// <summary>
/// Emits lightweight, contextual onboarding hints for early interactions.
/// </summary>
public class OnboardingHintSystem : MonoBehaviour
{
    [Header("Messages")]
    [SerializeField, TextArea(2, 4)] private string scanOpportunityHint =
        "The scanner awakens when your focus steadies on a creature.";
    [SerializeField, TextArea(2, 4)] private string firstEncounterHint =
        "A quiet presence lingers nearby. It will approach if you keep calm.";
    [SerializeField, TextArea(2, 4)] private string archiveDoorHint =
        "The archive door releases with a low sigh of light.";

    private static bool scanHintShown;
    private static bool encounterHintShown;
    private static bool doorHintShown;

    private ArchiveManager archiveManager;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (FindObjectOfType<OnboardingHintSystem>() != null)
        {
            return;
        }

        GameObject hintObject = new GameObject("Onboarding Hint System");
        hintObject.AddComponent<OnboardingHintSystem>();
    }

    private void OnEnable()
    {
        if (archiveManager == null)
        {
            archiveManager = FindObjectOfType<ArchiveManager>();
        }

        if (archiveManager != null)
        {
            archiveManager.OnStateChanged += HandleArchiveStateChanged;
        }

        ScanProgressTracker.OnScanCountChanged += HandleScanCountChanged;
    }

    private void OnDisable()
    {
        if (archiveManager != null)
        {
            archiveManager.OnStateChanged -= HandleArchiveStateChanged;
        }

        ScanProgressTracker.OnScanCountChanged -= HandleScanCountChanged;
    }

    public static void NotifyScanOpportunity()
    {
        if (scanHintShown || ScanProgressTracker.ScanCount > 0)
        {
            return;
        }

        scanHintShown = true;
        AppendHint("scan", hintSystem => hintSystem.scanOpportunityHint);
    }

    public static void NotifyCreatureEncounter()
    {
        if (encounterHintShown)
        {
            return;
        }

        encounterHintShown = true;
        AppendHint("encounter", hintSystem => hintSystem.firstEncounterHint);
    }

    private static void AppendHint(string context, System.Func<OnboardingHintSystem, string> hintSelector)
    {
        OnboardingHintSystem hintSystem = FindObjectOfType<OnboardingHintSystem>();
        if (hintSystem == null)
        {
            GameObject hintObject = new GameObject("Onboarding Hint System");
            hintSystem = hintObject.AddComponent<OnboardingHintSystem>();
        }

        string message = hintSelector?.Invoke(hintSystem);
        if (!string.IsNullOrWhiteSpace(message))
        {
            ArchiveLogUI.AppendEntry(message);
        }
        else
        {
            Debug.LogWarning($"Onboarding hint '{context}' was empty.", hintSystem);
        }
    }

    private void HandleArchiveStateChanged(ArchiveManager.ArchiveState state)
    {
        if (doorHintShown || state != ArchiveManager.ArchiveState.PartialOnline)
        {
            return;
        }

        doorHintShown = true;
        if (!string.IsNullOrWhiteSpace(archiveDoorHint))
        {
            ArchiveLogUI.AppendEntry(archiveDoorHint);
        }
    }

    private void HandleScanCountChanged(int scanCount)
    {
        if (scanCount > 0)
        {
            scanHintShown = true;
        }
    }
}
