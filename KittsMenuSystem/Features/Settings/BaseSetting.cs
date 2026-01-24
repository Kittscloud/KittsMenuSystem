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
    public int SettingId {
        get => Base.SettingId;
        set => Base.SettingId = value;
    }
}
