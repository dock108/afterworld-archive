using System.Collections.Generic;

public static class EncounterFailureTracker
{
    private static readonly Dictionary<string, int> FailuresByCreatureId = new Dictionary<string, int>();

    public static event System.Action<string, int> OnFailureLogged;

    public static int GetFailureCount(string creatureId)
    {
        if (string.IsNullOrWhiteSpace(creatureId))
        {
            return 0;
        }

        return FailuresByCreatureId.TryGetValue(creatureId, out int count) ? count : 0;
    }

    public static void RegisterFailure(string creatureId)
    {
        if (string.IsNullOrWhiteSpace(creatureId))
        {
            return;
        }

        int next = GetFailureCount(creatureId) + 1;
        FailuresByCreatureId[creatureId] = next;
        OnFailureLogged?.Invoke(creatureId, next);
    }

    public static List<EncounterFailureRecord> CreateRecords()
    {
        List<EncounterFailureRecord> records = new List<EncounterFailureRecord>();
        foreach (KeyValuePair<string, int> entry in FailuresByCreatureId)
        {
            records.Add(new EncounterFailureRecord
            {
                CreatureId = entry.Key,
                FailureCount = entry.Value
            });
        }

        return records;
    }

    public static void LoadRecords(IEnumerable<EncounterFailureRecord> records)
    {
        FailuresByCreatureId.Clear();
        if (records == null)
        {
            return;
        }

        foreach (EncounterFailureRecord record in records)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.CreatureId))
            {
                continue;
            }

            FailuresByCreatureId[record.CreatureId] = record.FailureCount;
        }
    }
}
