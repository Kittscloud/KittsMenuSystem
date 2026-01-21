using KittsMenuSystem.Features.Menus;
using KittsMenuSystem.Features.Settings;
using System.Collections.Generic;

namespace KittsMenuSystem.Examples;

internal class AccessExmaple : Menu
{
    // This is a menu to show the check access feature which is always false
    // Becuase it's always false you will never actually see this menu
    // Meaning that this menu is literally pointless to put in exmaples but it's just to show
    public override List<BaseSetting> Settings(ReferenceHub hub) => [];

    public override bool CheckAccess(ReferenceHub hub) => false;

    public override string Name { get; set; } = "Access Menu";
    public override int Id { get; set; } = -243;
}
