namespace VintagePresence;

internal static class Constants
{
    internal const string ModLogPrefix = "[VintagePresence]";
    internal const string ApplicationId = "1445733433153425468";
    internal const string ConfigFile = "vintage-presence.json";

    internal const string DefaultDetailsTemplate = "Surviving the world";
    internal const string DefaultStateTemplate = "Playing Solo";

    internal const string DefaultLargeImageKey = "default";
    internal const string DefaultLargeImageText = "Vintage Presence Mod";

    internal const string DefaultSmallImageKey = "gear";

    internal const string DefaultSmallImageText = "Vintage Story";
    // TODO: Add the return when you make the templates.
    // internal const string DefaultSmallImageText = "Deaths: {deaths}";

    internal const int UpdateIntervalInMs = 5000;

    internal const string ToggleKeyCombinationCode = "vintage-presence-settings-dialog";

    internal static readonly string[] LargeImageOptions =
    [
        // TODO: Add custom images
        DefaultLargeImageKey,
        "sword",
        "anvil",
        "farming",
        "mining",
        "hammer",
        "coins",
        "map",
        "cooking"
        // "survival",
        // "creative",
        // "wilderness",
        // "underground",
        // "settlement",
        // "seasons_spring",
        // "seasons_summer",
        // "seasons_autumn",
        // "seasons_winter"
    ];

    internal static readonly string[] SmallImageOptions =
    [
        DefaultSmallImageKey,
        "none",
        "sword",
        "anvil",
        "farming",
        "mining",
        "hammer",
        "coins",
        "map",
        "cooking"
    ];
}