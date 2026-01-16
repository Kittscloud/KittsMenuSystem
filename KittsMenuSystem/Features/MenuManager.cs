using HarmonyLib;
using KittsMenuSystem.Examples;
using KittsMenuSystem.Features.Interfaces;
using KittsMenuSystem.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features;

public static class MenuManager
{
    private static readonly Dictionary<ReferenceHub, Menu> _menuSync = [];
    private static readonly List<Menu> _loadedMenus = [];
    private static readonly Dictionary<Assembly, SSTextArea[]> _pinned = [];

    /// <summary>
    /// Contains every <see cref="ReferenceHub"/> with their loaded <see cref="Menu"/>.
    /// </summary>
    public static IReadOnlyDictionary<ReferenceHub, Menu> MenuSync => _menuSync;
    /// <summary>
    /// Contains all loaded <see cref="Menu"/>s.
    /// </summary>
    public static IReadOnlyList<Menu> LoadedMenus => _loadedMenus;
    /// <summary>
    /// Contains <see cref="Assembly"/> with their pins.
    /// </summary>
    public static IReadOnlyDictionary<Assembly, SSTextArea[]> Pinned => _pinned;

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
    /// Calling assembly loads immediately if menu system initialized, otherwise queues and loads on plugin initialization.
    /// </summary>
    public static void QueueOrRegister()
    {
        if (KittsMenuSystem.Config is null)
        {
            Assembly assembly = Assembly.GetCallingAssembly();

            if (!_waitingAssemblies.Contains(assembly))
                _waitingAssemblies.Enqueue(assembly);
        }
        else
            Assembly.GetCallingAssembly().Register();
    }

    /// <summary>
    /// Register menus in <see cref="Assembly.GetCallingAssembly"/>.
    /// </summary>
    public static void RegisterAll() => Assembly.GetCallingAssembly().Register();

    /// <summary>
    /// Register all menus in the given <see cref="Assembly"/>.
    /// </summary>
    /// <param name="assembly">Target assembly.</param>
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
            Log.Debug("MenuManager.Register", $"Loading assembly {assembly.GetName().Name}...");
            List<Menu> allMenus = [.. assembly.GetTypes()
                .Where(t => t.BaseType == typeof(Menu) &&
                    !t.IsAbstract &&
                    !t.IsInterface &&
                    t != typeof(AssemblyMenu) &&
                    (t != typeof(MainExample) || KittsMenuSystem.Config.EnableExamples))
                .Select(t => Activator.CreateInstance(t) as Menu)];

            IEnumerable<Menu> orderedMenus = allMenus.OrderBy(m => m.MenuRelated == null ? 0 : 1).ThenBy(m => m.Id);
            List<Menu> registeredMenus = [];

            foreach (Menu menu in orderedMenus)
                try
                {
                    menu.Register();
                    registeredMenus.Add(menu);
                }
                catch (Exception e)
                {
                    Log.Error("MenuManager.Register", $"Error loading menu {menu.Name}: {e.Message}");
                    Log.Debug("MenuManager.Register", e.ToString());
                }

            Log.Info("MenuManager.Register", $"Loaded assembly {assembly.GetName().Name}: {registeredMenus.Count}/{allMenus.Count} menus registered.");
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
            if (menu == null || (menu.MenuRelated == typeof(MainExample) && !KittsMenuSystem.Config.EnableExamples))
                return;

            Log.Debug("MenuManager.Register", $"Loading menu {menu.Name}...");

            if (menu.CheckSameId())
                throw new ArgumentException($"Menu ID {menu.Id} already registered.");
            if (menu.Id == 0)
                throw new ArgumentException("Menu ID cannot be 0 (reserved for Main Menu).");
            if (string.IsNullOrEmpty(menu.Name))
                throw new ArgumentException("Menu name cannot be empty.");
            if (_loadedMenus.Any(m => m.Name == menu.Name))
                throw new ArgumentException($"Duplicate menu name '{menu.Name}'.");

            Dictionary<Type, List<int>> typeIds = [];
            foreach (ServerSpecificSettingBase s in menu.Settings)
            {
                if (s is SSGroupHeader) continue;

                ServerSpecificSettingBase setting = s is ISetting isSet ? isSet.Base : s;
                Type type = setting.GetType();

                typeIds.GetOrAdd(type, () => []);

                if (typeIds[type].Contains(setting.SettingId))
                    throw new ArgumentException($"Duplicate setting ID {setting.SettingId} in menu {menu.Name}.");

                typeIds[type].Add(setting.SettingId);
            }

            if (menu.MenuRelated != null && !_loadedMenus.Any(m => m.GetType() == menu.MenuRelated))
                throw new ArgumentException($"Menu {menu.Name} has invalid related menu {menu.MenuRelated.FullName}.");

            _loadedMenus.Add(menu);
            menu.OnRegistered();
            Log.Debug("MenuManager.Register", $"Menu {menu.Name} registered successfully.");
        }
        catch (Exception e)
        {
            Log.Error("MenuManager.Register", $"Failed to load menu {menu.Name}: {e.Message}");
            Log.Debug("MenuManager.Register", e.ToString());
        }
    }

    private static bool CheckSameId(this Menu menu)
    {
        if (menu.MenuRelated == null)
            return _loadedMenus.Any(x => x.Id == menu.Id && menu.MenuRelated == null);

        return _loadedMenus.Where(x => x.MenuRelated == menu.MenuRelated).Any(x => x.Id == menu.Id);
    }

    /// <summary>
    /// Unregister menu.
    /// </summary>
    /// <param name="menu">The menu.</param>
    public static void Unregister(this Menu menu)
    {
        if (_loadedMenus.Contains(menu))
            _loadedMenus.Remove(menu);

        foreach (KeyValuePair<ReferenceHub, Menu> sync in _menuSync)
            if (sync.Value == menu)
                sync.Key.LoadMenu(null);
    }

    /// <summary>
    /// Unregister all menus.
    /// </summary>
    public static void UnregisterAll()
    {
        foreach (KeyValuePair<ReferenceHub, Menu> sync in _menuSync)
            sync.Key.LoadMenu(null);

        _loadedMenus.Clear();

        foreach (Menu menu in _loadedMenus.ToList())
            menu.Unregister();
    }

    /// <summary>
    /// Get main menu for target <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">The target <see cref="ReferenceHub"/></param>
    /// <returns>In-built parameters shown to <see cref="ReferenceHub"/>.</returns>
    private static ServerSpecificSettingBase[] GetMainMenu(this ReferenceHub hub)
    {
        List<ServerSpecificSettingBase> mainMenu = [];

        if (KittsMenuSystem.Config.AllowPinnedContent)
            mainMenu.AddRange(_pinned.Values.SelectMany(pin => pin));

        if (_loadedMenus.Where(x => x.CheckAccess(hub)).IsEmpty())
            return [.. mainMenu];

        mainMenu.Add(new SSGroupHeader("Main Menu"));
        foreach (Menu menu in _loadedMenus.Where(x => x.CheckAccess(hub) && x.MenuRelated == null))
            mainMenu.Add(new SSButton(menu.Id, string.Format(KittsMenuSystem.Config.Translation.OpenMenu.Label, menu.Name), KittsMenuSystem.Config.Translation.OpenMenu.ButtonText, null, string.IsNullOrEmpty(menu.Description) ? null : menu.Description));

        return [.. mainMenu];
    }

    /// <summary>
    /// Loaded <see cref="ReferenceHub"/> menu.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <returns><see cref="Menu"/> if <see cref="ReferenceHub"/> opens a menu, null if on the main menu.</returns>
    public static Menu GetCurrentMenu(this ReferenceHub hub) => _menuSync.TryGetValue(hub, out Menu menu) ? menu : null;

    /// <summary>
    /// Load <see cref="Menu"/> for <see cref="ReferenceHub"/> (Must have access).
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <param name="menu">Target <see cref="Menu"/>.</param>
    internal static void LoadMenu(this ReferenceHub hub, Menu menu)
    {
        hub.GetCurrentMenu()?.ProperlyDisable(hub);

        Log.Debug("MenuManager.LoadMenu", "Try loading " + (menu?.Name ?? "main menu") + " for player " + hub.nicknameSync.DisplayName);

        if (menu != null && !menu.CheckAccess(hub))
            menu = null;

        if (menu == null && _loadedMenus.Count(x => x.CheckAccess(hub) && x.MenuRelated == null) == 1 && !KittsMenuSystem.Config.ForceMainMenuEvenIfOnlyOne)
        {
            menu = _loadedMenus.First(x => x.CheckAccess(hub) && x.MenuRelated == null);
            Log.Debug("MenuManager.LoadMenu", $"Triggered the only menu registered: {menu.Name}.");
        }

        if (menu == null)
        {
            hub.SendMenu(null, hub.GetMainMenu());
            _menuSync[hub] = null;
            return;
        }

        List<ServerSpecificSettingBase> settings = menu.GetSettings(hub);
        _menuSync[hub] = menu;

        if (!menu.SettingsSync.ContainsKey(hub))
            Timing.RunCoroutine(hub.SyncMenu(menu, [.. settings]));
        else
            hub.SendMenu(menu, [.. settings]);

        menu.ProperlyEnable(hub);
    }

    /// <summary>
    /// Used when player has left server.
    /// </summary>
    /// <param name="hub">The target <see cref="ReferenceHub"/>.</param>
    internal static void DeleteFromMenuSync(this ReferenceHub hub)
    {
        hub.GetCurrentMenu()?.ProperlyDisable(hub);
        _menuSync.Remove(hub);
    }

    /// <summary>
    /// Get a menu by <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns><see cref="Menu"/> (If Found).</returns>
    public static Menu GetMenu(this Type type) => _loadedMenus.FirstOrDefault(x => x.GetType() == type);

    /// <summary>
    /// Reload a <see cref="Menu"/> for <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">The target <see cref="ReferenceHub"/>.</param>
    /// <param name="menu">The target <see cref="ReferenceHub"/>.</param>
    public static void ReloadMenu<T>(this ReferenceHub hub, T menu) where T : Menu => hub.LoadMenu(menu);

    /// <summary>
    /// Reload current <see cref="Menu"/> for <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">The target <see cref="ReferenceHub"/>.</param>
    public static void ReloadCurrentMenu(this ReferenceHub hub) => hub.LoadMenu(hub.GetCurrentMenu());

    /// <summary>
    /// Reload current <see cref="Menu"/> for all <see cref="ReferenceHub"/>s.
    /// </summary>
    public static void ReloadAll() { foreach (ReferenceHub hub in ReferenceHub.AllHubs) hub.ReloadCurrentMenu(); }

    /// <summary>
    /// Register <see cref="ServerSpecificSettingBase"/> displayed on the top of all menus.
    /// </summary>
    /// <param name="toPin">The list of <see cref="ServerSpecificSettingBase"/> to pin.</param>
    public static void RegisterPin(this SSTextArea[] toPin) =>
        _pinned[Assembly.GetCallingAssembly()] = toPin;

    /// <summary>
    /// Remove registered pins from <see cref="Assembly.GetCallingAssembly"/>.
    /// </summary>
    public static void UnregisterPins() => _pinned.Remove(Assembly.GetCallingAssembly());

    internal static void SendMenu(this ReferenceHub hub, Menu relatedMenu, ServerSpecificSettingBase[] settings, int? versionOverride = null)
    {
        if (relatedMenu != null)
            foreach (ServerSpecificSettingBase c in settings)
                if (c.SettingId < relatedMenu.Hash)
                    c.SetId(c.SettingId + relatedMenu.Hash, c.Label);

        hub.connectionToClient.Send(new SSSEntriesPack(settings, versionOverride ?? ServerSpecificSettingsSync.Version));
    }

    internal static AssemblyMenu GetMenu(Assembly assembly) => _loadedMenus.OfType<AssemblyMenu>().FirstOrDefault(x => x.Assembly == assembly);
}
