using DigitalManualSet.Core.Workflow;

namespace DigitalManualSet.Core.PackageCreation.Workflow;

/// <summary>
/// Workflow step representing the "Select Order" stage of the package creation process.
/// </summary>
public sealed class CreateOrderStep : WorkflowStep<PackageWorkflowStepId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateOrderStep" /> class.
    /// </summary>
    public CreateOrderStep()
        : base(PackageWorkflowStepId.CreateOrder, "Select Order")
    {

    }
}
