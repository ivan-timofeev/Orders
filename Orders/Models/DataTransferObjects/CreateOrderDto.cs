using System.ComponentModel.DataAnnotations;

namespace Orders.Models.DataTransferObjects;

public record CreateOrderDto
(
   [Required]
   Guid CustomerId,

   [Required, MinLength(1), MaxLength(10)]
   IEnumerable<RequestedItemDto> RequestedItems
);
