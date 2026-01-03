using UnityEngine;

public class CreatureEncounter : MonoBehaviour
{
    [SerializeField] private CreatureDatabase database;
    [SerializeField] private string creatureId;
    [SerializeField] private CreatureData creature;

    public CreatureData Creature => creature;

    private void Awake()
    {
        ResolveCreature();
    }

    public void ResolveCreature()
    {
        if (creature != null)
        {
            return;
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
}
