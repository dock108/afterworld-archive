using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveGameData
{
    public int Version = 1;
    public bool HasPlayerTransform;
    public Vector3 PlayerPosition;
    public Quaternion PlayerRotation;
    public ArchiveManager.ArchiveState ArchiveState = ArchiveManager.ArchiveState.Offline;
    public int ScanCount;
    public List<CreatureKnowledgeRecord> CreatureKnowledge = new List<CreatureKnowledgeRecord>();
    public List<EncounterFailureRecord> EncounterFailures = new List<EncounterFailureRecord>();
    public List<ElusiveRecordData> ElusiveRecords = new List<ElusiveRecordData>();
}

[Serializable]
public class CreatureKnowledgeRecord
{
    public string CreatureId;
    public CreatureUnderstandingState State;
}

[Serializable]
public class EncounterFailureRecord
{
    public string CreatureId;
    public int FailureCount;
}

[Serializable]
public class ElusiveRecordData
{
    public string CreatureId;
    public float RemainingCooldown;
    public float MinChance;
    public float MaxChance;
    public float RampDuration;
}
