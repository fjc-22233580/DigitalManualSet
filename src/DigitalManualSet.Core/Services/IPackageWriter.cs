using DigitalManualSet.Core.Models;
using DigitalManualSet.Core.PackageCreation;

namespace DigitalManualSet.Core.Services;

/// <summary>
/// The package document writer (to USB).
/// </summary>
public interface IPackageWriter
{
    /// <summary>
    /// Writes the packages documents asynchronous to the respective working folder.
    /// </summary>
    /// <param name="package">The package.</param>
    /// <param name="destination">The destination.</param>
    /// <param name="progress">The progress update mechanism.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task WriteDocumentsAsync(Package package, RemovableDrive destination, IProgress<PackageWriteProgress>? progress = null, CancellationToken cancellationToken = default);
}