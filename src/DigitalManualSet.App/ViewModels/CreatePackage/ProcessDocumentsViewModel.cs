using DigitalManualSet.App.Common;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;

namespace DigitalManualSet.App.ViewModels.CreatePackage;

/// <summary>
/// View model for the "Process Documents" step of the create-package workflow.
/// This minimal placeholder provides a display <see cref="Title"/> used by the UI.
/// </summary>
public class ProcessDocumentsViewModel : ViewModel, IPackageWorkflowStepViewModel
{
    /// <summary>
    /// Gets the display title for the workflow step.
    /// </summary>
    public string Title => "Placeholder: import documents.";
}
