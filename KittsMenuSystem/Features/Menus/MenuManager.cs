using KittsMenuSystem.Examples;
using KittsMenuSystem.Features.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Menus;

public static class MenuManager
{
    private readonly static Dictionary<ReferenceHub, Menu> _syncedMenus = [];

    /// <summary>
    /// Contains every <see cref="ReferenceHub"/> with their loaded <see cref="Menu"/>.
    /// </summary>
    public static IReadOnlyDictionary<ReferenceHub, Menu> SyncedMenus => _syncedMenus;

    #region Registering
    private readonly static List<Menu> _registeredMenus = [];
    private readonly static Dictionary<Assembly, List<BaseSetting>> _pinnedTopSettings = [];
    private readonly static Dictionary<Assembly, List<BaseSetting>> _pinnedBottomSettings = [];

    /// <summary>
    /// Contains all loaded <see cref="Menu"/>s.
    /// </summary>
    public static IReadOnlyList<Menu> RegisteredMenus => _registeredMenus;

    /// <summary>
    /// Contains <see cref="Assembly"/> with their top pinned settings.
    /// </summary>
    public static IReadOnlyDictionary<Assembly, List<BaseSetting>> PinnedTopSettings => _pinnedTopSettings;

    /// <summary>
    /// Contains <see cref="Assembly"/> with their top pinned settings.
    /// </summary>
    public static IReadOnlyDictionary<Assembly, List<BaseSetting>> PinnedBottomSettings => _pinnedBottomSettings;

    private static readonly Queue<Assembly> _waitingAssemblies = new();

    /// <summary>
    /// Register waiting assemblies when plugin is loaded.
    /// </summary>
    internal static void RegisterQueuedAssemblies()
    {
        while (_waitingAssemblies.TryDequeue(out Assembly assembly))
            assembly.Register();
    }

    /// <summary>
    /// Register menus in <see cref="Assembly.GetCallingAssembly"/>.
    /// </summary>
    public static void RegisterAllMenus() => Assembly.GetCallingAssembly().Register();

    /// <summary>
    /// Register all menus in target <see cref="Assembly"/>.
    /// </summary>
    /// <param name="assembly">Target <see cref="Assembly"/>.</param>
    private static void Register(this Assembly assembly)
    {
        if (KittsMenuSystem.Config == null)
        {
            if (!_waitingAssemblies.Contains(assembly))
                _waitingAssemblies.Enqueue(assembly);

            return;
        }

        try
        {
            Log.Debug("MenuManager.Register", $"Loading assembly {assembly.GetName().Name}..");

            List<Menu> allMenus = [.. assembly.GetTypes()
                .Where(t => t.BaseType == typeof(Menu) &&
                    !t.IsAbstract &&
                    !t.IsInterface &&
                    t != typeof(CentralMainMenu) &&
                    t != typeof(GlobalMenu) &&
                    (t != typeof(MainExample) || KittsMenuSystem.Config.EnableExamples))
                .Select(t => Activator.CreateInstance(t) as Menu)];

            IEnumerable<Menu> orderedMenus = allMenus.OrderBy(m => m.ParentMenu == null ? 0 : 1).ThenBy(m => m.Id);
            List<Menu> _registeredMenus = [];

            foreach (Menu menu in orderedMenus)
                try
                {
                    menu.Register();
                    _registeredMenus.Add(menu);
                }
                catch (Exception e)
                {
                    Log.Error("MenuManager.Register", $"Error loading menu {menu.Name}: {e.Message}");
                    Log.Debug("MenuManager.Register", e.ToString());
                }

            Log.Info("MenuManager.Register", $"Loaded assembly {assembly.GetName().Name}: {_registeredMenus.Count}/{allMenus.Count} menus registered");
        }
        catch (Exception e)
        {
            Log.Error("MenuManager.Register", $"Failed to load assembly {assembly.GetName().Name}: {e.Message}");
            Log.Debug("MenuManager.Register", e.ToString());
        }
    }

    /// <summary>
    /// Register a <see cref="Menu"/>.
    /// </summary>
    /// <param name="menu"><see cref="Menu"/> to register.</param>
    /// <exception cref="ArgumentException">Thrown if menu is invalid or has duplicate IDs.</exception>
    public static void Register(this Menu menu)
    {
        try
        {
            if (menu == null || (menu.ParentMenu == typeof(MainExample) && !KittsMenuSystem.Config.EnableExamples))
                return;

            Log.Debug("MenuManager.Register", $"Loading menu {menu.Name}..");

            if (menu.CheckSameId())
                throw new ArgumentException($"Menu ID {menu.Id} already registered");
            if (menu.Id == 0)
                throw new ArgumentException("Menu ID cannot be 0 (reserved for Main Menu)");
            if (menu.Id == 1)
                throw new ArgumentException("Menu ID cannot be 1 (reserved for Global Menu)");
            if (string.IsNullOrEmpty(menu.Name))
                throw new ArgumentException("Menu name cannot be empty");
            if (_registeredMenus.Any(m => m.Name == menu.Name))
                throw new ArgumentException($"Duplicate menu name '{menu.Name}'");

            if (menu.ParentMenu != null && !_registeredMenus.Any(m => m.GetType() == menu.ParentMenu))
                throw new ArgumentException($"Menu {menu.Name} has invalid related menu {menu.ParentMenu.FullName}");

            _registeredMenus.Add(menu);
            menu.OnRegistered();
            Log.Debug("MenuManager.Register", $"Menu {menu.Name} registered successfully");
        }
        catch (Exception e)
        {
            Log.Error("MenuManager.Register", $"Failed to load menu {menu.Name}: {e.Message}");
            Log.Debug("MenuManager.Register", e.ToString());
        }
    }

    private static bool CheckSameId(this Menu menu)
    {
        if (menu.ParentMenu == null)
            return _registeredMenus.Any(x => x.Id == menu.Id && menu.ParentMenu == null);

        return _registeredMenus.Where(x => x.ParentMenu == menu.ParentMenu).Any(x => x.Id == menu.Id);
    }

    /// <summary>
    /// Unregister menu.
    /// </summary>
    /// <param name="menu">The menu.</param>
    public static void Unregister(this Menu menu)
    {
        if (_registeredMenus.Contains(menu))
            _registeredMenus.Remove(menu);

        foreach (KeyValuePair<ReferenceHub, Menu> sync in _syncedMenus)
            if (sync.Value == menu)
                sync.Key.LoadMenu(null);
    }

    /// <summary>
    /// Unregister all menus.
    /// </summary>
    public static void UnregisterAllMenus()
    {
        foreach (KeyValuePair<ReferenceHub, Menu> sync in _syncedMenus)
            sync.Key.LoadMenu(null);

        foreach (Menu menu in _registeredMenus)
            menu.Unregister();

        _registeredMenus.Clear();
    }

    /// <summary>
    /// Used when player has left server.
    /// </summary>
    /// <param name="hub">The target <see cref="ReferenceHub"/>.</param>
    internal static void DeleteFromSyncedMenus(this ReferenceHub hub)
    {
        hub.GetCurrentMenu()?.OnClose(hub);
        _syncedMenus.Remove(hub);
    }

    /// <summary>
    /// Register list of <see cref="BaseSetting"/>s displayed on the top of all menus.
    /// </summary>
    /// <param name="toPin">The list of <see cref="ServerSpecificSettingBase"/> to pin.</param>
    public static void RegisterTopPinnedSettings(this List<BaseSetting> toPin) => _pinnedTopSettings[Assembly.GetCallingAssembly()] = toPin;

    /// <summary>
    /// Remove top pinnedsettings  from <see cref="Assembly.GetCallingAssembly"/>.
    /// </summary>
    public static void UnregisterTopPinnedSettings() => _pinnedTopSettings.Remove(Assembly.GetCallingAssembly());

    /// <summary>
    /// Register list of <see cref="BaseSetting"/>s displayed on the bomttom of all menus.
    /// </summary>
    /// <param name="toPin">The list of <see cref="ServerSpecificSettingBase"/> to pin.</param>
    public static void RegisterBottomPinnedSettings(this List<BaseSetting> toPin) => _pinnedBottomSettings[Assembly.GetCallingAssembly()] = toPin;

    /// <summary>
    /// Remove bottom pinned settings from <see cref="Assembly.GetCallingAssembly"/>.
    /// </summary>
    public static void UnregisterBottomPinnedSettings() => _pinnedBottomSettings.Remove(Assembly.GetCallingAssembly());
    #endregion

    #region Loading Menu
    /// <summary>
    /// Load <see cref="Menu"/> for <see cref="ReferenceHub"/>.
    /// Null loads main menu.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <param name="menu">Target <see cref="Menu"/>.</param>
    internal static void LoadMenu(this ReferenceHub hub, Menu menu)
    {
        hub.GetCurrentMenu()?.OnClose(hub);

        if (menu == null)
        {
            List<Menu> mainMenus = [.. _registeredMenus.Where(m => m.CheckAccess(hub) && m.ParentMenu == null)];

            menu = mainMenus.Count switch
            {
                1 => mainMenus[0],
                _ => new CentralMainMenu()
            };

            Log.Debug("MenuManager.LoadMenu", mainMenus.Count == 1
                ? $"Triggered the only main menu: {menu.Name}."
                : $"Built central main menu with {mainMenus.Count} submenus for {hub.nicknameSync.DisplayName}"
            );
        }

        if (!menu.CheckAccess(hub))
        {
            Log.Warn("MenuManager.LoadMenu", $"{hub.nicknameSync.DisplayName} tried loading {menu.Name} without access");
            return;
        }

        List<BaseSetting> settings = menu.GetSettings(hub, true, true);
        _syncedMenus[hub] = menu;

        hub.SendSettings(settings);

        menu.OnOpen(hub);
    }

    /// <summary>
    /// Send settings to target <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <param name="settings">List of <see cref="BaseSetting"/> to send.</param>
    internal static void SendSettings(this ReferenceHub hub, List<BaseSetting> settings)
    {
        List<ServerSpecificSettingBase> settingsToSend = [];

        foreach (BaseSetting setting in settings)
            settingsToSend.Add(setting.Base);

        hub.connectionToClient.Send(new SSSEntriesPack([.. settingsToSend], ServerSpecificSettingsSync.Version));
    }
    #endregion

    #region Menu Utils
    /// <summary>
    /// Get a <see cref="BaseSetting"/> by Id for a hub from <typeparamref name="TMenu"/>.
    /// </summary>
    /// <typeparam name="TMenu">Target menu type.</typeparam>
    /// <typeparam name="TSetting">Target SS setting type, must inherit <see cref="BaseSetting"/> or <see cref="ServerSpecificSettingBase"/>.</typeparam>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <param name="settingId">ID of the setting.</param>
    /// <returns>The matching <see cref="BaseSetting"/> or dummy setting if not found.</returns>
    public static TSetting GetSetting<TMenu, TSetting>(this ReferenceHub hub, int settingId)
    where TMenu : Menu
    where TSetting : class
    {
        static T CreateDummy<T>() where T : class
        {
            Type t = typeof(T);

            if (typeof(BaseSetting).IsAssignableFrom(t))
            {
                if (t == typeof(Button)) return (T)(object)new Button(int.MinValue, "", "");
                if (t == typeof(Dropdown)) return (T)(object)new Dropdown(int.MinValue, "", []);
                if (t == typeof(Slider)) return (T)(object)new Slider(int.MinValue, "", 0, 1);
                if (t == typeof(ABButton)) return (T)(object)new ABButton(int.MinValue, "", "A", "B");
                if (t == typeof(TextBox)) return (T)(object)new TextBox(int.MinValue, "");
                if (t == typeof(TextArea)) return (T)(object)new TextArea(int.MinValue, "");
                if (t == typeof(Keybind)) return (T)(object)new Keybind(int.MinValue, "");
            }

            if (typeof(ServerSpecificSettingBase).IsAssignableFrom(t))
            {
                if (t == typeof(SSButton)) return (T)(object)new SSButton(int.MinValue, "", "");
                if (t == typeof(SSDropdownSetting)) return (T)(object)new SSDropdownSetting(int.MinValue, "", []);
                if (t == typeof(SSSliderSetting)) return (T)(object)new SSSliderSetting(int.MinValue, "", 0, 1);
                if (t == typeof(SSTwoButtonsSetting)) return (T)(object)new SSTwoButtonsSetting(int.MinValue, "", "A", "B");
                if (t == typeof(SSPlaintextSetting)) return (T)(object)new SSPlaintextSetting(int.MinValue, "");
                if (t == typeof(SSTextArea)) return (T)(object)new SSTextArea(int.MinValue, "");
                if (t == typeof(SSKeybindSetting)) return (T)(object)new SSKeybindSetting(int.MinValue, "");
            }

            throw new Exception($"Unsupported setting type: {t.Name}");
        }

        if (!typeof(BaseSetting).IsAssignableFrom(typeof(TSetting)) && !typeof(ServerSpecificSettingBase).IsAssignableFrom(typeof(TSetting)))
        {
            Log.Error("MenuManager.GetSetting", $"{nameof(TSetting)} must inherit BaseSetting or ServerSpecificSettingBase, returning dummy");
            return CreateDummy<TSetting>();
        }

        foreach (Menu menu in _registeredMenus.Where(m => m is TMenu))
        {
            if (!menu.BuiltSettings.TryGetValue(hub, out List<BaseSetting> builtSettings)) continue;

            foreach (BaseSetting builtSetting in builtSettings)
            {
                if (builtSetting is BaseSetting b && typeof(TSetting) == b.GetType() &&
                        (b.SettingId == settingId || b.SettingId - menu.Hash == settingId))
                    return b as TSetting;

                if (builtSetting.Base is ServerSpecificSettingBase ssb && typeof(TSetting) == ssb.GetType() &&
                        (ssb.SettingId == settingId || ssb.SettingId - menu.Hash == settingId))
                    return ssb as TSetting;
            }
        }

        Log.Warn("MenuManager.GetSetting", $"Failed to find setting of type {typeof(TSetting).Name} ({settingId}) for hub {hub.nicknameSync.DisplayName}, returning dummy");
        return CreateDummy<TSetting>();
    }

    /// <summary>
    /// Loaded <see cref="ReferenceHub"/> menu.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <returns><see cref="Menu"/> if <see cref="ReferenceHub"/> opens a menu, null if on the main menu.</returns>
    public static Menu GetCurrentMenu(this ReferenceHub hub) => _syncedMenus.TryGetValue(hub, out Menu menu) ? menu : null;

    /// <summary>
    /// Get a menu by <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns><see cref="Menu"/> (If Found).</returns>
    public static Menu GetMenu(this Type type) => _registeredMenus.FirstOrDefault(x => x.GetType() == type);

    /// <summary>
    /// Reload current <see cref="Menu"/> for <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">The target <see cref="ReferenceHub"/>.</param>
    public static void ReloadCurrentMenu(this ReferenceHub hub) => hub.LoadMenu(hub.GetCurrentMenu());

    /// <summary>
    /// Reload current <see cref="Menu"/> for all <see cref="ReferenceHub"/>s.
    /// </summary>
    public static void ReloadAll() { foreach (ReferenceHub hub in ReferenceHub.AllHubs.Where(h => h.isClient)) hub.ReloadCurrentMenu(); }
    #endregion
}
