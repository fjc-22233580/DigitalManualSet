using DigitalManualSet.Core.Documents;

namespace DigitalManualSet.Infrastructure.Documents;

/// <summary>
/// Downloads documents from a local directory with simulated delays.
/// Used for testing document download workflows.
/// </summary>
public class LocalDocumentDownloadService : IDocumentDownloadService
{
    private readonly string _sourceDirectory;

    /// <summary>
    /// Initializes a new instance of the LocalDocumentDownloadService class.
    /// Creates test document files in the source directory during initialization.
    /// </summary>
    /// <param name="sourceDirectory">The directory containing source documents to simulate downloads from.</param>
    public LocalDocumentDownloadService(string sourceDirectory)
    {
        _sourceDirectory = sourceDirectory;

        CreateTestFiles();
    }

    /// <summary>
    /// Creates dummy test document files if they don't already exist.
    /// </summary>
    private void CreateTestFiles()
    {

        var dummyDirectory = @"C:\DummyDocuments";

        Directory.CreateDirectory(_sourceDirectory);

        var documentNumbers = new[]
        {
            "DOC-1001",
            "DOC-1002",
            "DOC-1003",
            "DOC-1004",
            "DOC-1005",
            "DOC-1006",
            "DOC-1007",
            "DOC-1008",
            "DOC-1009",
            "DOC-1010",
            "DOC-1011",
            "DOC-1012"
        };

        foreach (var documentNumber in documentNumbers)
        {
            var filePath = Path.Combine(_sourceDirectory, $"{documentNumber}.pdf");

            if (File.Exists(filePath))
                continue;

            File.WriteAllText(filePath, $"Dummy content for {documentNumber}");
        }
    }

    /// <summary>
    /// Downloads a document from the local source directory to the destination directory.
    /// Simulates the download process with random delays and progress reporting.
    /// </summary>
    /// <inheritdoc />
    public async Task<DocumentDownloadResult> DownloadAsync(Document document, string destinationDirectory, IProgress<string>? progress = null)
    {
        try
        {
            progress?.Report("Finding document...");

            var delay = Random.Shared.Next(1750, 2500);
            await Task.Delay(delay);

            Directory.CreateDirectory(destinationDirectory);

            var sourceFile = Directory
                .EnumerateFiles(_sourceDirectory)
                .FirstOrDefault(file => string.Equals(Path.GetFileNameWithoutExtension(file), document.DocumentNumber, StringComparison.OrdinalIgnoreCase));

            if (sourceFile is null)
            {
                return new DocumentDownloadResult(
                    false,
                    null,
                    $"Document '{document.DocumentName}' could not be found.");
            }

            progress?.Report("Copying document...");

            var delay2 = Random.Shared.Next(2100, 3500);

            await Task.Delay(delay2);

            var destinationPath = Path.Combine(destinationDirectory, Path.GetFileName(sourceFile));

            File.Copy(sourceFile, destinationPath, overwrite: true);

            progress?.Report("Complete");

            return new DocumentDownloadResult(
                true,
                destinationPath,
                null);
        }
        catch (Exception ex)
        {
            return new DocumentDownloadResult(
                false,
                null,
                ex.Message);
        }
    }
}