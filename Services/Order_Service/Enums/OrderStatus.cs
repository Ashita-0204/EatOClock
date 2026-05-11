using System.Text.Json.Serialization;

namespace Order_Service.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    PLACED = 1,
    CONFIRMED = 2,
    PREPARING = 3,
    PICKED_UP = 4,
    DELIVERED = 5,
    CANCELLED = 6,
    READY_FOR_PICKUP = 7
}
