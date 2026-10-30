using DigitalManualSet.Core.Models;
using DigitalManualSet.Core.Orders;
using DigitalManualSet.Core.Services;
using DigitalManualSet.Core.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalManualSet.Core.PackageCreation.Workflow;

/// <summary>
/// Workflow step that creates, validates, and finalizes a package on a removable drive.
/// </summary>
public class CompletePackageStep : WorkflowStep<PackageWorkflowStepId>
{
    #region Consts

    private static readonly TimeSpan StepDelay = TimeSpan.FromMilliseconds(500);

    #endregion

    #region Fields

    private readonly IPackageWriter _packageWriter;
    private readonly IManifestGenerator _manifestGenerator;
    private readonly ICompletionCertificateGenerator _certificateGenerator;
    private readonly ISplashScreenGenerator _splashScreenGenerator;
    private readonly IPackageValidator _packageValidator;
    private readonly IVolumeRenamer _volumeRenamer;

    #endregion


    /// <summary>
    /// Initializes a new instance of the <see cref="CompletePackageStep"/> class.
    /// </summary>
    /// <param name="package">The package to create.</param>
    /// <param name="serviceProvider">The service provider used to resolve package creation services.</param>
    public CompletePackageStep(Package package, IServiceProvider serviceProvider) 
        : base(PackageWorkflowStepId.CompletePackage, "Complete package", package)
    {
        _packageWriter = serviceProvider.GetRequiredService<IPackageWriter>();
        _manifestGenerator = serviceProvider.GetRequiredService<IManifestGenerator>();
        _certificateGenerator = serviceProvider.GetRequiredService<ICompletionCertificateGenerator>();
        _splashScreenGenerator = serviceProvider.GetRequiredService<ISplashScreenGenerator>();
        _packageValidator = serviceProvider.GetRequiredService<IPackageValidator>();
        _volumeRenamer = serviceProvider.GetRequiredService<IVolumeRenamer>();
    }

    #region Properties

    /// <summary>
    /// Gets the order associated with the package.
    /// </summary>
    public Order Order => Package.Order!;

    /// <summary>
    /// Gets the number of documents in the package.
    /// </summary>
    public int DocumentCount => Package.DocumentCount;

    /// <summary>
    /// Gets the total size of the documents in the package.
    /// </summary>
    public long TotalDocumentsSize => Package.TotalDocumentsSize;

    /// <summary>
    /// Gets or sets the removable drive selected as the package output destination.
    /// </summary>
    public RemovableDrive? SelectedDrive { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Creates the package, generates its supporting files, validates it, and finalizes the USB volume.
    /// </summary>
    /// <param name="progress">The optional progress reporter.</param>
    /// <param name="cancellationToken">The token used to cancel package creation.</param>
    /// <exception cref="InvalidOperationException">The completed package fails validation.</exception>
    public async Task CreatePackageAsync(IProgress<PackageCreationProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        const int totalSteps = 6;

        // 1. Copy documents
        progress?.Report(new PackageCreationProgress(
            Stage: PackageCreationStage.CopyDocuments,
            CurrentStep: 1,
            TotalSteps: totalSteps,
            Status: "Preparing to copy documents..."));

        await _packageWriter.WriteDocumentsAsync(Package, SelectedDrive!, progress, cancellationToken);

        // Cache copying info
        Package.Output = new PackageOutput
        {
            RootPath = SelectedDrive!.RootPath,
            DocumentsCopiedAt = DateTime.Now
        };

        await Task.Delay(StepDelay, cancellationToken);


        // 2. Generate manifest
        progress?.Report(new PackageCreationProgress(
            Stage: PackageCreationStage.GenerateManifest,
            CurrentStep: 2,
            TotalSteps: totalSteps,
            Status: "Generating manifest...", 
            StageProgress: 1));

        await _manifestGenerator.GenerateAsync(Package, cancellationToken);
        await Task.Delay(StepDelay, cancellationToken);


        // 3. Generate splash screen
        progress?.Report(new PackageCreationProgress(
            Stage: PackageCreationStage.GenerateSplashScreen,
            CurrentStep: 3,
            TotalSteps: totalSteps,
            Status: "Generating splash screen...", 
            StageProgress: 1));

        await _splashScreenGenerator.GenerateAsync(Package, cancellationToken);
        await Task.Delay(StepDelay, cancellationToken);


        // 4. Generate completion certificate
        progress?.Report(new PackageCreationProgress(
            Stage: PackageCreationStage.GenerateCertificate,
            CurrentStep: 4,
            TotalSteps: totalSteps,
            Status: "Generating completion certificate...", 
            StageProgress: 1));

        await _certificateGenerator.GenerateAsync(Package, cancellationToken);
        await Task.Delay(StepDelay, cancellationToken);


        // 5. Validate completed package
        progress?.Report(new PackageCreationProgress(
            Stage: PackageCreationStage.ValidatePackage,
            CurrentStep: 5,
            TotalSteps: totalSteps,
            Status: "Validating package...", 
            StageProgress: 1));

        var validationResult = await _packageValidator.ValidateAsync(Package, cancellationToken);
        await Task.Delay(StepDelay, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Package validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        // 6. Finalise USB
        progress?.Report(new PackageCreationProgress(
            Stage: PackageCreationStage.FinaliseUsb,
            CurrentStep: 6,
            TotalSteps: totalSteps,
            Status: "Finalising USB...", 
            StageProgress: 1));

        await _volumeRenamer.RenameAsync(Package.Output.RootPath, $"{Package.Order!.OrderNumber}-DOC", cancellationToken);
        await Task.Delay(StepDelay, cancellationToken);

        // All six stages complete
        progress?.Report(new PackageCreationProgress(
            Stage: PackageCreationStage.FinaliseUsb,
            CurrentStep: 6,
            TotalSteps: totalSteps,
            Status: "Package complete.",
            StageProgress: 1));
    }

    #endregion
}