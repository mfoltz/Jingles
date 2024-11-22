using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Stunlock.Localization;
using System.Reflection;
using System.Text.Json;
using Unity.Entities;
using VampireCommandFramework;

namespace Nocturnalia.Services;
internal class LocalizationService
{
    const string ENGLISH_RESOURCE = "Nocturnalia.Localization.English.json";
    const string PREFABS_RESOURCE = "Nocturnalia.Localization.Prefabs.json";
    struct Code
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
    struct Node
    {
        public string Guid { get; set; }
        public string Text { get; set; }
    }
    struct Words
    {
        public string Original { get; set; }
        public string Translation { get; set; }
    }
    struct LocalizationFile
    {
        public Code[] Codes { get; set; }
        public Node[] Nodes { get; set; }
        public Words[] Words { get; set; }
    }

    static readonly Dictionary<string, string> Localization = [];
    static readonly Dictionary<int, string> PrefabNames = [];
    public LocalizationService()
    {
        LoadLocalizations();
        LoadPrefabNames();
    }
    static void LoadLocalizations()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(ENGLISH_RESOURCE);

        using StreamReader localizationReader = new(stream);
        string jsonContent = localizationReader.ReadToEnd();
        var localizationFile = JsonSerializer.Deserialize<LocalizationFile>(jsonContent);

        localizationFile.Nodes
            .ToDictionary(x => x.Guid, x => x.Text)
            .ForEach(kvp => Localization[kvp.Key] = kvp.Value);
    }
    static void LoadPrefabNames()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(PREFABS_RESOURCE);

        using StreamReader reader = new(stream);
        string jsonContent = reader.ReadToEnd();
        var prefabNames = JsonSerializer.Deserialize<Dictionary<int, string>>(jsonContent);
        prefabNames.ForEach(kvp => PrefabNames[kvp.Key] = kvp.Value);
    }
    internal static void HandleReply(ChatCommandContext ctx, string message)
    {
        ctx.Reply(message);
    }
    internal static void HandleServerReply(EntityManager entityManager, User user, string message)
    {
        ServerChatUtils.SendSystemMessageToClient(entityManager, user, message);
    }
    static string GetLocalizationFromKey(LocalizationKey key)
    {
        var guid = key.Key.ToGuid().ToString();
        return GetLocalization(guid);
    }
    public static string GetPrefabName(PrefabGUID prefabGUID)
    {
        if (PrefabNames.TryGetValue(prefabGUID.GuidHash, out var itemLocalizationHash))
        {
            return GetLocalization(itemLocalizationHash);
        }
        return prefabGUID.LookupName();
    }
    public static string GetLocalization(string Guid)
    {
        if (Localization.TryGetValue(Guid, out var Text))
        {
            return Text;
        }

        return "Couldn't find key for localization...";
    }
}
