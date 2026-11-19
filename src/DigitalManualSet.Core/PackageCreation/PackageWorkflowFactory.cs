using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.Orders;
using DigitalManualSet.Core.PackageCreation.Workflow;
using DigitalManualSet.Core.Services;
using DigitalManualSet.Core.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DigitalManualSet.Core.PackageCreation;

/// <summary>
/// Creates configured instances of the package creation workflow.
/// </summary>
public static class PackageWorkflowFactory
{
    /// <summary>
    /// Creates the default package creation workflow.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>
    /// A configured package creation workflow.
    /// </returns>
    public static Workflow<PackageWorkflowStepId> Create(IServiceProvider serviceProvider)
    {
        var package = new Package();

        return new Workflow<PackageWorkflowStepId>(
        [
            new CreateOrderStep(
                package,
                serviceProvider.GetRequiredService<IOpenOrderProvider>(),
                serviceProvider.GetRequiredService<ILogger<CreateOrderStep>>()),
            new ProcessDocumentsStep(
                package,
                serviceProvider.GetRequiredService<IDocumentProvider>(),
                serviceProvider.GetRequiredService<ILogger<ProcessDocumentsStep>>()),
            new CompletePackageStep(package, serviceProvider)
        ]);
    }
}