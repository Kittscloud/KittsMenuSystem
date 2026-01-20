using Mirror;
using System;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Wrappers;

/// <summary>
/// Initialize new <see cref="Dropdown"/> setting with base <see cref="SSDropdownSetting"/> that calls <see cref="Action"/> when changed.
/// </summary>
/// <remarks>
/// Initialize new <see cref="Dropdown"/>.
/// </remarks>
/// <param name="id">Id of <see cref="SSDropdownSetting"/>.</param>
/// <param name="label">Label of <see cref="SSDropdownSetting"/>.</param>
/// <param name="options">Options of <see cref="SSDropdownSetting"/>.</param>
/// <param name="onChanged">Triggers <see cref="Action"/> when index of <see cref="SSDropdownSetting"/> changed.</param>
/// <param name="defaultOptionIndex">Sets <see cref="SSDropdownSetting.DefaultOptionIndex"/>.</param>
/// <param name="entryType">Sets <see cref="SSDropdownSetting.EntryType"/>.</param>
/// <param name="hint">Hint of <see cref="SSDropdownSetting"/>.</param>
public class Dropdown(int? id, string label, string[] options, Action<ReferenceHub, int, SSDropdownSetting> onChanged = null, int defaultOptionIndex = 0, SSDropdownSetting.DropdownEntryType entryType = SSDropdownSetting.DropdownEntryType.Regular, string hint = null) 
    : BaseSetting(new SSDropdownSetting(SetValidId(id, label), label, options, defaultOptionIndex, entryType, hint))
{
    /// <summary>
    /// Initialize new <see cref="Dropdown"/> setting (automatic id) with base <see cref="SSDropdownSetting"/> that calls <see cref="Action"/> when changed.
    /// </summary>
    /// <param name="label">Label of <see cref="SSDropdownSetting"/>.</param>
    /// <param name="options">Options of <see cref="SSDropdownSetting"/>.</param>
    /// <param name="onChanged">Triggers <see cref="Action"/> when index of <see cref="SSDropdownSetting"/> changed.</param>
    /// <param name="defaultOptionIndex">Sets <see cref="SSDropdownSetting.DefaultOptionIndex"/>.</param>
    /// <param name="entryType">Sets <see cref="SSDropdownSetting.EntryType"/>.</param>
    /// <param name="hint">Hint of <see cref="SSDropdownSetting"/>.</param>
    public Dropdown(string label, string[] options, Action<ReferenceHub, int, SSDropdownSetting> onChanged = null, int defaultOptionIndex = 0, SSDropdownSetting.DropdownEntryType entryType = SSDropdownSetting.DropdownEntryType.Regular, string hint = null)
        : this(null, label, options, onChanged, defaultOptionIndex, entryType, hint) { }

    /// <summary>
    /// Method called when index changed: <br></br>
    /// - <see cref="ReferenceHub"/> that selected a new index.<br></br>
    /// - <see cref="int"/> (New Selected Index)<br></br>
    /// - <see cref="SSDropdownSetting"/> (Synced Class).
    /// </summary>
    public Action<ReferenceHub, int, SSDropdownSetting> OnChanged { get; } = onChanged;

    /// <summary>
    /// Makes sure the Id is valid (not null)
    /// </summary>
    internal static int SetValidId(int? id, string label) =>
        id ?? (label + "Button").GetStableHashCode();
}
