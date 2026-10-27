namespace DigitalManualSet.Core.Models;

/// <summary>
/// Represents a removable drive available as a package output destination.
/// </summary>
/// <param name="RootPath">The root path of the drive.</param>
/// <param name="VolumeLabel">The volume label assigned to the drive.</param>
/// <param name="Model">The model name of the drive.</param>
/// <param name="AvailableSpace">The available space on the drive, in bytes.</param>
/// <param name="TotalSpace">The total capacity of the drive, in bytes.</param>
/// <param name="WriteStatus">The write access status of the drive.</param>
/// <param name="IsEffectivelyEmpty">A value indicating whether the drive contains no relevant files.</param>
public sealed record RemovableDrive(
    string RootPath,
    string VolumeLabel,
    string Model,
    long AvailableSpace,
    long TotalSpace,
    DriveWriteStatus WriteStatus,
    bool IsEffectivelyEmpty)
{
    /// <summary>
    /// Gets the space used on the drive, in bytes.
    /// </summary>
    public long UsedSpace => TotalSpace - AvailableSpace;

    /// <summary>
    /// Gets a display-friendly representation of the drive's write status.
    /// </summary>
    public string WriteStatusText => WriteStatus switch
    {
        DriveWriteStatus.Writable => "Writable",
        DriveWriteStatus.WriteProtected => "Write protected",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets a display-friendly representation of whether the drive is empty.
    /// </summary>
    public string IsEmptyText => IsEffectivelyEmpty ? "Empty" : "Contains Files";
}
