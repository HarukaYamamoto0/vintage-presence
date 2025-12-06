// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace VintagePresence.PresenceTemplate;

public class PresenceContext
{
    public required string GameMode { get; init; }
    public required double? Day { get; init; }
    public required string TimeOfDay { get; init; }

    public required string PlayerName { get; init; }
    public required double? Health { get; init; }
    public required double? MaxHealth { get; init; }
    public required int? Deaths { get; init; }

    public required int? OnlinePlayers { get; init; }

    public required string Coords { get; init; }

    public required double? Temperature { get; init; }
    public required string Weather { get; init; }

    public required string ModVersion { get; init; }
    
    public required string GameVersion { get; init; }
    public required string Season { get; init; }
}