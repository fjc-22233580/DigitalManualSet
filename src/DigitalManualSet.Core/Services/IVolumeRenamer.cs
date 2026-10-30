namespace DigitalManualSet.Core.Services;

/// <summary>
/// Renames the volume label of a package output drive.
/// </summary>
public interface IVolumeRenamer
{
    /// <summary>
    /// Renames the volume at the specified path.
    /// </summary>
    /// <param name="rootPath">The root path of the volume to rename.</param>
    /// <param name="volumeLabel">The new volume label.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    Task RenameAsync(string rootPath, string volumeLabel, CancellationToken cancellationToken = default);
}
