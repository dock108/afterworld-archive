using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Vestige/Creatures/Creature Data", fileName = "CreatureData_")]
public class CreatureData : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private CreatureRarity rarity;
    [TextArea(2, 6)]
    [SerializeField] private string encounterNotes;
    [TextArea(2, 8)]
    [SerializeField] private string deepDiveNotes;
    [SerializeField] private string[] behaviors;

    public string Id => id;
    public string DisplayName => displayName;
    public CreatureRarity Rarity => rarity;
    public string EncounterNotes => encounterNotes;
    public string DeepDiveNotes => deepDiveNotes;
    public IReadOnlyList<string> Behaviors => behaviors;
}

public enum CreatureRarity
{
    Common,
    Uncommon,
    Rare,
    Mythic
}
