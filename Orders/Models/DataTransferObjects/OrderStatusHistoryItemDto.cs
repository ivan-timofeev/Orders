using System.Text.Json.Serialization;
using Orders.Models.DomainModels;

namespace Orders.Models.DataTransferObjects;

public class OrderStatusHistoryItemDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required OrderStatusEnum OrderStatus { get; init; }

    public required DateTime EnterToStatusDateTimeUtc { get; init; }

    public string? Details { get; init; }
}
