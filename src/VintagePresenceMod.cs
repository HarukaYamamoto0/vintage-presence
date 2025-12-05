using VintagePresence.Discord;
using VintagePresence.GUI;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VintagePresence;

// ReSharper disable once UnusedType.Global
public sealed class VintagePresenceMod : ModSystem
{
    private const string ModLogPrefix = Constants.ModLogPrefix;
    private const string ApplicationId = Constants.ApplicationId;
    private const string LargeImageKey = Constants.DefaultLargeImageKey;
    private const string SmallImageKey = Constants.DefaultSmallImageKey;

    private ICoreClientAPI? _capi;
    private VintagePresenceConfig? _config;
    private ConfigGuiDialog? _settingsGuiDialog;
    private long _listenerId;

    private int _updateIntervalInMs;

    private static readonly DiscordService Discord = new();

    public override bool ShouldLoad(EnumAppSide forSide)
    {
        return forSide == EnumAppSide.Client;
    }

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        api.Logger.Event($"{ModLogPrefix} loaded");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        _capi = api;

        _config = VintagePresenceConfig.GetSettings(api);
        _config.Validate(api);
        _updateIntervalInMs = _config.UpdateIntervalSeconds * 1000;

        _settingsGuiDialog = new ConfigGuiDialog(_capi);
        _settingsGuiDialog.RegisterHotKey();

        Discord.OnStatusChange = status =>
        {
            if (!_config.DebugLogging) return;
            _capi.Logger.Event($"{ModLogPrefix} Discord status changed to {status}");
        };

        Discord.Init(ApplicationId);
        Discord.Connect();

        // initially snapshot
        UpdatePresence(0);

        _listenerId = _capi.Event.RegisterGameTickListener(UpdatePresence, _updateIntervalInMs);
        if (_config.DebugLogging) _capi.Logger.Event($"{ModLogPrefix} client-side started");
    }

    private void UpdatePresence(float _)
    {
        var capi = _capi;
        // ReSharper disable once UseNullPropagation
        if (capi is null)
            return;

        var player = capi.World?.Player;
        if (player is null)
            return;

        try
        {
            var activity = new DiscordActivityOptions
            {
                Details = _config?.DetailsTemplate ?? BuildDetails(player),
                State = _config?.StateTemplate ?? BuildState(capi),
                LargeImageKey = LargeImageKey,
                SmallImageKey = SmallImageKey,
                SmallImageText = BuildSmallImageText(player),

                UseTimestamp = true,
                TimestampMode = DiscordTimestampMode.Remaining
            };

            Discord.UpdateActivity(activity);
        }
        catch (Exception ex)
        {
            if (_config is { DebugLogging: true })
            {
                capi.Logger.Error($"{ModLogPrefix} Failed to update presence: {ex}");
            }
        }
    }

    private static string BuildDetails(IClientPlayer player)
    {
        var mode = player.WorldData.CurrentGameMode;
        return mode switch
        {
            EnumGameMode.Creative => "Building in Creative",
            EnumGameMode.Spectator => "Spectating the world",
            EnumGameMode.Guest => "Visiting as Guest",
            _ => "Surviving the world"
        };
    }

    private static string BuildState(ICoreClientAPI capi)
    {
        var allPlayers = capi.World?.AllOnlinePlayers;
        var playerCount = allPlayers?.Length ?? 0;

        return playerCount switch
        {
            <= 1 => "Playing Solo",
            2 => "Playing with 1 other player",
            _ => $"Playing with {playerCount - 1} other players"
        };
    }

    private static string BuildSmallImageText(IClientPlayer player)
    {
        var deaths = player.WorldData.Deaths;
        return $"Total deaths: {deaths}";
    }

    public override void Dispose()
    {
        if (_listenerId != 0 && _capi != null)
        {
            _capi.Event.UnregisterGameTickListener(_listenerId);
            _listenerId = 0;
        }

        Discord.ClearActivity();
        Discord.Dispose();

        base.Dispose();
    }
}