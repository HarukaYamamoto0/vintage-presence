using VintagePresence.Discord;
using VintagePresence.GUI;
using VintagePresence.PresenceTemplate;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

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

        Discord.ClearActivity();
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
            var ctx = BuildPresenceContext();

            var engine = new PresenceTemplateEngine();
            var details = engine.Render(_config.DetailsTemplate, ctx);
            var state = engine.Render(_config.StateTemplate, ctx);

            var smallImageKey = _config.SmallImageKey;
            if (smallImageKey == "none") smallImageKey = null;

            var activity = new DiscordActivityOptions
            {
                Details = details,
                State = state,
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

    private PresenceContext BuildPresenceContext()
    {
        if (_capi?.World?.Player is null)
            throw new InvalidOperationException("Cannot build PresenceContext without a valid player/world.");

        var world = _capi.World;
        var player = world.Player;
        var entity = player.Entity;
        var calendar = world.Calendar;
        var pos = entity.Pos.AsBlockPos;

        // Climate
        var climate = world.BlockAccessor.GetClimateAt(pos);

        // Health
        var healthTree = entity.WatchedAttributes?.GetTreeAttribute("health");
        var curHealth = healthTree?.GetFloat("currenthealth") ?? 0f;
        var maxHealth = healthTree?.GetFloat("maxhealth") ?? 0f;

        var totalDays = calendar.ElapsedDays;
        var hour = calendar.HourOfDay;

        // Time of day
        var timeOfDay = hour switch
        {
            >= 5 and < 10 => "Morning",
            >= 10 and < 17 => "Day",
            >= 17 and < 21 => "Evening",
            _ => "Night"
        };

        // Weather
        var distanceToRain = world.BlockAccessor.GetDistanceToRainFall(pos);
        var isRaining = distanceToRain < 5;

        var weather = climate?.Temperature switch
        {
            <= 0f when isRaining => "Snow",
            <= 0f => "Cold",
            _ when isRaining => "Rain",
            _ => "Clear"
        };

        // Season
        var seasonEnum = calendar.GetSeason(pos);
        var season = seasonEnum.ToString();

        var onlinePlayers = world.AllOnlinePlayers.Length;
        var deaths = player.WorldData.Deaths;

        return new PresenceContext
        {
            GameMode = player.WorldData.CurrentGameMode.ToString(),
            Day = (int)totalDays,
            TimeOfDay = timeOfDay,
            PlayerName = player.PlayerName,
            Health = curHealth,
            MaxHealth = maxHealth,
            Deaths = deaths,
            OnlinePlayers = onlinePlayers,
            Temperature = climate?.Temperature ?? 0f,
            Weather = weather,
            Season = season,
            Coords = $"{pos.X}, {pos.Y}, {pos.Z}",
            ModVersion = Mod?.Info?.Version ?? "dev",
            GameVersion = GameVersion.ShortGameVersion,
        };
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
        LogDebug("Disposing mod...");

        if (_updateListenerId != 0 && _capi is not null)
        {
            _capi.Event.UnregisterGameTickListener(_updateListenerId);
            _updateListenerId = 0;
        }

        try
        {
            Discord.ClearActivity();
            Thread.Sleep(500);
            Discord.Dispose();
        }
        catch (Exception ex)
        {
            _capi?.Logger.Warning($"{Constants.ModLogPrefix} Error disposing Discord: {ex.Message}");
        }

        base.Dispose();
    }
}