using KittsMenuSystem.Features.Settings;
using System.Collections.Generic;
using System.Linq;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Menus;

/// <summary>
/// Used to load keybinds when player not in settings so they can be access from everywhere.
/// </summary>
internal class KeybindMenu : Menu
{
    public override List<BaseSetting> Settings(ReferenceHub hub) =>
        [.. MenuManager.RegisteredMenus
            .Where(m => m.CheckAccess(hub))
            .SelectMany(m => m.GetSettings(hub))
            .Where(s => s.Base is SSKeybindSetting)
        ];

    public override string Name { get; set; } = "All Keybinds";
    public override int Id { get; set; } = 1;
}