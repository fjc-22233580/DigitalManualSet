using DigitalManualSet.Core.Orders;
using DigitalManualSet.Core.Workflow;

namespace DigitalManualSet.Core.PackageCreation.Workflow;

/// <summary>
/// Workflow step representing the "Select Order" stage of the package creation process.
/// </summary>
public sealed class CreateOrderStep : WorkflowStep<PackageWorkflowStepId>
{
    private readonly IOpenOrderProvider _openOrderProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateOrderStep" /> class.
    /// </summary>
    public CreateOrderStep(Package package, IOpenOrderProvider openOrderProvider)
        : base(PackageWorkflowStepId.CreateOrder, "Select Order", package)
    {
        _openOrderProvider = openOrderProvider;
    }

    public IReadOnlyList<Order> AvailableOrders { get; private set; } = [];

    /// <summary>
    /// Applies the selected order to the package.
    /// </summary>
    /// <param name="order">The confirmed order.</param>
    public void SetOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        Package.Order = order;
        OnNavigationStateChanged();
    }

    public override async Task OnEnterAsync()
    {
        AvailableOrders =
            await _openOrderProvider.GetOpenOrdersAsync();

        ClearOrder();
    }


    public override bool CanMoveNext => Package.Order is not null;

    public void ClearOrder()
    {
        Package.Order = null;

        OnNavigationStateChanged();
    }
}
