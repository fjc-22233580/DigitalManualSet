namespace DigitalManualSet.Core.Documents;

/// <summary>
/// Validates documents against the master document register and applies register metadata.
/// </summary>
public interface IDocumentRegisterValidator
{
    /// <summary>
    /// Validates the supplied documents against the document register.
    /// </summary>
    /// <param name="documents">The documents to validate.</param>
    /// <returns>
    /// The documents with register metadata and validation statuses applied.
    /// </returns>
    IReadOnlyList<Document> Validate(IReadOnlyList<Document> documents);
}
