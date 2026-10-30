using DigitalManualSet.Core.Documents;

namespace DigitalManualSet.Infrastructure.Documents;

/// <summary>
/// Validates documents by checking that corresponding files exist in the working directory.
/// </summary>
public class DocumentValidationService : IDocumentValidationService
{
    /// <summary>
    /// Validates that all documents have matching files in the working directory.
    /// Sets IsValidated and LocalFilePath properties on each document.
    /// A document is valid if exactly one file exists with matching document number and file size is not zero.
    /// </summary>
    /// <inheritdoc />
    public DocumentValidationResult Validate(IEnumerable<Document> documents, string workingDirectory)
    {
        var errors = new List<string>();

        foreach (var document in documents)
        {
            document.IsValidated = false;
            document.LocalFilePath = null;

            var matchingFiles = Directory
                .EnumerateFiles(workingDirectory)
                .Where(file =>
                    string.Equals(
                        Path.GetFileNameWithoutExtension(file),
                        document.DocumentNumber,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingFiles.Count == 0)
            {
                errors.Add($"{document.DocumentNumber}: file not found.");

                continue;
            }

            if (matchingFiles.Count > 1)
            {
                errors.Add($"{document.DocumentNumber}: multiple files found.");

                continue;
            }

            var fileInfo = new FileInfo(matchingFiles[0]);

            if (fileInfo.Length == 0)
            {
                errors.Add($"{document.DocumentNumber}: file is empty.");
                continue;
            }

            // All checks passed so flag that this document is validated.
            document.IsValidated = true;
            var filePath = matchingFiles[0];
            document.LocalFilePath = filePath;
            document.Length = fileInfo.Length;
        }

        return new DocumentValidationResult(errors.Count == 0, errors);
    }
}