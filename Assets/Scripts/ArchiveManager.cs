using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the archive activation state machine and visual beats.
/// </summary>
public class ArchiveManager : MonoBehaviour
{
    public enum ArchiveState
    {
        Offline,
        Flicker,
        PartialOnline
    }

    public event System.Action<ArchiveState> OnStateChanged;

    [Header("State")]
    [SerializeField] private ArchiveState startingState = ArchiveState.Offline;

    [Header("Scan Trigger")]
    [SerializeField] private HoldToScanSystem scanSystem;

    [Header("Lighting")]
    [SerializeField] private Light[] archiveLights;
    [SerializeField] private float poweredIntensity = 1.2f;
    [SerializeField] private float flickerDuration = 1.4f;
    [SerializeField] private Vector2 flickerIntervalRange = new Vector2(0.05f, 0.15f);
    [SerializeField] private Vector2 flickerIntensityRange = new Vector2(0.15f, 1f);

    [Header("Screens")]
    [SerializeField] private GameObject[] screenObjects;
    [SerializeField] private float screenBootDelay = 0.2f;
    [SerializeField] private float screenBootStepDelay = 0.25f;

    [Header("Archive Door")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string doorOpenTrigger = "Open";
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Vector3 doorOpenEuler = new Vector3(0f, 90f, 0f);
    [SerializeField] private float doorOpenDuration = 1.2f;

    private ArchiveState currentState;
    private bool hasActivated;
    private Coroutine transitionRoutine;
    private Quaternion doorClosedRotation;

    public ArchiveState CurrentState => currentState;

    private void Awake()
    {
        if (doorTransform != null)
        {
            doorClosedRotation = doorTransform.localRotation;
        }

        ApplyState(startingState, true);
        currentState = startingState;
        hasActivated = startingState != ArchiveState.Offline;
    }

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

    public void HandleScanCompleted()
    {
        if (hasActivated)
        {
            return;
        }

        hasActivated = true;
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }

        transitionRoutine = StartCoroutine(TransitionAfterFirstScan());
    }

    private IEnumerator TransitionAfterFirstScan()
    {
        SetState(ArchiveState.Flicker);
        yield return FlickerLights();
        yield return BootScreens();
        yield return OpenDoor();
        SetState(ArchiveState.PartialOnline);
    }

    private void SetState(ArchiveState newState)
    {
        currentState = newState;
        ApplyState(newState, false);
        OnStateChanged?.Invoke(newState);
    }

    private void ApplyState(ArchiveState state, bool immediate)
    {
        switch (state)
        {
            case ArchiveState.Offline:
                SetLightsEnabled(false);
                SetLightsIntensity(0f);
                SetScreensActive(false);
                CloseDoor();
                break;
            case ArchiveState.Flicker:
                SetScreensActive(false);
                SetLightsEnabled(true);
                if (immediate)
                {
                    SetLightsIntensity(0f);
                }
                break;
            case ArchiveState.PartialOnline:
                SetLightsEnabled(true);
                SetLightsIntensity(poweredIntensity);
                SetScreensActive(true);
                break;
        }
    }

    private void SetLightsEnabled(bool enabled)
    {
        if (archiveLights == null)
        {
            return;
        }

        foreach (Light lightSource in archiveLights)
        {
            if (lightSource != null)
            {
                lightSource.enabled = enabled;
            }
        }
    }

    private void SetLightsIntensity(float intensity)
    {
        if (archiveLights == null)
        {
            return;
        }

        foreach (Light lightSource in archiveLights)
        {
            if (lightSource != null)
            {
                lightSource.intensity = intensity;
            }
        }
    }

    private void SetScreensActive(bool isActive)
    {
        if (screenObjects == null)
        {
            return;
        }

        foreach (GameObject screen in screenObjects)
        {
            if (screen != null)
            {
                screen.SetActive(isActive);
            }
        }
    }

    private IEnumerator FlickerLights()
    {
        float elapsed = 0f;
        SetLightsEnabled(true);

        while (elapsed < flickerDuration)
        {
            float intensityScale = Random.Range(flickerIntensityRange.x, flickerIntensityRange.y);
            SetLightsIntensity(poweredIntensity * intensityScale);

            float waitTime = Random.Range(flickerIntervalRange.x, flickerIntervalRange.y);
            elapsed += waitTime;
            yield return new WaitForSeconds(waitTime);
        }

        SetLightsIntensity(poweredIntensity);
    }

    private IEnumerator BootScreens()
    {
        if (screenObjects == null || screenObjects.Length == 0)
        {
            yield break;
        }

        SetScreensActive(false);
        if (screenBootDelay > 0f)
        {
            yield return new WaitForSeconds(screenBootDelay);
        }

        foreach (GameObject screen in screenObjects)
        {
            if (screen != null)
            {
                screen.SetActive(true);
            }

            if (screenBootStepDelay > 0f)
            {
                yield return new WaitForSeconds(screenBootStepDelay);
            }
        }
    }

    private IEnumerator OpenDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(doorOpenTrigger);
            yield break;
        }

        if (doorTransform == null)
        {
            yield break;
        }

        Quaternion startRotation = doorClosedRotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(doorOpenEuler);
        float elapsed = 0f;

        while (elapsed < doorOpenDuration)
        {
            float t = elapsed / Mathf.Max(doorOpenDuration, 0.01f);
            doorTransform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        doorTransform.localRotation = targetRotation;
    }

    private void CloseDoor()
    {
        if (doorTransform != null)
        {
            doorTransform.localRotation = doorClosedRotation;
        }
    }
}
