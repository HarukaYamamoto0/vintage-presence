// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace VintagePresence.Discord;

/// <summary>
/// Specifies the mode for displaying timestamps in Discord Rich Presence.
/// </summary>
public enum DiscordTimestampMode
{
    /// <summary>
    /// No timestamp display.
    /// </summary>
    None = 0,

    /// <summary>
    /// Displays elapsed time since activity started.
    /// </summary>
    Elapsed,

    /// <summary>
    /// Displays remaining time until the activity ends.
    /// </summary>
    Remaining
}

/// <summary>
/// Configuration options for Discord Rich Presence activity.
/// Contains all customizable fields for the presence display.
/// </summary>
public sealed class DiscordActivityOptions
{
    // Maintained for compatibility but not used directly in DiscordRPC
    public int? Type { get; set; }
    public int? StatusDisplayType { get; set; }

    /// <summary>
    /// Primary text displayed in the Rich Presence (first line).
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// URL for details (not currently supported by Discord).
    /// </summary>
    public string? DetailsUrl { get; set; }

    /// <summary>
    /// Secondary text displayed in the Rich Presence (second line).
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// URL for state (not currently supported by Discord).
    /// </summary>
    public string? StateUrl { get; set; }

    /// <summary>
    /// Key for the large image asset (registered in Discord Developer Portal).
    /// </summary>
    public string? LargeImageKey { get; set; }

    /// <summary>
    /// Tooltip text for the large image.
    /// </summary>
    public string? LargeImageText { get; set; }

    /// <summary>
    /// Key for the small image asset (registered in Discord Developer Portal).
    /// </summary>
    public string? SmallImageKey { get; set; }

    /// <summary>
    /// Tooltip text for the small image.
    /// </summary>
    public string? SmallImageText { get; set; }

    /// <summary>
    /// Whether to display timestamps in the Rich Presence.
    /// </summary>
    public bool UseTimestamp { get; set; }

    /// <summary>
    /// Mode for displaying timestamps (elapsed or remaining time).
    /// </summary>
    public DiscordTimestampMode TimestampMode { get; set; } = DiscordTimestampMode.None;

    /// <summary>
    /// End time in seconds for the remaining time mode.
    /// </summary>
    public double? EndTimeSeconds { get; set; }

    /// <summary>
    /// Label for the first action button.
    /// </summary>
    public string? Button1Label { get; set; }

    /// <summary>
    /// URL target for the first action button.
    /// </summary>
    public string? Button1Url { get; set; }

    /// <summary>
    /// Label for the second action button.
    /// </summary>
    public string? Button2Label { get; set; }

    /// <summary>
    /// URL target for the second action button.
    /// </summary>
    public string? Button2Url { get; set; }
}