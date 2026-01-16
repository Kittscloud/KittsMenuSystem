using System;
using System.Collections.Generic;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using KittsMenuSystem.Features;
using KittsMenuSystem.Features.Wrappers;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Examples;

internal class TextAreaExample : Menu
{
    // Exmaple to show what you can do with TextAreas, pretty much anything
    public override ServerSpecificSettingBase[] Settings =>
    [
        new GroupHeader("Different Text Area Types"),
        new TextArea("<color=#00FFFF>This</color> <size=30>text</size> <color=red>area</color> <u>supports</u> <i>Rich</i> <b>Text</b> <rotate=\"25\">Tags</rotate>."),
        new TextArea("This is another multi-line text area, but this one features auto-generated preview text when collapsed, with ellipses appearing when the text no longer fits. It also has an option enabled to collapse automatically when you switch off this settings tab. In other words, you will need to re-expand this text area each time you visit this tab.", SSTextArea.FoldoutMode.CollapseOnEntry),
        new TextArea("This multi-line text area is expanded by default but can be collapsed if needed. It will retain its previous state when toggling this tab on and off.", SSTextArea.FoldoutMode.ExtendedByDefault),
        new TextArea("This multi-line text area is similar to the one above, but it will re-expand itself after collapsing each time you visit this tab.", SSTextArea.FoldoutMode.ExtendOnEntry),
        new TextArea("This multi-line text area cannot be collapsed.\nIt remains fully expanded at all times, but supports URL links.\nExample link: <link=\"https://www.youtube.com/watch?v=dQw4w9WgXcQ\"><mark=#5865f215>[Click]</mark></link>"),
    ];

    public override string Name { get; set; } = "Text Area Example";
    public override int Id { get; set; } = -7;
    public override Type MenuRelated { get; set; } = typeof(MainExample);
}