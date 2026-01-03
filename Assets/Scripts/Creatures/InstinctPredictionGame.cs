using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CreatureController))]
public class InstinctPredictionGame : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField, Range(2f, 10f)] private float commonTimeout = 6f;
    [SerializeField, Range(2f, 10f)] private float uncommonTimeout = 5f;
    [SerializeField, Range(2f, 10f)] private float rareTimeout = 4f;
    [SerializeField, Range(1f, 10f)] private float mythicTimeout = 3.5f;
    [SerializeField, Range(0.5f, 4f)] private float behaviorDuration = 1.6f;
    [SerializeField, Range(1f, 6f)] private float resultDuration = 3f;

    [Header("Difficulty")]
    [SerializeField, Range(2, 4)] private int commonOptionCount = 3;
    [SerializeField, Range(2, 4)] private int uncommonOptionCount = 4;
    [SerializeField, Range(2, 4)] private int rareOptionCount = 4;
    [SerializeField, Range(2, 4)] private int mythicOptionCount = 4;

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

        CreatureRarity rarity = creature != null ? creature.Rarity : CreatureRarity.Common;
        targetInstinct = (CreatureInstinct)Random.Range(0, System.Enum.GetValues(typeof(CreatureInstinct)).Length);
        predictedInstinct = null;
        awaitingSelection = true;

        float predictionTimeout = GetPredictionTimeout(rarity);
        InstinctPredictionUI ui = InstinctPredictionUI.ShowPrompt(
            promptTitle,
            promptBody,
            BuildOptions(rarity, targetInstinct),
            HandleSelection);

        if (ui == null)
        {
            return;
        }

        predictionRoutine = StartCoroutine(PredictionFlow(creature, predictionTimeout));
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

    private IEnumerator PredictionFlow(CreatureData creature, float predictionTimeout)
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

    private CreatureInstinct[] BuildOptions(CreatureRarity rarity, CreatureInstinct guaranteed)
    {
        int optionCount = Mathf.Clamp(GetOptionCount(rarity), 2, System.Enum.GetValues(typeof(CreatureInstinct)).Length);
        CreatureInstinct[] allInstincts = new[]
        {
            CreatureInstinct.Flee,
            CreatureInstinct.Freeze,
            CreatureInstinct.Climb,
            CreatureInstinct.Investigate
        };

        if (optionCount >= allInstincts.Length)
        {
            return allInstincts;
        }

        CreatureInstinct[] options = new CreatureInstinct[optionCount];
        options[0] = guaranteed;
        int filled = 1;
        while (filled < optionCount)
        {
            CreatureInstinct candidate = allInstincts[Random.Range(0, allInstincts.Length)];
            bool exists = false;
            for (int i = 0; i < filled; i++)
            {
                if (options[i] == candidate)
                {
                    exists = true;
                    break;
                }
            }

            if (exists)
            {
                continue;
            }

            options[filled] = candidate;
            filled++;
        }

        for (int i = 0; i < options.Length; i++)
        {
            int swapIndex = Random.Range(i, options.Length);
            CreatureInstinct temp = options[i];
            options[i] = options[swapIndex];
            options[swapIndex] = temp;
        }

        return options;
    }

    private float GetPredictionTimeout(CreatureRarity rarity)
    {
        switch (rarity)
        {
            case CreatureRarity.Uncommon:
                return uncommonTimeout;
            case CreatureRarity.Rare:
                return rareTimeout;
            case CreatureRarity.Mythic:
                return mythicTimeout;
            default:
                return commonTimeout;
        }
    }

    private int GetOptionCount(CreatureRarity rarity)
    {
        switch (rarity)
        {
            case CreatureRarity.Uncommon:
                return uncommonOptionCount;
            case CreatureRarity.Rare:
                return rareOptionCount;
            case CreatureRarity.Mythic:
                return mythicOptionCount;
            default:
                return commonOptionCount;
        }
    }
}
