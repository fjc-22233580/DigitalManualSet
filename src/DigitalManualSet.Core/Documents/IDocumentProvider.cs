
namespace DigitalManualSet.Core.Documents;

/// <summary>
/// Provides access to documents from a data source.
/// </summary>
public interface IDocumentProvider
{
    /// <summary>
    /// Gets the CSV filename associated with the document provider.
    /// </summary>
    public string CsvFileName { get; }

    /// <summary>
    /// Retrieves a list of documents asynchronously.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only collection of documents.
    /// </returns>
    Task<IReadOnlyList<Document>> GetDocumentsAsync(CancellationToken cancellationToken = default);
}