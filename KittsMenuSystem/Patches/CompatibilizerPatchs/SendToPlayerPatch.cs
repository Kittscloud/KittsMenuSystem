using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using KittsMenuSystem.Features;
using UserSettings.ServerSpecific;
using static HarmonyLib.AccessTools;

namespace KittsMenuSystem.Patchs.CompatibilizerPatchs;

[HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer))]
[HarmonyPatch([typeof(ReferenceHub), typeof(ServerSpecificSettingBase[]), typeof(int?)])]
internal class SendToPlayerPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> transpiler, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent();

        newInstructions.AddRange(
        [
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldarg_1),
            new(OpCodes.Ldarg_2),
            new(OpCodes.Call, Method(typeof(Assembly), nameof(Assembly.GetCallingAssembly))),
            new(OpCodes.Call, Method(typeof(SendToPlayerPatch), nameof(SendToPlayer))),
            new(OpCodes.Ret)
        ]);

        foreach (CodeInstruction z in newInstructions)
            yield return z;

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }

    private static void SendToPlayer(ReferenceHub hub, ServerSpecificSettingBase[] settings, int? versionOverride, Assembly assembly)
    {
        AssemblyMenu menu = MenuManager.GetMenu(assembly);

        if (menu == null)
        {
            Log.Warn("SendToPlayerPatch.SendToPlayer", $"Assembly {assembly.GetName().Name} tried to send {settings.Length} settings but doesn't have a valid/registered menu.");
            Compatibilizer.Load([]);
            menu = MenuManager.GetMenu(assembly);
        }

        menu.ActuallySentToClient[hub] = settings;

        if (hub.GetCurrentMenu() == menu)
            menu.ReloadFor(hub);
        else
            hub.LoadMenu(null);
    }
}