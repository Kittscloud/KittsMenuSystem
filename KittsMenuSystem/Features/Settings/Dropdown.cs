using Mirror;
using System;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Settings;

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
    : BaseSetting(new SSDropdownSetting(id ?? Guid.NewGuid().ToString().GetStableHashCode(), label, options, defaultOptionIndex, entryType, hint))
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
    /// Shortcut to underlying <see cref="SSDropdownSetting.Options"/>.
    /// </summary>
    public string[] Options
    {
        get => (Base as SSDropdownSetting).Options;
        set => UpdateOptions(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="SSDropdownSetting.SendDropdownUpdate"/>.
    /// <see cref="SSDropdownSetting.Options"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdateOptions(string[] newOptions, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) =>
        (Base as SSDropdownSetting).SendDropdownUpdate(newOptions, applyOverride, receiveFilter);
}
