using DigitalManualSet.App.ViewModels.CreatePackage;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;
using DigitalManualSet.Core.PackageCreation.Workflow;
using DigitalManualSet.Core.Workflow;
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

   
    public IPackageWorkflowStepViewModel Resolve(IWorkflowStep<PackageWorkflowStepId> stepId)
    {
        IPackageWorkflowStepViewModel vm;

        switch (stepId.Id)
        {
            case PackageWorkflowStepId.CreateOrder:
                vm = ActivatorUtilities.CreateInstance<CreateOrderViewModel>(_serviceProvider, (CreateOrderStep)stepId);
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
