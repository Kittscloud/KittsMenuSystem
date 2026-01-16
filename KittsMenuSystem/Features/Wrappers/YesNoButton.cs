using Hints;
using KittsMenuSystem.Features.Interfaces;
using System;
using System.Reflection.Emit;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Wrappers;

/// <summary>
/// Initialize new wrapper for <see cref="SSTwoButtonsSetting"/>. Setting creates two buttons (Checkbox).
/// </summary>
/// <remarks>
/// Initialize new <see cref="YesNoButton"/>.
/// </remarks>
/// <param name="id">Id of <see cref="YesNoButton"/>.</param>
/// <param name="label">Label of <see cref="YesNoButton"/>.</param>
/// <param name="optionA">Labal of first button.</param>
/// <param name="optionB">Labal of second button.</param>
/// <param name="onChanged">Triggered when the player updates the selected button.</param>
/// <param name="defaultIsB">If no save value, B is selected by default.</param>
/// <param name="hint">Hint of <see cref="YesNoButton"/>.</param>
public class YesNoButton(int? id, string label, string optionA, string optionB, Action<ReferenceHub, bool, SSTwoButtonsSetting> onChanged = null, bool defaultIsB = false, string hint = null) 
    : SSTwoButtonsSetting(id, label, optionA, optionB, defaultIsB, hint), ISetting
{
    /// <summary>
    /// Initialize new <see cref="YesNoButton"/>.
    /// </summary>
    /// <param name="label">Label of <see cref="SSTwoButtonsSetting"/>.</param>
    /// <param name="optionA">Labal of first button.</param>
    /// <param name="optionB">Labal of second button.</param>
    /// <param name="onChanged">Triggered when the player updates the selected button.</param>
    /// <param name="defaultIsB">If no save value, B is selected by default.</param>
    /// <param name="hint">Hint of <see cref="YesNoButton"/>.</param>
    public YesNoButton(string label, string optionA, string optionB, Action<ReferenceHub, bool, SSTwoButtonsSetting> onChanged = null, bool defaultIsB = false, string hint = null)
    : this(null, label, optionA, optionB, onChanged, defaultIsB, hint) { }

    /// <summary>
    /// Method called when selected button updated: <br></br><br></br>
    /// - <see cref="ReferenceHub"/> that selected a new button.<br></br>
    /// - <see cref="bool"/> (<see cref="SSTwoButtonsSetting.SyncIsA"/>).<br></br>
    /// - <see cref="SSTwoButtonsSetting"/> (Synced Class).
    /// </summary>
    public Action<ReferenceHub, bool, SSTwoButtonsSetting> Action { get; } = onChanged;

    /// <summary>
    /// The base instance (client).
    /// </summary>
    public ServerSpecificSettingBase Base { get; private set; } = new SSTwoButtonsSetting(id, label, optionA, optionB, defaultIsB, hint);
}