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
    /// <returns>List of <see cref="BaseSetting"/> that was loaded</returns>
    internal static List<BaseSetting> LoadMenu(this ReferenceHub hub, Menu menu)
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
            return [];
        }

        List<BaseSetting> settings = menu.GetSettings(hub, true, true);
        _syncedMenus[hub] = menu;

        hub.SendSettings(settings);

        menu.OnOpen(hub);

        return settings;
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
    public static void ReloadAll() { foreach (ReferenceHub hub in ReferenceHub.AllHubs) hub.ReloadCurrentMenu(); }
    #endregion
}
