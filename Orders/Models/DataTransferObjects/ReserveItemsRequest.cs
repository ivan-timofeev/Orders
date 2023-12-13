using System.ComponentModel.DataAnnotations;

namespace Orders.Models.DataTransferObjects;

public record ReserveItemsRequest
(
    Guid TransactionalId,
    Guid OrderId,
    IEnumerable<RequestedItemDto> RequestedItems
);