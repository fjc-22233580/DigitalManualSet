using DigitalManualSet.App.ViewModels;

namespace DigitalManualSet.App.Navigation;

/// <summary>
/// Represents a screen in the UI navigation with its metadata and view model.
/// </summary>
public class UiScreen
{
    /// <summary>
    /// Gets or initializes the screen title.
    /// </summary>
    public string Title { get; init; } = "";

    /// <summary>
    /// Gets or initializes the screen description.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Gets or initializes the view model associated with this screen.
    /// </summary>
    public IScreenViewModel ViewModel { get; init; }
}