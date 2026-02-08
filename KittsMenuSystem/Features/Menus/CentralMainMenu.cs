using KittsMenuSystem.Features.Settings;
using System.Collections.Generic;
using System.Linq;

namespace KittsMenuSystem.Features.Menus;

/// <summary>
/// Used to combine multiple main menus into one central main menu
/// </summary>
internal class CentralMainMenu : Menu
{
    public override List<BaseSetting> Settings(ReferenceHub hub)
    {
        List<BaseSetting> settings = [];
        List<Menu> mainMenus = [.. MenuManager.RegisteredMenus.Where(m => m.CheckAccess(hub) && m.ParentMenu == null)];

        foreach (Menu menu in mainMenus)
        {
            settings.Add(new Button(
                string.Format(KittsMenuSystem.Config.Translation.OpenMenu.Label, menu.Name),
                KittsMenuSystem.Config.Translation.OpenMenu.ButtonText,
                (h, _) => h.LoadMenu(menu)
            ));
        }

        return settings;
    }

    public override string Name { get; } = "Main Menu";
    public override int Id { get; } = 0;
}