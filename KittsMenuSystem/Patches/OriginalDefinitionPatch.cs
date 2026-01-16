using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using KittsMenuSystem.Features;
using KittsMenuSystem.Features.Interfaces;
using UserSettings.ServerSpecific;
using static HarmonyLib.AccessTools;
using KittsMenuSystem.Features.Wrappers;

namespace KittsMenuSystem.Patchs;

[HarmonyPatch(typeof(ServerSpecificSettingBase), nameof(ServerSpecificSettingBase.OriginalDefinition), MethodType.Getter)]
internal class OriginalDefinitionPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent();

        newInstructions.AddRange(
        [
            new (OpCodes.Ldarg_0),
            new (OpCodes.Callvirt, PropertyGetter(typeof(ServerSpecificSettingBase), nameof(ServerSpecificSettingBase.SettingId))),
            new (OpCodes.Call, Method(typeof(OriginalDefinitionPatch), nameof(GetFirstSetting))),
            new (OpCodes.Ret),
        ]);

        for (int z = 0; z < newInstructions.Count; z++)
            yield return newInstructions[z];
        
        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }

    /// <summary>
    /// Get first setting correspondig to <see cref="Menu.Id"/>.
    /// </summary>
    /// <param name="id">Id of <see cref="ServerSpecificSettingBase"/>.</param>
    /// <returns><see cref="ServerSpecificSettingBase"/>, null if not found.</returns>
    private static ServerSpecificSettingBase GetFirstSetting(int id)
    {
        foreach (Menu menu in MenuManager.LoadedMenus)
            foreach (ServerSpecificSettingBase ss in menu.Settings)
            {
                int settingId = ss.SettingId + menu.Hash;

                if (settingId != id) continue;

                if (ss is ISetting setting)
                    return setting.Base;

                return ss;
            }

        return null;
    }
}