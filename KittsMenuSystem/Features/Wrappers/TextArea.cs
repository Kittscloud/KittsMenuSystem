using KittsMenuSystem.Features.Interfaces;
using System;
using TMPro;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Wrappers;

/// <summary>
/// Initialize new wrapper for <see cref="SSTextArea"/>. Setting creates an input text box.
/// </summary>
/// <remarks>
/// Initialize new <see cref="TextArea"/>.
/// </remarks>
/// <param name="id">Id of <see cref="TextArea"/>.</param>
/// <param name="content">Text in <see cref="TextArea"/>.</param>
/// <param name="foldoutMode">Mode that the text folds out.</param>
/// <param name="collapsedText">Text in <see cref="TextArea"/> when collapsed.</param>
/// <param name="textAlignment">Alignment of text.</param>
public class TextArea(int? id, string content, SSTextArea.FoldoutMode foldoutMode = SSTextArea.FoldoutMode.NotCollapsable, string collapsedText = null, TextAlignmentOptions textAlignment = TextAlignmentOptions.TopLeft) 
    : SSTextArea(id, content, foldoutMode, collapsedText, textAlignment), ISetting
{
    /// <summary>
    /// Initialize new <see cref="TextArea"/>.
    /// </summary>
    /// <param name="content">Text in <see cref="TextArea"/>.</param>
    /// <param name="foldoutMode">Mode that the text folds out.</param>
    /// <param name="collapsedText">Text in <see cref="TextArea"/> when collapsed.</param>
    /// <param name="textAlignment">Alignment of text.</param>
    public TextArea(string content, SSTextArea.FoldoutMode foldoutMode = SSTextArea.FoldoutMode.NotCollapsable, string collapsedText = null, TextAlignmentOptions textAlignment = TextAlignmentOptions.TopLeft)
        : this(null, content, foldoutMode, collapsedText, textAlignment) { }

    /// <summary>
    /// The base instance (sent in to the client).
    /// </summary>
    public ServerSpecificSettingBase Base { get; set; } = new SSTextArea(id, content, foldoutMode, collapsedText, textAlignment);
}