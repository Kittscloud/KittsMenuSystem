using Mirror;
using TMPro;
using UserSettings.ServerSpecific;

namespace KittsMenuSystem.Features.Settings;

/// <summary>
/// Initialize new <see cref="TextArea"/> setting with base <see cref="SSTextArea"/>.
/// </summary>
/// <remarks>
/// Initialize new <see cref="TextArea"/>.
/// </remarks>
/// <param name="id">Id of <see cref="SSTextArea"/>.</param>
/// <param name="content">Text in <see cref="SSTextArea"/>.</param>
/// <param name="foldoutMode">Mode that the text folds out.</param>
/// <param name="collapsedText">Text in <see cref="SSTextArea"/> when collapsed.</param>
/// <param name="textAlignment">Alignment of text.</param>
public class TextArea(int? id, string content, SSTextArea.FoldoutMode foldoutMode = SSTextArea.FoldoutMode.NotCollapsable, string collapsedText = null, TextAlignmentOptions textAlignment = TextAlignmentOptions.TopLeft)
    : BaseSetting(new SSTextArea(SetValidId(id, content), content, foldoutMode, collapsedText, textAlignment))
{
    /// <summary>
    /// Initialize new <see cref="TextArea"/> setting (automatic id) with base <see cref="SSTextArea"/>.
    /// </summary>
    /// <param name="content">Text in <see cref="SSTextArea"/>.</param>
    /// <param name="foldoutMode">Mode that the text folds out.</param>
    /// <param name="collapsedText">Text in <see cref="SSTextArea"/> when collapsed.</param>
    /// <param name="textAlignment">Alignment of text.</param>
    public TextArea(string content, SSTextArea.FoldoutMode foldoutMode = SSTextArea.FoldoutMode.NotCollapsable, string collapsedText = null, TextAlignmentOptions textAlignment = TextAlignmentOptions.TopLeft)
        : this(null, content, foldoutMode, collapsedText, textAlignment) { }

    /// <summary>
    /// Makes sure the Id is valid (not null)
    /// </summary>
    internal static int SetValidId(int? id, string label) =>
        id ?? (label + "TextArea").GetStableHashCode();
}
