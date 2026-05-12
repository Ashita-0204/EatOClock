using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification_Service.DTOs;
using Notification_Service.Interfaces;
using System.Security.Claims;

namespace Notification_Service.Controllers;

[ApiController]
[Route("api/v1/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly IEmailService _email;
    private readonly ISmsService _sms;

    public NotificationsController(
        INotificationService service,
        IEmailService email,
        ISmsService sms)
    {
        _service = service;
        _email   = email;
        _sms     = sms;
    }

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? string.Empty;

    // Get my notifications -----------------------------------------
    /// <summary>Get all notifications for the logged-in user.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetNotificationsAsync(UserId));

    //Unread badge count -------------------------------------------
    /// <summary>Get unread notification count (for nav badge).</summary>
    [Authorize(Roles = "Admin,Customer")]
    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount() =>
        Ok(new { count = await _service.GetUnreadCountAsync(UserId) });

    //  Mark one as read ---------------------------------------------
    /// <summary>Mark a single notification as read.</summary>
    [HttpPut("{id:guid}/read")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        await _service.MarkAsReadAsync(id, UserId);
        return NoContent();
    }

    //  Mark all as read ---------------------------------------------
    /// <summary>Mark all notifications as read.</summary>
    [HttpPut("read-all")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _service.MarkAllReadAsync(UserId);
        return NoContent();
    }

    /// <summary>Delete a notification.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id, UserId);
        return NoContent();
    }

    //  Admin broadcast ----------------------------------------------
    /// <summary>Admin only - broadcast a platform-wide SignalR notification.</summary>
    [HttpPost("broadcast")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Broadcast([FromBody] BroadcastDTO dto)
    {
        await _service.BroadcastAsync(dto);
        return Ok(new { message = "Broadcast sent." });
    }

    // Order status trigger --------------------------------
    /// <summary>
    /// Called by Order_Service when order status changes.
    /// Sends in-app (SignalR) + email + SMS based on what is provided.
    /// </summary>
    [HttpPost("order")]
    [Authorize(Roles = "Admin")]
    [AllowAnonymous]   // Internal - protected at gateway/network level
    public async Task<IActionResult> OrderNotification([FromBody] SendOrderNotificationDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RecipientId))
            return BadRequest("RecipientId is required.");
        await _service.SendOrderNotificationAsync(dto);
        return Ok(new { message = "Order notification dispatched." });
    }

    //Restaurant new-order alert -----------------------------------
    /// <summary>
    /// Called by Order_Service when a new order is placed.
    /// Sends in-app alert with audio flag + optional email/SMS to restaurant owner.
    /// </summary>
    [HttpPost("restaurant-alert")]
    [AllowAnonymous]
    public async Task<IActionResult> RestaurantAlert([FromBody] RestaurantNewOrderDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.OwnerId))
            return BadRequest("OwnerId is required.");
        await _service.SendRestaurantAlertAsync(dto);
        return Ok(new { message = "Restaurant alert dispatched." });
    }

    // -- Generic in-app + optional email/SMS trigger -------------------------
    /// <summary>
    /// Send any in-app notification to a user, with optional email and SMS.
    /// Useful for payment, promo, and delivery notifications.
    /// </summary>
    [HttpPost("send")]
    [AllowAnonymous]   // Internal - called by Payment_Service, Delivery_Service etc.
    public async Task<IActionResult> Send([FromBody] CreateNotificationDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RecipientId) || string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("RecipientId and Title are required.");
        await _service.SendNotificationAsync(dto);
        return Ok(new { message = "Notification sent." });
    }

    //  Standalone email trigger ------------------------------------
    /// <summary>
    /// Send a standalone email (e.g. promo, welcome, password reset OTP).
    /// Does NOT create an in-app notification.
    /// </summary>
    [HttpPost("send-email")]
    [AllowAnonymous]   // Internal
    public async Task<IActionResult> SendEmail([FromBody] SendEmailDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ToEmail))
            return BadRequest("ToEmail is required.");
        await _email.SendAsync(dto);
        return Ok(new { message = $"Email dispatched to {dto.ToEmail}." });
    }

    // Standalone SMS trigger --------------------------------------
    /// <summary>
    /// Send a standalone SMS (e.g. OTP, delivery PIN).
    /// Does NOT create an in-app notification.
    /// </summary>
    [HttpPost("send-sms")]
    [AllowAnonymous]   // Internal
    public async Task<IActionResult> SendSms([FromBody] SendSmsDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ToPhone))
            return BadRequest("ToPhone is required.");
        await _sms.SendAsync(dto);
        return Ok(new { message = $"SMS dispatched to {dto.ToPhone}." });
    }
}