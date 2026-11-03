using System.Text.Json;
using DigitalManualSet.Infrastructure.Services;

namespace DigitalManualSet.Tests.Infrastructure.Documents;

public class DocumentCatalogueServiceTests
{
    [Fact]
    public void GetDocumentsForModel_BeforeLoading_ThrowsInvalidOperationException()
    {
        using var manifest = new TemporaryManifest(CreateManifest("A100"));

        var service = new DocumentCatalogueService(manifest.FilePath);

        Assert.Throws<InvalidOperationException>(() => service.GetDocumentsForModel("A100"));
    }

    [Fact]
    public async Task LoadAsync_ValidManifest_LoadsSuccessfully()
    {
        using var manifest = new TemporaryManifest(CreateManifest("A100", "B200"));

        var service = new DocumentCatalogueService(manifest.FilePath);

        await service.LoadAsync();

        Assert.Empty(service.GetDocumentsForModel("A100"));
        Assert.Empty(service.GetDocumentsForModel("B200"));
    }

    [Fact]
    public async Task GetDocumentsForModel_UnknownModel_ReturnsEmpty()
    {
        using var manifest = new TemporaryManifest(CreateManifest("A100"));

        var service = new DocumentCatalogueService(manifest.FilePath);

        await service.LoadAsync();

        var result = service.GetDocumentsForModel("UNKNOWN");

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetDocumentsForModel_InvalidModelNumber_ReturnsEmpty(
        string? modelNumber)
    {
        using var manifest = new TemporaryManifest(CreateManifest("A100"));

        var service = new DocumentCatalogueService(manifest.FilePath);

        await service.LoadAsync();

        var result = service.GetDocumentsForModel(modelNumber!);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDocumentsForModel_ModelNumber_IsCaseInsensitive()
    {
        using var manifest = new TemporaryManifest(CreateManifest("A100"));

        var service = new DocumentCatalogueService(manifest.FilePath);

        await service.LoadAsync();

        var result = service.GetDocumentsForModel("a100");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDocumentsForModel_ModelNumber_TrimsWhitespace()
    {
        using var manifest = new TemporaryManifest(CreateManifest("  A100  "));

        var service = new DocumentCatalogueService(manifest.FilePath);

        await service.LoadAsync();

        var result = service.GetDocumentsForModel(" A100 ");

        Assert.Empty(result);
    }

    [Fact]
    public async Task LoadAsync_JsonPropertyNames_AreCaseInsensitive()
    {
        using var manifest = new TemporaryManifest(
            """
            {
                "models": [
                    {
                        "modelnumber": "A100",
                        "documents": []
                    }
                ]
            }
            """);

        var service = new DocumentCatalogueService(manifest.FilePath);

        await service.LoadAsync();

        Assert.Empty(service.GetDocumentsForModel("A100"));
    }

    [Fact]
    public async Task LoadAsync_EmptyModels_LoadsSuccessfully()
    {
        using var manifest = new TemporaryManifest(CreateManifest());

        var service = new DocumentCatalogueService(manifest.FilePath);

        await service.LoadAsync();

        Assert.Empty(service.GetDocumentsForModel("A100"));
    }

    [Fact]
    public async Task LoadAsync_MissingFile_ThrowsFileNotFoundException()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        var service = new DocumentCatalogueService(path);

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => service.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_InvalidJson_ThrowsJsonException()
    {
        using var manifest = new TemporaryManifest("{ invalid json }");

        var service = new DocumentCatalogueService(manifest.FilePath);

        await Assert.ThrowsAsync<JsonException>(
            () => service.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_NullManifest_ThrowsInvalidDataException()
    {
        using var manifest = new TemporaryManifest("null");

        var service = new DocumentCatalogueService(manifest.FilePath);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => service.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_DuplicateModelNumbers_ThrowsArgumentException()
    {
        using var manifest = new TemporaryManifest(CreateManifest("A100", "a100"));

        var service = new DocumentCatalogueService(manifest.FilePath);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_Reload_ReplacesPreviousCatalogue()
    {
        using var manifest = new TemporaryManifest(CreateManifest("A100"));

        var service = new DocumentCatalogueService(manifest.FilePath);

        await service.LoadAsync();

        // Replace manifest with another model.
        manifest.Write(CreateManifest("B200"));

        await service.LoadAsync();

        Assert.Empty(service.GetDocumentsForModel("B200"));
        Assert.Empty(service.GetDocumentsForModel("A100"));
    }

    [Fact]
    public async Task LoadAsync_FailedReload_PreservesPreviousCatalogue()
    {
        using var manifest = new TemporaryManifest(CreateManifest("A100"));

        var service = new DocumentCatalogueService(manifest.FilePath);

        await service.LoadAsync();

        // Replace the file with invalid JSON.
        manifest.Write("{ invalid json }");

        await Assert.ThrowsAsync<JsonException>(
            () => service.LoadAsync());

        // The previous catalogue should remain loaded.
        var result = service.GetDocumentsForModel("A100");

        Assert.Empty(result);
    }

    [Fact]
    public async Task LoadAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var manifest = new TemporaryManifest(CreateManifest("A100"));

        var service = new DocumentCatalogueService(manifest.FilePath);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.LoadAsync(cts.Token));
    }

    // Test helpers

    private static string CreateManifest(params string[] modelNumbers)
    {
        return JsonSerializer.Serialize(new
        {
            Models = modelNumbers.Select(number => new
            {
                ModelNumber = number,
                Documents = Array.Empty<object>()
            }).ToArray()
        });
    }

    /// <summary>
    /// Creates a temporary JSON manifest and removes it after testing.
    /// </summary>
    private sealed class TemporaryManifest : IDisposable
    {
        public string FilePath { get; }

        public TemporaryManifest(string content)
        {
            FilePath = Path.Combine(Path.GetTempPath(), $"DocumentCatalogue_{Guid.NewGuid():N}.json");
            Write(content);
        }

        public void Write(string content)
        {
            File.WriteAllText(FilePath, content);
        }

        public void Dispose()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
    }
}
