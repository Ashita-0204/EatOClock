using Notification_Service.Enums;

namespace Notification_Service.DTOs;
public class SendOrderNotificationDTO
{
    public string RecipientId { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
