using DigitalManualSet.Core.PackageCreation;
using DigitalManualSet.Core.Services;

namespace DigitalManualSet.Infrastructure.Services;

/// <summary>
/// Validates that the completed documentation package contains the
/// expected files and copied documents.
/// </summary>
public sealed class PackageValidator : IPackageValidator
{
    private static readonly string[] RequiredFiles =
    [
        "Manifest.json",
        "Certificate.pdf",
        "index.html",
        Path.Combine("assets", "css", "styles.css"),
        Path.Combine("assets", "js", "app.js")
    ];

    /// <summary>
    /// Checks the generated package for missing files or invalid document paths.
    /// </summary>
    /// <param name="package">The package.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentNullException"></exception>
    public Task<PackageValidationResult> ValidateAsync(Package package, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        var result = new PackageValidationResult();

        if (package.Output is null)
        {
            result.Errors.Add("Package output has not been configured.");
            return Task.FromResult(result);
        }

        var rootPath = package.Output.RootPath;

        // Check each of the files that should always exist
        // after the finalisation process.
        foreach (var requiredFile in RequiredFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fullPath = Path.Combine(rootPath, requiredFile);

            if (!File.Exists(fullPath))
            {
                result.Errors.Add($"Required file is missing: {requiredFile}");
            }
        }

        // Check that every document in the Package has a recorded
        // output path and that the copied file still exists.
        foreach (var document in package.Documents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(document.OutputRelativePath))
            {
                result.Errors.Add($"Document '{Path.GetFileName(document.LocalFilePath)}' has no output path.");
                continue;
            }

            var documentPath = Path.Combine(rootPath, document.OutputRelativePath);

            if (!File.Exists(documentPath))
            {
                result.Errors.Add($"Document is missing: {document.OutputRelativePath}");
            }
        }

        return Task.FromResult(result);
    }
}