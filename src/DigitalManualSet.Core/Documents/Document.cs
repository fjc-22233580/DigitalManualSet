using DigitalManualSet.Core.Models;

namespace DigitalManualSet.Core.Documents;

/// <summary>
/// Represents a document with metadata and validation state.
/// </summary>
public class Document
{
    /// <summary>
    /// Gets the document's unique identifying number.
    /// </summary>
    public string DocumentNumber { get; }

    /// <summary>
    /// Gets the document's display name.
    /// </summary>
    public string DocumentName { get; }

    /// <summary>
    /// Gets the document's revision level.
    /// </summary>
    public string Revision { get; }

    /// <summary>
    /// Gets the document's category.
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Gets the type of this document.
    /// </summary>
    public DocumentType Type { get; } = DocumentType.Manual;

    /// <summary>
    /// Gets or sets the output relative path of this document when on the destination USB.
    /// </summary>
    public string? OutputRelativePath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document has been validated.
    /// </summary>
    public bool IsValidated { get; set; }

    /// <summary>
    /// Gets or sets the local file path where the document is stored.
    /// </summary>
    public string LocalFilePath { get; set; }

    /// <summary>
    /// Gets the size of the document in bytes.
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    /// Initializes a new instance of the Document class with the specified metadata.
    /// </summary>
    public Document(string documentNumber, string documentName, string revision, string category)
    {
        DocumentNumber = documentNumber;
        DocumentName = documentName;
        Revision = revision;
        Category = category;
    }
}