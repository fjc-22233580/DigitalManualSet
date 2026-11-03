using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.Models;
using DigitalManualSet.Infrastructure.Services;

namespace DigitalManualSet.Tests.Infrastructure.Package;

public class PackageWriterTests
{
    private readonly PackageWriter _writer = new();

    [Fact]
    public async Task WriteDocumentsAsync_NullPackage_ThrowsArgumentNullException()
    {
        using var directory = new TemporaryDirectory();

        var drive = CreateDrive(directory.Destination);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _writer.WriteDocumentsAsync(null!, drive));
    }

    [Fact]
    public async Task WriteDocumentsAsync_NullDestination_ThrowsArgumentNullException()
    {
        var package = CreatePackage();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _writer.WriteDocumentsAsync(package, null!));
    }

    [Fact]
    public async Task WriteDocumentsAsync_UnwritableDrive_ThrowsInvalidOperationException()
    {
        using var directory = new TemporaryDirectory();

        var package = CreatePackage();

        var nonWritableStatus = Enum.GetValues<DriveWriteStatus>()
            .First(status => status != DriveWriteStatus.Writable);

        var drive = CreateDrive(
            directory.Destination,
            nonWritableStatus);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _writer.WriteDocumentsAsync(package, drive));
    }

    [Fact]
    public async Task WriteDocumentsAsync_NonEmptyDestination_ThrowsInvalidOperationException()
    {
        using var directory = new TemporaryDirectory();

        File.WriteAllText(
            Path.Combine(directory.Destination, "ExistingFile.txt"),
            "Existing content");

        var package = CreatePackage();
        var drive = CreateDrive(directory.Destination);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _writer.WriteDocumentsAsync(package, drive));
    }

    [Fact]
    public async Task WriteDocumentsAsync_InsufficientSpace_ThrowsIOException()
    {
        using var directory = new TemporaryDirectory();

        var document = directory.CreateSourceDocument(
            "F123456", DocumentType.Drawing);

        var package = CreatePackage(document);

        // The source file contains data, but the drive has no free space.
        var drive = CreateDrive(
            directory.Destination,
            availableSpace: 0);

        await Assert.ThrowsAsync<IOException>(() =>
            _writer.WriteDocumentsAsync(package, drive));
    }

    [Fact]
    public async Task WriteDocumentsAsync_EmptyPackage_CreatesDirectoryStructure()
    {
        using var directory = new TemporaryDirectory();

        var package = CreatePackage();
        var drive = CreateDrive(directory.Destination);

        await _writer.WriteDocumentsAsync(package, drive);

        var documentsRoot = Path.Combine(
            directory.Destination, "Documents");

        Assert.True(Directory.Exists(
            Path.Combine(documentsRoot, "Manuals")));

        Assert.True(Directory.Exists(
            Path.Combine(documentsRoot, "Drawings")));

        Assert.True(Directory.Exists(
            Path.Combine(documentsRoot, "Certificates")));

        Assert.True(Directory.Exists(
            Path.Combine(documentsRoot, "Other")));
    }

    [Theory]
    [InlineData(DocumentType.Manual, "Manuals")]
    [InlineData(DocumentType.Drawing, "Drawings")]
    [InlineData(DocumentType.Certificate, "Certificates")]
    [InlineData(DocumentType.Other, "Other")]
    public async Task WriteDocumentsAsync_DocumentType_CopiesToCorrectDirectory(
        DocumentType type,
        string expectedDirectory)
    {
        using var directory = new TemporaryDirectory();

        var document = directory.CreateSourceDocument(
            "DOC123456", type);

        var package = CreatePackage(document);
        var drive = CreateDrive(directory.Destination);

        await _writer.WriteDocumentsAsync(package, drive);

        var expectedRelativePath = Path.Combine(
            "Documents", expectedDirectory, "DOC123456.pdf");

        var expectedFullPath = Path.Combine(
            directory.Destination, expectedRelativePath);

        Assert.True(File.Exists(expectedFullPath));
        Assert.Equal(expectedRelativePath, document.OutputRelativePath);
    }

    [Fact]
    public async Task WriteDocumentsAsync_CopiedFile_PreservesContents()
    {
        using var directory = new TemporaryDirectory();

        var document = directory.CreateSourceDocument(
            "F123456",
            DocumentType.Drawing,
            "Original document content");

        var package = CreatePackage(document);
        var drive = CreateDrive(directory.Destination);

        await _writer.WriteDocumentsAsync(package, drive);

        var copiedPath = Path.Combine(
            directory.Destination,
            document.OutputRelativePath!);

        var content = await File.ReadAllTextAsync(copiedPath);

        Assert.Equal("Original document content", content);
    }

    [Fact]
    public async Task WriteDocumentsAsync_MultipleDocuments_CopiesAllFiles()
    {
        using var directory = new TemporaryDirectory();

        var drawing = directory.CreateSourceDocument(
            "F123456", DocumentType.Drawing);

        var manual = directory.CreateSourceDocument(
            "M654321", DocumentType.Manual);

        var certificate = directory.CreateSourceDocument(
            "C111111", DocumentType.Certificate);

        var package = CreatePackage(drawing, manual, certificate);
        var drive = CreateDrive(directory.Destination);

        await _writer.WriteDocumentsAsync(package, drive);

        foreach (var document in package.Documents)
        {
            Assert.NotNull(document.OutputRelativePath);

            Assert.True(File.Exists(
                Path.Combine(
                    directory.Destination,
                    document.OutputRelativePath)));
        }
    }

    [Fact]
    public async Task WriteDocumentsAsync_SystemDirectories_AreIgnored()
    {
        using var directory = new TemporaryDirectory();

        Directory.CreateDirectory(Path.Combine(
            directory.Destination, "System Volume Information"));

        Directory.CreateDirectory(Path.Combine(
            directory.Destination, "$RECYCLE.BIN"));

        var document = directory.CreateSourceDocument(
            "F123456", DocumentType.Drawing);

        var package = CreatePackage(document);
        var drive = CreateDrive(directory.Destination);

        await _writer.WriteDocumentsAsync(package, drive);

        Assert.NotNull(document.OutputRelativePath);
        Assert.True(File.Exists(Path.Combine(
            directory.Destination,
            document.OutputRelativePath)));
    }

    [Fact]
    public async Task WriteDocumentsAsync_ReportsProgress()
    {
        using var directory = new TemporaryDirectory();

        var document = directory.CreateSourceDocument(
            "F123456", DocumentType.Drawing);

        var package = CreatePackage(document);
        var drive = CreateDrive(directory.Destination);

        var progress = new TestProgress();

        await _writer.WriteDocumentsAsync(
            package, drive, progress);

        Assert.Equal(2, progress.Reports.Count);

        Assert.Equal(
            PackageCreationStage.CopyDocuments,
            progress.Reports[0].Stage);

        Assert.Equal(0, progress.Reports[0].StageProgress);
        Assert.Equal(1, progress.Reports[^1].StageProgress);

        Assert.Contains(
            "Copied 1/1 files.",
            progress.Reports[^1].Status);
    }

    [Fact]
    public async Task WriteDocumentsAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        using var directory = new TemporaryDirectory();

        var document = directory.CreateSourceDocument(
            "F123456", DocumentType.Drawing);

        var package = CreatePackage(document);
        var drive = CreateDrive(directory.Destination);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _writer.WriteDocumentsAsync(
                package,
                drive,
                cancellationToken: cts.Token));

        Assert.Null(document.OutputRelativePath);
    }

    // Test helpers

    private static DigitalManualSet.Core.PackageCreation.Package CreatePackage(params Document[] documents)
    {
        // Adjust if Package uses a different constructor.
        return new DigitalManualSet.Core.PackageCreation.Package
        {
            Documents = documents.ToList()
        };
    }

    private static RemovableDrive CreateDrive(
        string rootPath,
        DriveWriteStatus writeStatus = DriveWriteStatus.Writable,
        long availableSpace = long.MaxValue)
    {
        return new RemovableDrive(
            RootPath: rootPath,
            VolumeLabel: "Test USB",
            Model: "Test Removable Drive",
            AvailableSpace: availableSpace,
            TotalSpace: long.MaxValue,
            WriteStatus: writeStatus,
            IsEffectivelyEmpty: true);
    }

    private sealed class TestProgress : IProgress<PackageCreationProgress>
    {
        public List<PackageCreationProgress> Reports { get; } = [];

        public void Report(PackageCreationProgress value)
        {
            Reports.Add(value);
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        private readonly string _root;

        public string Source { get; }
        public string Destination { get; }

        public TemporaryDirectory()
        {
            _root = Path.Combine(
                Path.GetTempPath(),
                $"PackageWriterTests_{Guid.NewGuid():N}");

            Source = Path.Combine(_root, "Source");
            Destination = Path.Combine(_root, "Destination");

            Directory.CreateDirectory(Source);
            Directory.CreateDirectory(Destination);
        }

        public Document CreateSourceDocument(
            string number,
            DocumentType type,
            string content = "Test document content")
        {
            var filePath = Path.Combine(Source, $"{number}.pdf");

            File.WriteAllText(filePath, content);

            return new Document(
                number,
                "Test Document",
                "A",
                "Drawing")
            {
                Type = type,
                LocalFilePath = filePath
            };
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
