using System;
using KittsMenuSystem.Features;
using KittsMenuSystem.Features.Wrappers;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Examples;

internal class DemoExample : Menu
{
    private static readonly string[] _options =
    [
        "Option 1",
        "Option 2",
        "Option 3",
        "Option 4"
    ];

    public override ServerSpecificSettingBase[] Settings =>
    [
        new GroupHeader("GroupHeader"),
        new YesNoButton("TwoButtonsSetting", "Option A", "Option B"),
        new TextArea("TextArea"),
        new TextArea("Multiline collapsable TextArea.\nThis is another line\nAnd another.", TextArea.FoldoutMode.ExtendedByDefault),
        new Slider( "SliderSetting", 0.0f, 1f),
        new Plaintext("Plaintext"),
        new Keybind("KeybindSetting"),
        new Dropdown("DropdownSetting", _options),
        new Dropdown("Scrollable DropdownSetting", _options, entryType: Dropdown.DropdownEntryType.Scrollable),
        new Button("Button", "Press me!", (_, _) => { }),
        new GroupHeader("Hints", hint: "Group headers are used to separate settings into subcategories."),
        new YesNoButton("Another TwoButtonsSetting", "Option A", "Option B", hint: "Two Buttons are used to store Boolean values."),
        new Slider("Another SliderSetting", 0.0f, 1f, hint: "Sliders store a numeric value within a defined range."),
        new Plaintext("Another Plaintext", hint: "Plaintext fields store any provided text."),
        new Keybind("Another KeybindSetting", hint: "Allows checking if the player is currently holding the action key."),
        new Dropdown("Another DropdownSetting", _options, hint: "Stores an integer value between 0 and the length of options minus 1."),
        new Dropdown("Another Scrollable DropdownSetting", _options, entryType: Dropdown.DropdownEntryType.Scrollable),
        new Button("Another Button", "Press me! (again)", (_, _) => { }, hint: "Triggers an event whenever it is pressed.")
    ];

    public override string Name { get; set; } = "Demo Example";
    public override int Id { get; set; } = -6;
    public override Type MenuRelated { get; set; } = typeof(MainExample);
}