using DigitalManualSet.Core.Models;
using DigitalManualSet.Core.PackageCreation;
using DigitalManualSet.Core.Services;

namespace DigitalManualSet.Infrastructure.Services;

/// <summary>
/// Copies package documents to a removable output drive.
/// </summary>
public sealed class PackageWriter : IPackageWriter
{
    private const string DocumentsDirectoryName = "Documents";
    private const string ManualsDirectoryName = "Manuals";
    private const string DrawingsDirectoryName = "Drawings";
    private const string CertificatesDirectoryName = "Certificates";
    private const string OtherDirectoryName = "Other";

    /// <summary>
    /// Writes all package documents to the destination drive and reports copy progress.
    /// </summary>
    /// <param name="package">The package whose documents will be copied.</param>
    /// <param name="destination">The removable drive that will receive the documents.</param>
    /// <param name="progress">An optional progress reporter for the copy operation.</param>
    /// <param name="cancellationToken">A token that can cancel the copy operation.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="package"/> or <paramref name="destination"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the destination drive is not writable or is not empty.</exception>
    /// <exception cref="IOException">Thrown when the destination drive does not have enough available space.</exception>
    public async Task WriteDocumentsAsync(
        Package package,
        RemovableDrive destination,
        IProgress<PackageWriteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(destination);

        ValidateDestination(package, destination);

        var documentsRoot = Path.Combine(destination.RootPath, DocumentsDirectoryName);

        CreateDirectoryStructure(documentsRoot);

        var totalFiles = package.Documents.Count;

        for (var i = 0; i < totalFiles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var document = package.Documents[i];

            progress?.Report(new PackageWriteProgress(
                CurrentFile: i + 1,
                TotalFiles: totalFiles,
                FileName: Path.GetFileName(document.LocalFilePath)));

            var relativePath = Path.Combine(DocumentsDirectoryName, GetDirectoryName(document.Type), Path.GetFileName(document.LocalFilePath));

            var targetPath = Path.Combine(destination.RootPath, relativePath);

            await CopyFileAsync(document.LocalFilePath, targetPath, cancellationToken);

            document.OutputRelativePath = relativePath;
        }
    }

    /// <summary>
    /// Validates that the destination drive can receive the package documents.
    /// </summary>
    /// <param name="package">The package whose size will be checked.</param>
    /// <param name="destination">The destination drive to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when the destination drive is not writable or is not empty.</exception>
    /// <exception cref="IOException">Thrown when the destination drive does not have enough available space.</exception>
    private static void ValidateDestination(Package package, RemovableDrive destination)
    {
        if (destination.WriteStatus != DriveWriteStatus.Writable)
        {
            throw new InvalidOperationException("The selected USB drive is not writable.");
        }

        if (destination.IsEffectivelyEmpty == false)
        {
            throw new InvalidOperationException("The selected USB drive is not empty.");
        }

        var requiredSpace = package.Documents.Sum(document => new FileInfo(document.LocalFilePath).Length);

        if (requiredSpace > destination.AvailableSpace)
        {
            throw new IOException("The selected USB drive does not have enough available space.");
        }
    }

    /// <summary>
    /// Creates the directory structure used to organize package documents.
    /// </summary>
    /// <param name="documentsRoot">The root directory for package documents.</param>
    private static void CreateDirectoryStructure(string documentsRoot)
    {
        Directory.CreateDirectory(Path.Combine(documentsRoot, ManualsDirectoryName));
        Directory.CreateDirectory(Path.Combine(documentsRoot, DrawingsDirectoryName));
        Directory.CreateDirectory(Path.Combine(documentsRoot, CertificatesDirectoryName));
        Directory.CreateDirectory(Path.Combine(documentsRoot, OtherDirectoryName));
    }

    /// <summary>
    /// Gets the output directory name for a document type.
    /// </summary>
    /// <param name="documentType">The type of document to map.</param>
    /// <returns>The directory name for the document type.</returns>
    private static string GetDirectoryName(DocumentType documentType)
    {
        return documentType switch
        {
            DocumentType.Manual => ManualsDirectoryName,
            DocumentType.Drawing => DrawingsDirectoryName,
            DocumentType.Certificate => CertificatesDirectoryName,
            _ => OtherDirectoryName
        };
    }

    /// <summary>
    /// Copies a file asynchronously without overwriting an existing destination file.
    /// </summary>
    /// <param name="sourcePath">The path of the source file.</param>
    /// <param name="destinationPath">The path of the destination file.</param>
    /// <param name="cancellationToken">A token that can cancel the copy operation.</param>
    /// <returns>A task that represents the asynchronous file copy operation.</returns>
    private static async Task CopyFileAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
    {
        await using var source = new FileStream(
            sourcePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        await using var destination = new FileStream(
            destinationPath,
            FileMode.CreateNew, // Throws if file already exists.
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await source.CopyToAsync(destination, cancellationToken);
    }
}