namespace DigitalManualSet.Core.Models;

public sealed class DocumentManifestDto
{
    public List<ModelDocumentsDto> Models { get; set; } = new();
}

public sealed class ModelDocumentsDto
{
    public string ModelNumber { get; set; } = string.Empty;

    public List<CatalogueDocument> Documents { get; set; } = new();
}