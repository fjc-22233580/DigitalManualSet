using DigitalManualSet.App.Common;
using System.IO;
using DigitalManualSet.Core.Documents;

namespace DigitalManualSet.App.ViewModels.CreatePackage.Dialogs;

/// <summary>
/// View model for the add-document dialog.
/// </summary>
public class AddDocumentDialogViewModel : ViewModel
{
    #region Properties

    /// <summary>
    /// Initializes a new instance of the <see cref="AddDocumentDialogViewModel"/> class.
    /// </summary>
    public AddDocumentDialogViewModel()
    {

    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the name of the document to add.
    /// </summary>
    public string DocumentName
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanAdd));
        }
    }

    /// <summary>
    /// Gets or sets the path to the document file to add.
    /// </summary>
    public string FilePath
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanAdd));
        }
    }

    /// <summary>
    /// Gets a value indicating whether the entered document can be added.
    /// </summary>
    public bool CanAdd =>
        !string.IsNullOrWhiteSpace(DocumentName) &&
        !string.IsNullOrWhiteSpace(FilePath) &&
        File.Exists(FilePath);

    /// <summary>
    /// Gets the document created by the last successful call to <see cref="TryAdd"/>.
    /// </summary>
    public Document? Result { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a document from the entered values when they are valid.
    /// </summary>
    /// <returns><see langword="true"/> if the document was created; otherwise, <see langword="false"/>.</returns>
    public bool TryAdd()
    {
        if (!CanAdd)
        {
            return false;
        }

        string fileName = Path.GetFileName(FilePath);
        Result = new Document(fileName, DocumentName, "N/A", "N/A");

        return true;
    }

    #endregion
}