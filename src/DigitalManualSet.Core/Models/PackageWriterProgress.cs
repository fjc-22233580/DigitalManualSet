namespace DigitalManualSet.Core.Models;

/// <summary>
/// Represents progress information for copying package documents.
/// </summary>
/// <param name="CurrentFile">The one-based number of the file currently being copied.</param>
/// <param name="TotalFiles">The total number of files to copy.</param>
/// <param name="FileName">The name of the file currently being copied.</param>
public sealed record PackageWriteProgress(
    int CurrentFile,
    int TotalFiles,
    string FileName)
{
    /// <summary>
    /// Gets the copy progress as a percentage.
    /// </summary>
    public double Percentage => TotalFiles == 0 ? 0 : (double)CurrentFile / TotalFiles * 100;

    /// <summary>
    /// Gets a formatted status message for the current copy progress.
    /// </summary>
    public string Status => $"Copying {CurrentFile}/{TotalFiles} files...";
}
