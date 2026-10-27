namespace DigitalManualSet.Core.PackageCreation;

/// <summary>
/// Specifies the current stage of package creation.
/// </summary>
public enum PackageStage
{
    /// <summary>
    /// The package has been created.
    /// </summary>
    Created,

    /// <summary>
    /// The package documents have been resolved.
    /// </summary>
    DocumentsResolved,

    /// <summary>
    /// The package documents have been validated.
    /// </summary>
    DocumentsValidated,

    /// <summary>
    /// The package documents have been copied to the output destination.
    /// </summary>
    DocumentsCopied,

    /// <summary>
    /// The package has been generated.
    /// </summary>
    PackageGenerated,

    /// <summary>
    /// Package creation is complete.
    /// </summary>
    Complete
}
