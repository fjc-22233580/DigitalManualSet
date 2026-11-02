
using DigitalManualSet.Core.Documents;

namespace DigitalManualSet.Infrastructure.Documents;

/// <summary>
/// Coordinates the import of documents from supported ERP CSV exports.
/// </summary>
public class OrderImportOrchestrator : IOrderImportOrchestrator
{
    private readonly IDocumentRegisterValidator _validator;

    /// <summary>
    /// Defines the supported ERP exports and their document column mappings.
    /// </summary>
    private static readonly Dictionary<string, CsvImportDefinition> ImportDefinitions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // ProductStructureCP export:
            // Component Part, Component Part Description
            ["ProductStructureCP"] = new(8, 9),

            // QuickReportShopOrderManualandFDrgList export:
            // "Part No", "Description"   
            ["QuickReportShopOrderManualandFDrgList"] = new(3, 4)
        };


    /// <summary>
    /// Initializes a new instance of the <see cref="OrderImportOrchestrator"/> class.
    /// </summary>
    /// <param name="validator">The validator.</param>
    public OrderImportOrchestrator(IDocumentRegisterValidator validator)
    {
        _validator = validator;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Document>> ImportAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePaths);

        var documents = new List<Document>();

        foreach (var filePath in filePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(filePath);

            var definition = GetImportDefinition(fileName);

            var provider = new CsvDocumentProvider(filePath, definition);

            var importedDocuments = await provider.GetDocumentsAsync(cancellationToken);

            documents.AddRange(importedDocuments);
        }

        await Task.Delay(300);

        // Remove duplicates from the imported documents.
        var uniqueDocuments = documents.DistinctBy(d => d.DocumentNumber, StringComparer.OrdinalIgnoreCase).ToList();

        // Apply current revisions and lifecycle statuses.
        return _validator.Validate(uniqueDocuments);

    }

    /// <summary>
    /// Identifies the ERP export type from its filename.
    /// </summary>
    private static CsvImportDefinition GetImportDefinition(string fileName)
    {
        var definition = ImportDefinitions.FirstOrDefault(
            entry => fileName.StartsWith(
                entry.Key,
                StringComparison.OrdinalIgnoreCase));

        if (definition.Key is null)
        {
            throw new NotSupportedException($"Unsupported ERP CSV export: '{fileName}'.");
        }

        return definition.Value;
    }
}
