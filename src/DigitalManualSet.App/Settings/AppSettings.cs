namespace DigitalManualSet.App.Settings;

/// <summary>
/// Represents persisted user-configurable application settings.
/// These settings are typically saved to and loaded from the settings file.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the path to the folder used for working files by the application.
    /// Defaults to an empty string when not configured.
    /// </summary>
    public string WorkingFolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application's theme preference.
    /// The value is one of the <see cref="AppTheme"/> values and defaults to <see cref="AppTheme.System"/>.
    /// </summary>
    public AppTheme Theme { get; set; } = AppTheme.System;

    /// <summary>
    /// Gets or sets the last package file path opened by the user. Defaults to an empty string.
    /// </summary>
    public string LastOpenedPackagePath { get; set; } = string.Empty;
}
