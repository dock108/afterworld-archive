using UnityEngine;

public class CreatureEncounter : MonoBehaviour
{
    [Header("Progress")]
    [SerializeField] private Animator[] dioramaAnimators;
    [SerializeField] private string partialUnderstandingEntry = "Archive entry unlocked: {0} (partial understanding).";
    [SerializeField] private string fullUnderstandingEntry = "Archive entry completed: {0} (fully understood).";

    [SerializeField] private CreatureDatabase database;
    [SerializeField] private string creatureId;
    [SerializeField] private CreatureData creature;
    [SerializeField] private CreatureSpawnTable spawnTable;
    [SerializeField] private Afterworld.Systems.WorldConditionState worldConditions;
    [SerializeField] private bool rollFromSpawnTableOnAwake = true;
    [SerializeField] private bool isElusive;

    public CreatureData Creature => creature;
    public bool IsElusive => isElusive;

    private void Awake()
    {
        ResolveCreature();
        ApplyDioramaState();
    }

    public void ResolveCreature()
    {
        if (creature != null)
        {
            return;
        }

        if (rollFromSpawnTableOnAwake && spawnTable != null)
        {
            if (worldConditions == null)
            {
                worldConditions = FindObjectOfType<Afterworld.Systems.WorldConditionState>();
            }

            if (spawnTable.TryPickCreature(worldConditions, out CreatureData rolled))
            {
                creature = rolled;
                return;
            }
        }

        if (database == null)
        {
            database = Resources.Load<CreatureDatabase>("CreatureDatabase");
        }

        if (database == null)
        {
            Debug.LogWarning($"{nameof(CreatureEncounter)} on {name} could not find a CreatureDatabase.", this);
            return;
        }

        if (!database.TryGetCreature(creatureId, out CreatureData resolved))
        {
            Debug.LogWarning($"{nameof(CreatureEncounter)} on {name} could not find creature id '{creatureId}'.", this);
            return;
        }

        creature = resolved;
    }

    public void RegisterEncounter()
    {
        if (creature == null)
        {
            ResolveCreature();
        }

        if (creature != null)
        {
            Debug.Log($"Encountered {creature.DisplayName} ({creature.Rarity}).", this);
        }
    }

    public void RegisterMiniGameSuccess()
    {
        if (creature == null)
        {
            ResolveCreature();
        }

        if (creature == null)
        {
            return;
        }

        CreatureUnderstandingState previousState = CreatureArchiveProgress.GetUnderstanding(creature.Id);
        CreatureUnderstandingState newState = CreatureArchiveProgress.AdvanceUnderstanding(creature.Id);

        ApplyDioramaState(newState);
        AppendArchiveEntry(previousState, newState);
    }

    public void MarkElusive()
    {
        isElusive = true;
    }

    private void AppendArchiveEntry(CreatureUnderstandingState previousState, CreatureUnderstandingState newState)
    {
        if (newState == previousState || creature == null)
        {
            return;
        }

        string entry = null;
        switch (newState)
        {
            case CreatureUnderstandingState.Partial:
                entry = partialUnderstandingEntry;
                break;
            case CreatureUnderstandingState.Understood:
                entry = fullUnderstandingEntry;
                break;
        }

        if (!string.IsNullOrWhiteSpace(entry))
        {
            ArchiveLogUI.AppendEntry(string.Format(entry, creature.DisplayName));
        }
    }

    private void ApplyDioramaState()
    {
        if (creature == null)
        {
            return;
        }

        ApplyDioramaState(CreatureArchiveProgress.GetUnderstanding(creature.Id));
    }

    private void ApplyDioramaState(CreatureUnderstandingState state)
    {
        if (dioramaAnimators == null || dioramaAnimators.Length == 0)
        {
            return;
        }

        bool enableAnimation = state != CreatureUnderstandingState.Unknown;
        foreach (Animator animator in dioramaAnimators)
        {
            if (animator != null)
            {
                animator.enabled = enableAnimation;
            }
        }
    }
}
