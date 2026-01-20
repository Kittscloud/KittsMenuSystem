using KittsMenuSystem.Features.Wrappers;
using System.Collections.Generic;
using System.Reflection;

namespace KittsMenuSystem.Features.Menus;

internal class AssemblyMenu : Menu
{
    internal Assembly Assembly { get; set; }

    internal List<BaseSetting> OverrideSettings { get; set; }
    public override List<BaseSetting> Settings(ReferenceHub hub) => OverrideSettings ?? [];

    public override string Name { get; set; }
    public override int Id { get; set; }

    public override bool CheckAccess(ReferenceHub hub) =>
        (ActuallySentToClient.TryGetValue(hub, out List<BaseSetting> settings) && settings != null && !settings.IsEmpty()) ||
        (OverrideSettings != null && !OverrideSettings.IsEmpty());

    internal Dictionary<ReferenceHub, List<BaseSetting>> ActuallySentToClient { get; set; } = [];
}