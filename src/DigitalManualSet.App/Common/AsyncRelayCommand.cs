using System.Windows.Input;

namespace DigitalManualSet.App.Common;

/// <summary>
/// Provides a reusable asynchronous implementation of <see cref="ICommand"/>
/// for binding UI actions to asynchronous view model methods.
/// </summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;

    private bool _isExecuting;

    /// <summary>
    /// Initialises a new instance of the <see cref="AsyncRelayCommand"/> class.
    /// </summary>
    /// <param name="execute">The asynchronous operation to execute.</param>
    public AsyncRelayCommand(Func<Task> execute)
    {
        ArgumentNullException.ThrowIfNull(execute);

        _execute = execute;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="AsyncRelayCommand"/> class
    /// with a function that determines whether the command can execute.
    /// </summary>
    /// <param name="execute">The asynchronous operation to execute.</param>
    /// <param name="canExecute">
    /// A function that determines whether the command can currently execute.
    /// </param>
    public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);

        _execute = execute;
        _canExecute = canExecute;
    }

    /// <summary>
    /// Occurs when changes affect whether the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Determines whether the command can currently execute.
    /// </summary>
    /// <param name="parameter">The command parameter supplied by the binding.</param>
    /// <returns>
    /// <see langword="true"/> if the command can execute; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool CanExecute(object? parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke() ?? true);
    }

    /// <summary>
    /// Executes the command asynchronously.
    /// </summary>
    /// <param name="parameter">The command parameter supplied by the binding.</param>
    public async void Execute(object? parameter)
    {
        await ExecuteAsync();
    }

    /// <summary>
    /// Executes the asynchronous operation.
    /// </summary>
    public async Task ExecuteAsync()
    {
        if (!CanExecute(null))
        {
            return;
        }

        try
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();

            await _execute();
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Raises <see cref="CanExecuteChanged"/> to notify WPF that command
    /// availability may have changed.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}