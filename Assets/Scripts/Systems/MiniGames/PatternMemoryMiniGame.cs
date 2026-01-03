using System.Collections;
using UnityEngine;

namespace Afterworld.Systems.MiniGames
{
    [RequireComponent(typeof(CreatureController))]
    public class PatternMemoryMiniGame : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CreatureEncounter encounter;
        [SerializeField] private CreatureController creatureController;

        [Header("Timing")]
        [SerializeField, Range(0.3f, 1.5f)] private float revealInterval = 0.75f;
        [SerializeField, Range(2f, 12f)] private float inputTimeout = 6f;
        [SerializeField, Range(1f, 6f)] private float resultDuration = 3f;

        [Header("Difficulty")]
        [SerializeField, Range(2, 8)] private int commonSteps = 3;
        [SerializeField, Range(2, 8)] private int uncommonSteps = 4;
        [SerializeField, Range(2, 8)] private int rareSteps = 5;
        [SerializeField, Range(2, 10)] private int mythicSteps = 6;

        [Header("Options")]
        [SerializeField] private string[] optionLabels = { "Signal 1", "Signal 2", "Signal 3", "Signal 4" };

        [Header("Copy")]
        [SerializeField] private string promptTitle = "Pattern Memory";
        [SerializeField] private string observeBody = "Observe the pattern as it appears.";
        [SerializeField] private string repeatBody = "Repeat the pattern.";
        [SerializeField] private string successTitle = "Pattern matched";
        [SerializeField] private string successBody = "Archive resonance stabilizes.";
        [SerializeField] private string failureTitle = "Pattern fades";
        [SerializeField] private string failureBody = "The creature slips away, just out of reach.";

        private Coroutine memoryRoutine;
        private int[] expectedSequence;
        private int sequenceIndex;
        private bool awaitingInput;
        private bool resolved;

        private void Awake()
        {
            if (encounter == null)
            {
                encounter = GetComponent<CreatureEncounter>();
            }

            if (creatureController == null)
            {
                creatureController = GetComponent<CreatureController>();
            }
        }

        public void BeginMemoryChallenge(CreatureData creature)
        {
            if (memoryRoutine != null || optionLabels == null || optionLabels.Length == 0)
            {
                return;
            }

            CreatureRarity rarity = creature != null ? creature.Rarity : CreatureRarity.Common;
            int steps = GetStepCount(rarity);
            expectedSequence = new int[steps];
            string[] sequenceLabels = new string[steps];
            for (int i = 0; i < steps; i++)
            {
                int selection = Random.Range(0, optionLabels.Length);
                expectedSequence[i] = selection;
                sequenceLabels[i] = optionLabels[selection];
            }

            sequenceIndex = 0;
            awaitingInput = false;
            resolved = false;

            InstinctPredictionUI ui = InstinctPredictionUI.ShowMemorySequence(
                promptTitle,
                observeBody,
                repeatBody,
                sequenceLabels,
                optionLabels,
                revealInterval,
                HandleSequenceReady,
                HandleSelection);

            if (ui == null)
            {
                return;
            }

            memoryRoutine = StartCoroutine(MemoryFlow());
        }

        private void HandleSequenceReady()
        {
            awaitingInput = true;
        }

        private void HandleSelection(int index)
        {
            if (!awaitingInput || resolved || expectedSequence == null)
            {
                return;
            }

            if (index != expectedSequence[sequenceIndex])
            {
                resolved = true;
                awaitingInput = false;
                return;
            }

            sequenceIndex++;
            if (sequenceIndex >= expectedSequence.Length)
            {
                resolved = true;
                awaitingInput = false;
            }
        }

        private IEnumerator MemoryFlow()
        {
            float elapsed = 0f;
            while (!resolved)
            {
                if (awaitingInput && inputTimeout > 0f)
                {
                    elapsed += Time.deltaTime;
                    if (elapsed >= inputTimeout)
                    {
                        resolved = true;
                        awaitingInput = false;
                    }
                }

                yield return null;
            }

            bool success = expectedSequence != null && sequenceIndex >= expectedSequence.Length;
            InstinctPredictionUI.HidePrompt();

            if (success)
            {
                InstinctPredictionUI.ShowResult(successTitle, successBody, resultDuration);
                encounter?.RegisterMiniGameSuccess();
            }
            else
            {
                InstinctPredictionUI.ShowResult(failureTitle, failureBody, resultDuration);
                encounter?.MarkElusive();
            }

            if (resultDuration > 0f)
            {
                yield return new WaitForSeconds(resultDuration);
            }

            creatureController?.DisengageCalmly();
            memoryRoutine = null;
        }

        private int GetStepCount(CreatureRarity rarity)
        {
            switch (rarity)
            {
                case CreatureRarity.Uncommon:
                    return Mathf.Max(1, uncommonSteps);
                case CreatureRarity.Rare:
                    return Mathf.Max(1, rareSteps);
                case CreatureRarity.Mythic:
                    return Mathf.Max(1, mythicSteps);
                default:
                    return Mathf.Max(1, commonSteps);
            }
        }
    }
}
