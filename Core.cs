using BepInEx.Logging;
using ProjectM;
using ProjectM.Scripting;
using Jingles.Services;
using System.Text.Json;
using Unity.Entities;
using Unity.Mathematics;
using Stunlock.Core;

namespace Jingles;
internal static class Core
{
    public static World Server { get; } = GetWorld("Server") ?? throw new Exception("There is no Server world (yet)...");
    public static EntityManager EntityManager => Server.EntityManager;
    public static ServerScriptMapper ServerScriptMapper { get; internal set; }
    public static ServerGameManager ServerGameManager => ServerScriptMapper.GetServerGameManager();
    public static PrefabCollectionSystem PrefabCollectionSystem { get; internal set; }
    public static LocalizationService Localization { get; } = new();
    public static JinglesService JinglesService { get; internal set; } 
    public static ManualLogSource Log => Plugin.LogInstance;

    public static bool hasInitialized;
    public static void Initialize()
    {
        if (hasInitialized) return;

        ServerScriptMapper = Server.GetExistingSystemManaged<ServerScriptMapper>();
        PrefabCollectionSystem = Server.GetExistingSystemManaged<PrefabCollectionSystem>();

        InitializeRewards();
        if (Plugin.EnableJingles)
        {
            JinglesService = new();
        }

        hasInitialized = true;
    }
    static World GetWorld(string name)
    {
        foreach (var world in World.s_AllWorlds)
        {
            if (world.Name == name)
            {
                return world;
            }
        }
        return null;
    }
    static void InitializeRewards()
    {
        List<PrefabGUID> rewardPrefabs = ParseConfigString(Plugin.EventRewards).Select(x => new PrefabGUID(x)).ToList();
        List<int> rewardAmounts = ParseConfigString(Plugin.RewardAmounts);
        JinglesService.Rewards = rewardPrefabs.Zip(rewardAmounts, (prefab, amount) => new { prefab, amount }).ToDictionary(x => x.prefab, x => x.amount);
    }
    static List<int> ParseConfigString(string configString)
    {
        if (string.IsNullOrEmpty(configString))
        {
            return [];
        }
        return configString.Split(',').Select(int.Parse).ToList();
    }
    public class DataStructures
    {
        [Serializable]
        public class Float3(float x, float y, float z)
        {
            readonly float x = x, y = y, z = z;
            public float3 ToFloat3() => new(x, y, z);
        }

        static readonly JsonSerializerOptions prettyJsonOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true
        };

        public static List<Float3> SpawnLocations = [];

        static readonly Dictionary<string, string> filePaths = new()
        {
            {"SpawnLocations", JsonFiles.SpawnLocations},
        };
        public static void LoadSpawnLocations() => LoadData<Float3>(ref SpawnLocations, "SpawnLocations");
        public static void SaveSpawnLocations() => SaveData<Float3>(SpawnLocations, "SpawnLocations");
        public static void LoadData<T>(ref List<Float3> dataStructure, string key)
        {
            string path = filePaths[key];
            if (!File.Exists(path))
            {
                // If the file does not exist, create a new empty file to avoid errors on initial load.
                File.Create(path).Dispose();
                dataStructure = []; // Initialize as empty if file does not exist.
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                {
                    // Handle the empty file case
                    dataStructure = []; // Provide default empty dictionary
                }
                else
                {
                    var data = JsonSerializer.Deserialize<List<Float3>>(json, prettyJsonOptions);
                    dataStructure = data ?? []; // Ensure non-null assignment
                }
            }
            catch (IOException)
            {
                Log.LogError($"Failed to load {key} data from {path}...");
            }
        }
        public static void SaveData<T>(List<Float3> data, string key)
        {
            string path = filePaths[key];

            try
            {
                string json = JsonSerializer.Serialize(data, prettyJsonOptions);
                File.WriteAllText(path, json);
            }
            catch (IOException)
            {
                Log.LogError($"Failed to save {key} data to {path}...");
            }
        }
    }
    static class JsonFiles
    {
        public static readonly string SpawnLocations = Plugin.SpawnLocations;
    }
}