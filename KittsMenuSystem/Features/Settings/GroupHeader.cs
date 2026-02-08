using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Settings;

/// <summary>
/// Initialize new <see cref="GroupHeader"/> setting with base <see cref="SSGroupHeader"/>.
/// </summary>
/// <remarks>
/// Initialize new <see cref="GroupHeader"/>.
/// </remarks>
/// <param name="label">Label of <see cref="GroupHeader"/>.</param>
/// <param name="reducedPadding">Is padding reduced.</param>
/// <param name="hint">Hint of <see cref="GroupHeader"/>.</param>
public class GroupHeader(string label, bool reducedPadding = false, string hint = null)
    : BaseSetting(new SSGroupHeader(label, reducedPadding, hint))
{ }