namespace DigitalManualSet.Core.Models;

/// <summary>
/// Specifies the write access status of a drive.
/// </summary>
public enum DriveWriteStatus
{
    /// <summary>
    /// The write access status could not be determined.
    /// </summary>
    Unknown,

    /// <summary>
    /// The drive accepts write operations.
    /// </summary>
    Writable,

    /// <summary>
    /// The drive is protected against write operations.
    /// </summary>
    WriteProtected
}
