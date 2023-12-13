namespace Orders.Models.DomainModels;

public class Order
{
    public Guid Id { get; set; }
    public required Guid CustomerId { get; set; }
    public required IList<OrderStatusHistoryItem> OrderStatusHistory { get; set; }
    public required IList<RequestedItem> RequestedItems { get; set; }

    public Order()
    {
        OrderStatusHistory = new List<OrderStatusHistoryItem>();
        RequestedItems = new List<RequestedItem>();
    }
}

public class RequestedItem
{
    public Guid Id { get; set; }
    
    public required Guid ItemId { get; set; }
    public required int Quantity { get; set; }
}
