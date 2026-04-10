using CustomPlayerEffects;
using KittsMenuSystem.Features.Menus;
using KittsMenuSystem.Features.Settings;
using LabApi.Features.Wrappers;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UserSettings.ServerSpecific;
using static UserSettings.ServerSpecific.Examples.SSLightSpawnerExample;

namespace KittsMenuSystem.Examples;

internal class UtilityExmaple : Menu
{
    private static readonly HashSet<ReferenceHub> _activeSpeedBoosts = [];

    private readonly List<ColorPreset> _presets = [
        new("White", Color.white),
        new("Black", Color.black),
        new("Gray", Color.gray),
        new("Red", Color.red),
        new("Green", Color.green),
        new("Blue", Color.blue),
        new("Yellow", Color.yellow),
        new("Cyan", Color.cyan),
        new("Magenta", Color.magenta),
    ];
    private readonly LightShadows[] _shadowsType = EnumUtils<LightShadows>.Values;
    private readonly LightType[] _lightType = EnumUtils<LightType>.Values;
    private readonly TextArea _selectedColorTextArea = new(5, "Selected Color: None");
    private readonly List<LightSourceToy> _spawnedLightSources = [];

    public override List<BaseSetting> Settings(ReferenceHub hub)
    {
        List<BaseSetting> settings = [];

        settings = [
            // It's always good to have your own button at the top of all menus to reload menus
            // As this will update things that don't get auto updated, such as text areas
            new Button("Reload Menu", "Reload", (h, _) => ReloadFor(h)),

            new GroupHeader("Abilities"),
            new Keybind("Speed Boost (Human-only)", (h, isPressed, _) =>
            {
                bool toggleMode = h.GetSetting<UtilityExmaple, SSTwoButtonsSetting>(1).SyncIsB;

                if (toggleMode)
                {
                    if (!isPressed) return;
                    SetSpeedBoost(h, !_activeSpeedBoosts.Contains(h));
                }
                else
                    SetSpeedBoost(h, isPressed);
            }, KeyCode.Y, hint: "Increase your speed by draining your health."),
            new ABButton(1, "Speed Boost - Activation Mode", "Hold", "Toggle"),

            // Settings do not need to set the SettingId, you only need SettingIds if you are trying to use GetSetting or TryGetSetting
            // If you do not set the SettingId then a random integer will be used as the SettingId
            // NOTE: SettingId makes it easier when debugging, but again, aren't needed
            new GroupHeader("Death"),
            new Button("Kill Yourself", "Click Me", (h, _) => h.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown))),
            new Button("Kill Yourself with Hold Time", "Hold Me", (h, _) => h.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown)), holdTimeSeconds: 1f),
            new Keybind("Kill Yourself Keybind", (h, isPressed, _) => { if (isPressed) h.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown)); }),
            
            // You can also use the hub parameter to display specific things to that hub, or have defaults
            // For exmaple, you can put the players display name as a placeholder so its not empty when first going into the menu
            new GroupHeader("Name Change"),
            new TextBox("Name", (h, newName, _) => h.nicknameSync.DisplayName = newName, hub.nicknameSync.DisplayName),

            // You can do anything with this hub, display it's information, get the Player object, anything
            new GroupHeader("About Hub"),
            new TextArea($"Display Name: {hub.nicknameSync.DisplayName}\nNetId: {hub.netId}\nPlayerId: {hub.PlayerId}\nRole Name: {hub.roleManager.CurrentRole.RoleName}\nAnd so on"),

            // This section shows how you can use the overrideVersion to notify the player that something has change in the menu
            new GroupHeader("Override Version"),
            new Button("Increase Version", "Increase", (h, _) => ReloadFor(h, ServerSpecificSettingsSync.Version + 1)),
            new Keybind("Increase Version", onPressed: (h, _) => ReloadFor(h, ServerSpecificSettingsSync.Version + 1)),

            // This section is to show how the AddedSettings work
            // Each time this button is pressed, it adds a new setting to the DemoExample menu through AddedSettings.
            // AddedSetting is used when you want to add a setting to a menu from outside that menu class, as seen below
            new GroupHeader("AddedSettings"),
            new Button("Add Setting to DemoExmaple Menu", "Add Setting", (_, _) => {
                Menu demoExampleMenu = MenuManager.GetMenu(typeof(DemoExample)); // Make sure we get use MenuManager.GetMenu
                demoExampleMenu.AddedSettings.Add(new(null, new TextArea("This was added from the UtilityExample menu")));
            }),
            
            // If you modify AddedSettings in a menu, you must reload the menu for changes to appear.
            // You can also insert a setting after a specific setting ID by setting TargetId.
            // If TargetId is null, the setting is added to the end (as shown above).
            new Button("Add etting Below Speed Boost AB Button in this Menu", "Add Setting", (h, _) => {
                AddedSettings.Add(new(1, new TextArea("This was added from this menu")));
                ReloadFor(h); // Must do to update menu
            }),

            // This section shows you how you can use all sorts of settings and features together to make one whole feature of your own
            new GroupHeader("Spawning Light Source"),
            new Slider(2, "Intensity", 0, 100, (h, _, _) => ReloadColorInfoForUser(h), 1, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
            new Slider(3, "Range", 0, 100, null, 10, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
            new Dropdown(4, "Color", [.. _presets.Select(x => x.Name)], (h, _, _) => ReloadColorInfoForUser(h)),
            _selectedColorTextArea,
            new Dropdown(6, "Shadows Type", [.. _shadowsType.Select(x => x.ToString())]),
            new Slider(7, "Shadow Strength", 0, 100),
            new Dropdown(8, "Light Type", [.. _lightType.Select(x => x.ToString())]),
            new Button("Confirm Spawning", "Spawn", (h, _) => Spawn(h))

        ];

        // This part is used to delete spawned lights
        // This setup might be a little confusing, basically we set the settingId of the delete button to the netId of the toy
        // Then in the destroy function we delete the toy using the setting id which will be the netId toy
        // One this to be aware of, if the netId (or whatever you are using) collides with another Id the menu will break
        // This is why we add 1_000_000 to the number to make sure we get no conflicts we just need to make sure we minus 1_000_000 when deleting
        if (_spawnedLightSources.Count != 0)
        {
            settings.Add(new GroupHeader("Spawned Lights"));
            settings.Add(new Button("All Lights", "Destroy All (HOLD)", (_, _) => DestroyAll(), 2f));

            foreach (LightSourceToy toy in _spawnedLightSources)
            {
                int id = 1_000_000 + (int)toy.Base.netId;
                settings.Add(new Button(id, $"Light #{id}", "Destroy (HOLD)", (_, s) => Destroy(s.SettingId), 0.4f));
            }
        }

        return [.. settings.Where(s => s != null)];
    }

    private void SetSpeedBoost(ReferenceHub hub, bool enable)
    {
        if (!hub.IsHuman())
            return;

        if (enable)
        {
            hub.playerEffectsController.EnableEffect<Scp207>();
            _activeSpeedBoosts.Add(hub);
        }
        else
        {
            hub.playerEffectsController.DisableEffect<Scp207>();
            _activeSpeedBoosts.Remove(hub);
        }
    }

    private void ReloadColorInfoForUser(ReferenceHub hub) => (_selectedColorTextArea.Base as SSTextArea).SendTextUpdate(GetColorInfoForUser(hub), false, receiveFilter: (h) => h == hub);
    public string GetColorInfoForUser(ReferenceHub hub) => "Selected color: <color=" + GetColorInfo(hub).ToHex() + ">███████████</color>";
    private Color GetColorInfo(ReferenceHub hub) => _presets[hub.GetSetting<UtilityExmaple, SSDropdownSetting>(4).SyncSelectionIndexRaw].Color;

    private void Spawn(ReferenceHub hub)
    {
        LightSourceToy toy = LightSourceToy.Create();
        if (toy == null)
            return;

        toy.Intensity = hub.GetSetting<UtilityExmaple, SSSliderSetting>(2).SyncFloatValue;
        toy.Range = hub.GetSetting<UtilityExmaple, SSSliderSetting>(3).SyncFloatValue;
        toy.Color = GetColorInfo(hub);
        toy.ShadowType = _shadowsType[hub.GetSetting<UtilityExmaple, SSDropdownSetting>(6).SyncSelectionIndexRaw];
        toy.ShadowStrength = hub.GetSetting<UtilityExmaple, SSSliderSetting>(7).SyncFloatValue;
        toy.Type = _lightType[hub.GetSetting<UtilityExmaple, SSDropdownSetting>(8).SyncSelectionIndexRaw];
        toy.Transform.position = hub.transform.position;

        _spawnedLightSources.Add(toy);

        MenuManager.ReloadAll();
    }

    private void DestroyAll()
    {
        foreach (LightSourceToy toy in _spawnedLightSources.ToList())
            NetworkServer.Destroy(toy.GameObject);

        _spawnedLightSources.Clear();
        MenuManager.ReloadAll();
    }

    private void Destroy(int netId)
    {
        LightSourceToy toy = _spawnedLightSources.FirstOrDefault(t => t.Base.netId == netId - 1_000_000);
        if (toy == null)
            return;

        _spawnedLightSources.Remove(toy);
        NetworkServer.Destroy(toy.GameObject);

        MenuManager.ReloadAll();
    }

    // Exmaple of connecting events or doing some other code when the menu is registered
    protected internal override void OnRegistered()
    {
        ReferenceHub.OnPlayerRemoved += OnDisconnect;
        PlayerRoleManager.OnRoleChanged += OnRoleChanged;
    }

    private static void OnDisconnect(ReferenceHub hub) => _activeSpeedBoosts.Remove(hub);

    private void OnRoleChanged(ReferenceHub userHub, PlayerRoleBase prevRole, PlayerRoleBase newRole)
    {
        if (!userHub.IsHuman())
            SetSpeedBoost(userHub, false);
    }

    // You can also check access of hub, if they don't meet conditions, menu will not appear for them
    public override bool CheckAccess(ReferenceHub hub) => true; // Some condition

    public override string Name { get; } = "Utility Exmaple";
    public override int Id { get; } = -5;
    public override Type ParentMenu { get; set; } = typeof(MainExample);
}