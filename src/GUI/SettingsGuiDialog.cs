using Vintagestory.API.Client;

namespace VintagePresence.GUI;

public class SettingsGuiDialog : GuiDialog
{
    private const string HotkeyCode = Constants.ToggleKeyCombinationCode;
    private const string DialogId = "vintage-presence-settings-dialog";

    public override string ToggleKeyCombinationCode => HotkeyCode;
    public override bool DisableMouseGrab => true;

    public SettingsGuiDialog(ICoreClientAPI capi) : base(capi)
    {
        SetupDialog();
    }

    private void SetupDialog()
    {
        var dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);
        var textBounds = ElementBounds.Fixed(0, 40, 300, 100);
        var bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);

        bgBounds.BothSizing = ElementSizing.FitToChildren;
        bgBounds.WithChildren(textBounds);

        SingleComposer = capi.Gui.CreateCompo(DialogId, dialogBounds)
            .AddShadedDialogBG(bgBounds)
            .AddDialogTitleBar("Vintage Presence Settings", OnTitleBarCloseClicked)
            .AddStaticText("This is a test", CairoFont.WhiteDetailText(), textBounds)
            .Compose();
    }

    public void RegisterHotKey()
    {
        capi.Input.RegisterHotKey(
            Constants.ToggleKeyCombinationCode,
            "Open Settings",
            GlKeys.P,
            HotkeyType.GUIOrOtherControls,
            ctrlPressed: true,
            shiftPressed: true,
            altPressed: false
        );

        capi.Input.SetHotKeyHandler(Constants.ToggleKeyCombinationCode, OnKeyCombinationPressed);
    }

    private bool OnKeyCombinationPressed(KeyCombination keyCombination)
    {
        if (IsOpened()) OnTryClose();
        else TryOpen();

        return true;
    }

    private void OnTitleBarCloseClicked()
    {
        OnTryClose();
    }

    private void OnTryClose()
    {
        TryClose();
    }
}