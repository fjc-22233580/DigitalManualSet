using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.Orders;
using DigitalManualSet.Core.Workflow;

namespace DigitalManualSet.Core.PackageCreation.Workflow;

/// <summary>
/// Workflow step representing the "Process Documents" stage of the package creation workflow.
/// </summary>
public sealed class ProcessDocumentsStep : WorkflowStep<PackageWorkflowStepId>
{
    private readonly IDocumentProvider _documentProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessDocumentsStep"/> class.
    /// </summary>
    /// <param name="package">The package being created.</param>
    /// <param name="documentProvider">The service to retrieve documents.</param>
    public ProcessDocumentsStep(Package package, IDocumentProvider documentProvider)
        : base(PackageWorkflowStepId.ProcessDocuments, "Process Documents", package)
    {
        _documentProvider = documentProvider;
    }
    
    /// <summary>
    /// Gets the working folder path for the package.
    /// </summary>
    public string WorkingFolder => Package.WorkingFolder;

    /// <summary>
    /// Sets the working folder for document processing.
    /// </summary>
    /// <param name="rootFolder">The root folder path to set.</param>
    public void SetWorkingFolder(string rootFolder)
    {
        Package.SetWorkingFolder(rootFolder);
    }

    /// <summary>
    /// Loads available documents when entering this workflow step.
    /// </summary>
    public override async Task OnEnterAsync()
    {
        Package.ClearDocuments();
    }

    /// <summary>
    /// Gets a value indicating whether the workflow can proceed to the next step.
    /// Returns true only if all documents have been processed.
    /// </summary>
    public override bool CanMoveNext => Package.Documents is not null;

    /// <summary>
    /// Updates the validation state of the package based on document validation status.
    /// Sets validated documents and triggers navigation state change if all are valid.
    /// </summary>
    /// <param name="workingDocuments"></param>
    public void UpdateValidationState(List<Document> workingDocuments)
    {
        if (workingDocuments.All(doc => doc.IsValidated))
        {
            Package.SetValidatedDocuments(workingDocuments);
            OnNavigationStateChanged();
        }
    }
}
