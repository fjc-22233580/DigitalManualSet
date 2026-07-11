using DigitalManualSet.App.Common;
using DigitalManualSet.App.Navigation;
using DigitalManualSet.App.ViewModels.CreatePackage;

namespace DigitalManualSet.App.ViewModels;

/// <summary>
/// The home screen VM - contains tiles to navigate to various parts of the application.
/// </summary>
/// <seealso cref="DigitalManualSet.App.Common.ViewModel" />
/// <seealso cref="DigitalManualSet.App.ViewModels.IScreenViewModel" />
public class HomeViewModel : ViewModel, IScreenViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">The navigation service.</param>
    public HomeViewModel(INavigationService navigationService)
    {
        CreatePackageCommand = new RelayCommand(navigationService.NavigateTo<CreatePackageViewModel>);
        OpenPackageCommand = new RelayCommand(navigationService.NavigateTo<OpenPackageViewModel>);
        SettingsCommand = new RelayCommand(navigationService.NavigateTo<SettingsViewModel>);
    }

    /// <summary>
    /// Gets the create package tile command.
    /// </summary>
    public RelayCommand CreatePackageCommand { get; }

    /// <summary>
    /// Gets the open package tile command.
    /// </summary>
    public RelayCommand OpenPackageCommand { get; }


    /// <summary>
    /// Gets the settings tile command.
    /// </summary>
    public RelayCommand SettingsCommand { get; }
}