using System.IO;
using DigitalManualSet.Core.Common;

namespace DigitalManualSet.App.Common;

/// <summary>
/// Provides common file system paths used by the application.
/// Paths are resolved for the current user and are based on <see cref="AppInfo.ProductName"/>.
/// </summary>
public class AppPathProvider : IAppPathProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppPathProvider"/> class.
    /// </summary>
    public AppPathProvider()
    {
        // Create the logs folder.
        Directory.CreateDirectory(LogsFolder);
    }

    /// <summary>
    /// Gets the application's folder inside the current user's Local Application Data directory.
    /// Example: <c>%LOCALAPPDATA%\&lt;ProductName&gt;</c>.
    /// </summary>
    public string LocalAppDataFolder
    {
        get
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, AppInfo.ProductName);
        }
    }

    /// <summary>
    /// Gets the application's folder inside the current user's Documents directory.
    /// Example: <c>%USERPROFILE%\Documents\&lt;ProductName&gt;</c>.
    /// </summary>
    public string DocumentsFolder
    {
        get
        {
            var userDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(userDocuments, AppInfo.ProductName);
        }
    }

    /// <summary>
    /// Gets the full path to the application's JSON settings file stored under <see cref="LocalAppDataFolder"/>.
    /// </summary>
    public string SettingsFilePath => Path.Combine(LocalAppDataFolder, $"{AppInfo.ProductName} Settings.json");

    /// <summary>
    /// Gets the folder path where application log files are stored under <see cref="LocalAppDataFolder"/>.
    /// </summary>
    public string LogsFolder => Path.Combine(LocalAppDataFolder, "Logs");

    /// <summary>
    /// Gets the test documents folder path.
    /// </summary>
    public string TestDocuments => Path.Combine(LocalAppDataFolder, "Test Documents");
    
    /// <summary>
    /// Gets the open orders file path - held in I Drive mapped network location.
    /// </summary>
    public string OpenOrdersFilePath => @"I:\Data\Mfg\Opns\Serial Recording\MACHDTC.txt";
}
