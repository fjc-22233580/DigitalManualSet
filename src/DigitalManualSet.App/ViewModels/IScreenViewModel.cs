namespace DigitalManualSet.App.ViewModels;

/// <summary>
/// Defines a marker interface for core VMs.
/// </summary>
public interface IScreenViewModel
{
    /// <summary>
    /// Called each time the screen is navigated to.
    /// </summary>
    void OnNavigatedTo() { }
}