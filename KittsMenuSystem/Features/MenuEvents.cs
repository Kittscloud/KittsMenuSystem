using KittsMenuSystem.Features.Menus;
using KittsMenuSystem.Features.Settings;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using MEC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UserSettings.ServerSpecific;
using Utf8Json;

namespace KittsMenuSystem.Features;

internal class MenuEvents : CustomEventsHandler
{
    public override void OnPlayerJoined(PlayerJoinedEventArgs ev) =>
        Timing.RunCoroutine(ev.Player.ReferenceHub.SyncAllMenus());

    public override void OnPlayerLeft(PlayerLeftEventArgs ev) =>
        ev.Player.ReferenceHub.DeleteFromSyncedMenus();

    public override void OnPlayerGroupChanged(PlayerGroupChangedEventArgs ev) =>
        ev.Player.ReferenceHub.ReloadCurrentMenu();

    public static void OnSettingReceived(ReferenceHub hub, ServerSpecificSettingBase ss)
    {
        Log.Debug("EventHandler.OnSettingReceived", $"Received input for {hub.nicknameSync.DisplayName}: {ss.SettingId} ({ss.GetType().Name})");

        try
        {
            // If still in sync cache, just store
            if (MenuManager.SyncCache.TryGetValue(hub, out List<ServerSpecificSettingBase> cache))
            {
                cache.Add(ss);
                Log.Debug("EventHandler.OnSettingReceived", "Redirected value to SyncCache");
                return;
            }

			Menu menu = hub.GetCurrentMenu();

			// Find the sent setting to act on
			menu.BuiltSettings.TryGetValue(hub, out List<BaseSetting> builtSettings);
			BaseSetting target = builtSettings?.FirstOrDefault(b => b.SettingId == ss.SettingId);

            if (target == null)
            {
                Log.Warn("EventHandler.OnSettingReceived", $"No target settings found, discarding input.");
                return;
            }

			Log.Debug("EventHandler.OnSettingReceived", $"Target setting found: {target.SettingId} ({target.GetType().Name})");

			// Sync setting
			target.Base = ss;

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
                // Restore setting id with has
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

            MenuState[hub] = (isOpen, currentMenu is KeybindMenu ? state.LastMenu : currentMenu);

            if (currentMenu is not KeybindMenu)
                hub.LoadMenu(new KeybindMenu());
        }
        else
        {
            hub.LoadMenu(state.LastMenu);
            MenuState[hub] = (isOpen, state.LastMenu);
        }
    }
}
