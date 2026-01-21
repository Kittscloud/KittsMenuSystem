using HarmonyLib;
using KittsMenuSystem.Features;
using KittsMenuSystem.Features.Menus;
using KittsMenuSystem.Features.Settings;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Patchs;

[HarmonyPatch]
internal static class CompatibilityPatch
{
    private static readonly HashSet<Assembly> LockedAssembly = [];

    private static void Load(ServerSpecificSettingBase[] settings)
    {
        Assembly assembly = Assembly.GetCallingAssembly();

        if (assembly == typeof(ReferenceHub).Assembly || LockedAssembly.Contains(assembly))
            return;

        Log.Debug("CompatibilityPatch.Load", $"{assembly.GetName().Name} setting {nameof(ServerSpecificSettingsSync.DefinedSettings)}");

        AssemblyMenu menu = MenuManager.RegisteredMenus
            .OfType<AssemblyMenu>()
            .FirstOrDefault(m => m.Assembly == assembly);

        bool hasHeader = settings?.FirstOrDefault() is SSGroupHeader;
        string headerName = hasHeader ? settings[0].Label : null;
        ServerSpecificSettingBase[] finalSettings = hasHeader ? [.. settings.Skip(1)] : settings;

        // Convert all SSBase into BaseSetting
        List<BaseSetting> wrapped = finalSettings?.Select(BaseSetting.Wrap).ToList() ?? [];

        if (menu != null)
        {
            menu.OverrideSettings = wrapped;
            if (hasHeader)
                menu.Name = headerName;

            foreach (ReferenceHub hub in ReferenceHub.AllHubs.Where(h => h.GetCurrentMenu() == null))
                hub.LoadMenu(null);

            menu.ReloadForAll();
            return;
        }

        string name = assembly.GetName().Name;
        if (!hasHeader && LabApi.Loader.PluginLoader.Plugins.Any(p => p.Value == assembly))
            name = LabApi.Loader.PluginLoader.Plugins.First(p => p.Value == assembly).Key.Name;

        if (MenuManager.RegisteredMenus.Any(m => m.Name == name))
        {
            Log.Warn("CompatibilityPatch.Load", $"Assembly {name} tried duplicate menu name, using assembly name.");
            name = assembly.GetName().Name;
        }

        if (MenuManager.RegisteredMenus.Any(m => m.Name == name))
        {
            Log.Error("CompatibilityPatch.Load", $"Assembly {name} failed to register menu.");
            LockedAssembly.Add(assembly);
            return;
        }

        menu = new AssemblyMenu
        {
            Assembly = assembly,
            Name = hasHeader ? headerName : name,
            OverrideSettings = wrapped,
            Id = -Mathf.Abs(name.GetStableHashCode())
        };

        menu.Register();

        foreach (ReferenceHub hub in ReferenceHub.AllHubs.Where(h => h.GetCurrentMenu() == null))
            hub.LoadMenu(null);
    }

    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Setter)]
    [HarmonyPrefix]
    private static bool SetDefinedSettings(ServerSpecificSettingBase[] value)
    {
        Load(value);
        return false;
    }

    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool GetDefinedSettings(ref ServerSpecificSettingBase[] __result)
    {
        Assembly asm = Assembly.GetCallingAssembly();
        __result = asm == typeof(ReferenceHub).Assembly ? [] : MenuManager.GetMenu(asm)?.OverrideSettings.Select(bs => bs.Base).ToArray();
        return false;
    }

    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub))]
    [HarmonyPrefix]
    private static bool SendToPlayer(ReferenceHub hub) => false;

    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), [typeof(ReferenceHub), typeof(ServerSpecificSettingBase[]), typeof(int?)])]
    [HarmonyPrefix]
    private static bool SendToPlayer(ReferenceHub hub, ServerSpecificSettingBase[] collection, int? versionOverride = null)
    {
        Assembly asm = Assembly.GetCallingAssembly();
        AssemblyMenu menu = MenuManager.GetMenu(asm);

        if (menu == null)
        {
            Load([]);
            menu = MenuManager.GetMenu(asm);
        }

        // Wrap collection in BaseSetting
        menu.ActuallySentToClient[hub] = collection?.Select(BaseSetting.Wrap).ToList() ?? [];

        if (hub.GetCurrentMenu() == menu)
            menu.ReloadFor(hub);
        else
            hub.LoadMenu(null);

        return false;
    }

    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.ServerPrevalidateClientResponse))]
    [HarmonyPrefix]
    private static bool Prevalidate(ref bool __result)
    {
        __result = true;
        return false;
    }

    [HarmonyPatch(typeof(SSPlaintextSetting), nameof(SSPlaintextSetting.CharacterLimitOriginal), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool PlaintextLimit(ref int __result)
    {
        __result = 64;
        return false;
    }
}
