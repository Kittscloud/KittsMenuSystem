using KittsMenuSystem.Features.Interfaces;
using System;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Wrappers;

/// <summary>
/// Initialize new wrapper for <see cref="SSDropdownSetting"/>. Setting makes a dropdown where an input is selected.
/// </summary>
/// <remarks>
/// Initialize new <see cref="Dropdown"/>.
/// </remarks>
/// <param name="id">Id of <see cref="Dropdown"/>.</param>
/// <param name="label">Label of <see cref="Dropdown"/>.</param>
/// <param name="options">Options avaiable.</param>
/// <param name="onChanged">Method when index changed.</param>
/// <param name="defaultOptionIndex">Defaulted selected index.</param>
/// <param name="entryType">Set the entry type.</param>
/// <param name="hint">Hint of <see cref="Dropdown"/>.</param>
public class Dropdown(int? id, string label, string[] options, Action<ReferenceHub, int, SSDropdownSetting> onChanged = null, int defaultOptionIndex = 0, SSDropdownSetting.DropdownEntryType entryType = SSDropdownSetting.DropdownEntryType.Regular, string hint = null) 
    : SSDropdownSetting(id, label, options, defaultOptionIndex, entryType, hint), ISetting
{
    /// <summary>
    /// Initialize new <see cref="Dropdown"/>.
    /// </summary>
    /// <param name="label">Label of <see cref="Dropdown"/>.</param>
    /// <param name="options">Options avaiable.</param>
    /// <param name="onChanged">Method when index changed.</param>
    /// <param name="defaultOptionIndex">Defaulted selected index.</param>
    /// <param name="entryType">Set the entry type.</param>
    /// <param name="hint">Hint of <see cref="Dropdown"/>.</param>
    public Dropdown(string label, string[] options, Action<ReferenceHub, int, SSDropdownSetting> onChanged = null, int defaultOptionIndex = 0, SSDropdownSetting.DropdownEntryType entryType = SSDropdownSetting.DropdownEntryType.Regular, string hint = null)
        : this(null, label, options, onChanged, defaultOptionIndex, entryType, hint) { }

    /// <summary>
    /// Method called when index updated: <br></br><br></br>
    /// - <see cref="ReferenceHub"/> that selected a new index.<br></br>
    /// - <see cref="int"/> (New Selected Index).<br></br>
    /// - <see cref="SSPlaintextSetting"/> (Synced Class).
    /// </summary>
    public Action<ReferenceHub, int, SSDropdownSetting> Action { get; } = onChanged;

    /// <summary>
    /// The base instance (client).
    /// </summary>
    public ServerSpecificSettingBase Base { get; set; } = new SSDropdownSetting(id, label, options, defaultOptionIndex, entryType, hint);
}