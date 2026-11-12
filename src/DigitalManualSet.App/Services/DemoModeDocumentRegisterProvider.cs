using DigitalManualSet.App.Settings;
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Infrastructure.Documents;
using System.IO;

namespace DigitalManualSet.App.Services;

/// <summary>
/// Document register provider that checks <see cref="AppSettings.DemoMode"/> on every access, returning
/// the built-in demo register in demo mode and the master register otherwise.
/// </summary>
public class DemoModeDocumentRegisterProvider : IDocumentRegisterProvider
{
    private readonly AppSettingsService _appSettingsService;
    private readonly IDocumentRegisterProvider _masterRegisterProvider;
    private readonly IDocumentRegisterProvider _demoRegisterProvider;

    public DemoModeDocumentRegisterProvider(AppSettingsService appSettingsService, string masterRegisterFilePath)
    {
        _appSettingsService = appSettingsService;
        _masterRegisterProvider = new CsvDocumentRegisterProvider(masterRegisterFilePath);
        _demoRegisterProvider = new CsvDocumentRegisterProvider(
            Path.Combine(AppContext.BaseDirectory, "TestData", "DemoDocumentRegister.csv"));
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, DocumentRegisterEntry> Register =>
        _appSettingsService.Settings.DemoMode ? _demoRegisterProvider.Register : _masterRegisterProvider.Register;

    /// <inheritdoc />
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await _masterRegisterProvider.LoadAsync(cancellationToken);
        await _demoRegisterProvider.LoadAsync(cancellationToken);
    }
}
