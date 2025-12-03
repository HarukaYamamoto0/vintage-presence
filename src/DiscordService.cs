using DiscordRPC;

namespace VintagePresence;

public sealed class DiscordService : IDisposable
{
    private const int MaxDetailsLength = 128;
    private const int MaxStateLength = 128;
    private const int MaxImageTextLength = 128;
    private const int MaxButtonLabelLength = 32;

    private DiscordRpcClient? _client;
    private DiscordActivityOptions? _currentActivity;

    /// <summary>
    /// Optional callback invoked when the Discord connection or activity status changes.
    /// </summary>
    /// <remarks>
    /// The string argument represents the new status value, such as
    /// "connected", "ready", "disconnected", "error" or "activity_updated".
    /// </remarks>
    public Action<string>? OnStatusChange { get; set; }

    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// Indicates whether the underlying Discord RPC client is currently initialized.
    /// </summary>
    public bool IsConnected => _client?.IsInitialized == true;

    /// <summary>
    /// Creates and configures the underlying Discord RPC client for the given application id.
    /// </summary>
    /// <param name="clientId">Discord application id used for Rich Presence.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="clientId"/> is null, empty, or whitespace.
    /// </exception>
    public void Init(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client id cannot be null or empty.", nameof(clientId));

        DisposeClient();

        _client = new DiscordRpcClient(clientId)
        {
            Logger = new DiscordRPC.Logging.ConsoleLogger(
                DiscordRPC.Logging.LogLevel.Warning,
                true
            )
        };

        _client.OnConnectionEstablished += (_, _) => NotifyStatus("connected");

        _client.OnReady += (_, _) =>
        {
            NotifyStatus("ready");

            if (_currentActivity is not null)
                UpdateActivity(_currentActivity);
        };

        _client.OnClose += (_, _) => NotifyStatus("disconnected");
        _client.OnError += (_, _) => NotifyStatus("error");
    }

    /// <summary>
    /// Connects the Discord RPC client if it has been initialized and is not yet connected.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the client has not been initialized via <see cref="Init"/>.
    /// </exception>
    public void Connect()
    {
        if (_client is null)
            throw new InvalidOperationException("Discord client not initialized. Call Init() first.");

        if (_client.IsInitialized)
            return;

        _client.Initialize();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// Disconnects and disposes the Discord RPC client and clears the current activity.
    /// </summary>
    /// <remarks>
    /// After calling this method, the client instance is released and
    /// <see cref="IsConnected"/> will return false.
    /// </remarks>
    public void Disconnect()
    {
        DisposeClient();
        _currentActivity = null;
        NotifyStatus("disconnected");
    }

    /// <summary>
    /// Updates the Rich Presence activity and sends it to Discord if the client is connected.
    /// </summary>
    /// <param name="activity">
    /// Activity options describing details, state, images, timestamps, and buttons.
    /// </param>
    /// <remarks>
    /// The activity is always cached internally. If the client is not yet connected,
    /// it will be sent automatically the next time the client becomes ready.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="activity"/> is null.
    /// </exception>
    public void UpdateActivity(DiscordActivityOptions activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        _currentActivity = activity;

        if (!IsConnected)
            return;

        var presence = BuildRichPresence(activity);
        _client!.SetPresence(presence);

        NotifyStatus("activity_updated");
    }

    /// <summary>
    /// Clears the current Rich Presence activity from Discord if the client is connected.
    /// </summary>
    /// <remarks>
    /// The cached activity is reset locally, and the presence is removed on Discord.
    /// If the client is not connected, only the cached activity is cleared.
    /// </remarks>
    public void ClearActivity()
    {
        _currentActivity = null;

        if (!IsConnected)
            return;

        _client!.ClearPresence();
        NotifyStatus("activity_cleared");
    }

    /// <summary>
    /// Builds a RichPresence payload from the given activity options.
    /// </summary>
    /// <param name="activity">Activity options used to populate the presence fields.</param>
    /// <returns>
    /// A RichPresence instance with details, state, assets, timestamps, and buttons
    /// derived from the supplied options.
    /// </returns>
    private static RichPresence BuildRichPresence(DiscordActivityOptions activity)
    {
        var presence = new RichPresence();

        if (!string.IsNullOrWhiteSpace(activity.Details))
            presence.Details = PadToMinLength(Truncate(activity.Details, MaxDetailsLength));

        if (!string.IsNullOrWhiteSpace(activity.State))
            presence.State = PadToMinLength(Truncate(activity.State, MaxStateLength));

        var assets = new Assets();

        if (!string.IsNullOrWhiteSpace(activity.LargeImageKey))
            assets.LargeImageKey = activity.LargeImageKey;

        if (!string.IsNullOrWhiteSpace(activity.LargeImageText))
            assets.LargeImageText = PadToMinLength(
                Truncate(activity.LargeImageText, MaxImageTextLength)
            );

        if (!string.IsNullOrWhiteSpace(activity.SmallImageKey))
            assets.SmallImageKey = activity.SmallImageKey;

        if (!string.IsNullOrWhiteSpace(activity.SmallImageText))
            assets.SmallImageText = PadToMinLength(
                Truncate(activity.SmallImageText, MaxImageTextLength)
            );

        presence.Assets = assets;

        if (activity.UseTimestamp)
        {
            presence.Timestamps = activity.TimestampMode switch
            {
                DiscordTimestampMode.Elapsed => new Timestamps(DateTime.UtcNow),
                DiscordTimestampMode.Remaining when activity.EndTimeSeconds.HasValue
                    => BuildRemainingTimestamp(activity.EndTimeSeconds.Value),
                _ => presence.Timestamps
            };
        }

        var buttons = BuildButtons(activity);
        if (buttons.Count > 0)
            presence.Buttons = buttons.ToArray();

        return presence;
    }

    /// <summary>
    /// Creates button entries for Rich Presence based on the configured labels and URLs.
    /// </summary>
    /// <param name="activity">Activity options that may contain up to two button definitions.</param>
    /// <returns>
    /// A list of Button instances. Only buttons with both label and URL sets are included.
    /// </returns>
    private static List<Button> BuildButtons(DiscordActivityOptions activity)
    {
        var buttons = new List<Button>(2);

        if (!string.IsNullOrWhiteSpace(activity.Button1Label) &&
            !string.IsNullOrWhiteSpace(activity.Button1Url))
        {
            buttons.Add(new Button
            {
                Label = Truncate(activity.Button1Label, MaxButtonLabelLength),
                Url = activity.Button1Url
            });
        }

        if (!string.IsNullOrWhiteSpace(activity.Button2Label) &&
            !string.IsNullOrWhiteSpace(activity.Button2Url))
        {
            buttons.Add(new Button
            {
                Label = Truncate(activity.Button2Label, MaxButtonLabelLength),
                Url = activity.Button2Url
            });
        }

        return buttons;
    }

    /// <summary>
    /// Builds a timestamp range starting at the current time and ending
    /// after the specified number of seconds.
    /// </summary>
    /// <param name="endTimeSeconds">Duration in seconds to add to the current time.</param>
    /// <returns>
    /// A Timestamps instance representing a start and end time for a remaining duration.
    /// </returns>
    private static Timestamps BuildRemainingTimestamp(double endTimeSeconds)
    {
        var start = DateTime.UtcNow;
        var end = start.AddSeconds(endTimeSeconds);
        return new Timestamps(start, end);
    }

    /// <summary>
    /// Invokes the status change callback if one is registered.
    /// </summary>
    /// <param name="status">Status string to forward to listeners.</param>
    private void NotifyStatus(string status)
    {
        OnStatusChange?.Invoke(status);
    }

    /// <summary>
    /// Truncates a string to a maximum length, appending "..." if truncation occurs.
    /// </summary>
    /// <param name="value">Input string to truncate.</param>
    /// <param name="maxLength">Maximum allowed length, including ellipsis.</param>
    /// <returns>
    /// The original string if within the limit; otherwise a truncated version
    /// ending with ellipsis.
    /// </returns>
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value[..(maxLength - 3)] + "...";
    }

    /// <summary>
    /// Pads short strings with an invisible filler character to reach a minimum length.
    /// </summary>
    /// <param name="value">Input string to pad.</param>
    /// <param name="minLength">Minimum required length for the string.</param>
    /// <returns>
    /// The padded string is shorter than <paramref name="minLength"/>;
    /// otherwise the original string.
    /// </returns>
    private static string PadToMinLength(string value, int minLength = 2)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        const char filler = '\u3164';

        if (value.Length > 0 && value.Length < minLength)
            return value + new string(filler, minLength - value.Length);

        return value;
    }

    /// <summary>
    /// Disposes the current Discord RPC client instance, if any.
    /// </summary>
    /// <remarks>
    /// After this call, the internal client reference is set to null and
    /// any existing connection is closed.
    /// </remarks>
    private void DisposeClient()
    {
        if (_client is null)
            return;

        _client.Dispose();
        _client = null;
    }

    /// <summary>
    /// Releases all resources used by the DiscordService and disconnects from Discord.
    /// </summary>
    public void Dispose()
    {
        Disconnect();
    }
}