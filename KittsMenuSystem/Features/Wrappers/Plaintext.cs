using Hints;
using KittsMenuSystem.Features.Interfaces;
using System;
using System.Reflection.Emit;
using TMPro;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Wrappers;

/// <summary>
/// Initialize new wrapper for <see cref="SSPlaintextSetting"/>. Setting creates an input text box.
/// </summary>
/// <remarks>
/// Initialize new <see cref="Plaintext"/>.
/// </remarks>
/// <param name="id">Id of <see cref="Plaintext"/>.</param>
/// <param name="label">Label of <see cref="Plaintext"/>.</param>
/// <param name="onChanged">Triggered when the player updates the value.</param>
/// <param name="placeholder">Value shown if content is empty.</param>
/// <param name="characterLimit">Maximum characters.</param>
/// <param name="contentType">Type of content taken.</param>
/// <param name="hint">Hint of <see cref="Plaintext"/>.</param>
public class Plaintext(int? id, string label, Action<ReferenceHub, string, SSPlaintextSetting> onChanged = null, string placeholder = "...", int characterLimit = 64, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, string hint = null) 
    : SSPlaintextSetting(id, label, placeholder, characterLimit, contentType, hint), ISetting
{
    /// <summary>
    /// Initialize new <see cref="Plaintext"/>.
    /// </summary>
    /// <param name="label">Label of <see cref="Plaintext"/>.</param>
    /// <param name="onChanged">Triggered when the player updates the value.</param>
    /// <param name="placeholder">Value shown if content is empty.</param>
    /// <param name="characterLimit">Maximum characters.</param>
    /// <param name="contentType">Type of content taken.</param>
    /// <param name="hint">Hint of <see cref="Plaintext"/>.</param>
    public Plaintext(string label, Action<ReferenceHub, string, SSPlaintextSetting> onChanged = null, string placeholder = "...", int characterLimit = 64, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, string hint = null)
        : this(null, label, onChanged, placeholder, characterLimit, contentType, hint) { }

    /// <summary>
    /// Method called when value updated: <br></br><br></br>
    /// - <see cref="ReferenceHub"/> that updated the value.<br></br>
    /// - <see cref="string"/> (New Value)./><br></br>
    /// - <see cref="SSPlaintextSetting"/> (Synced Class).
    /// </summary>
    public Action<ReferenceHub, string, SSPlaintextSetting> OnChanged { get; } = onChanged;

    /// <summary>
    /// The base instance (sent in to the client).
    /// </summary>
    public ServerSpecificSettingBase Base { get; set; } = new SSPlaintextSetting(id, label, placeholder, characterLimit, contentType, hint);
}