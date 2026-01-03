using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Handles holding input to scan a target with progress UI and audio feedback.
/// </summary>
public class HoldToScanSystem : MonoBehaviour
{
    [Header("Scan Input")]
    [SerializeField] private string inputButton = "Fire1";

    [Header("Scan Timing")]
    [SerializeField, Range(1.5f, 2f)] private float scanDurationSeconds = 1.8f;

    [Header("Targeting")]
    [SerializeField] private Transform scanOrigin;
    [SerializeField] private float maxScanDistance = 6f;
    [SerializeField, Range(0f, 20f)] private float maxAimAngle = 7f;

    [Header("UI")]
    [SerializeField] private CanvasGroup scanUiGroup;
    [SerializeField] private Image progressFill;

    [Header("Audio")]
    [SerializeField] private AudioSource scanAudioSource;
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.2f;
    [SerializeField] private float minVolume = 0.2f;
    [SerializeField] private float maxVolume = 0.6f;

    [Header("Events")]
    public UnityEvent OnScanCompleted;

    private float scanElapsed;
    private ScanTarget currentTarget;
    private Vector3 initialAimDirection;

    private void Awake()
    {
        if (scanOrigin == null && Camera.main != null)
        {
            scanOrigin = Camera.main.transform;
        }

        SetUiVisible(false);
        UpdateProgressUi(0f);

        if (scanAudioSource != null)
        {
            scanAudioSource.loop = true;
            scanAudioSource.playOnAwake = false;
            scanAudioSource.volume = 0f;
        }
    }

    private void Update()
    {
        if (!Input.GetButton(inputButton))
        {
            CancelScan();
            return;
        }

        if (currentTarget == null)
        {
            TryBeginScan();
            return;
        }

        if (!IsTargetStillValid())
        {
            CancelScan();
            return;
        }

        scanElapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(scanElapsed / scanDurationSeconds);
        UpdateProgressUi(progress);
        UpdateScanAudio(progress);

        if (progress >= 1f)
        {
            CompleteScan();
        }
    }

    private void TryBeginScan()
    {
        if (scanOrigin == null)
        {
            return;
        }

        if (TryGetTarget(out ScanTarget target))
        {
            currentTarget = target;
            scanElapsed = 0f;
            initialAimDirection = (target.ScanPoint - scanOrigin.position).normalized;
            SetUiVisible(true);
            UpdateProgressUi(0f);
            StartScanAudio();
        }
    }

    private bool TryGetTarget(out ScanTarget target)
    {
        target = null;

        Ray ray = new Ray(scanOrigin.position, scanOrigin.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxScanDistance))
        {
            return false;
        }

        target = hit.collider.GetComponentInParent<ScanTarget>();
        return target != null;
    }

    private bool IsTargetStillValid()
    {
        if (scanOrigin == null || currentTarget == null)
        {
            return false;
        }

        Vector3 toTarget = currentTarget.ScanPoint - scanOrigin.position;
        float distance = toTarget.magnitude;
        if (distance > maxScanDistance)
        {
            return false;
        }

        Vector3 direction = toTarget.normalized;
        float aimAngle = Vector3.Angle(scanOrigin.forward, direction);
        if (aimAngle > maxAimAngle)
        {
            return false;
        }

        float driftAngle = Vector3.Angle(initialAimDirection, direction);
        if (driftAngle > maxAimAngle)
        {
            return false;
        }

        return true;
    }

    private void CompleteScan()
    {
        OnScanCompleted?.Invoke();
        CancelScan();
    }

    private void CancelScan()
    {
        if (currentTarget == null && scanElapsed <= 0f)
        {
            return;
        }

        currentTarget = null;
        scanElapsed = 0f;
        UpdateProgressUi(0f);
        SetUiVisible(false);
        StopScanAudio();
    }

    private void UpdateProgressUi(float progress)
    {
        if (progressFill != null)
        {
            progressFill.fillAmount = progress;
        }
    }

    private void SetUiVisible(bool visible)
    {
        if (scanUiGroup == null)
        {
            return;
        }

        scanUiGroup.alpha = visible ? 1f : 0f;
        scanUiGroup.interactable = visible;
        scanUiGroup.blocksRaycasts = visible;
    }

    private void StartScanAudio()
    {
        if (scanAudioSource == null || scanAudioSource.clip == null)
        {
            return;
        }

        scanAudioSource.volume = minVolume;
        scanAudioSource.pitch = minPitch;
        scanAudioSource.Play();
    }

    private void UpdateScanAudio(float progress)
    {
        if (scanAudioSource == null || !scanAudioSource.isPlaying)
        {
            return;
        }

        scanAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, progress);
        scanAudioSource.volume = Mathf.Lerp(minVolume, maxVolume, progress);
    }

    private void StopScanAudio()
    {
        if (scanAudioSource == null)
        {
            return;
        }

        if (scanAudioSource.isPlaying)
        {
            scanAudioSource.Stop();
        }

        scanAudioSource.volume = 0f;
    }
}
