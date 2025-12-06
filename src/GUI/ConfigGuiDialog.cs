using Vintagestory.API.Client;

namespace VintagePresence.GUI;

public class ConfigGuiDialog : GuiDialog
{
    private const string DialogId = "vintage-presence-settings-dialog";
    private const int SwitchSize = 24;
    private const int TextOffsetX = 40;

    public override string ToggleKeyCombinationCode => Constants.ToggleKeyCombinationCode;
    public override bool DisableMouseGrab => true;

    private VintagePresenceConfig _config;

    public ConfigGuiDialog(ICoreClientAPI capi) : base(capi)
    {
        var liveConfig = VintagePresenceMod.LoadConfig(capi);

        // Always work with a local copy so as not to affect the mod until you save.
        _config = liveConfig.Clone();
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

            // Details template
            .AddStaticText("Details template",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(0, 30, 150, 20))
            .AddTextInput(
                ElementBounds.Fixed(0, 50, 380, 25),
                val => _config.DetailsTemplate = val,
                CairoFont.WhiteSmallText(),
                "DetailsTemplate")

            // State template
            .AddStaticText("State template",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(0, 85, 150, 20))
            .AddTextInput(
                ElementBounds.Fixed(0, 105, 380, 25),
                val => _config.StateTemplate = val,
                CairoFont.WhiteSmallText(),
                "StateTemplate")

            // Large Image Key
            .AddStaticText("Large image",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(0, 140, 150, 20))
            .AddDropDown(
                Constants.LargeImageOptions,
                Constants.LargeImageOptions,
                Array.IndexOf(Constants.LargeImageOptions, _config.LargeImageKey),
                (code, _) => _config.LargeImageKey = code,
                ElementBounds.Fixed(0, 160, 380, 25),
                "LargeImageKey")

            // LargeImageText
            .AddStaticText("Large image text",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(0, 195, 150, 20))
            .AddTextInput(
                ElementBounds.Fixed(0, 215, 380, 25),
                val => _config.LargeImageText = val,
                CairoFont.WhiteSmallText(),
                "LargeImageText")

            // Small Image Key
            .AddStaticText("Small image",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(0, 250, 150, 20))
            .AddDropDown(
                Constants.SmallImageOptions,
                Constants.SmallImageOptions,
                Array.IndexOf(Constants.SmallImageOptions, _config.SmallImageKey),
                (code, _) => _config.SmallImageKey = code,
                ElementBounds.Fixed(0, 270, 380, 25),
                "SmallImageKey")

            // SmallImageText
            .AddStaticText("Small image text",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(0, 305, 150, 20))
            .AddTextInput(
                ElementBounds.Fixed(0, 325, 380, 25),
                val => _config.SmallImageText = val,
                CairoFont.WhiteSmallText(),
                "SmallImageText")

            // Debug Logging
            .AddSwitch(val => _config.DebugLogging = val,
                ElementBounds.Fixed(0, 360, SwitchSize, SwitchSize),
                "DebugLogging")
            .AddStaticText("Debug Logging",
                CairoFont.WhiteDetailText(),
                ElementBounds.Fixed(TextOffsetX, 362, 300, 24))

            // Buttons
            .AddSmallButton("Save", OnSave, ElementBounds.Fixed(0, 405, 100, 30))
            .AddSmallButton("Reset", OnReset, ElementBounds.Fixed(110, 405, 100, 30))
            .AddSmallButton("Cancel", OnCancel, ElementBounds.Fixed(220, 405, 100, 30))
            .EndChildElements();

        SingleComposer = composer.Compose();

        SingleComposer.GetTextInput("DetailsTemplate").SetValue(_config.DetailsTemplate);
        SingleComposer.GetTextInput("StateTemplate").SetValue(_config.StateTemplate);
        SingleComposer.GetTextInput("LargeImageText").SetValue(_config.LargeImageText);
        SingleComposer.GetTextInput("SmallImageText")?.SetValue(_config.SmallImageText);
        SingleComposer.GetSwitch("DebugLogging").SetValue(_config.DebugLogging);
    }

    private bool OnSave()
    {
        var wasCorrect = _config.Validate(capi);

        if (wasCorrect)
        {
            // If a correction was made, update the UI to show the corrected values.
            SingleComposer.GetTextInput("DetailsTemplate").SetValue(_config.DetailsTemplate);
            SingleComposer.GetTextInput("StateTemplate").SetValue(_config.StateTemplate);
            SingleComposer.GetTextInput("LargeImageText").SetValue(_config.LargeImageText);
            SingleComposer.GetTextInput("SmallImageText").SetValue(_config.SmallImageText);

            SingleComposer.GetDropDown("LargeImageKey").SetSelectedIndex(
                Array.IndexOf(Constants.LargeImageOptions, _config.LargeImageKey));
            SingleComposer.GetDropDown("SmallImageKey").SetSelectedIndex(
                Array.IndexOf(Constants.SmallImageOptions, _config.SmallImageKey));

            capi.ShowChatMessage(
                $"{Constants.ModLogPrefix} Some settings were corrected. Please review before closing.");
            return true; // Keep it open for the user to see the fixes.
        }

        VintagePresenceMod.SaveConfig(capi, _config);
        TryClose();
        return true;
    }

    private bool OnReset()
    {
        _config = VintagePresenceConfig.CreateDefault();

        // Rebuilds the dialog with the default values.
        SingleComposer?.Dispose();
        SetupDialog();

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