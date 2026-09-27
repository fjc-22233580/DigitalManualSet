using DigitalManualSet.Core.Workflow;

namespace DigitalManualSet.Core.PackageCreation.Workflow;

/// <summary>
/// Represents the workflow step where the user selects the target output.
/// </summary>
public sealed class SelectOutputStep : WorkflowStep<PackageWorkflowStepId>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SelectOutputStep"/> class.
    /// </summary>
    /// <param name="package"></param>
    public SelectOutputStep(Package package)
        : base(PackageWorkflowStepId.SelectOutput, "Select Output", package)
    {

    }
}