using KittsMenuSystem.Features.Interfaces;
using System;
using System.Runtime.CompilerServices;
using TMPro;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Wrappers;

/// <summary>
/// Initialize new wrapper for <see cref="SSGroupHeader"/>. Setting creates an input text box.
/// </summary>
/// <remarks>
/// Initialize new <see cref="GroupHeader"/>.
/// </remarks>
/// <param name="label">Label of <see cref="Plaintext"/>.</param>
/// <param name="reducedPadding">Is padding reduced.</param>
/// <param name="hint">Hint of <see cref="Plaintext"/>.</param>
public class GroupHeader(string label, bool reducedPadding = false, string hint = null) 
    : SSGroupHeader(label, reducedPadding, hint), ISetting
{
    /// <summary>
    /// The base instance (sent in to the client).
    /// </summary>
    public ServerSpecificSettingBase Base { get; set; } = new SSGroupHeader(label, reducedPadding, hint);
}