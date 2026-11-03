
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Infrastructure.Documents;
using Xunit;

namespace DigitalManualSet.Tests.Infrastructure.Documents;

public class LocalDocumentDownloadServiceTests
{
    [Fact]
    public async Task DownloadAsync_ExistingDocument_CopiesFile()
    {
        using var directories = new TemporaryDirectories();

        directories.CreateSourceFile("F123456.pdf", "Test PDF content");

        var service = new LocalDocumentDownloadService(directories.Source);
        var document = CreateDocument("F123456");

        var result = await service.DownloadAsync(document, directories.Destination);

        var destinationFile = Path.Combine(directories.Destination, "F123456.pdf");

        Assert.NotNull(result);
        Assert.True(File.Exists(destinationFile));
        Assert.Equal("Test PDF content", await File.ReadAllTextAsync(destinationFile));
    }

    [Fact]
    public async Task DownloadAsync_MissingDocument_DoesNotCopyFile()
    {
        using var directories = new TemporaryDirectories();

        directories.CreateSourceFile("F999999.pdf", "Other document");

        var service = new LocalDocumentDownloadService(directories.Source);
        var document = CreateDocument("F123456");

        var result = await service.DownloadAsync(document, directories.Destination);

        Assert.NotNull(result);
        Assert.Empty(Directory.GetFiles(directories.Destination));
    }

    [Fact]
    public async Task DownloadAsync_DocumentNumberMatch_IsCaseInsensitive()
    {
        using var directories = new TemporaryDirectories();

        directories.CreateSourceFile("f123456_RevA.pdf", "Drawing content");

        var service = new LocalDocumentDownloadService(directories.Source);
        var document = CreateDocument("F123456");

        await service.DownloadAsync(document, directories.Destination);

        Assert.True(File.Exists(
            Path.Combine(directories.Destination, "f123456_RevA.pdf")));
    }

    [Fact]
    public async Task DownloadAsync_ExistingDestinationFile_Overwrites()
    {
        using var directories = new TemporaryDirectories();

        directories.CreateSourceFile("F123456.pdf", "New content");

        var destinationFile = Path.Combine(
            directories.Destination, "F123456.pdf");

        await File.WriteAllTextAsync(destinationFile, "Old content");

        var service = new LocalDocumentDownloadService(directories.Source);

        await service.DownloadAsync(
            CreateDocument("F123456"),
            directories.Destination);

        Assert.Equal("New content", await File.ReadAllTextAsync(destinationFile));
    }

    [Fact]
    public async Task DownloadAsync_SuccessfulCopy_ReportsProgress()
    {
        using var directories = new TemporaryDirectories();

        directories.CreateSourceFile("F123456.pdf", "Test content");

        var service = new LocalDocumentDownloadService(directories.Source);

        var messages = new List<string>();
        var progress = new TestProgress(messages);

        await service.DownloadAsync(
            CreateDocument("F123456"),
            directories.Destination,
            progress);

        Assert.Equal(
        [
            "Finding document...",
            "Copying document...",
            "Complete"
        ], messages);
    }

    [Fact]
    public async Task DownloadAsync_SourceDirectoryMissing_HandlesException()
    {
        using var directories = new TemporaryDirectories();

        Directory.Delete(directories.Source);

        var service = new LocalDocumentDownloadService(directories.Source);

        var exception = await Record.ExceptionAsync(() =>
            service.DownloadAsync(
                CreateDocument("F123456"),
                directories.Destination));

        Assert.Null(exception);
        Assert.Empty(Directory.GetFiles(directories.Destination));
    }

    private static Document CreateDocument(string number)
    {
        return new Document(
            number,
            "Test Document",
            "A",
            "Drawing");
    }

    private sealed class TestProgress(List<string> messages) : IProgress<string>
    {
        public void Report(string value)
        {
            messages.Add(value);
        }
    }

    /// <summary>
    /// Creates and cleans up temporary source and destination directories.
    /// </summary>
    private sealed class TemporaryDirectories : IDisposable
    {
        private readonly string _root;

        public string Source { get; }
        public string Destination { get; }

        public TemporaryDirectories()
        {
            _root = Path.Combine(
                Path.GetTempPath(),
                $"DocumentDownloadTests_{Guid.NewGuid():N}");

            Source = Path.Combine(_root, "Source");
            Destination = Path.Combine(_root, "Destination");

            Directory.CreateDirectory(Source);
            Directory.CreateDirectory(Destination);
        }

        public void CreateSourceFile(string fileName, string content)
        {
            File.WriteAllText(Path.Combine(Source, fileName), content);
        }

        public void Dispose()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
    }
}
