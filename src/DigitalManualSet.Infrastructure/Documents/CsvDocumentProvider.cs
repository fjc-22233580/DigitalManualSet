
using System.Text;
using DigitalManualSet.Core.Documents;

namespace DigitalManualSet.Infrastructure.Documents;

/// <summary>
/// Provides documents by reading them from a CSV file.
/// Supports the default document format and configurable ERP exports.
/// </summary>
public class CsvDocumentProvider : IDocumentProvider
{
    /// <summary>
    /// The expected number of columns in the default document format.
    /// </summary>
    private const int ExpectedColumnCount = 4;

    private static readonly string[] DocumentPrefixes =
    [
        "F",
        "M",
        "HS"
    ];

    private readonly string _filePath;
    private readonly CsvImportDefinition? _importDefinition;

    /// <summary>
    /// Gets the filename of the CSV file without extension.
    /// </summary>
    public string CsvFileName => Path.GetFileNameWithoutExtension(_filePath);

    /// <summary>
    /// Initialises a new instance of the CsvDocumentProvider class.
    /// </summary>
    /// <param name="filePath">The path of the CSV file.</param>
    /// <param name="importDefinition">
    /// Optional ERP import configuration. When null, the default
    /// four-column document format is used.
    /// </param>
    public CsvDocumentProvider(string filePath, CsvImportDefinition? importDefinition = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The documents file could not be found.", filePath);
        }

        // If we have an import definition then validate it.
        if (importDefinition is not null 
            && (importDefinition.DocumentNumberColumnIndex < 0 || importDefinition.DocumentNameColumnIndex < 0))
        {
            throw new ArgumentOutOfRangeException(nameof(importDefinition), "CSV column indices cannot be negative.");
        }

        _filePath = filePath;
        _importDefinition = importDefinition;
    }

    /// <summary>
    /// Reads and parses documents from the CSV file asynchronously.
    /// When an import definition is supplied, only document numbers
    /// beginning with F, M or HS are imported.
    /// </summary>
    /// <inheritdoc />
    public async Task<IReadOnlyList<Document>> GetDocumentsAsync(CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(_filePath);

        // Skip the header row.
        if (await reader.ReadLineAsync(cancellationToken) is null)
        {
            return Array.Empty<Document>();
        }

        var documents = new List<Document>();
        var values = new List<string>();
        var currentValue = new StringBuilder();

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            ParseLine(line, values, currentValue);

            // Document parsing for inbuilt dummy documents
            if (_importDefinition is null)
            {
                // Default four-column document format.
                if (values.Count != ExpectedColumnCount)
                {
                    throw new InvalidDataException($"The documents row does not contain " + $"{ExpectedColumnCount} columns: '{line}'.");
                }

                var documentNumber = values[0].Trim();
                var documentName = values[1].Trim();
                var revision = values[2].Trim();
                var category = values[3].Trim();

                if (string.IsNullOrWhiteSpace(documentNumber) ||
                    string.IsNullOrWhiteSpace(documentName) ||
                    string.IsNullOrWhiteSpace(revision) ||
                    string.IsNullOrWhiteSpace(category))
                {
                    throw new InvalidDataException($"The documents row contains an empty value: '{line}'.");
                }

                documents.Add(new Document(documentNumber, documentName, revision, category));
                continue;
            }

            // Configured ERP export format.
            var numberIndex = _importDefinition.DocumentNumberColumnIndex;
            var nameIndex = _importDefinition.DocumentNameColumnIndex;

            if (values.Count <= Math.Max(numberIndex, nameIndex))
            {
                throw new InvalidDataException($"The CSV row does not contain the configured " + $"columns: '{line}'.");
            }

            // Only import M, F and HS document references.
            if (!HasDocumentPrefix(values[numberIndex].AsSpan().Trim()))
            {
                continue;
            }

            var number = values[numberIndex].Trim();
            var name = values[nameIndex].Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidDataException($"Document '{number}' has no description.");
            }

            // Revision and category will be populated later
            // using the master document register.
            documents.Add(new Document(number, name, string.Empty, string.Empty));
        }

        return documents;
    }

    /// <summary>
    /// Parses a CSV line handling quoted fields and escaped quotes.
    /// </summary>
    /// <param name="line">The CSV line to parse.</param>
    /// <param name="values">The list that receives the parsed column values. Cleared before parsing.</param>
    /// <param name="currentValue">A reusable buffer for building column values. Cleared before parsing.</param>
    /// <exception cref="InvalidDataException">
    /// Thrown when the line contains unmatched quotes.
    /// </exception>
    private static void ParseLine(string line, List<string> values, StringBuilder currentValue)
    {
        values.Clear();
        currentValue.Clear();
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
    }

    /// <summary>
    /// Determines whether a document number starts with a supported prefix.
    /// </summary>
    /// <param name="number">The trimmed document number.</param>
    /// <returns><see langword="true" /> when the number starts with F, M or HS.</returns>
    private static bool HasDocumentPrefix(ReadOnlySpan<char> number)
    {
        foreach (var prefix in DocumentPrefixes)
        {
            if (number.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
