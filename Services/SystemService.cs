﻿using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Shared.Systems;
using Unity.Entities;

namespace Nocturnalia.Services;
public class SystemService(World world)
{
    readonly World _world = world ?? throw new ArgumentNullException(nameof(world));

    DebugEventsSystem _debugEventsSystem;
    public DebugEventsSystem DebugEventsSystem => _debugEventsSystem ??= GetSystem<DebugEventsSystem>();

    PrefabCollectionSystem _prefabCollectionSystem;
    public PrefabCollectionSystem PrefabCollectionSystem => _prefabCollectionSystem ??= GetSystem<PrefabCollectionSystem>();

    ServerGameSettingsSystem _serverGameSettingsSystem;
    public ServerGameSettingsSystem ServerGameSettingsSystem => _serverGameSettingsSystem ??= GetSystem<ServerGameSettingsSystem>();

    ServerScriptMapper _serverScriptMapper;
    public ServerScriptMapper ServerScriptMapper => _serverScriptMapper ??= GetSystem<ServerScriptMapper>();

    ModifyUnitStatBuffSystem_Spawn _modifyUnitStatBuffSystem_Spawn;
    public ModifyUnitStatBuffSystem_Spawn ModifyUnitStatBuffSystem_Spawn => _modifyUnitStatBuffSystem_Spawn ??= GetSystem<ModifyUnitStatBuffSystem_Spawn>();

    ModifyUnitStatBuffSystem_Destroy _modifyUnitStatBuffSystem_Destroy;
    public ModifyUnitStatBuffSystem_Destroy ModifyUnitStatBuffSystem_Destroy => _modifyUnitStatBuffSystem_Destroy ??= GetSystem<ModifyUnitStatBuffSystem_Destroy>();

    EntityCommandBufferSystem _entityCommandBufferSystem;
    public EntityCommandBufferSystem EntityCommandBufferSystem => _entityCommandBufferSystem ??= GetSystem<EntityCommandBufferSystem>();

    GameDataSystem _gameDataSystem;
    public GameDataSystem GameDataSystem => _gameDataSystem ??= GetSystem<GameDataSystem>();

    ScriptSpawnServer _scriptSpawnServer;
    public ScriptSpawnServer ScriptSpawnServer => _scriptSpawnServer ??= GetSystem<ScriptSpawnServer>();

    CombatMusicSystem_Server _combatMusicSystem_Server;
    public CombatMusicSystem_Server CombatMusicSystem_Server => _combatMusicSystem_Server ??= GetSystem<CombatMusicSystem_Server>();

    NameableInteractableSystem _nameableInteractableSystem;
    public NameableInteractableSystem NameableInteractableSystem => _nameableInteractableSystem ??= GetSystem<NameableInteractableSystem>();

    EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;
    public EndSimulationEntityCommandBufferSystem EndSimulationEntityCommandBufferSystem => _endSimulationEntityCommandBufferSystem ??= GetSystem<EndSimulationEntityCommandBufferSystem>();

    NetworkIdSystem.Singleton _networkIdSystem_Singleton;
    public NetworkIdSystem.Singleton NetworkIdSystem => _networkIdSystem_Singleton = ServerScriptMapper.GetSingleton<NetworkIdSystem.Singleton>();
    T GetSystem<T>() where T : ComponentSystemBase
    {
        return _world.GetExistingSystemManaged<T>() ?? throw new InvalidOperationException($"Failed to get {Il2CppType.Of<T>().FullName} from the Server...");
    }
}