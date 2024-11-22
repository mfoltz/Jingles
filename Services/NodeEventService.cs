using ProjectM;
using ProjectM.Scripting;
using Stunlock.Core;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Nocturnalia.Services;
internal class NodeEventService
{
    static ServerGameManager ServerGameManager => Core.ServerGameManager;
    static SystemService SystemService => Core.SystemService;
    static PrefabCollectionSystem PrefabCollectionSystem => SystemService.PrefabCollectionSystem;

    public static readonly List<float3> ScheduledCoords = [];
    public static readonly List<float3> IntervalCoords = [];

    static readonly List<TimeSpan> ScheduledTimes = ParseScheduleTimes(Plugin.ScheduledNodeEvents);
    static readonly WaitForSeconds IntervalDelay = new(Plugin.NodeEventsInterval);

    static readonly WaitForSeconds DayDelay = new(86400);

    static readonly PrefabGUID CrystalDropTableResource = new(-1823047667);
    static readonly int DropTableQuantity = Plugin.DropTableQuantity;
    static readonly float DropTableRate = Plugin.DropTableRate;

    const int DROP_TABLE_QUANTITY = 150;
    const float DROP_TABLE_RATE = 1f;
    public NodeEventService()
    {
        if (Core.DataStructures.ScheduledCoords.Any())
        {
            foreach (Core.DataStructures.Float3 coord in Core.DataStructures.ScheduledCoords)
            {
                ScheduledCoords.Add(coord.ToFloat3());
            }
        }

        if (Core.DataStructures.IntervalCoords.Any())
        {
            foreach (Core.DataStructures.Float3 coord in Core.DataStructures.IntervalCoords)
            {
                IntervalCoords.Add(coord.ToFloat3());
            }
        }

        if (DropTableQuantity != DROP_TABLE_QUANTITY || DropTableRate != DROP_TABLE_RATE) ModifyResourcePrefab();

        if (ScheduledTimes.Any()) Core.StartCoroutine(ScheduledEvents());
        if (Plugin.NodeEventsInterval > 0f) Core.StartCoroutine(IntervalEvents());
    }
    static IEnumerator IntervalEvents()
    {
        int index = 0;

        while (true)
        {
            float3 coord = IntervalCoords[index++ % IntervalCoords.Count];

            yield return TriggerIntervalEvents(coord);

            yield return IntervalDelay;
        }
    }
    static IEnumerator ScheduledEvents()
    {
        while (true)
        {
            // Wait until the next scheduled time
            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan? nextEvent = GetNextScheduledTime(now);

            if (nextEvent.HasValue)
            {
                TimeSpan waitTime = nextEvent.Value - now;

                if (waitTime < TimeSpan.Zero)
                {
                    waitTime += TimeSpan.FromDays(1);
                }

                yield return new WaitForSeconds((float)waitTime.TotalSeconds);
            }
            else
            {
                yield return DayDelay; // No events today, wait 24 hours
            }

            // Trigger scheduled events at their respective times
            for (int i = 0; i < ScheduledTimes.Count; i++)
            {
                // Trigger the event at the current coordinate
                float3 coord = ScheduledCoords[i % ScheduledCoords.Count];
                yield return TriggerScheduledEvent(coord);

                // Wait for the time until the next scheduled event
                TimeSpan nowAfterEvent = DateTime.Now.TimeOfDay;
                TimeSpan nextTime = ScheduledTimes[(i + 1) % ScheduledTimes.Count];

                TimeSpan delay = nextTime - nowAfterEvent;
                if (delay < TimeSpan.Zero)
                {
                    delay += TimeSpan.FromDays(1); // Handle wrap-around past midnight
                }

                yield return new WaitForSeconds((float)delay.TotalSeconds);
            }
        }
    }
    static IEnumerator TriggerIntervalEvents(float3 coord)
    {
        // Logic for interval-based events

        yield return null;
    }
    static IEnumerator TriggerScheduledEvent(float3 coord)
    {
        // Logic for schedule-based events

        yield return null;
    }
    static TimeSpan? GetNextScheduledTime(TimeSpan now)
    {
        foreach (TimeSpan scheduledTime in ScheduledTimes)
        {
            if (scheduledTime > now)
            {
                return scheduledTime;
            }
        }

        return null; // No more events today
    }
    static List<TimeSpan> ParseScheduleTimes(string input)
    {
        List<TimeSpan> timeSpans = [];
        string[] parts = input.Split(',');

        foreach (var part in parts)
        {
            if (TimeSpan.TryParse(part, out TimeSpan time))
            {
                timeSpans.Add(time);
            }
        }

        timeSpans.Sort();
        return timeSpans;
    }
    static void ModifyResourcePrefab()
    {
        if (PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(CrystalDropTableResource, out Entity dropTablePrefab)
            && ServerGameManager.TryGetBuffer<DropTableDataBuffer>(dropTablePrefab, out var buffer))
        {
            DropTableDataBuffer dropTableDataBuffer = buffer[0];

            if (DropTableQuantity != DROP_TABLE_QUANTITY)
            {
                dropTableDataBuffer.Quantity = DropTableQuantity;
            }

            if (DropTableRate != DROP_TABLE_RATE)
            {
                dropTableDataBuffer.DropRate = DropTableRate;
            }

            buffer[0] = dropTableDataBuffer;
        }
    }
}
