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
    /// Gets or sets name of <see cref="Menu"/>.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets Hash of <see cref="Menu"/> based on <see cref="Name"/>. Used to seperate menu settings.
    /// </summary>
    public int Hash => Mathf.Abs(Name.GetHashCode() % 100000);

    /// <summary>
    /// Gets or sets the id of <see cref="Menu"/> (Must be greater than 0).
    /// </summary>
    public abstract int Id { get; }

    /// <summary>
    /// Parent <see cref="Menu"/>.
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
    /// Executed when <see cref="ReferenceHub"/> opens the <see cref="Menu"/>.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    protected internal virtual void OnOpen(ReferenceHub hub) { }

    /// <summary>
    /// Executed when <see cref="ReferenceHub"/> closes the <see cref="Menu"/>.
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
    /// Original definitions for built settings.
    /// </summary>
    internal Dictionary<int, ServerSpecificSettingBase> DefinitionCache { get; } = [];

    /// <summary>
    /// Built settings per <see cref="ReferenceHub"/> for this <see cref="Menu"/>.
    /// </summary>
    internal Dictionary<ReferenceHub, List<BaseSetting>> BuiltSettings { get; } = [];

    /// <summary>
    /// Gets built settings for a given <see cref="ReferenceHub"/> in this <see cref="Menu"/>.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <param name="callSettings">Should function call settings.</param>
    /// <param name="rebuildSettings">Should function rebuild settings.</param>
    /// <returns>List of built <see cref="BaseSetting"/>s.</returns>
    internal List<BaseSetting> GetSettings(ReferenceHub hub, bool callSettings, bool rebuildSettings)
    {
        BuiltSettings.TryGetValue(hub, out List<BaseSetting> settings);
        settings ??= BuildSettings(hub);

        if (!rebuildSettings && callSettings)
            Settings(hub);

        if (rebuildSettings)
            settings = RebuildSettings(hub);

        Log.Debug("Menu.GetSettings", $"Got {settings.Count} settings for {hub.nicknameSync.DisplayName} in {Name} ({Id})");

        return settings;
    }

    /// <summary>
    /// Builds the settings to display for a given <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <returns>List of <see cref="BaseSetting"/>s built.</returns>
    private List<BaseSetting> BuildSettings(ReferenceHub hub)
    {
        Log.Debug("Menu.BuildSettings", $"Building settings for {hub.nicknameSync.DisplayName}");

        List<BaseSetting> built = GenerateSettings(hub);

        BuiltSettings[hub] = built;

        foreach (BaseSetting s in built)
            DefinitionCache[s.Base.SettingId] = s.Base;

        Log.Debug("Menu.BuildSettings", $"Built {built.Count} settings for {hub.nicknameSync.DisplayName}");

        return built;
    }

    /// <summary>
    /// Rebuilds settings for a hub, keeping already-built settings and adding new ones.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <returns>List of <see cref="BaseSetting"/>s rebuilt.</returns>
    private List<BaseSetting> RebuildSettings(ReferenceHub hub)
    {
        Log.Debug("Menu.RebuildSettings", $"Rebuilding settings for {hub.nicknameSync.DisplayName}");

        BuiltSettings.TryGetValue(hub, out List<BaseSetting> existing);
        existing ??= BuildSettings(hub);

        Dictionary<int, BaseSetting> existingMap = existing.ToDictionary(s => s.SettingId, s => s);

        List<BaseSetting> generated = GenerateSettings(hub);
        List<BaseSetting> rebuilt = new(generated.Count);

        foreach (BaseSetting gen in generated)
        {
            if (existingMap.TryGetValue(gen.SettingId, out BaseSetting old))
                rebuilt.Add(old);
            else
                rebuilt.Add(gen);
        }

        BuiltSettings[hub] = rebuilt;

        foreach (BaseSetting s in rebuilt)
            DefinitionCache[s.Base.SettingId] = s.Base;

        Log.Debug("Menu.RebuildSettings", $"Rebuilt {rebuilt.Count} settings for {hub.nicknameSync.DisplayName}");

        return rebuilt;
    }

    /// <summary>
    /// Generate the settings to display for a given <see cref="ReferenceHub"/> in this <see cref="Menu"/>.
    /// Includes _pinned content, return/submenu buttons, headers, and any hub-specific overrides.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    /// <returns>List of <see cref="BaseSetting"/>s generated.</returns>
    private List<BaseSetting> GenerateSettings(ReferenceHub hub)
    {
        List<BaseSetting> settings = [];

        settings.AddRange(MenuManager.PinnedTopSettings.Values.SelectMany(p => p));

        if (ParentMenu != null)
        {
            settings.Add(new Button(
                string.Format(KittsMenuSystem.Config.Translation.ReturnTo.Label, MenuManager.GetMenu(ParentMenu)?.Name ?? "Unknown"),
                KittsMenuSystem.Config.Translation.ReturnTo.ButtonText,
                (h, _) => h.LoadMenu(ParentMenu.GetMenu())
            ));
        }
        else if (ParentMenu == null &&
            GetType() != typeof(CentralMainMenu) &&
            GetType() != typeof(GlobalMenu) &&
            MenuManager.RegisteredMenus.Count(m => m.CheckAccess(hub) && m.ParentMenu == null) > 1)
        {
            settings.Add(new Button(
                string.Format(KittsMenuSystem.Config.Translation.ReturnTo.Label, "Main Menu"),
                KittsMenuSystem.Config.Translation.ReturnTo.ButtonText,
                (h, _) => h.LoadMenu(null)
            ));
        }

        List<Menu> subMenus = [.. MenuManager.RegisteredMenus.Where(m => m.CheckAccess(hub) && m.ParentMenu == GetType())];

        if (!subMenus.IsEmpty())
        {
            settings.Add(new GroupHeader("Sub Menu(s)"));
            foreach (Menu subMenu in subMenus)
                settings.Add(new Button(
                    string.Format(KittsMenuSystem.Config.Translation.OpenMenu.Label, subMenu.Name),
                    KittsMenuSystem.Config.Translation.OpenMenu.ButtonText,
                    (h, _) => h.LoadMenu(subMenu)
                ));
        }

        settings.Add(new GroupHeader(Name));
        settings.AddRange(Settings(hub));

        settings.AddRange(MenuManager.PinnedBottomSettings.Values.SelectMany(p => p));

        Dictionary<int, BaseSetting> seen = [];
        List<BaseSetting> final = [];

        foreach (BaseSetting setting in settings)
        {
            if (GetType() != typeof(GlobalMenu))
                setting.SettingId += Hash;

            if (seen.ContainsKey(setting.SettingId))
                continue;

            seen[setting.SettingId] = setting;
            final.Add(setting);
        }

        return final;
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
            ReloadFor(hub);
    }
    #endregion
}
