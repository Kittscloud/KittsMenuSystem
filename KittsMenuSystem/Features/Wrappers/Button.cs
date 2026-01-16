using System;
using KittsMenuSystem.Features.Interfaces;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Wrappers;

/// <summary>
/// Initialize new wrapper for <see cref="SSButton"/>. Setting calls <see cref="Action"/> when pressed.
/// </summary>
/// <remarks>
/// Initialize new <see cref="Button"/>.
/// </remarks>
/// <param name="id">Id of <see cref="Button"/>.</param>
/// <param name="label">Label of <see cref="Button"/>.</param>
/// <param name="buttonText">Text of <see cref="Button"/>.</param>
/// <param name="onClick">Method when <see cref="Button"/> pressed.</param>
/// <param name="holdTimeSeconds">Time in seconds before method is called.</param>
/// <param name="hint">Hint of <see cref="Button"/>.</param>
public class Button(int? id, string label, string buttonText, Action<ReferenceHub, SSButton> onClick, float? holdTimeSeconds = null, string hint = null)
    : SSButton(id, label, buttonText,  holdTimeSeconds, hint), ISetting
{
    /// <summary>
    /// Initialize new <see cref="Button"/>.
    /// </summary>
    /// <param name="label">Label of <see cref="Button"/>.</param>
    /// <param name="buttonText">Text of <see cref="Button"/>.</param>
    /// <param name="onClick">Method when <see cref="Button"/> pressed.</param>
    /// <param name="holdTimeSeconds">Time in seconds before method is called.</param>
    /// <param name="hint">Hint of <see cref="Button"/>.</param>
    public Button(string label, string buttonText, Action<ReferenceHub, SSButton> onClick, float? holdTimeSeconds = null, string hint = null)
        : this(null, label, buttonText, onClick, holdTimeSeconds, hint) { }

    /// <summary>
    /// The mehtod executed when pressed: <br></br><br></br>
    /// - <see cref="ReferenceHub"/> that pressed the button.<br></br>
    /// - <see cref="SSButton"/> (Synced Class).
    /// </summary>
    public Action<ReferenceHub, SSButton> Action { get; } = onClick;

    /// <summary>
    /// The base instance (client).
    /// </summary>
    public ServerSpecificSettingBase Base { get; } = new SSButton(id, label, buttonText, holdTimeSeconds, hint);
}
