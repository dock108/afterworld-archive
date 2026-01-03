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
        return next;
    }
}
