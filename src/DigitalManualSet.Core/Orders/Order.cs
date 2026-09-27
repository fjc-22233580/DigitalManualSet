namespace DigitalManualSet.Core.Orders;

public class Order
{
    public string OrderNumber { get; }
    public string CustomerName { get; }
    public string SystemId { get; }

    public Order(string orderNumber, string customerName ,string systemId)
    {
        OrderNumber = orderNumber;
        CustomerName = customerName;
        SystemId = systemId;
    }
}