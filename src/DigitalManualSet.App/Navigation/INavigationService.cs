using DigitalManualSet.App.ViewModels;

namespace DigitalManualSet.App.Navigation;

/// <summary>
/// Defines the contract for navigation services in the application.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Occurs when navigation to a view model is requested.
    /// </summary>
    event Action<Type>? NavigationRequested;

    /// <summary>
    /// Requests navigation to a screen associated with the specified view model type.
    /// </summary>
    /// <typeparam name="TViewModel">The view model type to navigate to.</typeparam>
    void NavigateTo<TViewModel>() where TViewModel : IScreenViewModel;
}