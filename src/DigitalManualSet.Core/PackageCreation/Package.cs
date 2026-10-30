using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.Orders;

namespace DigitalManualSet.Core.PackageCreation;

/// <summary>
/// Represents a package being created, containing order and document information.
/// </summary>
public class Package
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the Package class.
    /// </summary>
    public Package()
    {

    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the stage that this package is at.
    /// </summary>
    public PackageStage Stage { get; set; }

    /// <summary>
    /// Gets or sets the USB output details.
    /// </summary>
    public PackageOutput Output { get; set; }

    /// <summary>
    /// Gets or sets the working folder path for the package.
    /// </summary>
    public string WorkingFolder { get; private set; }

    /// <summary>
    /// Gets or sets the order associated with this package.
    /// </summary>
    public Order? Order { get; set; } = null;

    /// <summary>
    /// Gets the list of documents in this package.
    /// </summary>
    public List<Document>? Documents { get; private set; } = null;

    /// <summary>
    /// Gets the document count.
    /// </summary>
    public int DocumentCount => Documents!.Count;

    /// <summary>
    /// Gets the total size of all documents in bytes.
    /// </summary>
    public long TotalDocumentsSize => Documents!.Sum(doc => doc.Length);

    #endregion

    #region Methods

    /// <summary>
    /// Sets the package documents to the provided validated documents.
    /// Clears existing documents before setting new ones.
    /// </summary>
    /// <param name="validatedDocuments">The validated documents to set.</param>
    public void SetValidatedDocuments(IReadOnlyList<Document> validatedDocuments)
    {
        ClearDocuments();

        Documents = [.. validatedDocuments];
    }

    /// <summary>
    /// Sets the working folder path based on the order number and customer name.
    /// </summary>
    /// <param name="rootFolder">The root folder path.</param>
    public void SetWorkingFolder(string rootFolder)
    {
        string folderName = $"{Order?.OrderNumber}-{Order?.CustomerName}";
        WorkingFolder = Path.Combine(rootFolder, folderName);
    }

    /// <summary>
    /// Clears all documents from the package.
    /// </summary>
    public void ClearDocuments()
    {
        Documents?.Clear();
        Documents = null;
    }

    #endregion
}