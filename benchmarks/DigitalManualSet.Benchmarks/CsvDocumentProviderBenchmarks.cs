using System.Text;
using BenchmarkDotNet.Attributes;
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Infrastructure.Documents;

namespace DigitalManualSet.Benchmarks;

/// <summary>
/// Benchmarks CSV document parsing for both the default and ERP import formats.
/// </summary>
[MemoryDiagnoser]
public class CsvDocumentProviderBenchmarks
{
    private string _defaultFilePath = string.Empty;
    private string _erpFilePath = string.Empty;
    private CsvDocumentProvider _defaultProvider = null!;
    private CsvDocumentProvider _erpProvider = null!;

    [Params(100, 1_000, 10_000)]
    public int RowCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _defaultFilePath = Path.Combine(Path.GetTempPath(), $"dms-bench-default-{Guid.NewGuid():N}.csv");
        _erpFilePath = Path.Combine(Path.GetTempPath(), $"dms-bench-erp-{Guid.NewGuid():N}.csv");

        var defaultCsv = new StringBuilder("DocumentNumber,DocumentName,Revision,Category\n");
        var erpCsv = new StringBuilder("Line,Item,Description,Qty,Notes\n");
        string[] prefixes = ["F", "M", "HS", "P", "X"];

        for (var i = 0; i < RowCount; i++)
        {
            defaultCsv.Append($"M{i:D6},\"Manual {i}, \"\"Quoted\"\" part\",A,Manuals\n");

            var prefix = prefixes[i % prefixes.Length];
            erpCsv.Append($"{i},{prefix}{i:D6},\"Description {i}, with comma\",1,Note {i}\n");
        }

        File.WriteAllText(_defaultFilePath, defaultCsv.ToString());
        File.WriteAllText(_erpFilePath, erpCsv.ToString());

        _defaultProvider = new CsvDocumentProvider(_defaultFilePath);
        _erpProvider = new CsvDocumentProvider(_erpFilePath, new CsvImportDefinition(1, 2));
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        File.Delete(_defaultFilePath);
        File.Delete(_erpFilePath);
    }

    [Benchmark(Baseline = true)]
    public Task<IReadOnlyList<Document>> DefaultFormat() => _defaultProvider.GetDocumentsAsync();

    [Benchmark]
    public Task<IReadOnlyList<Document>> ErpFormat() => _erpProvider.GetDocumentsAsync();
}
