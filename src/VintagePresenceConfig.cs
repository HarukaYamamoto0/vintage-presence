// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

using System.Text.RegularExpressions;
using Vintagestory.API.Common;

namespace VintagePresence;

public partial class VintagePresenceConfig
{
    // Discord Settings
    public string DiscordAppId { get; set; } = Constants.ApplicationId;

    // Template Settings
    public string DetailsTemplate { get; set; } = "Playing as {player}";
    public string StateTemplate { get; set; } = "{deaths} deaths | {online}/{maxplayers} online";
    public string LargeImageKey { get; set; } = Constants.DefaultLargeImageKey;
    public string LargeImageText { get; set; } = "Vintage Story";
    public string SmallImageKey { get; set; } = Constants.DefaultSmallImageKey;
    public string SmallImageText { get; set; } = "{deaths}";

    // Update Settings
    public int UpdateIntervalSeconds { get; set; } = 10;
    public bool EnableRichPresence { get; set; } = true;

    // Privacy Settings
    public bool ShowPlayerName { get; set; } = true;
    public bool ShowServerInfo { get; set; } = true;
    public bool ShowDeaths { get; set; } = true;
    public bool ShowPlaytime { get; set; } = true;

    // Advanced
    public bool ShowTimestamp { get; set; } = true;
    public bool ResetOnDeath { get; set; } = false;
    public bool DebugLogging { get; set; } = false;
    public bool IsFirstTime { get; set; } = true;

    public void Validate(ICoreAPI api)
    {
        // Validate ApplicationId
        if (!string.IsNullOrWhiteSpace(DiscordAppId)) return;
        api.Logger.Warning(
            "The Discord app ID is not configured correctly. Using default...");
        DiscordAppId = Constants.ApplicationId;

        UpdateIntervalSeconds = Math.Max(5, UpdateIntervalSeconds); // Not very often, and it shouldn't be.
    }

    public static VintagePresenceConfig GetSettings(ICoreAPI api)
    {
        var config = api.LoadModConfig<VintagePresenceConfig>(Constants.ConfigFile);

        if (config == null)
        {
            config = new VintagePresenceConfig { IsFirstTime = true };
            api.StoreModConfig(config, Constants.ConfigFile);
            api.Logger.Event($"{Constants.ModLogPrefix} Created new config file");
        }
        else if (config.IsFirstTime)
        {
            config.IsFirstTime = false;
            api.StoreModConfig(config, Constants.ConfigFile);
        }

        return config;
    }
}