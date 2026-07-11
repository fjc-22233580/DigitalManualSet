using DigitalManualSet.App.Common;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;

namespace DigitalManualSet.App.ViewModels.CreatePackage;

/// <summary>
/// View model for the "Create Order" step of the create-package workflow.
/// This is a minimal placeholder that exposes a display <see cref="Title"/>.
/// </summary>
public class CreateOrderViewModel : ViewModel, IPackageWorkflowStepViewModel
{
    /// <summary>
    /// Gets the display title for the workflow step.
    /// </summary>
    public string Title => "Placeholder: select order.";
}
