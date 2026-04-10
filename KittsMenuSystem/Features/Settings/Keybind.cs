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
/// <param name="onPressed">Triggers <see cref="Action"/> when <see cref="Keybind"/> is being pressed.</param>
/// <param name="suggestedKey">Sets <see cref="SSKeybindSetting.SuggestedKey"/>.</param>
/// <param name="preventInteractionOnGui">Sets <see cref="SSKeybindSetting.PreventInteractionOnGUI"/>, will not work when in settings unless in the menu with the keybind.</param>
/// <param name="allowSpectatorTrigger">Sets <see cref="SSKeybindSetting.AllowSpectatorTrigger"/></param>
/// <param name="hint">Hint of <see cref="SSKeybindSetting"/>.</param>
public class Keybind(int? id, string label, Action<ReferenceHub, bool, SSKeybindSetting> onUsed = null, Action<ReferenceHub, SSKeybindSetting> onPressed = null, KeyCode suggestedKey = KeyCode.None, bool preventInteractionOnGui = true, bool allowSpectatorTrigger = true, string hint = null)
    : BaseSetting(new SSKeybindSetting(id ?? Guid.NewGuid().ToString().GetStableHashCode(), label, suggestedKey, preventInteractionOnGui, allowSpectatorTrigger, hint))
{
    /// <summary>
    /// Initialize new <see cref="Keybind"/> setting (automatic id).
    /// </summary>
    public Keybind(string label, KeyCode suggestedKey = KeyCode.None, bool preventInteractionOnGui = true, bool allowSpectatorTrigger = true, string hint = null)
        : this(null, label, null, null, suggestedKey, preventInteractionOnGui, allowSpectatorTrigger, hint) { }

    /// <summary>
    /// Initialize new <see cref="Keybind"/> with OnUsed callback.
    /// </summary>
    public Keybind(string label, Action<ReferenceHub, bool, SSKeybindSetting> onUsed, KeyCode suggestedKey = KeyCode.None, bool preventInteractionOnGui = true, bool allowSpectatorTrigger = true, string hint = null)
        : this(null, label, onUsed, null, suggestedKey, preventInteractionOnGui, allowSpectatorTrigger, hint) { }

    /// <summary>
    /// Initialize new <see cref="Keybind"/> with OnPressed callback.
    /// </summary>
    public Keybind(string label, Action<ReferenceHub, SSKeybindSetting> onPressed, KeyCode suggestedKey = KeyCode.None, bool preventInteractionOnGui = true, bool allowSpectatorTrigger = true, string hint = null)
        : this(null, label, null, onPressed, suggestedKey, preventInteractionOnGui, allowSpectatorTrigger, hint) { }

    /// <summary>
    /// Method called when used: <br></br>
    /// - <see cref="ReferenceHub"/> that used the keybind.<br></br>
    /// - <see cref="bool"/> (If the <see cref="SSKeybindSetting"/> is being pressed)<br></br>
    /// - <see cref="SSKeybindSetting"/> (Synced Class).
    /// </summary>
    /// <remarks>
    /// The <see cref="SSKeybindSetting"/> is always being used, only the <see cref="SSKeybindSetting.SyncIsPressed"/> is changing.
    /// </remarks>
    public Action<ReferenceHub, bool, SSKeybindSetting> OnUsed { get; }
        = onUsed ?? ((hub, isPressed, setting) =>
        {
            if (isPressed)
                onPressed?.Invoke(hub, setting);
        });

    /// <summary>
    /// Method called when pressed: <br></br>
    /// - <see cref="ReferenceHub"/> that used the keybind.<br></br>
    /// - <see cref="SSKeybindSetting"/> (Synced Class).
    /// </summary>
    public Action<ReferenceHub, SSKeybindSetting> OnPressed { get; } = onPressed;
}