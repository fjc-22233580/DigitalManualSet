using DigitalManualSet.Core.Orders;
using DigitalManualSet.Core.Workflow;
using Microsoft.Extensions.Logging;

namespace DigitalManualSet.Core.PackageCreation.Workflow;

/// <summary>
/// Workflow step representing the "Select Order" stage of the package creation process.
/// </summary>
public sealed class CreateOrderStep : WorkflowStep<PackageWorkflowStepId>
{
    private readonly IOpenOrderProvider _openOrderProvider;
    private readonly ILogger<CreateOrderStep> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateOrderStep" /> class.
    /// </summary>
    public CreateOrderStep(Package package, IOpenOrderProvider openOrderProvider, ILogger<CreateOrderStep> logger)
        : base(PackageWorkflowStepId.CreateOrder, "Select Order", package)
    {
        _openOrderProvider = openOrderProvider;
        _logger = logger;
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

    /// <summary>
    /// Logs the confirmed order. As this is the first step, exiting always means moving forward.
    /// </summary>
    public override Task OnExitAsync()
    {
        _logger.LogInformation("Package started with {OrderNumber}", Package.Order?.OrderNumber);

        return Task.CompletedTask;
    }


    public override bool CanMoveNext => Package.Order is not null;

    public void ClearOrder()
    {
        Package.Order = null;

        OnNavigationStateChanged();
    }
}
