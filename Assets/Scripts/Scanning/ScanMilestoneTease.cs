using UnityEngine;

/// <summary>
/// Triggers a short glitch tease and terminal message on a milestone scan.
/// </summary>
public class ScanMilestoneTease : MonoBehaviour
{
    [SerializeField] private HoldToScanSystem scanSystem;
    [SerializeField, Min(1)] private int milestoneScanIndex = 3;
    [SerializeField] private float glitchDuration = 0.35f;
    [SerializeField] private float glitchMaxOffset = 6f;
    [SerializeField] private float glitchMinInterval = 0.03f;
    [SerializeField] private float glitchMaxInterval = 0.08f;

    private int lastScanCount;
    private bool hasTriggeredMilestone;

    private void OnEnable()
    {
        if (scanSystem == null)
        {
            scanSystem = FindObjectOfType<HoldToScanSystem>();
        }

        if (scanSystem != null)
        {
            scanSystem.OnScanCompleted.AddListener(HandleScanCompleted);
        }

        lastScanCount = ScanProgressTracker.ScanCount;
        hasTriggeredMilestone = milestoneScanIndex > 0 && lastScanCount >= milestoneScanIndex;
        ScanProgressTracker.OnScanCountChanged += HandleScanCountChanged;
    }

    private void OnDisable()
    {
        if (scanSystem != null)
        {
            scanSystem.OnScanCompleted.RemoveListener(HandleScanCompleted);
        }

        ScanProgressTracker.OnScanCountChanged -= HandleScanCountChanged;
    }

    private void HandleScanCompleted()
    {
        HandleScanCountChanged(ScanProgressTracker.ScanCount);
    }

    private void HandleScanCountChanged(int scanCount)
    {
        if (hasTriggeredMilestone || milestoneScanIndex <= 0)
        {
            lastScanCount = scanCount;
            return;
        }

        if (lastScanCount < milestoneScanIndex && scanCount >= milestoneScanIndex)
        {
            hasTriggeredMilestone = true;
            TriggerMilestone();
        }

        lastScanCount = scanCount;
    }

    private void TriggerMilestone()
    {
        ArchiveLogUI.AppendEntry("Milestone logged. I will act surprised.");
        ArchiveLogUI.TriggerGlitch(glitchDuration, glitchMaxOffset, glitchMinInterval, glitchMaxInterval);
    }
}
