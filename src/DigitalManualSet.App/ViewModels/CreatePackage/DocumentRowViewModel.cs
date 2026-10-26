using DigitalManualSet.App.Common;
using DigitalManualSet.App.Services.Interfaces;
using DigitalManualSet.Core.Documents;

namespace DigitalManualSet.App.ViewModels.CreatePackage;

/// <summary>
/// Represents a single document row in the download package view.
/// </summary>
public class DocumentRowViewModel : ViewModel
{
    #region Events

    public event EventHandler DeleteRequested;

    #endregion

    #region Fields

    private readonly Document _document;
    private readonly IDocumentDownloadService _downloadService;
    private readonly string _destinationFolder;
    private readonly IMessageService? _messageService;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the DocumentRowViewModel.
    /// </summary>
    /// <param name="rowNumber">The row number.</param>
    /// <param name="document">The document.</param>
    /// <param name="downloadService">The download service.</param>
    /// <param name="destinationFolder">The destination folder.</param>
    /// <param name="isManuallyAdded">if set to <c>true</c> [is manually added].</param>
    /// <param name="messageService">The message service.</param>
    public DocumentRowViewModel(int rowNumber,
        Document document,
        IDocumentDownloadService downloadService,
        string destinationFolder,
        bool isManuallyAdded = false,
        IMessageService? messageService = null)
    {
        RowNumber = rowNumber;
        IsManuallyAdded = isManuallyAdded;
        _document = document;
        _downloadService = downloadService;
        _destinationFolder = destinationFolder;
        _messageService = messageService;

        Status = "Pending";

        DeleteDocumentCommand = new RelayCommand(DeleteDocument);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the row number (1-based index).
    /// </summary>
    public int RowNumber { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is manually added.
    /// </summary>
    public bool IsManuallyAdded { get; }

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
    /// Gets the delete document command.
    /// </summary>
    public RelayCommand DeleteDocumentCommand { get; }

    #endregion

    #region Methods

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

    /// <summary>
    /// Deletes this document, if the user has confirmed.
    /// </summary>
    private void DeleteDocument()
    {
        if (_messageService is not null)
        {
            var result = _messageService.ShowQuestion("Confirm deletion", $"Do you wish to delete document: {DocumentNumber}?");
            if (result)
            {
                DeleteRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    #endregion
}