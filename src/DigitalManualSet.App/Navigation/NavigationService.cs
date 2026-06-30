using DigitalManualSet.App.ViewModels;

namespace DigitalManualSet.App.Navigation;

/// <summary>
/// Provides navigation functionality between screens in the application.
/// </summary>
public class NavigationService : INavigationService
{
    /// <summary>
    /// Occurs when navigation to a view model is requested.
    /// </summary>
    public event Action<Type>? NavigationRequested;

    /// <summary>
    /// Requests navigation to a screen associated with the specified view model type.
    /// </summary>
    /// <typeparam name="TViewModel">The view model type to navigate to.</typeparam>
    public void NavigateTo<TViewModel>() where TViewModel : IScreenViewModel
    {
        NavigationRequested?.Invoke(typeof(TViewModel));
    }
}