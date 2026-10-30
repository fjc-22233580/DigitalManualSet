using DigitalManualSet.Core.PackageCreation;
using DigitalManualSet.Core.Services;
using System.Security.Cryptography;
using System.Text.Json;

namespace DigitalManualSet.Infrastructure.Services;

/// <summary>
/// Generates a JSON manifest containing package and document metadata.
/// </summary>
public sealed class ManifestGenerator : IManifestGenerator
{
    private const string ManifestFileName = "Manifest.json";

    /// <summary>
    /// Generates a manifest for the specified package and writes it to the package output directory.
    /// </summary>
    /// <param name="package">The package for which to generate the manifest.</param>
    /// <param name="cancellationToken">The token used to cancel manifest generation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="package" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">The package output is not configured, or a document has no output path.</exception>
    /// <exception cref="FileNotFoundException">A copied document cannot be found in the package output directory.</exception>
    public async Task GenerateAsync(Package package, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        if (package.Output is null)
        {
            throw new InvalidOperationException("Package output has not been configured.");
        }

        var documents = new List<ManifestDocument>();

        foreach (var document in package.Documents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(document.OutputRelativePath))
            {
                throw new InvalidOperationException($"Document '{document.LocalFilePath}' has no output path.");
            }

            var outputPath = Path.Combine(package.Output.RootPath, document.OutputRelativePath);

            if (!File.Exists(outputPath))
            {
                throw new FileNotFoundException("A copied document could not be found.", outputPath);
            }

            documents.Add(new ManifestDocument
            {
                FileName = Path.GetFileName(outputPath),
                Type = document.Type.ToString(),
                Path = document.OutputRelativePath.Replace('\\', '/'),
                Sha256 = await CalculateSha256Async(outputPath, cancellationToken)
            });
        }

        var manifest = new Manifest
        {
            OrderNumber = package.Order!.OrderNumber,
            CustomerName = package.Order!.CustomerName,
            SystemId = package.Order!.SystemId,
            GeneratedAtUtc = DateTime.UtcNow,
            Documents = documents
        };

        var manifestPath = Path.Combine(package.Output.RootPath, ManifestFileName);

        await using var stream = File.Create(manifestPath);

        await JsonSerializer.SerializeAsync(
            stream,
            manifest,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            },
            cancellationToken);
    }

    /// <summary>
    /// Calculates the SHA-256 hash of a file.
    /// </summary>
    /// <param name="filePath">The path of the file to hash.</param>
    /// <param name="cancellationToken">The token used to cancel hashing.</param>
    /// <returns>The file hash as an uppercase hexadecimal string.</returns>
    private static async Task<string> CalculateSha256Async(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(filePath);

        var hash = await SHA256.HashDataAsync(stream, cancellationToken);

        return Convert.ToHexString(hash);
    }

    /// <summary>
    /// Represents the package-level data written to the manifest.
    /// </summary>
    private sealed class Manifest
    {
        public string OrderNumber { get; init; }

        public string CustomerName { get; init; }

        public string SystemId { get; init; }

        public DateTime GeneratedAtUtc { get; init; }

        public required IReadOnlyList<ManifestDocument> Documents { get; init; }
    }

    /// <summary>
    /// Represents a document entry written to the manifest.
    /// </summary>
    private sealed class ManifestDocument
    {
        public required string FileName { get; init; }

        public required string Type { get; init; }

        public required string Path { get; init; }

        public required string Sha256 { get; init; }
    }
}