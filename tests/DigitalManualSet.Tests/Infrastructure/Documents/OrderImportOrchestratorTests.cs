
using System.Reflection;
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Infrastructure.Documents;
using Xunit;

namespace DigitalManualSet.Tests.Infrastructure.Documents;

public class OrderImportOrchestratorTests
{
    private static (OrderImportOrchestrator Orchestrator, ValidatorSpy Spy) CreateSut()
    {
        var validator = DispatchProxy.Create<IDocumentRegisterValidator, ValidatorSpy>();

        return (new OrderImportOrchestrator(validator), (ValidatorSpy)(object)validator);
    }

    [Fact]
    public async Task ImportAsync_NullFilePaths_ThrowsArgumentNullException()
    {
        var (orchestrator, _) = CreateSut();

        await Assert.ThrowsAsync<ArgumentNullException>(() => orchestrator.ImportAsync(null!));
    }

    [Fact]
    public async Task ImportAsync_EmptyFilePaths_ReturnsEmpty()
    {
        var (orchestrator, spy) = CreateSut();

        var result = await orchestrator.ImportAsync([]);

        Assert.Empty(result);
        Assert.Equal(1, spy.CallCount);
        Assert.Empty(spy.ReceivedDocuments);
    }

    [Fact]
    public async Task ImportAsync_ProductStructureCP_ImportsDocuments()
    {
        using var csv = new TemporaryCsv(
            "ProductStructureCP",
            "A,B,C,D,E,F,G,H,Number,Description",
            "1,2,3,4,5,6,7,8,F123456,Drawing One",
            "1,2,3,4,5,6,7,8,M654321,Manual One");

        var (orchestrator, spy) = CreateSut();

        var result = await orchestrator.ImportAsync([csv.FilePath]);

        Assert.Equal(2, result.Count);
        Assert.Equal(2, spy.ReceivedDocuments.Count);
        Assert.Equal(1, spy.CallCount);
    }

    [Fact]
    public async Task ImportAsync_QuickReport_ImportsDocuments()
    {
        using var csv = new TemporaryCsv(
            "QuickReportShopOrderManualandFDrgList",
            "A,B,C,Part No,Description",
            "1,2,3,F123456,Drawing One",
            "1,2,3,HS654321,Safety Document");

        var (orchestrator, spy) = CreateSut();

        var result = await orchestrator.ImportAsync([csv.FilePath]);

        Assert.Equal(2, result.Count);
        Assert.Equal(1, spy.CallCount);
    }

    [Fact]
    public async Task ImportAsync_MultipleFiles_CombinesDocuments()
    {
        using var csv1 = new TemporaryCsv(
            "ProductStructureCP",
            "A,B,C,D,E,F,G,H,Number,Description",
            "1,2,3,4,5,6,7,8,F123456,Drawing One");

        using var csv2 = new TemporaryCsv(
            "QuickReportShopOrderManualandFDrgList",
            "A,B,C,Part No,Description",
            "1,2,3,M654321,Manual One");

        var (orchestrator, spy) = CreateSut();

        var result = await orchestrator.ImportAsync(
            [csv1.FilePath, csv2.FilePath]);

        Assert.Equal(2, result.Count);
        Assert.Equal(1, spy.CallCount);
    }

    [Fact]
    public async Task ImportAsync_DuplicateDocuments_RemovesDuplicates()
    {
        using var csv1 = new TemporaryCsv(
            "ProductStructureCP",
            "A,B,C,D,E,F,G,H,Number,Description",
            "1,2,3,4,5,6,7,8,F123456,Drawing One");

        using var csv2 = new TemporaryCsv(
            "QuickReportShopOrderManualandFDrgList",
            "A,B,C,Part No,Description",
            "1,2,3,f123456,Duplicate Drawing",
            "1,2,3,M654321,Manual One");

        var (orchestrator, spy) = CreateSut();

        var result = await orchestrator.ImportAsync(
            [csv1.FilePath, csv2.FilePath]);

        Assert.Equal(2, result.Count);
        Assert.Equal(2, spy.ReceivedDocuments.Count);
        Assert.Equal("F123456", spy.ReceivedDocuments[0].DocumentNumber);
        Assert.Equal("M654321", spy.ReceivedDocuments[1].DocumentNumber);
    }

    [Fact]
    public async Task ImportAsync_UnsupportedFile_ThrowsNotSupportedException()
    {
        using var csv = new TemporaryCsv(
            "UnsupportedExport",
            "Number,Description",
            "F123456,Drawing One");

        var (orchestrator, spy) = CreateSut();

        await Assert.ThrowsAsync<NotSupportedException>(
            () => orchestrator.ImportAsync([csv.FilePath]));

        Assert.Equal(0, spy.CallCount);
    }

    [Fact]
    public async Task ImportAsync_InvalidCsv_ThrowsInvalidDataException()
    {
        using var csv = new TemporaryCsv(
            "ProductStructureCP",
            "A,B,C,D,E,F,G,H,Number,Description",
            "1,2,3,F123456,Drawing One");

        var (orchestrator, spy) = CreateSut();

        await Assert.ThrowsAsync<InvalidDataException>(
            () => orchestrator.ImportAsync([csv.FilePath]));

        Assert.Equal(0, spy.CallCount);
    }

    [Fact]
    public async Task ImportAsync_UnsupportedDocumentPrefixes_AreFiltered()
    {
        using var csv = new TemporaryCsv(
            "QuickReportShopOrderManualandFDrgList",
            "A,B,C,Part No,Description",
            "1,2,3,X123456,Unsupported Document",
            "1,2,3,F123456,Valid Drawing");

        var (orchestrator, spy) = CreateSut();

        var result = await orchestrator.ImportAsync([csv.FilePath]);

        var document = Assert.Single(result);

        Assert.Equal("F123456", document.DocumentNumber);
        Assert.Single(spy.ReceivedDocuments);
    }

    [Fact]
    public async Task ImportAsync_UsesValidatorResult()
    {
        using var csv = new TemporaryCsv(
            "QuickReportShopOrderManualandFDrgList",
            "A,B,C,Part No,Description",
            "1,2,3,F123456,Drawing One");

        var (orchestrator, spy) = CreateSut();

        var validated = new List<Document>
        {
            new("F123456", "Validated Drawing", "B", "Released")
        };

        spy.ResultOverride = validated;

        var result = await orchestrator.ImportAsync([csv.FilePath]);

        Assert.Same(validated, result);
        Assert.Single(spy.ReceivedDocuments);
        Assert.Equal(1, spy.CallCount);
    }

    [Fact]
    public async Task ImportAsync_CancelledToken_Throws()
    {
        using var csv = new TemporaryCsv(
            "QuickReportShopOrderManualandFDrgList",
            "A,B,C,Part No,Description",
            "1,2,3,F123456,Drawing One");

        var (orchestrator, spy) = CreateSut();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => orchestrator.ImportAsync([csv.FilePath], cts.Token));

        Assert.Equal(0, spy.CallCount);
    }

    /// <summary>
    /// A test double that records calls to the validator.
    /// </summary>
    public class ValidatorSpy : DispatchProxy
    {
        public int CallCount { get; private set; }

        public IReadOnlyList<Document> ReceivedDocuments { get; private set; }
            = [];

        public List<Document>? ResultOverride { get; set; }

        protected override object? Invoke(
            MethodInfo? targetMethod,
            object?[]? args)
        {
            if (targetMethod?.Name != "Validate")
            {
                throw new NotSupportedException();
            }

            CallCount++;

            var documents = (IEnumerable<Document>)args![0]!;

            var captured = documents.ToList();

            ReceivedDocuments = captured;

            return ResultOverride ?? captured;
        }
    }

    /// <summary>
    /// Creates a temporary CSV file and deletes it after testing.
    /// </summary>
    private sealed class TemporaryCsv : IDisposable
    {
        public string FilePath { get; }

        public TemporaryCsv(string prefix, params string[] lines)
        {
            FilePath = Path.Combine(
                Path.GetTempPath(),
                $"{prefix}_{Guid.NewGuid():N}.csv");

            File.WriteAllLines(FilePath, lines);
        }

        public void Dispose()
        {
            File.Delete(FilePath);
        }
    }
}
