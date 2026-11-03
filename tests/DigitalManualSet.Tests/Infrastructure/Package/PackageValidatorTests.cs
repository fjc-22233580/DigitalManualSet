using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.PackageCreation;
using DigitalManualSet.Infrastructure.Services;

namespace DigitalManualSet.Tests.Infrastructure.Package;

public class PackageValidatorTests
{
    private readonly PackageValidator _validator = new();

    private static readonly string[] RequiredFiles =
    [
        "Manifest.json",
        "Certificate.pdf",
        "index.html",
        Path.Combine("assets", "css", "styles.css"),
        Path.Combine("assets", "js", "app.js")
    ];

    [Fact]
    public async Task ValidateAsync_NullPackage_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _validator.ValidateAsync(null!));
    }

    [Fact]
    public async Task ValidateAsync_MissingOutput_ReturnsError()
    {
        var package = CreatePackage(null);

        var result = await _validator.ValidateAsync(package);

        Assert.Single(result.Errors);
        Assert.Contains(
            "Package output has not been configured.",
            result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_CompletePackage_ReturnsNoErrors()
    {
        using var directory = new TemporaryDirectory();
        directory.CreateRequiredFiles();

        var package = CreatePackage(directory.Path);

        var result = await _validator.ValidateAsync(package);

        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("Manifest.json")]
    [InlineData("Certificate.pdf")]
    [InlineData("index.html")]
    [InlineData("assets/css/styles.css")]
    [InlineData("assets/js/app.js")]
    public async Task ValidateAsync_MissingRequiredFile_ReturnsError(
        string missingFile)
    {
        using var directory = new TemporaryDirectory();
        directory.CreateRequiredFiles();

        // Remove one of the required files.
        var relativePath = missingFile.Replace(
            '/', Path.DirectorySeparatorChar);

        File.Delete(Path.Combine(directory.Path, relativePath));

        var package = CreatePackage(directory.Path);

        var result = await _validator.ValidateAsync(package);

        Assert.Single(result.Errors);
        Assert.Contains(
            $"Required file is missing: {relativePath}",
            result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_DocumentWithoutOutputPath_ReturnsError()
    {
        using var directory = new TemporaryDirectory();
        directory.CreateRequiredFiles();

        var document = CreateDocument("F123456");
        document.LocalFilePath = "F123456.pdf";
        document.OutputRelativePath = null;

        var package = CreatePackage(directory.Path, document);

        var result = await _validator.ValidateAsync(package);

        Assert.Single(result.Errors);
        Assert.Contains(
            "Document 'F123456.pdf' has no output path.",
            result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_MissingDocumentFile_ReturnsError()
    {
        using var directory = new TemporaryDirectory();
        directory.CreateRequiredFiles();

        var document = CreateDocument("F123456");
        document.OutputRelativePath =
            Path.Combine("documents", "F123456.pdf");

        var package = CreatePackage(directory.Path, document);

        var result = await _validator.ValidateAsync(package);

        Assert.Single(result.Errors);
        Assert.Contains(
            $"Document is missing: {document.OutputRelativePath}",
            result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_ExistingDocumentFile_ReturnsNoErrors()
    {
        using var directory = new TemporaryDirectory();
        directory.CreateRequiredFiles();

        var relativePath = Path.Combine("documents", "F123456.pdf");
        directory.CreateFile(relativePath);

        var document = CreateDocument("F123456");
        document.OutputRelativePath = relativePath;

        var package = CreatePackage(directory.Path, document);

        var result = await _validator.ValidateAsync(package);

        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_MultipleMissingFiles_ReturnsAllErrors()
    {
        using var directory = new TemporaryDirectory();

        var document = CreateDocument("F123456");
        document.OutputRelativePath =
            Path.Combine("documents", "F123456.pdf");

        var package = CreatePackage(directory.Path, document);

        var result = await _validator.ValidateAsync(package);

        // Five required files plus one missing document.
        Assert.Equal(6, result.Errors.Count);
    }

    [Fact]
    public async Task ValidateAsync_CancelledToken_Throws()
    {
        using var directory = new TemporaryDirectory();

        var package = CreatePackage(directory.Path);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _validator.ValidateAsync(package, cts.Token));
    }

    // Test helpers

    private static Document CreateDocument(string number)
    {
        return new Document(
            number,
            "Test Document",
            "A",
            "Drawing");
    }

    private static DigitalManualSet.Core.PackageCreation.Package CreatePackage(
        string? rootPath,
        params Document[] documents)
    {
        // Adjust this helper to match your Package model.
        return new DigitalManualSet.Core.PackageCreation.Package
        {
            Output = rootPath is null
                ? null
                : new PackageOutput { RootPath = rootPath },
            Documents = documents.ToList()
        };
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public string Path { get; }

        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"PackageValidatorTests_{Guid.NewGuid():N}");

            Directory.CreateDirectory(Path);
        }

        public void CreateRequiredFiles()
        {
            foreach (var file in RequiredFiles)
            {
                CreateFile(file);
            }
        }

        public void CreateFile(string relativePath)
        {
            var fullPath = System.IO.Path.Combine(
                Path, relativePath);

            Directory.CreateDirectory(
                System.IO.Path.GetDirectoryName(fullPath)!);

            File.WriteAllText(fullPath, "Test content");
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
