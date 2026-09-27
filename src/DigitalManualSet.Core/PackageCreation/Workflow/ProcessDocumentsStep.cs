using DigitalManualSet.Core.Workflow;

namespace DigitalManualSet.Core.PackageCreation.Workflow;

/// <summary>
/// Workflow step representing the "Process Documents" stage of the package creation workflow.
/// </summary>
public sealed class ProcessDocumentsStep : WorkflowStep<PackageWorkflowStepId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessDocumentsStep"/> class.
    /// </summary>
    /// <param name="package"></param>
    public ProcessDocumentsStep(Package package)
        : base(PackageWorkflowStepId.ProcessDocuments, "Process Documents", package)
    {

    }
}
