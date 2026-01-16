using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Patchs.CompatibilizerPatchs;

[HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), [typeof(ReferenceHub)])]
internal class SendToPlayerDSPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> transpiler, ILGenerator generator)
    {
        yield return new CodeInstruction(OpCodes.Ret);
    }
}