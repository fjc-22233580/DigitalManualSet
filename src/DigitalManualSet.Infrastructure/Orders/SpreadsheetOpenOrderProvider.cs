using DigitalManualSet.Core.Orders;
using ExcelDataReader;
using Microsoft.Extensions.Logging;

namespace DigitalManualSet.Infrastructure.Orders;

/// <summary>
/// Provides open orders read from the "data" worksheet of an Excel spreadsheet.
/// </summary>
/// <remarks>
/// Call <see cref="InitializeAsync"/> before <see cref="GetOpenOrdersAsync"/>. The spreadsheet is copied
/// to a local temporary file before parsing to avoid sharing conflicts with users who have it open.
/// </remarks>
public class SpreadsheetOpenOrderProvider : IOpenOrderProvider
{
    /// <summary>
    /// Zero-based column index of the order number.
    /// </summary>
    private const int OrderNumberIndex = 0;

    /// <summary>
    /// Zero-based column index of the system ID.
    /// </summary>
    private const int SystemIdIndex = 6;

    /// <summary>
    /// Zero-based column index of the customer name.
    /// </summary>
    private const int CustomerNameIndex = 10;

    /// <summary>
    /// The logger.
    /// </summary>
    private readonly ILogger<SpreadsheetOpenOrderProvider> _logger;

    /// <summary>
    /// Gets the unique open orders loaded by <see cref="InitializeAsync"/>, or <c>null</c> if not loaded or loading failed.
    /// </summary>
    public IReadOnlyList<Order>? Orders { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpreadsheetOpenOrderProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <c>null</c>.</exception>
    public SpreadsheetOpenOrderProvider(ILogger<SpreadsheetOpenOrderProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Loads unique open orders from the specified spreadsheet into <see cref="Orders"/>.
    /// </summary>
    /// <remarks>
    /// Rows with a blank order number are skipped, and only the first occurrence of each order number is kept.
    /// Errors are logged rather than thrown, leaving <see cref="Orders"/> as <c>null</c>.
    /// </remarks>
    /// <param name="filePath">The path to the open orders spreadsheet.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>

    public async Task InitializeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        Orders = null;

        // Copy the network file to a local temporary file to avoid file sharing issues.
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"OpenOrders_{Guid.NewGuid():N}.xlsx");

        try
        {
            // Copy the shared file to a temporary local file.
            await using (var source = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            await using (var destination = new FileStream(
                tempFilePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None))
            {
                await source.CopyToAsync(destination, cancellationToken);
            }

            _logger.LogDebug("Open orders file copied to {TempFilePath}", tempFilePath);

            // Parse the local copy on a background thread.
            Orders = await Task.Run<IReadOnlyList<Order>>(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Required by ExcelDataReader on .NET Core / .NET 5+.
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using var stream = File.Open(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);

                // Move to the "data" worksheet.
                var foundSheet = false;
                do
                {
                    if (string.Equals(reader.Name, "data", StringComparison.OrdinalIgnoreCase))
                    {
                        foundSheet = true;
                        break;
                    }
                }
                while (reader.NextResult());

                if (!foundSheet)
                {
                    throw new InvalidOperationException("Worksheet 'data' was not found in the open orders file.");
                }

                var orders = new List<Order>();
                var seenOrderNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Skip the header row.
                reader.Read();

                while (reader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Column indexes are zero-based.
                    var orderNumber = reader.GetValue(OrderNumberIndex)?.ToString()?.Trim();

                    if (string.IsNullOrWhiteSpace(orderNumber))
                    {
                        continue;
                    }

                    // Keep only the first occurrence of each order number.
                    if (!seenOrderNumbers.Add(orderNumber))
                    {
                        continue;
                    }

                    var order = new Order(
                        orderNumber: orderNumber,
                        customerName: reader.GetValue(CustomerNameIndex)?.ToString()?.Trim() ?? string.Empty,
                        systemId: reader.GetValue(SystemIdIndex)?.ToString()?.Trim() ?? string.Empty);

                    orders.Add(order);
                }

                return orders;

            }, cancellationToken);

            _logger.LogInformation("Successfully loaded {OrderCount} open orders from {FilePath}", Orders.Count, filePath);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Open orders initialisation was cancelled.");

            throw;
        }
        catch (Exception ex)
        {
            Orders = null;

            _logger.LogError(ex, "Failed to initialise open orders from {FilePath}", filePath);
        }
        finally
        {
            // Always attempt to remove the local copy.
            try
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                    _logger.LogDebug("Temporary open orders file deleted: {TempFilePath}", tempFilePath);
                }
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "Failed to delete temporary open orders file {TempFilePath}", tempFilePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Access denied when deleting temporary open orders file {TempFilePath}", tempFilePath);
            }
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Order>> GetOpenOrdersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<Order> orders = Orders ?? [];

        return Task.FromResult(orders);
    }
}