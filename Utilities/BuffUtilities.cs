using Nocturnalia.Services;
using ProjectM;
using ProjectM.Gameplay.Scripting;
using ProjectM.Network;
using ProjectM.Scripting;
using Stunlock.Core;
using Unity.Entities;

namespace Nocturnalia.Utilities;
internal static class BuffUtilities
{
    static EntityManager EntityManager => Core.EntityManager;
    static ServerGameManager ServerGameManager => Core.ServerGameManager;
    static SystemService SystemService => Core.SystemService;
    static DebugEventsSystem DebugEventsSystem => SystemService.DebugEventsSystem;
    public static bool TryApplyBuff(Entity character, PrefabGUID buffPrefab)
    {
        ApplyBuffDebugEvent applyBuffDebugEvent = new()
        {
            BuffPrefabGUID = buffPrefab
        };

        FromCharacter fromCharacter = new()
        {
            Character = character,
            User = character
        };

        if (!ServerGameManager.HasBuff(character, buffPrefab.ToIdentifier()))
        {
            DebugEventsSystem.ApplyBuff(fromCharacter, applyBuffDebugEvent);

            return true;
        }

        return false;
    }
    public static bool TryApplyBuffWithOwner(Entity target, Entity owner, PrefabGUID buffPrefab)
    {
        ApplyBuffDebugEvent applyBuffDebugEvent = new()
        {
            BuffPrefabGUID = buffPrefab,
            Who = target.Read<NetworkId>()
        };

        FromCharacter fromCharacter = new() // fam should be entityOwner
        {
            Character = target,
            User = owner
        };

        if (!ServerGameManager.HasBuff(target, buffPrefab.ToIdentifier()))
        {
            DebugEventsSystem.ApplyBuff(fromCharacter, applyBuffDebugEvent);

            return true;
        }

        return false;
    }
    public static void ApplyPermanentBuff(Entity player, PrefabGUID buffPrefab)
    {
        bool appliedBuff = TryApplyBuff(player, buffPrefab);

        if (appliedBuff && ServerGameManager.TryGetBuff(player, buffPrefab.ToIdentifier(), out Entity buffEntity))
        {
            ModifyPermanentBuff(buffEntity);
        }
    }
    static void ModifyPermanentBuff(Entity buffEntity)
    {
        if (buffEntity.Has<RemoveBuffOnGameplayEvent>())
        {
            buffEntity.Remove<RemoveBuffOnGameplayEvent>();
        }

        if (buffEntity.Has<RemoveBuffOnGameplayEventEntry>())
        {
            buffEntity.Remove<RemoveBuffOnGameplayEventEntry>();
        }

        if (buffEntity.Has<CreateGameplayEventsOnSpawn>())
        {
            buffEntity.Remove<CreateGameplayEventsOnSpawn>();
        }

        if (buffEntity.Has<GameplayEventListeners>())
        {
            buffEntity.Remove<GameplayEventListeners>();
        }

        if (!buffEntity.Has<Buff_Persists_Through_Death>())
        {
            buffEntity.Add<Buff_Persists_Through_Death>();
        }

        if (buffEntity.Has<DestroyOnGameplayEvent>())
        {
            buffEntity.Remove<DestroyOnGameplayEvent>();
        }

        if (buffEntity.Has<LifeTime>())
        {
            LifeTime lifeTime = buffEntity.Read<LifeTime>();
            lifeTime.Duration = 9999f;
            lifeTime.EndAction = LifeTimeEndAction.None;
            buffEntity.Write(lifeTime);
        }
    }
    public static void HandleVisual(Entity entity, PrefabGUID visual)
    {
        ApplyBuffDebugEvent applyBuffDebugEvent = new()
        {
            BuffPrefabGUID = visual,
        };

        FromCharacter fromCharacter = new()
        {
            Character = entity,
            User = entity
        };

        DebugEventsSystem.ApplyBuff(fromCharacter, applyBuffDebugEvent);
        if (ServerGameManager.TryGetBuff(entity, applyBuffDebugEvent.BuffPrefabGUID.ToIdentifier(), out Entity buff))
        {
            if (buff.Has<Buff>())
            {
                BuffCategory component = buff.Read<BuffCategory>();
                component.Groups = BuffCategoryFlag.None;
                buff.Write(component);
            }
            if (buff.Has<CreateGameplayEventsOnSpawn>())
            {
                buff.Remove<CreateGameplayEventsOnSpawn>();
            }
            if (buff.Has<GameplayEventListeners>())
            {
                buff.Remove<GameplayEventListeners>();
            }
            if (buff.Has<LifeTime>())
            {
                LifeTime lifetime = buff.Read<LifeTime>();
                lifetime.Duration = 9999f; // need to try changing this to 9999 instead? death to console spam
                lifetime.EndAction = LifeTimeEndAction.None;
                buff.Write(lifetime);
            }
            if (buff.Has<RemoveBuffOnGameplayEvent>())
            {
                buff.Remove<RemoveBuffOnGameplayEvent>();
            }
            if (buff.Has<RemoveBuffOnGameplayEventEntry>())
            {
                buff.Remove<RemoveBuffOnGameplayEventEntry>();
            }
            if (buff.Has<DealDamageOnGameplayEvent>())
            {
                buff.Remove<DealDamageOnGameplayEvent>();
            }
            if (buff.Has<HealOnGameplayEvent>())
            {
                buff.Remove<HealOnGameplayEvent>();
            }
            if (buff.Has<BloodBuffScript_ChanceToResetCooldown>())
            {
                buff.Remove<BloodBuffScript_ChanceToResetCooldown>();
            }
            if (buff.Has<ModifyMovementSpeedBuff>())
            {
                buff.Remove<ModifyMovementSpeedBuff>();
            }
            if (buff.Has<ApplyBuffOnGameplayEvent>())
            {
                buff.Remove<ApplyBuffOnGameplayEvent>();
            }
            if (buff.Has<DestroyOnGameplayEvent>())
            {
                buff.Remove<DestroyOnGameplayEvent>();
            }
            if (buff.Has<WeakenBuff>())
            {
                buff.Remove<WeakenBuff>();
            }
            if (buff.Has<ReplaceAbilityOnSlotBuff>())
            {
                buff.Remove<ReplaceAbilityOnSlotBuff>();
            }
            if (buff.Has<AmplifyBuff>())
            {
                buff.Remove<AmplifyBuff>();
            }
        }
    }
}
