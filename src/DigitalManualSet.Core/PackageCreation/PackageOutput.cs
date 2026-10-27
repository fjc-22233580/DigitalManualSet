namespace DigitalManualSet.Core.PackageCreation;

/// <summary>
/// Contains the output details for a created package.
/// </summary>
public sealed class PackageOutput
{
    /// <summary>
    /// Gets or sets the root path where the package was written.
    /// </summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the package documents were copied.
    /// </summary>
    public DateTime? DocumentsCopiedAt { get; set; }

}
