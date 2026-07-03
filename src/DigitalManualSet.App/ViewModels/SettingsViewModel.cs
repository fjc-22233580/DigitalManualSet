using DigitalManualSet.App.Common;
using DigitalManualSet.App.Services;
using DigitalManualSet.App.Settings;
using System.Diagnostics;
using System.IO;

namespace DigitalManualSet.App.ViewModels;

/// <summary>
/// The settings view; provides a dropdown to change the UI theme,
/// and shows the current working folder,
/// with a button to open that path using the file explorer.
/// </summary>
/// <seealso cref="DigitalManualSet.App.Common.ViewModel" />
/// <seealso cref="DigitalManualSet.App.ViewModels.IScreenViewModel" />
public class SettingsViewModel : ViewModel, IScreenViewModel
{
    #region Fields

    private readonly AppSettingsService _appSettingsService;
    private readonly AppThemeService _appThemeService;
    private ComboBoxItemViewModel<AppTheme> _selectedTheme;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    /// <param name="appSettingsService">Service for loading and saving application settings.</param>
    /// <param name="appThemeService">Service used to apply theme changes to the UI.</param>
    public SettingsViewModel(AppSettingsService appSettingsService, AppThemeService appThemeService)
    {
        _appSettingsService = appSettingsService;
        _appThemeService = appThemeService;
        ThemeColors =
        [
            new ComboBoxItemViewModel<AppTheme>(AppTheme.System, "Use system theme"),
            new ComboBoxItemViewModel<AppTheme>(AppTheme.Light, "Light"),
            new ComboBoxItemViewModel<AppTheme>(AppTheme.Dark, "Dark"),
        ];

        // Set the drop down to match our depersisted value.
        _selectedTheme = ThemeColors.First(theme => theme.Value == _appSettingsService.Settings.Theme);

        OpenWorkingFolderCommand = new RelayCommand(OpenWorkingFolder);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the available theme options shown in the UI drop down.
    /// </summary>
    public List<ComboBoxItemViewModel<AppTheme>> ThemeColors { get; }

    /// <summary>
    /// Gets or sets the currently selected theme item. Changing this property
    /// will apply the theme and persist the change asynchronously.
    /// </summary>
    public ComboBoxItemViewModel<AppTheme> SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (_selectedTheme != value)
            {
                _selectedTheme = value;
                OnPropertyChanged();
                _ = SaveSelectedThemeAsync(_selectedTheme.Value);
            }
        }
    }

    /// <summary>
    /// Gets the command that opens the configured working folder using file explorer.
    /// </summary>
    public RelayCommand OpenWorkingFolderCommand { get; }

    /// <summary>
    /// Gets the path to the configured working folder from persisted settings.
    /// </summary>
    public string WorkingFolderPath => _appSettingsService.Settings.WorkingFolderPath;

    #endregion

    #region Methods

    /// <summary>
    /// Applies the specified theme and saves the setting.
    /// </summary>
    /// <param name="theme">Theme to apply and persist.</param>
    private async Task SaveSelectedThemeAsync(AppTheme theme)
    {
        _appThemeService.ApplyTheme(theme);
        _appSettingsService.Settings.Theme = theme;
        await _appSettingsService.SaveAsync();
    }

    /// <summary>
    /// Ensures the working folder exists and opens it in the system file explorer.
    /// </summary>
    private void OpenWorkingFolder()
    {
        var path = _appSettingsService.Settings.WorkingFolderPath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    #endregion

}
