// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace VintagePresence;

public enum DiscordTimestampMode
{
    None = 0,
    Elapsed,
    Remaining
}

public sealed class DiscordActivityOptions
{
    // Maintained for compatibility but not used directly in DiscordRPC.
    public int? Type { get; set; }
    public int? StatusDisplayType { get; set; }

    public string? Details { get; set; }
    public string? DetailsUrl { get; set; } // Not supported

    public string? State { get; set; }
    public string? StateUrl { get; set; } // Not supported

    public string? LargeImageKey { get; set; }
    public string? LargeImageText { get; set; }

    public string? SmallImageKey { get; set; }
    public string? SmallImageText { get; set; }

    public bool UseTimestamp { get; set; }
    public DiscordTimestampMode TimestampMode { get; set; } = DiscordTimestampMode.None;
    public double? EndTimeSeconds { get; set; }

    public string? Button1Label { get; set; }
    public string? Button1Url { get; set; }

    public string? Button2Label { get; set; }
    public string? Button2Url { get; set; }
}