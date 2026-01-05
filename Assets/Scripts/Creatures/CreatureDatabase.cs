using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Vestige/Creatures/Creature Database", fileName = "CreatureDatabase")]
public class CreatureDatabase : ScriptableObject
{
    [SerializeField] private List<CreatureData> creatures = new List<CreatureData>();

    public IReadOnlyList<CreatureData> Creatures => creatures;

    public bool TryGetCreature(string creatureId, out CreatureData creature)
    {
        creature = null;
        if (string.IsNullOrWhiteSpace(creatureId))
        {
            return false;
        }

        foreach (CreatureData entry in creatures)
        {
            if (entry != null && entry.Id == creatureId)
            {
                creature = entry;
                return true;
            }
        }

        return false;
    }
}
