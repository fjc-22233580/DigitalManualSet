namespace DigitalManualSet.Core.Documents;

/// <summary>
/// Coordinates importing documents from supported order export files.
/// </summary>
public interface IOrderImportOrchestrator
{
    /// <summary>
    /// Imports documents from the specified files and applies document register validation.
    /// </summary>
    /// <param name="filePaths">The paths of the supported order export files to import.</param>
    /// <param name="cancellationToken">The token used to cancel the import operation.</param>
    /// <returns>The imported, de-duplicated, and validated documents.</returns>
    /// <exception cref="NotSupportedException">A file is not a supported order export.</exception>
    Task<IReadOnlyList<Document>> ImportAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default);
}
