using Stunlock.Core;
using System.Collections;
using Unity.Entities;

namespace Jingles.Services;
internal class JinglesService
{
    static EntityManager EntityManager => Core.EntityManager;

    public static Dictionary<PrefabGUID, int> Rewards = [];

    public JinglesService()
    {

    }
    static IEnumerator UpdateLoop()
    {
        yield return null;
    }
}