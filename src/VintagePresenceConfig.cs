using Vintagestory.API.Common;

// ReSharper disable RedundantDefaultMemberInitializer

namespace VintagePresence;

public class VintagePresenceConfig
{
    // Template Settings
    public string DetailsTemplate { get; set; } = Constants.DefaultDetailsTemplate;
    public string StateTemplate { get; set; } = Constants.DefaultStateTemplate;
    public string LargeImageKey { get; set; } = Constants.DefaultLargeImageKey;
    public string LargeImageText { get; set; } = Constants.DefaultLargeImageText;
    public string SmallImageKey { get; set; } = Constants.DefaultSmallImageKey;
    public string SmallImageText { get; set; } = Constants.DefaultSmallImageText;

    // Advanced
    public bool DebugLogging { get; set; } = false;
    public bool IsFirstTime { get; set; } = true;

    /// <summary>
    /// Validates and corrects invalid configuration values.
    /// </summary>
    /// <returns>True if any value has been corrected.</returns>
    public bool Validate(ICoreAPI api)
    {
        var corrected = false;

        // Validate templates (they cannot be empty).
        if (string.IsNullOrWhiteSpace(DetailsTemplate))
        {
            api.Logger.Warning($"{Constants.ModLogPrefix} Details template is empty. Using default.");
            DetailsTemplate = Constants.DefaultDetailsTemplate;
            corrected = true;
        }

        if (string.IsNullOrWhiteSpace(StateTemplate))
        {
            api.Logger.Warning($"{Constants.ModLogPrefix} State template is empty. Using default.");
            StateTemplate = Constants.DefaultStateTemplate;
            corrected = true;
        }

        // Validate Large Image Key
        if (string.IsNullOrWhiteSpace(LargeImageKey) ||
            !Constants.LargeImageOptions.Contains(LargeImageKey))
        {
            api.Logger.Warning($"{Constants.ModLogPrefix} Invalid large image key '{LargeImageKey}'. Using default.");
            LargeImageKey = Constants.DefaultLargeImageKey;
            corrected = true;
        }

        // Validate Small Image Key
        if (!string.IsNullOrWhiteSpace(SmallImageKey) &&
            Constants.SmallImageOptions.Contains(SmallImageKey)) return corrected;

        api.Logger.Warning($"{Constants.ModLogPrefix} Invalid small image key '{SmallImageKey}'. Using default.");
        SmallImageKey = Constants.DefaultSmallImageKey;
        corrected = true;

        return corrected;
    }

    /// <summary>
    /// Creates a configuration with clean default values.
    /// </summary>
    public static VintagePresenceConfig CreateDefault()
    {
        return new VintagePresenceConfig
        {
            IsFirstTime = false
        };
    }

    /// <summary>
    /// Creates a shallow copy of the current configuration instance.
    /// </summary>
    /// <returns>A new instance of <see cref="VintagePresenceConfig"/> with the same values as the current instance.</returns>
    public VintagePresenceConfig Clone()
    {
        return (VintagePresenceConfig)MemberwiseClone();
    }
}