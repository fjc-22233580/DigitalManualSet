using DigitalManualSet.App.Common;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;
using DigitalManualSet.Core.PackageCreation.Workflow;
using DigitalManualSet.Core.Workflow;

namespace DigitalManualSet.App.ViewModels.CreatePackage;

/// <summary>
/// View model for the "Complete Package" step of the create-package workflow.
/// This implementation is a minimal placeholder that provides a display <see cref="Title"/>.
/// </summary>
public class CompletePackageViewModel : ViewModel, IPackageWorkflowStepViewModel
{
    private readonly CompletePackageStep _Model;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompletePackageViewModel"/> class.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <exception cref="System.ArgumentException">Expected a {nameof(CreateOrderStep)}. - step</exception>
    public CompletePackageViewModel(CompletePackageStep step)
    {
        _Model = step ?? throw new ArgumentException($"Expected a {nameof(CreateOrderStep)}.", nameof(step));

    }

    /// <summary>
    /// Gets the human-readable title for this workflow step.
    /// </summary>
    public string Title => "Placeholder: Complete package.";
}
