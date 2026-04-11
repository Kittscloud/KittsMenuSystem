using Mirror;
using System;
using TMPro;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Settings;

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
/// <param name="contentType">Sets <see cref="SSPlaintextSetting.ContentType"/>.</param>
/// <param name="hint">Hint of <see cref="SSPlaintextSetting"/>.</param>
public class TextBox(int? id, string label, Action<ReferenceHub, string, SSPlaintextSetting> onChanged = null, string placeholder = "...", int characterLimit = 64, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, string hint = null)
    : BaseSetting(new SSPlaintextSetting(id ?? (label + nameof(TextBox)).GetStableHashCode(), label, placeholder, characterLimit, contentType, hint))
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
    /// Shortcut to underlying <see cref="SSPlaintextSetting.Placeholder"/>.
    /// </summary>
    public string Placeholder
    {
        get => (Base as SSPlaintextSetting).Placeholder;
        set => UpdatePlaceholder(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="SSPlaintextSetting.SendPlaintextUpdate"/> but only for <see cref="SSPlaintextSetting.Placeholder"/>.
    /// <see cref="SSPlaintextSetting.Placeholder"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdatePlaceholder(string newPlaceholder, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) =>
        (Base as SSPlaintextSetting).SendPlaintextUpdate(newPlaceholder, (ushort)CharacterLimit, ContentType, applyOverride, receiveFilter);

    /// <summary>
    /// Shortcut to underlying <see cref="SSPlaintextSetting.CharacterLimit"/>.
    /// </summary>
    public int CharacterLimit
    {
        get => (Base as SSPlaintextSetting).CharacterLimit;
        set => UpdateCharacterLimit(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="SSPlaintextSetting.SendPlaintextUpdate"/> but only for <see cref="SSPlaintextSetting.CharacterLimit"/>.
    /// <see cref="SSPlaintextSetting.CharacterLimit"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdateCharacterLimit(int newCharacterLimit, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) =>
        (Base as SSPlaintextSetting).SendPlaintextUpdate(Placeholder, (ushort)newCharacterLimit, ContentType, applyOverride, receiveFilter);

    /// <summary>
    /// Shortcut to underlying <see cref="SSPlaintextSetting.ContentType"/>.
    /// </summary>
    public TMP_InputField.ContentType ContentType
    {
        get => (Base as SSPlaintextSetting).ContentType;
        set => UpdateContentType(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="SSPlaintextSetting.SendPlaintextUpdate"/> but only for <see cref="SSPlaintextSetting.ContentType"/>.
    /// <see cref="SSPlaintextSetting.ContentType"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdateContentType(TMP_InputField.ContentType newContentType, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) =>
        (Base as SSPlaintextSetting).SendPlaintextUpdate(Placeholder, (ushort)CharacterLimit, newContentType, applyOverride, receiveFilter);
}
