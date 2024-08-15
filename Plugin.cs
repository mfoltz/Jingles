using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;
using VampireCommandFramework;

namespace Jingles;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    Harmony _harmony;
    internal static Plugin Instance { get; private set; }
    public static Harmony Harmony => Instance._harmony;
    public static ManualLogSource LogInstance => Instance.Log;

    // Config paths
    public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
    public static readonly string SpawnLocations = Path.Combine(ConfigPath, "SpawnLocations");

    // Config options
    static ConfigEntry<bool> _enableJingles;
    static ConfigEntry<string> _eventRewards;
    static ConfigEntry<string> _rewardAmounts;
    static ConfigEntry<int> _eventFrequency;

    // Public accessors for config options
    public static bool EnableJingles => _enableJingles.Value;
    public static string EventRewards => _eventRewards.Value;
    public static string RewardAmounts => _rewardAmounts.Value;
    public static int EventFrequency => _eventFrequency.Value;

    public override void Load()
    {
        Instance = this;
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        CommandRegistry.RegisterAll();
        InitConfig();
        LoadData();
        Core.Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME}[{MyPluginInfo.PLUGIN_VERSION}] loaded!");
    }
    static void CreateDirectories(List<string> paths)
    {
        foreach (var path in paths)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
    static void InitConfig()
    {
        CreateDirectories(filePaths);

        _enableJingles = InitConfigEntry("General", "EnableJingles", false, "Enable or disable the Jingles plugin");
        _eventRewards = InitConfigEntry("General", "EventRewards", "-257494203", "Comma-separated list of prefab hashes used as rewards.");
        _rewardAmounts = InitConfigEntry("General", "RewardAmounts", "250", "Comma-separated list of reward amounts, should match the length of eventRewards.");
        _eventFrequency = InitConfigEntry("General", "EventFrequency", 180, "Frequency of events in minutes");
    }
    static ConfigEntry<T> InitConfigEntry<T>(string section, string key, T defaultValue, string description)
    {
        // Bind the configuration entry and get its value
        var entry = Instance.Config.Bind(section, key, defaultValue, description);

        // Check if the key exists in the configuration file and retrieve its current value
        var configFile = Path.Combine(ConfigPath, $"{MyPluginInfo.PLUGIN_GUID}.cfg");
        if (File.Exists(configFile))
        {
            var config = new ConfigFile(configFile, true);
            if (config.TryGetEntry(section, key, out ConfigEntry<T> existingEntry))
            {
                // If the entry exists, update the value to the existing value
                entry.Value = existingEntry.Value;
            }
        }
        return entry;
    }
    public override bool Unload()
    {
        Config.Clear();
        _harmony.UnpatchSelf();
        return true;
    }
    static void LoadData()
    {
        Core.DataStructures.LoadSpawnLocations();
    }

    static readonly List<string> filePaths =
    [
        ConfigPath,
        SpawnLocations
    ];
}
