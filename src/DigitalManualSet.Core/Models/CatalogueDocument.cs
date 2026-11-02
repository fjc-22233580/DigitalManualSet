namespace DigitalManualSet.Core.Models;

public record CatalogueDocument
{
    public string DocumentName { get; init; } = string.Empty;
    public string DocumentNumber { get; init; } = string.Empty;
    public string Revision { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public string? Note { get; init; }
}