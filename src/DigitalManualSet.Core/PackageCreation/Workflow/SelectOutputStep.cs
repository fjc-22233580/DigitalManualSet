using DigitalManualSet.Core.Models;
using DigitalManualSet.Core.Services;
using DigitalManualSet.Core.Workflow;

namespace DigitalManualSet.Core.PackageCreation.Workflow;

/// <summary>
/// Represents the workflow step where the user selects the target output.
/// </summary>
public sealed class SelectOutputStep : WorkflowStep<PackageWorkflowStepId>
{
    #region Fields

    private readonly IPackageWriter _packageWriter;

    #endregion

    #region Constructor

    /// <summary>
    /// Initialises a new instance of the <see cref="SelectOutputStep"/> class.
    /// </summary>
    /// <param name="package">The package whose documents will be copied.</param>
    /// <param name="packageWriter">The writer used to copy package documents to the selected drive.</param>
    public SelectOutputStep(Package package, IPackageWriter packageWriter)
        : base(PackageWorkflowStepId.SelectOutput, "Select Output", package)
    {
        _packageWriter = packageWriter;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the removable drive selected as the package output destination.
    /// </summary>
    public RemovableDrive? SelectedDrive { get; set; }

    /// <summary>
    /// Gets a value indicating whether the package documents have been copied successfully.
    /// </summary>
    public bool IsComplete { get; private set; }

    /// <summary>
    /// Gets a value indicating whether navigation to the next workflow step is allowed.
    /// </summary>
    public override bool CanMoveNext => IsComplete;

    #endregion

    #region Methods

    /// <summary>
    /// Copies the package documents to the selected output drive.
    /// </summary>
    /// <param name="progress">An optional progress reporter for the copy operation.</param>
    /// <param name="cancellationToken">A token that can cancel the copy operation.</param>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no output drive is selected.</exception>
    public async Task CopyDocumentsAsync(IProgress<PackageWriteProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (SelectedDrive is null)
        {
            throw new InvalidOperationException("A USB drive must be selected before copying documents.");
        }

        await _packageWriter.WriteDocumentsAsync(
            Package,
            SelectedDrive,
            progress,
            cancellationToken);

        MarkComplete();
    }

    /// <summary>
    /// Marks the workflow step as complete and records the package output location.
    /// </summary>
    private void MarkComplete()
    {
        Package.Stage = PackageStage.DocumentsCopied;

        Package.Output = new PackageOutput
        {
            RootPath = SelectedDrive!.RootPath,
            DocumentsCopiedAt = DateTime.Now
        };

        IsComplete = true;

        OnNavigationStateChanged();
    }

    #endregion

}