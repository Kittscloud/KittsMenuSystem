using HarmonyLib;
using KittsMenuSystem.Features;
using KittsMenuSystem.Features.Wrappers;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using System;
using UserSettings.ServerSpecific;
using EventHandler = KittsMenuSystem.Features.EventHandler;

namespace KittsMenuSystem;

public class KittsMenuSystem : Plugin
{
    public static Plugin Instance { get; private set; }

    public override string Name { get; } = "KittsMenuSystem";
    public override string Author { get; } = "Kittscloud";
    public override string Description { get; } = "Kitts Menu System";
    public override LoadPriority Priority { get; } = LoadPriority.Lowest;

    public override Version Version { get; } = new Version(0, 1, 0);
    public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);

    public static Config Config { get; set; }
    private bool _errorLoadingConfig = false;

    private Harmony _harmony;
    private EventHandler _handler;

    public override void Enable()
    {
        Instance = this;

        if (_errorLoadingConfig)
            Log.Error("There was an error loading the config files, please check them or generate new ones");

        if (!Config.IsEnabled)
            return;

        MenuManager.RegisterAll();

        Instance = this;

        _harmony = new Harmony("fr.kittscloud.patches");
        _harmony.PatchAll();

        _handler = new EventHandler();
        CustomHandlersManager.RegisterEventsHandler(_handler);

        MenuManager.RegisterQueuedAssemblies();

        ServerSpecificSettingsSync.ServerOnSettingValueReceived += EventHandler.OnSettingReceived;

        Log.Info($"Successfully Enabled {Name}@{Version}");
    }

    public override void Disable()
    {
        this.SaveConfig(Config, "config.yml");

        MenuManager.UnregisterAll();

        CustomHandlersManager.UnregisterEventsHandler(_handler);

        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= EventHandler.OnSettingReceived;

        Instance = null;

        _harmony.UnpatchAll();
        _harmony = null;

        _handler = null;

        Log.Info($"Successfully Disabled {Name}@{Version}");
    }

    /// <inheritdoc/>
    public override void LoadConfigs()
    {
        _errorLoadingConfig = !this.TryLoadConfig("config.yml", out Config config);
        Config = config ?? new Config();

        base.LoadConfigs();
    }
}
