using System.Collections.ObjectModel;
using System.Diagnostics;
using DigitalManualSet.App.Common;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;
using DigitalManualSet.Core.Common;
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.PackageCreation.Workflow;
using DigitalManualSet.Core.Workflow;
using System.IO;

namespace DigitalManualSet.App.ViewModels.CreatePackage;

/// <summary>
/// View model for the "Process Documents" step of the create-package workflow.
/// </summary>
public class ProcessDocumentsViewModel : ViewModel, IPackageWorkflowStepViewModel
{
    private readonly IDocumentDownloadService _downloadService;
    private readonly IAppPathProvider _appPathProvider;
    private readonly IDocumentValidationService _documentValidationService;
    private readonly ProcessDocumentsStep _step;
    private DocumentValidationResult? _validationResult;


    /// <summary>
    /// Initializes a new instance of ProcessDocumentsViewModel and prepares document downloads.
    /// </summary>
    public ProcessDocumentsViewModel(IWorkflowStep<PackageWorkflowStepId> step,
        IDocumentDownloadService downloadService,
        IAppPathProvider appPathProvider,
        IDocumentValidationService documentValidationService)
    {
        _downloadService = downloadService;
        _appPathProvider = appPathProvider;
        _documentValidationService = documentValidationService;
        _step = step as ProcessDocumentsStep ?? throw new ArgumentException($"Expected a {nameof(CreateOrderStep)}.", nameof(step));

        var docs = _step.AvailableDocuments;

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

        BeginDownloadCommand = new RelayCommand(Execute);

        int rowNumber = 1;
        foreach (var stepAvailableDocument in _step.AvailableDocuments)
        {
            AvailableDocuments.Add(new DocumentRowViewModel(rowNumber, stepAvailableDocument, _downloadService, DestinationFolder));
            rowNumber++;
        }
    }

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
    /// Executes the download process asynchronously.
    /// </summary>
    private void Execute()
    {
        _ = DownloadAllAsync();
    }

    private bool _isDownloading;

    private int _completedDocumentsCount;

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
    /// Gets the display title for the workflow step.
    /// </summary>
    public string Title => "Placeholder: import documents.";
}
