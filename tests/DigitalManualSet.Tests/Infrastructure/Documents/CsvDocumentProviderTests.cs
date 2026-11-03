
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Infrastructure.Documents;
using Xunit;

namespace DigitalManualSet.Tests.Infrastructure.Documents;

public class CsvDocumentProviderTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidFilePath_ThrowsArgumentException(string? path)
    {
        Assert.ThrowsAny<ArgumentException>(() => new CsvDocumentProvider(path!));
    }

    [Fact]
    public void Constructor_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        var path = Path.Combine(
            Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");

        Assert.Throws<FileNotFoundException>(() => new CsvDocumentProvider(path));
    }

    [Fact]
    public void CsvFileName_ReturnsFileNameWithoutExtension()
    {
        using var csv = new TemporaryCsv("Header");

        var provider = new CsvDocumentProvider(csv.FilePath);

        Assert.Equal(Path.GetFileNameWithoutExtension(csv.FilePath), provider.CsvFileName);
    }

    [Fact]
    public async Task GetDocumentsAsync_ValidCsv_ReturnsDocuments()
    {
        using var csv = new TemporaryCsv("Number,Name,Revision,Category", "F123456,Drawing One,A,Mechanical", "M654321,Manual One,B,Electrical");

        var provider = new CsvDocumentProvider(csv.FilePath);

        var documents = await provider.GetDocumentsAsync();

        Assert.Equal(2, documents.Count);
    }

    [Fact]
    public async Task GetDocumentsAsync_EmptyFile_ReturnsEmpty()
    {
        using var csv = new TemporaryCsv();

        var provider = new CsvDocumentProvider(csv.FilePath);

        var documents = await provider.GetDocumentsAsync();

        Assert.Empty(documents);
    }

    [Fact]
    public async Task GetDocumentsAsync_HeaderOnly_ReturnsEmpty()
    {
        using var csv = new TemporaryCsv("Number,Name,Revision,Category");

        var provider = new CsvDocumentProvider(csv.FilePath);

        var documents = await provider.GetDocumentsAsync();

        Assert.Empty(documents);
    }

    [Fact]
    public async Task GetDocumentsAsync_BlankLines_AreSkipped()
    {
        using var csv = new TemporaryCsv(
            "Number,Name,Revision,Category",
            "F123456,Drawing One,A,Mechanical",
            "",
            "   ",
            "M654321,Manual One,B,Electrical");

        var provider = new CsvDocumentProvider(csv.FilePath);

        var documents = await provider.GetDocumentsAsync();

        Assert.Equal(2, documents.Count);
    }

    [Theory]
    [InlineData("F123456,Drawing One,A")]
    [InlineData("F123456,Drawing One,A,Mechanical,Extra")]
    public async Task GetDocumentsAsync_InvalidColumnCount_Throws(
        string row)
    {
        using var csv = new TemporaryCsv("Number,Name,Revision,Category", row);

        var provider = new CsvDocumentProvider(csv.FilePath);

        await Assert.ThrowsAsync<InvalidDataException>(() => provider.GetDocumentsAsync());
    }

    [Theory]
    [InlineData(",Drawing One,A,Mechanical")]
    [InlineData("F123456,,A,Mechanical")]
    [InlineData("F123456,Drawing One,,Mechanical")]
    [InlineData("F123456,Drawing One,A,")]
    public async Task GetDocumentsAsync_EmptyRequiredField_Throws(
        string row)
    {
        using var csv = new TemporaryCsv("Number,Name,Revision,Category", row);

        var provider = new CsvDocumentProvider(csv.FilePath);

        await Assert.ThrowsAsync<InvalidDataException>(() => provider.GetDocumentsAsync());
    }

    [Fact]
    public async Task GetDocumentsAsync_QuotedComma_ParsesSuccessfully()
    {
        using var csv = new TemporaryCsv(
            "Number,Name,Revision,Category",
            "F123456,\"Drawing, Assembly\",A,Mechanical");

        var provider = new CsvDocumentProvider(csv.FilePath);

        var documents = await provider.GetDocumentsAsync();

        Assert.Single(documents);
    }

    [Fact]
    public async Task GetDocumentsAsync_EscapedQuotes_ParsesSuccessfully()
    {
        using var csv = new TemporaryCsv(
            "Number,Name,Revision,Category",
            "F123456,\"Drawing \"\"Special\"\"\",A,Mechanical");

        var provider = new CsvDocumentProvider(csv.FilePath);

        var documents = await provider.GetDocumentsAsync();

        Assert.Single(documents);
    }

    [Fact]
    public async Task GetDocumentsAsync_UnmatchedQuotes_Throws()
    {
        using var csv = new TemporaryCsv(
            "Number,Name,Revision,Category",
            "F123456,\"Unclosed Name,A,Mechanical");

        var provider = new CsvDocumentProvider(csv.FilePath);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => provider.GetDocumentsAsync());
    }

    [Fact]
    public async Task GetDocumentsAsync_ErpImport_FiltersDocumentPrefixes()
    {
        using var csv = new TemporaryCsv(
            "Number,Description",
            "F123456,Drawing",
            "M123456,Manual",
            "HS123456,Health and Safety",
            "X123456,Other Document",
            "ABC123,Unknown Document");

        var definition = new CsvImportDefinition(0, 1);
        var provider = new CsvDocumentProvider(csv.FilePath, definition);

        var documents = await provider.GetDocumentsAsync();

        Assert.Equal(3, documents.Count);
    }

    [Fact]
    public async Task GetDocumentsAsync_ErpImport_EmptyDescription_Throws()
    {
        using var csv = new TemporaryCsv(
            "Number,Description",
            "F123456,");

        var definition = new CsvImportDefinition(0, 1);
        var provider = new CsvDocumentProvider(csv.FilePath, definition);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => provider.GetDocumentsAsync());
    }

    [Fact]
    public async Task GetDocumentsAsync_CancelledToken_Throws()
    {
        using var csv = new TemporaryCsv(
            "Number,Name,Revision,Category",
            "F123456,Drawing One,A,Mechanical");

        var provider = new CsvDocumentProvider(csv.FilePath);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => provider.GetDocumentsAsync(cts.Token));
    }

    /// <summary>
    /// Creates a temporary CSV file and deletes it after testing.
    /// </summary>
    private sealed class TemporaryCsv : IDisposable
    {
        public string FilePath { get; }

        public TemporaryCsv(params string[] lines)
        {
            FilePath = Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid():N}.csv");

            File.WriteAllLines(FilePath, lines);
        }

        public void Dispose()
        {
            File.Delete(FilePath);
        }
    }
}
