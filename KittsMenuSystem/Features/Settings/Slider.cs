using Mirror;
using System;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Settings;

/// <summary>
/// Initialize new <see cref="Slider"/> setting with base <see cref="SSSliderSetting"/> that calls <see cref="Action"/> when changed.
/// </summary>
/// <remarks>
/// Initialize new instance of <see cref="Slider"/>.
/// </remarks>
/// <param name="id">Id of <see cref="Slider"/>.</param>
/// <param name="label">Label of <see cref="SSSliderSetting"/></param>
/// <param name="minValue">Minimum value of <see cref="SSSliderSetting"/>.</param>
/// <param name="maxValue">Maximum value of <see cref="SSSliderSetting"/>.</param>
/// <param name="onChanged">Triggers <see cref="Action"/> when <see cref="Slider"/> changed.</param>
/// <param name="defaultValue">Default value of <see cref="SSSliderSetting"/>.</param>
/// <param name="integer">Sets <see cref="SSSliderSetting.Integer"/>.</param>
/// <param name="valueToStringFormat">Sets <see cref="SSSliderSetting.ValueToStringFormat"/>.</param>
/// <param name="finalDisplayFormat">Sets <see cref="SSSliderSetting.FinalDisplayFormat"/>..</param>
/// <param name="hint">Hint of <see cref="SSSliderSetting"/>.</param>
public class Slider(int? id, string label, float minValue, float maxValue, Action<ReferenceHub, float, SSSliderSetting> onChanged = null, float defaultValue = 0, bool integer = false, string valueToStringFormat = "0.##", string finalDisplayFormat = "{0}", string hint = null) 
    : BaseSetting(new SSSliderSetting(SetValidId(id, label), label, minValue, maxValue, defaultValue, integer, valueToStringFormat, finalDisplayFormat, hint))
{
    /// <summary>
    /// Initialize new <see cref="Slider"/> setting (automatic id) with base <see cref="SSSliderSetting"/> that calls <see cref="Action"/> when changed.
    /// </summary>
    /// <param name="label">Label of <see cref="SSSliderSetting"/></param>
    /// <param name="minValue">Minimum value of <see cref="SSSliderSetting"/>.</param>
    /// <param name="maxValue">Maximum value of <see cref="SSSliderSetting"/>.</param>
    /// <param name="onChanged">Triggers <see cref="Action"/> when <see cref="Slider"/> changed.</param>
    /// <param name="defaultValue">Default value of <see cref="SSSliderSetting"/>.</param>
    /// <param name="integer">Sets <see cref="SSSliderSetting.Integer"/>.</param>
    /// <param name="valueToStringFormat">Sets <see cref="SSSliderSetting.ValueToStringFormat"/>.</param>
    /// <param name="finalDisplayFormat">Sets <see cref="SSSliderSetting.FinalDisplayFormat"/>..</param>
    /// <param name="hint">Hint of <see cref="SSSliderSetting"/>.</param>
    public Slider(string label, float minValue, float maxValue, Action<ReferenceHub, float, SSSliderSetting> onChanged = null, float defaultValue = 0, bool integer = false, string valueToStringFormat = "0.##", string finalDisplayFormat = "{0}", string hint = null)
        : this(null, label, minValue, maxValue, onChanged, defaultValue, integer, valueToStringFormat, finalDisplayFormat, hint) { }

    /// <summary>
    /// Method called when value changed: <br></br>
    /// - <see cref="ReferenceHub"/> that updated the value.<br></br>
    /// - <see cref="float"/> (New Value)<br></br>
    /// - <see cref="SSSliderSetting"/> (Synced Class).
    /// </summary>
    public Action<ReferenceHub, float, SSSliderSetting> OnChanged { get; } = onChanged;

    /// <summary>
    /// Makes sure the Id is valid (not null)
    /// </summary>
    internal static int SetValidId(int? id, string label) =>
        id ?? (label + "Slider").GetStableHashCode();
}
