using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigitalManualSet.App.Common;
using Microsoft.Extensions.Logging;

namespace DigitalManualSet.App.Settings;

/// <summary>
/// Service responsible for loading and saving the application's user settings.
/// The service exposes the currently loaded <see cref="AppSettings"/> instance and
/// provides <see cref="LoadAsync"/> and <see cref="SaveAsync"/> for persistence.
/// </summary>
public sealed class AppSettingsService
{
    /// <summary>
    /// Holds a reference to the logger.
    /// </summary>
    private readonly ILogger<AppSettingsService> _logger;


    // Shared JSON options used when reading and writing the settings file.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            // Stores enum values as text, e.g. "System", instead of 0, 1, 2.
            new JsonStringEnumConverter()
        }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettingsService"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record warnings and information during load/save operations.</param>
    public AppSettingsService(ILogger<AppSettingsService> logger)
    {
        _logger = logger;

        // Start with defaults so Settings is always safe to use.
        Settings = CreateDefaults();
    }

    /// <summary>
    /// Gets the in-memory application settings object. This instance is updated when
    /// <see cref="LoadAsync"/> is called and is written to disk by <see cref="SaveAsync"/>.
    /// </summary>
    public AppSettings Settings { get; private set; }
   

    /// <summary>
    /// Loads the application settings from the user's settings file.
    /// If the file does not exist the defaults are saved and used. Any errors during
    /// load fall back to default settings and are logged as warnings.
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            // Ensure the folder exists before trying to read or write settings.
            Directory.CreateDirectory(AppPaths.LocalAppDataFolder);

            // First run: create a settings file using default values.
            if (!File.Exists(AppPaths.SettingsFilePath))
            {
                await SaveAsync();
                return;
            }

            var json = await File.ReadAllTextAsync(AppPaths.SettingsFilePath);

            // If deserialisation fails or returns null, fall back to defaults.
            Settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? CreateDefaults();

        }
        catch (Exception ex)
        {
            // Do not stop the app from starting just because settings failed to load.
            _logger.LogWarning(ex, "Failed to load app settings. Default settings will be used.");
            Settings = CreateDefaults();
        }
    }

    /// <summary>
    /// Persists the current <see cref="Settings"/> instance to the user's settings file.
    /// Errors during save are logged as warnings but do not throw.
    /// </summary>
    public async Task SaveAsync()
    {
        try
        {
            // Ensure the folder exists before writing the settings file.
            Directory.CreateDirectory(AppPaths.LocalAppDataFolder);

            var json = JsonSerializer.Serialize(Settings, JsonOptions);
            await File.WriteAllTextAsync(AppPaths.SettingsFilePath, json);
            _logger.LogInformation("Persistence file updated.");
        }
        catch (Exception ex)
        {
            // Settings save failure should be logged, but should not crash the app.
            _logger.LogWarning(ex, "Failed to save app settings.");
        }
    }

    /// <summary>
    /// Creates a new <see cref="AppSettings"/> instance populated with defaults.
    /// </summary>
    private static AppSettings CreateDefaults()
    {
        return new AppSettings
        {
            // Default to a user-accessible folder.
            WorkingFolderPath = AppPaths.DocumentsFolder,
            Theme = AppTheme.System,
            LastOpenedPackagePath = string.Empty
        };
    }
}