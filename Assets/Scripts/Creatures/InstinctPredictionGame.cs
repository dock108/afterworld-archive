using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CreatureController))]
public class InstinctPredictionGame : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField, Range(2f, 10f)] private float predictionTimeout = 5f;
    [SerializeField, Range(0.5f, 4f)] private float behaviorDuration = 1.6f;
    [SerializeField, Range(1f, 6f)] private float resultDuration = 3f;

    [Header("Copy")]
    [SerializeField] private string promptTitle = "Instinct Prediction";
    [SerializeField] private string promptBody = "Predict the creature's next instinct.";
    [SerializeField] private string successTitle = "Prediction confirmed";
    [SerializeField] private string successBodyFallback = "Deeper archive readings are now available.";
    [SerializeField] private string failureTitle = "Prediction missed";
    [SerializeField] private string failureBody = "The creature disengages calmly.";

    private CreatureController controller;
    private Coroutine predictionRoutine;
    private CreatureInstinct targetInstinct;
    private CreatureInstinct? predictedInstinct;
    private bool awaitingSelection;

    private void Awake()
    {
        controller = GetComponent<CreatureController>();
    }

    public void BeginPrediction(CreatureData creature)
    {
        if (predictionRoutine != null)
        {
            return;
        }

        targetInstinct = (CreatureInstinct)Random.Range(0, System.Enum.GetValues(typeof(CreatureInstinct)).Length);
        predictedInstinct = null;
        awaitingSelection = true;

        InstinctPredictionUI ui = InstinctPredictionUI.ShowPrompt(
            promptTitle,
            promptBody,
            new[]
            {
                CreatureInstinct.Flee,
                CreatureInstinct.Freeze,
                CreatureInstinct.Climb,
                CreatureInstinct.Investigate
            },
            HandleSelection);

        if (ui == null)
        {
            return;
        }

        predictionRoutine = StartCoroutine(PredictionFlow(creature));
    }

    private void HandleSelection(CreatureInstinct selection)
    {
        if (!awaitingSelection)
        {
            return;
        }

        predictedInstinct = selection;
        awaitingSelection = false;
    }

    private IEnumerator PredictionFlow(CreatureData creature)
    {
        float elapsed = 0f;
        while (awaitingSelection && elapsed < predictionTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        awaitingSelection = false;
        InstinctPredictionUI.HidePrompt();

        if (controller != null)
        {
            controller.PlayInstinctBehavior(targetInstinct, behaviorDuration);
        }

        if (behaviorDuration > 0f)
        {
            yield return new WaitForSeconds(behaviorDuration);
        }

        bool success = predictedInstinct.HasValue && predictedInstinct.Value == targetInstinct;
        if (success)
        {
            string details = creature != null && !string.IsNullOrWhiteSpace(creature.DeepDiveNotes)
                ? creature.DeepDiveNotes
                : successBodyFallback;
            InstinctPredictionUI.ShowResult(successTitle, details, resultDuration);
        }
        else
        {
            InstinctPredictionUI.ShowResult(failureTitle, failureBody, resultDuration);
            controller?.DisengageCalmly();
        }

        if (resultDuration > 0f)
        {
            yield return new WaitForSeconds(resultDuration);
        }

        predictionRoutine = null;
    }
}
