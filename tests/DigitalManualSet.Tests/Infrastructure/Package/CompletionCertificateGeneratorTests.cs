using System.Text;
using DigitalManualSet.Core.Orders;
using DigitalManualSet.Core.PackageCreation;
using DigitalManualSet.Infrastructure.Services;
using QuestPDF.Infrastructure;

namespace DigitalManualSet.Tests.Infrastructure.Package;

public class CompletionCertificateGeneratorTests
{
    private readonly CompletionCertificateGenerator _generator = new();

    [Fact]
    public async Task GenerateAsync_NullPackage_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _generator.GenerateAsync(null!));
    }

    [Fact]
    public async Task GenerateAsync_OutputNotConfigured_ThrowsInvalidOperationException()
    {
        var package = new DigitalManualSet.Core.PackageCreation.Package();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _generator.GenerateAsync(package));
    }

    [Fact]
    public async Task GenerateAsync_ManifestMissing_ThrowsFileNotFoundException()
    {
        using var directory = new TemporaryDirectory();

        var package = CreatePackage(directory.Path);

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _generator.GenerateAsync(package));
    }

    [Fact]
    public async Task GenerateAsync_ValidPackage_CreatesPdfCertificate()
    {
        using var directory = new TemporaryDirectory();

        // Create a dummy manifest.
        var manifestPath = Path.Combine(
            directory.Path, "Manifest.json");

        await File.WriteAllTextAsync(
            manifestPath,
            """{"OrderNumber":"A123456","Documents":[]}""");

        var package = CreatePackage(directory.Path);

        // Configure QuestPDF for testing (if eligible for Community).
        QuestPDF.Settings.License = LicenseType.Community;

        await _generator.GenerateAsync(package);

        var certificatePath = Path.Combine(
            directory.Path, "Certificate.pdf");

        Assert.True(File.Exists(certificatePath));

        var fileInfo = new FileInfo(certificatePath);

        Assert.True(fileInfo.Length > 0);

        // Confirm the generated file has a PDF signature.
        var bytes = await File.ReadAllBytesAsync(certificatePath);

        Assert.True(bytes.Length >= 5);

        var signature = Encoding.ASCII.GetString(bytes, 0, 5);

        Assert.Equal("%PDF-", signature);
    }

    [Fact]
    public async Task GenerateAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var directory = new TemporaryDirectory();

        var manifestPath = Path.Combine(
            directory.Path, "Manifest.json");

        await File.WriteAllTextAsync(
            manifestPath,
            """{"OrderNumber":"A123456","Documents":[]}""");

        var package = CreatePackage(directory.Path);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _generator.GenerateAsync(package, cts.Token));

        Assert.False(File.Exists(
            Path.Combine(directory.Path, "Certificate.pdf")));
    }

    // Test helpers

    private static DigitalManualSet.Core.PackageCreation.Package CreatePackage(string outputPath)
    {
        // Adjust to match your Package and output model constructors.
        return new DigitalManualSet.Core.PackageCreation.Package
        {
            Order = new Order(
                orderNumber: "A123456",
                customerName: "Test Customer",
                systemId: "SYS-001"),

            Output = new PackageOutput
            {
                RootPath = outputPath
            },

            Documents = []
        };
    }

    /// <summary>
    /// Creates and cleans up a temporary directory.
    /// </summary>
    private sealed class TemporaryDirectory : IDisposable
    {
        public string Path { get; }

        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"CertificateTests_{Guid.NewGuid():N}");

            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
