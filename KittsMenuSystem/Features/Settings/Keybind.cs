using Mirror;
using System;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Settings;

/// <summary>
/// Initialize new <see cref="Keybind"/> setting with base <see cref="SSKeybindSetting"/> that calls <see cref="Action"/> when used.
/// </summary>
/// <remarks>
/// Initialize new instance of <see cref="Keybind"/>.
/// </remarks>
/// <param name="id">Id of <see cref="SSKeybindSetting"/>.</param>
/// <param name="label">Label of <see cref="SSKeybindSetting"/>.</param>
/// <param name="onUsed">Triggers <see cref="Action"/> when <see cref="Keybind"/> used.</param>
/// <param name="suggestedKey">Sets <see cref="SSKeybindSetting.SuggestedKey"/>.</param>
/// <param name="preventInteractionOnGui">Sets <see cref="SSKeybindSetting.PreventInteractionOnGUI"/>, will not work when in settings unless in the menu with the keybind.</param>
/// <param name="allowSpectatorTrigger">Sets <see cref="SSKeybindSetting.AllowSpectatorTrigger"/></param>
/// <param name="hint">Hint of <see cref="SSKeybindSetting"/>.</param>
public class Keybind(int? id, string label, Action<ReferenceHub, bool, SSKeybindSetting> onUsed = null, KeyCode suggestedKey = KeyCode.None, bool preventInteractionOnGui = true, bool allowSpectatorTrigger = true, string hint = null)
    : BaseSetting(new SSKeybindSetting(SetValidId(id, label), label, suggestedKey, preventInteractionOnGui, allowSpectatorTrigger, hint))
{
    /// <summary>
    /// Initialize new <see cref="Keybind"/> setting (automatic id) with base <see cref="SSKeybindSetting"/> that calls <see cref="Action"/> when used.
    /// </summary>
    /// <param name="label">Label of <see cref="SSKeybindSetting"/>.</param>
    /// <param name="onUsed">Triggers <see cref="Action"/> when <see cref="Keybind"/> used.</param>
    /// <param name="suggestedKey">Sets <see cref="SSKeybindSetting.SuggestedKey"/>.</param>
    /// <param name="preventInteractionOnGui">Sets <see cref="SSKeybindSetting.PreventInteractionOnGUI"/>.</param>
    /// <param name="allowSpectatorTrigger">Sets <see cref="SSKeybindSetting.AllowSpectatorTrigger"/></param>
    /// <param name="hint">Hint of <see cref="SSKeybindSetting"/>.</param>
    public Keybind(string label, Action<ReferenceHub, bool, SSKeybindSetting> onUsed = null, KeyCode suggestedKey = KeyCode.None, bool preventInteractionOnGui = true, bool allowSpectatorTrigger = true, string hint = null)
        : this(null, label, onUsed, suggestedKey, preventInteractionOnGui, allowSpectatorTrigger, hint) { }

    /// <summary>
    /// Method called when used: <br></br>
    /// - <see cref="ReferenceHub"/> that used the keybind.<br></br>
    /// - <see cref="bool"/> (If Keybind is pressed)<br></br>
    /// - <see cref="SSKeybindSetting"/> (Synced Class).
    /// </summary>
    public Action<ReferenceHub, bool, SSKeybindSetting> OnUsed { get; } = onUsed;

    /// <summary>
    /// Makes sure the Id is valid (not null)
    /// </summary>
    internal static int SetValidId(int? id, string label) =>
        id ?? (label + "Keybind").GetStableHashCode();
}
