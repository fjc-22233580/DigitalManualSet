using DigitalManualSet.App.Settings;
using DigitalManualSet.Core.Orders;
using DigitalManualSet.Infrastructure.Orders;
using System.IO;

namespace DigitalManualSet.App.Services;

/// <summary>
/// Open order provider that checks <see cref="AppSettings.DemoMode"/> on every request, returning
/// built-in test data in demo mode and spreadsheet data otherwise. This allows demo mode to be
/// toggled at runtime.
/// </summary>
public class DemoModeOpenOrderProvider : IOpenOrderProvider
{
    private readonly AppSettingsService _appSettingsService;
    private readonly SpreadsheetOpenOrderProvider _spreadsheetOpenOrderProvider;

    public DemoModeOpenOrderProvider(AppSettingsService appSettingsService, SpreadsheetOpenOrderProvider spreadsheetOpenOrderProvider)
    {
        _appSettingsService = appSettingsService;
        _spreadsheetOpenOrderProvider = spreadsheetOpenOrderProvider;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Order>> GetOpenOrdersAsync(CancellationToken cancellationToken = default)
    {
        if (_appSettingsService.Settings.DemoMode)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "TestData", "ValidOpenOrders.csv");
            return new CsvOpenOrderProvider(filePath).GetOpenOrdersAsync(cancellationToken);
        }

        return _spreadsheetOpenOrderProvider.GetOpenOrdersAsync(cancellationToken);
    }
}
