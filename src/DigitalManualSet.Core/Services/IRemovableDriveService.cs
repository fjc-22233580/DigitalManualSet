using DigitalManualSet.Core.Models;

namespace DigitalManualSet.Core.Services;

/// <summary>
/// Defines a service for discovering available removable drives.
/// </summary>
public interface IRemovableDriveService
{
    /// <summary>
    /// Gets the removable drives currently available for use.
    /// </summary>
    /// <returns>A read-only list of available removable drives.</returns>
    IReadOnlyList<RemovableDrive> GetAvailableDrives();
}
