using CustomPlayerEffects;
using GameCore;
using InventorySystem;
using InventorySystem.Items;
using KittsMenuSystem.Features;
using KittsMenuSystem.Features.Wrappers;
using LabApi.Features.Wrappers;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UserSettings.ServerSpecific;
using UserSettings.ServerSpecific.Examples;
using static UserSettings.ServerSpecific.Examples.SSLightSpawnerExample;

namespace KittsMenuSystem.Examples;

internal class SpecificUtilityExmaple : Menu
{
    // You don't have to use just settings, there are cases where you want the hub of the player using the menu
    // The hub can be used for many things, default values, listing info about the hub, using the hub with your own function
    // GetSettingsFor is probably the preferred and better way to put settings on a menu
    // This is due to the fact that GetSettingsFor will append the settings after the usual Settings override used in other menus
    public override ServerSpecificSettingBase[] GetSettingsFor(ReferenceHub hub)
    {
        List<ServerSpecificSettingBase> _settings = [];

        _settings = [
            // It's always good to have your own button at the top of all menus to reload menus
            // As this will update things that don't get auto updated, such as text areas
            new Button("Reload Menu", "Reload", (h, _) => ReloadFor(h)),
            
            // For exmaple, you can put the players name as a placeholder so its not empty when first going in the menu
            new GroupHeader("Name Change with Default"),
            new Plaintext("Name", (hub, newName, _) => hub.nicknameSync.DisplayName = newName, hub.nicknameSync.DisplayName),

            // You can do anything wiht this hub, display it's information, get the Player object, anything
            new GroupHeader("About Hub"),
            new TextArea($"Display Name: {hub.nicknameSync.DisplayName}\nNetId: {hub.netId}\nPlayerId: {hub.PlayerId}\nRole Name: {hub.roleManager.CurrentRole.RoleName}\nAnd so on")
        ];

        return [.. _settings.Where(s => s != null)];
    }

    // You can also check access of hub, if they don't meet conditions, menu will not appear for them
    public override bool CheckAccess(ReferenceHub hub) => true; // Some condition

    public override string Name { get; set; } = "Specific Utility Exmaple";
    public override int Id { get; set; } = -6453;
    public override Type MenuRelated { get; set; } = typeof(MainExample);
}