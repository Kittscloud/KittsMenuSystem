using KittsMenuSystem.Features.Settings;
using System.Collections.Generic;
using System.Linq;

namespace KittsMenuSystem.Features.Menus;

/// <summary>
/// Used to load all keybind settings when menu tab is closed allowing all keybinds to be accessible when closed.
/// </summary>
internal class GlobalMenu : Menu
{
    public override List<BaseSetting> Settings(ReferenceHub hub) =>
        [.. MenuManager.RegisteredMenus
            .Where(m => m.CheckAccess(hub))
            .SelectMany(m => m.GetSettings(hub))
            .Where(s => s is Keybind)
        ];

    public override string Name { get; } = "Global Menu";
    public override int Id { get; } = 1;
}