using Mirror;
using System;
using TMPro;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Wrappers;

/// <summary>
/// Initialize new <see cref="TextBox"/> setting with base <see cref="SSPlaintextSetting"/> that calls <see cref="Action"/> when changed.
/// </summary>
/// <remarks>
/// Initialize new <see cref="TextBox"/>.
/// </remarks>
/// <param name="id">Id of <see cref="SSPlaintextSetting"/>.</param>
/// <param name="label">Label of <see cref="SSPlaintextSetting"/>.</param>
/// <param name="onChanged">Triggers <see cref="Action"/> when <see cref="SSPlaintextSetting"/> changed.</param>
/// <param name="placeholder"><see cref="SSPlaintextSetting.Placeholder"/> shown if content is empty.</param>
/// <param name="characterLimit">Sets <see cref="SSPlaintextSetting.CharacterLimit"/>.</param>
/// <param name="contentType">Type of content taken.</param>
/// <param name="hint">Hint of <see cref="SSPlaintextSetting"/>.</param>
public class TextBox(int? id, string label, Action<ReferenceHub, string, SSPlaintextSetting> onChanged = null, string placeholder = "...", int characterLimit = 64, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, string hint = null) 
    : BaseSetting(new SSPlaintextSetting(SetValidId(id, label), label, placeholder, characterLimit, contentType, hint))
{
    /// <summary>
    /// Initialize new <see cref="TextBox"/> setting (automatic id) with base <see cref="SSPlaintextSetting"/> that calls <see cref="Action"/> when changed.
    /// </summary>
    public TextBox(string label, Action<ReferenceHub, string, SSPlaintextSetting> onChanged = null, string placeholder = "...", int characterLimit = 64, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, string hint = null)
        : this(null, label, onChanged, placeholder, characterLimit, contentType, hint) { }

    /// <summary>
    /// Method called when value changed: <br></br>
    /// - <see cref="ReferenceHub"/> that updated the value.<br></br>
    /// - <see cref="string"/> (New Value)<br></br>
    /// - <see cref="SSPlaintextSetting"/> (Synced Class).
    /// </summary>
    public Action<ReferenceHub, string, SSPlaintextSetting> OnChanged { get; } = onChanged;

    /// <summary>
    /// Makes sure the Id is valid (not null)
    /// </summary>
    internal static int SetValidId(int? id, string label) =>
        id ?? (label + "TextBox").GetStableHashCode();
}
