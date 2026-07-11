using DigitalManualSet.Core.Workflow;

namespace DigitalManualSet.Tests.Core.Workflow;

/// <summary>
/// Unit tests for the <see cref="Workflow{TStepId}"/> implementation using dummy test steps.
/// </summary>
public class WorkflowTests
{
    /// <summary>
    /// Verifies the workflow constructor sets the first provided step as current.
    /// </summary>
    [Fact]
    public void Constructor_WhenStepsProvided_SetsFirstStepAsCurrent()
    {
        // Arrange
        var steps = CreateSteps();

        // Act
        var workflow = new Workflow<DummyWorkflowStepId>(steps);

        // Assert
        Assert.Equal(DummyWorkflowStepId.StepOne, workflow.CurrentStep.Id);
        Assert.Equal(0, workflow.CurrentIndex);
        Assert.True(workflow.IsFirstStep);
        Assert.False(workflow.IsLastStep);
    }

    /// <summary>
    /// Verifies that constructing a workflow with no steps throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Constructor_WhenStepsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var steps = Array.Empty<IWorkflowStep<DummyWorkflowStepId>>();

        // Act
        var exception = Assert.Throws<ArgumentException>(() => new Workflow<DummyWorkflowStepId>(steps));

        // Assert
        Assert.Contains("Workflow must contain at least one step", exception.Message);
    }

    /// <summary>
    /// Verifies moving to the next step advances the current index and updates flags.
    /// </summary>
    [Fact]
    public async Task MoveNextAsync_WhenAllowed_MovesToNextStep()
    {
        // Arrange
        var steps = CreateSteps();
        var workflow = new Workflow<DummyWorkflowStepId>(steps);

        // Act
        await workflow.MoveNextAsync();

        // Assert
        Assert.Equal(DummyWorkflowStepId.StepTwo, workflow.CurrentStep.Id);
        Assert.Equal(1, workflow.CurrentIndex);
        Assert.False(workflow.IsFirstStep);
        Assert.False(workflow.IsLastStep);
    }

    /// <summary>
    /// Verifies moving back restores the previous step as current.
    /// </summary>
    [Fact]
    public async Task MoveBackAsync_WhenAllowed_MovesToPreviousStep()
    {
        // Arrange
        var steps = CreateSteps();
        var workflow = new Workflow<DummyWorkflowStepId>(steps);

        await workflow.MoveNextAsync();

        // Act
        await workflow.MoveBackAsync();

        // Assert
        Assert.Equal(DummyWorkflowStepId.StepOne, workflow.CurrentStep.Id);
        Assert.Equal(0, workflow.CurrentIndex);
        Assert.True(workflow.IsFirstStep);
    }

    /// <summary>
    /// Verifies the workflow does not advance when the current step disallows moving next.
    /// </summary>
    [Fact]
    public async Task MoveNextAsync_WhenCurrentStepCannotMoveNext_DoesNotMove()
    {
        // Arrange
        var steps = new List<IWorkflowStep<DummyWorkflowStepId>>
        {
            new DummyWorkflowStep(
                DummyWorkflowStepId.StepOne,
                "Step One",
                canMoveNext: false),

            new DummyWorkflowStep(
                DummyWorkflowStepId.StepTwo,
                "Step Two")
        };

        var workflow = new Workflow<DummyWorkflowStepId>(steps);

        // Act
        await workflow.MoveNextAsync();

        // Assert
        Assert.Equal(DummyWorkflowStepId.StepOne, workflow.CurrentStep.Id);
        Assert.Equal(0, workflow.CurrentIndex);
        Assert.True(workflow.IsFirstStep);
    }

    /// <summary>
    /// Verifies that when moving next the new step's OnEnterAsync is invoked.
    /// </summary>
    [Fact]
    public async Task MoveNextAsync_WhenAllowed_CallsOnEnterAsyncOnNewStep()
    {
        // Arrange
        var stepOne = new DummyWorkflowStep(DummyWorkflowStepId.StepOne, "Step One");
        var stepTwo = new DummyWorkflowStep(DummyWorkflowStepId.StepTwo, "Step Two");

        var workflow = new Workflow<DummyWorkflowStepId>(
        [
            stepOne,
            stepTwo
        ]);

        // Act
        await workflow.MoveNextAsync();

        // Assert
        Assert.False(stepOne.Entered);
        Assert.True(stepTwo.Entered);
    }

    /// <summary>
    /// Verifies that when moving next the previous step's OnExitAsync is invoked.
    /// </summary>
    [Fact]
    public async Task MoveNextAsync_WhenAllowed_CallsOnExitAsyncOnPreviousStep()
    {
        // Arrange
        var stepOne = new DummyWorkflowStep(DummyWorkflowStepId.StepOne, "Step One");
        var stepTwo = new DummyWorkflowStep(DummyWorkflowStepId.StepTwo, "Step Two");

        var workflow = new Workflow<DummyWorkflowStepId>(
        [
            stepOne,
            stepTwo
        ]);

        // Act
        await workflow.MoveNextAsync();

        // Assert
        Assert.True(stepOne.Exited);
        Assert.False(stepTwo.Exited);
    }



    /// <summary>
    /// Helper that constructs a list of three dummy steps used by multiple tests.
    /// </summary>
    private static List<IWorkflowStep<DummyWorkflowStepId>> CreateSteps()
    {
        return
        [
            new DummyWorkflowStep(DummyWorkflowStepId.StepOne, "Step One"),
            new DummyWorkflowStep(DummyWorkflowStepId.StepTwo, "Step Two"),
            new DummyWorkflowStep(DummyWorkflowStepId.StepThree, "Step Three")
        ];
    }
}
