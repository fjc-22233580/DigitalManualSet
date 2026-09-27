namespace DigitalManualSet.Core.Orders;

/// <summary>
/// Provides the collection of orders currently available for selection.
/// </summary>
public interface IOpenOrderProvider
{
    /// <summary>
    /// Retrieves the currently available open orders.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A read-only collection of open orders.
    /// </returns>
    Task<IReadOnlyList<Order>> GetOpenOrdersAsync(CancellationToken cancellationToken = default);
}