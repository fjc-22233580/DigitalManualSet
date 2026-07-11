namespace DigitalManualSet.Core.Workflow;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TStepId">The type of the step identifier.</typeparam>
/// <seealso cref="DigitalManualSet.Core.Workflow.IWorkflowStep&lt;TStepId&gt;" />
public abstract class WorkflowStep<TStepId> : IWorkflowStep<TStepId> where TStepId : notnull
{
    /// <summary>
    /// Initialises a new instance of the <see cref="WorkflowStep{TStepId}"/> class.
    /// </summary>
    /// <param name="id">
    /// The unique identifier for the workflow step.
    /// </param>
    /// <param name="title">
    /// The display title for the workflow step.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="id"/> or <paramref name="title"/> is null,
    /// empty, or whitespace.
    /// </exception>
    protected WorkflowStep(TStepId id, string title)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Workflow step title cannot be empty.", nameof(title));
        }

        Id = id;
        Title = title;
    }

    /// <inheritdoc />
    public TStepId Id { get; }

    /// <inheritdoc />
    public string Title { get; }

    /// <inheritdoc />
    public virtual bool CanMoveNext => true;

    /// <inheritdoc />
    public virtual bool CanMoveBack => true;

    /// <inheritdoc />
    public virtual Task OnEnterAsync()
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task OnExitAsync()
    {
        return Task.CompletedTask;
    }
}