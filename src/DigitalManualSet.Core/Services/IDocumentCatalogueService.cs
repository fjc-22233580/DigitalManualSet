using DigitalManualSet.Core.Models;

namespace DigitalManualSet.Core.Services;

/// <summary>
/// Loads and provides access to the documents available for each model.
/// </summary>
public interface IDocumentCatalogueService
{
    /// <summary>
    /// Loads the document catalogue from its configured source.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the load operation.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    Task LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the documents associated with a model number.
    /// </summary>
    /// <param name="modelNumber">The model number to search for.</param>
    /// <returns>The documents for the model, or an empty list when no documents are found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the catalogue has not been loaded.</exception>
    IReadOnlyList<CatalogueDocument> GetDocumentsForModel(string modelNumber);
}
