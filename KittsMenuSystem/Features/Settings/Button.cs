using Mirror;
using System;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Settings;

/// <summary>
/// Initialize new <see cref="Button"/> setting with base <see cref="SSButton"/> that calls <see cref="Action"/> when pressed.
/// </summary>
/// <remarks>
/// Initialize new <see cref="Button"/>.
/// </remarks>
/// <param name="id">Id of <see cref="SSButton"/>.</param>
/// <param name="label">Label of <see cref="SSButton"/>.</param>
/// <param name="buttonText">Text of <see cref="SSButton"/>.</param>
/// <param name="onPressed">Triggers <see cref="Action"/> when <see cref="Button"/> pressed.</param>
/// <param name="holdTimeSeconds">Sets <see cref="SSButton.HoldTimeSeconds"/>.</param>
/// <param name="hint">Hint of <see cref="SSButton"/>.</param>
public class Button(int? id, string label, string buttonText, Action<ReferenceHub, SSButton> onPressed = null, float? holdTimeSeconds = null, string hint = null)
    : BaseSetting(new SSButton(SetValidId(id, label), label, buttonText, holdTimeSeconds, hint))
{
    /// <summary>
    /// Initialize new <see cref="Button"/> setting (automatic id) with base <see cref="SSButton"/> that calls <see cref="Action"/> when pressed.
    /// </summary>
    /// <param name="label">Label of <see cref="SSButton"/>.</param>
    /// <param name="buttonText">Text of <see cref="SSButton"/>.</param>
    /// <param name="onPressed">Triggers <see cref="Action"/> when <see cref="Button"/> pressed.</param>
    /// <param name="holdTimeSeconds">Sets <see cref="SSButton.HoldTimeSeconds"/>.</param>
    /// <param name="hint">Hint of <see cref="SSButton"/>.</param>
    public Button(string label, string buttonText, Action<ReferenceHub, SSButton> onPressed, float? holdTimeSeconds = null, string hint = null)
        : this(null, label, buttonText, onPressed, holdTimeSeconds, hint) { }

    /// <summary>
    /// Method called when pressed: <br></br>
    /// - <see cref="ReferenceHub"/> that pressed the button.<br></br>
    /// - <see cref="SSButton"/> (Synced Class).
    /// </summary>
    public Action<ReferenceHub, SSButton> OnPressed { get; } = onPressed;

    /// <summary>
    /// Makes sure the Id is valid (not null)
    /// </summary>
    internal static int SetValidId(int? id, string label) =>
        id ?? (label + "Button").GetStableHashCode();
}
