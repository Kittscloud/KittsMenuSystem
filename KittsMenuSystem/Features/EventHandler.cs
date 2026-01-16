using KittsMenuSystem.Features;
using KittsMenuSystem.Features.Wrappers;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UserSettings.ServerSpecific;
using static UnityEngine.GraphicsBuffer;

namespace KittsMenuSystem.Features;

internal class EventHandler : CustomEventsHandler
{
    public override void OnPlayerJoined(PlayerJoinedEventArgs ev) => 
        Timing.RunCoroutine(ev.Player.ReferenceHub.SyncAll());

    public override void OnPlayerLeft(PlayerLeftEventArgs ev) =>
        ev.Player.ReferenceHub.DeleteFromMenuSync();

    public override void OnPlayerGroupChanged(PlayerGroupChangedEventArgs ev) =>
        Timing.CallDelayed(0.1f, () => { if (!Parameters.SyncCache.ContainsKey(ev.Player.ReferenceHub)) ev.Player.ReferenceHub.ReloadCurrentMenu(); });

    public static void OnSettingReceived(ReferenceHub hub, ServerSpecificSettingBase ss)
    {
        Log.Debug("EventHandler.OnSettingReceived", $"Received input for {hub.nicknameSync.DisplayName}: {ss.SettingId} ({ss.GetType().Name})");

        try
        {
            if (Parameters.SyncCache.TryGetValue(hub, out List<ServerSpecificSettingBase> value))
            {
                value.Add(ss);
                Log.Debug("EventHandler.OnSettingReceived", "Received value that been flagged as \"SyncCached\". Redirected values to Cache.");
                return;
            }

            // Special case: return to main menu
            if (ss.SettingId == -999)
            {
                hub.LoadMenu(null);
                return;
            }

            // Restore original definition or adjust for pins/headers
            if (ss.OriginalDefinition != null)
            {
                ss.Label = ss.OriginalDefinition.Label;
                ss.HintDescription = ss.OriginalDefinition.HintDescription;
                ss.SettingId = ss.OriginalDefinition.SettingId;
            }
            else
                ss.SettingId -= hub.GetCurrentMenu()?.Hash ?? 0;

            Log.Debug("EventHandler.OnSettingReceived", $"Adjusted values: {ss.SettingId} ({ss.GetType().Name})");

            Menu menu = hub.GetCurrentMenu();

            // Check menu access
            if (!menu?.CheckAccess(hub) ?? false)
            {
                Log.Warn("EventHandler.OnSettingReceived", $"{hub.nicknameSync.DisplayName} tried to access {menu?.Name ?? "unknown menu"} without permission.");
                hub.LoadMenu(null);
                return;
            }

            static Keybind TryGetKeybinding(ReferenceHub hub, ServerSpecificSettingBase ss, Menu menu)
            {
                if (menu == null)
                    return null;

                if (!menu.SentSettings.TryGetValue(hub, out var sentSettings))
                    return null;

                return sentSettings
                    .OfType<Keybind>()
                    .FirstOrDefault(k =>
                        k.SettingId == ss.SettingId ||
                        k.SettingId == ss.SettingId - menu.Hash ||
                        k.SettingId - menu.Hash == ss.SettingId);
            }

            // Handle Keybinds
            if (ss is SSKeybindSetting kbSetting && menu != null)
            {
                Keybind kb = TryGetKeybinding(hub, ss, menu);
                if (kb != null)
                {
                    kb.Action?.Invoke(hub, kbSetting.SyncIsPressed, kbSetting);
                    return;
                }
            }

            // Navigate to upper menu if SettingId == 0
            if (ss.SettingId == 0 && menu != null)
            {
                hub.LoadMenu(menu.MenuRelated.GetMenu());
                return;
            }

            // Handle submenu or regular setting actions
            if (menu != null)
            {
                // Open a submenu if it exists
                if (menu.TryGetSubMenu(ss.SettingId, out Menu subMenu))
                {
                    hub.LoadMenu(subMenu);
                    return;
                }

                // Update internal synced settings
                List<ServerSpecificSettingBase> hubSettings = menu.InternalSettingsSync[hub];
                int index = hubSettings.FindIndex(x => x.SettingId == ss.SettingId);
                if (index != -1) hubSettings[index] = ss;
                else hubSettings.Add(ss);

                // Find the sent setting to act on
                menu.SentSettings.TryGetValue(hub, out var customSettings);
                ServerSpecificSettingBase target = (customSettings?.FirstOrDefault(b => b.SettingId == ss.SettingId)
                    ?? customSettings?.FirstOrDefault(b => b.SettingId == ss.SettingId - menu.Hash)
                    ?? customSettings?.FirstOrDefault(b => b.SettingId - menu.Hash == ss.SettingId))
                    ?? throw new Exception("Failed to find the sent setting for the hub.");

                Log.Debug("EventHandler.OnSettingReceived", $"Target setting found: {target.SettingId} ({target.GetType().Name})");

                // Invoke action when actually invoked by player
                switch (target)
                {
                    case Button btn: if (btn.SyncLastPress.ElapsedTicks == 0L) btn.Action?.Invoke(hub, btn); break;
                    case Dropdown dd: dd.Action?.Invoke(hub, ((SSDropdownSetting)ss).SyncSelectionIndexRaw, (dd as SSDropdownSetting)); break;
                    case Slider sl: sl.Action?.Invoke(hub, ((SSSliderSetting)ss).SyncFloatValue, (sl as SSSliderSetting)); break;
                    case YesNoButton yn: yn.Action?.Invoke(hub, ((SSTwoButtonsSetting)ss).SyncIsA, (yn as SSTwoButtonsSetting)); break;
                    case Plaintext pt: pt.OnChanged?.Invoke(hub, ((SSPlaintextSetting)ss).SyncInputText, (pt as SSPlaintextSetting)); break;
                    default: Log.Error("EventHandler.OnSettingReceived", $"Unhandled setting type: {target.SettingId} ({target.GetType().Name})"); return;
                }

                // Adjust SettingId after menu hash
                if (ss.SettingId > menu.Hash)
                    ss.SettingId -= menu.Hash;

                // Trigger menu input event
                menu.OnInput(hub, ss);
            }
            else
            {
                // Load a standalone menu if exists
                var m = MenuManager.LoadedMenus.FirstOrDefault(x => x.Id == ss.SettingId) ?? throw new KeyNotFoundException($"Invalid menu ID {ss.SettingId}. Please report this bug.");
                hub.LoadMenu(m);
            }
        }
        catch (Exception e)
        {
            Log.Error("EventHandler.OnSettingReceived", $"Error receiving input {ss.SettingId} ({ss.GetType().Name}): {e.Message}");
            Log.Debug(e.ToString());

            if (KittsMenuSystem.Config.ShowErrorToClient)
            {
                hub.SendMenu(null,
                [
                    new SSTextArea(-5, $"<color=red><b>{KittsMenuSystem.Config.Translation.ServerError}\n{((hub.serverRoles.RemoteAdmin || KittsMenuSystem.Config.ShowFullErrorToClient) && KittsMenuSystem.Config.ShowFullErrorToModerators ? e.ToString() : KittsMenuSystem.Config.Translation.NoPermission)}</b></color>", SSTextArea.FoldoutMode.CollapsedByDefault, KittsMenuSystem.Config.Translation.ServerError),
                    new SSButton(-999, KittsMenuSystem.Config.Translation.ReloadButton.Label, KittsMenuSystem.Config.Translation.ReloadButton.ButtonText)
                ]);
            }
        }
    }
}