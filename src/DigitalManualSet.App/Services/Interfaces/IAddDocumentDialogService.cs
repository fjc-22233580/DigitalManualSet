using DigitalManualSet.Core.Documents;

namespace DigitalManualSet.App.Services.Interfaces;

/// <summary>
/// Defines a service for displaying the add-document dialog.
/// </summary>
public interface IAddDocumentDialogService
{
    /// <summary>
    /// Displays the add-document dialog and returns the selected document.
    /// </summary>
    /// <returns>The selected document, or <see langword="null"/> if the dialog is cancelled.</returns>
    Document? ShowDialog();
}
