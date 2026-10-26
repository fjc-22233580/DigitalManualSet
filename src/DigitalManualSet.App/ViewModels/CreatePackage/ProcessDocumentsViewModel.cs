using System.Collections.ObjectModel;
using System.Diagnostics;
using DigitalManualSet.App.Common;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;
using DigitalManualSet.Core.Common;
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.PackageCreation.Workflow;
using DigitalManualSet.Core.Workflow;
using System.IO;
using DigitalManualSet.App.Services.Interfaces;

namespace DigitalManualSet.App.ViewModels.CreatePackage;

/// <summary>
/// View model for the "Process Documents" step of the create-package workflow.
/// </summary>
public class ProcessDocumentsViewModel : ViewModel, IPackageWorkflowStepViewModel
{
    #region Fields

    private readonly IDocumentDownloadService _downloadService;
    private readonly IAppPathProvider _appPathProvider;
    private readonly IDocumentValidationService _documentValidationService;
    private readonly IAddDocumentDialogService _addDocumentDialogService;
    private readonly IMessageService _messageService;
    private readonly ProcessDocumentsStep _step;
    private DocumentValidationResult? _validationResult;
    private bool _isDownloading;
    private int _completedDocumentsCount;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of ProcessDocumentsViewModel and prepares document downloads.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <param name="downloadService">The download service.</param>
    /// <param name="appPathProvider">The application path provider.</param>
    /// <param name="documentValidationService">The document validation service.</param>
    /// <param name="addDocumentDialogService">The add document dialog service.</param>
    /// <exception cref="System.ArgumentException">Expected a {nameof(CreateOrderStep)}. - step</exception>
    public ProcessDocumentsViewModel(IWorkflowStep<PackageWorkflowStepId> step,
        IDocumentDownloadService downloadService,
        IAppPathProvider appPathProvider,
        IDocumentValidationService documentValidationService,
        IAddDocumentDialogService addDocumentDialogService,
        IMessageService messageService)
    {
        _step = step as ProcessDocumentsStep ?? throw new ArgumentException($"Expected a {nameof(CreateOrderStep)}.", nameof(step));
        _downloadService = downloadService;
        _appPathProvider = appPathProvider;
        _documentValidationService = documentValidationService;
        _addDocumentDialogService = addDocumentDialogService;
        _messageService = messageService;

        OpenDestinationFolder = new RelayCommand(() =>
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = DestinationFolder,
                UseShellExecute = true
            });
        });

        _step.SetWorkingFolder(_appPathProvider.DocumentsFolder);
        Directory.CreateDirectory(DestinationFolder);

        DocumentCount = _step.AvailableDocuments.Count;

        BeginDownloadCommand = new RelayCommand(Download);
        AddDocumentCommand = new RelayCommand(AddDocument);

        int rowNumber = 1;
        foreach (var document in _step.AvailableDocuments)
        {
            AvailableDocuments.Add(new DocumentRowViewModel(rowNumber, document, _downloadService, DestinationFolder));
            rowNumber++;
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the overall download progress as a percentage (0-100).
    /// </summary>
    public double OverallProgress
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets or sets the total count of documents to download.
    /// </summary>
    public int DocumentCount
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets a formatted string showing download progress and validation status.
    /// </summary>
    public string DownloadStatus =>
        $"{_completedDocumentsCount} / {DocumentCount}"
        + (_isDownloading ? " downloading" : " downloaded")
        + (_validationResult?.IsValid == true ? " and validated" : "");

    /// <summary>
    /// Gets the command to begin downloading all documents.
    /// </summary>
    public RelayCommand BeginDownloadCommand { get; }

    /// <summary>
    /// Gets the command to add a custom document.
    /// </summary>
    public RelayCommand AddDocumentCommand { get; }

    /// <summary>
    /// Gets the command to open the destination folder in the file explorer.
    /// </summary>
    public RelayCommand OpenDestinationFolder { get; }

    /// <summary>
    /// Gets the destination folder path where documents are downloaded.
    /// </summary>
    public string DestinationFolder => _step.WorkingFolder;

    /// <summary>
    /// Gets the collection of documents available for download.
    /// </summary>
    public ObservableCollection<DocumentRowViewModel> AvailableDocuments { get; } = new();

    /// <summary>
    /// Gets the filename associated with the documents.
    /// </summary>
    public string DocumentsFilename => _step.DocumentsFilename;

    /// <summary>
    /// Gets a value indicating whether any of the documents are manually added.
    /// </summary>
    public bool IsAnyManuallyAddedDocument => AvailableDocuments.Any(x => x.IsManuallyAdded);

    /// <summary>
    /// Gets the display title for the workflow step.
    /// </summary>
    public string Title => "Placeholder: import documents.";

    #endregion

    #region Methods

    /// <summary>
    /// Executes the download process asynchronously.
    /// </summary>
    private void Download()
    {
        _ = DownloadAllAsync();
    }

    /// <summary>
    /// Adds a custom document to the list
    /// </summary>
    private void AddDocument()
    {
        var result = _addDocumentDialogService.ShowDialog();

        if (result is null)
        {
            return;
        }

        
        int count = AvailableDocuments.Count;
        var vm = new DocumentRowViewModel(++count, result, _downloadService, DestinationFolder, isManuallyAdded: true, _messageService);
        vm.DeleteRequested += CustomDocument_DeleteRequested;
        AvailableDocuments.Add(vm);
        OnPropertyChanged(nameof(IsAnyManuallyAddedDocument));
    }

    /// <summary>
    /// Downloads all available documents concurrently and validates them afterward.
    /// </summary>
    private async Task DownloadAllAsync()
    {
        _isDownloading = true;
        _validationResult = null;
        _completedDocumentsCount = 0;
        OverallProgress = 0;
        DocumentCount = _step.AvailableDocuments.Count;
        OnPropertyChanged(nameof(DownloadStatus));

        var tasks = AvailableDocuments.Select(async document =>
        {
            await document.DownloadAsync();

            _completedDocumentsCount++;

            OverallProgress = (double)_completedDocumentsCount / DocumentCount * 100;

            OnPropertyChanged(nameof(DownloadStatus));
        });

        await Task.WhenAll(tasks);


        _isDownloading = false;


        _validationResult = _documentValidationService.Validate(_step.AvailableDocuments, DestinationFolder);
        _step.UpdateValidationState();

        OnPropertyChanged(nameof(DownloadStatus));
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Called when the user has chosen to delete a manually added document.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void CustomDocument_DeleteRequested(object? sender, EventArgs e)
    {
        var item = AvailableDocuments.First(vm => vm == sender);
        AvailableDocuments.Remove(item);
        OnPropertyChanged(nameof(IsAnyManuallyAddedDocument));
    }

    #endregion
}
