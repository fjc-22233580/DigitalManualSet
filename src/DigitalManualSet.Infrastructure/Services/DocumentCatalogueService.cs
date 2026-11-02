using System.Text.Json;
using DigitalManualSet.Core.Models;
using DigitalManualSet.Core.Services;

namespace DigitalManualSet.Infrastructure.Services;

public sealed class DocumentCatalogueService : IDocumentCatalogueService
{
    private readonly string _manifestPath;

    private IReadOnlyDictionary<string, IReadOnlyList<CatalogueDocument>>? _documentsByModel;

    public DocumentCatalogueService(string manifestPath)
    {
        _manifestPath = manifestPath;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(_manifestPath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var manifest = await JsonSerializer.DeserializeAsync<DocumentManifestDto>(stream, options, cancellationToken);

        if (manifest is null)
        {
            throw new InvalidDataException("Document manifest could not be loaded.");
        }

        _documentsByModel 
            = manifest.Models.ToDictionary(model => model.ModelNumber.Trim(), IReadOnlyList<CatalogueDocument> (model) => model.Documents.AsReadOnly(), StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<CatalogueDocument> GetDocumentsForModel(string modelNumber)
    {
        if (_documentsByModel is null)
        {
            throw new InvalidOperationException("Document catalogue has not been loaded.");
        }

        if (string.IsNullOrWhiteSpace(modelNumber))
        {
            return [];
        }

        return _documentsByModel.TryGetValue(
            modelNumber.Trim(), out var documents)
            ? documents
            : [];
    }
}