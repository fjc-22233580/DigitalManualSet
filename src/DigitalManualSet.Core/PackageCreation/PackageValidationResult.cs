namespace DigitalManualSet.Core.PackageCreation;

/// <summary>
/// Contains the outcome of validating a generated package.
/// </summary>
public sealed class PackageValidationResult
{
    /// <summary>
    /// Gets whether validation completed without finding any errors.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Gets the validation errors found within the package.
    /// </summary>
    public List<string> Errors { get; } = [];
}