using DigitalManualSet.App.Common;
using DigitalManualSet.App.Navigation;

namespace DigitalManualSet.App.ViewModels;

/// <summary>
/// 
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
    }

    /// <summary>
    /// Gets the create package tile command.
    /// </summary>
    public RelayCommand CreatePackageCommand { get; }
}