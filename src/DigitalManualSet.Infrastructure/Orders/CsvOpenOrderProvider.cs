using DigitalManualSet.Core.Orders;

namespace DigitalManualSet.Infrastructure.Orders;

public class CsvOpenOrderProvider : IOpenOrderProvider
{
    private const int ExpectedColumnCount = 3;

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

        // The first row contains:
        // OrderNumber,CustomerName,SystemID
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
            var systemId = values[2].Trim();

            if (string.IsNullOrWhiteSpace(orderNumber) ||
                string.IsNullOrWhiteSpace(customerName) ||
                string.IsNullOrWhiteSpace(systemId))
            {
                throw new InvalidDataException(
                    $"The open-orders row contains an empty value: '{line}'.");
            }

            orders.Add(new Order(orderNumber, customerName, systemId));
        }

        return orders;
    }

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