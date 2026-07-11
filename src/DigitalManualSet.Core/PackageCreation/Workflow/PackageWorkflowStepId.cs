namespace DigitalManualSet.Core.PackageCreation.Workflow;

/// <summary>
/// Identifies the steps used by the package creation workflow.
/// </summary>
public enum PackageWorkflowStepId
{
    /// <summary>
    /// The step where the user selects the order or source for the package (displayed as "Select Order").
    /// </summary>
    CreateOrder,

    /// <summary>
    /// The step where documents are imported and prepared for inclusion in the package
    /// (displayed as "Process Documents").
    /// </summary>
    ProcessDocuments,

    /// <summary>
    /// The step where the user selects the output for the package.
    /// </summary>
    SelectOutput,

    /// <summary>
    /// The final step shown when the package workflow is complete.
    /// </summary>
    CompletePackage
}