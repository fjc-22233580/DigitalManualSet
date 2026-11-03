using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.Models;
using DigitalManualSet.Core.Orders;
using DigitalManualSet.Core.PackageCreation;
using DigitalManualSet.Infrastructure.Services;

namespace DigitalManualSet.Tests.Infrastructure.Package;

public class ManifestGeneratorTests
{
    private readonly ManifestGenerator _generator = new();

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
    public async Task GenerateAsync_EmptyPackage_CreatesManifest()
    {
        using var directory = new TemporaryDirectory();

        var package = CreatePackage(directory.Path);

        await _generator.GenerateAsync(package);

        var manifestPath = Path.Combine(
            directory.Path, "Manifest.json");

        Assert.True(File.Exists(manifestPath));

        using var json = JsonDocument.Parse(
            await File.ReadAllTextAsync(manifestPath));

        var root = json.RootElement;

        Assert.Equal("A123456",
            root.GetProperty("orderNumber").GetString());

        Assert.Equal("Test Customer",
            root.GetProperty("customerName").GetString());

        Assert.Equal("SYS-001",
            root.GetProperty("systemId").GetString());

        Assert.Empty(
            root.GetProperty("documents").EnumerateArray());
    }

    [Fact]
    public async Task GenerateAsync_DocumentWithoutOutputPath_Throws()
    {
        using var directory = new TemporaryDirectory();

        var document = CreateDocument("F123456");
        document.OutputRelativePath = null;

        var package = CreatePackage(directory.Path, document);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _generator.GenerateAsync(package));

        Assert.False(File.Exists(
            Path.Combine(directory.Path, "Manifest.json")));
    }

    [Fact]
    public async Task GenerateAsync_MissingCopiedDocument_ThrowsFileNotFoundException()
    {
        using var directory = new TemporaryDirectory();

        var document = CreateDocument("F123456");

        document.OutputRelativePath = Path.Combine(
            "Documents", "Drawings", "F123456.pdf");

        var package = CreatePackage(directory.Path, document);

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _generator.GenerateAsync(package));
    }

    [Fact]
    public async Task GenerateAsync_ValidDocument_IncludesCorrectMetadata()
    {
        using var directory = new TemporaryDirectory();

        var relativePath = Path.Combine(
            "Documents", "Drawings", "F123456.pdf");

        directory.CreateFile(relativePath, "Drawing content");

        var document = CreateDocument("F123456");
        document.Type = DocumentType.Drawing;
        document.OutputRelativePath = relativePath;

        var package = CreatePackage(directory.Path, document);

        await _generator.GenerateAsync(package);

        using var json = await ReadManifestAsync(directory.Path);

        var documents = json.RootElement
            .GetProperty("documents");

        var entry = Assert.Single(documents.EnumerateArray());

        Assert.Equal("F123456.pdf",
            entry.GetProperty("fileName").GetString());

        Assert.Equal("Drawing",
            entry.GetProperty("type").GetString());

        Assert.Equal("Documents/Drawings/F123456.pdf",
            entry.GetProperty("path").GetString());
    }

    [Fact]
    public async Task GenerateAsync_DocumentHash_MatchesSha256()
    {
        using var directory = new TemporaryDirectory();

        const string content = "Test document content";

        var relativePath = Path.Combine(
            "Documents", "Drawings", "F123456.pdf");

        directory.CreateFile(relativePath, content);

        var document = CreateDocument("F123456");
        document.OutputRelativePath = relativePath;

        var package = CreatePackage(directory.Path, document);

        await _generator.GenerateAsync(package);

        using var json = await ReadManifestAsync(directory.Path);

        var entry = Assert.Single(
            json.RootElement
                .GetProperty("documents")
                .EnumerateArray());

        var actualHash = entry.GetProperty("sha256").GetString();

        var expectedHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(content)));

        Assert.Equal(expectedHash, actualHash);
        Assert.Equal(64, actualHash!.Length);
    }

    [Fact]
    public async Task GenerateAsync_MultipleDocuments_IncludesAllEntries()
    {
        using var directory = new TemporaryDirectory();

        var drawingPath = Path.Combine(
            "Documents", "Drawings", "F123456.pdf");

        var manualPath = Path.Combine(
            "Documents", "Manuals", "M654321.pdf");

        directory.CreateFile(drawingPath, "Drawing content");
        directory.CreateFile(manualPath, "Manual content");

        var drawing = CreateDocument("F123456");
        drawing.Type = DocumentType.Drawing;
        drawing.OutputRelativePath = drawingPath;

        var manual = CreateDocument("M654321");
        manual.Type = DocumentType.Manual;
        manual.OutputRelativePath = manualPath;

        var package = CreatePackage(
            directory.Path, drawing, manual);

        await _generator.GenerateAsync(package);

        using var json = await ReadManifestAsync(directory.Path);

        var documents = json.RootElement
            .GetProperty("documents")
            .EnumerateArray()
            .ToList();

        Assert.Equal(2, documents.Count);

        Assert.Contains(documents,
            d => d.GetProperty("fileName").GetString() == "F123456.pdf");

        Assert.Contains(documents,
            d => d.GetProperty("fileName").GetString() == "M654321.pdf");
    }

    [Fact]
    public async Task GenerateAsync_GeneratedTimestamp_IsUtc()
    {
        using var directory = new TemporaryDirectory();

        var package = CreatePackage(directory.Path);

        var before = DateTime.UtcNow;

        await _generator.GenerateAsync(package);

        var after = DateTime.UtcNow;

        using var json = await ReadManifestAsync(directory.Path);

        var timestamp = json.RootElement
            .GetProperty("generatedAtUtc")
            .GetDateTime();

        Assert.Equal(DateTimeKind.Utc, timestamp.Kind);
        Assert.InRange(timestamp, before, after);
    }

    [Fact]
    public async Task GenerateAsync_ExistingManifest_OverwritesFile()
    {
        using var directory = new TemporaryDirectory();

        var manifestPath = Path.Combine(
            directory.Path, "Manifest.json");

        await File.WriteAllTextAsync(
            manifestPath, "Old manifest content");

        var package = CreatePackage(directory.Path);

        await _generator.GenerateAsync(package);

        var content = await File.ReadAllTextAsync(manifestPath);

        Assert.DoesNotContain("Old manifest content", content);

        using var json = JsonDocument.Parse(content);

        Assert.Equal("A123456",
            json.RootElement.GetProperty("orderNumber").GetString());
    }

    [Fact]
    public async Task GenerateAsync_CancelledToken_Throws()
    {
        using var directory = new TemporaryDirectory();

        var relativePath = Path.Combine(
            "Documents", "Drawings", "F123456.pdf");

        directory.CreateFile(relativePath, "Test content");

        var document = CreateDocument("F123456");
        document.OutputRelativePath = relativePath;

        var package = CreatePackage(directory.Path, document);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _generator.GenerateAsync(package, cts.Token));

        Assert.False(File.Exists(
            Path.Combine(directory.Path, "Manifest.json")));
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
        string outputPath,
        params Document[] documents)
    {
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

            Documents = documents.ToList()
        };
    }

    private static async Task<JsonDocument> ReadManifestAsync(
        string directory)
    {
        var path = Path.Combine(directory, "Manifest.json");

        var json = await File.ReadAllTextAsync(path);

        return JsonDocument.Parse(json);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public string Path { get; }

        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"ManifestTests_{Guid.NewGuid():N}");

            Directory.CreateDirectory(Path);
        }

        public void CreateFile(string relativePath, string content)
        {
            var fullPath = System.IO.Path.Combine(
                Path, relativePath);

            Directory.CreateDirectory(
                System.IO.Path.GetDirectoryName(fullPath)!);

            File.WriteAllText(fullPath, content);
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
