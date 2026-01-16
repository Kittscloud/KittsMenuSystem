using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Mirror;
using NorthwoodLib.Pools;
using KittsMenuSystem.Features;
using UnityEngine;
using UserSettings.ServerSpecific;
using static HarmonyLib.AccessTools;

namespace KittsMenuSystem.Patchs.CompatibilizerPatchs;

[HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Setter)]
internal static class Compatibilizer
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent();
        newInstructions.InsertRange(0,
        [
            // Comptabilisater.Load(value);
            new(OpCodes.Ldarg_0),
            new(OpCodes.Call, Method(typeof(Compatibilizer), nameof(Load))),
            new(OpCodes.Ret),
        ]);

        foreach (CodeInstruction z in newInstructions)
            yield return z;

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }

    private static readonly HashSet<Assembly> LockedAssembly = [];

    internal static void Load(ServerSpecificSettingBase[] settings)
    {
        if (!KittsMenuSystem.Config.CompatibilityEnabled)
            return;

        Assembly assembly = Assembly.GetCallingAssembly();

        Log.Debug("Compatibilizer.Load", $"{assembly.GetName().Name} tried to set {nameof(ServerSpecificSettingsSync.DefinedSettings)}. Game Assembly: {typeof(ReferenceHub).Assembly.GetName().Name}");

        if (LockedAssembly.Contains(assembly) || assembly == typeof(ReferenceHub).Assembly)
        {
            Log.Debug("Compatibilizer.Load", "Assembly is locked or is a part of base game.");
            return;
        }

        if (MenuManager.LoadedMenus.OfType<AssemblyMenu>().Any(x => x.Assembly == assembly))
        {
            AssemblyMenu m = MenuManager.LoadedMenus.OfType<AssemblyMenu>().First(m => m.Assembly == assembly);

            m.OverrideSettings = settings;

            if (m.OverrideSettings?.First() is SSGroupHeader)
            {
                m.Name = m.OverrideSettings.First().Label;
                m.OverrideSettings = [.. m.OverrideSettings.Skip(1)];
            }

            foreach (ReferenceHub hub in ReferenceHub.AllHubs.Where(h => h.GetCurrentMenu() == null))
                hub.LoadMenu(null);

            m.ReloadForAll();
            return;
        }

        string name = assembly.GetName().Name;

        AssemblyMenu menu = new()
        {
            Assembly = assembly,
            OverrideSettings = settings,
            Name = name,
        };

        if (menu.OverrideSettings?.First() is SSGroupHeader)
        {
            menu.Name = menu.OverrideSettings.First().Label;
            menu.OverrideSettings = [.. menu.OverrideSettings.Skip(1)];
        }
        else if (LabApi.Loader.PluginLoader.Plugins.Any(x => x.Value == assembly))
            menu.Name = LabApi.Loader.PluginLoader.Plugins.First(x => x.Value == assembly).Key.Name;

        if (MenuManager.LoadedMenus.Any(x => x.Name == menu.Name))
        {
            Log.Warn("Compatibilizer.Load", $"Assembly {name} tried to register by compatibilisation menu {menu.Name} but a menu already exist with this name. Using assembly name");
            menu.Name = name;
        }

        if (MenuManager.LoadedMenus.Any(x => x.Name == menu.Name))
        {
            Log.Error("Compatibilizer.Load", $"Assembly {name} tried to register by compatibilisation but a menu was already registered with this name. Aborting needed.");
            LockedAssembly.Add(assembly);
            return;
        }

        menu.Id = -Mathf.Abs(menu.Name.GetStableHashCode());

        menu.Register();

        foreach (ReferenceHub hub in ReferenceHub.AllHubs.Where(h => h.GetCurrentMenu() == null))
            hub.LoadMenu(null);
    }
}