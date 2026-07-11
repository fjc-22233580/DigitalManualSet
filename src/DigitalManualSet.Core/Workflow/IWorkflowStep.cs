namespace DigitalManualSet.Core.Workflow;


/// <summary>
/// Represents a single step within a workflow.
/// </summary>
/// <typeparam name="TStepId">
/// The type used to uniquely identify workflow steps.
/// </typeparam>
/// <remarks>
/// A workflow step contains the metadata, transition rules, and lifecycle hooks
/// for one stage of a workflow.
/// </remarks>
public interface IWorkflowStep<TStepId> where TStepId : notnull
{
    /// <summary>
    /// Gets the unique identifier for the workflow step.
    /// </summary>
    /// <remarks>
    /// This value should be stable within the workflow and can be used by the
    /// application layer to map the step to a screen, view model, or other
    /// presentation-specific behaviour.
    /// </remarks>
    TStepId Id { get; }

    /// <summary>
    /// Gets the display title for the workflow step.
    /// </summary>
    /// <remarks>
    /// This can be shown in the user interface, for example in a progress
    /// indicator, step header, or breadcrumb.
    /// </remarks>
    string Title { get; }

    /// <summary>
    /// Gets a value indicating whether the workflow can move forward from this step.
    /// </summary>
    /// <remarks>
    /// Individual steps can use this to prevent the user from continuing until
    /// the step is complete or valid.
    /// </remarks>
    bool CanMoveNext { get; }

    /// <summary>
    /// Gets a value indicating whether the workflow can move backwards from this step.
    /// </summary>
    /// <remarks>
    /// Most steps will allow backwards movement, but this can be disabled for
    /// stages where returning would be invalid or unsafe.
    /// </remarks>
    bool CanMoveBack { get; }

    /// <summary>
    /// Performs any logic required when the workflow enters this step.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This can be used to initialise step-specific data.
    /// </remarks>
    Task OnEnterAsync();

    /// <summary>
    /// Performs any logic required when the workflow exits this step.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This can be used to validate, persist, or finalise step-specific state
    /// before moving to another workflow step.
    /// </remarks>
    Task OnExitAsync();
}