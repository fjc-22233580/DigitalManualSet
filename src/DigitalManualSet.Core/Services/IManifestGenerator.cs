using DigitalManualSet.Core.PackageCreation;

namespace DigitalManualSet.Core.Services;

/// <summary>
/// Defines a service for generating a manifest for a package.
/// </summary>
public interface IManifestGenerator
{
    /// <summary>
    /// Generates a manifest for the specified package asynchronously.
    /// </summary>
    /// <param name="package">The package for which the manifest is generated.</param>
    /// <param name="cancellationToken">A token that can cancel the generation operation.</param>
    /// <returns>A task that represents the asynchronous manifest generation operation.</returns>
    Task GenerateAsync(Package package, CancellationToken cancellationToken = default);
}
