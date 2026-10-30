/// <summary>
/// Identifies a stage in the package creation workflow.
/// </summary>
public enum PackageCreationStage
{
    /// <summary>Copies the package documents to the output destination.</summary>
    CopyDocuments,

    /// <summary>Generates the package manifest.</summary>
    GenerateManifest,

    /// <summary>Generates the package splash screen.</summary>
    GenerateSplashScreen,

    /// <summary>Generates the package completion certificate.</summary>
    GenerateCertificate,

    /// <summary>Validates the completed package.</summary>
    ValidatePackage,

    /// <summary>Finalizes the output USB volume.</summary>
    FinaliseUsb
}

/// <summary>
/// Represents progress through the package creation workflow.
/// </summary>
/// <param name="Stage">The package creation stage currently in progress.</param>
/// <param name="CurrentStep">The one-based number of the current workflow step.</param>
/// <param name="TotalSteps">The total number of workflow steps.</param>
/// <param name="Status">The human-readable status of the current step.</param>
/// <param name="StageProgress">The progress within the current stage, from zero to one.</param>
/// <param name="Detail">Optional additional information about the current operation.</param>
public sealed record PackageCreationProgress(
    PackageCreationStage Stage,
    int CurrentStep,
    int TotalSteps,
    string Status,
    double StageProgress = 0,
    string? Detail = null)
{
    /// <summary>
    /// Gets the overall package creation percentage based on completed steps and current-stage progress.
    /// </summary>
    public double Percentage => ((CurrentStep - 1) + StageProgress) / TotalSteps * 100;
}
