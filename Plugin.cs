using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;
using VampireCommandFramework;

namespace Nocturnalia;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    Harmony _harmony;
    internal static Plugin Instance { get; private set; }
    public static Harmony Harmony => Instance._harmony;
    public static ManualLogSource LogInstance => Instance.Log;

    // Config file paths
    public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
    public static readonly string ScheduledCoords = Path.Combine(ConfigPath, "ScheduledCoords");
    public static readonly string IntervalCoords = Path.Combine(ConfigPath, "IntervalCoords");

    // List for directory creation
    static readonly List<string> FilePaths =
    [
        ConfigPath,
        ScheduledCoords,
        IntervalCoords
    ];

    // Config settings
    static ConfigEntry<bool> _crystalNodeEvents;
    static ConfigEntry<int> _dropTableQuantity;
    static ConfigEntry<float> _dropTableRate;
    static ConfigEntry<float> _nodeEventsInterval;
    static ConfigEntry<string> _scheduledNodeEvents;

    // Public accessors for config options
    public static readonly bool CrystalNodes = _crystalNodeEvents.Value;
    public static readonly int DropTableQuantity = _dropTableQuantity.Value;
    public static readonly float DropTableRate = _dropTableRate.Value;
    public static readonly float NodeEventsInterval = _nodeEventsInterval.Value;
    public static readonly string ScheduledNodeEvents = _scheduledNodeEvents.Value;
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
        CreateDirectories(FilePaths);

        _crystalNodeEvents = InitConfigEntry("CrystalNodes", "CrystalNodeEvents", false, "Enable/disable crystal nodes event.");
        _dropTableQuantity = InitConfigEntry("CrystalNodes", "DropTableQuantity", 150, "Adjust to modify base quantity of crystal harvested from resource node (150 is vanilla).");
        _dropTableRate = InitConfigEntry("CrystalNodes", "DropTableRate", 1f, "Adjust to modify base drop rate chance of crystal harvested from resource node (1 is vanilla).");
        _nodeEventsInterval = InitConfigEntry("CrystalNodes", "NodeEventsInterval", 180f, "Interval between crystal node events. Can be used with scheduling, leave at 0 for scheduled only.");
        _scheduledNodeEvents = InitConfigEntry("CrystalNodes", "ScheduledNodeEvents", "", "Scheduled crystal node events. Can be used with intervals, leave blank for intervals only. Uses timezone of server. (12:30,13:30,14:00)");
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
        Core.DataStructures.LoadNodeCoords();
    }
}
