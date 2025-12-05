using Vintagestory.API.Client;

namespace VintagePresence.GUI;

public class ConfigGuiDialog : GuiDialog
{
    private const string DialogId = "vintage-presence-settings-dialog";
    private const int SwitchSize = 24;
    private const int TextOffsetX = 40;

    public override string ToggleKeyCombinationCode => Constants.ToggleKeyCombinationCode;
    public override bool DisableMouseGrab => true;

    private readonly VintagePresenceConfig _config;

    public ConfigGuiDialog(ICoreClientAPI capi) : base(capi)
    {
        _config = VintagePresenceConfig.GetSettings(capi);
        _config.Validate(capi);
        SetupDialog();
    }

    private void SetupDialog()
    {
        var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
        var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;

        var composer = capi.Gui.CreateCompo(DialogId, dialogBounds)
            .AddShadedDialogBG(bgBounds)
            .AddDialogTitleBar("Presence Settings", OnTitleBarCloseClicked)
            .BeginChildElements(bgBounds)

            // Enable Rich Presence
            .AddSwitch(val => _config.EnableRichPresence = val,
                ElementBounds.Fixed(0, 40, SwitchSize, SwitchSize),
                "EnableRichPresence")
            .AddStaticText("Enable Rich Presence",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(TextOffsetX, 42, 300, 24))

            // Show Player Name
            .AddSwitch(val => _config.ShowPlayerName = val,
                ElementBounds.Fixed(0, 75, SwitchSize, SwitchSize),
                "ShowPlayerName")
            .AddStaticText("Show Player Name",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(TextOffsetX, 77, 300, 24))

            // Show Server Info
            .AddSwitch(val => _config.ShowServerInfo = val,
                ElementBounds.Fixed(0, 110, SwitchSize, SwitchSize),
                "ShowServerInfo")
            .AddStaticText("Show Server Info",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(TextOffsetX, 112, 300, 24))

            // Show Deaths
            .AddSwitch(val => _config.ShowDeaths = val,
                ElementBounds.Fixed(0, 145, SwitchSize, SwitchSize),
                "ShowDeaths")
            .AddStaticText("Show Deaths",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(TextOffsetX, 147, 300, 24))

            // Show Playtime
            .AddSwitch(val => _config.ShowPlaytime = val,
                ElementBounds.Fixed(0, 180, SwitchSize, SwitchSize),
                "ShowPlaytime")
            .AddStaticText("Show Playtime",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(TextOffsetX, 182, 300, 24))

            // Show Timestamp
            .AddSwitch(val => _config.ShowTimestamp = val,
                ElementBounds.Fixed(0, 215, SwitchSize, SwitchSize),
                "ShowTimestamp")
            .AddStaticText("Show Timestamp",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(TextOffsetX, 217, 300, 24))

            // Reset On Death
            .AddSwitch(val => _config.ResetOnDeath = val,
                ElementBounds.Fixed(0, 250, SwitchSize, SwitchSize),
                "ResetOnDeath")
            .AddStaticText("Reset on Death",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(TextOffsetX, 252, 300, 24))

            // Debug Logging
            .AddSwitch(val => _config.DebugLogging = val,
                ElementBounds.Fixed(0, 285, SwitchSize, SwitchSize),
                "DebugLogging")
            .AddStaticText("Debug Logging",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(TextOffsetX, 287, 300, 24))

            // Update Interval
            .AddStaticText("Update Interval (seconds):",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(0, 325, 200, 24))
            .AddTextInput(ElementBounds.Fixed(210, 323, 100, 30),
                OnIntervalChanged,
                CairoFont.WhiteDetailText(),
                "UpdateInterval")

            // Buttons
            .AddSmallButton("Save", OnSave, ElementBounds.Fixed(0, 365, 100, 30))
            .AddSmallButton("Cancel", OnCancel, ElementBounds.Fixed(110, 365, 100, 30))
            .EndChildElements();

        SingleComposer = composer.Compose();

        SingleComposer.GetSwitch("EnableRichPresence").SetValue(_config.EnableRichPresence);
        SingleComposer.GetSwitch("ShowPlayerName").SetValue(_config.ShowPlayerName);
        SingleComposer.GetSwitch("ShowServerInfo").SetValue(_config.ShowServerInfo);
        SingleComposer.GetSwitch("ShowDeaths").SetValue(_config.ShowDeaths);
        SingleComposer.GetSwitch("ShowPlaytime").SetValue(_config.ShowPlaytime);
        SingleComposer.GetSwitch("ShowTimestamp").SetValue(_config.ShowTimestamp);
        SingleComposer.GetSwitch("ResetOnDeath").SetValue(_config.ResetOnDeath);
        SingleComposer.GetSwitch("DebugLogging").SetValue(_config.DebugLogging);
        SingleComposer.GetTextInput("UpdateInterval").SetValue(_config.UpdateIntervalSeconds.ToString());
    }

    private void OnIntervalChanged(string value)
    {
        if (int.TryParse(value, out var interval))
        {
            _config.UpdateIntervalSeconds = Math.Max(5, interval);
        }
    }

    private bool OnSave()
    {
        _config.Validate(capi);
        capi.StoreModConfig(_config, Constants.ConfigFile);
        capi.ShowChatMessage("Rich Presence settings saved!");
        TryClose();
        return true;
    }

    private bool OnCancel()
    {
        TryClose();
        return true;
    }

    public void RegisterHotKey()
    {
        capi.Input.RegisterHotKey(
            Constants.ToggleKeyCombinationCode,
            "Open Presence Settings",
            GlKeys.P,
            HotkeyType.GUIOrOtherControls,
            ctrlPressed: true,
            shiftPressed: true
        );

        capi.Input.SetHotKeyHandler(Constants.ToggleKeyCombinationCode, _ =>
        {
            if (IsOpened())
                TryClose();
            else
                TryOpen();

            return true;
        });
    }

    private void OnTitleBarCloseClicked()
    {
        TryClose();
    }
}