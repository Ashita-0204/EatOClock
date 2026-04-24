using Notification_Service.Enums;

namespace Notification_Service.DTOs;
public class RestaurantNewOrderDTO
{
    public string OwnerId { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? OwnerEmail { get; set; }
    public string? OwnerPhone { get; set; }
}