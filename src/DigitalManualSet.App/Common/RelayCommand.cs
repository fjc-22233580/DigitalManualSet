using System.Windows.Input;

namespace DigitalManualSet.App.Common;

/// <summary>
/// Provides a reusable implementation of <see cref="ICommand"/> for binding UI actions to view model methods.
/// </summary>
/// <remarks>
/// This command is intended for simple synchronous actions. It accepts an execute delegate and an optional
/// can-execute delegate. When the command state changes, call <see cref="RaiseCanExecuteChanged"/> so WPF
/// can re-query whether the command is currently available.
/// </remarks>
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    /// <summary>
    /// Initialises a new instance of the <see cref="RelayCommand"/> class using a parameterless action.
    /// </summary>
    /// <param name="execute">The action to execute when the command is invoked.</param>
    /// <param name="canExecute">An optional function that determines whether the command can execute.</param>
    public RelayCommand(
        Action execute,
        Func<bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);

        _execute = _ => execute();
        _canExecute = canExecute is null
            ? null
            : _ => canExecute();
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="RelayCommand"/> class.
    /// </summary>
    /// <param name="execute">The action to execute when the command is invoked.</param>
    /// <param name="canExecute">
    /// An optional predicate that determines whether the command can currently execute.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="execute"/> is <see langword="null"/>.
    /// </exception>
    public RelayCommand(
        Action<object?> execute,
        Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Occurs when changes affect whether the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Determines whether the command can execute using the supplied parameter.
    /// </summary>
    /// <param name="parameter">The command parameter supplied by the binding.</param>
    /// <returns>
    /// <see langword="true"/> if the command can execute; otherwise, <see langword="false"/>.
    /// </returns>
    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    /// <summary>
    /// Executes the command using the supplied parameter.
    /// </summary>
    /// <param name="parameter">The command parameter supplied by the binding.</param>
    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    /// <summary>
    /// Raises <see cref="CanExecuteChanged"/> to notify WPF that command availability may have changed.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}