using DigitalManualSet.Core.Workflow;

namespace DigitalManualSet.Core.PackageCreation.Workflow;

public class CompletePackageStep : WorkflowStep<PackageWorkflowStepId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompletePackageStep"/> class.
    /// </summary>
    /// <param name="package">The package.</param>
    public CompletePackageStep(Package package) 
        : base(PackageWorkflowStepId.CompletePackage, "Complete package", package)
    {


    }
}