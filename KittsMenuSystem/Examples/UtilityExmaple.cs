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

    private readonly List<BaseSetting> _addedSettings = [];
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
    private readonly List<LightSourceToy> _spawnedToys = [];

    public override List<BaseSetting> Settings(ReferenceHub hub)
    {
        List<BaseSetting> settings = [];

        settings = [
            // It's always good to have your own button at the top of all menus to reload menus
            // As this will update things that don't get auto updated, such as text areas
            new Button("Reload Menu", "Reload", (h, _) => ReloadFor(h)),

            //new GroupHeader("Abilities"),
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

            // Settings do not need IDs, you only need IDs if you are trying to use GetSetting
            // However two settings without Ids cannot be of the same type and have the same label
            // NOTE: Setting IDs makes it easier when debugging, but again, aren't needed
            new GroupHeader("Death"),
            new Button("Kill Yourself", "Click Me", (h, _) => hub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown))),
            new Button("Kill Yourself with Hold Time", "Hold Me", (h, _) => h.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown)), holdTimeSeconds: 1f),
            new Keybind("Kill Yourself Keybind", (h, isPressed, _) => { if (isPressed) h.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown)); }),
            
            // You can also use the hub parameter from the GetSetting function to display specific things to that hub, or have defaults
            // For exmaple, you can put the players name as a placeholder so its not empty when first going in the menu
            // The reason default values are funciton is becuase the default value needs to be set after settings have been built
            // This is to avoid any errors if you try to get a setting or value for the default that hasn't actually been built yet
            new GroupHeader("Name Change"),
            new TextBox("Name", (h, newName, _) => h.nicknameSync.DisplayName = newName, hub.nicknameSync.DisplayName),

            // You can do anything with this hub, display it's information, get the Player object, anything
            new GroupHeader("About Hub"),
            new TextArea($"Display Name: {hub.nicknameSync.DisplayName}\nNetId: {hub.netId}\nPlayerId: {hub.PlayerId}\nRole Name: {hub.roleManager.CurrentRole.RoleName}\nAnd so on"),

            // This section shows you how you can use all sorts of settings and features together to make one whole feature of your own
            new GroupHeader("Spawning Light Source"),
            new Slider(2, "Intensity", 0, 100, (hub, _, _) => ReloadColorInfoForUser(hub), 1, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
            new Slider(3, "Range", 0, 100, null, 10, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
            new Dropdown(4, "Color", [.. _presets.Select(x => x.Name)], (hub, _, _) => ReloadColorInfoForUser(hub)),
            _selectedColorTextArea,
            new Dropdown(6, "Shadows Type", [.. _shadowsType.Select(x => x.ToString())]),
            new Slider(7, "Shadow Strength", 0, 100),
            new Dropdown(8, "Light Type", [.. _lightType.Select(x => x.ToString())]),
            new Button("Confirm Spawning", "Spawn", (hub, _) => Spawn(hub))
        ];

        // Make sure to always rebuild settings if you add them somewhere else
        _addedSettings.Clear();

        if (_spawnedToys.Count != 0)
        {
            _addedSettings.Add(new GroupHeader("Spawned Lights"));
            _addedSettings.Add(new Button(9, "All Lights", "Destroy All (HOLD)", (_, _) => DestroyAll(), 2f));

            foreach (LightSourceToy toy in _spawnedToys)
            {
                int id = (int)toy.Base.netId;
                _addedSettings.Add(new Button(id, $"Light #{id}", "Destroy (HOLD)", (_, s) => Destroy(s.SettingId), 0.4f));
            }
        }

        // You can also add another list that can be updated whenever, such as when adding destroy buttons for spawning lights
        settings.AddRange(_addedSettings);

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

        _spawnedToys.Add(toy);

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

    private void DestroyAll()
    {
        foreach (LightSourceToy toy in _spawnedToys.ToList())
            NetworkServer.Destroy(toy.GameObject);

        _spawnedToys.Clear();
        MenuManager.ReloadAll();
    }

    private void Destroy(int netId)
    {
        LightSourceToy toy = _spawnedToys.FirstOrDefault(t => t.Base.netId == netId);
        if (toy == null)
            return;

        _spawnedToys.Remove(toy);
        NetworkServer.Destroy(toy.GameObject);

        MenuManager.ReloadAll();
    }

    // You can also check access of hub, if they don't meet conditions, menu will not appear for them
    public override bool CheckAccess(ReferenceHub hub) => true; // Some condition

    public override string Name { get; set; } = "Utility Exmaple";
    public override int Id { get; set; } = -8;
    public override Type ParentMenu { get; set; } = typeof(MainExample);
}