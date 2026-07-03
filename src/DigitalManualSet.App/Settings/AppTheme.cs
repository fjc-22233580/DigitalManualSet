namespace DigitalManualSet.App.Settings;

/// <summary>
/// Represents the application's theme preference.
/// Stored in settings and used to control the UI theme mode.
/// </summary>
public enum AppTheme
{
    /// <summary>
    /// Follow the operating system setting.
    /// </summary>
    System,

    /// <summary>
    /// Use a light theme.
    /// </summary>
    Light,

    /// <summary>
    /// Use a dark theme.
    /// </summary>
    Dark
}