using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Nocturnalia.Utilities;
internal static class EntityUtilities
{
    public static IEnumerable<Entity> GetEntitiesEnumerable(EntityQuery entityQuery) 
    {
        JobHandle handle = GetEntities(entityQuery, out NativeArray<Entity> entities, Allocator.TempJob);
        handle.Complete();

        try
        {
            foreach (Entity entity in entities)
            {
                if (entity.Exists())
                {
                    yield return entity;
                }
            }
        }
        finally
        {
            entities.Dispose();
        }
    }
    static JobHandle GetEntities(EntityQuery entityQuery, out NativeArray<Entity> entities, Allocator allocator = Allocator.TempJob)
    {
        entities = entityQuery.ToEntityArray(allocator);
        return default;
    }
}
