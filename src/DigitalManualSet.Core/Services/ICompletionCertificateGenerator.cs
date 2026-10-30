using DigitalManualSet.Core.PackageCreation;

namespace DigitalManualSet.Core.Services;

/// <summary>
/// Generates a completion certificate for a package.
/// </summary>
public interface ICompletionCertificateGenerator
{
    /// <summary>
    /// Generates a completion certificate for the specified package.
    /// </summary>
    /// <param name="package">The package for which to generate the certificate.</param>
    /// <param name="cancellationToken">The token used to cancel generation.</param>
    Task GenerateAsync(Package package, CancellationToken cancellationToken = default);
}
