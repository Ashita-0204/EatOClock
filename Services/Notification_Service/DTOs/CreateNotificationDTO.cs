using Notification_Service.Enums;

namespace Notification_Service.DTOs;
public class CreateNotificationDTO
{
    public string RecipientId { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.SYSTEM;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
