namespace DigitalManualSet.Core.Documents;

/// <summary>
/// Provides document register entries used to validate imported documents.
/// </summary>
public interface IDocumentRegisterProvider
{
    /// <summary>
    /// Gets the loaded document register entries keyed by document number.
    /// </summary>
    IReadOnlyDictionary<string, DocumentRegisterEntry> Register { get; }

    /// <summary>
    /// Loads the document register entries asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the load operation.</param>
    Task LoadAsync(CancellationToken cancellationToken = default);
}
