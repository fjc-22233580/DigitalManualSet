using DigitalManualSet.App.Common;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;

namespace DigitalManualSet.App.ViewModels.CreatePackage;

/// <summary>
/// View model for the "Select Output" (USB drive or other type of output) step of the create-package workflow.
/// This placeholder exposes a <see cref="Title"/> used by the UI when rendering the step.
/// </summary>
public class SelectOutputViewModel : ViewModel, IPackageWorkflowStepViewModel
{
    /// <summary>
    /// Gets the display title for the workflow step.
    /// </summary>
    public string Title => "Placeholder: select USB drive.";
}
