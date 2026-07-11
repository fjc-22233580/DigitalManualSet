using DigitalManualSet.Core.PackageCreation.Workflow;
using DigitalManualSet.Core.Workflow;

namespace DigitalManualSet.Core.PackageCreation;

/// <summary>
/// Creates configured instances of the package creation workflow.
/// </summary>
public static class PackageWorkflowFactory
{
    /// <summary>
    /// Creates the default package creation workflow.
    /// </summary>
    /// <returns>
    /// A configured package creation workflow.
    /// </returns>
    public static Workflow<PackageWorkflowStepId> Create()
    {
        return new Workflow<PackageWorkflowStepId>(
        [
            new CreateOrderStep(),
            new ProcessDocumentsStep(),
            new SelectOutputStep(),
        ]);
    }
}