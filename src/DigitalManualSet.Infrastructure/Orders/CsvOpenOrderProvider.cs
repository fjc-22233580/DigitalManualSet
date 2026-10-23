using DigitalManualSet.Core.Orders;

namespace DigitalManualSet.Infrastructure.Orders;

public class CsvOpenOrderProvider : IOpenOrderProvider
{
    // Number of columns expected in each row of the open-orders export.
    private const int ExpectedColumnCount = 6;

    // Zero-based index of the System ID column (the last column in the export).
    private const int SystemIdColumnIndex = 5;

    private readonly string _filePath;

    /// <summary>
    /// Initialises a new instance of the
    /// <see cref="CsvOpenOrderProvider"/> class.
    /// </summary>
    /// <param name="filePath">
    /// The path of the CSV file containing the open orders.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is empty.
    /// </exception>
    public CsvOpenOrderProvider(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The open-orders file could not be found.", filePath);
        }

        _filePath = filePath;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Order>> GetOpenOrdersAsync(CancellationToken cancellationToken = default)
    {
        var lines = await File.ReadAllLinesAsync(_filePath, cancellationToken);

        if (lines.Length == 0)
        {
            return Array.Empty<Order>();
        }

        var orders = new List<Order>();

        // The export contains one row per order line, so the same order number can appear many times.
        // Track the order numbers already added so that each order is only returned once.
        var seenOrderNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // The first row is a header and is skipped. Each data row contains:
        // OrderNumber,CustomerName,ShipDate,CatalogNumber,Quantity,SystemID
        foreach (var line in lines.Skip(1))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = ParseLine(line);

            if (values.Count != ExpectedColumnCount)
            {
                throw new InvalidDataException($"The open-orders row does not contain {ExpectedColumnCount} columns: '{line}'.");
            }

            var orderNumber = values[0].Trim();
            var customerName = values[1].Trim();
            var systemId = values[SystemIdColumnIndex].Trim();

            if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(customerName))
            {
                throw new InvalidDataException($"The open-orders row contains an empty value: '{line}'.");
            }

            // Skip rows for systems that are not relevant, and rows for orders
            // that have already been added from an earlier line.
            if (!SystemIdPrefixes.IsValid(systemId) || !seenOrderNumbers.Add(orderNumber))
            {
                continue;
            }

            orders.Add(new Order(orderNumber, customerName, systemId));
        }

        return orders;
    }

    /// <summary>
    /// Splits a single CSV row into its column values.
    /// </summary>
    /// <remarks>
    /// Commas inside double quotes are treated as part of the value, and a
    /// doubled quote (<c>""</c>) inside a quoted value is read as a single quote.
    /// </remarks>
    /// <param name="line">The CSV row to parse.</param>
    /// <returns>The column values in the order they appear in the row.</returns>
    /// <exception cref="InvalidDataException">
    /// Thrown when the row contains an unmatched quote.
    /// </exception>
    private static IReadOnlyList<string> ParseLine(string line)
    {
        var values = new List<string>();
        var currentValue = new System.Text.StringBuilder();
        var isInsideQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];

            if (character == '"')
            {
                var isEscapedQuote =
                    isInsideQuotes &&
                    index + 1 < line.Length &&
                    line[index + 1] == '"';

                if (isEscapedQuote)
                {
                    currentValue.Append('"');
                    index++;
                }
                else
                {
                    isInsideQuotes = !isInsideQuotes;
                }

                continue;
            }

            if (character == ',' && !isInsideQuotes)
            {
                values.Add(currentValue.ToString());
                currentValue.Clear();
                continue;
            }

            currentValue.Append(character);
        }

        if (isInsideQuotes)
        {
            throw new InvalidDataException($"The CSV row contains an unmatched quote: '{line}'.");
        }

        values.Add(currentValue.ToString());

        return values;
    }
}