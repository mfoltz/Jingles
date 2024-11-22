using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;
using static Nocturnalia.Core.DataStructures;

namespace Nocturnalia.Commands;

[CommandGroup(name: "jingles")]
internal static class JinglesCommand
{
    [Command(name: "addspawn", shortHand: "add", adminOnly: false, usage: ".jingles add", description: "Adds current location to the spawn pool for Jingles.")]
    public static void AddSpawnLocationCommand(ChatCommandContext ctx)
    {
        Entity character = ctx.Event.SenderCharacterEntity;
        float3 location = character.Read<Translation>().Value;

        if (!Core.DataStructures.SpawnLocations.Contains(new Float3(location.x, location.y, location.z)))
        {
            Core.DataStructures.SpawnLocations.Add(new Float3(location.x, location.y, location.z));
            Core.Log.LogInfo($"Added spawn at <color=white>{location}</color>.");
        }
        else
        {
            Core.Log.LogInfo($"Spawn location already exists here!");
        }
    }
}