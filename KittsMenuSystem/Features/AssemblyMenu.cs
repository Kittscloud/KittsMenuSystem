using KittsMenuSystem.Features.Wrappers;
using System.Collections.Generic;
using System.Reflection;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features;

internal class AssemblyMenu : Menu
{
    internal Assembly Assembly { get; set; }

    internal ServerSpecificSettingBase[] OverrideSettings { get; set; }
    public override ServerSpecificSettingBase[] Settings => OverrideSettings ?? [];

    public override string Name { get; set; }
    public override int Id { get; set; }

    public override bool CheckAccess(ReferenceHub hub) =>
        (ActuallySentToClient.TryGetValue(hub, out ServerSpecificSettingBase[] settings) && settings != null && !settings.IsEmpty()) ||
        (OverrideSettings != null && !OverrideSettings.IsEmpty());

    internal Dictionary<ReferenceHub, ServerSpecificSettingBase[]> ActuallySentToClient { get; set; } = [];
}