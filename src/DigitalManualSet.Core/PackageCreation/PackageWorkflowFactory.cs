using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.Orders;
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
    /// <param name="openOrderProvider">The open order provider.</param>
    /// <param name="documentProvider">The document provider.</param>
    /// <returns>
    /// A configured package creation workflow.
    /// </returns>
    public static Workflow<PackageWorkflowStepId> Create(IOpenOrderProvider openOrderProvider, IDocumentProvider documentProvider)
    {
        var package = new Package();

        return new Workflow<PackageWorkflowStepId>(
        [
            new CreateOrderStep(package, openOrderProvider),
            new ProcessDocumentsStep(package, documentProvider),
            new SelectOutputStep(package),
        ]);
    }
}