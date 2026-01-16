using KittsMenuSystem.Features.Interfaces;
using System;
using TMPro;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Wrappers;

/// <summary>
/// Initialize new wrapper for <see cref="SSKeybindSetting"/>. Setting binds a key to an action.
/// </summary>
/// <remarks>
/// Initialize new instance of <see cref="Keybind"/>.
/// </remarks>
/// <param name="id">Id of <see cref="Keybind"/>.</param>
/// <param name="label">Label of <see cref="Keybind"/>.</param>
/// <param name="onUsed">Method when keybind pressed.</param>
/// <param name="suggestedKey">Key suggest to player, not the default.</param>
/// <param name="preventInteractionOnGui">Prevent keybind when GUI (Menus, RA, etc) is open.</param>
/// <param name="allowSpectatorTrigger">All spectators to trigger</param>
/// <param name="hint">Hint of <see cref="Keybind"/>.</param>
public class Keybind(int? id, string label, Action<ReferenceHub, bool, SSKeybindSetting> onUsed = null, KeyCode suggestedKey = KeyCode.None, bool preventInteractionOnGui = true, bool allowSpectatorTrigger = true, string hint = null) 
    : SSKeybindSetting(id, label, suggestedKey, preventInteractionOnGui, allowSpectatorTrigger, hint), ISetting
{
    /// <summary>
    /// Initialize new instance of <see cref="Keybind"/>.
    /// </summary>
    /// <param name="label">Label of <see cref="Keybind"/>.</param>
    /// <param name="onUsed">Method when keybind pressed.</param>
    /// <param name="suggestedKey">Key suggest to player, not the default.</param>
    /// <param name="preventInteractionOnGui">Prevent keybind when GUI (Menus, RA, etc) is open.</param>
    /// <param name="allowSpectatorTrigger">All spectators to trigger</param>
    /// <param name="hint">Hint of <see cref="Keybind"/>.</param>
    public Keybind(string label, Action<ReferenceHub, bool, SSKeybindSetting> onUsed = null, KeyCode suggestedKey = KeyCode.None, bool preventInteractionOnGui = true, bool allowSpectatorTrigger = true, string hint = null)
        : this(null, label, onUsed, suggestedKey, preventInteractionOnGui, allowSpectatorTrigger, hint) { }

    /// <summary>
    /// Method called when keybind pressed: <br></br><br></br>
    /// - <see cref="ReferenceHub"/> that pressed the keybind<br></br>
    /// </summary>
    public Action<ReferenceHub, bool, SSKeybindSetting> Action { get; } = onUsed;

    /// <summary>
    /// The base instance (client).
    /// </summary>
    public ServerSpecificSettingBase Base { get; } = new SSKeybindSetting(id, label, suggestedKey, preventInteractionOnGui, allowSpectatorTrigger, hint);
}