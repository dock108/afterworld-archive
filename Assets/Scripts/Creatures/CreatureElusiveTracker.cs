using System.Collections.Generic;
using UnityEngine;

public static class CreatureElusiveTracker
{
    private class ElusiveRecord
    {
        public float EligibleTime;
        public float MinChance;
        public float MaxChance;
        public float RampDuration;
    }

    private static readonly Dictionary<string, ElusiveRecord> Records = new Dictionary<string, ElusiveRecord>();

    public static void MarkElusive(string creatureId, float cooldownMin, float cooldownMax, float minChance, float maxChance, float rampDuration)
    {
        if (string.IsNullOrWhiteSpace(creatureId))
        {
            return;
        }

        float normalizedCooldownMin = Mathf.Max(0f, cooldownMin);
        float normalizedCooldownMax = Mathf.Max(normalizedCooldownMin, cooldownMax);
        float normalizedMinChance = Mathf.Clamp01(minChance);
        float normalizedMaxChance = Mathf.Clamp(normalizedMinChance, 0f, 1f);

        float delay = normalizedCooldownMax > 0f
            ? Random.Range(normalizedCooldownMin, normalizedCooldownMax)
            : 0f;

        Records[creatureId] = new ElusiveRecord
        {
            EligibleTime = Time.time + delay,
            MinChance = normalizedMinChance,
            MaxChance = normalizedMaxChance,
            RampDuration = Mathf.Max(0f, rampDuration)
        };
    }

    public static bool CanSpawn(string creatureId)
    {
        if (string.IsNullOrWhiteSpace(creatureId))
        {
            return true;
        }

        if (!Records.TryGetValue(creatureId, out ElusiveRecord record))
        {
            return true;
        }

        if (Time.time < record.EligibleTime)
        {
            return false;
        }

        float elapsed = Time.time - record.EligibleTime;
        float progress = record.RampDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / record.RampDuration);
        float chance = Mathf.Lerp(record.MinChance, record.MaxChance, progress);
        return Random.value <= chance;
    }

    public static void ClearElusive(string creatureId)
    {
        if (string.IsNullOrWhiteSpace(creatureId))
        {
            return;
        }

        Records.Remove(creatureId);
    }

    public static List<ElusiveRecordData> CreateRecords()
    {
        List<ElusiveRecordData> records = new List<ElusiveRecordData>();
        foreach (KeyValuePair<string, ElusiveRecord> entry in Records)
        {
            float remaining = Mathf.Max(0f, entry.Value.EligibleTime - Time.time);
            records.Add(new ElusiveRecordData
            {
                CreatureId = entry.Key,
                RemainingCooldown = remaining,
                MinChance = entry.Value.MinChance,
                MaxChance = entry.Value.MaxChance,
                RampDuration = entry.Value.RampDuration
            });
        }

        return records;
    }

    public static void LoadRecords(IEnumerable<ElusiveRecordData> records)
    {
        Records.Clear();
        if (records == null)
        {
            return;
        }

        foreach (ElusiveRecordData record in records)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.CreatureId))
            {
                continue;
            }

            Records[record.CreatureId] = new ElusiveRecord
            {
                EligibleTime = Time.time + Mathf.Max(0f, record.RemainingCooldown),
                MinChance = Mathf.Clamp01(record.MinChance),
                MaxChance = Mathf.Clamp(record.MaxChance, 0f, 1f),
                RampDuration = Mathf.Max(0f, record.RampDuration)
            };
        }
    }
}
