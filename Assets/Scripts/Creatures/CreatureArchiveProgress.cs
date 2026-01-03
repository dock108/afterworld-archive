using System.Collections.Generic;

public enum CreatureUnderstandingState
{
    Unknown,
    Partial,
    Understood
}

public static class CreatureArchiveProgress
{
    private static readonly Dictionary<string, CreatureUnderstandingState> UnderstandingById =
        new Dictionary<string, CreatureUnderstandingState>();
    private static int cachedKnowledgeTotal;

    public static event System.Action<int> OnKnowledgeChanged;

    public static CreatureUnderstandingState GetUnderstanding(string creatureId)
    {
        if (string.IsNullOrWhiteSpace(creatureId))
        {
            return CreatureUnderstandingState.Unknown;
        }

        return UnderstandingById.TryGetValue(creatureId, out CreatureUnderstandingState state)
            ? state
            : CreatureUnderstandingState.Unknown;
    }

    public static CreatureUnderstandingState AdvanceUnderstanding(string creatureId)
    {
        if (string.IsNullOrWhiteSpace(creatureId))
        {
            return CreatureUnderstandingState.Unknown;
        }

        CreatureUnderstandingState current = GetUnderstanding(creatureId);
        CreatureUnderstandingState next = current switch
        {
            CreatureUnderstandingState.Unknown => CreatureUnderstandingState.Partial,
            CreatureUnderstandingState.Partial => CreatureUnderstandingState.Understood,
            _ => CreatureUnderstandingState.Understood
        };

        UnderstandingById[creatureId] = next;
        NotifyKnowledgeChanged();
        return next;
    }

    public static int GetKnowledgeTotal()
    {
        return CalculateKnowledgeTotal();
    }

    public static List<CreatureKnowledgeRecord> CreateRecords()
    {
        List<CreatureKnowledgeRecord> records = new List<CreatureKnowledgeRecord>();
        foreach (KeyValuePair<string, CreatureUnderstandingState> entry in UnderstandingById)
        {
            records.Add(new CreatureKnowledgeRecord
            {
                CreatureId = entry.Key,
                State = entry.Value
            });
        }

        return records;
    }

    public static void LoadRecords(IEnumerable<CreatureKnowledgeRecord> records)
    {
        UnderstandingById.Clear();
        if (records != null)
        {
            foreach (CreatureKnowledgeRecord record in records)
            {
                if (record == null || string.IsNullOrWhiteSpace(record.CreatureId))
                {
                    continue;
                }

                UnderstandingById[record.CreatureId] = record.State;
            }
        }

        NotifyKnowledgeChanged();
    }

    private static void NotifyKnowledgeChanged()
    {
        int total = CalculateKnowledgeTotal();
        if (total == cachedKnowledgeTotal)
        {
            return;
        }

        cachedKnowledgeTotal = total;
        OnKnowledgeChanged?.Invoke(total);
    }

    private static int CalculateKnowledgeTotal()
    {
        int total = 0;
        foreach (CreatureUnderstandingState state in UnderstandingById.Values)
        {
            total += state switch
            {
                CreatureUnderstandingState.Partial => 1,
                CreatureUnderstandingState.Understood => 2,
                _ => 0
            };
        }

        return total;
    }
}
