using KittsMenuSystem.Examples;
using KittsMenuSystem.Features.Settings;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UserSettings.ServerSpecific;
using static UnityEngine.Rendering.RayTracingAccelerationStructure;

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
    private readonly static Dictionary<Assembly, List<TextArea>> _pinned = [];
    
    /// <summary>
    /// Contains all loaded <see cref="Menu"/>s.
    /// </summary>
    public static IReadOnlyList<Menu> RegisteredMenus => _registeredMenus;

    /// <summary>
    /// Contains <see cref="Assembly"/> with their pins.
    /// </summary>
    public static IReadOnlyDictionary<Assembly, List<TextArea>> Pinned => _pinned;

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
                    t != typeof(AssemblyMenu) &&
                    t != typeof(CentralMainMenu) &&
                    t != typeof(KeybindMenu) &&
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
                throw new ArgumentException("Menu ID cannot be 1 (reserved for Keybinds Menu)");
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
    /// Register <see cref="ServerSpecificSettingBase"/> displayed on the top of all menus.
    /// </summary>
    /// <param name="toPin">The list of <see cref="ServerSpecificSettingBase"/> to pin.</param>
    public static void RegisterPins(this List<TextArea> toPin) => _pinned[Assembly.GetCallingAssembly()] = toPin;

    /// <summary>
    /// Remove registered pins from <see cref="Assembly.GetCallingAssembly"/>.
    /// </summary>
    public static void UnregisterAllPins() => _pinned.Remove(Assembly.GetCallingAssembly());
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

    internal static AssemblyMenu GetMenu(Assembly assembly) => _registeredMenus.OfType<AssemblyMenu>().FirstOrDefault(x => x.Assembly == assembly);

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

    #region Sending Menu
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
                : $"Built central main menu with {mainMenus.Count} submenus for {hub.nicknameSync.DisplayName}");
        }

        if (!menu.CheckAccess(hub))
        {
            Log.Warn("MenuManager.LoadMenu", $"{hub.nicknameSync.DisplayName} tried loading {menu.Name} without access");
            return [];
        }

        List<BaseSetting> settings = menu.GetSettings(hub);
        _syncedMenus[hub] = menu;

        if (menu is not CentralMainMenu and not KeybindMenu && !menu.SyncedSettings.ContainsKey(hub))
            Timing.RunCoroutine(hub.SyncMenu(menu));
        else
            hub.SendSettings(settings);

        menu.OnOpen(hub);

        return settings;
    }

    /// <summary>
    /// Send settings to target <see cref="ReferenceHub"/>.
    /// Only used for Main Menu.
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

    #region Syncing Menus
    internal static readonly Dictionary<ReferenceHub, List<ServerSpecificSettingBase>> SyncCache = [];

    /// <summary>
    /// Get a <see cref="ServerSpecificSettingBase"/> by Id for a hub using a menu of type <typeparamref name="TMenu"/>.
    /// </summary>
    /// <typeparam name="TMenu">Target menu type.</typeparam>
    /// <typeparam name="TSetting">Target SS setting type, must inherit <see cref="ServerSpecificSettingBase"/>.</typeparam>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <param name="settingId">ID of the setting.</param>
    /// <returns>The matching <see cref="ServerSpecificSettingBase"/> or null if not found.</returns>
    public static TSetting GetSetting<TMenu, TSetting>(this ReferenceHub hub, int settingId)
        where TMenu : Menu
        where TSetting : ServerSpecificSettingBase
    {
        if (typeof(TSetting).BaseType == typeof(BaseSetting))
        {
            Log.Error("MenuManager.GetSetting", $"{nameof(TSetting)} needs to be of base type");
            return null;
        }

        foreach (Menu menu in _registeredMenus.Where(m => m is TMenu))
        {
            if (!menu.BuiltSettings.TryGetValue(hub, out List<BaseSetting> settings))
            {
                Log.Debug("MenuManager.GetSetting", $"No synced settings for hub {hub.nicknameSync.DisplayName} in menu {menu.Name} ({menu.Id})");
                continue;
            }

            ServerSpecificSettingBase t = settings
                .Select(b => b.Base)
                .Where(s => s is TSetting)
                .FirstOrDefault(s => 
                    s.SettingId == settingId || 
                    s.SettingId - menu.Hash == settingId
                );

            return t as TSetting;
        }

        Log.Warn("MenuManager.GetSetting", $"Failed to find setting of type {typeof(TSetting).Name} ({settingId}) for hub {hub.nicknameSync.DisplayName}");
        return null;
    }

    /// <summary>
    /// Sync settings from a single menu for a ReferenceHub.
    /// </summary>
    internal static IEnumerator<float> SyncMenu(this ReferenceHub hub, Menu menu)
    {
        if (!menu.CheckAccess(hub))
        {
            Log.Debug("MenuManager.SyncMenu", $"{hub.nicknameSync.DisplayName} has no access to {menu.Name}");
            yield break;
        }

        List<BaseSetting> sendSettings = [.. menu.Settings(hub).Where(bs => bs.Base.ResponseMode == ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange)];

        hub.SendSettings(sendSettings);

        float timeout = 0f;
        const float timeoutLimit = 10f;
        const float waitTime = 0.01f;

        while (SyncCache[hub].Count < sendSettings.Count && timeout < timeoutLimit)
        {
            timeout += waitTime;
            yield return waitTime;
        }

        if (SyncCache[hub].Count < sendSettings.Count)
        {
            Log.Error("MenuManager.SyncMenu", $"Timeout syncing {hub.nicknameSync.DisplayName} on {menu.Name}");
            yield break;
        }

        // Map synced ServerSpecificSettingBase back into BaseSetting
        List<BaseSetting> syncedWrapped = [.. menu.GetSettings(hub)
            .Select(bs =>
            {
                ServerSpecificSettingBase synced = SyncCache[hub].FirstOrDefault(x => x.SettingId == bs.Base.SettingId);
                if (synced != null) bs.Base = synced;
                return bs;
            })
            .Where(bs => bs.Base != null)];

        menu.SyncedSettings[hub] = syncedWrapped;

        Log.Debug("MenuManager.SyncMenu", $"Synced {syncedWrapped.Count} settings for {hub.nicknameSync.DisplayName} in {menu.Name}");

        sendSettings.Clear();
        SyncCache[hub].Clear();

        hub.LoadMenu(new KeybindMenu());

        MenuEvents.MenuState[hub] = (false, null);
    }

    /// <summary>
    /// Sync settings for all accessible menus for a ReferenceHub.
    /// </summary>
    internal static IEnumerator<float> SyncAllMenus(this ReferenceHub hub)
    {
        SyncCache.Add(hub, []);

        List<Menu> accessibleMenus = [.. _registeredMenus.Where(m => m.CheckAccess(hub))];
        if (!accessibleMenus.Any())
        {
            Log.Warn("MenuManager.SyncAllMenus", $"No accessible menus for {hub.nicknameSync.DisplayName}");
            yield break;
        }

        for (int i = 0; i < accessibleMenus.Count; i++)
        {
            Menu menu = accessibleMenus[i];
            bool isLast = i == accessibleMenus.Count - 1;

            IEnumerator<float> enumerator = hub.SyncMenu(menu);
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        // Cleanup
        SyncCache.Remove(hub);
    }
    #endregion
}
