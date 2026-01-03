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

    private int scanCount;

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
    }

    private void OnDisable()
    {
        if (scanSystem != null)
        {
            scanSystem.OnScanCompleted.RemoveListener(HandleScanCompleted);
        }
    }

    private void HandleScanCompleted()
    {
        if (milestoneScanIndex <= 0)
        {
            return;
        }

        scanCount++;
        if (scanCount != milestoneScanIndex)
        {
            return;
        }

        ArchiveLogUI.AppendEntry("Milestone logged. I will act surprised.");
        ArchiveLogUI.TriggerGlitch(glitchDuration, glitchMaxOffset, glitchMinInterval, glitchMaxInterval);
    }
}
