using DigitalManualSet.App.Common;
using DigitalManualSet.App.Services.Interfaces;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;
using DigitalManualSet.Core.Common;
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.Models;
using DigitalManualSet.Core.PackageCreation.Workflow;
using DigitalManualSet.Core.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

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
    private readonly IOrderImportOrchestrator _orderImportOrchestrator;
    private readonly IDocumentCatalogueService _catalogueService;
    private readonly IDocumentRegisterValidator _documentRegisterValidator;
    private readonly ProcessDocumentsStep _step;
    private DocumentValidationResult? _validationResult;
    private bool _isDownloading;
    private int _completedDocumentsCount;
    private List<Document> _workingDocuments = [];
    private List<string> _documentSourceFiles = [];

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
    /// <param name="messageService">The message service.</param>
    /// <param name="orderImportOrchestrator">The order import orchestrator.</param>
    /// <param name="catalogueService">The catalogue service.</param>
    /// <param name="documentRegisterValidator"></param>
    /// <exception cref="System.ArgumentException">Expected a {nameof(CreateOrderStep)}. - step</exception>
    public ProcessDocumentsViewModel(ProcessDocumentsStep step,
        IDocumentDownloadService downloadService,
        IAppPathProvider appPathProvider,
        IDocumentValidationService documentValidationService,
        IAddDocumentDialogService addDocumentDialogService,
        IMessageService messageService,
        IOrderImportOrchestrator orderImportOrchestrator,
        IDocumentCatalogueService catalogueService,
        IDocumentRegisterValidator documentRegisterValidator)
    {
        _step = step ?? throw new ArgumentException($"Expected a {nameof(CreateOrderStep)}.", nameof(step));
        _downloadService = downloadService;
        _appPathProvider = appPathProvider;
        _documentValidationService = documentValidationService;
        _addDocumentDialogService = addDocumentDialogService;
        _messageService = messageService;
        _orderImportOrchestrator = orderImportOrchestrator;
        _catalogueService = catalogueService;
        _documentRegisterValidator = documentRegisterValidator;

        _step.SetWorkingFolder(_appPathProvider.DocumentsFolder);
        Directory.CreateDirectory(DestinationFolder);

        CreateCommands();
        CreateFixedDocuments();
    }

    #endregion

    #region Properties

    /// <summary>
    /// True if we're currently running an import.
    /// </summary>
    public bool IsImportRunning
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets the import status string which is shown in the UI.
    /// </summary>
    public string ImportStatus
    {
        get;
        set => SetProperty(ref field, value);
    } = "Awaiting documents";

    /// <summary>
    /// Gets or sets the overall download progress as a percentage (0-100).
    /// </summary>
    public double OverallProgress
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool AnyItemsObsolete => _workingDocuments.Any(doc => doc.IsObsolete);

    /// <summary>
    /// Gets or sets the total count of documents to download.
    /// </summary>
    public int DocumentCount => _workingDocuments.Count;

    public int FilesCount => _documentSourceFiles.Count;

    /// <summary>
    /// Gets a formatted string showing download progress and validation status.
    /// </summary>
    public string DownloadStatus =>
        $"{_completedDocumentsCount} / {DocumentCount}"
        + (_isDownloading ? " downloading" : " downloaded")
        + (_validationResult?.IsValid == true ? " and validated" : "");

    /// <summary>
    /// Gets the destination folder path where documents are downloaded.
    /// </summary>
    public string DestinationFolder => _step.WorkingFolder;

    /// <summary>
    /// Gets the collection of documents available for download.
    /// </summary>
    public ObservableCollection<DocumentRowViewModel> AvailableDocuments { get; } = new();


    /// <summary>
    /// Gets a value indicating whether any of the documents are manually added.
    /// </summary>
    public bool IsAnyManuallyAddedDocument => AvailableDocuments.Any(x => x.IsManuallyAdded);

    /// <summary>
    /// Gets the display title for the workflow step.
    /// </summary>
    public string Title => "Placeholder: import documents.";

    #endregion

    #region Commands

    /// <summary>
    /// Gets the import documents command.
    /// </summary>
    public AsyncRelayCommand ImportDocumentsCommand { get; private set; }

    /// <summary>
    /// Gets the command to begin downloading all documents.
    /// </summary>
    public AsyncRelayCommand BeginDownloadCommand { get; private set; }

    /// <summary>
    /// Gets the command to add a custom document.
    /// </summary>
    public RelayCommand AddDocumentCommand { get; private set; }

    /// <summary>
    /// Gets the command to open the destination folder in the file explorer.
    /// </summary>
    public RelayCommand OpenDestinationFolder { get; private set; }

    /// <summary>
    /// Creates the various ICommands for this VM.
    /// </summary>
    private void CreateCommands()
    {
        OpenDestinationFolder = new RelayCommand(() =>
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = DestinationFolder,
                UseShellExecute = true
            });
        });

        BeginDownloadCommand = new AsyncRelayCommand(DownloadAllAsync);
        AddDocumentCommand = new RelayCommand(AddDocument, CanAddDocuments);
        ImportDocumentsCommand = new AsyncRelayCommand(ImportOrderFilesAsync, CanAddDocuments);
    }


    #endregion

    #region Methods


    private bool CanAddDocuments()
    {
        return _isDownloading == false;
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
        // Update UI
        _isDownloading = true;
        ImportDocumentsCommand.RaiseCanExecuteChanged();
        AddDocumentCommand.RaiseCanExecuteChanged();
        _validationResult = null;
        _completedDocumentsCount = 0;
        OverallProgress = 0;
        OnPropertyChanged(nameof(DownloadStatus));

        // Create downloading tasks
        var tasks = AvailableDocuments.Select(async document =>
        {
            var success = await document.DownloadAsync();

            // Flag if document wasn't found.
            if (success)
            {
                _completedDocumentsCount++;
            }

            // Update UI
            OverallProgress = (double)_completedDocumentsCount / DocumentCount * 100;
            OnPropertyChanged(nameof(DownloadStatus));
        });

        // Carry out the downloading tasks
        await Task.WhenAll(tasks);

        // Update UI
        _isDownloading = false;
        ImportDocumentsCommand.RaiseCanExecuteChanged();
        AddDocumentCommand.RaiseCanExecuteChanged();

        // Carry out validation
        _validationResult = _documentValidationService.Validate(_workingDocuments, DestinationFolder);
        if (_validationResult.IsValid)
        {
            var validItems = _workingDocuments
                .Where(doc => doc.IsValidated)
                .ToList();

            _step.UpdateValidationState(validItems);
        }

        // Update UI
        OnPropertyChanged(nameof(DownloadStatus));
        OnPropertyChanged(nameof(DocumentCount));
    }

    /// <summary>
    /// Imports the order files asynchronous.
    /// </summary>
    private async Task ImportOrderFilesAsync()
    {

        // Ask user to pick the files
        var dialog = new OpenFileDialog
        {
            Title = "Select ERP Order Exports",
            Filter = "CSV Files (*.csv)|*.csv",
            Multiselect = true,
            CheckFileExists = true
        };

        // Only procced if they have chosen something
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        // Check we have new files - we cache them so we can check if come here again.
        var newItems = dialog.FileNames.Except(_documentSourceFiles, StringComparer.OrdinalIgnoreCase).ToList();
        if (newItems.Count == 0)
        {
            return;
        }
        
        IReadOnlyList<Document> processedDocuments = null;

        try
        {
            // Update UI
            IsImportRunning = true;
            ImportStatus = "Processing documents";

            // Carry out the import async.
            processedDocuments = await _orderImportOrchestrator.ImportAsync(newItems);
            IsImportRunning = false;
        }
        catch (NotSupportedException e)
        {
            ImportStatus = $"Operation failed: {e.Message}";
        }
        catch (InvalidDataException e)
        {
            ImportStatus = $"Operation failed: {e.Message}";
        }
        catch (OperationCanceledException e)
        {
            ImportStatus = $"Operation was cancelled ({e.Message})";
        }
        catch (Exception e)
        {
            ImportStatus = $"Unknown error encountered: ({e.Message})";
        }
        finally
        {
            IsImportRunning = false;
        }

        // Check we parsed something
        if (processedDocuments is null)
        {
            return;
        }

        // All seems okay, proceed.

        // Cache the user selected files
        _documentSourceFiles.AddRange(newItems);

        // Find unique items - prevents adding items twice
        var newDocuments = processedDocuments.ExceptBy(_workingDocuments.Select(d => d.DocumentNumber), d => d.DocumentNumber, StringComparer.OrdinalIgnoreCase).ToList();

        // Cache and create views.
        _workingDocuments.AddRange(newDocuments);
        foreach (var document in newDocuments)
        { 
            AvailableDocuments.Add(new DocumentRowViewModel(AvailableDocuments.Count + 1, document, _downloadService, DestinationFolder));
        }

        OnPropertyChanged(nameof(FilesCount));
        OnPropertyChanged(nameof(AnyItemsObsolete));
        OnPropertyChanged(nameof(DownloadStatus));
    }


    /// <summary>
    /// Creates the fixed / generic documents.
    /// </summary>
    private void CreateFixedDocuments()
    {
        string modelNumber = Regex.Match(_step.Order.SystemId, @"^(?:E\d+|\d+)", RegexOptions.IgnoreCase).Value;
        var orderFixedDocuments = _catalogueService.GetDocumentsForModel(modelNumber);
        if (orderFixedDocuments.Count == 0)
        {
            return;
        }

        var documents = orderFixedDocuments.Select(Create<Document>).ToList();
        var validatedDocuments = _documentRegisterValidator.Validate(documents);
        var newDocuments = validatedDocuments.ExceptBy(_workingDocuments.Select(d => d.DocumentNumber), d => d.DocumentNumber, StringComparer.OrdinalIgnoreCase).ToList();

        _workingDocuments.AddRange(newDocuments);
        foreach (var document in newDocuments)
        {
            AvailableDocuments.Add(new DocumentRowViewModel(AvailableDocuments.Count + 1, document, _downloadService, DestinationFolder));
        }

        OnPropertyChanged(nameof(FilesCount));
        OnPropertyChanged(nameof(AnyItemsObsolete));
        OnPropertyChanged(nameof(DownloadStatus));

    }

    /// <summary>
    /// Factory for creating a document from a catalogue document.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arg">The argument.</param>
    /// <returns></returns>
    private Document Create<T>(CatalogueDocument arg)
    {
        return new Document(arg.DocumentNumber, arg.DocumentName, arg.Revision, "")
        {
            CategoryType = DocumentCategory.Generic,
        };
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
