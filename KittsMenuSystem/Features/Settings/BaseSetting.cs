using System;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Settings;

public abstract class BaseSetting(ServerSpecificSettingBase @base)
{
    /// <summary>
    /// Base instance sent to client.
    /// </summary>
    public ServerSpecificSettingBase Base { get; internal set; } = @base;

    /// <summary>
    /// Shortcut to underlying <see cref="ServerSpecificSettingBase.SettingId"/>.
    /// </summary>
    public int SettingId
    {
        get => Base.SettingId;
        set => Base.SettingId = value;
    }

    /// <summary>
    /// Shortcut to underlying <see cref="ServerSpecificSettingBase.Label"/>.
    /// </summary>
    public string Label
    {
        get => Base.Label;
        set => UpdateLabel(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="ServerSpecificSettingBase.SendLabelUpdate"/>.
    /// <see cref="ServerSpecificSettingBase.Label"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdateLabel(string newLabel, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) => 
        Base.SendLabelUpdate(newLabel, applyOverride, receiveFilter);

    /// <summary>
    /// Shortcut to underlying <see cref="ServerSpecificSettingBase.HintDescription"/>.
    /// </summary>
    public string HintDescription
    {
        get => Base.HintDescription;
        set => UpdateHintDescription(value);
    }

    /// <summary>
    /// Shortcut to underlying <see cref="ServerSpecificSettingBase.SendHintUpdate"/>.
    /// <see cref="ServerSpecificSettingBase.HintDescription"/> will go back to default (what was set in the menu) when rejoining server.
    /// </summary>
    public void UpdateHintDescription(string newHintDescription, bool applyOverride = true, Func<ReferenceHub, bool> receiveFilter = null) =>
        Base.SendHintUpdate(newHintDescription, applyOverride, receiveFilter);
}
