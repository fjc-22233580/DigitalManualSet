
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.Models;

namespace DigitalManualSet.Infrastructure.Documents;

/// <summary>
/// Validates imported documents against the master document register
/// and applies authoritative document metadata.
/// </summary>
public class DocumentRegisterValidator : IDocumentRegisterValidator
{
    /// <inheritdoc />
    public IReadOnlyList<Document> Validate(IReadOnlyList<Document> documents, IReadOnlyDictionary<string, DocumentRegisterEntry> register)
    {
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(register);

        foreach (var document in documents)
        {
            // Determine a fallback document type from its number.
            document.Type = DetermineDocumentType(
                document.DocumentNumber);

            if (register.TryGetValue(document.DocumentNumber.Trim(), out var entry) == false)
            {
                document.RegisterStatus = DocumentRegisterStatus.NotFound;
                document.LifecyclePhase = null;
                continue;
            }

            // Apply metadata from the master register.
            if (!string.IsNullOrWhiteSpace(entry.Description))
            {
                document.DocumentName = entry.Description;
            }

            document.Revision = entry.Revision;
            document.LifecyclePhase = entry.LifecyclePhase;

            // Determine the document type.
            document.Type = DetermineDocumentType(
                document.DocumentNumber,
                entry.PartType);

            // Determine lifecycle status.
            var phase = entry.LifecyclePhase.Trim();

            document.RegisterStatus = phase.ToUpperInvariant() switch
            {
                "OBSOLETE" => DocumentRegisterStatus.Obsolete,
                "PRODUCTION" or "CUSTOM" => DocumentRegisterStatus.Active,
                _ => DocumentRegisterStatus.RequiresReview
            };

            // Missing revisions require manual investigation.
            if (string.IsNullOrWhiteSpace(entry.Revision))
            {
                document.RegisterStatus = DocumentRegisterStatus.RequiresReview;
            }
        }

        return documents;
    }

    /// <summary>
    /// Determines the document type using master register metadata,
    /// falling back to the document number prefix.
    /// </summary>
    private static DocumentType DetermineDocumentType(string documentNumber, string? partType = null)
    {
        // Normalise ERP part types, e.g. "(Drawing)" -> "DRAWING".
        var normalisedPartType = partType?
            .Trim()
            .Trim('(', ')')
            .Trim()
            .ToUpperInvariant();

        switch (normalisedPartType)
        {
            case "DRAWING":
            case "DRAWINGS":
                return DocumentType.Drawing;

            case "MANUAL":
            case "MANUALS":
                return DocumentType.Manual;

            case "CERT":
            case "CERTIFICATE":
            case "CERTIFICATES":
                return DocumentType.Certificate;
        }

        // Fallback classification by document number.
        if (documentNumber.StartsWith("F", StringComparison.OrdinalIgnoreCase))
        {
            return DocumentType.Drawing;
        }

        if (documentNumber.StartsWith("M", StringComparison.OrdinalIgnoreCase) || documentNumber.StartsWith("HS", StringComparison.OrdinalIgnoreCase))
        {
            return DocumentType.Manual;
        }

        return DocumentType.Other;
    }
}
