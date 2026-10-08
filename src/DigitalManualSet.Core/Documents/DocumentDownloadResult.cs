namespace DigitalManualSet.Core.Documents;

/// <summary>
/// Represents the result of a document download operation.
/// </summary>
/// <param name="Success">Indicates whether the download was successful.</param>
/// <param name="FilePath">The local file path where the document was saved, if successful; otherwise null.</param>
/// <param name="ErrorMessage">The error message if the download failed; otherwise null.</param>
public record DocumentDownloadResult(bool Success, string? FilePath, string? ErrorMessage);