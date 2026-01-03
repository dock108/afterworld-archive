using UnityEngine;

/// <summary>
/// Emits a short flavor log when a scan completes.
/// </summary>
public class ScanFlavorLog : MonoBehaviour
{
    [SerializeField, TextArea(2, 4)] private string[] messages;
    [SerializeField] private bool randomize = true;

    private ScannableObject scannable;
    private int messageIndex;

    private void Awake()
    {
        scannable = GetComponent<ScannableObject>();
    }

    private void OnEnable()
    {
        if (scannable == null)
        {
            scannable = GetComponent<ScannableObject>();
        }

        if (scannable != null)
        {
            scannable.OnScanComplete.AddListener(HandleScanComplete);
        }
    }

    private void OnDisable()
    {
        if (scannable != null)
        {
            scannable.OnScanComplete.RemoveListener(HandleScanComplete);
        }
    }

    private void HandleScanComplete()
    {
        string message = GetMessage();
        if (!string.IsNullOrWhiteSpace(message))
        {
            ArchiveLogUI.AppendEntry(message);
        }
    }

    private string GetMessage()
    {
        if (messages == null || messages.Length == 0)
        {
            return string.Empty;
        }

        if (messages.Length == 1)
        {
            return messages[0];
        }

        if (randomize)
        {
            return messages[Random.Range(0, messages.Length)];
        }

        string message = messages[Mathf.Clamp(messageIndex, 0, messages.Length - 1)];
        messageIndex = (messageIndex + 1) % messages.Length;
        return message;
    }
}
