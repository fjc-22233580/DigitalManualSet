using DigitalManualSet.App.Common;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;
using DigitalManualSet.Core.PackageCreation.Workflow;

namespace DigitalManualSet.App.Services;

/// <summary>
/// Resolves an <see cref="IPackageWorkflowStepViewModel"/> instance for a given package workflow step identifier.
/// Implementations map workflow step ids to the appropriate view-model interface used by the UI workflow.
/// </summary>
public interface IPackageWorkflowStepViewModelResolver
{
    /// <summary>
    /// Resolves the view-model interface for the specified workflow step.
    /// </summary>
    /// <param name="stepId">The identifier of the package workflow step to resolve.</param>
    /// <returns>The <see cref="IPackageWorkflowStepViewModel"/> that corresponds to the given step id.</returns>
    IPackageWorkflowStepViewModel Resolve(PackageWorkflowStepId stepId);
}
