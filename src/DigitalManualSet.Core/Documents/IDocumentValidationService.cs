namespace DigitalManualSet.Core.Documents;

/// <summary>
/// Provides functionality for validating documents.
/// </summary>
public interface IDocumentValidationService
{
    /// <summary>
    /// Validates a collection of documents in the specified working directory.
    /// </summary>
    /// <param name="documents">The documents to validate.</param>
    /// <param name="workingDirectory">The directory containing the documents to validate.</param>
    /// <returns>The validation result containing success status and any errors.</returns>
    DocumentValidationResult Validate(IEnumerable<Document> documents, string workingDirectory);
}