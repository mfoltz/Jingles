using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;
using static Nocturnalia.Core.DataStructures;

namespace Nocturnalia.Commands;

[CommandGroup(name: "nodes")]
internal static class NodeCommands
{
    [Command(name: "add", shortHand: "a", adminOnly: true, usage: ".nodes a [scheduled/interval/s/i]", description: "Adds current location to the coords pool for scheduled or interval.")] // note to self: need to make it easier/doable to specify which coords for scheduled go with which time :thinking: probably a more general area check for coords already placed as well instead of exact match
    public static void AddEventCoordinateCommand(ChatCommandContext ctx, string type)
    {
        string typeToLower = type.ToLower();

        Entity character = ctx.Event.SenderCharacterEntity;
        float3 location = character.Read<Translation>().Value;

        if (typeToLower == "s" || typeToLower == "scheduled")
        {
            Float3 float3 = new(location.x, location.y, location.z);

            if (!ScheduledCoords.Contains(float3) && !IntervalCoords.Contains(float3))
            {
                ScheduledCoords.Add(float3);
                Core.Log.LogInfo($"Scheduled nodes event coord added at <color=white>{location}</color>!");
            }
            else
            {
                Core.Log.LogInfo($"Schedule or interval nodes event coord already exists here!");
            }
        }
        else if (typeToLower == "i" || typeToLower == "interval")
        {
            Float3 float3 = new(location.x, location.y, location.z);

            if (!IntervalCoords.Contains(float3) && !ScheduledCoords.Contains(float3))
            {
                IntervalCoords.Add(float3);
                Core.Log.LogInfo($"Interval nodes event coord added at <color=white>{location}</color>!");
            }
            else
            {
                Core.Log.LogInfo($"Interval or scheduled nodes event coord already exists here!");
            }
        }
        else
        {
            Core.Log.LogInfo("Invalid entry! Must either be 'scheduled/s' or 'interval/i'.");
            return;
        }
    }

    // maybe separate command for scheduled ones that takes an int as well to refer to the time in the list? or takes a string of the matching time scheduled as configured
}