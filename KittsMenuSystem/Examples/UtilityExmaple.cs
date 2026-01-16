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

internal class UtilityExmaple : Menu
{
    private static readonly HashSet<ReferenceHub> _activeSpeedBoosts = [];

    private readonly List<ServerSpecificSettingBase> _addedSettings = [];
    private List<ColorPreset> _presets;
    private LightShadows[] _shadowsType;
    private LightType[] _lightType;
    private SSTextArea _selectedColorTextArea;
    private readonly List<LightSourceToy> _spawnedToys = [];

    public override ServerSpecificSettingBase[] Settings => GetSettings();

    public ServerSpecificSettingBase[] GetSettings()
    {
        List<ServerSpecificSettingBase> _settings = [];

        // Init Light Spawner Lists
        _presets ??=
        [
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
        _shadowsType ??= EnumUtils<LightShadows>.Values;
        _lightType ??= EnumUtils<LightType>.Values;
        _selectedColorTextArea ??= new SSTextArea(5, "Selected Color: None");

        _settings = [
            new GroupHeader("Abilities"),
            // Note that for a keybind to work and be registered, the player must open the menu that the keybind is on
            new Keybind("Speed Boost (Human-only)", (hub, isPressed, _) =>
            {
                bool toggleMode = hub.GetParameter<UtilityExmaple, SSTwoButtonsSetting>(1).SyncIsB;

                if (toggleMode)
                {
                    if (!isPressed) return;
                    SetSpeedBoost(hub, !_activeSpeedBoosts.Contains(hub));
                }
                else
                    SetSpeedBoost(hub, isPressed);
            }, KeyCode.Y, hint: "Increase your speed by draining your health."),
            new YesNoButton(1, "Speed Boost - Activation Mode", "Hold", "Toggle"),

            // Settings do not have to have IDs, you only need IDs if you are trying to use GetParameter
            new GroupHeader("Death"),
            new Button("Kill Yourself", "Click Me", (hub, _) => hub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown))),
            new Button("Kill Yourself with Hold Time", "Hold Me", (hub, _) => hub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown)), holdTimeSeconds: 1f),
            new Keybind("Kill Yourself Keybind", (hub, isPressed, _) => { if (isPressed) hub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown)); }),
            
            new GroupHeader("Name Change"),
            new Plaintext("Name", (hub, newName, _) => {
                Player.Dictionary.TryGetValue(hub, out Player value);
                value.DisplayName = newName;
            }),

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

        // You can also add another list that can be updated whenever, such as when adding destroy buttons for spawning lights
        _settings.AddRange(_addedSettings);

        return [.. _settings.Where(s => s != null)];
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

    private void ReloadColorInfoForUser(ReferenceHub hub) => _selectedColorTextArea.SendTextUpdate(GetColorInfoForUser(hub), receiveFilter: (h) => h == hub);
    public string GetColorInfoForUser(ReferenceHub hub) => "Selected color: <color=" + this.GetColorInfo(hub).ToHex() + ">███████████</color>";
    private Color GetColorInfo(ReferenceHub hub) => _presets[hub.GetParameter<UtilityExmaple, SSDropdownSetting>(4).SyncSelectionIndexRaw].Color;

    private void Spawn(ReferenceHub hub)
    {
        LightSourceToy toy = LightSourceToy.Create();
        if (toy == null)
            return;

        toy.Intensity = hub.GetParameter<UtilityExmaple, SSSliderSetting>(2).SyncFloatValue;
        toy.Range = hub.GetParameter<UtilityExmaple, SSSliderSetting>(3).SyncFloatValue;
        toy.Color = GetColorInfo(hub);
        toy.ShadowType = _shadowsType[hub.GetParameter<UtilityExmaple, SSDropdownSetting>(6).SyncSelectionIndexRaw];
        toy.ShadowStrength = hub.GetParameter<UtilityExmaple, SSSliderSetting>(7).SyncFloatValue;
        toy.Type = _lightType[hub.GetParameter<UtilityExmaple, SSDropdownSetting>(8).SyncSelectionIndexRaw];
        toy.Transform.position = hub.transform.position;

        _spawnedToys.Add(toy);

        RebuildDestroyButtons();

        MenuManager.ReloadAll();
    }

    private void RebuildDestroyButtons()
    {
        _addedSettings.Clear();

        if (_spawnedToys.Count == 0)
            return;

        _addedSettings.Add(new GroupHeader("Spawned Lights"));
        _addedSettings.Add(new Button(9, "All Lights", "Destroy All (HOLD)", null, 2f));

        foreach (LightSourceToy toy in _spawnedToys)
        {
            int id = (int)toy.Base.netId;
            _addedSettings.Add(new Button(id, $"Light #{id}", "Destroy (HOLD)", null, 0.4f));
        }
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

    // You also don't have to use the Actions in the Settings, you can use this override and check if IDs match and then run code, as seen below
    public override void OnInput(ReferenceHub hub, ServerSpecificSettingBase setting)
    {
        if (setting.SettingId > 10)
            Destroy(setting.SettingId);
        if (setting.SettingId == 9)
            DestroyAll();

        base.OnInput(hub, setting);
    }

    private void DestroyAll()
    {
        foreach (LightSourceToy toy in _spawnedToys.ToList())
            NetworkServer.Destroy(toy.GameObject);

        _spawnedToys.Clear();
        RebuildDestroyButtons();
        MenuManager.ReloadAll();
    }

    private void Destroy(int netId)
    {
        LightSourceToy toy = _spawnedToys.FirstOrDefault(t => t.Base.netId == netId);
        if (toy == null)
            return;

        _spawnedToys.Remove(toy);
        NetworkServer.Destroy(toy.GameObject);

        RebuildDestroyButtons();
        MenuManager.ReloadAll();
    }

    // You can also check access of hub, if they don't meet conditions, menu will not appear for them
    public override bool CheckAccess(ReferenceHub hub) => true; // Some condition

    public override string Name { get; set; } = "Utility Exmaple";
    public override int Id { get; set; } = -8;
    public override Type MenuRelated { get; set; } = typeof(MainExample);
}