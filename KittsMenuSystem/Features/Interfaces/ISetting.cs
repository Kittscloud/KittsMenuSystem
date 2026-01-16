namespace KittsMenuSystem.Features.Interfaces;

using UserSettings.ServerSpecific;

internal interface ISetting
{
    ServerSpecificSettingBase Base { get; }
}