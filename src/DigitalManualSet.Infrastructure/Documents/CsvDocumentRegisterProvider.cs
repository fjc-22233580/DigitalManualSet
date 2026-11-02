
using System.Text;
using DigitalManualSet.Core.Documents;

namespace DigitalManualSet.Infrastructure.Documents;

/// <summary>
/// Provides the master document register from a CSV file.
/// </summary>
public class CsvDocumentRegisterProvider : IDocumentRegisterProvider
{
    private readonly string _filePath;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, DocumentRegisterEntry> Register { get; private set; } =
        new Dictionary<string, DocumentRegisterEntry>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initialises a new instance of the CsvDocumentRegisterProvider class.
    /// </summary>
    public CsvDocumentRegisterProvider(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The master document register could not be found.", filePath);
        }

        _filePath = filePath;
    }

    /// <inheritdoc />
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var lines = await File.ReadAllLinesAsync(
            _filePath, cancellationToken);

        if (lines.Length == 0)
        {
            throw new InvalidDataException("The master document register is empty.");
        }

        // Identify columns from the header row.
        var headers = ParseLine(lines[0]);

        var numberIndex = GetColumnIndex(headers, "Number");
        var descriptionIndex = GetColumnIndex(headers, "Description");
        var lifecycleIndex = GetColumnIndex(headers, "Lifecycle Phase");
        var revisionIndex = GetColumnIndex(headers, "Rev");
        var partTypeIndex = GetColumnIndex(headers, "Part Type");

        var maxIndex = new[]
        {
            numberIndex,
            descriptionIndex,
            lifecycleIndex,
            revisionIndex,
            partTypeIndex
        }.Max();

        var documents = new Dictionary<string, DocumentRegisterEntry>(StringComparer.OrdinalIgnoreCase);

        // Skip the header row.
        for (var index = 1; index < lines.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = lines[index];

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = ParseLine(line);

            if (values.Count <= maxIndex)
            {
                throw new InvalidDataException($"Invalid master register row at line {index + 1}.");
            }

            var number = values[numberIndex].Trim();
            var description = values[descriptionIndex].Trim();
            var lifecycle = values[lifecycleIndex].Trim();
            var revision = values[revisionIndex].Trim();
            var partType = values[partTypeIndex].Trim();

            if (string.IsNullOrWhiteSpace(number))
            {
                throw new InvalidDataException($"Empty document number at line {index + 1}.");
            }

            var document = new DocumentRegisterEntry(
                number,
                description,
                lifecycle,
                revision,
                partType);

            if (!documents.TryAdd(number, document))
            {
                throw new InvalidDataException($"Duplicate document number '{number}' at line {index + 1}.");
            }
        }

        Register = documents;
    }

    /// <summary>
    /// Returns the index of a named CSV column.
    /// </summary>
    private static int GetColumnIndex(IReadOnlyList<string> headers, string columnName)
    {
        for (var index = 0; index < headers.Count; index++)
        {
            if (string.Equals(headers[index].Trim().TrimStart('\uFEFF'), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        throw new InvalidDataException($"Required CSV column '{columnName}' was not found.");
    }

    /// <summary>
    /// Parses a CSV line handling quoted fields and escaped quotes.
    /// </summary>
    private static IReadOnlyList<string> ParseLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
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
