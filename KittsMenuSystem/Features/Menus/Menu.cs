using KittsMenuSystem.Features.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Menus;

public abstract class Menu
{
    #region Inherited
    /// <summary>
    /// Gets or sets name of Menu.
    /// </summary>
    public abstract string Name { get; set; }

    /// <summary>
    /// Gets or sets the id of Menu (Must be greater than 0).
    /// </summary>
    public abstract int Id { get; set; }

    /// <summary>
    /// Parent menu.
    /// </summary>
    #nullable enable
    public virtual Type? ParentMenu { get; set; } = null;
    #nullable disable

    /// <summary>
    /// Gets in-built settings.
    /// </summary>
    public virtual List<BaseSetting> Settings(ReferenceHub hub) => [];

    /// <summary>
    /// <see cref="Menu"/> avaliable to <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">The target <see cref="ReferenceHub"/></param>
    /// <returns><see cref="ReferenceHub"/> can use menu.</returns>
    public virtual bool CheckAccess(ReferenceHub hub) => true;

    /// <summary>
    /// Executed when <see cref="ReferenceHub"/> opens the menu.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    protected internal virtual void OnOpen(ReferenceHub hub) { }

    /// <summary>
    /// Executed when <see cref="ReferenceHub"/> closes the menu.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    protected internal virtual void OnClose(ReferenceHub hub) { }

    /// <summary>
    /// Called when the <see cref="Menu"/> is registered.
    /// </summary>
    protected internal virtual void OnRegistered() { }
    #endregion

    #region Settings
    /// <summary>
    /// Synced settings for a specified <see cref="ReferenceHub"/>.
    /// </summary>
    internal Dictionary<ReferenceHub, List<BaseSetting>> SyncedSettings { get; } = [];

    /// <summary>
    /// Built settings per <see cref="ReferenceHub"/> for this menu.
    /// </summary>
    internal Dictionary<ReferenceHub, List<BaseSetting>> BuiltSettings { get; } = [];

    /// <summary>
    /// Gets the settings to display for a given <see cref="ReferenceHub"/> in this menu.
    /// Includes _pinned content, return/submenu buttons, headers, and any hub-specific overrides.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <returns>List of <see cref="ServerSpecificSettingBase"/> to send to the client.</returns>
    internal List<BaseSetting> GetSettings(ReferenceHub hub)
    {
        Log.Debug("Menu.GetSettings", $"Generating settings for {hub.nicknameSync.DisplayName} in {Name} ({Id})");

        List<BaseSetting> settings = [];

        // Add _pinned textareas
        settings.AddRange(MenuManager.Pinned.Values.SelectMany(s => s));
        if (!MenuManager.Pinned.Values.IsEmpty()) Log.Debug("Menu.GetSettings", $"Added {MenuManager.Pinned.Values.Count()} _pinned settings for {hub.nicknameSync.DisplayName}");

        if (ParentMenu != null)
        {
            Log.Debug("Menu.GetSettings", $"{Name} ({Id}) has a parent menu");

            settings.Add(new Button(
                string.Format(KittsMenuSystem.Config.Translation.ReturnTo.Label, MenuManager.GetMenu(ParentMenu)?.Name ?? "Unknown"),
                KittsMenuSystem.Config.Translation.ReturnTo.ButtonText, (h, _) => h.LoadMenu(ParentMenu.GetMenu())
            ));

            Log.Debug("Menu.GetSettings", $"Added button returning to {ParentMenu.GetMenu().Name} ({ParentMenu.GetMenu().Id}) for {hub.nicknameSync.DisplayName}");
        }
        else if (ParentMenu == null && GetType() != typeof(CentralMainMenu) && GetType() != typeof(KeybindMenu) && MenuManager.RegisteredMenus.Count(m => m.CheckAccess(hub) && m.ParentMenu == null) > 1)
        {
            Log.Debug("Menu.GetSettings", $"{Name} ({Id}) is one of the main menus");

            settings.Add(new Button(
                string.Format(KittsMenuSystem.Config.Translation.ReturnTo.Label, "Main Menu"),
                KittsMenuSystem.Config.Translation.ReturnTo.ButtonText, (h, _) => h.LoadMenu(null)
            ));

            Log.Debug("Menu.GetSettings", $"Added button returning to main menu for {hub.nicknameSync.DisplayName}");
        }

        List<Menu> subMenus = [.. MenuManager.RegisteredMenus.Where(m => m.CheckAccess(hub) && m.ParentMenu == GetType())];

        if (!subMenus.IsEmpty())
        {
            Log.Debug("Menu.GetSettings", $"{Name} ({Id}) has submenu(s)");

            settings.Add(new GroupHeader("Sub Menu(s)"));

            foreach (Menu subMenu in subMenus)
            {
                settings.Add(new Button(
                    string.Format(KittsMenuSystem.Config.Translation.OpenMenu.Label, subMenu.Name),
                    KittsMenuSystem.Config.Translation.OpenMenu.ButtonText, (h, _) => h.LoadMenu(subMenu)
                ));

                Log.Debug("Menu.GetSettings", $"Added sub menu button going to {subMenu.Name} ({subMenu.Id}) for {hub.nicknameSync.DisplayName}");
            }
        }

        settings.Add(new GroupHeader(Name));
        Log.Debug("Menu.GetSettings", $"Added main header of {Name} ({Id}) for {hub.nicknameSync.DisplayName}");

        // AssemblyMenu hub-specific overrides
        if (this is AssemblyMenu assemblyMenu && assemblyMenu.ActuallySentToClient.TryGetValue(hub, out List<BaseSetting> overrideSettings) && overrideSettings != null)
        {
            if (overrideSettings.Count == 0) settings.RemoveAt(settings.Count - 1); // Remove footer if empty
            settings.AddRange(overrideSettings);
            Log.Debug("Menu.GetSettings", $"Applied {overrideSettings.Count} assembly menu override settings for {hub.nicknameSync.DisplayName}");
            return settings;
        }

        settings.AddRange(Settings(hub));

		Dictionary<int, BaseSetting> seenIds = [];
        List<BaseSetting> filteredSettings = [];

        foreach (BaseSetting setting in settings)
        {
            // Hash setting with menu to avoid other settings from other menus colliding
            if (GetType() != typeof(KeybindMenu))
            {
                setting.SettingId += Hash;
                Log.Debug("Menu.GetSettings", $"Hashed {setting.GetType().Name} ({setting.Base.SettingId}) with {Hash}, new Id: {setting.SettingId}");
            }

            if (seenIds.ContainsKey(setting.SettingId))
            {
                Log.Warn("Menu.GetSettings", $"Skipping duplicate {setting.GetType().Name} ({setting.SettingId}) in {Name} ({Id}) for {hub.nicknameSync.DisplayName}");
                continue;
            }

            seenIds[setting.SettingId] = setting;
            filteredSettings.Add(setting);
        }

        Log.Debug("Menu.GetSettings", $"Finalized {settings.Count} settings on {Name} ({Id}) for {hub.nicknameSync.DisplayName}");

        BuiltSettings[hub] = filteredSettings;
        return filteredSettings;
    }
    #endregion

    #region Reloading
    /// <summary>
    /// Reload this <see cref="Menu"/> for <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">The target <see cref="ReferenceHub"/>.</param>
    public void ReloadFor(ReferenceHub hub) => hub.LoadMenu(this);

    /// <summary>
    /// Reload this <see cref="Menu"/> for all <see cref="ReferenceHub"/>s.
    /// </summary>
    public void ReloadForAll()
    {
        foreach (ReferenceHub hub in MenuManager.SyncedMenus.Where(x => x.Value == this).Select(x => x.Key).ToList())
            hub.LoadMenu(this);
    }
    #endregion

    /// <summary>
    /// Gets Hash of menu based on <see cref="Name"/>. Used to seperate menu settings.
    /// </summary>
    public int Hash => Mathf.Abs(Name.GetHashCode() % 100000);
}
