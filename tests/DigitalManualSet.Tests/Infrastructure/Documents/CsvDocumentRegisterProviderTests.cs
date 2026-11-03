
using DigitalManualSet.Infrastructure.Documents;
using Xunit;

namespace DigitalManualSet.Tests.Infrastructure.Documents;

public class CsvDocumentRegisterProviderTests
{
    private const string Header =
        "Number,Description,Lifecycle Phase,Rev,Part Type";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidFilePath_ThrowsArgumentException(string? path)
    {
        Assert.ThrowsAny<ArgumentException>(() => new CsvDocumentRegisterProvider(path!));
    }

    [Fact]
    public void Constructor_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");

        Assert.Throws<FileNotFoundException>(() => new CsvDocumentRegisterProvider(path));
    }

    [Fact]
    public void Register_BeforeLoading_IsEmpty()
    {
        using var csv = new TemporaryCsv(Header);
        var provider = new CsvDocumentRegisterProvider(csv.FilePath);
        Assert.Empty(provider.Register);
    }

    [Fact]
    public async Task LoadAsync_ValidCsv_LoadsDocuments()
    {
        using var csv = new TemporaryCsv(
            Header,
            "F123456,Drawing One,Released,A,Drawing",
            "M654321,Manual One,Released,B,Manual");

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await provider.LoadAsync();

        Assert.Equal(2, provider.Register.Count);
        Assert.True(provider.Register.ContainsKey("F123456"));
        Assert.True(provider.Register.ContainsKey("M654321"));
    }

    [Fact]
    public async Task LoadAsync_HeaderOnly_ReturnsEmptyRegister()
    {
        using var csv = new TemporaryCsv(Header);

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await provider.LoadAsync();

        Assert.Empty(provider.Register);
    }

    [Fact]
    public async Task LoadAsync_EmptyFile_ThrowsInvalidDataException()
    {
        using var csv = new TemporaryCsv();

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await Assert.ThrowsAsync<InvalidDataException>(() => provider.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_BlankLines_AreSkipped()
    {
        using var csv = new TemporaryCsv(
            Header,
            "F123456,Drawing One,Released,A,Drawing",
            "",
            "   ",
            "M654321,Manual One,Released,B,Manual");

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await provider.LoadAsync();

        Assert.Equal(2, provider.Register.Count);
    }

    [Fact]
    public async Task LoadAsync_ReorderedHeaders_LoadsCorrectly()
    {
        using var csv = new TemporaryCsv(
            "Rev,Part Type,Number,Lifecycle Phase,Description",
            "A,Drawing,F123456,Released,Drawing One");

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await provider.LoadAsync();

        Assert.Single(provider.Register);
        Assert.True(provider.Register.ContainsKey("F123456"));
    }

    [Fact]
    public async Task LoadAsync_Headers_AreCaseInsensitive()
    {
        using var csv = new TemporaryCsv(
            "NUMBER,DESCRIPTION,LIFECYCLE PHASE,REV,PART TYPE",
            "F123456,Drawing One,Released,A,Drawing");

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await provider.LoadAsync();

        Assert.Single(provider.Register);
    }

    [Fact]
    public async Task LoadAsync_HeaderWithBom_LoadsSuccessfully()
    {
        using var csv = new TemporaryCsv(
            "\uFEFFNumber,Description,Lifecycle Phase,Rev,Part Type",
            "F123456,Drawing One,Released,A,Drawing");

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await provider.LoadAsync();

        Assert.Single(provider.Register);
    }

    [Theory]
    [InlineData("Number,Description,Lifecycle Phase,Rev")]
    [InlineData("Number,Description,Lifecycle Phase,Part Type")]
    [InlineData("Number,Description,Rev,Part Type")]
    [InlineData("Number,Lifecycle Phase,Rev,Part Type")]
    [InlineData("Description,Lifecycle Phase,Rev,Part Type")]
    public async Task LoadAsync_MissingRequiredHeader_Throws(
        string header)
    {
        using var csv = new TemporaryCsv(header);

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => provider.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_RowWithMissingColumns_Throws()
    {
        using var csv = new TemporaryCsv(
            Header,
            "F123456,Drawing One,Released,A");

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => provider.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_EmptyDocumentNumber_Throws()
    {
        using var csv = new TemporaryCsv(
            Header,
            ",Drawing One,Released,A,Drawing");

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => provider.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_DuplicateDocumentNumbers_Throws()
    {
        using var csv = new TemporaryCsv(
            Header,
            "F123456,Drawing One,Released,A,Drawing",
            "F123456,Drawing Two,Released,B,Drawing");

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => provider.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_DuplicateNumbers_AreCaseInsensitive()
    {
        using var csv = new TemporaryCsv(
            Header,
            "F123456,Drawing One,Released,A,Drawing",
            "f123456,Drawing Two,Released,B,Drawing");

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => provider.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_RegisterLookup_IsCaseInsensitive()
    {
        using var csv = new TemporaryCsv(
            Header,
            "F123456,Drawing One,Released,A,Drawing");

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await provider.LoadAsync();

        Assert.True(provider.Register.ContainsKey("f123456"));
    }

    [Fact]
    public async Task LoadAsync_QuotedComma_ParsesSuccessfully()
    {
        using var csv = new TemporaryCsv(
            Header,
            "F123456,\"Drawing, Assembly\",Released,A,Drawing");

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await provider.LoadAsync();

        Assert.Single(provider.Register);
    }

    [Fact]
    public async Task LoadAsync_UnmatchedQuotes_Throws()
    {
        using var csv = new TemporaryCsv(
            Header,
            "F123456,\"Drawing One,Released,A,Drawing");

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => provider.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_CancelledToken_Throws()
    {
        using var csv = new TemporaryCsv(
            Header,
            "F123456,Drawing One,Released,A,Drawing");

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.LoadAsync(cts.Token));
    }

    [Fact]
    public async Task LoadAsync_FailedReload_PreservesPreviousRegister()
    {
        using var csv = new TemporaryCsv(
            Header,
            "F123456,Drawing One,Released,A,Drawing");

        var provider = new CsvDocumentRegisterProvider(csv.FilePath);

        await provider.LoadAsync();
        Assert.Single(provider.Register);

        // Replace the source with invalid data.
        File.WriteAllLines(csv.FilePath,
        [
            Header,
            "F123456,Drawing One"
        ]);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => provider.LoadAsync());

        // Previous successful load should remain available.
        Assert.Single(provider.Register);
        Assert.True(provider.Register.ContainsKey("F123456"));
    }

    /// <summary>
    /// Creates a temporary CSV file and removes it after testing.
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
