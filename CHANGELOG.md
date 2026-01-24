# KittsMenuSystem Changelog

## Version 0.4.0
- Added `BuildSettings` function - Used to build original settings.
- Added `RebuildSettings` function - Used to rebuild the settings.
- Added `GenerateSettings` function - Used to generate settings to be built.
- Added `DefinitionCache` - Used to store original definitions.
- Added `OriginalDefinition` patch - Used to get the GetOriginalDefinitio stored in the `DefinitionCache`.
- Added `RestoreFromOriginal` function - Used to store original definition when `OnSettingReceived` is called.
- Added `GlobalMenu` - Loaded when settings are closed and conatins all settings from all menus.
- Added `PinnedTopSettings` - List of `BaseSetting` pinned to the top of all menus.
- Added `PinnedBottomSettings` - List of `BaseSetting` pinned to the bottom of all menus.
- Added `RegisterTopPinnedSettings` function - Registers the `PinnedTopSettings`.
- Added `UnregisterTopPinnedSettings` function - Unregisters the `PinnedTopSettings`.
- Added `RegisterBottomPinnedSettings` function - Registers the `PinnedBottomSettings`.
- Added `UnregisterBottomPinnedSettings` function - Unregisters the `PinnedBottomSettings`.
- Updated `GetSetting` function - Moved to `Menu.cs`, is per menu and returns a dummy setting if null.
- Updated `GetSettings` function - Now returns `BuiltSettings`, populating if empty, with options to rebuild and call settings.
- Removed `Pinned` - Moved to `PinnedTopSettings` and `PinnedBottomSettings`.
- Removed `RegisterPins` - Moved to `RegisterTopPinnedSettings` and `RegisterBottomPinnedSettings`.
- Removed `UnregisterAllPins` - Moved to `UnregisterTopPinnedSettings` and `UnregisterBottomPinnedSettings`.
- Removed `LockedAssembly` - No longer needed.
- Removed `Load` function - No longer needed.
- Removed `SyncMenu` function - No longer needed.
- Removed `SyncAllMenus` function - No longer needed.
- Removed `Wrap` function - No longer needed.
- Removed `KeybindMenu` - No longer needed.
- Removed `SyncCache` - No longer needed.
- Removed `SetDefinedSettings` patch - No longer needed.
- Removed `GetDefinedSettings` patch - No longer needed.
- Removed `SendToPlayer` patch - No longer needed.
- Removed `SendToPlayer` patch - No longer needed.
- Removed `PlaintextLimit` patch function - No longer needed.

## Version 0.3.4
- Fixed `GetSetting` function - Fixed a bug where function could not find setting while SSSetting was in the unhashed state.

## Version 0.3.3
- Fixed `SSSetings`- Fixed a bug where the returns `SSSetings` from a `BaseSetting` would be the hashed id.

## Version 0.3.2
- Fixed `Versions` - Forgot to update the version of the plugin, rookie mistake.
- Updated `EventHandler.cs` - Now named `MenuEvents.cs`.
- Updated `Dependencies` - Removed unnecessary dependencies.
- Removed `ReloadMenu` function - Don't see a use for it.

## Version 0.3.1
- Fixed `KeybindMenu` - Fixed an issue where keybind menu was loading when in settings.
- Fixed `Server-Specific` - Fixed an issue where pressing the `Server-Specific` tab while in the `Server-Sepcific` tab would cause an error.
- Fixed `Config` - Fixed unused configurations in the config file.

## Version 0.3.0
- Added `KeybindMenu` - Used to load all keybinds when player not in settings.
- Added `OnStatusReceived` function - Updates the settings to and from `KeybindMenu` when player not in settings tab.
- Updated `KittsMenuSystem.Features.Wrappers` namespace - Now named `KittsMenuSystem.Features.Settings`.
- Removed `All Keybinds` on all `Menus` - No longer needed due to `KeybindMenu`. 

## Version 0.2.0
- Added `AccessExmaple.cs` - Exmaple menu where you don't have access (literally pointless).
- Added `BaseSetting` - All `Wrappers` now use `BaseSetting` instead of `ServerSpecificSettingBase` and all functions have been updated to work with `BaseSetting`.
- Added `CentralMainMenu.cs` - Used when multple main menus.
- Added `Patches.cs` - Contains `GetDefinedSettings`, `SetDefinedSettings` `SendToPlayer` and`Prevalidate` and `PlaintextLimit` patches.
- Added `Keybinds` everywhere - Keybinds are always added at the bottom of all menus so they can be used everywhere .
- Updated Exmaples - Uses `BaseSetting` and updated `Menu`.
- Updated `LICENSE` - License is now `AGPL v3.0`.
- Updated `README.md` - Changed some information.
- Updated `OnSettingReceived` fucntion - Now works with `BaseSetting` and is more simple.
- Updated `Wrappers` - Updated all `Wrappers` to use `BaseSetting`. 
- Updated `SettingSync` - Now named `SyncedSettings`, list of `BaseSetting` and set to `internal`.
- Updated `MenuRelated` - Now named `ParentMenu`.
- Updated `SentSettings` - Now named `BuiltSettings` and is a list of `BaseSetting`.
- Updated `GetSettings` fucntion - Now builds pins, buttons and settings as `BaseSetting`.
- Updated `ProperlyEnable` fucntion - Now named `OnOpen`
- Updated `ProperlyDisable` fucntion - Now named `OnClose`
- Updated `GetParamter` function - Now named `GetSetting`
- Updated `MenuSync` - Now named `SyncedMenus` and can no longer be mutated.
- Updated `LoadedMenus` - Now named `RegisteredMenus` and can no longer be mutated.
- Updated `Pinned` - Can no longer be mutated.
- Updated `RegisterAll` function - Now named `RegisterAllMenus`.
- Updated `UnregisterAll` function - Now named `UnregisterAllMenus`.
- Updated `Register` function - Now `internal`.
- Updated `SendMenu` function - Now named `SendSettings` and converts list of `BaseSetting` to `ServerSpecificSettingBase` before sending.
- Moved `Log.cs` - Now located in the `Features` folder.
- Moved `MenuManager.cs` - Now located in the `Menus` folder.
- Moved `AssemblyMenu.cs` - Now located in the `Menus` folder.
- Moved `Menu.cs` - Now located in the `Menus` folder.
- Moved `Parameters.cs` - Moved functions to `MenuManager.cs`.
- Moved `Wrappers` - Moved all wrappers to `Settings` folder.
- Removed `OnInput` fucntion - Don't see a use for it.
- Removed `TryGetSubMenu` function - Sub menu buttons now have an `Action` going straight to sub menu.
- Removed `GetSettingFor` function - Now uses the override `Settings` function.
- Removed `Settings` override - Now uses the override `Settings` function.
- Removed `InternalSettingsSync` - Now uses `SyncedSettings`.
- Removed `QueueOrRegister` function - Don't see a use for it.
- Removed `ISetting` interface.
- Removed `AllowPinnedContent` from `config` - Pinned content always allowed.
- Removed `CompatibilityEnabled ` from `config` - Compatibility always enabled.
- Removed `ForceMainMenuEvenIfOnlyOne` from `config` -  Central main menu only shows when multple menus.
- Removed `ReturnToMenu` from `config` - Use `ReturnTo` instead.
- Removed `Compatibilizer.cs` - No longer needed or moved.
- Removed `CompatibilizerGetter.cs` - No longer needed or moved.
- Removed `SendToPlayerDSPatch.cs` - No longer needed or moved.
- Removed `SendToPlayer.cs` - No longer needed or moved.
- Removed `SetIdPatch.cs` - No longer needed or moved.
- Removed `OriginalDefinition.cs` - No longer needed or moved.
- Removed `PrevalidateResponsePatch.cs` - No longer needed or moved.
- Removed `TemporaryPatch.cs` - No longer needed or moved.
