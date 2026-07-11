using DigitalManualSet.App.ViewModels.CreatePackage;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;
using DigitalManualSet.Core.PackageCreation.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalManualSet.App.Services;

/// <summary>
/// Resolves concrete view-model instances that implement <see cref="IPackageWorkflowStepViewModel"/>
/// for a given <see cref="PackageWorkflowStepId"/>. Uses the application's <see cref="IServiceProvider"/>
/// to obtain the required view-model instances.
/// </summary>
public class PackageWorkflowStepViewModelResolver : IPackageWorkflowStepViewModelResolver
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageWorkflowStepViewModelResolver"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve view-model instances.</param>
    public PackageWorkflowStepViewModelResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Resolves the <see cref="IPackageWorkflowStepViewModel"/> implementation for the specified step id.
    /// </summary>
    /// <param name="stepId">The workflow step identifier to resolve.</param>
    /// <returns>An instance implementing <see cref="IPackageWorkflowStepViewModel"/> for the step.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported <paramref name="stepId"/> is provided.</exception>
    public IPackageWorkflowStepViewModel Resolve(PackageWorkflowStepId stepId)
    {
        IPackageWorkflowStepViewModel vm;

        switch (stepId)
        {
            case PackageWorkflowStepId.CreateOrder:
                vm = _serviceProvider.GetRequiredService<CreateOrderViewModel>();
                break;
            case PackageWorkflowStepId.ProcessDocuments:
                vm = _serviceProvider.GetRequiredService<ProcessDocumentsViewModel>();
                break;
            case PackageWorkflowStepId.SelectOutput:
                vm = _serviceProvider.GetRequiredService<SelectOutputViewModel>();
                break;
            case PackageWorkflowStepId.CompletePackage:
                vm = _serviceProvider.GetRequiredService<CompletePackageViewModel>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(stepId), stepId, null);
        }

        return vm;
    }
}
