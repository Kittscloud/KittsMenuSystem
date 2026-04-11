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
    : BaseSetting(new SSSliderSetting(id ?? (label + nameof(Slider)).GetStableHashCode(), label, minValue, maxValue, defaultValue, integer, valueToStringFormat, finalDisplayFormat, hint))
{
    /// <summary>
    /// Initialize new <see cref="Slider"/> setting (automatic id) with base <see cref="SSSliderSetting"/> that calls <see cref="Action"/> when changed.
    /// </summary>
    /// <param name="label">Label of <see cref="SSSliderSetting"/></param>
    /// <param name="minValue">Sets <see cref="SSSliderSetting.MinValue"/>.</param>
    /// <param name="maxValue">Sets <see cref="SSSliderSetting.MaxValue"/>.</param>
    /// <param name="onChanged">Triggers <see cref="Action"/> when <see cref="Slider"/> changed.</param>
    /// <param name="defaultValue">Sets <see cref="SSSliderSetting.DefaultValue"/>.</param>
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
    /// Shortcut to underlying <see cref="SSSliderSetting.MinValue"/>.
    /// </summary>
    public float MinValue
    {
        get => (Base as SSSliderSetting).MinValue;
        set => UpdateMinValue(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="SSSliderSetting.SendSliderUpdate"/> but only for <see cref="SSSliderSetting.MinValue"/>.
    /// <see cref="SSSliderSetting.MinValue"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdateMinValue(float newMinValue, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) =>
        (Base as SSSliderSetting).SendSliderUpdate(newMinValue, MaxValue, Integer, ValueToStringFormat, FinalDisplayFormat, applyOverride, receiveFilter);

    /// <summary>
    /// Shortcut to underlying <see cref="SSSliderSetting.MaxValue"/>.
    /// </summary>
    public float MaxValue
    {
        get => (Base as SSSliderSetting).MaxValue;
        set => UpdateMaxValue(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="SSSliderSetting.SendSliderUpdate"/> but only for <see cref="SSSliderSetting.MaxValue"/>.
    /// <see cref="SSSliderSetting.MaxValue"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdateMaxValue(float newMaxValue, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) =>
        (Base as SSSliderSetting).SendSliderUpdate(MinValue, newMaxValue, Integer, ValueToStringFormat, FinalDisplayFormat, applyOverride, receiveFilter);

    /// <summary>
    /// Shortcut to underlying <see cref="SSSliderSetting.Integer"/>.
    /// </summary>
    public bool Integer
    {
        get => (Base as SSSliderSetting).Integer;
        set => UpdateInteger(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="SSSliderSetting.SendSliderUpdate"/> but only for <see cref="SSSliderSetting.Integer"/>.
    /// <see cref="SSSliderSetting.Integer"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdateInteger(bool newInteger, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) =>
        (Base as SSSliderSetting).SendSliderUpdate(MinValue, MaxValue, newInteger, ValueToStringFormat, FinalDisplayFormat, applyOverride, receiveFilter);

    /// <summary>
    /// Shortcut to underlying <see cref="SSSliderSetting.ValueToStringFormat"/>.
    /// </summary>
    public string ValueToStringFormat
    {
        get => (Base as SSSliderSetting).ValueToStringFormat;
        set => UpdateValueToStringFormat(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="SSSliderSetting.SendSliderUpdate"/> but only for <see cref="SSSliderSetting.ValueToStringFormat"/>.
    /// <see cref="SSSliderSetting.ValueToStringFormat"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdateValueToStringFormat(string newValueToStringFormat, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) =>
        (Base as SSSliderSetting).SendSliderUpdate(MinValue, MaxValue, Integer, newValueToStringFormat, FinalDisplayFormat, applyOverride, receiveFilter);

    /// <summary>
    /// Shortcut to underlying <see cref="SSSliderSetting.FinalDisplayFormat"/>.
    /// </summary>
    public string FinalDisplayFormat
    {
        get => (Base as SSSliderSetting).FinalDisplayFormat;
        set => UpdateFinalDisplayFormat(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="SSSliderSetting.SendSliderUpdate"/> but only for <see cref="SSSliderSetting.FinalDisplayFormat"/>.
    /// <see cref="SSSliderSetting.FinalDisplayFormat"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdateFinalDisplayFormat(string newFinalDisplayFormat, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) =>
        (Base as SSSliderSetting).SendSliderUpdate(MinValue, MaxValue, Integer, ValueToStringFormat, newFinalDisplayFormat, applyOverride, receiveFilter);
}
