using DigitalManualSet.App.ViewModels;

namespace DigitalManualSet.App.Navigation;

/// <summary>
/// Manages the collection of UI screens available for navigation.
/// </summary>
public class ScreenRegistry
{
    private readonly HomeViewModel _homeViewModel;
    private readonly CreatePackageViewModel _createPackageViewModel;
    private readonly OpenPackageViewModel _openPackageViewModel;

    /// <summary>
    /// Initializes a new instance of the ScreenRegistry with the provided view models.
    /// </summary>
    public ScreenRegistry(
        HomeViewModel homeViewModel,
        CreatePackageViewModel createPackageViewModel,
        OpenPackageViewModel openPackageViewModel)
    {
        _homeViewModel = homeViewModel;
        _createPackageViewModel = createPackageViewModel;
        _openPackageViewModel = openPackageViewModel;
    }

    /// <summary>
    /// Gets the collection of all available UI screens.
    /// </summary>
    /// <returns>A read-only list of UI screens for navigation.</returns>
    public IReadOnlyList<UiScreen> GetScreens()
    {
        return
        [
            new UiScreen
            {
                Title = "Home",
                Description = "Start or continue a documentation package.",
                ViewModel = _homeViewModel
            },
            new UiScreen
            {
                Title = "Create Package",
                Description = "Create a new order-specific documentation package.",
                ViewModel = _createPackageViewModel
            },
            new UiScreen
            {
                Title = "Open Package",
                Description = "Open a previously saved package.",
                ViewModel = _openPackageViewModel
            }
        ];
    }
}