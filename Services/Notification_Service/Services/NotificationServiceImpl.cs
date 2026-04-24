using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Notification_Service.Data;
using Notification_Service.DTOs;
using Notification_Service.Enums;
using Notification_Service.Hubs;
using Notification_Service.Interfaces;
using Notification_Service.Models;

namespace Notification_Service.Services;

public class NotificationServiceImpl : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly IEmailService _email;
    private readonly ISmsService _sms;

    public NotificationServiceImpl(AppDbContext db, IHubContext<NotificationHub> hub,
        IEmailService email, ISmsService sms)
    {
        _db    = db;
        _hub   = hub;
        _email = email;
        _sms   = sms;
    }

    // -- UC-57: Read notifications --------------------------------------------
    public async Task<List<NotificationDTO>> GetNotificationsAsync(string userId) =>
        await _db.Notifications
            .Where(n => n.RecipientId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => ToDto(n))
            .ToListAsync();

    // -- UC-63: Unread badge count --------------------------------------------
    public async Task<int> GetUnreadCountAsync(string userId) =>
        await _db.Notifications.CountAsync(n => n.RecipientId == userId && !n.IsRead);

    // -- UC-64: Mark single as read -------------------------------------------
    public async Task MarkAsReadAsync(Guid notificationId, string userId)
    {
        var n = await _db.Notifications
            .FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.RecipientId == userId);
        if (n is null) return;
        n.IsRead = true;
        await _db.SaveChangesAsync();
    }

    // -- UC-64: Mark all as read ----------------------------------------------
    public async Task MarkAllReadAsync(string userId) =>
        await _db.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

    public async Task DeleteAsync(Guid notificationId, string userId) =>
        await _db.Notifications
            .Where(n => n.NotificationId == notificationId && n.RecipientId == userId)
            .ExecuteDeleteAsync();

    // -- UC-62: Admin broadcast (in-app only, no DB persist) -----------------
    public async Task BroadcastAsync(BroadcastDTO dto) =>
        await _hub.Clients.All.SendAsync("ReceiveNotification", new
        {
            title     = dto.Title,
            message   = dto.Message,
            type      = "SYSTEM",
            createdAt = DateTime.UtcNow
        });

    // -- UC-57/58/59/60: Order status notification (all channels) -------------
    public async Task SendOrderNotificationAsync(SendOrderNotificationDTO dto)
    {
        var (title, message) = GetOrderMessage(dto.OrderStatus);
        var notification = await PersistAndPush(dto.RecipientId, NotificationType.ORDER, title, message, dto.OrderId);

        FireEmail(dto.Email, dto.RecipientId, title,
            $"<p>{message}</p><p><strong>Order ID:</strong> {dto.OrderId}</p>");

        FireSms(dto.Phone, $"EatOClock: {title} - {message}");
    }

    // -- Generic send (in-app + optional email/SMS) ---------------------------
    public async Task SendNotificationAsync(CreateNotificationDTO dto)
    {
        var notification = await PersistAndPush(
            dto.RecipientId, dto.Type, dto.Title, dto.Message, dto.OrderId);

        FireEmail(dto.Email, dto.RecipientId, dto.Title, $"<p>{dto.Message}</p>");
        FireSms(dto.Phone, $"EatOClock: {dto.Title} - {dto.Message}");
    }

    // -- UC-61: Restaurant owner new-order alert ------------------------------
    public async Task SendRestaurantAlertAsync(RestaurantNewOrderDTO dto)
    {
        var title   = " New Order Received!";
        var message = $"New order from {dto.CustomerName} - ₹{dto.TotalAmount:F2}. Order ID: {dto.OrderId}";

        // In-app with PlaySound flag in MetaData so frontend triggers audio
        var n = new Notification
        {
            RecipientId = dto.OwnerId,
            Type        = NotificationType.ORDER,
            Title       = title,
            Message     = message,
            OrderId     = dto.OrderId,
            MetaData    = "play_sound"
        };
        _db.Notifications.Add(n);
        await _db.SaveChangesAsync();

        await _hub.Clients.Group(dto.OwnerId).SendAsync("ReceiveNotification", new
        {
            notificationId = n.NotificationId,
            title          = n.Title,
            message        = n.Message,
            type           = "ORDER",
            playSound      = true,          // frontend uses this to play audio
            createdAt      = n.CreatedAt
        });

        FireEmail(dto.OwnerEmail, dto.OwnerId, title,
            $"<p>You have a new order from <strong>{dto.CustomerName}</strong>.</p>" +
            $"<p>Amount: <strong>₹{dto.TotalAmount:F2}</strong></p>" +
            $"<p>Order ID: {dto.OrderId}</p>");

        FireSms(dto.OwnerPhone, $"EatOClock: New order from {dto.CustomerName} - ₹{dto.TotalAmount:F2}");
    }

    // -- Helpers --------------------------------------------------------------

    private async Task<Notification> PersistAndPush(
        string recipientId, NotificationType type,
        string title, string message, Guid? orderId = null)
    {
        var n = new Notification
        {
            RecipientId = recipientId,
            Type        = type,
            Title       = title,
            Message     = message,
            OrderId     = orderId
        };
        _db.Notifications.Add(n);
        await _db.SaveChangesAsync();

        await _hub.Clients.Group(recipientId).SendAsync("ReceiveNotification", ToDto(n));
        return n;
    }

    private void FireEmail(string? email, string name, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(email)) return;
        _ = _email.SendAsync(new SendEmailDTO
        {
            ToEmail = email,
            ToName  = name,
            Subject = subject,
            Body    = body
        });
    }

    private void FireSms(string? phone, string text)
    {
        if (string.IsNullOrWhiteSpace(phone)) return;
        _ = _sms.SendAsync(new SendSmsDTO { ToPhone = phone, Message = text });
    }

    private static (string title, string message) GetOrderMessage(string status) =>
        status.ToUpper() switch
        {
            "PLACED"    => ("Order Placed ",    "Your order has been placed successfully!"),
            "CONFIRMED" => ("Order Confirmed ", "The restaurant has confirmed your order."),
            "PREPARING" => ("Preparing Your Food ", "The restaurant is preparing your food."),
            "PICKED_UP" => ("Order Picked Up ", "Your order is on its way!"),
            "DELIVERED" => ("Order Delivered ", "Your food has arrived. Enjoy your meal!"),
            "CANCELLED" => ("Order Cancelled ", "Your order has been cancelled."),
            _           => ("Order Update",       $"Your order status: {status}")
        };

    private static NotificationDTO ToDto(Notification n) => new()
    {
        NotificationId = n.NotificationId,
        RecipientId    = n.RecipientId,
        Type           = n.Type,
        Title          = n.Title,
        Message        = n.Message,
        IsRead         = n.IsRead,
        CreatedAt      = n.CreatedAt,
        OrderId        = n.OrderId
    };
}