# KittsMenuSystem
*LabAPI Server-Specific Menu Framework*

[![License](https://img.shields.io/badge/License-AGPL%20v3.0-blue?style=for-the-badge)](https://github.com/Kittscloud/KittsMenuSystem/blob/main/LICENSE) [![Downloads](https://img.shields.io/github/downloads/Kittscloud/KittsMenuSystem/total?style=for-the-badge)](https://github.com/Kittscloud/ServerSpecificsSyncer/releases/latest) [![GitHub release](https://img.shields.io/github/v/release/Kittscloud/KittsMenuSystem?style=for-the-badge)](https://github.com/Kittscloud/KittsMenuSystem/releases/latest) [![](https://img.shields.io/badge/.NET-4.8.1-512BD4?logo=dotnet&logoColor=fff&style=for-the-badge)](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net481) [![GitHub stars](https://img.shields.io/github/stars/Kittscloud/KittsMenuSystem?style=for-the-badge)](https://github.com/Kittscloud/KittsMenuSystem/stargazers) [![GitHub issues](https://img.shields.io/github/issues/Kittscloud/KittsMenuSystem?style=for-the-badge)](https://github.com/Kittscloud/KittsMenuSystem/issues)

`KittsMenuSystem` is a framework that adds a complete ServerSpecific menu system for `SCP Secret Laboratory` using `LabAPI`.

## Important
`KittsMenuSystem` is mostly based on another menu framework called [SSMenuSystem](https://github.com/skyfr0676/SSMenuSystem), which has been discontinued. Since the original framework was discontinued, I have taken it over, updating it to the latest versions and improving the plugin overall.

That being said, the original plugin developer, [skyfr0676](https://github.com/skyfr0676), created a fantastic plugin, and I am personally very grateful that this project existed in the first place.

## Consider Supporting?
If you enjoy this project and would like to support future development, I would greatly appreciate it if you considered donating via my [Ko-Fi](https://ko-fi.com/kittscloud).

## NuGet Package
The `NuGet Package` offers easy intergation of this tool. You can find the `NuGet Packages` here:
- [KittsMenuSystem](https://www.nuget.org/packages/KittsMenuSystem)

## How to use KittsMenuSystem:
To install `KittsMenuSystem` on your server, you will need:
- `Harmony` `v2.4.2` or later.
- `KittsMenuSystem` latest version.

All of these files can be found in the [latest release](https://github.com/Kittscloud/KittsMenuSystem/releases/latest).

Once you have these:
- Place `Harmony.dll` in the `dependencies` folder.
- Place `KittsMenuSystem.dll` in the `plugins` folder.

Run the server and you're set!

### Configurations:
| Parameter                    | Type          | Description                                                           | Default Value |
|------------------------------|---------------|-----------------------------------------------------------------------|---------------|
| `IsEnabled`                  | `bool`        | Is plugin enabled.                                                    | `true`        |
| `Debug`                      | `bool`        | Sends debug logs to console.                                          | `false`       |
| `ShowErrorToClient`          | `bool`        | Whether players can see errors or not.                                | `true`        |
| `ShowFullErrorToClient`      | `bool`        | Whether players can see total errors (plugin content) or not.         | `false`       |
| `ShowFullErrorToModerators`  | `bool`        | Whether moderators (RA access) can see total errors (plugin content). | `true`        |
| `EnableExamples`             | `bool`        | Whether example menus in-built to the plugin are enabled.             | `true`        |
| `Translation`                | `Translation` | Plugin translation labels and buttons (see below).                    | `Translation` |

### Translation:
| Parameter               | Type                | Description                                    | Default             |
|-------------------------|---------------------|------------------------------------------------|---------------------|
| `OpenMenu`              | `ButtonConfig`      | Open menu button. {0} = menu name.             | `ButtonConfig`      |
| `ReturnTo`              | `ButtonConfig`      | Return to button. {0} = menu name.             | `ButtonConfig`      |
| `ReloadButton`          | `ButtonConfig`      | Reload menu button.                            | `ButtonConfig`      |
| `ServerError`           | `string`            | Text shown when an error is occurrs.           | `string`            |
| `SubMenuTitle`          | `GroupHeaderConfig` | Sub-menus title.                               | `GroupHeaderConfig` |
| `NoPermission`          | `string`            | Error text shown wehn insufficient permission. | `string`            |

### Default YML Config File:
```yml
# Is plugin enabled
is_enabled: true
# Sends debug logs to console
debug: false
# Whether players can see errors or not
show_error_to_client: true
# Whether players can see total errors including plugin content or not
show_full_error_to_client: false
# Whether moderators (RA access) can see total errors including plugin content or not
show_full_error_to_moderators: true
# Whether example menus in-built to the plugin are enabled
enable_examples: true
translation:
# Open menu button. {0} = menu name
  open_menu:
    label: Open {0}
    button_text: Open
    hint: 
  # Return to button. {0} = menu name
  return_to:
    label: Return to {0}
    button_text: Return
    hint: 
  # Reload menu button
  reload_button:
    label: Reload menus
    button_text: Reload
    hint: 
  # Text shown when an error is occurrs
  server_error: Internal Server Error
  # Sub-menus title
  sub_menu_title:
    label: Sub-Menus
    hint: ''
  # Error text shown wehn insufficient permission
  no_permission: Insufficient permissions to view full error details
```

### Want to use in your own project?
To install in your project, simply reference the `KittsMenuSystem.dll` file, found in the [latest release](https://github.com/Kittscloud/KittsMenuSystem/releases/latest).

Create a new class inheriting the `Menu` class.

### Example Menu
```csharp
public class Test : Menu
{
    public override List<BaseSetting> Settings(ReferenceHub hub) => [
        // Settings in here.
    ];

    public override string Name { get; set; } = "Test";
    public override int Id { get; set; } = -1;
}
```

### Menu Class
| Parameter / Method          | Type / Return Type   | Description                                                              |
|-----------------------------|----------------------|--------------------------------------------------------------------------|
| `Name`                      | `string`             | Name of the menu, displayed as header and on buttons. Must be unique.    |
| `Id`                        | `int`                | ID of the menu. Must be greater than 0.                                  |
| `ParentMenu`                | `Type?`              | Optional parent menu type for sub-menus.                                 |
| `Hash`                      | `int`                | Read-only hash derived from `Name` to separate settings.                 |
| `CheckAccess(ReferenceHub)` | `bool`               | Determines whether a player can access the menu. Default returns `true`. |
| `Settings(ReferenceHub)`    | `List<BaseSettings>` | Override to provide hub-specific basesettings.                           |
| `OnOpen(ReferenceHub)`      | `void`               | Called when a player opens the menu.                                     |
| `OnClose(ReferenceHub)`     | `void`               | Called when a player closes the menu.                                    |
| `ReloadFor(ReferenceHub)`   | `void`               | Reloads this menu for a specific player.                                 |
| `ReloadForAll()`            | `void`               | Reloads this menu for all players.                                       |
| `OnRegistered()`            | `void`               | Called when the menu is registered.                                      |

### MenuManager Class
| Parameter / Method                                                | Type / Return Type                             | Description                                                              |
|-------------------------------------------------------------------|------------------------------------------------|--------------------------------------------------------------------------|
| `SyncedMenus`                                                     | `IReadOnlyDictionary<ReferenceHub, Menu>`      | Currently loaded menu for each player.                                   |
| `ResgisteredMenus`                                                | `IReadOnlyList<Menu>`                          | All menus registered in the system.                                      |
| `Pinned`                                                          | `IReadOnlyDictionary<Assembly, List<TextArea>` | All pinned content registered by assemblies.                             |
| `RegisterAllMenus()`                                              | `void`                                         | Registers all menus in the calling assembly.                             |
| `Register(Menu)`                                                  | `void`                                         | Registers a specific menu instance. Throws exception if invalid.         |
| `Unregister(Menu)`                                                | `void`                                         | Unregisters a menu and removes it from all players.                      |
| `UnregisterAllMenus()`                                            | `void`                                         | Unregisters all menus.                                                   |
| `GetCurrentMenu(ReferenceHub)`                                    | `Menu`                                         | Returns the menu the player currently has open.                          |
| `DeleteFromMenuSync(ReferenceHub)`                                | `void`                                         | Removes the player from `MenuSync` when leaving.                         |
| `RegisterTopPinnedSettings(List<BaseSetting>)`                    | `void`                                         | Register list of `BaseSettings` displayed on the top of all `Menus`.     |
| `UnregisterTopPinnedSettings()`                                   | `void`                                         | Remove top pinnedsettings  from `Assembly.GetCallingAssembly`.           |
| `RegisterBottomPinnedSettings(List<BaseSetting>)`                 | `void`                                         | Register list of `BaseSettings` displayed on the bomttom of all `Menus`. |
| `UnregisterBottomPinnedSettings()`                                | `void`                                         | Remove bottom pinned settings from  `Assembly.GetCallingAssembly`.       |
| `GetSetting<TMenu, TSetting>(ReferenceHub, int)`                  | `TSetting`                                     | Gets a `BaseSetting` by Id for a `ReferenceHub` from `TMenu`.            |
| `TryGetSetting<TMenu, TSetting>(ReferenceHub, int, out TSetting)` | `bool`                                         | Trys to get a `BaseSetting` by Id for a `ReferenceHub` from `TMenu`.     |
| `GetCurrentMenu(ReferenceHub)`                                    | `Menu`                                         | Gets the `ReferenceHub's` loaded `Menu`.                                 |
| `GetMenu(Type)`                                                   | `Menu`                                         | Returns a menu instance by type.                                         |
| `ReloadCurrentMenu(ReferenceHub)`                                 | `void`                                         | Reloads the current menu for a player.                                   |
| `ReloadAll()`                                                     | `void`                                         | Reloads current menu for all players.                                    |

You can look at the [Examples](https://github.com/Kittscloud/KittsMenuSystem/tree/main/KittsMenuSystem/Examples) folder to get a better idea of how `Menus` and `Settings` are implemented.

When you enable your plugin, simply run:
```csharp
MenuManager.RegisterAllMenus();
```
This will register all menus in your assembly. It is important to keep `KittsMenuSystem.dll` in the `plugins` folder, as it must run as a plugin in order to register menus from all assemblies with `KittsMenuSystem.dll`.

## Found a bug or have feedback?
If you have found a bug please make an issue on GitHub or the quickest way is to message me on discord at `kittscloud`.

Also message me on discord if you have feedback for me, I'd appreciate it very much. Thank you!
