namespace Order_Service.Models;

public enum OrderStatus
{
    PLACED = 1,
    CONFIRMED = 2,
    PREPARING = 3,
    PICKED_UP = 4,
    DELIVERED = 5,
    CANCELLED = 6
}
