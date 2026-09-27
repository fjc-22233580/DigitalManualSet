using DigitalManualSet.App.Common;
using DigitalManualSet.App.Services;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;
using DigitalManualSet.Core.Orders;
using DigitalManualSet.Core.PackageCreation;
using DigitalManualSet.Core.PackageCreation.Workflow;
using DigitalManualSet.Core.Workflow;

namespace DigitalManualSet.App.ViewModels.CreatePackage;

/// <summary>
/// Placeholder for new package view/VM
/// </summary>
/// <seealso cref="DigitalManualSet.App.Common.ViewModel" />
/// <seealso cref="DigitalManualSet.App.ViewModels.IScreenViewModel" />
/// <summary>
/// View model that drives the "Create Package" multi-step workflow.
/// Exposes the current step, navigation commands and progress information used by the UI.
/// </summary>
public class CreatePackageViewModel : ViewModel, IScreenViewModel
{
    #region Fields

    private readonly IPackageWorkflowStepViewModelResolver _stepViewModelResolver;
    private readonly Workflow<PackageWorkflowStepId> _workflow;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePackageViewModel" /> class.
    /// </summary>
    /// <param name="stepViewModelResolver">Resolver used to obtain the concrete view-model for the current workflow step.</param>
    /// <param name="openOrderProvider">The open order provider.</param>
    public CreatePackageViewModel(IPackageWorkflowStepViewModelResolver stepViewModelResolver, IOpenOrderProvider openOrderProvider)
    {
        _stepViewModelResolver = stepViewModelResolver;
        _workflow = PackageWorkflowFactory.Create(openOrderProvider);

        NextCommand = new RelayCommand(async () => await MoveNextAsync(), () => CanMoveNext);
        BackCommand = new RelayCommand(async () => await MoveBackAsync(), () => CanMoveBack);

        // Fire-and-forget first step.
        _ = InitializeCurrentStepAsync();

        _currentStepViewModel = _stepViewModelResolver.Resolve(_workflow.CurrentStep);

        _workflow.CurrentStep.NavigationStateChanged += OnNavigationStateChanged;
    }

    private void OnNavigationStateChanged(object? sender, EventArgs e)
    {
        NextCommand.RaiseCanExecuteChanged();
        BackCommand.RaiseCanExecuteChanged();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the display title for the current workflow step.
    /// </summary>
    public string CurrentStepTitle => _workflow.CurrentStep.Title;

    /// <summary>
    /// Gets the identifier for the current workflow step.
    /// </summary>
    public PackageWorkflowStepId CurrentStepId => _workflow.CurrentStep.Id;

    /// <summary>
    /// Gets a short information string describing the current step index and total steps.
    /// Example: "Step 2 of 4".
    /// </summary>
    public string StepInfo => $"Step {_workflow.CurrentIndex + 1} of {_workflow.Steps.Count}";

    /// <summary>
    /// Gets a value indicating whether the workflow can move forward from the current step.
    /// </summary>
    public bool CanMoveNext => _workflow.CanMoveNext;

    /// <summary>
    /// Gets a value indicating whether the workflow can move backward from the current step.
    /// </summary>
    public bool CanMoveBack => _workflow.CanMoveBack;

    #endregion

    #region Commands

    /// <summary>
    /// Command used to advance the workflow to the next step.
    /// </summary>
    public RelayCommand NextCommand { get; }

    /// <summary>
    /// Command used to move the workflow to the previous step.
    /// </summary>
    public RelayCommand BackCommand { get; }

    #endregion

    #region Step ViewModel

    /// <summary>
    /// Gets the view-model interface for the currently active workflow step. The
    /// resolver is used to obtain the concrete implementation for the UI.
    /// </summary>
    private IPackageWorkflowStepViewModel _currentStepViewModel;

    public IPackageWorkflowStepViewModel CurrentStepViewModel
    {
        get => _currentStepViewModel;
        private set
        {
            _currentStepViewModel = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the overall progress through the workflow as a percentage (0-100).
    /// </summary>
    public double ProgressPercentage
    {
        get
        {
            if (_workflow.Steps.Count == 0)
            {
                return 0;
            }

            var currentStepNumber = _workflow.CurrentIndex + 1;
            return (double)currentStepNumber / _workflow.Steps.Count * 100;
        }
    }

    #endregion

    #region Methods

    private async Task InitializeCurrentStepAsync()
    {
        try
        {
            await _workflow.CurrentStep.OnEnterAsync();
            RefreshWorkflowProperties();
        }
        catch (Exception ex)
        {
            // Log or handle initialization errors so exceptions don't get silently swallowed.
            // e.g. Log.Error(ex, "Failed to initialize workflow step.");
        }
    }

    /// <summary>
    /// Moves the workflow to the next step and refreshes UI-bound properties.
    /// </summary>
    private async Task MoveNextAsync()
    {
        var previousStep = _workflow.CurrentStep;
        previousStep.NavigationStateChanged -= OnNavigationStateChanged;

        await _workflow.MoveNextAsync();

        _workflow.CurrentStep.NavigationStateChanged += OnNavigationStateChanged;

        RefreshWorkflowProperties();
    }

    /// <summary>
    /// Moves the workflow to the previous step and refreshes UI-bound properties.
    /// </summary>
    private async Task MoveBackAsync()
    {

        var previousStep = _workflow.CurrentStep;
        previousStep.NavigationStateChanged -= OnNavigationStateChanged;

        await _workflow.MoveBackAsync();

        _workflow.CurrentStep.NavigationStateChanged += OnNavigationStateChanged;
        RefreshWorkflowProperties();
    }

    /// <summary>
    /// Raises property change notifications for properties that depend on the current step,
    /// and updates the commands' CanExecute state.
    /// </summary>
    private void RefreshWorkflowProperties()
    {
        CurrentStepViewModel = _stepViewModelResolver.Resolve(_workflow.CurrentStep);

        OnPropertyChanged(nameof(CurrentStepViewModel));
        OnPropertyChanged(nameof(CurrentStepTitle));
        OnPropertyChanged(nameof(CurrentStepId));
        OnPropertyChanged(nameof(StepInfo));
        OnPropertyChanged(nameof(CanMoveNext));
        OnPropertyChanged(nameof(CanMoveBack));
        OnPropertyChanged(nameof(ProgressPercentage));

        NextCommand.RaiseCanExecuteChanged();
        BackCommand.RaiseCanExecuteChanged();
    }

    #endregion

}
