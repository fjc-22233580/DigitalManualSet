
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Infrastructure.Documents;
using Xunit;

namespace DigitalManualSet.Tests.Infrastructure.Documents;

public class DocumentValidationServiceTests
{
    private readonly DocumentValidationService _service = new();

    [Fact]
    public void Validate_ExistingFile_MarksDocumentAsValidated()
    {
        using var directory = new TemporaryDirectory();

        var filePath = directory.CreateFile("F123456.pdf", "Test document content");

        var document = CreateDocument("F123456");

        _service.Validate([document], directory.Path);

        Assert.True(document.IsValidated);
        Assert.Equal(filePath, document.LocalFilePath);
        Assert.Equal(
            new FileInfo(filePath).Length,
            document.Length);
    }

    [Fact]
    public void Validate_MissingFile_MarksDocumentAsInvalid()
    {
        using var directory = new TemporaryDirectory();

        var document = CreateDocument("F123456");

        _service.Validate([document], directory.Path);

        Assert.False(document.IsValidated);
        Assert.Null(document.LocalFilePath);
    }

    [Fact]
    public void Validate_EmptyFile_MarksDocumentAsInvalid()
    {
        using var directory = new TemporaryDirectory();

        directory.CreateFile("F123456.pdf", "");

        var document = CreateDocument("F123456");

        _service.Validate([document], directory.Path);

        Assert.False(document.IsValidated);
        Assert.Null(document.LocalFilePath);
    }

    [Fact]
    public void Validate_MultipleMatchingFiles_MarksDocumentAsInvalid()
    {
        using var directory = new TemporaryDirectory();

        directory.CreateFile("F123456_RevA.pdf", "Content A");
        directory.CreateFile("F123456_RevB.pdf", "Content B");

        var document = CreateDocument("F123456");

        _service.Validate([document], directory.Path);

        Assert.False(document.IsValidated);
        Assert.Null(document.LocalFilePath);
    }

    [Fact]
    public void Validate_FileNameMatching_IsCaseInsensitive()
    {
        using var directory = new TemporaryDirectory();

        directory.CreateFile("f123456.pdf", "Test content");

        var document = CreateDocument("F123456");

        _service.Validate([document], directory.Path);

        Assert.True(document.IsValidated);
        Assert.NotNull(document.LocalFilePath);
    }

    [Fact]
    public void Validate_MultipleDocuments_ValidatesIndividually()
    {
        using var directory = new TemporaryDirectory();

        directory.CreateFile("F123456.pdf", "Valid content");
        directory.CreateFile("M654321.pdf", "");

        var valid = CreateDocument("F123456");
        var empty = CreateDocument("M654321");
        var missing = CreateDocument("HS111111");

        _service.Validate([valid, empty, missing], directory.Path);

        Assert.True(valid.IsValidated);
        Assert.False(empty.IsValidated);
        Assert.False(missing.IsValidated);
    }

    [Fact]
    public void Validate_PreviouslyValidatedDocument_ResetsValidationState()
    {
        using var directory = new TemporaryDirectory();

        var filePath = directory.CreateFile("F123456.pdf", "Test content");

        var document = CreateDocument("F123456");

        // First validation succeeds.
        _service.Validate([document], directory.Path);

        Assert.True(document.IsValidated);
        Assert.NotNull(document.LocalFilePath);

        // Remove the file and validate again.
        File.Delete(filePath);

        _service.Validate([document], directory.Path);

        Assert.False(document.IsValidated);
        Assert.Null(document.LocalFilePath);
    }

    [Fact]
    public void Validate_MissingWorkingDirectory_ThrowsDirectoryNotFoundException()
    {
        using var directory = new TemporaryDirectory();

        var missingPath = System.IO.Path.Combine(directory.Path, "DoesNotExist");

        var document = CreateDocument("F123456");

        Assert.Throws<DirectoryNotFoundException>(() =>
            _service.Validate([document], missingPath));
    }

    private static Document CreateDocument(string number)
    {
        return new Document(
            number,
            "Test Document",
            "A",
            "Drawing");
    }

    /// <summary>
    /// Creates and cleans up a temporary working directory.
    /// </summary>
    private sealed class TemporaryDirectory : IDisposable
    {
        public string Path { get; }

        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"DocumentValidationTests_{Guid.NewGuid():N}");

            Directory.CreateDirectory(Path);
        }

        public string CreateFile(string fileName, string content)
        {
            var filePath = System.IO.Path.Combine(Path, fileName);

            File.WriteAllText(filePath, content);

            return filePath;
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
