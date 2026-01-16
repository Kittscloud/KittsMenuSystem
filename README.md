# KittsMenuSystem
*LabAPI Server-Specific Menu Framework*

![License](https://img.shields.io/badge/License-LGPL%20v2.1-blue?style=for-the-badge)
![GitHub all releases](https://img.shields.io/github/downloads/Kittscloud/KittsMenuSystem/total?style=for-the-badge)
![GitHub release](https://img.shields.io/github/v/release/Kittscloud/KittsMenuSystem?style=for-the-badge)
![](https://img.shields.io/badge/.NET-4.8.1-512BD4?logo=dotnet&logoColor=fff&style=for-the-badge)
![GitHub stars](https://img.shields.io/github/stars/Kittscloud/KittsMenuSystem?style=for-the-badge)
![GitHub issues](https://img.shields.io/github/issues/Kittscloud/KittsMenuSystem?style=for-the-badge)

`KittsMenuSystem` is a framework that adds a complete ServerSpecific menu system for `SCP Secret Laboratory` using `LabAPI`.

## Important
`KittsMenuSystem` is mostly based on another menu framework called [`SSMenuSystem`](https://github.com/skyfr0676/SSMenuSystem), which has been discontinued. Since the original framework was discontinued, I have taken it over, updating it to the latest versions and improving the plugin overall.

That being said, the original plugin developer, [`skyfr0676`](https://github.com/skyfr0676), created a fantastic plugin, and I am personally very grateful that this project existed in the first place.

## Consider Supporting?
If you enjoy this project and would like to support future development, I would greatly appreciate it if you considered donating via my [`Ko-Fi`](https://ko-fi.com/kittscloud)

## How to use KittsMenuSystem:
### As a Server Owner
To install `KittsMenuSystem` on your server, you will need:
- `Harmony` version 2.4.2 or later.
- The `KittsMenuSystem.dll` file, available in the releases.

Once you have these:
- Place `Harmony.dll` in the `dependencies` folder
- Place `KittsMenuSystem.dll` in the `plugins` folder

Run the server and you're set!

### Configurations:
| Parameter                    | Type   | Description                                                            | Default Value |
|------------------------------|--------|------------------------------------------------------------------------|---------------|
| `IsEnabled`                  | `bool` | Define if the plugin is enabled or not                                 | `true`        |
| `Debug`                      | `bool` | Define if the plugin can log debug info or not                         | `false`       |
| `AllowPinnedContent`         | `bool` | Defines whether pinned content is allowed                              | `true`        |
| `ShowErrorToClient`          | `bool` | Define if players can see error messages                               | `true`        |
| `ShowFullErrorToClient`      | `bool` | **HIGHLY UNRECOMMENDED TO SET TRUE** allows players to see full errors | `false`       |
| `ShowFullErrorToModerators`  | `bool` | Moderators (RA access) can see full errors                             | `true`        |
| `ForceMainMenuEvenIfOnlyOne` | `bool` | If only one menu is registered and false, menu opens automatically     | `false`       |
| `EnableExamples`             | `bool` | Enables example menus.                                                 | `true`        |
| `CompatibilityEnabled`       | `bool` | Register menus from other plugins using KittsMenuSystem                | `true`        |
| `Translation`                | `bool` | Sets default translations (see below)                                  | `Translation` |

### Translation:

| Parameter               | Type                | Description                                          | Default   |
|-------------------------|---------------------|------------------------------------------------------|-----------|
| `OpenMenu`              | `ButtonConfig`      | Used in the main menu, `{0}` = menu name             | `Open`    |
| `ReturnToMenu`          | `ButtonConfig`      | Displayed when a menu is opened                      | `Return`  |
| `ReturnTo`              | `ButtonConfig`      | Displayed when sub-menu is opened `{0}` = menu name  | `Return`  |
| `ReloadButton`          | `ButtonConfig`      | Reload menus button                                  | `Reload`  |
| `GlobalKeybindingTitle` | `GroupHeaderConfig` | Display the global keybindings header                | `default` |
| `ServerError`           | `string`            | Text shown when an error is occurred                 | `default` |
| `SubMenuTitle`          | `GroupHeaderConfig` | Title of sub-menus                                   | `default` |
| `NoPermission`          | `string`            | Shown when permission blocks full error              | `default` |

### Default YML config file:
```yml
# Is plugin enabled
is_enabled: true
# Sends debug logs to console
debug: false
# Allows pins (pins are seen on all menus).
allow_pinned_content: true
# Whether players can see errors or not.
show_error_to_client: true
# Whether players can see total errors including plugin content or not. Recommended set to false
show_full_error_to_client: false
# Whether moderators (RA access) can see total errors including plugin content or not.
show_full_error_to_moderators: true
# If only one menu registered and false, menu opens automatically.
force_main_menu_even_if_only_one: false
# Whether example menus in-built to the plugin are enabled.
enable_examples: true
# Whether the compatibility system will register all menus in other plugins that use KittsMenuSystem.
compatibility_enabled: true
translation:
# Main menu button opening menu. {0} = menu name.
  open_menu:
    label: Open {0}
    button_text: Open
    hint: 
  # Button shown when menu open.
  return_to_menu:
    label: Return to menu
    button_text: Return
    hint: 
  # Button shown in sub-menu returning to parent. {0} = menu name.
  return_to:
    label: Return to {0}
    button_text: Return
    hint: 
  # Reload menus button.
  reload_button:
    label: Reload menus
    button_text: Reload
    hint: 
  # Global keybinding header.
  global_keybinding_title:
    label: Global Keybinding
    hint: Global keybindings shared across menus
  # Error text shown to prevent client crash. Supports TMP tags.
  server_error: Internal Server Error
  # Sub-menu title.
  sub_menu_title:
    label: Sub-Menus
    hint: ''
  # Message shown when permission blocks full error details.
  no_permission: Insufficient permissions to view full error details

```

### As a Developer
To install in your project, simply reference the `KittsMenuSystem.dll` file, found in the releases.

Create a new class inheriting the `Menu` class

### Example Menu
```csharp
public class Test : Menu
{
    public override ServerSpecificSettingBase[] Settings => [
        // Settings in here.
    ];

    public override string Name { get; set; } = "Test";
    public override int Id { get; set; } = -1;
}
```

### Parameters of Menu
| Parameter / Method            | Type / Return Type                                                  | Description                                                                                         |
|-------------------------------|---------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------|
| `Name`                        | `string`                                                            | Name of the menu, displayed as header and on buttons. Must be unique.                               |
| `Id`                          | `int`                                                               | ID of the menu. Must be greater than 0.                                                             |
| `Description`                 | `string`                                                            | Optional description of the menu, displayed as hint.                                                |
| `MenuRelated`                 | `Type?`                                                             | Optional parent menu type for sub-menus.                                                            |
| `Settings`                    | `ServerSpecificSettingBase[]`                                       | Array of in-built settings for the menu.                                                            |
| `SettingsSync`                | `ReadOnlyDictionary<ReferenceHub, List<ServerSpecificSettingBase>>` | Synced parameters per player.                                                                       |
| `Hash`                        | `int`                                                               | Read-only hash derived from `Name` to separate settings per client.                                 |
| `CheckAccess(hub)`            | `bool`                                                              | Determines whether a player can access the menu. Default returns `true`.                            |
| `GetSettings(hub)`            | `List<ServerSpecificSettingBase>`                                   | Generates settings to send to the player, including pinned content, headers, sub-menu buttons, etc. |
| `GetSettingsFor(hub)`         | `ServerSpecificSettingBase[]`                                       | Override point to provide hub-specific settings. Default returns `null`.                            |
| `OnInput(hub, setting)`       | `void`                                                              | Called when a player interacts with a setting.                                                      |
| `ProperlyEnable(hub)`         | `void`                                                              | Called when a player opens the menu.                                                                |
| `ProperlyDisable(hub)`        | `void`                                                              | Called when a player closes the menu.                                                               |
| `TryGetSubMenu(id, out menu)` | `bool`                                                              | Tries to get a sub-menu by its ID.                                                                  |
| `ReloadFor(hub)`              | `void`                                                              | Reloads this menu for a specific player.                                                            |
| `ReloadForAll()`              | `void`                                                              | Reloads this menu for all players.                                                                  |
| `OnRegistered()`              | `void`                                                              | Called when the menu is registered.                                                                 |

### Parameters of MenuManager
| Parameter / Method                             | Type / Return Type                            | Description                                                      |
|------------------------------------------------|-----------------------------------------------|------------------------------------------------------------------|
| `MenuSync`                                     | `IReadOnlyDictionary<ReferenceHub, Menu>`     | Currently loaded menu for each player.                           |
| `LoadedMenus`                                  | `IReadOnlyList<Menu>`                         | All menus registered in the system.                              |
| `Pinned`                                       | `IReadOnlyDictionary<Assembly, SSTextArea[]>` | All pinned content registered by assemblies.                     |
| `QueueOrRegister()`                            | `void`                                        | Queues or immediately registers the calling assembly’s menus.    |
| `RegisterAll()`                                | `void`                                        | Registers all menus in the calling assembly.                     |
| `Register(this Menu menu)`                     | `void`                                        | Registers a specific menu instance. Throws exception if invalid. |
| `Unregister(this Menu menu)`                   | `void`                                        | Unregisters a menu and removes it from all players.              |
| `UnregisterAll()`                              | `void`                                        | Unregisters all menus.                                           |
| `GetCurrentMenu(this ReferenceHub hub)`        | `Menu`                                        | Returns the menu the player currently has open.                  |
| `LoadMenu(this ReferenceHub hub, Menu menu)`   | `void`                                        | Loads a menu for a player, handling access and pinned content.   |
| `DeleteFromMenuSync(this ReferenceHub hub)`    | `void`                                        | Removes the player from `MenuSync` when leaving.                 |
| `GetMenu(this Type type)`                      | `Menu`                                        | Returns a menu instance by type.                                 |
| `ReloadMenu<T>(this ReferenceHub hub, T menu)` | `void`                                        | Reloads a specific menu for a player.                            |
| `ReloadCurrentMenu(this ReferenceHub hub)`     | `void`                                        | Reloads the current menu for a player.                           |
| `ReloadAll()`                                  | `void`                                        | Reloads current menu for all players.                            |
| `RegisterPin(this SSTextArea[] toPin)`         | `void`                                        | Registers pins for the calling assembly.                         |
| `UnregisterPins()`                             | `void`                                        | Removes all pins registered by the calling assembly.             |
| `GetMenu(Assembly assembly)`                   | `AssemblyMenu`                                | Returns the `AssemblyMenu` for a given assembly (if any).        |

You can look at the [`Examples`](https://github.com/skyfr0676/SSMenuSystem/tree/master/Examples) folder to get a better idea of how `Menus` and `Settings` are implemented.

When you enable your plugin, simply run:
```csharp
MenuManager.RegisterAll();
```
This will register all menus in your assembly. It is important you keep the `KittsMenuSystem.dll` in the `plugins` folder as `KittsMenuSystem.dll` needs to run as a plugin so it can register all menus from all assemblies with the `KittsMenuSystem.dll`

## Found a bug or have feedback?
If you have found a bug please make an issue on GitHub or the quickest way is to message me on discord at `kittscloud`.

Also message me on discord if you have feedback for me, I'd appreciate it very much. Thank you!