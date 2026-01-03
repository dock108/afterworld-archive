using UnityEngine;

public static class ScanProgressTracker
{
    public static event System.Action<int> OnScanCountChanged;

    private static int scanCount;

    public static int ScanCount => scanCount;

    public static void RegisterScan()
    {
        SetCount(scanCount + 1);
    }

    public static void SetCount(int count)
    {
        int normalized = Mathf.Max(0, count);
        if (normalized == scanCount)
        {
            return;
        }

        scanCount = normalized;
        OnScanCountChanged?.Invoke(scanCount);
    }
}
