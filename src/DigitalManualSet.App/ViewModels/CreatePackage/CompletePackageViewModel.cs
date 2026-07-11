using DigitalManualSet.App.Common;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;

namespace DigitalManualSet.App.ViewModels.CreatePackage;

/// <summary>
/// View model for the "Complete Package" step of the create-package workflow.
/// This implementation is a minimal placeholder that provides a display <see cref="Title"/>.
/// </summary>
public class CompletePackageViewModel : ViewModel, IPackageWorkflowStepViewModel
{
    /// <summary>
    /// Gets the human-readable title for this workflow step.
    /// </summary>
    public string Title => "Placeholder: Complete package.";
}
