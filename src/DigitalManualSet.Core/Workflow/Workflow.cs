namespace DigitalManualSet.Core.Workflow;

/// <summary>
/// Coordinates movement through a sequence of workflow steps.
/// </summary>
/// <remarks>
/// The workflow owns an ordered collection of <see cref="Workflow{TStepId}"/> instances
/// and tracks which step is currently active. It contains workflow
/// movement logic, while individual steps define their own transition rules and
/// lifecycle behaviour.
/// </remarks>
public sealed class Workflow<TStepId> where TStepId : notnull
{
    private readonly List<IWorkflowStep<TStepId>> _steps;
    private int _currentIndex;

    /// <summary>
    /// Initialises a new instance of the <see cref="Workflow"/> class.
    /// </summary>
    /// <param name="steps">
    /// The ordered collection of steps that make up the workflow.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="steps"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="steps"/> does not contain any workflow steps.
    /// </exception>
    public Workflow(IEnumerable<IWorkflowStep<TStepId>> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);

        _steps = steps.ToList();

        if (_steps.Count == 0)
        {
            throw new ArgumentException("Workflow must contain at least one step.", nameof(steps));
        }
    }

    /// <summary>
    /// Gets the step that is currently active in the workflow.
    /// </summary>
    public IWorkflowStep<TStepId> CurrentStep => _steps[_currentIndex];

    /// <summary>
    /// Gets all steps in the workflow in their configured order.
    /// </summary>
    public IReadOnlyList<IWorkflowStep<TStepId>> Steps => _steps;

    /// <summary>
    /// Gets the zero-based index of the current workflow step.
    /// </summary>
    public int CurrentIndex => _currentIndex;

    /// <summary>
    /// Gets a value indicating whether the current step is the first step.
    /// </summary>
    public bool IsFirstStep => _currentIndex == 0;

    /// <summary>
    /// Gets a value indicating whether the current step is the last step.
    /// </summary>
    public bool IsLastStep => _currentIndex == _steps.Count - 1;

    /// <summary>
    /// Gets a value indicating whether the workflow can move to the next step.
    /// </summary>
    /// <remarks>
    /// This checks both the workflow position and the transition rule defined by
    /// the current step.
    /// </remarks>
    public bool CanMoveNext => !IsLastStep && CurrentStep.CanMoveNext;

    /// <summary>
    /// Gets a value indicating whether the workflow can move to the previous step.
    /// </summary>
    /// <remarks>
    /// This checks both the workflow position and the transition rule defined by
    /// the current step.
    /// </remarks>
    public bool CanMoveBack => !IsFirstStep && CurrentStep.CanMoveBack;

    /// <summary>
    /// Moves the workflow to the next step, if movement is currently allowed.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// If the workflow is already on the final step, or the current step does not
    /// allow forward movement, no transition will occur.
    /// </remarks>
    public async Task MoveNextAsync()
    {
        if (!CanMoveNext)
        {
            return;
        }

        await CurrentStep.OnExitAsync();

        _currentIndex++;

        await CurrentStep.OnEnterAsync();
    }

    /// <summary>
    /// Moves the workflow to the previous step, if movement is currently allowed.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// If the workflow is already on the first step, or the current step does not
    /// allow backwards movement, no transition will occur.
    /// </remarks>
    public async Task MoveBackAsync()
    {
        if (!CanMoveBack)
        {
            return;
        }

        await CurrentStep.OnExitAsync();

        _currentIndex--;

        await CurrentStep.OnEnterAsync();
    }

    /// <summary>
    /// Moves the workflow directly to the step with the specified identifier.
    /// </summary>
    /// <param name="stepId">
    /// The unique identifier of the step to move to.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="stepId"/> is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no workflow step exists with the specified identifier.
    /// </exception>
    /// <remarks>
    /// This is useful for controlled non-linear navigation, such as returning to
    /// a known earlier step from a summary screen. It should be used carefully,
    /// because it bypasses normal next/back movement.
    /// </remarks>
    public async Task MoveToAsync(TStepId stepId)
    {

        int targetIndex = -1;

        for (int index = 0; index < _steps.Count; index++)
        {
            if (EqualityComparer<TStepId>.Default.Equals(_steps[index].Id, stepId))
            {
                targetIndex = index;
                break;
            }
        }

        if (targetIndex < 0)
        {
            throw new InvalidOperationException($"Workflow step '{stepId}' was not found.");
        }

        if (targetIndex == _currentIndex)
        {
            return;
        }

        await CurrentStep.OnExitAsync();

        _currentIndex = targetIndex;

        await CurrentStep.OnEnterAsync();
    }
}