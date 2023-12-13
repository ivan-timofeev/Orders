namespace Orders.Models.DataTransferObjects;

public class OrderDto
{
    public Guid Id { get; set; }
    public required Guid CustomerId { get; set; }
    public required IEnumerable<OrderStatusHistoryItemDto> OrderStatusHistory { get; set; }
    public required IEnumerable<RequestedItemDto> RequestedItems { get; set; }
}