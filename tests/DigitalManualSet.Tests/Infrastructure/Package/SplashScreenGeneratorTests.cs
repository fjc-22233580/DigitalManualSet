using System.Text.Json;
using System.Text.RegularExpressions;
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.Models;
using DigitalManualSet.Core.Orders;
using DigitalManualSet.Core.PackageCreation;
using DigitalManualSet.Infrastructure.Services;

namespace DigitalManualSet.Tests.Infrastructure.Package;

public class SplashScreenGeneratorTests
{
    private readonly SplashScreenGenerator _generator = new();

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
    public async Task GenerateAsync_ValidPackage_CreatesIndexHtml()
    {
        using var directory = new TemporaryDirectory();

        var package = CreatePackage(directory.Path);

        await _generator.GenerateAsync(package);

        var indexPath = Path.Combine(directory.Path, "index.html");

        Assert.True(File.Exists(indexPath));

        var html = await File.ReadAllTextAsync(indexPath);

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<title>Digital Documentation</title>", html);
        Assert.Contains("window.packageData =", html);
    }

    [Fact]
    public async Task GenerateAsync_CopiesEmbeddedAssets()
    {
        using var directory = new TemporaryDirectory();

        var package = CreatePackage(directory.Path);

        await _generator.GenerateAsync(package);

        var expectedFiles = new[]
        {
            Path.Combine("assets", "css", "styles.css"),
            Path.Combine("assets", "js", "app.js"),
            Path.Combine("assets", "images", "company-logo.svg")
        };

        foreach (var relativePath in expectedFiles)
        {
            var fullPath = Path.Combine(directory.Path, relativePath);

            Assert.True(File.Exists(fullPath));
            Assert.True(new FileInfo(fullPath).Length > 0);
        }
    }

    [Fact]
    public async Task GenerateAsync_HtmlReferencesCorrectAssets()
    {
        using var directory = new TemporaryDirectory();

        var package = CreatePackage(directory.Path);

        await _generator.GenerateAsync(package);

        var html = await File.ReadAllTextAsync(
            Path.Combine(directory.Path, "index.html"));

        Assert.Contains("assets/css/styles.css", html);
        Assert.Contains("assets/js/app.js", html);
        Assert.Contains("assets/images/company-logo.svg", html);
    }

    [Fact]
    public async Task GenerateAsync_EmbedsOrderInformation()
    {
        using var directory = new TemporaryDirectory();

        var package = CreatePackage(directory.Path);

        await _generator.GenerateAsync(package);

        using var json = await ReadPackageDataAsync(directory.Path);

        var order = json.RootElement.GetProperty("order");

        Assert.Equal("A123456",
            order.GetProperty("number").GetString());

        Assert.Equal("Test Customer",
            order.GetProperty("customer").GetString());

        Assert.Equal("SYS-001",
            order.GetProperty("systemId").GetString());

        Assert.False(string.IsNullOrWhiteSpace(
            json.RootElement.GetProperty("generatedOn").GetString()));
    }

    [Fact]
    public async Task GenerateAsync_EmbedsDocumentMetadata()
    {
        using var directory = new TemporaryDirectory();

        var relativePath = Path.Combine(
            "Documents", "Manuals", "M123456.pdf");

        var document = CreateDocument("M123456");
        document.Type = DocumentType.Manual;
        document.OutputRelativePath = relativePath;

        var package = CreatePackage(directory.Path, document);

        await _generator.GenerateAsync(package);

        using var json = await ReadPackageDataAsync(directory.Path);

        var documents = json.RootElement.GetProperty("documents");

        var entry = Assert.Single(documents.EnumerateArray());

        Assert.Equal("M123456",
            entry.GetProperty("number").GetString());

        Assert.Equal("Test Document",
            entry.GetProperty("name").GetString());

        Assert.Equal("A",
            entry.GetProperty("revision").GetString());

        Assert.Equal("Manual",
            entry.GetProperty("type").GetString());

        Assert.Equal("M123456.pdf",
            entry.GetProperty("fileName").GetString());

        Assert.Equal("Documents/Manuals/M123456.pdf",
            entry.GetProperty("path").GetString());
    }

    [Fact]
    public async Task GenerateAsync_DocumentsWithoutOutputPath_AreExcluded()
    {
        using var directory = new TemporaryDirectory();

        var valid = CreateDocument("F123456");
        valid.OutputRelativePath = Path.Combine(
            "Documents", "Drawings", "F123456.pdf");

        var invalid = CreateDocument("M654321");
        invalid.OutputRelativePath = null;

        var package = CreatePackage(
            directory.Path, valid, invalid);

        await _generator.GenerateAsync(package);

        using var json = await ReadPackageDataAsync(directory.Path);

        var documents = json.RootElement.GetProperty("documents");

        var entry = Assert.Single(documents.EnumerateArray());

        Assert.Equal("F123456",
            entry.GetProperty("number").GetString());
    }

    [Fact]
    public async Task GenerateAsync_ScriptInjection_IsEscaped()
    {
        using var directory = new TemporaryDirectory();

        const string maliciousValue =
            "</script><script>alert('test')</script>";

        var package = CreatePackage(directory.Path);

        package.Order = new Order(
            "A123456",
            maliciousValue,
            "SYS-001");

        await _generator.GenerateAsync(package);

        var html = await File.ReadAllTextAsync(
            Path.Combine(directory.Path, "index.html"));

        // The injected closing script tag must not appear literally.
        Assert.DoesNotContain(
            "</script><script>",
            html);

        // The original value should still be preserved in the JSON.
        using var json = await ReadPackageDataAsync(directory.Path);

        Assert.Equal(
            maliciousValue,
            json.RootElement
                .GetProperty("order")
                .GetProperty("customer")
                .GetString());
    }

    [Fact]
    public async Task GenerateAsync_CancelledToken_Throws()
    {
        using var directory = new TemporaryDirectory();

        var package = CreatePackage(directory.Path);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _generator.GenerateAsync(package, cts.Token));

        Assert.False(File.Exists(
            Path.Combine(directory.Path, "index.html")));
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

    /// <summary>
    /// Extracts and parses the JSON assigned to window.packageData.
    /// </summary>
    private static async Task<JsonDocument> ReadPackageDataAsync(
        string directory)
    {
        var html = await File.ReadAllTextAsync(
            Path.Combine(directory, "index.html"));

        var match = Regex.Match(
            html,
            @"window\.packageData\s*=\s*(\{.*?\})\s*;\s*</script>",
            RegexOptions.Singleline);

        Assert.True(match.Success, "Embedded package JSON was not found.");

        return JsonDocument.Parse(match.Groups[1].Value);
    }

    /// <summary>
    /// Creates and removes a temporary output directory.
    /// </summary>
    private sealed class TemporaryDirectory : IDisposable
    {
        public string Path { get; }

        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"SplashScreenTests_{Guid.NewGuid():N}");

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
