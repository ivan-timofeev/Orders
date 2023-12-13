namespace Orders.Models.DataTransferObjects;

public class ReserveItemsResponse
{
    public required Guid TransactionalId { get; init; }
    public required Guid OrderId { get; init; }
    public required ReserveItemsResponseStatusEnum Status { get; init; }
    public string? Message { get; init; }
}

public enum ReserveItemsResponseStatusEnum
{
    Success,
    Error
}
