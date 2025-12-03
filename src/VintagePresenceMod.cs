using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VintagePresence;

// ReSharper disable once UnusedType.Global
public class VintagePresenceMod : ModSystem
{
    private const string ModLogPrefix = "[VintagePresence]";
    private const string ConfigFileName = "ForgeCordConfig.json";

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        api.Logger.Event($"{ModLogPrefix} loaded!");
    }

    public override bool ShouldLoad(EnumAppSide forSide)
    {
        return forSide == EnumAppSide.Client;
    }
}