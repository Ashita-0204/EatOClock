using Notification_Service.DTOs;

namespace Notification_Service.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDTO>> GetNotificationsAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(Guid notificationId, string userId);
    Task MarkAllReadAsync(string userId);
    Task DeleteAsync(Guid notificationId, string userId);
    Task BroadcastAsync(BroadcastDTO dto);
    Task SendOrderNotificationAsync(SendOrderNotificationDTO dto);
    Task SendNotificationAsync(CreateNotificationDTO dto);
    Task SendRestaurantAlertAsync(RestaurantNewOrderDTO dto);
}

public interface IEmailService
{
    Task SendAsync(SendEmailDTO dto);
}

public interface ISmsService
{
    Task SendAsync(SendSmsDTO dto);
}