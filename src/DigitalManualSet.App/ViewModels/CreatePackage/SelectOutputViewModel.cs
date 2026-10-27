using DigitalManualSet.App.Common;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;
using DigitalManualSet.Core.Models;
using DigitalManualSet.Core.PackageCreation.Workflow;
using DigitalManualSet.Core.Services;
using DigitalManualSet.Core.Workflow;
using System.Collections.ObjectModel;

namespace DigitalManualSet.App.ViewModels.CreatePackage;

/// <summary>
/// View model for selecting a removable output drive and copying the package documents to it.
/// </summary>
/// <seealso cref="DigitalManualSet.App.Common.ViewModel" />
/// <seealso cref="DigitalManualSet.App.ViewModels.CreatePackage.Interfaces.IPackageWorkflowStepViewModel" />
public class SelectOutputViewModel : ViewModel, IPackageWorkflowStepViewModel
{
    #region Fields

    private readonly SelectOutputStep _Model;
    private readonly IRemovableDriveService _removableDriveService;
    private RemovableDrive? _selectedDrive;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectOutputViewModel" /> class.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <param name="removableDriveService">The removable drive service.</param>
    /// <exception cref="System.ArgumentException">Expected a {nameof(CreateOrderStep)}. - step</exception>
    public SelectOutputViewModel(SelectOutputStep step, IRemovableDriveService removableDriveService)
    {
        _Model = step ?? throw new ArgumentException($"Expected a {nameof(CreateOrderStep)}.", nameof(step));
        _removableDriveService = removableDriveService;

        RefreshDrives();

        _selectedDrive = _Model.SelectedDrive;

        RefreshDrivesCommand = new RelayCommand(RefreshDrives);
        CopyDocumentsCommand = new AsyncRelayCommand(CopyDocumentsAsync, CanCopyDocuments);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the removable drives currently available for package output.
    /// </summary>
    public ObservableCollection<RemovableDrive> AvailableDrives { get; }= [];

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
                _Model.SelectedDrive = value;
                CopyDocumentsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets the command that refreshes the available removable drives.
    /// </summary>
    public RelayCommand RefreshDrivesCommand { get; }

    /// <summary>
    /// Gets the command that copies the package documents to the selected drive.
    /// </summary>
    public AsyncRelayCommand CopyDocumentsCommand { get; }

    /// <summary>
    /// Gets the display title for the workflow step.
    /// </summary>
    public string Title => "Placeholder: select USB drive.";

    /// <summary>
    /// Gets or sets a value indicating whether documents are currently being copied.
    /// </summary>
    public bool IsCopying
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets or sets the copy progress as a percentage.
    /// </summary>
    public double CopyProgress
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets or sets the current status message for the copy operation.
    /// </summary>
    public string CopyStatus
    {
        get;
        set => SetProperty(ref field, value);
    } 

    #endregion

    #region Methods

    /// <summary>
    /// Copies the package documents to the selected removable drive and updates progress.
    /// </summary>
    private async Task CopyDocumentsAsync()
    {
        try
        {
            IsCopying = true;
            CopyProgress = 0;
            CopyStatus = "Preparing USB...";

            var progress = new Progress<PackageWriteProgress>(update =>
            {
                if (!IsCopying)
                {
                    return;
                }

                CopyProgress = update.Percentage;
                CopyStatus = update.Status;
            });

            await _Model.CopyDocumentsAsync(progress);

            CopyProgress = 100;
            CopyStatus = "Documents copied successfully.";
        }
        catch (Exception ex)
        {
            IsCopying = false;
            CopyStatus = $"Copy failed: {ex.Message}";
        }
        finally
        {
            IsCopying = false;
            CopyDocumentsCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Determines whether the package documents can currently be copied.
    /// </summary>
    /// <returns><see langword="true"/> when a writable drive is selected and copying is not in progress.</returns>
    private bool CanCopyDocuments()
    {
        return _Model.IsComplete == false &&
               IsCopying == false &&
               SelectedDrive is not null &&
               SelectedDrive.WriteStatus == DriveWriteStatus.Writable;
    }

    /// <summary>
    /// Refreshes the collection of available removable drives.
    /// </summary>
    private void RefreshDrives()
    {
        CopyStatus = "Pending transfer";

        var drives = _removableDriveService.GetAvailableDrives();

        AvailableDrives.Clear();

        foreach (var drive in drives)
        {
            AvailableDrives.Add(drive);
        }
    }

    #endregion
}
