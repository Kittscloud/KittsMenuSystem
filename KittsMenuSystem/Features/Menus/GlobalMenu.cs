using KittsMenuSystem.Features.Settings;
using System.Collections.Generic;
using System.Linq;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Menus;

/// <summary>
/// Used to load all settings when player is not in settings so they can be accessed from everywhere.
/// </summary>
internal class GlobalMenu : Menu
{
    public override List<BaseSetting> Settings(ReferenceHub hub)
    {
        List<BaseSetting> settings = [.. MenuManager.RegisteredMenus
            .Where(m => m.CheckAccess(hub))
            .SelectMany(m => m.GetSettings(hub, false, false))
            .Where(s => s.Base is SSKeybindSetting)
        ];

        foreach (Menu menu in MenuManager.RegisteredMenus.Where(m => m.CheckAccess(hub)))
            settings.AddRange(menu.GetSettings(hub, false, false));

        return settings;
    }

    public override string Name { get; } = "Global Menu";
    public override int Id { get; } = 1;
}