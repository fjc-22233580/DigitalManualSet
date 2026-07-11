namespace DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;

/// <summary>
/// Backing interface for view models that represent a step in the "Create Package" workflow.
/// Implementations should provide a human-readable <see cref="Title"/> used by the UI when
/// rendering the workflow step (for example in headers or progress indicators).
/// </summary>
public interface IPackageWorkflowStepViewModel
{
    /// <summary>
    /// Gets the display title for the workflow step.
    /// </summary>
    string Title { get; }
}
