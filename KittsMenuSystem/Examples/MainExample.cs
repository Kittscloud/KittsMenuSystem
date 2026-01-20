using KittsMenuSystem.Features.Menus;
using KittsMenuSystem.Features.Wrappers;
using System.Collections.Generic;

namespace KittsMenuSystem.Examples;

internal class MainExample : Menu
{
    public override List<BaseSetting> Settings(ReferenceHub hub) => [
        new TextArea("Hello! Welcome to the exmaples.\nDemo Exmaple - Contains all settings you can use\nTextArea Exmaple - Shows you how you can use text areas\n" +
            "Utitly Exmaple - This contains a whole bunch of exmaples of how you can actualy utilize things this framework has to offer, but please " +
            "do remember, this is not all you can do with this framework, honestly, the possibilities is almost unlimited, only limited to what the framework " +
            "has to offer, by the way, it has full access to making your own menu so it's got quite a lot\n\n" +
            "IMPORTANT NOTE: This plugin was inspired and mainly based off another outdated plugin by a person named skyfr0676, " +
            "this is the github repo: <link=\"https://github.com/skyfr0676/SSMenuSystem\"><u><b>https://github.com/skyfr0676/SSMenuSystem</b></u></link>, " +
            "this person has really made something great, but has abandoned the plugin, this is where I have came in, I've taken it over and updated it tremendously.")
    ];

    public override string Name { get; set; } = "Main Exmaple Menu";
    public override int Id { get; set; } = -2427;
}