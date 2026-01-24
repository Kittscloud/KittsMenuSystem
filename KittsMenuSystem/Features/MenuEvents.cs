using KittsMenuSystem.Features.Menus;
using KittsMenuSystem.Features.Settings;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features;

internal class MenuEvents : CustomEventsHandler
{
    public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
        foreach (Menu menu in MenuManager.RegisteredMenus.Where(m => m.CheckAccess(ev.Player.ReferenceHub)))
            menu.ReloadFor(ev.Player.ReferenceHub);

        ev.Player.ReferenceHub.LoadMenu(new GlobalMenu());
        MenuState[ev.Player.ReferenceHub] = (false, null);
    }

    public override void OnPlayerLeft(PlayerLeftEventArgs ev) =>
        ev.Player.ReferenceHub.DeleteFromSyncedMenus();

    public override void OnPlayerGroupChanged(PlayerGroupChangedEventArgs ev) =>
        ev.Player.ReferenceHub.ReloadCurrentMenu();

    public static void OnSettingReceived(ReferenceHub hub, ServerSpecificSettingBase ss)
    {
        Log.Debug("EventHandler.OnSettingReceived", $"Received input for {hub.nicknameSync.DisplayName}: {ss.SettingId} ({ss.GetType().Name})");

        try
        {   
            Menu menu = hub.GetCurrentMenu();

			BaseSetting target = menu.GetSettings(hub, false, false).FirstOrDefault(b => b.SettingId == ss.SettingId);

            if (target == null)
            {
                Log.Warn("EventHandler.OnSettingReceived", $"No target setting found, discarding input.");
                return;
            }

			Log.Debug("EventHandler.OnSettingReceived", $"Target setting found: {target.SettingId} ({target.GetType().Name})");

            // Sync setting
            target.Base = ss;

            // Restore original definition
            RestoreFromOriginal(ss, ss.OriginalDefinition);

            // Remove menu hash from setting id to send to invoking
            ss.SettingId -= menu.Hash;

            try
            {
                // Invoke action depending on type
                switch (target)
                {
                    case Button btn when ss is SSButton ssBtn && ssBtn.SyncLastPress.ElapsedMilliseconds == 0L: btn.OnPressed?.Invoke(hub, ssBtn); break;
                    case Dropdown dd when ss is SSDropdownSetting ssDd: dd.OnChanged?.Invoke(hub, ssDd.SyncSelectionIndexRaw, ssDd); break;
                    case Slider sl when ss is SSSliderSetting ssSl: sl.OnChanged?.Invoke(hub, ssSl.SyncFloatValue, ssSl); break;
                    case ABButton yn when ss is SSTwoButtonsSetting ssYn: yn.OnChanged?.Invoke(hub, ssYn.SyncIsA, ssYn); break;
                    case TextBox pt when ss is SSPlaintextSetting ssPt: pt.OnChanged?.Invoke(hub, ssPt.SyncInputText, ssPt); break;
                    case Keybind kb when ss is SSKeybindSetting ssKb: kb.OnUsed?.Invoke(hub, ssKb.SyncIsPressed, ssKb); break;
                    default: throw new InvalidCastException($"Unhandled BaseSetting type {target.GetType().Name}");
                }
            }
            finally
            {
                // Restore hash
                ss.SettingId += menu.Hash;
            }

            Log.Debug("EventHandler.OnSettingReceived", $"Successfully handled input for {hub.nicknameSync.DisplayName}: {ss.SettingId} ({ss.GetType().Name})");
        }
        catch (Exception e)
        {
            Log.Error("EventHandler.OnSettingReceived", $"Error processing {ss.SettingId} ({ss.GetType().Name}): {e.Message}");
            Log.Debug("EventHandler.OnSettingReceived", e.ToString());

            if (KittsMenuSystem.Config.ShowErrorToClient)
            {
                hub.SendSettings([
                    new TextArea($"<color=red><b>{KittsMenuSystem.Config.Translation.ServerError}\n{((hub.serverRoles.RemoteAdmin || KittsMenuSystem.Config.ShowFullErrorToClient) && KittsMenuSystem.Config.ShowFullErrorToModerators ? e.ToString() : KittsMenuSystem.Config.Translation.NoPermission)}</b></color>", SSTextArea.FoldoutMode.CollapsedByDefault, KittsMenuSystem.Config.Translation.ServerError),
                    new Button(KittsMenuSystem.Config.Translation.ReloadButton.Label, KittsMenuSystem.Config.Translation.ReloadButton.ButtonText, (h, _) => h.LoadMenu(null))
                ]);
            }
        }
    }

    private static void RestoreFromOriginal(ServerSpecificSettingBase target, ServerSpecificSettingBase original)
    {
        if (original == null)
            return;

        // base fields
        target.Label = original.Label;
        target.HintDescription = original.HintDescription;

        // type specific restore
        switch (target)
        {
            case SSButton btn when original is SSButton ob:
                btn.ButtonText = ob.ButtonText;
                break;
            case SSSliderSetting sl when original is SSSliderSetting os:
                sl.MinValue = os.MinValue;
                sl.MaxValue = os.MaxValue;
                sl.Integer = os.Integer;
                sl.ValueToStringFormat = os.ValueToStringFormat;
                sl.FinalDisplayFormat = os.FinalDisplayFormat;
                break;
            case SSDropdownSetting dd when original is SSDropdownSetting od:
                dd.Options = od.Options;
                dd.DefaultOptionIndex = od.DefaultOptionIndex;
                break;
            case SSTwoButtonsSetting ab when original is SSTwoButtonsSetting oa:
                ab.OptionA = oa.OptionA;
                ab.OptionB = oa.OptionB;
                ab.DefaultIsB = oa.DefaultIsB;
                break;
            case SSPlaintextSetting tb when original is SSPlaintextSetting ot:
                tb.CharacterLimit = ot.CharacterLimit;
                tb.ContentType = ot.ContentType;
                tb.Placeholder = ot.Placeholder;
                break;
            case SSKeybindSetting kb when original is SSKeybindSetting ok:
                kb.SuggestedKey = ok.SuggestedKey;
                kb.AssignedKeyCode = ok.AssignedKeyCode;
                break;
        }
    }

    internal static readonly Dictionary<ReferenceHub, (bool TabOpen, Menu LastMenu)> MenuState = [];

    public static void OnStatusReceived(ReferenceHub hub, SSSUserStatusReport sr)
    {
        bool isOpen = sr.TabOpen;

        if (!MenuState.TryGetValue(hub, out var state))
        {
            MenuState[hub] = (isOpen, hub.GetCurrentMenu());
            return;
        }

        if (state.TabOpen == isOpen)
            return;

        Log.Debug("EventHandler.OnStatusReceived", $"Tab state changed for {hub.nicknameSync.DisplayName}: {state.TabOpen} -> {isOpen}");

        if (!isOpen)
        {
            Menu currentMenu = hub.GetCurrentMenu();

            MenuState[hub] = (isOpen, currentMenu is GlobalMenu ? state.LastMenu : currentMenu);

            if (currentMenu is not GlobalMenu)
                hub.LoadMenu(new GlobalMenu());
        }
        else
        {
            hub.LoadMenu(state.LastMenu);
            MenuState[hub] = (isOpen, state.LastMenu);
        }
    }
}
