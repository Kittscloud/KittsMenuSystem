using KittsMenuSystem.Features.Interfaces;
using MEC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Wrappers;

public abstract class Menu
{
    /// <summary>
    /// Synced parameters for a specified <see cref="ReferenceHub"/>.
    /// </summary>
    internal readonly Dictionary<ReferenceHub, List<ServerSpecificSettingBase>> InternalSettingsSync = [];

    /// <summary>
    /// Synced parameters for a specified <see cref="ReferenceHub"/>.
    /// </summary>
    public ReadOnlyDictionary<ReferenceHub, List<ServerSpecificSettingBase>> SettingsSync => new(InternalSettingsSync);

    /// <summary>
    /// <see cref="Menu"/> avaliable to <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">The target <see cref="ReferenceHub"/></param>
    /// <returns><see cref="ReferenceHub"/> can use menu.</returns>
    public virtual bool CheckAccess(ReferenceHub hub) => true;

    /// <summary>
    /// Parent menu.
    /// </summary>
    #nullable enable
    public virtual Type? MenuRelated { get; set; } = null;
    #nullable disable

    /// <summary>
    /// Gets in-built settings.
    /// </summary>
    public virtual ServerSpecificSettingBase[] Settings { get; } = [];

    /// <summary>
    /// Gets settings sent to <see cref="ReferenceHub"/>.
    /// </summary>
    internal readonly Dictionary<ReferenceHub, ServerSpecificSettingBase[]> SentSettings = [];

    /// <summary>
    /// Gets Hash of menu based on <see cref="Name"/>. Used to seperate menu settings for client.
    /// </summary>
    public int Hash => Mathf.Abs(Name.GetHashCode() % 100000);

    /// <summary>
    /// Gets or sets name of Menu.
    /// </summary>
    public abstract string Name { get; set; }

    /// <summary>
    /// Gets or sets description of Menu, shown as <see cref="ServerSpecificSettingBase.HintDescription"/>.
    /// </summary>
    protected internal virtual string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the id of Menu (Must be greater than 0).
    /// </summary>
    public abstract int Id { get; set; }

    /// <summary>
    /// Gets the settings to display for a given <see cref="ReferenceHub"/> in this menu.
    /// Includes pinned content, submenu buttons, headers, and any hub-specific overrides.
    /// </summary>
    /// <param name="hub">The target hub/player.</param>
    /// <returns>List of <see cref="ServerSpecificSettingBase"/> to send to the client.</returns>
    internal List<ServerSpecificSettingBase> GetSettings(ReferenceHub hub)
    {
        Log.Debug("Menu.GetSettings", $"Generating settings for hub {hub.nicknameSync.DisplayName} in menu {Name}.");

        List<ServerSpecificSettingBase> settings = [];

        // Include pinned content if enabled
        if (KittsMenuSystem.Config.AllowPinnedContent)
        {
            List<SSTextArea> pinned = [.. MenuManager.Pinned.Values.SelectMany(p => p)];
            settings.AddRange(pinned);
            Log.Debug("Menu.GetSettings", $"Added {pinned.Count} pinned settings for {hub.nicknameSync.DisplayName}.");
        }

        // Determine main menu and whether to show return buttons
        Menu mainMenu = MenuManager.LoadedMenus.FirstOrDefault(x => x.CheckAccess(hub) && x.MenuRelated == null);
        bool forceReturn = mainMenu != this || KittsMenuSystem.Config.ForceMainMenuEvenIfOnlyOne;

        // Return button if needed
        if (forceReturn)
        {
            string label = MenuRelated != null
                ? string.Format(KittsMenuSystem.Config.Translation.ReturnTo.Label, MenuManager.GetMenu(MenuRelated)?.Name ?? "Unknown")
                : KittsMenuSystem.Config.Translation.ReturnToMenu.Label;
            string text = MenuRelated != null
                ? KittsMenuSystem.Config.Translation.ReturnTo.ButtonText
                : KittsMenuSystem.Config.Translation.ReturnToMenu.ButtonText;

            settings.Add(new SSButton(0, label, text));
            Log.Debug("Menu.GetSettings", $"Added return button '{label}' for {hub.nicknameSync.DisplayName}.");
        }

        // Add group headers
        if (mainMenu == this && !KittsMenuSystem.Config.ForceMainMenuEvenIfOnlyOne)
        {
            settings.Add(new SSGroupHeader(Name));
            Log.Debug("Menu.GetSettings", $"Added main header '{Name}' for {hub.nicknameSync.DisplayName}.");
        }
        else if (MenuManager.LoadedMenus.Any(x => x.MenuRelated == GetType() && x != this))
        {
            settings.Add(new SSGroupHeader(KittsMenuSystem.Config.Translation.SubMenuTitle.Label, hint: KittsMenuSystem.Config.Translation.SubMenuTitle.Hint));
            Log.Debug("Menu.GetSettings", $"Added sub-menu header for {hub.nicknameSync.DisplayName}.");
        }

        // Add submenu buttons
        List<Menu> submenus = [.. MenuManager.LoadedMenus.Where(x => x.MenuRelated == GetType() && x != this)];
        foreach (Menu submenu in submenus)
        {
            settings.Add(new SSButton(submenu.Id,
                string.Format(KittsMenuSystem.Config.Translation.OpenMenu.Label, submenu.Name),
                KittsMenuSystem.Config.Translation.OpenMenu.ButtonText,
                hint: string.IsNullOrEmpty(Description) ? null : Description)
            );
            Log.Debug("Menu.GetSettings", $"Added sub-menu button '{submenu.Name}' for {hub.nicknameSync.DisplayName}.");
        }

        // Footer header
        if (forceReturn)
        {
            settings.Add(new SSGroupHeader(Name, hint: Description));
            Log.Debug("Menu.GetSettings", $"Added footer header '{Name}' for {hub.nicknameSync.DisplayName}.");
        }

        // AssemblyMenu hub-specific overrides
        if (this is AssemblyMenu assemblyMenu && assemblyMenu.ActuallySentToClient.TryGetValue(hub, out ServerSpecificSettingBase[] overrideSettings) && overrideSettings != null)
        {
            if (overrideSettings.Length == 0) settings.RemoveAt(settings.Count - 1); // Remove footer if empty
            settings.AddRange(overrideSettings);
            Log.Debug("Menu.GetSettings", $"Applied {overrideSettings.Length} assembly menu override settings for {hub.nicknameSync.DisplayName}.");
            return settings;
        }

        // Combine normal settings and hub-specific dynamic settings
        List<ServerSpecificSettingBase> allSettings = [];
        void AddSettings(IEnumerable<ServerSpecificSettingBase> src)
        {
            foreach (ServerSpecificSettingBase s in src ?? [])
            {
                allSettings.Add(s);
                settings.Add(s is ISetting i ? i.Base : s);
            }
        }

        AddSettings(Settings);
        AddSettings(GetSettingsFor(hub));

        SentSettings[hub] = [.. allSettings];
        Log.Debug("Menu.GetSettings", $"Finalized settings list for {hub.nicknameSync.DisplayName}: {settings.Count} items.");

        return settings;
    }

    /// <summary>
    /// Executed when action runs.
    /// </summary>
    /// <param name="hub"></param>
    /// <param name="setting"></param>
    public virtual void OnInput(ReferenceHub hub, ServerSpecificSettingBase setting) { }

    /// <summary>
    /// Executed when <see cref="ReferenceHub"/> opens the menu.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    protected internal virtual void ProperlyEnable(ReferenceHub hub) { }

    /// <summary>
    /// Executed when <see cref="ReferenceHub"/> closes the menu.
    /// </summary>
    /// <param name="hub">Target <see cref="ReferenceHub"/>.</param>
    protected internal virtual void ProperlyDisable(ReferenceHub hub) { }

    /// <summary>
    /// Try get sub menu related to target <see cref="Menu"/>.
    /// </summary>
    /// <param name="id">Sub menu id.</param>
    /// <param name="menu"></param>
    /// <returns>The sub-<see cref="Menu"/> (If Found).</returns>
    public bool TryGetSubMenu(int id, out Menu menu) => (menu = MenuManager.LoadedMenus.FirstOrDefault(x => x.Id == id && x.MenuRelated == GetType() && x != this)) != null;

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
        foreach (ReferenceHub hub in MenuManager.MenuSync.Where(x => x.Value == this).Select(x => x.Key).ToList())
            hub.LoadMenu(this);
    }

    /// <summary>
    /// Called when the <see cref="Menu"/> is registered.
    /// </summary>
    protected internal virtual void OnRegistered() { }

    /// <summary>
    /// Get settings for the specific <see cref="ReferenceHub"/>
    /// <param name="hub">The target <see cref="ReferenceHub"/>.</param>
    /// <returns>A list of <see cref="ServerSpecificSettingBase"/> that will be sent into the player.</returns>
    /// </summary>
    public virtual ServerSpecificSettingBase[] GetSettingsFor(ReferenceHub hub) => null;
}
