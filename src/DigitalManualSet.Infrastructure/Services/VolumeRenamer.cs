using DigitalManualSet.Core.Models;
using DigitalManualSet.Core.Services;

namespace DigitalManualSet.Infrastructure.Services;

/// <summary>
/// Renames the volume label of the target removable drive.
/// </summary>
public sealed class VolumeRenamer : IVolumeRenamer
{
    /// <summary>
    /// Updates the volume label of the supplied drive.
    /// </summary>
    public Task RenameAsync(string rootPath, string volumeLabel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rootPath);

        if (string.IsNullOrWhiteSpace(volumeLabel))
        {
            throw new ArgumentException("A volume label must be supplied.", nameof(volumeLabel));
        }

        // Volume labels must 11 chars or fewer - actually NTFS supports 32 but for simplicity we assume 11.
        if (volumeLabel.Length > 11)
        {
            throw new ArgumentOutOfRangeException(nameof(volumeLabel), "Volume labels must be 11 characters or fewer.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Recreate the DriveInfo from the root path so the operation
        // targets the currently mounted drive.
        var driveInfo = new DriveInfo(rootPath);

        if (driveInfo.IsReady == false)
        {
            throw new IOException("The target drive is no longer available.");
        }

        // DriveInfo handles the underlying Windows volume rename.
        driveInfo.VolumeLabel = volumeLabel;

        return Task.CompletedTask;
    }
}