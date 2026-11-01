namespace DigitalManualSet.Core.Documents;

public record CsvImportDefinition(
    int DocumentNumberColumnIndex,
    int DocumentNameColumnIndex);