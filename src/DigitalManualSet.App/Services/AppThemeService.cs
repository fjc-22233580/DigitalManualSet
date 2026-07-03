using DigitalManualSet.App.Settings;
using System.Windows;

namespace DigitalManualSet.App.Services;

/// <summary>
/// Service responsible for applying the application's theme.
/// Either when we de-persist or if the user changes it.
/// </summary>
public class AppThemeService
{
    /// <summary>
    /// Applies the specified <see cref="AppTheme"/> to the running application.
    /// </summary>
    /// <param name="theme">The theme to apply (System, Light, Dark).</param>
    public void ApplyTheme(AppTheme theme)
    {
        switch (theme)
        {
#pragma warning disable WPF0001 //In >.NET9 setting this programmatically is experimental so disable warning.
            case AppTheme.System:
                Application.Current.ThemeMode = ThemeMode.System;
                break;
            case AppTheme.Light:
                Application.Current.ThemeMode = ThemeMode.Light;
                break;
            case AppTheme.Dark:
                Application.Current.ThemeMode = ThemeMode.Dark;
                break;
#pragma warning restore WPF0001
        }
    }
}
