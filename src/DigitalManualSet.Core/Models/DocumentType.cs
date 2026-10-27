namespace DigitalManualSet.Core.Models;

/// <summary>
/// Specifies the category of a document.
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// A manual document.
    /// </summary>
    Manual,

    /// <summary>
    /// A drawing document.
    /// </summary>
    Drawing,

    /// <summary>
    /// A certificate document.
    /// </summary>
    Certificate,

    /// <summary>
    /// A document that does not match another category.
    /// </summary>
    Other
}
