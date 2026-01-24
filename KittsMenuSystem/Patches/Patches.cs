using HarmonyLib;
using KittsMenuSystem.Features.Menus;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Patchs;

[HarmonyPatch]
internal static class CompatibilityPatch
{
    [HarmonyPatch(typeof(ServerSpecificSettingBase), nameof(ServerSpecificSettingBase.OriginalDefinition), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool GetOriginalDefinition(ServerSpecificSettingBase __instance, ref ServerSpecificSettingBase __result)
    {
        int id = __instance.SettingId;

        foreach (Menu menu in MenuManager.RegisteredMenus)
            if (menu.DefinitionCache.TryGetValue(id, out __result))
                return false;

        __result = null;
        return false;
    }

    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.ServerPrevalidateClientResponse))]
    [HarmonyPrefix]
    private static bool Prevalidate(ref bool __result)
    {
        __result = true;
        return false;
    }
}
