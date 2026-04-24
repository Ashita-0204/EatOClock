using Notification_Service.Enums;

namespace Notification_Service.Models;

public class Notification
{
    public Guid NotificationId { get; set; } = Guid.NewGuid();
    public string RecipientId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? OrderId { get; set; }
    public string? MetaData { get; set; }
}
