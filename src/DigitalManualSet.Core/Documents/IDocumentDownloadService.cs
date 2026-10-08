namespace DigitalManualSet.Core.Documents;

/// <summary>
/// Provides functionality for downloading documents.
/// </summary>
public interface IDocumentDownloadService
{
    /// <summary>
    /// Downloads a document to the specified destination directory asynchronously.
    /// </summary>
    /// <param name="document">The document to download.</param>
    /// <param name="destinationDirectory">The directory where the document will be saved.</param>
    /// <param name="progress">Optional progress reporter for download status messages.</param>
    /// <returns>A task representing the asynchronous download operation.</returns>
    Task<DocumentDownloadResult> DownloadAsync(Document document, string destinationDirectory, IProgress<string>? progress = null);
}