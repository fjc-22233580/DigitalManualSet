using DigitalManualSet.Core.Workflow;

namespace DigitalManualSet.Tests.Core.Workflow;

/// <summary>
/// Test helper workflow step that exposes configurable CanMoveNext/CanMoveBack
/// values and records whether <see cref="OnEnterAsync"/> and <see cref="OnExitAsync"/>
/// have been invoked. Used by workflow unit tests to verify step transitions.
/// </summary>
public class DummyWorkflowStep : WorkflowStep<DummyWorkflowStepId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DummyWorkflowStep"/> class.
    /// </summary>
    /// <param name="id">The step id.</param>
    /// <param name="title">The display title for the step.</param>
    /// <param name="canMoveNext">If false, this step will prevent moving to the next step.</param>
    /// <param name="canMoveBack">If false, this step will prevent moving to the previous step.</param>
    public DummyWorkflowStep(DummyWorkflowStepId id, 
        string title, 
        bool canMoveNext = true,
        bool canMoveBack = true) : base(id, title)
    {
        CanMoveNextValue = canMoveNext;
        CanMoveBackValue = canMoveBack;
    }

    /// <summary>
    /// True when <see cref="OnEnterAsync"/> has been called for this step.
    /// </summary>
    public bool Entered { get; private set; }

    /// <summary>
    /// True when <see cref="OnExitAsync"/> has been called for this step.
    /// </summary>
    public bool Exited { get; private set; }

    /// <summary>
    /// Backing value used to control the <see cref="CanMoveNext"/> behavior in tests.
    /// </summary>
    public bool CanMoveNextValue { get; set; }

    /// <summary>
    /// Backing value used to control the <see cref="CanMoveBack"/> behavior in tests.
    /// </summary>
    public bool CanMoveBackValue { get; set; }

    /// <inheritdoc />
    public override bool CanMoveNext => CanMoveNextValue;

    /// <inheritdoc />
    public override bool CanMoveBack => CanMoveBackValue;

    /// <inheritdoc />
    public override Task OnEnterAsync()
    {
        Entered = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task OnExitAsync()
    {
        Exited = true;
        return Task.CompletedTask;
    }
}
