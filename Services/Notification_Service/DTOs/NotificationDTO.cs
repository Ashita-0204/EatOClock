using Notification_Service.Enums;

namespace Notification_Service.DTOs;

public class NotificationDTO
{
    public Guid NotificationId { get; set; }
    public string RecipientId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? OrderId { get; set; }
}
