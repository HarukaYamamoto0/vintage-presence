using VintagePresence.Discord;
using VintagePresence.GUI;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VintagePresence;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class VintagePresenceMod : ModSystem
{
    private ICoreClientAPI? _capi;
    private ConfigGuiDialog? _settingsDialog;
    private long _updateListenerId;

    private static VintagePresenceConfig _config = null!;
    private static readonly DiscordService Discord = new();

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        api.Logger.Event($"{Constants.ModLogPrefix} loaded");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        _capi = api;
        _config = LoadConfig(api);

        InitializeDiscord();
        InitializeGui(api);

        UpdatePresence(0);
        _updateListenerId = api.Event.RegisterGameTickListener(UpdatePresence, Constants.UpdateIntervalInMs);

        LogDebug("Client-side started");
    }

    private void InitializeDiscord()
    {
        Discord.OnStatusChange = status => LogDebug($"Discord status changed to {status}");
        Discord.Init(Constants.ApplicationId);
        Discord.Connect();
    }

    private void InitializeGui(ICoreClientAPI api)
    {
        _settingsDialog = new ConfigGuiDialog(api);
        _settingsDialog.RegisterHotKey();
    }

    private void UpdatePresence(float _)
    {
        if (_capi?.World?.Player is null)
            return;

        try
        {
            var smallImageKey = _config.SmallImageKey;
            if (smallImageKey == "none") smallImageKey = null;

            var activity = new DiscordActivityOptions
            {
                Details = _config.DetailsTemplate,
                State = _config.StateTemplate,
                LargeImageKey = _config.LargeImageKey,
                LargeImageText = _config.LargeImageText,
                SmallImageKey = smallImageKey,
                SmallImageText = _config.SmallImageText,
                UseTimestamp = true,
                TimestampMode = DiscordTimestampMode.Elapsed
            };

            Discord.UpdateActivity(activity);
        }
        catch (Exception ex)
        {
            if (_config.DebugLogging)
                _capi?.Logger.Error($"{Constants.ModLogPrefix} Failed to update presence: {ex}");
        }
    }

    public static VintagePresenceConfig LoadConfig(ICoreAPI api)
    {
        var config = api.LoadModConfig<VintagePresenceConfig>(Constants.ConfigFile);

        if (config is null)
        {
            config = VintagePresenceConfig.CreateDefault();
            config.IsFirstTime = true;
            api.StoreModConfig(config, Constants.ConfigFile);
            api.Logger.Event($"{Constants.ModLogPrefix} Created new config file");
            return config;
        }

        if (!config.IsFirstTime) return config;
        config.IsFirstTime = false;
        api.StoreModConfig(config, Constants.ConfigFile);

        return config;
    }

    public static void SaveConfig(ICoreClientAPI capi, VintagePresenceConfig config)
    {
        config.Validate(capi);
        _config = config;
        capi.StoreModConfig(config, Constants.ConfigFile);
    }

    private void LogDebug(string message)
    {
        if (_config.DebugLogging)
            _capi?.Logger.Event($"{Constants.ModLogPrefix} {message}");
    }

    public override void Dispose()
    {
        if (_updateListenerId != 0 && _capi is not null)
        {
            _capi.Event.UnregisterGameTickListener(_updateListenerId);
            _updateListenerId = 0;
        }

        Discord.ClearActivity();
        Discord.Dispose();

        base.Dispose();
    }
}