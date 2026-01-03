using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Afterworld/Creatures/Creature Data", fileName = "CreatureData_")]
public class CreatureData : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private CreatureRarity rarity;
    [TextArea(2, 6)]
    [SerializeField] private string encounterNotes;
    [SerializeField] private string[] behaviors;

    public string Id => id;
    public string DisplayName => displayName;
    public CreatureRarity Rarity => rarity;
    public string EncounterNotes => encounterNotes;
    public IReadOnlyList<string> Behaviors => behaviors;
}

public enum CreatureRarity
{
    Common,
    Uncommon,
    Rare
}
