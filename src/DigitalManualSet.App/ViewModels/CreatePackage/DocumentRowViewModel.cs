using DigitalManualSet.App.Common;
using DigitalManualSet.Core.Documents;

namespace DigitalManualSet.App.ViewModels.CreatePackage;

/// <summary>
/// Represents a single document row in the download package view.
/// </summary>
public class DocumentRowViewModel : ViewModel
{
    private readonly Document _document;
    private readonly IDocumentDownloadService _downloadService;
    private readonly string _destinationFolder;

    /// <summary>
    /// Initializes a new instance of the DocumentRowViewModel.
    /// </summary>
    public DocumentRowViewModel(int rowNumber, Document document, IDocumentDownloadService downloadService, string destinationFolder)
    {
        RowNumber = rowNumber;
        _document = document;
        _downloadService = downloadService;
        _destinationFolder = destinationFolder;

        Status = "Pending";
    }

    /// <summary>
    /// Gets the row number (1-based index).
    /// </summary>
    public int RowNumber { get; }

    /// <summary>
    /// Gets the document's identifying number.
    /// </summary>
    public string DocumentNumber => _document.DocumentNumber;

    /// <summary>
    /// Gets the document's display name.
    /// </summary>
    public string DocumentName => _document.DocumentName;

    /// <summary>
    /// Gets the document's revision.
    /// </summary>
    public string Revision => _document.Revision;

    /// <summary>
    /// Gets the document's category.
    /// </summary>
    public string Category => _document.Category;

    /// <summary>
    /// Gets or sets the current download status message.
    /// </summary>
    public string Status
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the document is currently downloading.
    /// </summary>
    public bool IsDownloading
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Downloads the document asynchronously to the destination folder.
    /// Updates Status property with progress and result messages.
    /// </summary>
    public async Task DownloadAsync()
    {
        try
        {
            IsDownloading = true;
            Status = "Starting...";

            var progress = new Progress<string>(message =>
            {
                Status = message;
            });

            var result = await _downloadService.DownloadAsync(
                _document,
                _destinationFolder,
                progress);

            Status = result.Success
                ? "Downloaded"
                : $"Failed: {result.ErrorMessage}";
        }
        catch (Exception ex)
        {
            Status = $"Failed: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
        }
    }
}