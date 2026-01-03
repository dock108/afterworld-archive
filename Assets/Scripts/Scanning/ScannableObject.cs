using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Represents an object that can be scanned and emits scan lifecycle events.
/// </summary>
public class ScannableObject : MonoBehaviour
{
    [Header("Scan Events")]
    public UnityEvent OnScanStart;
    public UnityEvent OnScanComplete;

    public void TriggerScanStart()
    {
        OnScanStart?.Invoke();
    }

    public void TriggerScanComplete()
    {
        OnScanComplete?.Invoke();
    }
}
