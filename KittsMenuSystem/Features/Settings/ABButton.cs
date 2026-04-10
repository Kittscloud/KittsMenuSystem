using Mirror;
using System;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Settings;

/// <summary>
/// Initialize new <see cref="ABButton"/> setting with base <see cref="SSTwoButtonsSetting"/> that calls <see cref="Action"/> when changed.
/// </summary>
/// <remarks>
/// Initialize new <see cref="ABButton"/>.
/// </remarks>
/// <param name="id">Id of <see cref="SSTwoButtonsSetting"/>.</param>
/// <param name="label">Label of <see cref="SSTwoButtonsSetting"/>.</param>
/// <param name="optionA">Label of <see cref="SSTwoButtonsSetting.OptionA"/>.</param>
/// <param name="optionB">Label of <see cref="SSTwoButtonsSetting.OptionB"/>.</param>
/// <param name="onChanged">Tiggers <see cref="Action"/> when <see cref="ABButton"/> changes.</param>
/// <param name="defaultIsB">Sets <see cref="SSTwoButtonsSetting.DefaultIsB"/>.</param>
/// <param name="hint">Hint of <see cref="SSTwoButtonsSetting"/>.</param>
public class ABButton(int? id, string label, string optionA, string optionB, Action<ReferenceHub, bool, SSTwoButtonsSetting> onChanged = null, bool defaultIsB = false, string hint = null)
    : BaseSetting(new SSTwoButtonsSetting(id ?? Guid.NewGuid().ToString().GetStableHashCode(), label, optionA, optionB, defaultIsB, hint))
{
    /// <summary>
    /// Initialize new <see cref="ABButton"/> setting (automatic id) with base <see cref="SSTwoButtonsSetting"/> that calls <see cref="Action"/> when changed.
    /// </summary>
    /// <param name="label">Label of <see cref="SSTwoButtonsSetting"/>.</param>
    /// <param name="optionA">Text of <see cref="SSTwoButtonsSetting.OptionA"/>.</param>
    /// <param name="optionB">Text of <see cref="SSTwoButtonsSetting.OptionB"/>.</param>
    /// <param name="onChanged">Tiggers <see cref="Action"/> when <see cref="ABButton"/> changes.</param>
    /// <param name="defaultIsB">Sets <see cref="SSTwoButtonsSetting.DefaultIsB"/>.</param>
    /// <param name="hint">Hint of <see cref="SSTwoButtonsSetting"/>.</param>
    public ABButton(string label, string optionA, string optionB, Action<ReferenceHub, bool, SSTwoButtonsSetting> onChanged = null, bool defaultIsB = false, string hint = null)
        : this(null, label, optionA, optionB, onChanged, defaultIsB, hint) { }

    /// <summary>
    /// Method called when selected button changed: <br></br>
    /// - <see cref="ReferenceHub"/> that selected a new button.<br></br>
    /// - <see cref="bool"/> (<see cref="SSTwoButtonsSetting.SyncIsA"/>)<br></br>
    /// - <see cref="SSTwoButtonsSetting"/> (Synced Class).
    /// </summary>
    public Action<ReferenceHub, bool, SSTwoButtonsSetting> OnChanged { get; } = onChanged;

    /// <summary>
    /// Shortcut to underlying <see cref="SSTwoButtonsSetting.OptionA"/>.
    /// </summary>
    public string OptionA
    {
        get => (Base as SSTwoButtonsSetting).OptionA;
        set => UpdateOptionA(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="SSTwoButtonsSetting.SendTwoButtonUpdate"/> but only for <see cref="SSTwoButtonsSetting.OptionA"/>.
    /// <see cref="SSTwoButtonsSetting.OptionA"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdateOptionA(string newOptionA, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) =>
        (Base as SSTwoButtonsSetting).SendTwoButtonUpdate(newOptionA, OptionB, applyOverride, receiveFilter);

    /// <summary>
    /// Shortcut to underlying <see cref="SSTwoButtonsSetting.OptionB"/>.
    /// </summary>
    public string OptionB
    {
        get => (Base as SSTwoButtonsSetting).OptionB;
        set => UpdateOptionB(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="SSTwoButtonsSetting.SendTwoButtonUpdate"/> but only for <see cref="SSTwoButtonsSetting.OptionB"/>.
    /// <see cref="SSTwoButtonsSetting.OptionB"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdateOptionB(string newOptionB, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) =>
        (Base as SSTwoButtonsSetting).SendTwoButtonUpdate(OptionA, newOptionB, applyOverride, receiveFilter);
}
