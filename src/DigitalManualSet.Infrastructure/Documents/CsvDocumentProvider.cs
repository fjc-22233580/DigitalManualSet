using System.Text;
using DigitalManualSet.Core.Documents;

namespace DigitalManualSet.Infrastructure.Documents;

/// <summary>
/// Provides documents by reading them from a CSV file.
/// </summary>
public class CsvDocumentProvider : IDocumentProvider
{
    /// <summary>
    /// The expected number of columns in each document row.
    /// </summary>
    private const int ExpectedColumnCount = 4;

    private readonly string _filePath;

    /// <summary>
    /// Gets the filename of the CSV file without extension.
    /// </summary>
    public string CsvFileName => Path.GetFileNameWithoutExtension(_filePath);

    /// <summary>
    /// Initialises a new instance of the <see cref="CsvDocumentProvider"/> class.
    /// </summary>
    /// <param name="filePath">The path of the CSV file containing the documents.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is null or empty.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the file at <paramref name="filePath"/> does not exist.
    /// </exception>
    public CsvDocumentProvider(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The documents file could not be found.", filePath);
        }

        _filePath = filePath;
    }

    /// <summary>
    /// Reads and parses all documents from the CSV file asynchronously.
    /// Expected CSV format: DocumentNumber,DocumentName,Revision,Category (with header row).
    /// </summary>
    /// <inheritdoc />
    public async Task<IReadOnlyList<Document>> GetDocumentsAsync(CancellationToken cancellationToken = default)
    {
        var lines = await File.ReadAllLinesAsync(_filePath, cancellationToken);

        if (lines.Length == 0)
        {
            return Array.Empty<Document>();
        }

        var documents = new List<Document>();

        // The first row contains:
        // DocumentNumber,DocumentName,Revision,Category
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
                throw new InvalidDataException($"The documents row does not contain {ExpectedColumnCount} columns: '{line}'.");
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
                throw new InvalidDataException(
                    $"The documents row contains an empty value: '{line}'.");
            }

            documents.Add(new Document(documentNumber, documentName, revision, category));
        }

        return documents;
    }

    /// <summary>
    /// Parses a CSV line handling quoted fields and escaped quotes.
    /// </summary>
    /// <param name="line">The CSV line to parse.</param>
    /// <returns>A read-only list of parsed column values.</returns>
    /// <exception cref="InvalidDataException">
    /// Thrown when the line contains unmatched quotes.
    /// </exception>
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