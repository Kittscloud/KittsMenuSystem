using System.ComponentModel;

namespace KittsMenuSystem;

public class Config
{
    /// <summary>
    /// Is plugin enabled.
    /// </summary>
    [Description("Is plugin enabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Sends debug logs to console.
    /// </summary>
    [Description("Sends debug logs to console")]
    public bool Debug { get; set; } = false;

    /// <summary>
    /// Allows players to see errors.
    /// </summary>
    [Description("Whether players can see errors or not")]
    public bool ShowErrorToClient { get; set; } = true;

    /// <summary>
    /// Allows players to see total errors including plugin content.
    /// </summary>
    [Description("Whether players can see total errors including plugin content or not")]
    public bool ShowFullErrorToClient { get; set; } = false;

    /// <summary>
    /// Allows moderators (RA access) to see total errors including plugin content.
    /// </summary>
    [Description("Whether moderators (RA access) can see total errors including plugin content or not")]
    public bool ShowFullErrorToModerators { get; set; } = true;

    /// <summary>
    /// Whether in-built menus events are enabled.
    /// </summary>
    [Description("Whether in-built menus events are enabled")]
    public bool EnableExamples { get; set; } = true;

    /// <summary>
    /// Plugin translation labels and buttons.
    /// </summary>
    public Translation Translation { get; set; } = new Translation();
}

public class Translation
{
    /// <summary>
    /// Open menu button. {0} = menu name.
    /// </summary>
    [Description("Open menu button. {0} = menu name")]
    public ButtonConfig OpenMenu { get; set; } = new("Open {0}", "Open");

    /// <summary>
    /// Return to button. {0} = menu name.
    /// </summary>
    [Description("Return to button. {0} = menu name")]
    public ButtonConfig ReturnTo { get; set; } = new("Return to {0}", "Return");

    /// <summary>
    /// Reload menu button.
    /// </summary>
    [Description("Reload menu button")]
    public ButtonConfig ReloadButton { get; set; } = new("Reload menus", "Reload");

    /// <summary>
    /// Text shown when an error is occurrs.
    /// </summary>
    [Description("Text shown when an error is occurrs")]
    public string ServerError { get; set; } = "Internal Server Error";

    /// <summary>
    /// Sub-menus title.
    /// </summary>
    [Description("Sub-menus title")]
    public GroupHeaderConfig SubMenuTitle { get; set; } = new("Sub-Menus", null);

    /// <summary>
    /// Error text shown wehn insufficient permission.
    /// </summary>
    [Description("Error text shown wehn insufficient permission")]
    public string NoPermission { get; set; } = "Insufficient permissions to view full error details";
}

public class ButtonConfig
{
    /// <summary>
    /// Label of the button.
    /// </summary>
    public string Label { get; set; } = "MISSING_LABEL";

    /// <summary>
    /// Text shown on the button.
    /// </summary>
    public string ButtonText { get; set; } = "MISSING_VALUE";

    /// <summary>
    /// Optional hint for the button.
    /// </summary>
    public string Hint { get; set; }

    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public ButtonConfig() { }

    /// <summary>
    /// Initialize a new <see cref="ButtonConfig"/> instance.
    /// </summary>
    /// <param name="label">Label of the button.</param>
    /// <param name="buttonText">Text shown on the button.</param>
    /// <param name="hint">Optional hint for the button.</param>
    public ButtonConfig(string label, string buttonText, string hint = null)
    {
        Label = label;
        ButtonText = buttonText;
        Hint = hint;
    }
}

public class GroupHeaderConfig
{
    /// <summary>
    /// Label of <see cref="GroupHeaderConfig"/>.
    /// </summary>
    public string Label { get; set; } = "MISSING_LABEL";

    /// <summary>
    /// Hint for <see cref="GroupHeaderConfig"/>.
    /// </summary>
    public string Hint { get; set; } = "";

    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public GroupHeaderConfig() { }

    /// <summary>
    /// Initialize new <see cref="GroupHeaderConfig"/>.
    /// </summary>
    /// <param name="label">Label of <see cref="GroupHeaderConfig"/>.</param>
    /// <param name="hint">Hint of <see cref="GroupHeaderConfig"/>.</param>
    public GroupHeaderConfig(string label, string hint = null)
    {
        Label = label;
        Hint = hint ?? "";
    }
}
