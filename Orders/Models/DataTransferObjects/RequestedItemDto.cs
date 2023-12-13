using System.ComponentModel.DataAnnotations;

namespace Orders.Models.DataTransferObjects;

public sealed record RequestedItemDto
(
    [Required]
    Guid ItemId,
    
    [Range(1, 100)]
    int RequestedQuantity
);