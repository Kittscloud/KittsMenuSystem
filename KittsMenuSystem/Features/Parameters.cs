using KittsMenuSystem.Features.Interfaces;
using KittsMenuSystem.Features.Wrappers;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features;

/// <summary>
/// Get parameters from <see cref="Menu"/>.
/// </summary>
public static class Parameters
{
    /// <summary>
    /// Get synced parameter value for <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <param name="settingId">Id of setting.</param>
    /// <typeparam name="TMenu">Target <see cref="Menu"/>.</typeparam>
    /// <typeparam name="TSs">Setting type.</typeparam>
    /// <returns>Instance of <see cref="ServerSpecificSettingBase"/> containing synecd value. null if not found.</returns>
    public static TSs GetParameter<TMenu, TSs>(this ReferenceHub hub, int settingId)
        where TMenu : Menu
        where TSs : ServerSpecificSettingBase
    {
        if (typeof(TSs).BaseType == typeof(ISetting))
        {
            Log.Error("Parameters.GetParameter", $"{nameof(TSs)} needs to be of base type.");
            return null;
        }

        foreach (Menu menu in MenuManager.LoadedMenus.Where(x => x is TMenu))
        {
            if (!menu.SettingsSync.TryGetValue(hub, out List<ServerSpecificSettingBase> settings))
                continue;

            ServerSpecificSettingBase t = settings.Where(x => x is TSs).FirstOrDefault(x => x.SettingId == settingId);
            return t as TSs;
        }

        return null;
    }

    /// <summary>
    /// Sync parameters from multiple <see cref="Menu"/>s for a <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <param name="menus"><see cref="Menu"/>s to sync.</param>
    /// <param name="parameters">Parameters to send after syncing.</param>
    /// <returns>Enumerator for <see cref="Timing.RunCoroutine(IEnumerator{float})"/>.</returns>
    internal static IEnumerator<float> SyncMenus(this ReferenceHub hub, IEnumerable<Menu> menus, ServerSpecificSettingBase[] parameters = null)
    {
        SyncCache.Add(hub, []);

        Menu[] accessibleMenus = [.. menus.Where(m => m.CheckAccess(hub))];

        for (int i = 0; i < accessibleMenus.Length; i++)
        {
            bool isLast = i == accessibleMenus.Length - 1;

            IEnumerator<float> enumerator = hub.SyncMenu(accessibleMenus[i], isLast ? parameters : null, isLast);

            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        SyncCache.Remove(hub);
    }

    /// <summary>
    /// Sync parameters from a single <see cref="Menu"/> for a <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <param name="menu"><see cref="Menu"/> to sync.</param>
    /// <param name="parameters">Parameters to send after syncing.</param>
    /// <param name="isLastMenu">Is last <see cref="Menu"/>?</param>
    /// <returns>Enumerator for <see cref="Timing.RunCoroutine(IEnumerator{float})"/>.</returns>
    internal static IEnumerator<float> SyncMenu(this ReferenceHub hub, Menu menu, ServerSpecificSettingBase[] parameters, bool isLastMenu = false)
    {
        const float timeoutLimit = 10f;
        const float waitTime = 0.01f;

        if (!menu.CheckAccess(hub))
        {
            Log.Debug("Parameters.SyncMenu", $"{hub.nicknameSync.DisplayName} does not have access to {menu.Name}.");
            yield break;
        }

        Log.Debug("Parameters.SyncMenu", $"Syncing {hub.nicknameSync.DisplayName} settings for menu {menu.Name}.");

        List<ServerSpecificSettingBase> sendSettings = [.. menu.Settings
        .Concat(menu.GetSettingsFor(hub) ?? [])
        .Where(s => s.ResponseMode == ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange)
        .Select(s => s is ISetting set ? set.Base : s)];

        hub.SendMenu(menu, [.. sendSettings]);

        float timeout = 0f;
        while (SyncCache[hub].Count < sendSettings.Count && timeout < timeoutLimit)
        {
            timeout += waitTime;
            yield return waitTime;
        }

        if (SyncCache[hub].Count < sendSettings.Count)
        {
            Log.Error("Parameters.SyncMenu", $"Timeout syncing {hub.nicknameSync.DisplayName} on {menu.Name}.");
            yield break;
        }

        foreach (ServerSpecificSettingBase setting in SyncCache[hub])
        {
            ServerSpecificSettingBase original = sendSettings.FirstOrDefault(s => s.SettingId == setting.SettingId);

            if (original == null)
                continue;

            setting.Label = original.Label;
            setting.SettingId -= menu.Hash;
            setting.HintDescription = original.HintDescription;
        }

        menu.InternalSettingsSync[hub] = [.. SyncCache[hub]];

        Log.Debug("Parameters.SyncMenu", $"Synced {menu.InternalSettingsSync[hub].Count} settings for {hub.nicknameSync.DisplayName} in {menu.Name}.");

        sendSettings.Clear();
        SyncCache[hub].Clear();

        if (!isLastMenu)
            yield break;

        if (parameters != null)
            hub.SendMenu(menu, parameters);

        hub.LoadMenu(null);
    }

    /// <summary>
    /// Sync all accessible <see cref="Menu"/>s for a <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <returns>Enumerator for <see cref="Timing.RunCoroutine(IEnumerator{float})"/>.</returns>
    internal static IEnumerator<float> SyncAll(this ReferenceHub hub)
    {
        Menu[] accessibleMenus = [.. MenuManager.LoadedMenus.Where(m => m.CheckAccess(hub))];
        if (!accessibleMenus.Any())
        {
            Log.Warn("Parameters.SyncAll", $"No accessible menus for {hub.nicknameSync.DisplayName}.");
            yield break;
        }

        // Proper way to delegate to another IEnumerator<float>
        IEnumerator<float> enumerator = hub.SyncMenus(accessibleMenus);
        while (enumerator.MoveNext())
            yield return enumerator.Current;
    }

    /// <summary>
    /// Parameters sync cache, updated on <see cref="EventHandler.OnSettingReceived"/>.
    /// </summary>
    internal static readonly Dictionary<ReferenceHub, List<ServerSpecificSettingBase>> SyncCache = [];

    /// <summary>
    /// Get all synced parameters for <see cref="ReferenceHub"/>
    /// </summary>
    /// <param name="referenceHub">Target <see cref="ReferenceHub"/>.</param>
    /// <returns>All synced parameters for <see cref="ReferenceHub"/>.</returns>
    public static List<ServerSpecificSettingBase> GetAllSyncedParameters(ReferenceHub referenceHub)
    {
        List<ServerSpecificSettingBase> toReturn = [];

        foreach (Menu menu in MenuManager.LoadedMenus.Where(x => x.InternalSettingsSync.ContainsKey(referenceHub)))
            toReturn.AddRange(menu.InternalSettingsSync[referenceHub]);

        return toReturn;
    }

    /// <summary>
    /// Get synced parameters for target menu.
    /// </summary>
    /// <param name="referenceHub">Target <see cref="ReferenceHub"/>.</param>
    /// <typeparam name="T">Target <see cref="Menu"/>.</typeparam>
    /// <returns>All synced parameters from target <see cref="Menu"/> for <see cref="ReferenceHub"/>.</returns>
    public static List<ServerSpecificSettingBase> GetMenuSpecificSyncedParameters<T>(ReferenceHub referenceHub) where T : Menu
    {
        List<ServerSpecificSettingBase> toReturn = [];

        foreach (Menu menu in MenuManager.LoadedMenus.Where(x => x.InternalSettingsSync.ContainsKey(referenceHub) && x is T))
            toReturn.AddRange(menu.InternalSettingsSync[referenceHub]);

        return toReturn;
    }
}
