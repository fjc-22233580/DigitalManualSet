using DigitalManualSet.Core.PackageCreation;

namespace DigitalManualSet.Core.Services;

/// <summary>
/// Validates the contents and structure of a completed package.
/// </summary>
public interface IPackageValidator
{
    /// <summary>
    /// Validates the specified package.
    /// </summary>
    /// <param name="package">The package to validate.</param>
    /// <param name="cancellationToken">The token used to cancel validation.</param>
    /// <returns>The result of package validation.</returns>
    Task<PackageValidationResult> ValidateAsync(Package package, CancellationToken cancellationToken = default);
}
