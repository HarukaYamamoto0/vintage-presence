using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VintagePresence;

// ReSharper disable once UnusedType.Global
public sealed class VintagePresenceMod : ModSystem
{
    private const string ModLogPrefix = "[VintagePresence]";
    private const string ApplicationId = "1445733433153425468";

    private const string LargeImageKey = "game_icon";
    private const string LargeImageText = "Game is Vintage Story";
    private const string SmallImageKey = "gear_icon";

    private const int UpdateIntervalInMs = 5000;

    private ICoreClientAPI? _capi;
    private long _listenerId;

    private static readonly DiscordService Discord = new();

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        api.Logger.Event($"{ModLogPrefix} loaded (common)");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        _capi = api;

        Discord.OnStatusChange = status =>
        {
            if (status == "activity_updated") return; // Do not register this unless it is under development.
            api.Logger.Event($"{ModLogPrefix} Discord status changed to {status}");
        };

        Discord.Init(ApplicationId);
        Discord.Connect();

        // initially snapshot
        UpdatePresence(0);

        _listenerId = api.Event.RegisterGameTickListener(UpdatePresence, UpdateIntervalInMs);
        api.Logger.Event($"{ModLogPrefix} client-side started");
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
                Details = BuildDetails(player),
                State = BuildState(capi),
                LargeImageKey = LargeImageKey,
                LargeImageText = LargeImageText,
                SmallImageKey = SmallImageKey,
                SmallImageText = BuildSmallImageText(player),

                UseTimestamp = true,
                TimestampMode = DiscordTimestampMode.Remaining,

                Button1Label = "Download the mod",
                Button1Url = "https://mods.vintagestory.at/vintagepresence"
            };

            Discord.UpdateActivity(activity);
        }
        catch (Exception ex)
        {
            capi.Logger.Error($"{ModLogPrefix} Failed to update presence: {ex}");
        }
    }

    private static string BuildDetails(IClientPlayer player)
    {
        // This can be varied later depending on the game mode, world, etc.
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