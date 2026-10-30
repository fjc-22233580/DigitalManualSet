using DigitalManualSet.Core.PackageCreation;

namespace DigitalManualSet.Core.Services;

/// <summary>
/// Generates a splash screen for a package.
/// </summary>
public interface ISplashScreenGenerator
{
    /// <summary>
    /// Generates a splash screen for the specified package.
    /// </summary>
    /// <param name="package">The package for which to generate the splash screen.</param>
    /// <param name="cancellationToken">The token used to cancel generation.</param>
    Task GenerateAsync(Package package, CancellationToken cancellationToken = default);
}
