using DigitalManualSet.App.Common;
using DigitalManualSet.App.Navigation;

namespace DigitalManualSet.App.ViewModels;

/// <summary>
/// Manages the shell view, handling screen navigation and current screen state.
/// </summary>
/// <remarks>
/// This view model serves as the main container for the application, managing navigation between different screens
/// and maintaining state for the currently displayed screen. It communicates with the navigation service to handle
/// navigation requests and provides a home command to return to the initial screen.
/// </remarks>
/// <seealso cref="DigitalManualSet.App.Common.ViewModel" />
public class ShellViewModel : ViewModel
{
    /// <summary>
    /// The navigation service used to handle navigation between screens.
    /// </summary>
    private readonly INavigationService _navigationService;

    /// <summary>
    /// The collection of registered screens that can be navigated to.
    /// </summary>
    private readonly IReadOnlyList<UiScreen> _screens;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
    /// </summary>
    /// <param name="screenRegistry">The screen registry.</param>
    /// <param name="navigationService">The navigation service.</param>
    public ShellViewModel(ScreenRegistry screenRegistry, INavigationService navigationService)
    {
        _navigationService = navigationService;
        _screens = screenRegistry.GetScreens();
        CurrentScreen = _screens[0];

        _navigationService.NavigationRequested += NavigateTo;

        HomeCommand = new RelayCommand(GoHome);
    }

    /// <summary>
    /// Gets the home command.
    /// </summary>
    /// <value>
    /// The home command.
    /// </value>
    public RelayCommand HomeCommand { get; }

    /// <summary>
    /// Gets or sets the current screen.
    /// </summary>
    /// <value>
    /// The current screen.
    /// </value>
    public UiScreen CurrentScreen
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Goes to the home screen (the first registered screen).
    /// </summary>
    private void GoHome()
    {
        CurrentScreen = _screens[0];
    }

    /// <summary>
    /// Navigates to the screen associated with the specified view model type.
    /// </summary>
    /// <param name="viewModelType">The type of the view model to navigate to.</param>
    /// <exception cref="InvalidOperationException">Thrown when no screen is registered for the specified view model type.</exception>
    private void NavigateTo(Type viewModelType)
    {
        var screen = _screens.FirstOrDefault(screen => screen.ViewModel.GetType() == viewModelType);

        if (screen is null)
        {
            throw new InvalidOperationException(
                $"No screen registered for view model type '{viewModelType.Name}'.");
        }

        CurrentScreen = screen;
    }
}