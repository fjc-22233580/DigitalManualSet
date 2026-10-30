using DigitalManualSet.Core.Services;

namespace DigitalManualSet.Infrastructure.Services;

/// <summary>
/// Removes user content from a removable drive while leaving
/// Windows-managed filesystem folders untouched.
/// </summary>
public sealed class RemovableDriveCleaner : IRemovableDriveCleaner
{
    private static readonly HashSet<string> IgnoredEntries =
    [
        "System Volume Information",
        "$RECYCLE.BIN"
    ];

    /// <summary>
    /// Removes user files and directories from the root of a removable drive.
    /// </summary>
    /// <param name="rootPath">The root path of the removable drive to clear.</param>
    /// <param name="cancellationToken">The token used to cancel the clearing operation.</param>
    /// <returns>A task representing the asynchronous clearing operation.</returns>
    /// <exception cref="ArgumentException"><paramref name="rootPath" /> is null, empty, or whitespace.</exception>
    /// <exception cref="IOException">The selected drive is no longer available.</exception>
    /// <exception cref="InvalidOperationException">The path is not the root of a drive.</exception>
    public Task ClearAsync(string rootPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("A drive root path must be supplied.", nameof(rootPath));
        }

        var drive = new DriveInfo(rootPath);

        if (!drive.IsReady)
        {
            throw new IOException("The selected drive is no longer available.");
        }

        // Safety check: only allow deletion from the actual root
        // of the selected drive, never an arbitrary directory.
        var suppliedPath = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar);

        var driveRoot = Path.GetFullPath(drive.RootDirectory.FullName).TrimEnd(Path.DirectorySeparatorChar);

        if (!string.Equals(suppliedPath, driveRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only the root of a drive can be cleared.");
        }

        // Directory/File deletion APIs are synchronous, so run the
        // destructive work away from the UI thread.
        return Task.Run(() => ClearDrive(driveRoot, cancellationToken), cancellationToken);
    }

    /// <summary>
    /// Deletes all non-system entries from a drive root.
    /// </summary>
    /// <param name="rootPath">The validated root path of the drive to clear.</param>
    /// <param name="cancellationToken">The token used to cancel deletion.</param>
    private static void ClearDrive(string rootPath, CancellationToken cancellationToken)
    {
        foreach (var path in Directory.EnumerateFileSystemEntries(rootPath))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = Path.GetFileName(path);

            if (IgnoredEntries.Contains(name))
            {
                continue;
            }

            if (Directory.Exists(path))
            {
                DeleteDirectory(path, cancellationToken);
            }
            else
            {
                DeleteFile(path);
            }
        }
    }

    /// <summary>
    /// Recursively deletes a directory and its contents while honoring cancellation.
    /// </summary>
    /// <param name="directoryPath">The directory to delete.</param>
    /// <param name="cancellationToken">The token used to cancel deletion.</param>
    private static void DeleteDirectory(string directoryPath, CancellationToken cancellationToken)
    {
        // Delete contents manually so cancellation can be checked
        // between individual files/directories.
        foreach (var file in Directory.EnumerateFiles(directoryPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            DeleteFile(file);
        }

        foreach (var directory in Directory.EnumerateDirectories(directoryPath))
        {
            cancellationToken.ThrowIfCancellationRequested();

            DeleteDirectory(
                directory,
                cancellationToken);
        }

        // A directory itself may also have its ReadOnly attribute set.
        var directoryInfo = new DirectoryInfo(directoryPath);

        directoryInfo.Attributes &= ~FileAttributes.ReadOnly;

        directoryInfo.Delete();
    }

    /// <summary>
    /// Removes the read-only attribute and deletes a file.
    /// </summary>
    /// <param name="filePath">The file to delete.</param>
    private static void DeleteFile(string filePath)
    {
        var file = new FileInfo(filePath);

        // Prevent a read-only file from unnecessarily stopping
        // the USB clearing operation.
        file.Attributes &= ~FileAttributes.ReadOnly;

        file.Delete();
    }
}