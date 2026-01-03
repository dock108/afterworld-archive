using System.Collections;
using UnityEngine;

/// <summary>
/// Powers on a gated object once archive knowledge reaches a threshold.
/// </summary>
public class ArchiveKnowledgeGate : MonoBehaviour
{
    [Header("Threshold")]
    [SerializeField] private int knowledgeThreshold = 3;

    [Header("Activation")]
    [SerializeField] private float activationDelay = 0.75f;
    [SerializeField] private Animator activationAnimator;
    [SerializeField] private string activationTrigger = "PowerOn";
    [SerializeField] private GameObject[] poweredObjects;
    [SerializeField] private bool disableObjectsOnStart = true;

    private bool isPowered;
    private Coroutine activationRoutine;

    private void Awake()
    {
        if (disableObjectsOnStart)
        {
            SetPoweredObjectsActive(false);
        }
    }

    private void OnEnable()
    {
        CreatureArchiveProgress.OnKnowledgeChanged += HandleKnowledgeChanged;
        HandleKnowledgeChanged(CreatureArchiveProgress.GetKnowledgeTotal());
    }

    private void OnDisable()
    {
        CreatureArchiveProgress.OnKnowledgeChanged -= HandleKnowledgeChanged;
    }

    private void HandleKnowledgeChanged(int totalKnowledge)
    {
        if (isPowered || totalKnowledge < knowledgeThreshold)
        {
            return;
        }

        if (activationRoutine != null)
        {
            StopCoroutine(activationRoutine);
        }

        activationRoutine = StartCoroutine(PowerOnSequence());
    }

    private IEnumerator PowerOnSequence()
    {
        if (activationDelay > 0f)
        {
            yield return new WaitForSeconds(activationDelay);
        }

        if (activationAnimator != null && !string.IsNullOrWhiteSpace(activationTrigger))
        {
            activationAnimator.SetTrigger(activationTrigger);
        }

        SetPoweredObjectsActive(true);
        isPowered = true;
    }

    private void SetPoweredObjectsActive(bool isActive)
    {
        if (poweredObjects == null)
        {
            return;
        }

        foreach (GameObject poweredObject in poweredObjects)
        {
            if (poweredObject != null)
            {
                poweredObject.SetActive(isActive);
            }
        }
    }
}
