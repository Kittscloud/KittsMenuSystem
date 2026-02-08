using HarmonyLib;
using KittsMenuSystem.Features;
using KittsMenuSystem.Features.Menus;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using System;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem;

public class KittsMenuSystem : Plugin
{
    public static Plugin Instance { get; private set; }

    public override string Name { get; } = "KittsMenuSystem";
    public override string Author { get; } = "Kittscloud";
    public override string Description { get; } = "";
    public override LoadPriority Priority { get; } = LoadPriority.Lowest;

    public override Version Version { get; } = new Version(0, 4, 3);
    public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);

    public static Config Config { get; set; }
    private bool _errorLoadingConfig = false;

    private Harmony _harmony;
    private MenuEvents _menuEvents;

    public override void Enable()
    {
        Instance = this;

        if (_errorLoadingConfig)
            Log.Error("There was an error loading the config files, please check them or generate new ones");

        if (!Config.IsEnabled)
            return;

        MenuManager.RegisterAllMenus();

        Instance = this;

        _harmony = new Harmony("fr.kittscloud.patches");
        _harmony.PatchAll();

        _menuEvents = new();
        CustomHandlersManager.RegisterEventsHandler(_menuEvents);

        ServerSpecificSettingsSync.ServerOnSettingValueReceived += MenuEvents.OnSettingReceived;
        ServerSpecificSettingsSync.ServerOnStatusReceived += MenuEvents.OnStatusReceived;

        MenuManager.RegisterQueuedAssemblies();

        Log.Info($"Successfully Enabled {Name}@{Version}");
    }

    public override void Disable()
    {
        this.SaveConfig(Config, "config.yml");

        MenuManager.UnregisterAllMenus();

        CustomHandlersManager.UnregisterEventsHandler(_menuEvents);

        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= MenuEvents.OnSettingReceived;
        ServerSpecificSettingsSync.ServerOnStatusReceived -= MenuEvents.OnStatusReceived;

        Instance = null;

        _harmony.UnpatchAll();
        _harmony = null;

        _menuEvents = null;

        Log.Info($"Successfully Disabled {Name}@{Version}");
    }

    public override void LoadConfigs()
    {
        _errorLoadingConfig = !this.TryLoadConfig("config.yml", out Config config);
        Config = config ?? new Config();

        base.LoadConfigs();
    }
}
