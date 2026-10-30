using DigitalManualSet.App.Common;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;
using DigitalManualSet.Core.Models;
using DigitalManualSet.Core.Orders;
using DigitalManualSet.Core.PackageCreation.Workflow;
using DigitalManualSet.Core.Services;
using System.Collections.ObjectModel;
using DigitalManualSet.App.Services.Interfaces;

namespace DigitalManualSet.App.ViewModels.CreatePackage;

/// <summary>
/// View model for the "Complete Package" step of the create-package workflow.
/// </summary>
public class CompletePackageViewModel : ViewModel, IPackageWorkflowStepViewModel
{
    #region Fields

    private readonly CompletePackageStep _model;
    private readonly IRemovableDriveService _removableDriveService;
    private readonly IMessageService _messageService;
    private readonly IRemovableDriveCleaner _removableDriveCleaner;
    private RemovableDrive? _selectedDrive;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CompletePackageViewModel" /> class.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <param name="removableDriveService">The removable drive service.</param>
    /// <param name="messageService">The message service.</param>
    /// <param name="removableDriveCleaner">The removable drive cleaner.</param>
    /// <exception cref="ArgumentException">Expected a {nameof(CreateOrderStep)}. - step</exception>
    /// <exception cref="System.ArgumentException">Expected a {nameof(CreateOrderStep)}. - step</exception>
    public CompletePackageViewModel(CompletePackageStep step, IRemovableDriveService removableDriveService, IMessageService messageService, IRemovableDriveCleaner removableDriveCleaner)
    {
        _model = step ?? throw new ArgumentException($"Expected a {nameof(CreateOrderStep)}.", nameof(step));
        _removableDriveService = removableDriveService;
        _messageService = messageService;
        _removableDriveCleaner = removableDriveCleaner;

        CreatePackageCommand = new AsyncRelayCommand(CreatePackageAsync, CanCreatePackage);
        RefreshDrivesCommand = new RelayCommand(RefreshDrives);

        RefreshDrives();
        _selectedDrive = _model.SelectedDrive;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the order for which the package is being created.
    /// </summary>
    public Order Order => _model.Order!;

    /// <summary>
    /// Gets the number of documents included in the package.
    /// </summary>
    public int DocumentCount => _model.DocumentCount;

    /// <summary>
    /// Gets the total size of the documents included in the package.
    /// </summary>
    public long TotalDocumentsSize => _model.TotalDocumentsSize;

    /// <summary>
    /// Gets the command used to refresh the available removable drives.
    /// </summary>
    public RelayCommand RefreshDrivesCommand { get; }

    /// <summary>
    /// Gets the command used to create the package on the selected removable drive.
    /// </summary>
    public AsyncRelayCommand CreatePackageCommand { get; }

    /// <summary>
    /// Gets or sets the removable drive selected as the package output destination.
    /// </summary>
    public RemovableDrive? SelectedDrive
    {
        get => _selectedDrive;
        set
        {
            if (SetProperty(ref _selectedDrive, value))
            {
                _model.SelectedDrive = value;
                CreatePackageCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(DriveSummary));
            }
        }
    }

    /// <summary>
    /// Gets a warning or status message for the selected removable drive.
    /// </summary>
    public string DriveSummary
    {
        get
        {
            string summary = string.Empty;

            if (SelectedDrive is null) return summary;

            if (SelectedDrive.WriteStatus != DriveWriteStatus.Writable)
            {
                summary = "USB Device cannot be written to and cannot be used. Select a different one.";
            }
            else if (SelectedDrive.IsEffectivelyEmpty == false)
            {
                summary = "USB Device may contains files, it can be used but USB will be cleared first.";
            }
            else
            {
                summary = "No warnings. USB is ready to use.";
            }
            return summary;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether package creation is in progress.
    /// </summary>
    public bool IsCreatingPackage
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                CreatePackageCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the overall package creation progress as a percentage.
    /// </summary>
    public double PackageProgress
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets or sets the current package creation status message.
    /// </summary>
    public string PackageStatus
    {
        get;
        set => SetProperty(ref field, value);
    } = "Pending download...";

    /// <summary>
    /// Gets or sets details about the current package creation operation.
    /// </summary>
    public string PackageDetail
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>
    /// Gets or sets the package creation stage currently in progress.
    /// </summary>
    public PackageCreationStage? CurrentStage
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets the removable drives currently available for package output.
    /// </summary>
    public ObservableCollection<RemovableDrive> AvailableDrives { get; } = [];

    /// <summary>
    /// Gets the human-readable title for this workflow step.
    /// </summary>
    public string Title => "Placeholder: Complete package.";

    /// <summary>
    /// Gets or sets the summary displayed after the documents are copied.
    /// </summary>
    public string DocumentsSummary
    {
        get;
        set => SetProperty(ref field, value);
    } = "—";

    /// <summary>
    /// Gets or sets the summary displayed after the manifest is generated.
    /// </summary>
    public string ManifestSummary
    {
        get;
        set => SetProperty(ref field, value);
    } = "—";

    /// <summary>
    /// Gets or sets the summary displayed after the splash screen is generated.
    /// </summary>
    public string SplashScreenSummary
    {
        get;
        set => SetProperty(ref field, value);
    } = "—";

    /// <summary>
    /// Gets or sets the summary displayed after the certificate is generated.
    /// </summary>
    public string CertificateSummary
    {
        get;
        set => SetProperty(ref field, value);
    } = "—";

    /// <summary>
    /// Gets or sets the summary displayed after package validation.
    /// </summary>
    public string ValidationSummary
    {
        get;
        set => SetProperty(ref field, value);
    } = "—";

    /// <summary>
    /// Gets or sets the USB label summary displayed after finalization.
    /// </summary>
    public string UsbLabelSummary
    {
        get;
        set => SetProperty(ref field, value);
    } = "—";

    #endregion

    #region Methods

    /// <summary>
    /// Clears the selected drive when necessary and creates the package while updating progress.
    /// </summary>
    private async Task CreatePackageAsync()
    {
        bool canProceed = true;

        if (SelectedDrive.IsEffectivelyEmpty == false)
        {
            canProceed = _messageService.ShowQuestion("Create Package", "The destination USB contains 1 or more files, if you proceed these files will removed and this may cause permanent loss of data. Do you wish to proceed?");
            
            if (canProceed)
            {
                await _removableDriveCleaner.ClearAsync(SelectedDrive.RootPath);
            }
        }

        if (canProceed)
        {
            try
            {
                IsCreatingPackage = true;

                ResetPackageSummary();

                PackageProgress = 0;
                PackageStatus = "Preparing package...";
                PackageDetail = string.Empty;

                var progress = new Progress<PackageCreationProgress>(update =>
                {
                    if (!IsCreatingPackage)
                    {
                        return;
                    }

                    CurrentStage = update.Stage;
                    PackageProgress = update.Percentage;
                    PackageStatus = update.Status;
                    PackageDetail = update.Detail ?? string.Empty;

                    UpdatePackageSummary(update);
                });

                await _model.CreatePackageAsync(progress);

                PackageProgress = 100;
                PackageStatus = "Package complete.";
                PackageDetail = string.Empty;
            }
            catch (OperationCanceledException)
            {
                IsCreatingPackage = false;
                PackageStatus = "Package creation cancelled.";
                PackageDetail = string.Empty;
            }
            catch (Exception ex)
            {
                IsCreatingPackage = false;
                PackageStatus = $"Package creation failed: {ex.Message}";
                PackageDetail = string.Empty;
            }
            finally
            {
                IsCreatingPackage = false;
                CreatePackageCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Resets the package stage summaries to their initial values.
    /// </summary>
    private void ResetPackageSummary()
    {
        DocumentsSummary = "—";
        ManifestSummary = "—";
        SplashScreenSummary = "—";
        CertificateSummary = "—";
        ValidationSummary = "—";
        UsbLabelSummary = "—";
    }

    /// <summary>
    /// Updates the completed-stage summary corresponding to a progress update.
    /// </summary>
    /// <param name="update">The package creation progress update.</param>
    private void UpdatePackageSummary(PackageCreationProgress update)
    {
        if (update.StageProgress < 1)
        {
            return;
        }

        switch (update.Stage)
        {
            case PackageCreationStage.CopyDocuments:
                DocumentsSummary = $"{DocumentCount} documents";
                break;

            case PackageCreationStage.GenerateManifest:
                ManifestSummary = "Created";
                break;

            case PackageCreationStage.GenerateSplashScreen:
                SplashScreenSummary = "Created";
                break;

            case PackageCreationStage.GenerateCertificate:
                CertificateSummary = "Created";
                break;

            case PackageCreationStage.ValidatePackage:
                ValidationSummary = "Passed";
                break;

            case PackageCreationStage.FinaliseUsb:
                UsbLabelSummary = $"{Order!.OrderNumber}-DOC";
                break;
        }
    }

    /// <summary>
    /// Determines whether package creation can currently begin.
    /// </summary>
    /// <returns><see langword="true" /> when a writable drive is selected and creation is idle.</returns>
    private bool CanCreatePackage()
    {
        return IsCreatingPackage == false &&
               _model.SelectedDrive is not null &&
               _model.SelectedDrive.WriteStatus == DriveWriteStatus.Writable;
    }

    /// <summary>
    /// Refreshes the collection of available removable drives.
    /// </summary>
    private void RefreshDrives()
    {
        var drives = _removableDriveService.GetAvailableDrives();

        AvailableDrives.Clear();

        foreach (var drive in drives)
        {
            AvailableDrives.Add(drive);
        }

        SelectedDrive = AvailableDrives.Count > 0 ? AvailableDrives[0] : null;
        OnPropertyChanged(nameof(DriveSummary));
    }

    #endregion
}
