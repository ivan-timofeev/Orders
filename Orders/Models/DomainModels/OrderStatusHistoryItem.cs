namespace Orders.Models.DomainModels;

public class OrderStatusHistoryItem
{
    public Guid Id { get; set; }
    public Order Order { get; set; }
    public required OrderStatusEnum OrderStatus { get; set; }
    public required DateTime EnterToStatusDateTimeUtc { get; set; }
    public string? Details { get; set; }

    public OrderStatusHistoryItem()
    {
        Order = null!;
    }
}