using KittsMenuSystem.Features.Interfaces;
using System;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Wrappers;

/// <summary>
/// Initialize new wrapper for <see cref="SSSliderSetting"/>. Setting creates a slider.
/// </summary>
/// <remarks>
/// Initialize new instance of <see cref="Slider"/>.
/// </remarks>
/// <param name="id">Id of <see cref="Slider"/>.</param>
/// <param name="label">Label of <see cref="Slider"/></param>
/// <param name="minValue">Minimum value of <see cref="Slider"/>.</param>
/// <param name="maxValue">Maximum value of <see cref="Slider"/>.</param>
/// <param name="onChanged">Triggered when the player updates the value.</param>
/// <param name="defaultValue">Default value of <see cref="Slider"/>. Must be between <see cref="SSSliderSetting.MinValue"/> and <see cref="SSSliderSetting.MaxValue"/></param>
/// <param name="integer">Integer only (<see cref="SSSliderSetting.SyncFloatValue"/> is an int).</param>
/// <param name="valueToStringFormat">Text value of <see cref="Slider"/> (Formatted as <see cref="float"/>::<see cref="float.ToString(string)"/>).</param>
/// <param name="finalDisplayFormat">Format of <see cref="SSSliderSetting.ValueToStringFormat"/>.</param>
/// <param name="hint">Hint of <see cref="Slider"/>.</param>
public class Slider(int? id, string label, float minValue, float maxValue, Action<ReferenceHub, float, SSSliderSetting> onChanged = null, float defaultValue = 0, bool integer = false, string valueToStringFormat = "0.##", string finalDisplayFormat = "{0}", string hint = null) 
    : SSSliderSetting(id, label, minValue, maxValue, defaultValue, integer, valueToStringFormat, finalDisplayFormat, hint), ISetting
{
    /// <summary>
    /// Initialize new instance of <see cref="Slider"/>.
    /// </summary>
    /// <param name="label">Label of <see cref="Slider"/></param>
    /// <param name="minValue">Minimum value of <see cref="Slider"/>.</param>
    /// <param name="maxValue">Maximum value of <see cref="Slider"/>.</param>
    /// <param name="onChanged">Triggered when the player updates the value.</param>
    /// <param name="defaultValue">Default value of <see cref="Slider"/>. Must be between <see cref="SSSliderSetting.MinValue"/> and <see cref="SSSliderSetting.MaxValue"/></param>
    /// <param name="integer">Integer only (<see cref="SSSliderSetting.SyncFloatValue"/> is an int).</param>
    /// <param name="valueToStringFormat">Text value of <see cref="Slider"/> (Formatted as <see cref="float"/>::<see cref="float.ToString(string)"/>).</param>
    /// <param name="finalDisplayFormat">Format of <see cref="SSSliderSetting.ValueToStringFormat"/>.</param>
    /// <param name="hint">Hint of <see cref="Slider"/>.</param>
    public Slider(string label, float minValue, float maxValue, Action<ReferenceHub, float, SSSliderSetting> onChanged = null, float defaultValue = 0, bool integer = false, string valueToStringFormat = "0.##", string finalDisplayFormat = "{0}", string hint = null)
        : this(null, label, minValue, maxValue, onChanged, defaultValue, integer, valueToStringFormat, finalDisplayFormat, hint) { }

    /// <summary>
    /// Method called when slider updated: <br></br><br></br>
    /// - <see cref="ReferenceHub"/> that updated the slider.<br></br>
    /// - <see cref="float"/> (New Value)./><br></br>
    /// - <see cref="SSSliderSetting"/> (Synced Class).
    /// </summary>
    public Action<ReferenceHub, float, SSSliderSetting> Action { get; } = onChanged;

    /// <summary>
    /// The base instance (client).
    /// </summary>
    public ServerSpecificSettingBase Base { get; set; } = new SSSliderSetting(id, label, minValue, maxValue, defaultValue, integer, valueToStringFormat, finalDisplayFormat, hint);
}