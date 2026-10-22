namespace DigitalManualSet.Core.Common;

public interface IAppPathProvider
{
    /// <summary>
    /// Gets the application's folder inside the current user's Local Application Data directory.
    /// Example: <c>%LOCALAPPDATA%\&lt;ProductName&gt;</c>.
    /// </summary>
    string LocalAppDataFolder { get; }


    /// <summary>
    /// Gets the application's folder inside the current user's Documents directory.
    /// Example: <c>%USERPROFILE%\Documents\&lt;ProductName&gt;</c>.
    /// </summary>
    string DocumentsFolder { get; }


    /// <summary>
    /// Gets the full path to the application's JSON settings file stored under <see cref="LocalAppDataFolder"/>.
    /// </summary>
    string SettingsFilePath { get; }


    /// <summary>
    /// Gets the folder path where application log files are stored under <see cref="LocalAppDataFolder"/>.
    /// </summary>
    string LogsFolder { get; }


    /// <summary>
    /// Gets the test documents folder path.
    /// </summary>
    string TestDocuments { get; }

}