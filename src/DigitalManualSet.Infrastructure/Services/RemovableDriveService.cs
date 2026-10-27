using DigitalManualSet.Core.Models;
using DigitalManualSet.Core.Services;
using DigitalManualSet.Infrastructure.Windows;
using System.Management;

namespace DigitalManualSet.Infrastructure.Services;

/// <summary>
/// Discovers and describes removable drives available on the system.
/// </summary>
public class RemovableDriveService : IRemovableDriveService
{
    /// <summary>
    /// Gets the removable drives that are currently ready for use.
    /// </summary>
    /// <returns>A read-only list of available removable drives.</returns>
    public IReadOnlyList<RemovableDrive> GetAvailableDrives()
    {
        return DriveInfo
            .GetDrives()
            .Where(IsSuitableDrive)
            .Select(CreateRemovableDrive)
            .ToList();
    }

    /// <summary>
    /// Determines whether a drive is a ready removable drive.
    /// </summary>
    /// <param name="drive">The drive to evaluate.</param>
    /// <returns><see langword="true"/> when the drive is removable and ready; otherwise, <see langword="false"/>.</returns>
    private static bool IsSuitableDrive(DriveInfo drive)
    {
        return drive is { DriveType: DriveType.Removable, IsReady: true };
    }

    /// <summary>
    /// Creates a removable-drive model from the specified system drive.
    /// </summary>
    /// <param name="drive">The system drive to describe.</param>
    /// <returns>A model containing the drive's metadata and write status.</returns>
    private static RemovableDrive CreateRemovableDrive(DriveInfo drive)
    {
        var rootPath = drive.RootDirectory.FullName;

        return new RemovableDrive(
            RootPath: rootPath,
            VolumeLabel: drive.VolumeLabel,
            Model: GetPhysicalDriveModel(drive.Name),
            AvailableSpace: drive.AvailableFreeSpace,
            TotalSpace: drive.TotalSize,
            WriteStatus: DriveWriteStatusChecker.GetWriteStatus(drive.RootDirectory.FullName),
            IsEffectivelyEmpty: IsEffectivelyEmpty(rootPath));
    }

    /// <summary>
    /// Gets the physical model name associated with a logical drive.
    /// </summary>
    /// <param name="driveName">The logical drive name.</param>
    /// <returns>The physical drive model name, or a fallback name when it cannot be determined.</returns>
    private static string GetPhysicalDriveModel(string driveName)
    {
        var logicalDrive = driveName.TrimEnd('\\');

        using var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_LogicalDisk WHERE DeviceID = '{logicalDrive}'");

        foreach (ManagementObject disk in searcher.Get())
        {
            foreach (ManagementObject partition in disk.GetRelated("Win32_DiskPartition"))
            {
                foreach (ManagementObject physicalDisk in partition.GetRelated("Win32_DiskDrive"))
                {
                    return physicalDisk["Model"]?.ToString()
                           ?? "Unknown USB Device";
                }
            }
        }

        return "Unknown USB Device";
    }

    /// <summary>
    /// Determines whether a drive contains no entries other than standard system entries.
    /// </summary>
    /// <param name="rootPath">The root path of the drive to inspect.</param>
    /// <returns><see langword="true"/> when the drive is effectively empty; otherwise, <see langword="false"/>.</returns>
    private static bool IsEffectivelyEmpty(string rootPath)
    {
        var ignoredEntries = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            "System Volume Information",
            "$RECYCLE.BIN"
        };

        try
        {
            return !Directory.EnumerateFileSystemEntries(rootPath)
                .Any(path => !ignoredEntries.Contains(Path.GetFileName(path)));
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }
}