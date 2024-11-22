using BepInEx.Logging;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Nocturnalia.Services;
using ProjectM;
using ProjectM.Physics;
using ProjectM.Scripting;
using System.Collections;
using System.Text.Json;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Nocturnalia;
internal static class Core
{
    static World Server { get; } = GetServerWorld() ?? throw new Exception("There is no Server world (yet)...");
    public static EntityManager EntityManager => Server.EntityManager;
    public static ServerGameManager ServerGameManager => SystemService.ServerScriptMapper.GetServerGameManager();
    public static SystemService SystemService { get; } = new(Server);
    public static double ServerTime => ServerGameManager.ServerTime;
    public static ManualLogSource Log => Plugin.LogInstance;

    public static bool hasInitialized;

    static MonoBehaviour MonoBehaviour;
    public static void Initialize()
    {
        if (hasInitialized) return;

        if (Plugin.CrystalNodes)
        {
            _ = new NodeEventService();
        }

        hasInitialized = true;
    }
    static World GetServerWorld()
    {
        return World.s_AllWorlds.ToArray().FirstOrDefault(world => world.Name == "Server");
    }
    public static void StartCoroutine(IEnumerator routine)
    {
        if (MonoBehaviour == null)
        {
            MonoBehaviour = new GameObject("Nocturnalia").AddComponent<IgnorePhysicsDebugSystem>();
            UnityEngine.Object.DontDestroyOnLoad(MonoBehaviour.gameObject);
        }

        MonoBehaviour.StartCoroutine(routine.WrapToIl2Cpp());
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

        public static List<Float3> ScheduledCoords = [];
        public static List<Float3> IntervalCoords = [];

        static readonly Dictionary<string, string> filePaths = new()
        {
            {"ScheduledCoords", JsonFiles.ScheduledCoords},
            {"IntervalCoords", JsonFiles.IntervalCoords}
        };
        public static void LoadScheduledCoords() => LoadData<Float3>(ref ScheduledCoords, "ScheduledCoords");
        public static void SaveScheduledCoords() => SaveData<Float3>(ScheduledCoords, "ScheduledCoords");
        public static void LoadIntervalCoords() => LoadData<Float3>(ref IntervalCoords, "IntervalCoords");
        public static void SaveIntervalCoords() => SaveData<Float3>(IntervalCoords, "IntervalCoords");
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
            catch (IOException ex)
            {
                Log.LogError($"Failed loading {key} from {path}: {ex}");
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
            catch (IOException ex)
            {
                Log.LogError($"Failed saving {key} to {path}: {ex}");
            }
        }
    }
    static class JsonFiles
    {
        public static readonly string ScheduledCoords = Plugin.ScheduledCoords;
        public static readonly string IntervalCoords = Plugin.IntervalCoords;
    }
}