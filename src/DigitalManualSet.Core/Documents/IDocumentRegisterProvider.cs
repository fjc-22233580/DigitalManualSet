namespace DigitalManualSet.Core.Documents;

/// <summary>
/// Provides document register entries used to validate imported documents.
/// </summary>
public interface IDocumentRegisterProvider
{
    /// <summary>
    /// Loads the document register entries asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the load operation.</param>
    /// <returns>A case-insensitive mapping from document number to register entry.</returns>
    Task<IReadOnlyDictionary<string, DocumentRegisterEntry>> LoadAsync(CancellationToken cancellationToken = default);
}
