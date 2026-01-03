using System.Collections.Generic;
using Afterworld.Systems;
using UnityEngine;

[CreateAssetMenu(menuName = "Afterworld/Creatures/Creature Spawn Table", fileName = "CreatureSpawnTable_")]
public class CreatureSpawnTable : ScriptableObject
{
    [System.Serializable]
    public class SpawnEntry
    {
        [SerializeField] private CreatureData creature;
        [SerializeField, Min(0)] private int weight = 1;
        [SerializeField] private WorldTimeOfDay[] allowedTimes;
        [SerializeField] private WorldWeather[] allowedWeather;

        public CreatureData Creature => creature;
        public int Weight => weight;

        public bool Matches(WorldConditionState conditions)
        {
            if (creature == null)
            {
                return false;
            }

            if (conditions == null)
            {
                return true;
            }

            if (allowedTimes != null && allowedTimes.Length > 0 && !Contains(allowedTimes, conditions.TimeOfDay))
            {
                return false;
            }

            if (allowedWeather != null && allowedWeather.Length > 0 && !Contains(allowedWeather, conditions.Weather))
            {
                return false;
            }

            return true;
        }

        private static bool Contains<T>(T[] collection, T value)
        {
            if (collection == null)
            {
                return false;
            }

            for (int i = 0; i < collection.Length; i++)
            {
                if (EqualityComparer<T>.Default.Equals(collection[i], value))
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Header("Weights by Rarity")]
    [SerializeField, Min(0)] private int commonRarityWeight = 60;
    [SerializeField, Min(0)] private int uncommonRarityWeight = 25;
    [SerializeField, Min(0)] private int rareRarityWeight = 12;
    [SerializeField, Min(0)] private int mythicRarityWeight = 3;

    [Header("Spawn Entries")]
    [SerializeField] private List<SpawnEntry> entries = new List<SpawnEntry>();

    public IReadOnlyList<SpawnEntry> Entries => entries;

    public bool TryPickCreature(WorldConditionState conditions, out CreatureData creature)
    {
        creature = null;
        if (entries == null || entries.Count == 0)
        {
            return false;
        }

        int totalWeight = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            SpawnEntry entry = entries[i];
            if (entry == null || !entry.Matches(conditions))
            {
                continue;
            }

            int entryWeight = Mathf.Max(0, entry.Weight) * GetRarityWeight(entry.Creature.Rarity);
            if (entryWeight <= 0)
            {
                continue;
            }

            totalWeight += entryWeight;
        }

        if (totalWeight <= 0)
        {
            return false;
        }

        int roll = Random.Range(0, totalWeight);
        for (int i = 0; i < entries.Count; i++)
        {
            SpawnEntry entry = entries[i];
            if (entry == null || !entry.Matches(conditions))
            {
                continue;
            }

            int entryWeight = Mathf.Max(0, entry.Weight) * GetRarityWeight(entry.Creature.Rarity);
            if (entryWeight <= 0)
            {
                continue;
            }

            roll -= entryWeight;
            if (roll < 0)
            {
                creature = entry.Creature;
                return creature != null;
            }
        }

        return false;
    }

    private int GetRarityWeight(CreatureRarity rarity)
    {
        switch (rarity)
        {
            case CreatureRarity.Uncommon:
                return uncommonRarityWeight;
            case CreatureRarity.Rare:
                return rareRarityWeight;
            case CreatureRarity.Mythic:
                return mythicRarityWeight;
            default:
                return commonRarityWeight;
        }
    }
}
