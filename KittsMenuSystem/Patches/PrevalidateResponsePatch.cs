using HarmonyLib;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Patchs;

/// <summary>
/// Patch to avoid checking <see cref="ServerSpecificSettingsSync.DefinedSettings"/>.
/// </summary>
[HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.ServerPrevalidateClientResponse))]
internal class PrevalidateResponsePatch
{
    private static bool Prefix(SSSClientResponse msg, ref bool __result)
    {
        __result = true;
        return false;
    }
}