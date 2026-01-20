using System;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Wrappers;

public abstract class BaseSetting(ServerSpecificSettingBase @base)
{
    /// <summary>
    /// Base instance sent to client.
    /// </summary>
    public ServerSpecificSettingBase Base { get; internal set; } = @base;

    /// <summary>
    /// Shortcut to underlying <see cref="ServerSpecificSettingBase.SettingId"/>.
    /// </summary>
    public int SettingId {
        get => Base.SettingId;
        set => Base.SettingId = value;
    }

    /// <summary>
    /// Used to create <see cref="BaseSetting"/> from a <see cref="ServerSpecificSettingBase"/>.
    /// </summary>
    /// <param name="ss"><see cref="ServerSpecificSettingBase"/> to warp</param>
    /// <returns>Wrapped <see cref="BaseSetting"/></returns>>
    internal static BaseSetting Wrap(ServerSpecificSettingBase ss)
    {
        return ss switch
        {
            SSButton b => new Button(b.SettingId, b.Label, b.ButtonText, null, b.HoldTimeSeconds, b.HintDescription),
            SSDropdownSetting d => new Dropdown(d.SettingId, d.Label, d.Options, null, d.SyncSelectionIndexValidated, d.EntryType, d.HintDescription),
            SSSliderSetting s => new Slider(s.SettingId, s.Label, s.MinValue, s.MaxValue, null, s.Integer ? s.SyncIntValue : s.SyncFloatValue, s.Integer, s.ValueToStringFormat, s.FinalDisplayFormat, s.HintDescription),
            SSTwoButtonsSetting t => new ABButton(t.SettingId, t.Label, t.OptionA, t.OptionB, null, t.SyncIsA == false, t.HintDescription),
            SSPlaintextSetting p => new TextBox(p.SettingId, p.Label, null, p.SyncInputText, p.CharacterLimit, p.ContentType, p.HintDescription),
            SSTextArea a => new TextArea(a.SettingId, a.Label, a.Foldout, a.HintDescription, a.AlignmentOptions),
            SSKeybindSetting k => new Keybind(k.SettingId, k.Label, null, k.SuggestedKey, k.PreventInteractionOnGUI, k.AllowSpectatorTrigger, k.HintDescription),
            _ => throw new Exception($"Unknown setting type: {ss.GetType().Name}")
        };
    }
}