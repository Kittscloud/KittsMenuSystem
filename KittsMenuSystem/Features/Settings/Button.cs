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
/// <param name="buttonText">Text of <see cref="SSButton.ButtonText"/>.</param>
/// <param name="onPressed">Triggers <see cref="Action"/> when <see cref="Button"/> pressed.</param>
/// <param name="holdTimeSeconds">Sets <see cref="SSButton.HoldTimeSeconds"/>.</param>
/// <param name="hint">Hint of <see cref="SSButton"/>.</param>
public class Button(int? id, string label, string buttonText, Action<ReferenceHub, SSButton> onPressed = null, float? holdTimeSeconds = null, string hint = null)
    : BaseSetting(new SSButton(id ?? Guid.NewGuid().ToString().GetStableHashCode(), label, buttonText, holdTimeSeconds, hint))
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
    /// Shortcut to underlying <see cref="SSButton.ButtonText"/>.
    /// </summary>
    public string ButtonText
    {
        get => (Base as SSButton).ButtonText;
        set => UpdateButtonText(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="SSButton.SendButtonUpdate"/> but only for <see cref="SSButton.ButtonText"/>.
    /// <see cref="SSButton.ButtonText"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdateButtonText(string newButtonText, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) =>
        (Base as SSButton).SendButtonUpdate(newButtonText, HoldTimeSeconds, applyOverride, receiveFilter);

    /// <summary>
    /// Shortcut to underlying <see cref="SSButton.HoldTimeSeconds"/>.
    /// </summary>
    public float HoldTimeSeconds
    {
        get => (Base as SSButton).HoldTimeSeconds;
        set => UpdateHoldTimeSeconds(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="SSButton.SendButtonUpdate"/> but only for <see cref="SSButton.HoldTimeSeconds"/>.
    /// <see cref="SSButton.HoldTimeSeconds"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdateHoldTimeSeconds(float newHoldTimeSeconds, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) =>
        (Base as SSButton).SendButtonUpdate(ButtonText, newHoldTimeSeconds, applyOverride, receiveFilter);
}
