namespace DigitalManualSet.Core.Documents;

/// <summary>
/// Represents the result of a document validation operation.
/// </summary>
/// <param name="IsValid">Indicates whether all documents passed validation.</param>
/// <param name="Errors">A read-only list of validation error messages, if any.</param>
public record DocumentValidationResult(bool IsValid, IReadOnlyList<string> Errors);
